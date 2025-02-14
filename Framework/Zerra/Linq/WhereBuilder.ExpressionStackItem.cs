﻿// Copyright © KaKush LLC
// Written By Steven Zawaski
// Licensed to you under the MIT license

using System;
using System.Linq.Expressions;

namespace Zerra.Linq
{
    public partial class WhereBuilder<TModel>
        where TModel : class, new()
    {
        private class ExpressionStackItem
        {
            public Expression<Func<TModel, bool>> Expression { get; set; }
            public bool And { get; set; }
            public bool Or { get; set; }
            public bool StartGroup { get; set; }
            public bool EndGroup { get; set; }
        }
    }
}