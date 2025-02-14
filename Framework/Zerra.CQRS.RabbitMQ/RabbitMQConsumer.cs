﻿// Copyright © KaKush LLC
// Written By Steven Zawaski
// Licensed to you under the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Zerra.Encryption;
using Zerra.Logging;

namespace Zerra.CQRS.RabbitMQ
{
    public partial class RabbitMQConsumer : ICommandConsumer, IEventConsumer, IDisposable
    {
        private const int retryDelay = 10000;

        private readonly string host;
        private readonly SymmetricConfig symmetricConfig;
        private readonly string environment;
        private readonly List<CommandComsumer> commandExchanges;
        private readonly List<EventComsumer> eventExchanges;

        private IConnection connection = null;
        private Func<ICommand, Task> commandHandlerAsync = null;
        private Func<ICommand, Task> commandHandlerAwaitAsync = null;
        private Func<IEvent, Task> eventHandlerAsync = null;

        public string ConnectionString => host;

        public RabbitMQConsumer(string host, SymmetricConfig symmetricConfig, string environment)
        {
            this.host = host;
            this.symmetricConfig = symmetricConfig;
            this.environment = environment;
            this.commandExchanges = new List<CommandComsumer>();
            this.eventExchanges = new List<EventComsumer>();
        }

        void ICommandConsumer.SetHandler(Func<ICommand, Task> handlerAsync, Func<ICommand, Task> handlerAwaitAsync)
        {
            if (this.connection != null)
                throw new InvalidOperationException("Connection already open");
            this.commandHandlerAsync = handlerAsync;
            this.commandHandlerAwaitAsync = handlerAwaitAsync;
        }
        void IEventConsumer.SetHandler(Func<IEvent, Task> handlerAsync)
        {
            if (this.connection != null)
                throw new InvalidOperationException("Connection already open");
            this.eventHandlerAsync = handlerAsync;
        }

        void ICommandConsumer.Open()
        {
            Open();
            _ = Log.InfoAsync($"{nameof(RabbitMQConsumer)} Command Server Started Connected To {this.host}");
        }
        void IEventConsumer.Open()
        {
            Open();
            _ = Log.InfoAsync($"{nameof(RabbitMQConsumer)} Event Server Started Connected To {this.host}");
        }
        private void Open()
        {
            if (this.connection != null)
                return;

            try
            {
                var factory = new ConnectionFactory() { HostName = host, DispatchConsumersAsync = true };
                this.connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {
                _ = Log.ErrorAsync($"{nameof(RabbitMQConsumer)} failed to open", ex);
                throw;
            }

            lock (commandExchanges)
            {
                lock (eventExchanges)
                {
                    OpenExchanges();
                }
            }
        }

        private void OpenExchanges()
        {
            if (this.connection == null)
                return;

            foreach (var exchange in commandExchanges.Where(x => !x.IsOpen))
                exchange.Open(this.connection, this.commandHandlerAsync, this.commandHandlerAwaitAsync);

            foreach (var exchange in eventExchanges.Where(x => !x.IsOpen))
                exchange.Open(this.connection, this.eventHandlerAsync);
        }

        void ICommandConsumer.Close()
        {
            Close();
            _ = Log.InfoAsync($"{nameof(RabbitMQConsumer)} Command Server Closed On {this.host}");
        }
        void IEventConsumer.Close()
        {
            Close();
            _ = Log.InfoAsync($"{nameof(RabbitMQConsumer)} Event Server Closed On {this.host}");
        }
        private void Close()
        {
            if (this.connection != null)
            {
                foreach (var exchange in commandExchanges.Where(x => x.IsOpen))
                    exchange.Dispose();
                foreach (var exchange in eventExchanges.Where(x => x.IsOpen))
                    exchange.Dispose();
                this.commandExchanges.Clear();
                this.eventExchanges.Clear();
                this.connection.Close();
                this.connection.Dispose();
                this.connection = null;
            }
        }

        public void Dispose()
        {
            this.Close();
            GC.SuppressFinalize(this);
        }

        void ICommandConsumer.RegisterCommandType(Type type)
        {
            lock (commandExchanges)
            {
                commandExchanges.Add(new CommandComsumer(type, symmetricConfig, environment));
                OpenExchanges();
            }
        }
        ICollection<Type> ICommandConsumer.GetCommandTypes()
        {
            return commandExchanges.Select(x => x.Type).ToArray();
        }

        void IEventConsumer.RegisterEventType(Type type)
        {
            lock (eventExchanges)
            {
                eventExchanges.Add(new EventComsumer(type, symmetricConfig, environment));
                OpenExchanges();
            }
        }
        ICollection<Type> IEventConsumer.GetEventTypes()
        {
            return eventExchanges.Select(x => x.Type).ToArray();
        }
    }
}
