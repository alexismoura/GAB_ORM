using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAB
{
    public interface IPersistenceObjectDataAccess<TEntity>
    {
        int Delete(TEntity objDelete);
        int Insert(TEntity objInsert);
        int InsertForced(TEntity objInsert);
        int InsertOrUpdate(TEntity objData);
        PersistencePropertyCollection Keys { get; }
        string TableName { get; }
        int Update(TEntity objUpdate);
    }
}
