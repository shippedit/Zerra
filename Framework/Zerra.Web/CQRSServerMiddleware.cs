﻿// Copyright © KaKush LLC
// Written By Steven Zawaski
// Licensed to you under the MIT license

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Zerra.CQRS;
using Zerra.CQRS.Network;
using Zerra.IO;
using Zerra.Logging;
using Zerra.Reflection;

namespace Zerra.Web
{
    public class CQRSServerMiddleware
    {
        private readonly RequestDelegate requestDelegate;
        private readonly CQRSServerMiddlewareSettings settings;

        public CQRSServerMiddleware(RequestDelegate requestDelegate, CQRSServerMiddlewareSettings settings)
        {            
            this.requestDelegate = requestDelegate;
            this.settings = settings;
        }

        public async Task Invoke(HttpContext context)
        {
            if ((!String.IsNullOrWhiteSpace(settings.Route) && context.Request.Path != settings.Route) || (context.Request.Method != "POST" && context.Request.Method != "OPTIONS"))
            {
                await requestDelegate(context);
                return;
            }

            if (context.Request.Method == "OPTIONS")
            {
                context.Response.Headers.Add(HttpCommon.AccessControlAllowOriginHeader, settings.AllowOriginsString);
                context.Response.Headers.Add(HttpCommon.AccessControlAllowMethodsHeader, "*");
                context.Response.Headers.Add(HttpCommon.AccessControlAllowHeadersHeader, "*");
                return;
            }

            ContentType? contentType;
            if (context.Request.ContentType.StartsWith("application/octet-stream"))
                contentType = ContentType.Bytes;
            else if (context.Request.ContentType.StartsWith("application/jsonnameless"))
                contentType = ContentType.JsonNameless;
            else if (context.Request.ContentType.StartsWith("application/json"))
                contentType = ContentType.Json;
            else
                contentType = null;

            if (!contentType.HasValue)
            {
                context.Response.StatusCode = 400;
                return;
            }

            if (contentType != settings.ContentType)
            {
                context.Response.StatusCode = 400;
                return;
            }

            string providerTypeRequestHeader;
            if (!context.Request.Headers.TryGetValue(HttpCommon.ProviderTypeHeader, out var providerTypeRequestHeaderValue))
            {
                context.Response.StatusCode = 400;
                return;
            }
            providerTypeRequestHeader = providerTypeRequestHeaderValue;

            string originRequestHeader = null;
            if (settings.AllowOrigins != null)
            {
                if (!context.Request.Headers.TryGetValue(HttpCommon.OriginHeader, out var originRequestHeaderValue))
                {
                    context.Response.StatusCode = 401;
                    return;
                }
                originRequestHeader = originRequestHeaderValue;

                if (settings.AllowOrigins != null && settings.AllowOrigins.Length > 0)
                {
                    if (settings.AllowOrigins.Contains(originRequestHeader))
                    {
                        _ = Log.TraceAsync($"{nameof(CQRSServerMiddleware)} Origin Not Allowed {originRequestHeader}");
                        context.Response.StatusCode = 401;
                        return;
                    }
                }
            }

            _ = Log.TraceAsync($"{nameof(CQRSServerMiddleware)} Received {providerTypeRequestHeaderValue}");

            try
            {
                var data = await ContentTypeSerializer.DeserializeAsync<CQRSRequestData>(contentType.Value, context.Request.Body);

                //Authorize
                //------------------------------------------------------------------------------------------------------------
                switch (settings.NetworkType)
                {
                    case NetworkType.Internal:
                        if (data.Claims != null)
                        {
                            var claimsIdentity = new ClaimsIdentity(data.Claims.Select(x => new Claim(x.Type, x.Value)), "CQRS");
                            Thread.CurrentPrincipal = new ClaimsPrincipal(claimsIdentity);
                        }
                        else
                        {
                            Thread.CurrentPrincipal = null;
                        }
                        break;
                    case NetworkType.Api:
                        if (settings.HttpAuthorizer != null)
                        {
                            var headers = context.Request.Headers.ToDictionary<KeyValuePair<string, StringValues>, string, IList<string>>(x => x.Key, x => x.Value.ToArray());
                            settings.HttpAuthorizer.Authorize(headers);
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }

                //Process and Respond
                //----------------------------------------------------------------------------------------------------
                if (!String.IsNullOrWhiteSpace(data.ProviderType))
                {
                    var providerType = Discovery.GetTypeFromName(data.ProviderType);
                    var typeDetail = TypeAnalyzer.GetTypeDetail(providerType);

                    if (!settings.InterfaceTypes.Contains(providerType))
                        throw new Exception($"Unhandled Provider Type {providerType.FullName}");

                    var exposed = typeDetail.Attributes.Any(x => x is ServiceExposedAttribute attribute && (!attribute.NetworkType.HasValue || attribute.NetworkType == settings.NetworkType))
                        && !typeDetail.Attributes.Any(x => x is ServiceBlockedAttribute attribute && (!attribute.NetworkType.HasValue || attribute.NetworkType == settings.NetworkType));
                    if (!exposed)
                        throw new Exception($"Provider {data.MessageType} is not exposed to {settings.NetworkType}");

                    _ = Log.TraceAsync($"Received Call: {providerType.GetNiceName()}.{data.ProviderMethod}");

                    var result = await settings.ProviderHandlerAsync.Invoke(providerType, data.ProviderMethod, data.ProviderArguments);

                    //Response Header
                    //context.Response.Headers.Add(HttpCommon.ProviderTypeHeader, data.ProviderType);
                    context.Response.Headers.Add(HttpCommon.AccessControlAllowOriginHeader, originRequestHeader);
                    context.Response.Headers.Add(HttpCommon.AccessControlAllowMethodsHeader, "*");
                    context.Response.Headers.Add(HttpCommon.AccessControlAllowHeadersHeader, "*");
                    switch (contentType.Value)
                    {
                        case ContentType.Bytes:
                            context.Response.Headers.Add(HttpCommon.ContentTypeHeader, HttpCommon.ContentTypeBytes);
                            break;
                        case ContentType.Json:
                            context.Response.Headers.Add(HttpCommon.ContentTypeHeader, HttpCommon.ContentTypeJson);
                            break;
                        case ContentType.JsonNameless:
                            context.Response.Headers.Add(HttpCommon.ContentTypeHeader, HttpCommon.ContentTypeJsonNameless);
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    int bytesRead;
                    if (result.Stream != null)
                    {
                        var bufferOwner = BufferArrayPool<byte>.Rent(HttpCommon.BufferLength);
                        var buffer = bufferOwner.AsMemory();
                        try
                        {
#if NETSTANDARD2_0
                            while ((bytesRead = await result.Stream.ReadAsync(bufferOwner, 0, bufferOwner.Length)) > 0)
                                await context.Response.Body.WriteAsync(bufferOwner, 0, bytesRead);
#else
                            while ((bytesRead = await result.Stream.ReadAsync(buffer)) > 0)
                                await context.Response.Body.WriteAsync(buffer.Slice(0, bytesRead));
#endif
                        }
                        finally
                        {
                            BufferArrayPool<byte>.Return(bufferOwner);
                        }
                        await context.Response.Body.FlushAsync();

                        return;
                    }
                    else
                    {
                        await ContentTypeSerializer.SerializeAsync(contentType.Value, context.Response.Body, result.Model);
                        await context.Response.Body.FlushAsync();
                        return;
                    }
                }
                else if (!String.IsNullOrWhiteSpace(data.MessageType))
                {
                    var commandType = Discovery.GetTypeFromName(data.MessageType);
                    var typeDetail = TypeAnalyzer.GetTypeDetail(commandType);

                    if (!typeDetail.Interfaces.Contains(typeof(ICommand)))
                        throw new Exception($"Type {data.MessageType} is not a command");

                    if (!settings.CommandTypes.Contains(commandType))
                        throw new Exception($"Unhandled Command Type {commandType.FullName}");

                    var exposed = typeDetail.Attributes.Any(x => x is ServiceExposedAttribute attribute && (!attribute.NetworkType.HasValue || attribute.NetworkType == settings.NetworkType))
                        && !typeDetail.Attributes.Any(x => x is ServiceBlockedAttribute attribute && (!attribute.NetworkType.HasValue || attribute.NetworkType == settings.NetworkType));
                    if (!exposed)
                        throw new Exception($"Command {data.MessageType} is not exposed to {settings.NetworkType}");

                    var command = (ICommand)System.Text.Json.JsonSerializer.Deserialize(data.MessageData, commandType);

                    if (data.MessageAwait)
                        await settings.HandlerAwaitAsync(command);
                    else
                        await settings.HandlerAsync(command);

                    //Response Header
                    context.Response.Headers.Add(HttpCommon.ProviderTypeHeader, data.ProviderType);
                    context.Response.Headers.Add(HttpCommon.AccessControlAllowOriginHeader, originRequestHeader);
                    context.Response.Headers.Add(HttpCommon.AccessControlAllowMethodsHeader, "*");
                    context.Response.Headers.Add(HttpCommon.AccessControlAllowHeadersHeader, "*");
                    switch (contentType.Value)
                    {
                        case ContentType.Bytes:
                            context.Response.Headers.Add(HttpCommon.ContentTypeHeader, HttpCommon.ContentTypeBytes);
                            break;
                        case ContentType.Json:
                            context.Response.Headers.Add(HttpCommon.ContentTypeHeader, HttpCommon.ContentTypeJson);
                            break;
                        case ContentType.JsonNameless:
                            context.Response.Headers.Add(HttpCommon.ContentTypeHeader, HttpCommon.ContentTypeJsonNameless);
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    //Response Body Empty

                    return;
                }

                context.Response.StatusCode = 400;
                return;
            }
            catch (Exception ex)
            {
                _ = Log.ErrorAsync(null, ex);

                context.Response.StatusCode = 500;

                //Response Header
                context.Response.Headers.Add(HttpCommon.AccessControlAllowOriginHeader, originRequestHeader);
                context.Response.Headers.Add(HttpCommon.AccessControlAllowMethodsHeader, "*");
                context.Response.Headers.Add(HttpCommon.AccessControlAllowHeadersHeader, "*");
                switch (contentType.Value)
                {
                    case ContentType.Bytes:
                        context.Response.Headers.Add(HttpCommon.ContentTypeHeader, HttpCommon.ContentTypeBytes);
                        break;
                    case ContentType.Json:
                        context.Response.Headers.Add(HttpCommon.ContentTypeHeader, HttpCommon.ContentTypeJson);
                        break;
                    case ContentType.JsonNameless:
                        context.Response.Headers.Add(HttpCommon.ContentTypeHeader, HttpCommon.ContentTypeJsonNameless);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                await ContentTypeSerializer.SerializeAsync(contentType.Value, context.Response.Body, ex);
                await context.Response.Body.FlushAsync();
            }
        }
    }
}