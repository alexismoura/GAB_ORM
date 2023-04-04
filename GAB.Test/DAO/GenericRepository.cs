using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAB.Test.DAO
{
    /// <summary>
    ///  Repositorio Generico
    /// </summary>
    /// <typeparam name="Entity">Objeto Model(Entidade do banco)</typeparam>
    public class GenericRepository<Entity> where Entity : new()
    {
        /// <summary>
        /// Objeto de persistencia
        /// </summary>
        protected GAB.PersistenceObject<Entity> Persistence
        {
            get
            {
                return new GAB.PersistenceObject<Entity>(ProviderUtils.DbProvider);
            }
        }

        /// <summary>
        ///  Insert Basico
        /// </summary>
        /// <param name="entity"></param>
        public void Insert(Entity entity)
        {
            Persistence.Insert(entity);
        }

        /// <summary>
        ///  Update Basico
        /// </summary>
        /// <param name="entity"></param>
        public void Update(Entity entity)
        {
            Persistence.Update(entity);
        }

        /// <summary>
        ///  Delete Basico
        /// </summary>
        /// <param name="entity"></param>
        public void Delete(Entity entity)
        {
            Persistence.Delete(entity);
        }

        /// <summary>
        ///  List Basico
        /// </summary>
        /// <returns>Todos os registros do banco</returns>
        public List<Entity> List()
        {
            string sql = String.Format("Select * From {0} ", Persistence.TableName);
            return Persistence.LoadData(sql);
        }

    }
}
