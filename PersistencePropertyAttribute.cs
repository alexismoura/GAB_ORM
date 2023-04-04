using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace GAB
{
    /// <summary>
    /// Define o tratamento que é dado aos parametros do tipo 
    /// IdentityKey durante as operações de Insert
    /// </summary>
    public enum InsertIdentityKeyBehavior
    {
        /// <summary>
        /// O banco de dados trata o parametro.
        /// Utilizado quando o atributo é do tipo AutoIncremental no banco de dados.
        /// </summary>
        DatabaseSet = 0,

        /// <summary>
        /// O ADO.NET atribui automaticamente o valor do atributo como máximo + 1.
        /// Simula um atributo do tipo AutoIncremental.
        /// </summary>
        AutomaticallySet,

        /// <summary>
        /// Utiliza o valor passado  como parametro.
        /// Trata o parametro IdentityKey como se fosse do tipo Key
        /// </summary>
        ManuallySet,

        /// <summary>
        /// O ADO.NET atribui automaticamente o valor do atributo como máximo + 1, se o máximo for maior do
        /// que 900.000.000, ou máximo + 1 + 900.000.000 se o valor for menor dor 900.000.000.
        /// Utilizado para gerar os identificadores temporarios.
        /// </summary>
        AutomaticallyTempSet
        
    }

    /// <summary>
    /// Identifica o tipo de parametro que a propriedade representa.
    /// </summary>
    public enum PersistenceParameterType
    {
        /// <summary>
        /// Idetifica um campo normal.
        /// </summary>
        Field = 0,
        /// <summary>
        /// Identifica um campo do tipo chave primária.
        /// </summary>
        Key,
        /// <summary>
        /// Identifica um campo do tipo chave primária identidade.
        /// </summary>
        IdentityKey,
        /// <summary>
        /// Identifica um campo do tipo chave estrangeira.
        /// </summary>
        ForeignKey

    }

    /// <summary>
    /// Identifica a direção em que os dados devem ser tratados no ADO.NET.
    /// </summary>
    public enum DirectionParameter
    {
        /// <summary>
        /// Identifica que o valor deverá apenas ser enviando para a base de dados.
        /// </summary>
        Output,
        /// <summary>
        /// Identifica que o valor deverá apenas ser recuperado da base de dados.
        /// </summary>
        Input,
        /// <summary>
        /// Identifica que o valor poderá ser enviado ou recuperado da base de dados.
        /// </summary>
        InputOutput,
        /// <summary>
        /// O parametro é inserido apenas pelo comando insert, mas ele também pode ser considerado como um Input.
        /// </summary>
        OutputOnlyInsert,

        /// <summary>
        /// O parametro é inserido apenas pelo comando insert
        /// </summary>
        OnlyInsert,
        /// <summary>
        /// O parametro busca o valor se ele existir no resultado,
        /// e ele se comportar da mesma forma que o parametro Output.
        /// </summary>
        InputOptionalOutput,
        /// <summary>
        /// O parametro busca o valor se ele existir no resultado.
        /// </summary>
        InputOptional,
        /// <summary>
        /// O parametro busca o valor se ele existir no resultado, e ele se comportar da mesma forma que o
        /// parametro Output que é inserido apenas pelo comando insert.
        /// </summary>
        InputOptionalOutputOnlyInsert
    }

    public class PersistencePropertyAttribute : Attribute
    {
        #region Variáveis Locais

        private string m_Name;
        private PersistenceParameterType m_ParameterType = PersistenceParameterType.Field;
        private int m_Size = 0;
        private PropertyInfo m_RepresentedProperty;
        private DirectionParameter m_Direction = DirectionParameter.InputOutput;

        #endregion

        #region Propriedades

        /// <summary>
        /// Nome que a propriedade representa no BD.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        /// <summary>
        /// Tipo de campo representado no banco de dados.
        /// </summary>
        public PersistenceParameterType ParameterType
        {
            get { return m_ParameterType; }
            set { m_ParameterType = value; }
        }

        /// <summary>
        /// Tamaho maximo do campo no BD.
        /// </summary>
        public int Size
        {
            get { return m_Size; }
            set { m_Size = value; }
        }

        /// <summary>
        /// Dados da propriedade que o attributo representa.
        /// </summary>
        public PropertyInfo RepresentedProperty
        {
            get { return m_RepresentedProperty; }
            set { m_RepresentedProperty = value; }
        }

        /// <summary>
        /// Sentido em que os dados da propriedade devem ser tratados pelo ADO.NET.
        /// </summary>
        /// <value>Valor default: InputOutput.</value>
        public DirectionParameter Direction
        {
            get { return m_Direction; }
            set { m_Direction = value; }
        }

        #endregion

        #region Construtores

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="name">Nome que o campo representa no BD.</param>
        public PersistencePropertyAttribute(string name)
        {
            m_Name = name;
        }

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="name">Nome que o campo representa no BD.</param>
        /// <param name="direction">Direção em que os dados devem ser tratados.</param>
        public PersistencePropertyAttribute(string name, DirectionParameter direction)
            : this(name)
        {
            m_Direction = direction;
        }
       
        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="name">Nome que o campo representa no BD.</param>
        /// <param name="type">Tipo do campo no BD.</param>
        public PersistencePropertyAttribute(string name, PersistenceParameterType parameterType)
        {
            m_Name = name;
            m_ParameterType = parameterType;
        }

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="name">Nome que o campo representa no BD.</param>
        /// <param name="type">Tipo do campo no BD.</param>
        /// <param name="direction">Direção em que os dados devem ser tratados.</param>
        public PersistencePropertyAttribute(string name, PersistenceParameterType parameterType, DirectionParameter direction)
            : this(name, parameterType)
        {
            m_Direction = direction;
        }

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="name">Nome que o campo representa no BD.</param>
        /// <param name="type">Tipo do campo no BD.</param>
        /// <param name="size">Tamanho que o campo.</param>
        public PersistencePropertyAttribute(string name, PersistenceParameterType parameterType, int size)
            : this(name, parameterType)
        {
            m_Size = size;
        }

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="name">Nome que o campo representa no BD.</param>
        /// <param name="type">Tipo do campo no BD.</param>
        /// <param name="size">Tamanho que o campo.</param>
        /// <param name="direction">Direção em que os dados devem ser tratados.</param>
        public PersistencePropertyAttribute(string name, PersistenceParameterType parameterType, int size, DirectionParameter direction)
            : this(name, parameterType, direction)
        {
            m_Size = size;
        }

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="name">Nome que o campo representa no BD.</param>
        /// <param name="size">Tamanho que o campo.</param>
        public PersistencePropertyAttribute(string name, int size)
        {
            m_Name = name;
            m_Size = size;
        }

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="name">Nome que o campo representa no BD.</param>
        /// <param name="size">Tamanho que o campo.</param>
        /// <param name="direction">Direção em que os dados devem ser tratados.</param>
        public PersistencePropertyAttribute(string name, int size, DirectionParameter direction)
        {
            m_Name = name;
            m_Size = size;
            m_Direction = direction;
        }


        #endregion

        #region Métodos Públicos
        
        public object DefaultValue(object obj)
        {
            return m_RepresentedProperty.GetValue(obj, null);
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
