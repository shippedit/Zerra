﻿// Copyright © KaKush LLC
// Written By Steven Zawaski
// Licensed to you under the MIT license

namespace Zerra.CQRS.RabbitMessage
{
    public class RabbitEventMessage
    {
        public IEvent Message { get; set; }
        public string[][] Claims { get; set; }
    }
}
