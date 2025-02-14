﻿#if !NET48
using Microsoft.AspNetCore.Http;
using System.Linq;
#endif

using System.Collections.Generic;

namespace Zerra.Identity
{
    public class IdentityHttpRequest
    {
        public string QueryString { get; private set; }
        public IReadOnlyDictionary<string, string> Query { get; private set; }

        public bool HasFormContentType { get; private set; }
        public IReadOnlyDictionary<string, string> Form { get; private set; }

        public IdentityHttpRequest(string queryString, Dictionary<string, string> query, bool hasFormContentType, Dictionary<string, string> form)
        {
            this.QueryString = queryString;
            this.Query = query;
            this.HasFormContentType = form != null;
            this.HasFormContentType = hasFormContentType;
            this.Form = form;
        }

#if !NET48
        public IdentityHttpRequest(HttpContext context)
        {
            this.QueryString = context.Request.QueryString.Value;
            this.Query = context.Request.Query.ToDictionary(x => x.Key, x => (string)x.Value);

            this.HasFormContentType = context.Request.HasFormContentType;
            if (this.HasFormContentType)
                this.Form = context.Request.Form.ToDictionary(x => x.Key, x => (string)x.Value);
        }
#endif
    }
}
