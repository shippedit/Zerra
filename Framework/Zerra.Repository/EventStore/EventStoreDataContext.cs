﻿// Copyright © KaKush LLC
// Written By Steven Zawaski
// Licensed to you under the MIT license

namespace Zerra.Repository.EventStore
{
    public abstract class EventStoreDataContext : IEventStoreEngineProvider
    {
        public abstract IEventStoreEngine GetEngine();
    }
}
