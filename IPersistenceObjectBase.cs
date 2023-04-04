using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace GAB
{
    public interface IPersistenceObjectBase
    {
        #region Propriedades

        /// <summary>
        /// Conexão com BD.
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// Provider utilizado para conexão com BD.
        /// </summary>
        Provider UserProvider { get; }

        #endregion

        #region Métodos

        /// <summary>
        /// Executa comandos sql.
        /// </summary>
        /// <param name="sqlQuery">Causa sql a ser executada.</param>
        /// <param name="useTransaction">Indica a necessidade de usar transaction.</param>
        /// <returns>Número de linhas afetadas.</returns>
        int ExecuteCommand(string sqlQuery, bool useTransaction);

        /// <summary>
        /// Executa comandos sql.
        /// </summary>
        /// <param name="sqlQuery">Causa sql a ser executada.</param>
        /// <returns>Número de linhas afetadas.</returns>
        int ExecuteCommand(string sqlQuery);

        int ExecuteSqlQueryCount(string sqlQuery, params Parameter[] parameters);

        object ExecuteScalar(string sqlQuery, params Parameter[] parameters);

        object ExecuteScalar(string sqlQuery);

        #endregion
    }
}
