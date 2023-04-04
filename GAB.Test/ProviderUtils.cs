using GAB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAB.Test
{
    public class ProviderUtils
    {
        /// <summary>
        /// Retorna o diretório raiz da aplicação
        /// </summary>
        public static string LocalPath
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
            }
        }

        public static string dbLocation
        {
            get
            {
                /*SQLite */
                return "Data Source= \"" + LocalPath + "\\DB.data\";";
                /*SQL Compact */
                //return "Data Source= \"" +LocalPath + "\\DB.sdf\";";
            }
        }   

        public static ProviderConfiguration DbProvider
        {
            get
            {
                /*SQLite */
                var prov = new Provider("Sqlite", typeof(System.Data.SQLite.SQLiteConnection),
                                                   typeof(System.Data.SQLite.SQLiteDataAdapter),
                                                   typeof(System.Data.SQLite.SQLiteCommand),
                                                   typeof(System.Data.SQLite.SQLiteParameter), "@", "SELECT @@IDENTITY;", false);
                prov.ExecuteCommandsOneAtATime = true;
                return new ProviderConfiguration(dbLocation, prov);

                /*SQL Compact */
                //var prov = new Provider("SqlCe", typeof(System.Data.SqlServerCe.SqlCeConnection),
                //                                   typeof(System.Data.SqlServerCe.SqlCeDataAdapter),
                //                                   typeof(System.Data.SqlServerCe.SqlCeCommand),
                //                                   typeof(System.Data.SqlServerCe.SqlCeParameter), "@", "SELECT @@IDENTITY;", false);
                //prov.ExecuteCommandsOneAtATime = true;
                //return new ProviderConfiguration(dbLocation, prov);

                /*SQL Server */
                //Provider prov = new Provider("MSSQL", typeof(SqlConnection), typeof(SqlDataAdapter), typeof(SqlCommand), typeof(SqlParameter), "@", "SELECT @@IDENTITY;", true);
                //prov.ExecuteCommandsOneAtATime = true;
                //return new ProviderConfiguration(System.Configuration.ConfigurationManager.AppSettings["connString"], prov);
            }
        }

        private static IDbConnection _dbConnection = null;

        public static IDbConnection DbConnection
        {
            get
            {
                if (_dbConnection == null)
                {
                    _dbConnection = new SqlConnection(System.Configuration.ConfigurationManager.AppSettings["connString"]);
                    _dbConnection.Open();
                }

                return _dbConnection;
            }
        }
    }
}
