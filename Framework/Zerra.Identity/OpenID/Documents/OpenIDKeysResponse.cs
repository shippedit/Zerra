﻿// Copyright © KaKush LLC
// Written By Steven Zawaski
// Licensed to you under the MIT license

using Zerra.Identity.Jwt;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Zerra.Encryption;

namespace Zerra.Identity.OpenID.Documents
{
    public class OpenIDKeysResponse : OpenIDDocument
    {
        public X509Certificate2[] Certs { get; protected set; }

        public override BindingDirection BindingDirection => BindingDirection.Response;

        public OpenIDKeysResponse(params X509Certificate2[] certs)
        {
            this.Certs = certs;
        }

        public OpenIDKeysResponse(Binding<JObject> binding)
        {
            if (binding.BindingDirection != this.BindingDirection)
                throw new ArgumentException("Binding has the wrong binding direction for this document");

            var json = binding.GetDocument();

            if (json == null)
                return;

            var keys = json["keys"].ToObject<JwtKey[]>();
            this.Certs = OpenIDKeysResponse.GetCerts(keys);
        }

        public override JObject GetJson()
        {
            var json = new JObject();

            var keys = OpenIDKeysResponse.GetKeys(this.Certs);
            json.Add("keys", JToken.FromObject(keys));

            return json;
        }

        private static X509Certificate2[] GetCerts(JwtKey[] keys)
        {
            var certs = new List<X509Certificate2>();
            foreach (var key in keys)
            {
                if (key.X509Certificates != null)
                {
                    foreach (var x509String in key.X509Certificates)
                    {
                        var cert = new X509Certificate2(Convert.FromBase64String(x509String));
                        //cert.FriendlyName = key.KeyID;
                        certs.Add(cert);
                    }
                }
                else
                {
                    throw new NotImplementedException("Not implemented loading non-x509 cert");
                }
            }
            return certs.ToArray();
        }

        private static JwtKey[] GetKeys(X509Certificate2[] certs)
        {
            var keys = new List<JwtKey>();
            foreach (var cert in certs)
            {
                var rsa = cert.GetRSAPublicKey();
                if (rsa == null)
                    throw new IdentityProviderException("X509 must be RSA");

                var parameters = rsa.ExportParameters(false);
                var publicKey = cert.Export(X509ContentType.Cert);
                var certString = Convert.ToBase64String(publicKey);
                var key = new JwtKey()
                {
                    KeyID = cert.Thumbprint,
                    Use = "sig",
                    KeyType = "RSA",
                    X509Thumbprint = cert.Thumbprint, //same as KeyID
                    Exponent = Base64UrlEncoder.ToBase64String(parameters.Exponent),
                    Modulus = Base64UrlEncoder.ToBase64String(parameters.Modulus),
                    X509Certificates = new string[]
                    {
                            certString
                    }
                };
                keys.Add(key);
            }
            return keys.ToArray();
        }
    }
}
