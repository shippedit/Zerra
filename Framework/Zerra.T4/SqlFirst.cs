﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Zerra.T4
{
    public static class SqlFirst
    {
        const string tab = "    ";
        const string tableQuery = "SELECT * FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME";
        const string columnQuery = @"
SELECT 
*
, CONVERT(bit, COLUMNPROPERTY(object_id(C.TABLE_SCHEMA+'.'+C.TABLE_NAME), C.COLUMN_NAME, 'IsIdentity')) AS IS_IDENTITY
, CONVERT(bit, (SELECT COUNT(1) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU WHERE KCU.TABLE_NAME = C.TABLE_NAME AND KCU.COLUMN_NAME = C.COLUMN_NAME AND OBJECTPROPERTY(object_id(KCU.CONSTRAINT_SCHEMA+'.'+KCU.CONSTRAINT_NAME), 'IsPrimaryKey') = '1')) AS IS_PRIMARYKEY
FROM INFORMATION_SCHEMA.COLUMNS C
WHERE C.TABLE_SCHEMA = '{0}' AND C.TABLE_NAME = '{1}'";
        const string relationQuery = @"
SELECT 
RC.CONSTRAINT_NAME FK_Name
, KF.TABLE_SCHEMA FK_Schema
, KF.TABLE_NAME FK_Table
, KF.COLUMN_NAME FK_Column
, RC.UNIQUE_CONSTRAINT_NAME PK_Name
, KP.TABLE_SCHEMA PK_Schema
, KP.TABLE_NAME PK_Table
, KP.COLUMN_NAME PK_Column
, RC.MATCH_OPTION MatchOption
, RC.UPDATE_RULE UpdateRule
, RC.DELETE_RULE DeleteRule
, TCKP.CONSTRAINT_TYPE
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KF ON RC.CONSTRAINT_NAME = KF.CONSTRAINT_NAME
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KP ON RC.UNIQUE_CONSTRAINT_NAME = KP.CONSTRAINT_NAME
JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS TCKP ON KP.CONSTRAINT_NAME = TCKP.CONSTRAINT_NAME
WHERE
(TCKP.CONSTRAINT_TYPE = 'PRIMARY KEY' OR TCKP.CONSTRAINT_TYPE = 'FOREIGN KEY')
AND ((KF.TABLE_SCHEMA = '{0}' AND KF.TABLE_NAME = '{1}') OR (KP.TABLE_SCHEMA = '{0}' AND KP.TABLE_NAME = '{1}'))";

        public static string GenerateModels(string connectionString, string namespaceString, string modelSuffix)
        {
            var sb = new StringBuilder();
            var tables = new List<Tuple<string, string>>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = tableQuery;
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            var schemaNameIndex = reader.GetOrdinal("TABLE_SCHEMA");
                            var tableNameIndex = reader.GetOrdinal("TABLE_NAME");
                            while (reader.Read())
                            {
                                var schemaName = reader.GetString(schemaNameIndex);
                                var tableName = reader.GetString(tableNameIndex);
                                tables.Add(new Tuple<string, string>(schemaName, tableName));
                            }
                        }
                    }
                }

                sb.Append("using System;").Append(Environment.NewLine);
                sb.Append("using System.Collections.Generic;").Append(Environment.NewLine);
                sb.Append("using Zerra.Repository;").Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
                sb.Append("namespace ").Append(namespaceString).Append(Environment.NewLine);
                sb.Append("{").Append(Environment.NewLine);

                bool usedFirst = false;
                foreach (var table in tables)
                {
                    var schemaName = table.Item1;
                    var tableName = table.Item2;
                    var usedNames = new Dictionary<string, int>();

                    if (usedFirst)
                        sb.Append(Environment.NewLine);
                    else
                        usedFirst = true;

                    sb.Append(tab).Append("[DataSourceEntity(\"").Append(tableName).Append("\")]").Append(Environment.NewLine);
                    sb.Append(tab).Append("public class ").Append(tableName).Append(modelSuffix).Append(Environment.NewLine);
                    sb.Append(tab).Append("{").Append(Environment.NewLine);

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = string.Format(columnQuery, schemaName, tableName);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                var columnNameIndex = reader.GetOrdinal("COLUMN_NAME");
                                var dataTypeIndex = reader.GetOrdinal("DATA_TYPE");
                                var isNullableIndex = reader.GetOrdinal("IS_NULLABLE");
                                var isPrimaryKeyIndex = reader.GetOrdinal("IS_PRIMARYKEY");
                                var isIdentityIndex = reader.GetOrdinal("IS_IDENTITY");
                                var characterMaximumLengthIndex = reader.GetOrdinal("CHARACTER_MAXIMUM_LENGTH");
                                var numericPrecisionIndex = reader.GetOrdinal("NUMERIC_PRECISION");
                                var numericScaleIndex = reader.GetOrdinal("NUMERIC_SCALE");
                                var datetimePrecisionIndex = reader.GetOrdinal("DATETIME_PRECISION");

                                while (reader.Read())
                                {
                                    var columnName = reader.GetString(columnNameIndex);
                                    var dataType = reader.GetString(dataTypeIndex);
                                    var isNullable = reader.GetString(isNullableIndex) == "YES";
                                    var isPrimaryKey = reader.GetBoolean(isPrimaryKeyIndex);
                                    var isIdentity = reader.GetBoolean(isIdentityIndex);
                                    var characterMaximumLength = !reader.IsDBNull(characterMaximumLengthIndex) ? reader.GetInt32(characterMaximumLengthIndex) : (int?)null;
                                    var numericPrecision = !reader.IsDBNull(numericPrecisionIndex) ? reader.GetByte(numericPrecisionIndex) : (byte?)null;
                                    var numericScale = !reader.IsDBNull(numericScaleIndex) ? reader.GetInt32(numericScaleIndex) : (int?)null;
                                    var datetimePrecision = !reader.IsDBNull(datetimePrecisionIndex) ? reader.GetInt16(datetimePrecisionIndex) : (short?)null;

                                    var csharpType = CSharpTypeFromSqlType(dataType, isNullable);
                                    if (isPrimaryKey)
                                    {
                                        if (isIdentity)
                                            sb.Append(tab).Append(tab).Append("[Identity(true)]").Append(Environment.NewLine);
                                        else
                                            sb.Append(tab).Append(tab).Append("[Identity]").Append(Environment.NewLine);
                                    }

                                    var dataSourceTypeAttribute = CSharpAttributeFromSqlType(dataType, isNullable, characterMaximumLength, numericPrecision, numericScale, datetimePrecision);
                                    if (dataSourceTypeAttribute != null)
                                        sb.Append(tab).Append(tab).Append(dataSourceTypeAttribute).Append(Environment.NewLine);
                                    sb.Append(tab).Append(tab).Append("public ").Append(csharpType).Append(" ").Append(columnName).Append(" { get; set; }").Append(Environment.NewLine);

                                    usedNames.Add(columnName, 0);
                                }
                            }
                        }
                    }

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = string.Format(relationQuery, schemaName, tableName);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                var fkTableIndex = reader.GetOrdinal("FK_Table");
                                var fkColumnIndex = reader.GetOrdinal("FK_Column");
                                var pkTableIndex = reader.GetOrdinal("PK_Table");
                                var pkColumnIndex = reader.GetOrdinal("PK_Column");
                                while (reader.Read())
                                {
                                    var fkTable = reader.GetString(fkTableIndex);
                                    var fkColumn = reader.GetString(fkColumnIndex);
                                    var pkTable = reader.GetString(pkTableIndex);
                                    var pkColumn = reader.GetString(pkColumnIndex);

                                    string propertyName = null;
                                    if (fkTable == tableName)
                                        propertyName = pkTable;
                                    else
                                        propertyName = fkTable;

                                    if (usedNames.TryGetValue(propertyName, out var propertyNameCount))
                                    {
                                        propertyNameCount++;
                                        usedNames[propertyName] = propertyNameCount;
                                        propertyName += propertyNameCount;
                                    }
                                    else
                                    {
                                        usedNames.Add(propertyName, 0);
                                    }

                                    sb.Append(tab).Append(tab).Append("[DataSourceRelation(\"").Append(fkColumn).Append("\")]").Append(Environment.NewLine);

                                    if (fkTable == tableName)
                                        sb.Append(tab).Append(tab).Append("public ").Append(pkTable).Append(modelSuffix).Append(" ").Append(propertyName).Append(" { get; set; }").Append(Environment.NewLine);
                                    else
                                        sb.Append(tab).Append(tab).Append("public ICollection<").Append(fkTable).Append(modelSuffix).Append("> ").Append(propertyName).Append(" { get; set; }").Append(Environment.NewLine);
                                }
                            }
                        }
                    }

                    sb.Append(tab).Append("}").Append(Environment.NewLine);
                }
                sb.Append("}");
            }

            return sb.ToString();
        }

        private static string CSharpAttributeFromSqlType(string sqlType, bool isNullable, int? characterMaximumLength, byte? numericPrecision, int? numericScale, short? datetimePrecision)
        {
            switch (sqlType)
            {
                case "bit":
                    return null;
                case "tinyint":
                    return null;
                case "smallint":
                    return null;
                case "int":
                    return null;
                case "bigint":
                    return null;
                case "real":
                    return null;
                case "float":
                    return null;
                case "smallmoney":
                    return null;
                case "money":
                    return null;
                case "numeric":
                    return $"[DataSourceType({numericPrecision}, {numericScale})]";
                case "decimal":
                    return $"[DataSourceType({numericPrecision}, {numericScale})]";
                case "date":
                    return null;
                case "smalldatetime":
                    return null;
                case "datetime":
                    return null;
                case "datetime2":
                    if (datetimePrecision != -1)
                        return $"[DataSourceType({datetimePrecision})]";
                    return null;
                case "datetimeoffset":
                    if (datetimePrecision != -1)
                        return $"[DataSourceType({datetimePrecision})]";
                    return null;
                case "time":
                    if (datetimePrecision != -1)
                        return $"[DataSourceType({datetimePrecision})]";
                    return null;
                case "timestamp":
                    return null;
                case "rowversion":
                    return null;
                case "varchar":
                    if (characterMaximumLength != -1)
                        if (!isNullable)
                            return $"[DataSourceType(true, {characterMaximumLength})]";
                        else
                            return $"[DataSourceType(false, {characterMaximumLength})]";
                    if (!isNullable)
                        return $"[DataSourceType(true)]";
                    return null;
                case "nvarchar":
                    if (characterMaximumLength != -1)
                        if (!isNullable)
                            return $"[DataSourceType(true, {characterMaximumLength})]";
                        else
                            return $"[DataSourceType(false, {characterMaximumLength})]";
                    if (!isNullable)
                        return $"[DataSourceType(true)]";
                    return null;
                case "text":
                    return "string";
                case "ntext":
                    return "string";
                case "char":
                    if (characterMaximumLength != -1)
                        if (!isNullable)
                            return $"[DataSourceType(true, {characterMaximumLength})]";
                        else
                            return $"[DataSourceType(false, {characterMaximumLength})]";
                    if (!isNullable)
                        return $"[DataSourceType(true)]";
                    return null;
                case "nchar":
                    if (characterMaximumLength != -1)
                        return $"[DataSourceType({characterMaximumLength})]";
                    return null;
                case "binary":
                    if (characterMaximumLength != -1)
                        return $"[DataSourceType({characterMaximumLength})]";
                    return null;
                case "varbinary":
                    if (characterMaximumLength != -1)
                        return $"[DataSourceType({characterMaximumLength})]";
                    return null;
                case "image":
                    if (characterMaximumLength != -1)
                        return $"[DataSourceType({characterMaximumLength})]";
                    return null;
                case "uniqueidentifier":
                    return null;
                case "sql_variant":
                    return "object";
                case "xml":
                    if (!isNullable)
                        return $"[DataSourceType(true)]";
                    return null;
                default:
                    throw new NotImplementedException(String.Format("SqlType not implemented {0}", sqlType));
            }
        }

        private static string CSharpTypeFromSqlType(string sqlType, bool isNullable)
        {
            switch (sqlType)
            {
                case "bit":
                    if (isNullable)
                        return "bool?";
                    else
                        return "bool";
                case "tinyint":
                    if (isNullable)
                        return "byte?";
                    else
                        return "byte";
                case "smallint":
                    if (isNullable)
                        return "short?";
                    else
                        return "short";
                case "int":
                    if (isNullable)
                        return "int?";
                    else
                        return "int";
                case "bigint":
                    if (isNullable)
                        return "long?";
                    else
                        return "long";
                case "real":
                    if (isNullable)
                        return "float?";
                    else
                        return "float";
                case "float":
                    if (isNullable)
                        return "double?";
                    else
                        return "double";
                case "smallmoney":
                    if (isNullable)
                        return "decimal?";
                    else
                        return "decimal";
                case "money":
                    if (isNullable)
                        return "decimal?";
                    else
                        return "decimal";
                case "numeric":
                    if (isNullable)
                        return "decimal?";
                    else
                        return "decimal";
                case "decimal":
                    if (isNullable)
                        return "decimal?";
                    else
                        return "decimal";
                case "date":
                    if (isNullable)
                        return "DateTime?";
                    else
                        return "DateTime";
                case "smalldatetime":
                    if (isNullable)
                        return "DateTime?";
                    else
                        return "DateTime";
                case "datetime":
                    if (isNullable)
                        return "DateTime?";
                    else
                        return "DateTime";
                case "datetime2":
                    if (isNullable)
                        return "DateTime?";
                    else
                        return "DateTime";
                case "datetimeoffset":
                    if (isNullable)
                        return "DateTimeOffset?";
                    else
                        return "DateTimeOffset";
                case "time":
                    if (isNullable)
                        return "TimeSpan?";
                    else
                        return "TimeSpan";
                case "timestamp":
                    return "byte[]";
                case "rowversion":
                    return "byte[]";
                case "varchar":
                    return "string";
                case "nvarchar":
                    return "string";
                case "text":
                    return "string";
                case "ntext":
                    return "string";
                case "char":
                    return "string";
                case "nchar":
                    return "string";
                case "binary":
                    return "byte[]";
                case "varbinary":
                    return "byte[]";
                case "image":
                    return "byte[]";
                case "uniqueidentifier":
                    if (isNullable)
                        return "Guid?";
                    else
                        return "Guid";
                case "sql_variant":
                    return "object";
                case "xml":
                    return "string";
                default:
                    throw new NotImplementedException(String.Format("SqlType not implemented {0}", sqlType));
            }
        }

        public static string GenerateProviders(string connectionString, string namespaceString, string modelSuffix, string baseProvider, string usingNamespace)
        {
            var sb = new StringBuilder();
            var tableNames = new List<string>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = tableQuery;
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            var tableNameIndex = reader.GetOrdinal("TABLE_NAME");
                            while (reader.Read())
                            {
                                var tableName = reader.GetString(tableNameIndex);
                                tableNames.Add(tableName);
                            }
                        }
                    }
                }

                if (!String.IsNullOrWhiteSpace(usingNamespace))
                    sb.Append("using ").Append(usingNamespace).Append(';').Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
                sb.Append("namespace ").Append(namespaceString).Append(Environment.NewLine);
                sb.Append("{").Append(Environment.NewLine);

                bool usedFirst = false;
                foreach (var tableName in tableNames)
                {
                    var usedNames = new Dictionary<string, int>();

                    if (usedFirst)
                        sb.Append(Environment.NewLine);
                    else
                        usedFirst = true;

                    sb.Append(tab).Append("public class ").Append(tableName).Append("Provider : ").Append(baseProvider).Append('<').Append(tableName).Append(modelSuffix).Append("> { }");
                }
                sb.Append(Environment.NewLine).Append("}");
            }

            return sb.ToString();
        }
    }
}
