using System;
using System.Collections.Generic;
using System.Text;

namespace GAB
{
    public class ProviderConfiguration
    {
        #region Variáveis Locais

        private Provider m_Provider;
        private string m_ConnectionString;

        #endregion

        #region Propriedades

        public Provider Provider
        {
            get { return m_Provider; }
            set { m_Provider = value; }
        }

        public string ConnectionString
        {
            get { return m_ConnectionString; }
            set { m_ConnectionString = value; }
        }

        public string Password { get; set; }

        #endregion

        #region Construtores

        public ProviderConfiguration(string connectionString, Provider provider)
        {
            m_ConnectionString = connectionString;
            m_Provider = provider;
        }

        public ProviderConfiguration(string connectionString, string password, Provider provider)
        {
            m_ConnectionString = connectionString;
            m_Provider = provider;
            Password = password;
        }

        #endregion

    }
}
