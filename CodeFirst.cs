using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace GAB
{
    internal class CodeFirst
    {
        private static List<string> TableExist { get; set; }

        [Flags]
        public enum CreateFlags
        {
            None = 0x000,
            ImplicitPK = 0x001,    // create a primary key for field called 'Id' (Orm.ImplicitPkName)
            ImplicitIndex = 0x002,    // create an index for fields ending in 'Id' (Orm.ImplicitIndexSuffix)
            AllImplicit = 0x003,    // do both above
            AutoIncPK = 0x004,    // force PK field to be auto inc
            FullTextSearch3 = 0x100,    // create virtual table using FTS3
            FullTextSearch4 = 0x200     // create virtual table using FTS4
        }

        /// <summary>
        /// Provider utilizado para conexão com BD.
        /// </summary>
        public ProviderConfiguration ProviderConfiguration { get; set; }

        public CodeFirst(ProviderConfiguration providerConfiguration)
        {
            this.ProviderConfiguration = providerConfiguration;
            if (TableExist == null)
                TableExist = new List<string>();
        }

        public CodeFirst CreateDataBase()
        {
            try
            {
                if (this.ProviderConfiguration.Provider.Name.ToUpper().Contains("SQLITE"))
                    CreateDataBaseSQLite();
                else if (this.ProviderConfiguration.Provider.Name.ToUpper().Contains("SQLCE"))
                    CreateDataBaseSQLce();
                else if (this.ProviderConfiguration.Provider.Name.ToUpper().Contains("SQLCE"))
                    CreateDataBaseSQLce();

            }
            catch (Exception ex)
            {
                throw;
            }
            return this;
        }

        public CodeFirst CreateTable(string tableName, PersistencePropertyCollection collectionKeys
                                        , PersistencePropertyCollection collectionPersistenceProperties)
        {
            try
            {
                if (this.ProviderConfiguration.Provider.Name.ToUpper().Contains("SQLITE"))
                    this.CreateTableSQLite(tableName, collectionKeys, collectionPersistenceProperties);
                else if (this.ProviderConfiguration.Provider.Name.ToUpper().Contains("SQLCE"))
                    this.CreateTableSQLce(tableName, collectionKeys, collectionPersistenceProperties);
                else if (this.ProviderConfiguration.Provider.Name.ToUpper().Contains("MYSQL"))
                    this.CreateTableMySql(tableName, collectionKeys, collectionPersistenceProperties);
            }
            catch (Exception ex)
            {
                throw;
            }
            return this;
        }

        #region SQLCe

        private void CreateDataBaseSQLce()
        {
            Match match = new Regex("^.*= \"(.*)\";$").Match(ProviderConfiguration.ConnectionString);
            if (!match.Success)
                throw new ArgumentException("Invalid local data base file");
            else
            {
                if (!System.IO.File.Exists(match.Groups[1].Value))
                {
                    Type tipo = Assembly.Load("System.Data.SqlServerCe")
                                                .GetTypes()
                                                .Where(t => String.Equals(t.Name, "SqlCeEngine", StringComparison.Ordinal))
                                                .FirstOrDefault();

                    object engine = Activator.CreateInstance(tipo, ProviderConfiguration.ConnectionString);
                    engine.GetType().GetMethod("CreateDatabase").Invoke(engine, null);
                }
            }
        }

        private CodeFirst CreateTableSQLce(string tableName, PersistencePropertyCollection keys
                                        , PersistencePropertyCollection props)
        {
            if (TableExists(tableName))
                return this;

            tableName = tableName.Replace("[", "").Replace("]", "");
            
            // Build query.
            var query = "CREATE TABLE [" + tableName + "](\n";
            var decls = keys.Select(p => "[" + p.Name + "] bigint IDENTITY NOT NULL").ToList();
            decls.AddRange(props.Where(p => keys.Any(y => y != p)).Select(p => "[" + p.Name + "] " + SqlTypeCe(p, true) + " "));
            var decl = string.Join(",\n", decls.ToArray());
            query += decl;
            query += ");\n";

            string constraint = string.Join(",\n", keys.Select(p => "\n ALTER TABLE [" + tableName + "] ADD CONSTRAINT [PK_" 
                                                              + tableName + p.Name + "] PRIMARY KEY ([" 
                                                              + p.Name + "]);\n").ToList());

            var conn = this.ProviderConfiguration.Provider.CreateConnection();
            conn.ConnectionString = this.ProviderConfiguration.ConnectionString;

            try
            {
                IDbCommand cmd = this.ProviderConfiguration.Provider.CreateCommand();
                cmd.Connection = conn;
                cmd.CommandText = query;

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                cmd.ExecuteNonQuery();

                cmd.CommandText = constraint;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }

            return this;
        }

        private bool TableExists(string tableName)
        {
            bool result = false;
            string query = @"SELECT TABLE_NAME 
                            FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 
                            '" + tableName + "'";

            var conn = this.ProviderConfiguration.Provider.CreateConnection();
            conn.ConnectionString = this.ProviderConfiguration.ConnectionString;

            try
            {
                IDbCommand cmd = this.ProviderConfiguration.Provider.CreateCommand();
                cmd.Connection = conn;
                cmd.CommandText = query;

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                var tableResult = cmd.ExecuteScalar();
                if (tableResult != null && tableResult.Equals(tableName))
                    result = true;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }

            return result;
        }

        private string SqlTypeCe(PersistencePropertyAttribute prop, bool storeDateTimeAsTicks)
        {
            var clrType = prop.RepresentedProperty.PropertyType;
            if (clrType == typeof(Boolean) || clrType == typeof(Byte) || clrType == typeof(UInt16) || clrType == typeof(SByte) || clrType == typeof(Int16) || clrType == typeof(Int32))
            {
                return SqlDbType.Int.ToString();
            }
            else if (clrType == typeof(UInt32) || clrType == typeof(Int64) || clrType == typeof(int?))
            {
                return "bigint";
            }
            else if (clrType == typeof(Single))
            {
                return "real";
            }
            else if (clrType == typeof(Double) || clrType == typeof(float))
            {
                return "float";
            }
            else if (clrType == typeof(Decimal))
            {
                return "money";
            }
            else if (clrType == typeof(String))
            {
                int? len = prop.Size;

                if (len.HasValue)
                {
                    if (len <= 4000)
                        return "nvarchar(" + (len.Value == 0 ? 255 : len.Value) + ")";
                    else if (len > 4000)
                        return "ntext";
                }

                return "nvarchar";
            }
            else if (clrType == typeof(TimeSpan))
            {
                return "bigint";
            }
            else if (clrType == typeof(DateTime))
            {
                return "datetime";
            }
            else if (clrType == typeof(DateTimeOffset))
            {
                return "bigint";
#if !USE_NEW_REFLECTION_API
            }
            else if (clrType.Name.Contains("Enum"))
            {
#else
            }
            else if (clrType.GetTypeInfo().IsEnum)
            {
#endif
                return "int";
            }
            else if (clrType == typeof(byte[]))
            {
                return "varbinary";
            }
            else if (clrType == typeof(Guid))
            {
                return "nvarchar(36)";
            }
            else
            {
                throw new NotSupportedException("Don't know about " + clrType);
            }
        }

        #endregion

        #region SQLite

        private CodeFirst CreateTableSQLite(string tableName, PersistencePropertyCollection keys
                                        , PersistencePropertyCollection props)
        {
            // Build query.
            var query = "CREATE TABLE IF NOT EXISTS '" + tableName + "'(\n";
            var decls = keys.Select(p => "'" + p.Name + "' " + SqlType(p, true) + " PRIMARY KEY AUTOINCREMENT NOT NULL").ToList();
            decls.AddRange(props.Where(p => keys.Any(y => y != p)).Select(p => "'" + p.Name + "' " + SqlType(p, true) + " "));
            var decl = string.Join(",\n", decls.ToArray());
            query += decl;
            query += ")";

            var conn = this.ProviderConfiguration.Provider.CreateConnection();
            conn.ConnectionString = this.ProviderConfiguration.ConnectionString;

            try
            {
                IDbCommand cmd = this.ProviderConfiguration.Provider.CreateCommand();
                cmd.Connection = conn;
                cmd.CommandText = query;

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //SendMessageDebugTrace("CommandText: " + cmd.CommandText);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }

            return this;
        }

        private void CreateDataBaseSQLite()
        {
            object conn = Activator.CreateInstance(Assembly.Load("System.Data.SQLite")
                                                            .GetTypes()
                                                            .Where(t => String.Equals(t.Name, "SQLiteConnection", StringComparison.Ordinal))
                                                            .FirstOrDefault());

            Match match = new Regex("^.*= \"(.*)\";$").Match(ProviderConfiguration.ConnectionString);
            if (!match.Success)
                throw new ArgumentException("Invalid local data base file");
            else
            {
                if (!System.IO.File.Exists(match.Groups[1].Value))
                    conn.GetType().GetMethod("CreateFile")
                                  .Invoke(conn, new[] { match.Groups[1].Value });

                //if(!String.IsNullOrEmpty(ProviderConfiguration.Password))
                //    conn.GetType().GetMethod("SetPassword")
                //                  .Invoke(conn, new[] { ProviderConfiguration.Password });
            }
        }

        #endregion

        #region MySql

        private CodeFirst CreateTableMySql(string tableName, PersistencePropertyCollection keys
                                        , PersistencePropertyCollection props)
        {
            var conn = this.ProviderConfiguration.Provider.CreateConnection();
            conn.ConnectionString = this.ProviderConfiguration.ConnectionString;
            IDbCommand cmd = this.ProviderConfiguration.Provider.CreateCommand();
            cmd.Connection = conn;
                

            if (TableExist == null || !TableExist.Any(x => x == "MYSQL_" + tableName))
            {
                var sql = "SHOW TABLES LIKE '" + tableName + "'";
                cmd.CommandText = sql;

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                var exist = cmd.ExecuteScalar();
                if (exist != null)
                    TableExist.Add("MYSQL_" + tableName);
                else
                {
                    // Build query.
                    var query = "CREATE TABLE " + tableName + "(\n";
                    var decls = keys != null && keys.Count() > 0 ? keys.Select(p => "" + p.Name + " " + SqlTypeMySql(p, true) + " UNSIGNED AUTO_INCREMENT PRIMARY KEY").ToList()
                                                                 : new List<string>();
                    decls.AddRange(props.Where(p => keys.Any(y => y != p)).Select(p => "" + p.Name + " " + SqlTypeMySql(p, true) + " "));
                    //decls.AddRange(props.Select(p => "" + p.Name + " " + SqlTypeMySql(p, true) + " "));
                    var decl = string.Join(",\n", decls.ToArray());
                    query += decl;
                    query += ")";

                    try
                    {
                        cmd.CommandText = query;

                        if (conn.State != ConnectionState.Open)
                            conn.Open();

                        cmd.ExecuteNonQuery();
                        TableExist.Add("MYSQL_" + tableName);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if (conn.State != ConnectionState.Closed)
                            conn.Close();
                    }

                }
            }
            return this;
        }

        private string SqlTypeMySql(PersistencePropertyAttribute prop, bool storeDateTimeAsTicks)
        {
            var clrType = prop.RepresentedProperty.PropertyType;
            if (clrType == typeof(Boolean) || clrType == typeof(Byte) || clrType == typeof(UInt16) || clrType == typeof(SByte) || clrType == typeof(Int16) || clrType == typeof(Int32) || clrType == typeof(System.Nullable<Int32>))
            {
                return "integer";
            }
            else if (clrType == typeof(UInt32) || clrType == typeof(Int64) || clrType == typeof(UInt64))
            {
                return "bigint";
            }
            else if (clrType == typeof(Single) || clrType == typeof(Double) || clrType == typeof(Decimal))
            {
                return "real(7,4)";
            }
            else if (clrType == typeof(String))
            {
                int? len = prop.Size;

                if (len.HasValue && len.Value <= 4000)
                    return "varchar(" + (len.Value == 0 ? 4000 : len.Value) + ")";
                else
                    return "text";

                return "varchar";
            }
            else if (clrType == typeof(TimeSpan))
            {
                return "bigint";
            }
            else if (clrType == typeof(DateTime))
            {
                return "datetime";
            }
            else if (clrType == typeof(DateTimeOffset))
            {
                return "bigint";
#if !USE_NEW_REFLECTION_API
            }
            else if (clrType.Name.Contains("Enum"))
            {
#else
            }
            else if (clrType.GetTypeInfo().IsEnum)
            {
#endif
                return "integer";
            }
            else if (clrType == typeof(byte[]))
            {
                return "blob";
            }
            else if (clrType == typeof(Guid))
            {
                return "varchar(36)";
            }
            else
            {
                throw new NotSupportedException("Don't know about " + clrType);
            }
        }

        #endregion

        private string SqlType(PersistencePropertyAttribute prop, bool storeDateTimeAsTicks)
        {
            var clrType = prop.RepresentedProperty.PropertyType;
            if (clrType == typeof(Boolean) || clrType == typeof(Byte) || clrType == typeof(UInt16) || clrType == typeof(SByte) || clrType == typeof(Int16) || clrType == typeof(Int32) || clrType == typeof(System.Nullable<Int32>))
            {
                return "integer";
            }
            else if (clrType == typeof(UInt32) || clrType == typeof(Int64) || clrType == typeof(UInt64))
            {
                return "bigint";
            }
            else if (clrType == typeof(Single) || clrType == typeof(Double) || clrType == typeof(Decimal))
            {
                return "real";
            }
            else if (clrType == typeof(String))
            {
                int? len = prop.Size;

                if (len.HasValue && len.Value <= 4000)
                    return "varchar(" + (len.Value == 0 ? 4000 : len.Value) + ")";
                else
                    return "text";

                return "varchar";
            }
            else if (clrType == typeof(TimeSpan))
            {
                return "bigint";
            }
            else if (clrType == typeof(DateTime))
            {
                return "datetime";
            }
            else if (clrType == typeof(DateTimeOffset))
            {
                return "bigint";
#if !USE_NEW_REFLECTION_API
            }
            else if (clrType.Name.Contains("Enum"))
            {
#else
            }
            else if (clrType.GetTypeInfo().IsEnum)
            {
#endif
                return "integer";
            }
            else if (clrType == typeof(byte[]))
            {
                return "blob";
            }
            else if (clrType == typeof(Guid))
            {
                return "varchar(36)";
            }
            else
            {
                throw new NotSupportedException("Don't know about " + clrType);
            }
        }

    }
}
