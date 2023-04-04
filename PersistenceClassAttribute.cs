using System;
using System.Collections.Generic;
using System.Text;

namespace GAB
{
    public class PersistenceClassAttribute : Attribute
    {
        #region Variáveis Locais

        private string m_Name;
        
        #endregion

        #region Propriedades

        /// <summary>
        /// Nome da tabela que a classe representa.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        #endregion

        #region Construtores

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="name">Nome da tabela que a classe representa.</param>
        public PersistenceClassAttribute(string name)
        {
            m_Name = name;
        }

        /// <summary>
        /// Construtor.
        /// </summary>
        public PersistenceClassAttribute()
        {

        }

        #endregion
    }
}
