using System;
using System.Collections.Generic;
using System.Text;

namespace GAB
{
    public class InfoPaging
    {
        #region Variáveis Locais

        private int m_StartRow;
        private int m_PageSize;       

        #endregion

        #region Propriedades

        /// <summary>
        /// Linha inicial da Página.
        /// </summary>
        public int StartRow
        {
            get { return m_StartRow; }
            set { m_StartRow = value; }
        }

        /// <summary>
        /// Tamanho do página.
        /// </summary>
        public int PageSize
        {
            get { return m_PageSize; }
            set { m_PageSize = value; }
        }

        #endregion

        #region Construtores

        /// <summary>
        /// Construtor.
        /// </summary>
        public InfoPaging()
        {
        }

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="startRow">Linha inicial.</param>
        /// <param name="pageSize">Tamanho da página.</param>
        public InfoPaging(int startRow, int pageSize)
        {
            m_StartRow = startRow;
            m_PageSize = pageSize;
        }

        #endregion
    }

    public class InfoSortExpression
    {
        #region Variáveis Locais

        private string m_SortColumn;
        private bool m_Reverse;
        private string m_AliasTable;
        private string m_DefaultFieldSort;

        #endregion

        #region Propriedades

        /// <summary>
        /// Coluna a ser ordenada.
        /// </summary>
        public string SortColumn
        {
            get { return m_SortColumn; }
            set { m_SortColumn = value; }
        }

        /// <summary>
        /// Identifica a ordem da ordenação.
        /// </summary>
        public bool Reverse
        {
            get { return m_Reverse; }
            set { m_Reverse = value; }
        }

        /// <summary>
        /// Apelido da tabela na query sql.
        /// </summary>
        public string AliasTable
        {
            get { return m_AliasTable; }
            set { m_AliasTable = value; }
        }

        /// <summary>
        /// Campo que vem ordenado como padrão.
        /// </summary>
        public string DefaultFieldSort
        {
            get { return m_DefaultFieldSort; }
            set { m_DefaultFieldSort = value; }
        }

        #endregion

        #region Construtores

        public InfoSortExpression()
        {

        }

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="sortExpression">Expressão de ordenação que a GridView passa como parametro.</param>
        public InfoSortExpression(string sortExpression)
        {
            if (sortExpression == null || sortExpression == "") return;

            m_Reverse = sortExpression.ToLower().EndsWith(" desc");
            if (m_Reverse)
            {
                m_SortColumn = sortExpression.Substring(0, sortExpression.Length - 5);
            }
            else
            {
                m_SortColumn = sortExpression;
            }
        }

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="sortExpression"></param>
        /// <param name="defaultFieldSort">Campo que vem ordenado por padrão.</param>
        public InfoSortExpression(string sortExpression, string defaultFieldSort)
            : this(sortExpression)
        {
            m_DefaultFieldSort = defaultFieldSort;

            if (sortExpression == null || sortExpression == "")
                sortExpression = defaultFieldSort;
        }

         /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="sortExpression"></param>
        /// <param name="defaultFieldSort">Campo que vem ordenado por padrão.</param>
        /// <param name="aliasTable">Apelido da tabela aonde o campo está sendo ordenado.</param>
        public InfoSortExpression(string sortExpression, string defaultFieldSort, string aliasTable)
            : this(sortExpression, defaultFieldSort)
        {
            m_AliasTable = aliasTable;
        }

        #endregion

    }

    /// <summary>
    /// Classe usada para ordenar a lista.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Comparer<T> : IComparer<T>
    {
        private string m_SortColumn;
        private bool m_Reverse;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sortExpression">Expressão de ordenação que a GridView passa como parametro.</param>
        public Comparer(string sortExpression)
        {
            m_Reverse = sortExpression.ToLower().EndsWith(" desc");
            if (m_Reverse)
            {
                m_SortColumn = sortExpression.Substring(0, sortExpression.Length - 5);
            }
            else
            {
                m_SortColumn = sortExpression;
            }

        }

        public int Compare(T a, T b)
        {
            int retVal;
            Type type = typeof(T);
            string s1, s2;

            s1 = type.GetProperty(m_SortColumn).GetValue(a, null).ToString();
            s2 = type.GetProperty(m_SortColumn).GetValue(b, null).ToString();

            retVal = string.Compare(s1, s2, StringComparison.CurrentCulture);

            return (retVal * (m_Reverse ? -1 : 1));
        }

    }
}
