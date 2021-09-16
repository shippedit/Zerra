﻿// Copyright © KaKush LLC
// Written By Steven Zawaski
// Licensed to you under the MIT license

using System;
using System.Threading.Tasks;
using Zerra.Collections;
using Zerra.CQRS;
using Zerra.Reflection;
using Zerra.Repository.EventStore;
using Zerra.Serialization;

namespace Zerra.Repository
{
    public abstract class AggregateRoot
    {
        private static Type typeCache = null;
        private static readonly object typeCacheLock = new object();
        private Type GetAggregateType()
        {
            if (typeCache == null)
            {
                lock (typeCacheLock)
                {
                    if (typeCache == null)
                    {
                        typeCache = this.GetType();
                    }
                }
            }
            return typeCache;
        }

        private static Type providerTypeCache = null;
        private static readonly object providerTypeCacheLock = new object();
        private Type GetProviderType()
        {
            if (providerTypeCache == null)
            {
                lock (providerTypeCacheLock)
                {
                    if (providerTypeCache == null)
                    {
                        var aggregateType = GetAggregateType();
                        var typeIAggregateEventStoreEngineProvider = typeof(IAggregateEventStoreEngineProvider<>);
                        providerTypeCache = TypeAnalyzer.GetGenericType(typeIAggregateEventStoreEngineProvider, aggregateType);
                    }
                }
            }
            return providerTypeCache;
        }

        public Guid ID { get; private set; }
        public ulong? LastEventNumber { get; private set; }
        public DateTime? LastEventDate { get; private set; }
        public string LastEventName { get; private set; }
        public bool IsCreated { get; private set; }
        public bool IsDeleted { get; private set; }

        private readonly string streamName;
        private readonly IEventStoreEngine eventStore;

        protected AggregateRoot(Guid id)
        {
            var providerType = GetProviderType();
            var implementationType = Discovery.GetImplementationType(providerType);
            var provider = (IEventStoreEngineProvider)Instantiator.CreateInstance(implementationType);
            this.eventStore = provider.GetEngine();
            this.ID = id;
            this.streamName = $"{GetAggregateType().FullName}-{id}";
        }

        public async Task Append<TEvent>(TEvent @event, bool validateEventNumber = false) where TEvent : IEvent
        {
            var eventType = typeof(TEvent);
            var eventName = eventType.Name;

            await ApplyEvent(@event, eventType);

            var serializer = new ByteSerializer(true, true);
            var eventModel = serializer.Serialize(@event);
            await this.eventStore.AppendAsync(new EventStoreAppend()
            {
                EventID = Guid.NewGuid(),
                StreamName = this.streamName,
                Data = eventModel,
                EventName = eventName,
                ExpectedEventNumber = validateEventNumber ? LastEventNumber : null,
                ExpectedState = validateEventNumber ? (LastEventNumber.HasValue ? EventStoreState.NotExisting : EventStoreState.Existing) : EventStoreState.Any
            });

            await Bus.DispatchAsync(@event);
        }

        public async Task Delete<TEvent>(TEvent @event, bool validateEventNumber = false) where TEvent : IEvent
        {
            var eventType = typeof(TEvent);
            var eventName = eventType.Name;

            await ApplyEvent(@event, eventType);

            await this.eventStore.TerminateAsync(new EventStoreTerminate()
            {
                EventID = Guid.NewGuid(),
                StreamName = this.streamName,
                EventName = eventName,
                ExpectedEventNumber = validateEventNumber ? LastEventNumber : null,
                ExpectedState = validateEventNumber ? (LastEventNumber.HasValue ? EventStoreState.NotExisting : EventStoreState.Existing) : EventStoreState.Any
            });

            await Bus.DispatchAsync(@event);
        }

        public Task<bool> RebuildOneEvent()
        {
            return Rebuild(LastEventNumber.HasValue ? LastEventNumber.Value + 1 : 0, null);
        }
        public async Task<bool> Rebuild(ulong? maxEventNumber = null, DateTime? maxEventDate = null)
        {
            var startEventNumber = LastEventNumber.HasValue ? LastEventNumber + 1 : null;

            //TODO error handle if aggregate doesn't exist?????
            var eventDatas = await this.eventStore.ReadAsync(streamName, startEventNumber, null, maxEventNumber, null, maxEventDate);
            if (eventDatas.Length == 0)
                return false;

            if (!IsCreated)
                IsCreated = true;

            var serializer = new ByteSerializer(true, true);
            foreach (var eventData in eventDatas)
            {
                this.LastEventNumber = eventData.Number;
                this.LastEventDate = eventData.Date;
                this.LastEventName = eventData.EventName;
                if (eventData.Deleted)
                    this.IsDeleted = true;
                var eventModel = serializer.Deserialize<IEvent>(eventData.Data.Span);
                await ApplyEvent(eventModel, eventModel.GetType());
            }
            return true;
        }

        private static readonly ConcurrentFactoryDictionary<Type, MethodDetail> methodCache = new ConcurrentFactoryDictionary<Type, MethodDetail>();
        private Task ApplyEvent(IEvent @event, Type eventType)
        {
            var methodDetail = methodCache.GetOrAdd(eventType, (t) =>
            {
                var aggregateType = GetAggregateType();
                var aggregateTypeDetail = TypeAnalyzer.GetType(aggregateType);
                MethodDetail methodDetail = null;
                foreach (var method in aggregateTypeDetail.MethodDetails)
                {
                    if (!method.MethodInfo.IsStatic && method.ParametersInfo.Count == 1 && method.ParametersInfo[0].ParameterType == t)
                    {
                        if (methodDetail != null)
                            throw new Exception($"Multiple aggregate event methods found in {aggregateType.Name} to accept {t.Name}");
                        methodDetail = method;
                    }
                }
                if (methodDetail == null)
                    throw new Exception($"No aggregate event methods found in {aggregateType.Name} to accept {t.Name}");
                return methodDetail;
            });

            return methodDetail.CallerAsync(this, new object[] { @event });
        }
    }
}