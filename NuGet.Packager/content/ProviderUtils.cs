using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;


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

    /// <summary>
    /// Chave da connectionString
    /// </summary>
    public static string ConnectionStrings 
    {
        get
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings["connectionStrings"].ConnectionString;
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

    public static GAB.ProviderConfiguration DbProvider
    {
        get
        {
            /*SQLite */
            //var prov = new Provider("Sqlite", typeof(System.Data.SQLite.SQLiteConnection),
            //                                   typeof(System.Data.SQLite.SQLiteDataAdapter),
            //                                   typeof(System.Data.SQLite.SQLiteCommand),
            //                                   typeof(System.Data.SQLite.SQLiteParameter), "@", "SELECT @@IDENTITY;", false);
            //prov.ExecuteCommandsOneAtATime = true;
            //return new ProviderConfiguration(dbLocation, prov);

            /*SQL Compact */
            //var prov = new Provider("SqlCe", typeof(System.Data.SqlServerCe.SqlCeConnection),
            //                                   typeof(System.Data.SqlServerCe.SqlCeDataAdapter),
            //                                   typeof(System.Data.SqlServerCe.SqlCeCommand),
            //                                   typeof(System.Data.SqlServerCe.SqlCeParameter), "@", "SELECT @@IDENTITY;", false);
            //prov.ExecuteCommandsOneAtATime = true;
            //return new ProviderConfiguration(dbLocation, prov);

            /*SQL Server */
            GAB.Provider prov = new GAB.Provider("MSSQL", typeof(SqlConnection), typeof(SqlDataAdapter), typeof(SqlCommand), typeof(SqlParameter), "@", "SELECT @@IDENTITY;", true);
            prov.ExecuteCommandsOneAtATime = true;
            return new GAB.ProviderConfiguration(ConnectionStrings, prov);
        }
    }
}
