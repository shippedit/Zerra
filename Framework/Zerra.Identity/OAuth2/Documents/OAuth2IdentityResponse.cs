﻿// Copyright © KaKush LLC
// Written By Steven Zawaski
// Licensed to you under the MIT license

using Zerra.Identity.TokenManagers;
using Newtonsoft.Json.Linq;
using System;

namespace Zerra.Identity.OAuth2.Documents
{
    public class OAuth2IdentityResponse : OAuth2Document
    {
        public string ServiceProvider { get; protected set; }
        public string UserID { get; protected set; }
        public string UserName { get; protected set; }
        public string[] Roles { get; protected set; }

        public override BindingDirection BindingDirection => BindingDirection.Response;

        public OAuth2IdentityResponse(string serviceProvider, string  token)
        {
            var identity = AuthTokenManager.GetIdentity(serviceProvider, token);
            if (identity == null)
                throw new IdentityProviderException("Invalid token");

            this.ServiceProvider = serviceProvider;
            this.UserID = identity.UserID;
            this.UserName = identity.UserName;
            this.Roles = identity.Roles;
        }

        public OAuth2IdentityResponse(Binding<JObject> binding)
        {
            if (binding.BindingDirection != this.BindingDirection)
                throw new ArgumentException("Binding has the wrong binding direction for this document");

            var json = binding.GetDocument();

            if (json == null)
                return;

            this.ServiceProvider = json[OAuth2Binding.ClientFormName]?.ToObject<string>();
            this.UserID = json["userid"]?.ToObject<string>();
            this.UserName = json["username"]?.ToObject<string>();
            this.Roles = json["roles"]?.ToObject<string[]>();
        }

        public override JObject GetJson()
        {
            var json = new JObject();

            if (this.ServiceProvider != null)
                json.Add(OAuth2Binding.ClientFormName, JToken.FromObject(this.ServiceProvider));
            if (this.UserID != null)
                json.Add("userid", JToken.FromObject(this.UserID));
            if (this.UserName != null)
                json.Add("username", JToken.FromObject(this.UserName));
            if (this.Roles != null)
                json.Add("roles", JToken.FromObject(this.Roles));

            return json;
        }
    }
}

