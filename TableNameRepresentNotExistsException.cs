using System;
using System.Collections.Generic;
using System.Text;

namespace GAB
{
    /// <summary>
    /// 
    /// </summary>
    class TableNameRepresentNotExistsException : Exception
    {
        public TableNameRepresentNotExistsException(string message)
            : base(message)
        {

        }
    }
}
