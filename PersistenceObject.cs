using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Reflection;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.Dynamic;

namespace GAB
{
    public class PersistenceObject<TEntity> : PersistenceObjectBase<TEntity> where TEntity : new()
    {

        #region Eventos



        #endregion

        #region Construtores

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="providerConfig">Provide para acesso aos dados.</param>
        public PersistenceObject(ProviderConfiguration providerConfig)
            : base(providerConfig)
        {

        }

        #endregion

        #region Métodos Privados

        /// <summary>
        /// Carrega um mapa com todos os campos contido no datareader.
        /// </summary>
        /// <param name="dataReader">DataReader a ser analizado.</param>
        /// <returns>Mapa dos campos.</returns>
        [Obsolete]
        private Dictionary<string, int> GetMapDataReader(IDataReader dataReader)
        {
            int countFields = dataReader.FieldCount;
            Dictionary<string, int> dic = new Dictionary<string, int>();

            for (int i = 0; i < countFields; i++)
                dic.Add(dataReader.GetName(i).ToLower(), i);

            return dic;
        }

        private void CheckInputOptional(ref PersistencePropertyCollection ppas, ref IDataReader dReader)
        {
            bool found;
            int j = 0;
            for (int i = 0; i < ppas.Count; i++)
            {
                // Procura as propriedades com o tipo de parametro InputOptional
                if (ppas[i].Direction == DirectionParameter.InputOptional || ppas[i].Direction == DirectionParameter.InputOptionalOutput
                    || ppas[i].Direction == DirectionParameter.InputOptionalOutputOnlyInsert)
                {
                    found = false;

                    // Verifica se a coluna opcional existe no resultado
                    for (j = 0; j < dReader.FieldCount; j++)
                    {
                        if (dReader.GetName(j).ToLower() == ppas[i].Name.ToLower())
                        {
                            // A coluna foi encontrado no resultado
                            found = true;
                            break;
                        }
                    }

                    // Se a coluna não for encontrada no resultado, ela será retirada da lista
                    if (!found)
                    {
                        ppas.RemoveAt(i);
                        i--;
                    }

                }
            }
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Carrega os dados com o retorno da query em um objeto dymamico.
        /// </summary>
        /// <param name="sqlQuery">Query.</param>
        /// <param name="parameters">Parametros da query.</param>
        /// <returns>Lista com os dados do retorno da query.</returns>
        public List<dynamic> LoadAnomimousData(string sqlQuery, params Parameter[] parameters)
        {
            return this.LoadDynamicData(sqlQuery, parameters);
        }

        /// <summary>
        /// Carrega os dados com o retorno da query em um objeto dymamico.
        /// </summary>
        /// <param name="sqlQuery">Query.</param>
        /// <param name="parameters">Parametros da query.</param>
        /// <returns>Lista com os dados do retorno da query.</returns>
        public List<dynamic> LoadDynamicData(string sqlQuery, params Parameter[] parameters)
        {
            IDbCommand cmd = UserProvider.CreateCommand();
            IDataReader dReader;

            List<dynamic> list = new List<dynamic>();
            
            cmd.Connection = Connection;
            cmd.CommandText = sqlQuery.Replace("?", UserProvider.ParameterPrefix);

            // Monta os parametros da instrução
            if (parameters != null)

                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i].ParameterName = parameters[i].ParameterName.Replace("?", UserProvider.ParameterPrefix);

                    IDbDataParameter p = cmd.CreateParameter();
                    p.ParameterName = parameters[i].ParameterName;
                    p.Value = parameters[i].Value;

                    cmd.Parameters.Add(p);
                }

            // Abre a conexão
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            try
            {
                SendMessageDebugTrace("CommandText: " + cmd.CommandText);
                dReader = cmd.ExecuteReader();

                while (dReader.Read())
                {
                    object obj = new ExpandoObject();// as IDictionary<string, object>;
                    for (var i = 0; i < dReader.FieldCount; i++)
                    {
                        //expandoObject.Add(dReader.GetName(i), dReader[i]);
                        try
                        {
                            ((IDictionary<string, object>)obj)[dReader.GetName(i)] = dReader[i];
                        }
                        catch(Exception)
                        {
                            ((IDictionary<string, object>)obj)[dReader.GetName(i)] = null;
                        }
                    }
                    dynamic result = ((dynamic)((System.Dynamic.ExpandoObject)(obj)));
                    list.Add(result as object);
                }
            }
            finally
            {
                if (!ConnectionAlwaysOpened)
                    Connection.Close();
            }
            return list;
        }


        /// <summary>
        /// Carrega os dados com o retorno da query.
        /// </summary>
        /// <param name="sqlQuery">Query.</param>
        /// <param name="sortExpression">Expressão de ordenação do comando sql.</param>
        /// <param name="infoPaging">Informações para paginação do resultado da query.</param>
        /// <param name="parameters">Parametros da query.</param>
        /// <returns>Lista com os dados do retorno da query.</returns>
        public List<TEntity> LoadDataWithSortExpression(string sqlQuery, InfoSortExpression sortExpression, InfoPaging infoPaging, Parameter[] parameters)
        {
            IDbCommand cmd = UserProvider.CreateCommand();
            IDataReader dReader;

            List<TEntity> list = new List<TEntity>();

            bool isMysqlDataBase = false; // Identifica se a atual base de dados é mysql.
            bool checkedInputOptional = false;
            int countStartPage = 0, countPageSize = 0;

            PersistencePropertyCollection listAttr = GetPropertiesAttributes(null, null);

            cmd.Connection = Connection;
            cmd.CommandText = sqlQuery.Replace("?", UserProvider.ParameterPrefix);

            // Monta a instrução de ordenação
            if (sortExpression != null && sortExpression.SortColumn != null && sortExpression.SortColumn != "")
            {
                if (sqlQuery.ToLower().IndexOf("order by") != -1)
                    throw new Exception("Já existe um comando de ordenação no sqlQuery, InforSortExpression não é válido.");
                else
                {
                    //Parallel.ForEach(listAttr, (ppa) =>
                    //    {
                    //        // Verifica qual campo do BD que a expressão de ordenação representa
                    //        if (sortExpression != null && sortExpression.SortColumn == ppa.RepresentedProperty.Name)
                    //            cmd.CommandText += " order by " + ((sortExpression.AliasTable != null) ? (sortExpression.AliasTable + ".") : "") +
                    //                                ppa.Name + ((sortExpression.Reverse) ? " desc" : "");
                    //    });

                    foreach (PersistencePropertyAttribute ppa in listAttr)
                    {
                        // Verifica qual campo do BD que a expressão de ordenação representa
                        if (sortExpression != null && sortExpression.SortColumn == ppa.RepresentedProperty.Name)
                            cmd.CommandText += " order by " + ((sortExpression.AliasTable != null) ? (sortExpression.AliasTable + ".") : "") +
                                                ppa.Name + ((sortExpression.Reverse) ? " desc" : "");
                    }
                }
            }

            // Monta os parametros da instrução
            if (parameters != null)

                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i].ParameterName = parameters[i].ParameterName.Replace("?", UserProvider.ParameterPrefix);

                    IDbDataParameter p = cmd.CreateParameter();
                    p.ParameterName = parameters[i].ParameterName;
                    p.Value = parameters[i].Value;

                    cmd.Parameters.Add(p);
                }

            // Abre a conexão
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            // TODO: Comando especifico do mysql
            if (infoPaging != null && UserProvider.Name.ToLower().IndexOf("mysql", StringComparison.CurrentCultureIgnoreCase) != -1)
            {
                isMysqlDataBase = true;
                cmd.CommandText += " limit " + infoPaging.StartRow + "," + infoPaging.PageSize;
            }

            try
            {
                SendMessageDebugTrace("CommandText: " + cmd.CommandText);
                dReader = cmd.ExecuteReader();

                // Carrega o mapeamento dos campos
                //Dictionary<string, int> mapFields = GetMapDataReader(dReader);

                while (dReader.Read())
                {
                    if (!checkedInputOptional)
                    {
                        CheckInputOptional(ref listAttr, ref dReader);
                        checkedInputOptional = true;
                    }

                    if (infoPaging != null && !isMysqlDataBase && countStartPage < infoPaging.StartRow)
                    {
                        countStartPage++;
                        continue;
                    }

                    TEntity objItem = new TEntity();
                    var columns = Enumerable.Range(0, dReader.FieldCount).Select(dReader.GetName).ToList();

                    foreach (PersistencePropertyAttribute ppa in listAttr)
                    {
                        object value;

                        try
                        {
                            if (columns.Any(x => x.Equals(ppa.Name)))
                                value = dReader[ppa.Name];
                            else
                                value = ppa.DefaultValue(objItem);
                        }
                        catch (KeyNotFoundException ex)
                        {
                            //throw new ColumnNotFoundException(ppa.Name, "");
                            throw new Exception("Coluna " + ppa.Name + " não encontrado no resultado." + ex.Message);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Erro ao recuperar o valor do campo: "
                                + ppa.Name + "; Sql: " + cmd.CommandText + "; Exception: "
                                + ex.Message);
                        }

                        if (value == DBNull.Value) value = null;

                        try
                        {
                            if (ppa.RepresentedProperty.PropertyType == typeof(bool))
                                value = Convert.ToBoolean(value);

                            if (value != null)
                            {
                                Type t1 = ppa.RepresentedProperty.PropertyType;
                                Type t2 = value.GetType();

                                value = ConvertType(value, t1, t2);
                            }
                            ppa.RepresentedProperty.SetValue(objItem, value, null);

                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Erro ao converter tipo de dados do campo: "
                                + ppa.RepresentedProperty.Name + "; Exception: "
                                + ex.Message);
                        }
                    }

                    countPageSize++;
                    list.Add(objItem);

                    // Verifica se o quantidade de itens da página foi preenchida
                    if (infoPaging != null && !isMysqlDataBase && countPageSize >= infoPaging.PageSize)
                        break; // Sai do loop do dReader
                }
            }
            finally
            {
                if (!ConnectionAlwaysOpened)
                    Connection.Close();
            }


            return list;

        }

        /// <summary>
        /// Carrega os dados com o retorno da query.
        /// </summary>
        /// <param name="sqlQuery">Query.</param>
        /// <param name="parameters">Parametros da query.</param>
        /// <returns>Lista com os dados do retorno da query.</returns>
        public List<TEntity> LoadData(string sqlQuery, params Parameter[] parameters)
        {
            return LoadDataWithSortExpression(sqlQuery, null, null, parameters);
        }

        /// <summary>
        /// Carrega os dados com o retorno da query.
        /// </summary>
        /// <param name="sqlQuery">Query.</param>
        /// <returns>Lista com os dados do retorno da query.</returns>
        public List<TEntity> LoadData(string sqlQuery)
        {
            return LoadData(sqlQuery, null);
        }

        /// <summary>
        /// Carrega um unico registro.
        /// </summary>
        /// <param name="sqlQuery">Query.</param>
        /// <returns>Objeto contendo o retorno da query.</returns>
        public TEntity LoadOneData(string sqlQuery)
        {
            return LoadOneData(sqlQuery, null);
        }

        /// <summary>
        /// Carrega um unico registro.
        /// </summary>
        /// <param name="sqlQuery"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public TEntity LoadOneData(string sqlQuery, params Parameter[] parameters)
        {
            List<TEntity> list = LoadData(sqlQuery, parameters);

            if (list.Count > 0)
                return list[0];
            else
                return default(TEntity); // throw new Exception("Registro não encontrado. Sql: " + sqlQuery);
        }

        #endregion
    }
}
