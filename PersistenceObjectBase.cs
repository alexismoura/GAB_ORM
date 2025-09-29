using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Data.Common;
using System.Diagnostics;

namespace GAB
{
    public delegate void DebugTraceDelegate(object sender, string message);

    public class PersistenceObjectBase<TEntity> //: IPersistenceObjectBase, IPersistenceObjectDataAccess<TEntity>
    {
        #region Variáveis Locais

        private ProviderConfiguration providerConfig;
        private IDbConnection conn;
        private PersistencePropertyCollection collectionPersistenceProperties;
        private PersistencePropertyCollection collectionKeys;
        private bool m_EnabledDebugTrace;
        private IDbTransaction transaction;

        #endregion

        #region Construtores

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="providerConfig">Provide para acesso aos dados.</param>
        public PersistenceObjectBase(ProviderConfiguration providerConfig)
        {
            if (providerConfig == null)
                throw new ArgumentNullException("providerConfig");

            this.providerConfig = providerConfig;

            //if (providerConfig.Provider.Name.ToUpper().Contains("SQLITE"))
                new CodeFirst(providerConfig).CreateDataBase()
                                         .CreateTable(TableName, this.Keys, collectionPersistenceProperties);
        }

        #endregion

        #region Eventos

        /// <summary>
        /// Evento acionado quando alguma message para debug é lançada.
        /// </summary>
        public event DebugTraceDelegate DebugTrace;

        #endregion

        #region Destrutor

        /// <summary>
        /// Destrutor
        /// </summary>
        ~PersistenceObjectBase()
        {

        }

        #endregion

        #region Propriedades

        /// <summary>
        /// Captura o nome da tabela que a class T representa.
        /// </summary>
        public string TableName
        {
            get
            {
                Type type = typeof(TEntity);
                object[] param = type.GetCustomAttributes(typeof(PersistenceClassAttribute), true);

                // Verifica se a classe referênciada possui o mapeamento
                if (param.Length == 0 || ((PersistenceClassAttribute)param[0]).Name == null)
                {
                    // Captura a lista de interfaces da classe
                    Type[] interfaces = type.GetInterfaces();

                    PersistenceClassAttribute pca;

                    // Procura nas interfaces o mapeamento para o nome da tabela
                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        // Verifica se o interface possui algum mapeamento
                        pca = GetPersistenceClassAttribute(interfaces[i]);

                        if (pca != null && pca.Name != null)
                        {
                            return pca.Name;
                        }

                    }

                    throw new Exception("The class: " + typeof(TEntity).FullName + ", not found AttributePersistenceClass");
                }

                return ((PersistenceClassAttribute)param[0]).Name;
            }
        }

        /// <summary>
        /// Obtem as chaves do objeto referenciado.
        /// </summary>
        public PersistencePropertyCollection Keys
        {
            get
            {
                if (collectionKeys == null)
                {
                    collectionKeys = new PersistencePropertyCollection();

                    LoadPersistencePropertyAttributes();

                    // Procura as propriedades que representam uma chave
                    foreach (PersistencePropertyAttribute ppa in collectionPersistenceProperties)
                        if (ppa.ParameterType == PersistenceParameterType.Key || ppa.ParameterType == PersistenceParameterType.IdentityKey)
                            collectionKeys.Add(ppa);
                }

                return collectionKeys;
            }
        }

        /// <summary>
        /// Conexão com BD.
        /// </summary>
        public IDbConnection Connection
        {
            get
            {
                if (conn == null)
                {
                    conn = UserProvider.CreateConnection();
                    conn.ConnectionString = providerConfig.ConnectionString;
                }

                return conn;
            }

        }

        /// <summary>
        /// Indica se a conexão deve ser mantida aberta durante o processo de insert
        /// </summary>
        private bool m_ConnectionAlwaysOpened = false;

        public bool ConnectionAlwaysOpened
        {
            get { return m_ConnectionAlwaysOpened; }
            set { m_ConnectionAlwaysOpened = value; }
        }

        /// <summary>
        /// Provider utilizado para conexão com BD.
        /// </summary>
        public Provider UserProvider
        {
            get { return providerConfig.Provider; }
        }

        /// <summary>
        /// Captura e define o estado do debug.
        /// </summary>
        public bool EnabledDebugTrace
        {
            get { return m_EnabledDebugTrace; }
            set { m_EnabledDebugTrace = value; }
        }

        #endregion

        #region Métodos Privados

        /// <summary>
        /// Converte tipos especificos do sistema para tipos do BD.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="ppa"></param>
        /// <returns></returns>
        protected static object ConvertToDatabaseType(object value, PersistencePropertyAttribute ppa)
        {
            Type type = ppa.RepresentedProperty.PropertyType;

            if (type == typeof(bool)) return Convert.ToBoolean(value);
            else if (type == typeof(uint)) return Convert.ToInt32(value);
            else if (type == typeof(ushort)) return Convert.ToInt16(value);
            else if (type.IsEnum)
            {
                switch (Enum.GetUnderlyingType(type).Name)
                {
                    case "Int16": return (short)value;
                    case "UInt16": return (ushort)value;
                    case "Int32": return (int)value;
                    case "UInt32": return (uint)value;
                    case "Byte": return (byte)value;
                    default: return (int)value;
                }
            }
            else return value;
        }

        /// <summary>
        /// Converte o valor do objeto para o tipo especifico.
        /// </summary>
        /// <param name="value">Valor.</param>
        /// <param name="type1">Tipo do objeto.</param>
        /// <param name="type2">Tipo esperado.</param>
        /// <returns></returns>
        protected static object ConvertType(object value, Type type1, Type type2)
        {
            if ((type1 == typeof(uint) || type1 == typeof(uint?)) && type2 == typeof(int))
                value = uint.Parse(value.ToString());
            else if ((type1 == typeof(ushort) || type1 == typeof(ushort?)) && type2 == typeof(short))
                value = ushort.Parse(value.ToString());
            else if (type1 == typeof(Int32) || type1 == typeof(Int32?))
                value = System.Convert.ToInt32(value);

            else if ((type1 == typeof(int) || type1 == typeof(int?)) && type2 == typeof(Decimal))
                value = Decimal.ToInt32((Decimal)value);

            // Verifica se o type de origem é um Enumerator
            else if (type2.IsEnum)
            {
                switch (Enum.GetUnderlyingType(type2).Name)
                {
                    case "Int16": return (short)value;
                    case "UInt16": return (ushort)value;
                    case "Int32": return (int)value;
                    case "UInt32": return (uint)value;
                    case "Byte": return (byte)value;
                    default: return (int)value;
                }
            }
            // Verifica se o tipo de destino é um enumetor
            else if (type1.IsEnum)
            {
                value = Enum.ToObject(type1, value);
            }
            else if (type1 == typeof(byte))
            {
                return Convert.ToByte(value);
            }
            else if (value != null && type1 == typeof(bool) && (type2 == typeof(int) || type2 == typeof(uint) ||
                type2 == typeof(short) || type2 == typeof(ushort) || type2 == typeof(string) || type2 == typeof(byte)))
            {
                string v = value.ToString().ToLower();

                if (v == "1" || v == "true" || v == "y" || v == "yes" || v == "s")
                    return true;
                else if (v == "0" || v == "false" || v == "n" || v == "no" || v == "n")
                    return false;
            }
            else if ((type1 == typeof(int) || type1 == typeof(int?)) && type2 != typeof(int))
            {
                if (type2 == typeof(decimal))
                    value = decimal.ToInt32((decimal)value);
                else if (type2 == typeof(float))
                    value = (int)(float)value;
                else if (type2 == typeof(double))
                    value = (int)(double)value;
                else if (type2 == typeof(short))
                    value = (int)(short)value;
                else if (type2 == typeof(long))
                    value = (int)(long)value;
            }
            else if (value is decimal && type1 == typeof(float))
                value = decimal.ToSingle((decimal)value);

            return value;
        }

        /// <summary>
        /// Captura uma nova chave.
        /// </summary>
        /// <returns></returns>
        protected int GetNewKey(string nameField)
        {
            int returnValue;
            IDbCommand cmd = UserProvider.CreateCommand();

            cmd.Connection = Connection;
            cmd.CommandText = "select max(" + nameField + ") from " + TableName;

            Connection.Open();

            try
            {
                string value = cmd.ExecuteScalar().ToString();

                if (value == "") returnValue = 0;
                else returnValue = int.Parse(value);
            }
            finally
            {
                Connection.Close();
            }

            return returnValue + 1;
        }

        /// <summary>
        /// Captura o PersistencePropertyAttribute da chave identidade da model
        /// </summary>
        /// <returns>PersistencePropertyAttribute que representa a chave indetidade mapeada na model.
        /// <para>Null se não for encontrado nenhum chave identidade na model.</para>
        /// </returns>
        private PersistencePropertyAttribute GetPropertyIdentityKey()
        {
            foreach (PersistencePropertyAttribute ppa in collectionPersistenceProperties)
                if (ppa.ParameterType == PersistenceParameterType.IdentityKey)
                    return ppa;

            return null;
        }

        /// <summary>
        /// Envia uma mensagem para o debug.
        /// </summary>
        /// <param name="message">Mensagem a ser enviada.</param>
        protected void SendMessageDebugTrace(string message)
        {
            if (m_EnabledDebugTrace && DebugTrace != null)
                DebugTrace(this, message);
        }

        /// <summary>
        /// Carrega a lista das propriedades mapeadas.
        /// </summary>
        protected void LoadPersistencePropertyAttributes()
        {
            if (collectionPersistenceProperties == null)
            {
                SendMessageDebugTrace("Loading PersistenceProperties...");
                collectionPersistenceProperties = new PersistencePropertyCollection();
                LoadPersistencePropertyAttributes(typeof(TEntity));
            }
        }

        /// <summary>
        /// Carrega a lista das propriedades mapeadas.
        /// </summary>
        /// <param name="type"></param>
        private void LoadPersistencePropertyAttributes(Type type)
        {
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                object[] attrs = property.GetCustomAttributes(typeof(PersistencePropertyAttribute), true);

                if (attrs != null && attrs.Length > 0)
                {
                    SendMessageDebugTrace("Load PersistenceProperty: " + attrs[0].ToString());
                    ((PersistencePropertyAttribute)attrs[0]).RepresentedProperty = property;
                    collectionPersistenceProperties.Add((PersistencePropertyAttribute)attrs[0]);
                }
                else
                {
                    var types = new[]
                          {
                              typeof (Enum),
                              typeof (String),
                              typeof (Char),

                              typeof (Boolean),
                              typeof (Byte),
                              typeof (Int16),
                              typeof (Int32),
                              typeof (Int64),
                              typeof (Single),
                              typeof (Double),
                              typeof (Decimal),

                              typeof (SByte),
                              typeof (UInt16),
                              typeof (UInt32),
                              typeof (UInt64),

                              typeof (DateTime),
                              typeof (DateTimeOffset),
                              typeof (TimeSpan),
                          };


                    var nullTypes = from t in types
                                    where t.IsValueType
                                    select typeof(Nullable<>).MakeGenericType(t);

                    var listTypes = types.Concat(nullTypes).ToArray();

                    if (!listTypes.Any(x => x.IsAssignableFrom(property.PropertyType)))
                        this.LoadPersistencePropertyAttributes(property.PropertyType);

                    var persistencePropertyAttribute = new PersistencePropertyAttribute(property.Name);
                    persistencePropertyAttribute.RepresentedProperty = property;
                    collectionPersistenceProperties.Add(persistencePropertyAttribute);
                }
            }

            // Verifica se o type é uma interface
            if (!type.IsInterface)
            {
                // Captura a lista de interfaces da classe
                Type[] interfaces = type.GetInterfaces();

                PersistenceClassAttribute pca;

                for (int i = 0; i < interfaces.Length; i++)
                {
                    // Verifica se o interface possui algum mapeamento
                    pca = GetPersistenceClassAttribute(interfaces[i]);

                    if (pca != null)
                    {
                        LoadPersistencePropertyAttributes(interfaces[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Captura o PersistenceClassAttribute contido o type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>null se não for encontrado o attributo.</returns>
        private PersistenceClassAttribute GetPersistenceClassAttribute(Type type)
        {
            object[] obj = type.GetCustomAttributes(typeof(PersistenceClassAttribute), true);

            if (obj.Length == 0)
                return null;
            else
                return (PersistenceClassAttribute)obj[0];
        }

        /// <summary>
        /// Captura os attributes PersistenceProperty das propriedades da classe refenciada.
        /// </summary>
        /// <param name="typesParam">Tipos de parametros a serem filtrados. null para não se aplicar nenhum filtro.</param>
        /// <param name="directions">Sentido dos atributos a serem filtrados. Default Input, InputOutput</param>
        /// <returns>Lista com todas os atributos, obedecendo o filtro.</returns>
        protected PersistencePropertyCollection GetPropertiesAttributes(PersistenceParameterType[] typesParam, DirectionParameter[] directions)
        {
            return GetPropertiesAttributes(typesParam, directions, false);
        }

        /// <summary>
        /// Captura os attributes PersistenceProperty das propriedades da classe refenciada.
        /// </summary>
        /// <param name="typesParam">Tipos de parametros a serem filtrados. null para não se aplicar nenhum filtro.</param>
        /// <param name="directions">Sentido dos atributos a serem filtrados. Default Input, InputOutput</param>
        /// <param name="returnFirstFound">True para retorna o primeiro valor encontrado.</param>
        /// <returns>Lista com todas os atributos, obedecendo o filtro.</returns>
        protected PersistencePropertyCollection GetPropertiesAttributes(PersistenceParameterType[] typesParam, DirectionParameter[] directions, bool returnFirstFound)
        {
            PersistencePropertyCollection listAttrs = new PersistencePropertyCollection();

            if (directions == null)
                directions = new DirectionParameter[] { DirectionParameter.Input, DirectionParameter.InputOptionalOutput, DirectionParameter.InputOutput, DirectionParameter.OutputOnlyInsert, DirectionParameter.InputOptional };

            // Captura o tipo da classe a ser tratada
            bool itemFound;

            LoadPersistencePropertyAttributes();

            //if (GetPropertiesAttributes(ref typesParam, ref directions, ref returnFirstFound, ref listAttrs, ref type))
            //    return listAttrs;

            foreach (PersistencePropertyAttribute ppa in collectionPersistenceProperties)
            {
                if (typesParam != null)
                {
                    itemFound = false;
                    // Verifica se o atributo está entre os tipos desejados
                    foreach (PersistenceParameterType ppt in typesParam)
                        if (ppt == ppa.ParameterType)
                        {
                            itemFound = true;
                            break;
                        }

                    // Se não encontrar o item do filtro, pula para a próxima propriedade
                    if (!itemFound) continue;
                }

                if (directions != null)
                {
                    itemFound = false;

                    foreach (DirectionParameter dp in directions)
                        if (dp == ppa.Direction)
                        {
                            itemFound = true;
                            break;
                        }

                    // Se não encontrar o item do filtro, pula para a próxima propriedade
                    if (!itemFound) continue;
                }

                listAttrs.Add(ppa);

                // Retorna o primeiro valor encontrado.
                if (returnFirstFound) break;
            }

            return listAttrs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd">Command aonde o parametro será relacionado.</param>
        /// <param name="ppa"></param>
        /// <param name="itemData"></param>
        /// <returns>Nome do parametro criado.</returns>
        protected string CreateDataParameter(ref IDbCommand cmd, PersistencePropertyAttribute ppa, ref TEntity itemData)
        {
            // Busca o valor na model
            object value = typeof(TEntity).GetProperty(ppa.RepresentedProperty.Name).GetValue(itemData, null);

            // Marca o value como dbNull
            value = (value == null ? DBNull.Value : value);

            // Nome do parametro
            string parameterName = UserProvider.ParameterPrefix + ppa.Name;

            SendMessageDebugTrace("Create DataParameter -> Name: " + parameterName + "; Value: " + value.ToString());

            // Cria uma instância do parametro
            IDbDataParameter dbParam = cmd.CreateParameter();
            dbParam.ParameterName = parameterName;

            // verifica o valor do parametro
            dbParam.Value = ConvertToDatabaseType(value, ppa);

            // Adiciona o parametro ao comando
            cmd.Parameters.Add(dbParam);

            return parameterName;
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        ///  Inicia a Transação
        /// </summary>
        /// <returns></returns>
        public IDbTransaction BeginTransaction()
        {
            if (Connection != null)
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();
                ConnectionAlwaysOpened = true;
                if (this.providerConfig.Provider.Name.ToUpper().Contains("SQLITE"))
                    new ArgumentNullException("not exist implement native SqliteTransaction");
                else
                    transaction = Connection.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
                return transaction;
            }
            else
                throw new ArgumentNullException("Connection cannot be null");
        }

        /// <summary>
        /// Realiza o Commit
        /// </summary>
        public void Commit()
        {
            if (transaction != null)
            {
                if (Connection.State == ConnectionState.Open)
                {
                    transaction.Commit();
                    Connection.Close();
                }
            }
        }

        /// <summary>
        /// Realiza o Rollback
        /// </summary>
        public void Rollback()
        {
            if (transaction != null)
            {
                transaction.Rollback();
                Connection.Close();
            }
            else
                throw new ArgumentNullException("Transaction cannot be null.");
        }

        /// <summary>
        /// Inseri os dados contidos no objInsert no BD.
        /// </summary>
        /// <param name="objInsert">Objeto com os dados a serem inseridos.</param>
        /// <returns>Chave inserido.</returns>
        public int Insert(TEntity objInsert)
        {
            return Insert(objInsert, InsertIdentityKeyBehavior.DatabaseSet);
        }

        /// <summary>
        /// Inseri os dados contidos no objInsert no BD.
        /// </summary>
        /// <param name="objInsert">Objeto com os dados a serem inseridos</param>
        /// <param name="insertBehavior">Indica o tratamento que será dado aos parametros do tipo IdentityKey</param>
        /// <returns></returns>
        public int Insert(TEntity objInsert, InsertIdentityKeyBehavior insertBehavior)
        {
            if (objInsert == null)
                throw new ArgumentNullException("ObjInsert cannot be null.");

            if (UserProvider.GenerateIdentity && (UserProvider.SqlQueryReturnIdentuty == "" || UserProvider.SqlQueryReturnIdentuty == null))
                throw new Exception("SqlQueryReturnIdentity não definido no provider.");

            SendMessageDebugTrace("ADO.NET call method insert.");
            IDbCommand cmd = UserProvider.CreateCommand();

            int returnValue = 0;
            string fieldsInsert = "", paramsInsert = "";

            // Captura a lista de attributos a serem tratados
            List<PersistencePropertyAttribute> list;

            if (insertBehavior == InsertIdentityKeyBehavior.ManuallySet)
            {
                list = GetPropertiesAttributes(new PersistenceParameterType[] { PersistenceParameterType.Field, PersistenceParameterType.ForeignKey, PersistenceParameterType.Key, PersistenceParameterType.IdentityKey },
                       new DirectionParameter[] { DirectionParameter.Output, DirectionParameter.InputOptionalOutput, DirectionParameter.InputOutput, DirectionParameter.OutputOnlyInsert, DirectionParameter.OnlyInsert, DirectionParameter.InputOptionalOutputOnlyInsert });
            }
            else
            {
                list = GetPropertiesAttributes(new PersistenceParameterType[] { PersistenceParameterType.Field, PersistenceParameterType.ForeignKey, PersistenceParameterType.Key },
                       new DirectionParameter[] { DirectionParameter.Output, DirectionParameter.InputOptionalOutput, DirectionParameter.InputOutput, DirectionParameter.OutputOnlyInsert, DirectionParameter.OnlyInsert, DirectionParameter.InputOptionalOutputOnlyInsert });

            }

            foreach (PersistencePropertyAttribute ppa in list)
            {
                fieldsInsert += ppa.Name + ",";
                paramsInsert += CreateDataParameter(ref cmd, ppa, ref objInsert) + ",";
            }


            // Verifica se a IdentityKey deve ser inserida com o valor máximo + 1
            if (insertBehavior == InsertIdentityKeyBehavior.AutomaticallySet || insertBehavior == InsertIdentityKeyBehavior.AutomaticallyTempSet)
            {
                int value = 0;

                // Gera Identidade automaticamente
                // Recupera a propriedade que representa a chave identidade da model.
                PersistencePropertyAttribute identityKey = GetPropertyIdentityKey();

                if (identityKey != null)
                {
                    // Verifica se a propriedade é do tipo int
                    if (identityKey.RepresentedProperty.PropertyType == typeof(int))
                    {
                        // Gera a chave
                        returnValue = GetNewKey(identityKey.Name);

                        //Verifica se o valor deve ser gerado acrescentado de 900.000.000
                        if (insertBehavior == InsertIdentityKeyBehavior.AutomaticallyTempSet)
                        {
                            if (returnValue < 900000000) returnValue += 900000000;

                        }

                        // Preenche o objInsert com o nova valor da chave
                        identityKey.RepresentedProperty.SetValue(objInsert, returnValue, null);

                        value = returnValue;

                        fieldsInsert = identityKey.Name + "," + fieldsInsert;
                        paramsInsert = UserProvider.ParameterPrefix + identityKey.Name + "," + paramsInsert;

                        // Adiciona o parametro com o valor da chave
                        IDbDataParameter dbParam = cmd.CreateParameter();
                        dbParam.ParameterName = UserProvider.ParameterPrefix + identityKey.Name;
                        dbParam.Value = value;
                        cmd.Parameters.Add(dbParam);
                    }
                    else
                        throw new NotSupportedException("Não é possivel gera um chave identidade para uma propriedade do tipo: " + identityKey.RepresentedProperty.PropertyType.FullName);
                }
                else
                    returnValue = 0;

            }
            // Fim Geração da Identidade


            cmd.Connection = Connection;
            cmd.CommandText = "insert into " + TableName + " (" + fieldsInsert.Substring(0, fieldsInsert.Length - 1) +
                              ")values(" + paramsInsert.Substring(0, paramsInsert.Length - 1) + ");";

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            try
            {
                // Verifica se existe alguma campo identidade idenficado no mapeamento da classe
                // e se o campo identidade deve ser preenchido pelo banco
                if (UserProvider.GenerateIdentity && insertBehavior == InsertIdentityKeyBehavior.DatabaseSet)
                {
                    if (!UserProvider.ExecuteCommandsOneAtATime)
                    {
                        SendMessageDebugTrace("CommandText: " + cmd.CommandText);
                        // Inseri o comando para recuperar a identidade gerada
                        cmd.CommandText += UserProvider.SqlQueryReturnIdentuty;
                        returnValue = int.Parse(cmd.ExecuteScalar().ToString());
                    }
                    else
                    {
                        SendMessageDebugTrace("Executing commands one at a time");
                        SendMessageDebugTrace("CommandText: " + cmd.CommandText);
                        // Executa o comando para inserir os dados
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();

                        SendMessageDebugTrace(UserProvider.SqlQueryReturnIdentuty);
                        // Carrega o comando para recuperar a identidade gerada
                        cmd.CommandText = UserProvider.SqlQueryReturnIdentuty;
                        // Recupera a chave identidade gerada
                        string temp = cmd.ExecuteScalar().ToString();
                        if (temp != "") returnValue = int.Parse(temp);
                    }
                }
                else
                {
                    SendMessageDebugTrace("CommandText: " + cmd.CommandText);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                string errorMsg = ex.InnerException != null ? ex.Message + ". " + ex.InnerException.Message : ex.Message;
                throw new Exception(errorMsg, ex);
            }
            finally
            {
                if (!m_ConnectionAlwaysOpened)
                    Connection.Close();
            }

            return returnValue;
        }

        /// <summary>
        /// <para>Inseri os dados contidos no objInsert não levando em consideração a chave identidade.</para>
        /// <para>Ou seja, constroi um comando sql com todos os dados do objeto que obedenção os parametros
        /// de direção InputOutput, Input, InputOnlyInsert.</para>
        /// </summary>
        /// <param name="objInsert">Objeto com os dados a serem inseridos.</param>
        /// <returns>Número de linhas afetadas.</returns>
        public int InsertForced(TEntity objInsert)
        {
            if (objInsert == null)
                throw new ArgumentNullException("ObjInsert it cannot be null.");

            IDbCommand cmd = UserProvider.CreateCommand();

            int returnValue = 0;
            string fieldsInsert = "", paramsInsert = "";

            // Captura a lista de attributos a serem tratados
            List<PersistencePropertyAttribute> list = GetPropertiesAttributes(null,
                    new DirectionParameter[] { DirectionParameter.Output, DirectionParameter.InputOutput, DirectionParameter.OutputOnlyInsert, DirectionParameter.OnlyInsert });

            foreach (PersistencePropertyAttribute ppa in list)
            {
                fieldsInsert += ppa.Name + ",";
                paramsInsert += CreateDataParameter(ref cmd, ppa, ref objInsert) + ",";
            }

            cmd.Connection = Connection;
            cmd.CommandText = "insert into " + TableName + " (" + fieldsInsert.Substring(0, fieldsInsert.Length - 1) +
                              ")values(" + paramsInsert.Substring(0, paramsInsert.Length - 1) + ");";

            bool close = true;

            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
                close = false;
            }

            try
            {
                SendMessageDebugTrace("CommandText: " + cmd.CommandText);
                returnValue = int.Parse(cmd.ExecuteNonQuery().ToString());
            }
            finally
            {
                if (close)
                    Connection.Close();
            }

            return returnValue;
        }

        /// <summary>
        /// Atualiza os dados contidos no objUpdate no BD.
        /// </summary>
        /// <param name="objUpdate">Objeto com os dados a serem atualizados.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <returns>Número de linhas afetadas.</returns>
        public int Update(TEntity objUpdate)
        {
            if (objUpdate == null)
                throw new ArgumentNullException("ObjUpdate it cannot be null.");

            IDbCommand cmd = UserProvider.CreateCommand();
            int returnValue = 0;
            string sqlQuery = "update " + TableName + " set ", clauseWhere = "";


            // Busca a lista dos campos a serem atualizados
            List<PersistencePropertyAttribute> listAttr = GetPropertiesAttributes(new PersistenceParameterType[] { PersistenceParameterType.Field, PersistenceParameterType.ForeignKey },
                                                new DirectionParameter[] { DirectionParameter.Output, DirectionParameter.InputOptionalOutput, DirectionParameter.InputOutput });

            foreach (PersistencePropertyAttribute ppa in listAttr)
            {
                sqlQuery += ppa.Name + "=" + CreateDataParameter(ref cmd, ppa, ref objUpdate) + ",";
            }

            // Carrega a lista de atributos para serem usados como condição
            listAttr = GetPropertiesAttributes(new PersistenceParameterType[] {
                PersistenceParameterType.Key, PersistenceParameterType.IdentityKey},
                new DirectionParameter[] { DirectionParameter.Output, DirectionParameter.InputOptionalOutput, DirectionParameter.InputOutput });

            // Verifica se existem parametros para contruir a cláusula codicional
            if (listAttr.Count == 0)
                throw new ConditionalClauseException("Parameters do not exist to build the conditional clause.");

            // Cria os parametros de condição
            foreach (PersistencePropertyAttribute ppa in listAttr)
            {
                if (clauseWhere.Length != 0) clauseWhere += " and ";
                clauseWhere += ppa.Name + "=" + CreateDataParameter(ref cmd, ppa, ref objUpdate);
            }

            cmd.Connection = Connection;

            cmd.CommandText = sqlQuery.Substring(0, sqlQuery.Length - 1) + " where " + clauseWhere;

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            try
            {
                SendMessageDebugTrace("CommandText: " + cmd.CommandText);
                returnValue = cmd.ExecuteNonQuery();
            }
            finally
            {
                if (!m_ConnectionAlwaysOpened)
                    Connection.Close();
            }

            return returnValue;

        }

        /// <summary>
        /// Remove o item do BD que o objDelete representa.
        /// </summary>
        /// <param name="objDelete">Objeto com os dados a serem Removidos.</param>
        /// <returns>Número de linhas afetadas.</returns>
        public int Delete(TEntity objDelete)
        {
            if (objDelete == null)
                throw new ArgumentNullException("ObjDelete it cannot be null.");

            IDbCommand cmd = UserProvider.CreateCommand();
            int returnValue = 0;
            string clauseWhere = "";
            // Carrega a lista de atributos para serem usados como condição
            List<PersistencePropertyAttribute> listAttr = GetPropertiesAttributes(new PersistenceParameterType[] {
                PersistenceParameterType.Key, PersistenceParameterType.IdentityKey},
                new DirectionParameter[] { DirectionParameter.Output, DirectionParameter.InputOptionalOutput, DirectionParameter.InputOutput });


            // Verifica se existem parametros para contruir a cláusula codicional
            if (listAttr.Count == 0)
                throw new ConditionalClauseException("Parameters do not exist to contruir the conditional clause.");

            // Cria as clausulas
            foreach (PersistencePropertyAttribute ppa in listAttr)
            {
                if (clauseWhere.Length != 0) clauseWhere += " and ";
                clauseWhere += ppa.Name + "=" + CreateDataParameter(ref cmd, ppa, ref objDelete);
            }

            cmd.Connection = Connection;

            cmd.CommandText = "delete from " + TableName + " where " + clauseWhere;

            Connection.Open();

            try
            {
                SendMessageDebugTrace("CommandText: " + cmd.CommandText);
                returnValue = cmd.ExecuteNonQuery();
            }
            finally
            {
                Connection.Close();
            }

            return returnValue;

        }

        /// <summary>
        /// Executa o camando que retorna o um número de registros.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int GetNumberRegFound(params Parameter[] parameters)
        {
            IDbCommand cmd = UserProvider.CreateCommand();
            int value = 0;

            string sqlParam = "", sql = "select count(*) from " + TableName;
            cmd.Connection = Connection;

            if (parameters != null)
                foreach (Parameter p in parameters)
                {
                    DbParameter param = providerConfig.Provider.CreateParameter();
                    param.ParameterName = p.ParameterName.Replace("?", UserProvider.ParameterPrefix);
                    param.Value = p.Value;

                    if (sqlParam != "")
                        sqlParam += " and ";

                    if (p.SourceColumn == null || p.SourceColumn == "")
                        throw new Exception("Name field that represent parameter \"" + p.ParameterName + "\" there isn'to.");

                    sqlParam += p.SourceColumn + "=" + param.ParameterName;

                    SendMessageDebugTrace("Create DataParameter -> Name: " + param.ParameterName + "; Value: " + param.Value + "; FieldName: " + param.SourceColumn);
                    cmd.Parameters.Add(param);
                }

            if (sqlParam != "")
                sql += " where ";

            cmd.CommandText = sql + sqlParam;
            Connection.Open();

            try
            {
                SendMessageDebugTrace("CommandText: " + cmd.CommandText);
                value = int.Parse(cmd.ExecuteScalar().ToString());
                SendMessageDebugTrace("Return: " + value.ToString());
            }
            finally
            {
                Connection.Close();
            }

            return value;
        }

        /// <summary>
        /// Executa o camando que retorna o um número de registros.
        /// </summary>
        /// <returns></returns>
        public int GetNumberRegFound()
        {
            return GetNumberRegFound(null);
        }

        /// <summary>
        /// Executa comandos sql.
        /// </summary>
        /// <param name="sqlQuery">Causa sql a ser executada.</param>
        /// <param name="useTransaction">Indica a necessidade de usar transaction.</param>
        /// <param name="parameters"></param>
        public int ExecuteCommand(string sqlQuery, bool useTransaction, params Parameter[] parameters)
        {
            if (sqlQuery == null)
                throw new ArgumentNullException("sqlQuery");
            else if (sqlQuery == "")
                throw new ArgumentException("sqlQuery cannot empty.");

            int valueReturn = 0;

            IDbCommand command = Connection.CreateCommand();

            command.Connection = Connection;

            if (parameters != null)
                for (int i = 0; i < parameters.Length; i++)
                {
                    string pName = parameters[i].ParameterName;
                    parameters[i].ParameterName = pName.Replace("?", UserProvider.ParameterPrefix);

                    sqlQuery = sqlQuery.Replace(pName, parameters[i].ParameterName);

                    IDbDataParameter p = command.CreateParameter();
                    p.ParameterName = parameters[i].ParameterName;
                    p.Value = parameters[i].Value;

                    if (p.Value == null)
                        p.Value = DBNull.Value;

                    command.Parameters.Add(p);
                }

            command.CommandText = sqlQuery;
            SendMessageDebugTrace(sqlQuery);

            Connection.Open();

            //command.CommandType = System.Data.CommandType.StoredProcedure;

            /*if (objData!=null)
                foreach (System.Reflection.PropertyInfo propInfo in properties)
                {
                    // Gera o parametros
                    object[] propertyAttributes = propInfo.GetCustomAttributes(typeof(PersistencePropertyAttribute), true);
                    command.Parameters.Add(CreateSqlParameter(objData, propInfo, propertyAttributes));
                }
            */
            if (useTransaction)
            {
                SendMessageDebugTrace("Begin Transaction");
                transaction = conn.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
                try
                {
                    SendMessageDebugTrace(command.CommandText);
                    valueReturn = command.ExecuteNonQuery();
                    SendMessageDebugTrace("Number of rows affected.");

                    SendMessageDebugTrace("Commit transaction");
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    SendMessageDebugTrace("Falied execute command\nRollback transaction");
                    transaction.Rollback();
                    throw ex;
                }
                finally
                {
                    Connection.Close();
                }
            }
            else
            {
                try
                {
                    SendMessageDebugTrace(command.CommandText);
                    valueReturn = command.ExecuteNonQuery();
                    SendMessageDebugTrace("Return: " + valueReturn.ToString());
                }
                catch (Exception ex)
                {
                    throw new Exception("SqlQuery: " + sqlQuery + "; Exception: " + ex.Message);
                }
                finally
                {
                    Connection.Close();
                }
            }


            return valueReturn;
        }

        /// <summary>
        /// Executa comandos sql.
        /// </summary>
        /// <param name="sqlQuery">Causa sql a ser executada.</param>
        /// <param name="useTransaction">Indica a necessidade de usar transaction.</param>
        public int ExecuteCommand(string sqlQuery, bool useTransaction)
        {
            return ExecuteCommand(sqlQuery, useTransaction, null);
        }

        /// <summary>
        /// Executa comandos sql.
        /// </summary>
        /// <param name="sqlQuery">Causa sql a ser executada.</param>
        public int ExecuteCommand(string sqlQuery)
        {
            return ExecuteCommand(sqlQuery, false);
        }


        public int InsertOrUpdate(TEntity objData)
        {
            return InsertOrUpdate(objData, InsertIdentityKeyBehavior.DatabaseSet);
        }

        /// <summary>
        /// Se o registro já existir na BD os dados serão atualizados, caso não existe um novo registro é criado.
        /// </summary>
        /// <param name="objData">Objeto conténdo os dados a serem utilizados na transação.</param>
        /// <exception cref="Exception">Se o tipo de dados utilizado não possuir chaves.</exception>
        /// <returns>0 se o registro for atualizado, senão a identidade do novo registro inserido.</returns>
        public int InsertOrUpdate(TEntity objData, InsertIdentityKeyBehavior insertIdentityKeyBehavior)
        {
            // Captura a lista de chave que a tabela possui
            PersistencePropertyCollection listKeys = Keys;
            Type type = typeof(TEntity);

            if (listKeys.Count == 0)
                throw new Exception("Invalid operation. Object of type \"" + type.FullName + "\" don't have keys.");

            int i = 0;
            Parameter[] parameters = new Parameter[listKeys.Count];

            foreach (PersistencePropertyAttribute ppa in listKeys)
            {
                parameters[i] = new Parameter();
                parameters[i].ParameterName = UserProvider.ParameterPrefix + ppa.RepresentedProperty.Name;
                parameters[i].Value = ppa.RepresentedProperty.GetValue(objData, null);

                if (parameters[i].Value == null)
                    parameters[i].Value = DBNull.Value;

                parameters[i].SourceColumn = ppa.Name;
                i++;
            }

            //Verifica se o número de registros atualizados é igual a zero
            if (Update(objData) == 0)
                //Insere Registro
                return Insert(objData, insertIdentityKeyBehavior);
            else
                return 0;


            /*int numRegFound = GetNumberRegFound(parameters);

            if (numRegFound == 0)
                return Insert(objData, insertIdentityKeyBehavior);
            else if (numRegFound == 1)
            {
                Update(objData);
                return 0;
            }
            else
                throw new Exception("There are duplicate keys on database");*/

        }

        public int ExecuteSqlQueryCount(string sqlQuery, params Parameter[] parameters)
        {
            object value = ExecuteScalar(sqlQuery, parameters);
            if (value != null)
                return int.Parse(value.ToString());
            else
                return 0;
        }

        public object ExecuteScalar(string sqlQuery, params Parameter[] parameters)
        {
            IDbCommand cmd = Connection.CreateCommand();
            object returnValue;

            if (parameters != null)
                for (int i = 0; i < parameters.Length; i++)
                {
                    string pName = parameters[i].ParameterName;
                    parameters[i].ParameterName = pName.Replace("?", UserProvider.ParameterPrefix);
                    sqlQuery = sqlQuery.Replace(pName, parameters[i].ParameterName);

                    IDbDataParameter p = cmd.CreateParameter();
                    p.ParameterName = parameters[i].ParameterName;
                    p.Value = parameters[i].Value;

                    cmd.Parameters.Add(p);
                }

            cmd.CommandText = sqlQuery;

            SendMessageDebugTrace(sqlQuery);

            Connection.Open();

            try
            {
                returnValue = cmd.ExecuteScalar();

                if (returnValue != null)
                    SendMessageDebugTrace("Return: " + returnValue.ToString());
                else
                    SendMessageDebugTrace("Return: null");

            }
            finally
            {
                Connection.Close();
            }

            return returnValue;
        }

        public object ExecuteScalar(string sqlQuery)
        {
            return ExecuteScalar(sqlQuery, null);
        }

        #endregion
    }
}
