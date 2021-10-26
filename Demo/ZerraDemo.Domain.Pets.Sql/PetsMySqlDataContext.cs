﻿using Zerra;
using Zerra.Repository.MySql;

namespace ZerraDemo.Domain.Pets.Sql
{
    public class PetsMySqlDataContext : MySqlDataContext
    {
        public override string ConnectionString => connectionString;
        private readonly string connectionString;
        public PetsMySqlDataContext()
        {
            this.connectionString = Config.GetSetting("PetsSqlConnectionStringMYSQL");
        }
    }
}
