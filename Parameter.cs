using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;

namespace GAB
{
    public class Parameter : DbParameter, IDataParameter
    {
        #region Variáveis Locais

        private System.Data.DbType type;
        private string m_ParameterName;
        private object m_Value;
        private int m_Size;
        private string m_SourceColumn;
        private ParameterDirection m_Direction;
        private bool m_IsNullable;

        #endregion

        #region Construtores

        /// <summary>
        /// Cria um parâmetro do ADO.NET
        /// </summary>
        public Parameter()
        {

        }

        /// <summary>
        /// Cria um parâmetro do ADO.NET passando o nome do parâmetro e o valor
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        public Parameter(string parameterName, object value)
        {
            m_ParameterName = parameterName;
            m_Value = value;
        }

        /// <summary>
        /// Cria um parâmetro do ADO.NET passando o nome do parâmetro, o valor e o tamanho
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="size"></param>
        public Parameter(string parameterName, object value, int size)
            : this(parameterName, value)
        {
            m_Size = size;
        }

        /// <summary>
        /// Cria um parâmetro do ADO.NET passando o nome do parâmetro e o nome do campo no BD
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="fieldName"></param>
        public Parameter(string parameterName, object value, string fieldName)
            : this(parameterName, value)
        {
            m_SourceColumn = fieldName;
        }

        /// <summary>
        /// Cria um parâmetro do ADO.NET passando o nome do parâmetro, 
        /// o valor o nome do campo no BD e o seu tamanho
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="fieldName"></param>
        /// <param name="size"></param>
        public Parameter(string parameterName, object value, string fieldName, int size)
            : this(parameterName, value, fieldName)
        {
            m_Size = size;
        }

        #endregion

        #region Propriedades

        public override DbType DbType 
        {
            get { return type; }
            set { type = value; }
        }

        public override string ParameterName
        {
            get
            {
                return m_ParameterName;
            }
            set
            {
                m_ParameterName = value;
            }
        }

        public override object Value
        {
            get
            {
                return m_Value;
            }
            set
            {
                m_Value = value;
            }
        }

        public override int Size
        {
            get
            {
                return m_Size;
            }
            set
            {
                m_Size = value;
            }
        }

        [Obsolete("Property is obsolete, see SourceColumn.")]
        public string FieldName
        {
            get { return m_SourceColumn; }
            set { m_SourceColumn = value; }
        }

        #endregion

        public override System.Data.ParameterDirection Direction
        {
            get { return m_Direction; }
            set { m_Direction = value; }
        }

        public override bool IsNullable
        {
            get { return m_IsNullable; }
            set { m_IsNullable = value; }
        }

        public override void ResetDbType()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override string SourceColumn
        {
            get { return m_SourceColumn; }
            set { m_SourceColumn = value; }
        }

        public override bool SourceColumnNullMapping
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public override System.Data.DataRowVersion SourceVersion
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }
    }
}
