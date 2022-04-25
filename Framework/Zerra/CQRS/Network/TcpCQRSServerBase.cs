﻿// Copyright © KaKush LLC
// Written By Steven Zawaski
// Licensed to you under the MIT license

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Zerra.Collections;
using Zerra.Logging;
using System.Collections.Generic;

namespace Zerra.CQRS.Network
{
    public abstract class TcpCQRSServerBase : IDisposable, IQueryServer, ICommandConsumer
    {
        protected readonly ConcurrentList<Type> interfaceTypes;
        protected readonly ConcurrentList<Type> commandTypes;

        private readonly SocketListener[] listeners;
        protected Func<Type, string, string[], Task<RemoteQueryCallResponse>> providerHandlerAsync = null;
        protected Func<ICommand, Task> handlerAsync = null;
        protected Func<ICommand, Task> handlerAwaitAsync = null;

        private bool started = false;
        private bool disposed = false;

        private readonly string serviceUrl;
        public string ConnectionString => serviceUrl;

        public TcpCQRSServerBase(string serverUrl)
        {
            this.serviceUrl = serverUrl;

            var endpoints = IPResolver.GetIPEndPoints(serverUrl, 80);

            this.listeners = new SocketListener[endpoints.Count];
            for (var i = 0; i < endpoints.Count; i++)
            {
                var socket = new Socket(endpoints[i].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.NoDelay = true;
                socket.Bind(endpoints[i]);
                var listener = new SocketListener(socket, Handle);
                this.listeners[i] = listener;
            }

            this.interfaceTypes = new ConcurrentList<Type>();
            this.commandTypes = new ConcurrentList<Type>();
        }

        void IQueryServer.SetHandler(Func<Type, string, string[], Task<RemoteQueryCallResponse>> handlerAsync)
        {
            this.providerHandlerAsync = handlerAsync;
        }

        void IQueryServer.RegisterInterfaceType(Type type)
        {
            interfaceTypes.Add(type);
        }
        ICollection<Type> IQueryServer.GetInterfaceTypes()
        {
            return interfaceTypes.ToArray();
        }

        void ICommandConsumer.SetHandler(Func<ICommand, Task> handlerAsync, Func<ICommand, Task> handlerAwaitAsync)
        {
            this.handlerAsync = handlerAsync;
            this.handlerAwaitAsync = handlerAwaitAsync;
        }

        void ICommandConsumer.RegisterCommandType(Type type)
        {
            commandTypes.Add(type);
        }
        ICollection<Type> ICommandConsumer.GetCommandTypes()
        {
            return commandTypes.ToArray();
        }

        void IQueryServer.Open()
        {
            Open();
            _ = Log.InfoAsync($"{nameof(TcpCQRSServerBase)} Query Server Started On {this.serviceUrl}");
        }
        void ICommandConsumer.Open()
        {
            Open();
            _ = Log.InfoAsync($"{nameof(TcpCQRSServerBase)} Command Server Started On {this.serviceUrl}");
        }
        protected void Open()
        {
            lock (this)
            {
                if (disposed)
                    throw new ObjectDisposedException(nameof(TcpCQRSServerBase));
                if (started)
                    return;

                foreach (var listener in listeners)
                    listener.Open();

                started = true;
            }
        }

        protected abstract void Handle(TcpClient client, CancellationToken cancellationToken);

        void IQueryServer.Close()
        {
            Close();
            _ = Log.InfoAsync($"{nameof(TcpCQRSServerBase)} Query Server Closed On {this.serviceUrl}");
        }
        void ICommandConsumer.Close()
        {
            Close();
            _ = Log.InfoAsync($"{nameof(TcpCQRSServerBase)} Command Server Closed On {this.serviceUrl}");
        }
        protected void Close()
        {
            lock (this)
            {
                foreach (var listener in listeners)
                    listener.Close();
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                if (disposed)
                    return;
                foreach (var listener in listeners)
                    listener.Dispose();
                interfaceTypes.Dispose();
                commandTypes.Dispose();
                disposed = true;
            }
        }
    }
}