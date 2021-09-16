﻿// Copyright © KaKush LLC
// Written By Steven Zawaski
// Licensed to you under the MIT license

namespace Zerra.CQRS.RabbitMessage
{
    public class Acknowledgement
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
