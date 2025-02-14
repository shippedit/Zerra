﻿// Copyright © KaKush LLC
// Written By Steven Zawaski
// Licensed to you under the MIT license

using MySql.Data.MySqlClient;
using Zerra.Logging;

namespace Zerra.Repository.MySql
{
    public abstract class MySqlDataContext : DataContext
    {
        public abstract string ConnectionString { get; }

        private IDataStoreEngine engine = null;
        protected override sealed IDataStoreEngine GetEngine()
        {
            if (engine == null)
            {
                lock (this)
                {
                    if (engine == null)
                    {
                        try
                        {
                            var connectionForParsing = new MySqlConnectionStringBuilder(ConnectionString);
                            _ = Log.InfoAsync($"{nameof(MySqlDataContext)} connecting to {connectionForParsing.Server}");
                        }
                        catch
                        {
                            _ = Log.InfoAsync($"{nameof(MySqlDataContext)} failed to parse {ConnectionString}");
                        }
                        engine = new MySqlEngine(ConnectionString);
                    }
                }
            }
            return engine;
        }
    }
}
