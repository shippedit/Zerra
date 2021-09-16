﻿// Copyright © KaKush LLC
// Written By Steven Zawaski
// Licensed to you under the MIT license

using System;
using System.Threading.Tasks;
using Zerra.Providers;

namespace Zerra.Logging
{
    public static class Log
    {
        public static Task InfoAsync(string message)
        {
            if (Resolver.TryGet(out ILoggingProvider provider))
            {
                return provider.InfoAsync(message);
            }
            return Task.CompletedTask;
        }
        public static Task DebugAsync(string message)
        {
            if (Resolver.TryGet(out ILoggingProvider provider))
            {
                return provider.DebugAsync(message);
            }
            return Task.CompletedTask;
        }
        public static Task WarnAsync(string message)
        {
            if (Resolver.TryGet(out ILoggingProvider provider))
            {
                return provider.WarnAsync(message);
            }
            return Task.CompletedTask;
        } 
        public static Task TraceAsync(string message)
        {
            if (Resolver.TryGet(out ILoggingProvider provider))
            {
                return provider.TraceAsync(message);
            }
            return Task.CompletedTask;
        }
        public static Task ErrorAsync(string message, Exception exception = null)
        {
            if (Resolver.TryGet(out ILoggingProvider provider))
            {
                return provider.ErrorAsync(message, exception);
            }
            return Task.CompletedTask;
        }
        public static Task CriticalAsync(string message, Exception exception = null)
        {
            if (Resolver.TryGet(out ILoggingProvider provider))
            {
                return provider.CriticalAsync(message, exception);
            }
            return Task.CompletedTask;
        }
    }
}
