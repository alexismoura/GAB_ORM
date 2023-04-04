using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Reflection;

namespace GAB
{
    /// <summary>
    /// Provider que irá fezer o controle DAO refrenciado para o projeto.
    /// </summary>
    public class Provider
    {
        #region Variáveis Locais

        private Type m_ConnectionType;
        private string m_ConnectionTypeName;
        private Type m_CommandType;
        private string m_CommandTypeName;
        private Type m_AdapterType;
        private string m_AdapterTypeName;
        private Type m_ParameterType;
        private string m_ParameterTypeName;
        private string m_ParameterPrefix;
        private string m_Name;

        private Assembly providerAssembly;
        private string m_AssemblyName;

        private string m_SqlQueryReturnIdentity;
        private bool m_GenerateIdentity = false;

        /// <summary>
        /// Identifica se os comando serão executados um de cada vez.
        /// </summary>
        private bool m_ExecuteCommandsOneAtATime = false;

        #endregion
        
        #region Propriedades

        /// <summary>
        /// Identifica se os comando serão executados um de cada vez.
        /// </summary>
        public bool ExecuteCommandsOneAtATime
        {
            get { return m_ExecuteCommandsOneAtATime; }
            set { m_ExecuteCommandsOneAtATime = value; }
        }

        /// <summary>
        /// Query sql que retorna a identidade gerada no auto incremental.
        /// </summary>
        public string SqlQueryReturnIdentuty
        {
            get { return m_SqlQueryReturnIdentity; }
        }

        /// <summary>
        /// Identifica se a identidade da chaves da tabelas serão geradas pelo BD ou pela aplicação.
        /// True quando a aplicação gera a identidade.
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool GenerateIdentity
        {
            get { return m_GenerateIdentity; }
            set { m_GenerateIdentity = value; }
        }

        /// <summary>
        /// Carrega o tipo de classe que cuida da conexão.
        /// </summary>
        public Type ConnectionType
        {
            get
            {
                if (m_ConnectionType == null)
                {
                    m_ConnectionType = ProviderAssembly.GetType(m_ConnectionTypeName, false);
                    if (m_ConnectionType == null)
                    {
                        throw new Exception(string.Format("Não é possível carrega a classe de conexão: {0} do assmbly: {1}", m_ConnectionTypeName, m_AssemblyName));
                    }
                }
                return m_ConnectionType;

            }
        }

        /// <summary>
        /// Carrega o tipo de classe que cuida do command sql.
        /// </summary>
        public Type CommandType
        {
            get
            {
                if (m_CommandType == null)
                {
                    m_CommandType = ProviderAssembly.GetType(m_CommandTypeName, false);
                    if (m_CommandType == null)
                    {
                        throw new Exception(string.Format("Não é possível carrega a classe de commando: {0} do assmbly: {1}", m_CommandTypeName, m_AssemblyName));
                    }
                }
                return m_CommandType;

            }
        }

        /// <summary>
        /// Carrega o tipo de classe que cuida do DataAdapter do provider.
        /// </summary>
        public Type DataAdapterType
        {
            get
            {
                if (m_AdapterType == null)
                {
                    m_AdapterType = ProviderAssembly.GetType(m_AdapterTypeName, false);
                    if (m_AdapterType == null)
                    {
                        throw new Exception(string.Format("Não é possível carrega a classe de adapter: {0} do assmbly: {1}", m_AdapterTypeName, m_AssemblyName));
                    }
                }
                return m_AdapterType;

            }
        }

        /// <summary>
        /// Carrega o tipo de classe que cuida do paramater do provider.
        /// </summary>
        public Type ParameterType
        {
            get
            {
                if (m_ParameterType == null)
                {
                    m_ParameterType = ProviderAssembly.GetType(m_ParameterTypeName, false);
                    if (m_ParameterType == null)
                    {
                        throw new Exception(string.Format("Não é possível carrega a classe de paramater: {0} do assmbly: {1}", m_ParameterTypeName, m_AssemblyName));
                    }
                }
                return m_ParameterType;

            }
        }

        /// <summary>
        /// Nome do do tipo de classe de acesso do provider.
        /// </summary>
        public string Name
        {
            get
            {
                return m_Name;
            }
        }

        /// <summary>
        /// Prefixo usado nos paramteros.
        /// </summary>
        public string ParameterPrefix
        {
            get
            {
                return m_ParameterPrefix;
            }
        }

        #endregion

        #region Construtores

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="name">Nome da biblioteca ADO a ser tratada pelo provider.</param>
        /// <param name="connection">Type do connection.</param>
        /// <param name="dataAdapter">Type do dataAdapter.</param>
        /// <param name="command">Type do command.</param>
        public Provider(string name, Type connection, Type dataAdapter, Type command)
        {
            // Verificação de tipos nulos
            if (connection == null)
                throw new ArgumentNullException("connection");
            else if (dataAdapter == null)
                throw new ArgumentNullException("dataAdapter");
            else if (command == null)
                throw new ArgumentNullException("command");
            
            m_Name = name;
            m_ConnectionTypeName = connection.FullName;
            m_AdapterTypeName = dataAdapter.FullName;
            m_CommandTypeName = command.FullName;
            m_ConnectionType = connection;
            m_AdapterType = dataAdapter;
            m_CommandType = command;
        }

        public Provider(string name, Type connection, Type dataAdapter, Type command, string sqlQueryReturnIdentity)
            : this(name, connection, dataAdapter, command)
        {
            m_SqlQueryReturnIdentity = sqlQueryReturnIdentity;
        }

        public Provider(string name, Type connection, Type dataAdapter, Type command, bool generateIdentity)
            : this(name, connection, dataAdapter, command)
        {
            m_GenerateIdentity = generateIdentity;
        }

        public Provider(string name, Type connection, Type dataAdapter, Type command, Type parameter, string parameterPrefix)
        {
            // Verificação de tipos nulos
            if (connection == null)
                throw new ArgumentNullException("connection");
            else if (dataAdapter == null)
                throw new ArgumentNullException("dataAdapter");
            else if (command == null)
                throw new ArgumentNullException("command");
            else if (parameter == null)
                throw new ArgumentNullException("parameter");
            else if (parameterPrefix == "" || parameterPrefix == null)
                throw new ArgumentNullException("paramterPrefix");

            m_Name = name;
            m_ConnectionTypeName = connection.FullName;
            m_AdapterTypeName = dataAdapter.FullName;
            m_CommandTypeName = command.FullName;
            m_ParameterTypeName = parameter.FullName;
            m_ConnectionType = connection;
            m_AdapterType = dataAdapter;
            m_CommandType = command;
            m_ParameterType = parameter;
            m_ParameterPrefix = parameterPrefix;
        }

        public Provider(string name, Type connection, Type dataAdapter, Type command, Type parameter, string parameterPrefix, string sqlQueryReturnIdentity)
            : this(name, connection, dataAdapter, command, parameter, parameterPrefix)
        {
            m_SqlQueryReturnIdentity = sqlQueryReturnIdentity;
        }

        public Provider(string name, Type connection, Type dataAdapter, Type command, Type parameter, string parameterPrefix, string sqlQueryReturnIdentity, bool generateIdentity)
            : this(name, connection, dataAdapter, command, parameter, parameterPrefix, sqlQueryReturnIdentity)
        {
            m_GenerateIdentity = generateIdentity;
        }

        public Provider(string name, Type connection, Type dataAdapter, Type command, Type parameter, string parameterPrefix, bool generateIdentity)
            : this(name, connection, dataAdapter, command, parameter, parameterPrefix)
        {
            m_GenerateIdentity = generateIdentity;
        }

        public Provider(string name, string assemblyName, string connection, string dataAdapter, string command)
        {
            m_Name = name;
            m_AssemblyName = assemblyName;
            m_ConnectionTypeName = connection;
            m_AdapterTypeName = dataAdapter;
            m_CommandTypeName = command;
        }

        public Provider(string name, string assemblyName, string connection, string dataAdapter, string command, string sqlQueryReturnIdentity)
            : this(name, assemblyName, connection, dataAdapter, command)
        {
            m_SqlQueryReturnIdentity = sqlQueryReturnIdentity;
        }

        public Provider(string name, string assemblyName, string connection, string dataAdapter, string command, bool generateIdentity)
            : this(name, assemblyName, connection, dataAdapter, command)
        {
            m_GenerateIdentity = generateIdentity;
        }

        public Provider(string name, string assemblyName, string connection, string dataAdapter, string command, string parameter, string parameterPrefix)
            : this(name, assemblyName, connection, dataAdapter, command)
        {
            m_ParameterTypeName = parameter;
            m_ParameterPrefix = parameterPrefix;
        }

        public Provider(string name, string assemblyName, string connection, string dataAdapter, string command, string parameter, string parameterPrefix, string sqlQueryReturnIdentity)
            : this(name, assemblyName, connection, dataAdapter, command, parameter, parameterPrefix)
        {
            m_SqlQueryReturnIdentity = sqlQueryReturnIdentity;
        }

        public Provider(string name, string assemblyName, string connection, string dataAdapter, string command, string parameter, string parameterPrefix, bool generateIdentity)
            : this(name, assemblyName, connection, dataAdapter, command, parameter, parameterPrefix)
        {
            m_GenerateIdentity = generateIdentity;
        }

        #endregion

        #region Métodos Locais

        /// <summary>
        /// Carrega o assembly do do namespace que contém os objetos de acesso.
        /// </summary>
        public Assembly ProviderAssembly
        {
            get
            {
                if (providerAssembly == null)
                {
                    // Verifica se o nome apresentado contém as informações completas sobre o 
                    // Library de acesso a dados
                    if (m_AssemblyName.IndexOf(',') == -1)
                    {
#if !PocketPC
                        // Carrega o assembly com dados parciais
                        providerAssembly = Assembly.LoadWithPartialName(m_AssemblyName);
#else
                        providerAssembly = Assembly.Load(m_AssemblyName);
#endif
                    }
                    else
                    {
                        // Carrega o assembly com os dados completos.
                        providerAssembly = Assembly.Load(m_AssemblyName);
                    }
                }
                return providerAssembly;
            }
        }

        #endregion

        #region Métodos Públicas

        /// <summary>
        /// Cria uma instância do connection que o provider representa.
        /// </summary>
        /// <returns>Connection.</returns>
        public IDbConnection CreateConnection()
        {
            object obj = null;
            obj = Activator.CreateInstance(ConnectionType);

            if (obj == null)
                throw new Exception(string.Format("Não é possível criar a classe connection: {0} do assmbly: {1}", m_ConnectionTypeName, m_AssemblyName));

            return (IDbConnection)obj;
        }

        /// <summary>
        /// Cria uma instância do command que o provider representa.
        /// </summary>
        /// <returns>Command.</returns>
        public IDbCommand CreateCommand()
        {
            object obj = null;
            obj = Activator.CreateInstance(CommandType);

            if (obj == null)
                throw new Exception(string.Format("Não é possível criar a classe command: {0} do assmbly: {1}", m_CommandTypeName, m_AssemblyName));

            return (IDbCommand)obj;
        }

        /// <summary>
        /// Cria uma instância do DataAdapter que o provider representa.
        /// </summary>
        /// <returns>DataAdapter.</returns>
        public IDbDataAdapter CreateDataAdapter()
        {
            object obj = Activator.CreateInstance(DataAdapterType);
            if (obj == null)
                throw new Exception(string.Format("Não é possível criar a classe adapter: {0} do assmbly: {1}", m_AdapterTypeName, m_AssemblyName));

            return (IDbDataAdapter)obj;
        }

        /// <summary>
        /// Cria uma instância do Parameter que o provider representa.
        /// </summary>
        /// <returns>Parameter.</returns>
        public System.Data.Common.DbParameter CreateParameter()
        {
            object obj = Activator.CreateInstance(ParameterType);
            if (obj == null)
                throw new Exception(string.Format("Não é possível criar a classe parameter: {0} do assmbly: {1}", m_ParameterTypeName, m_AssemblyName));

            return (System.Data.Common.DbParameter)obj;
        }

        #endregion
    }
}
