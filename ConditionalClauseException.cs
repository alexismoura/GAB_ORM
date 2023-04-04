using System;
using System.Collections.Generic;
using System.Text;

namespace GAB
{
    /// <summary>
    /// Exception que remete os erros com as claúsulas condicionais.
    /// </summary>
    class ConditionalClauseException : Exception
    {
        public ConditionalClauseException(string message)
            : base(message)
        {

        }
    }
}
