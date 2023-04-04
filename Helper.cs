using System;
using System.Collections.Generic;
using System.Text;

namespace GAB
{
    public class Helper
    {
        /// <summary>
        /// Verifica se a expressão de ordenação possui um item que represente reversão na ordenação.
        /// </summary>
        /// <param name="sortExpression">Expressão de ordenação.</param>
        /// <returns>True se existir reversão.</returns>
        public static bool SortExpression(string sortExpression)
        {
            return sortExpression.ToLower().EndsWith(" desc");
        }
    }
}
