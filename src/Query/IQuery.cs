using NDbPortal.Names;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NDbPortal.Query
{
    interface IQuery<T> where T : class
    {

        /// <summary>
        /// Get the entity by its Id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>Entity Type</returns>
        T Get(long id);

        /// <summary>
        /// Gets all the entities
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Find an entity with given property
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        T Find(Expression<Func<T, bool>> expression);

        /// <summary>
        /// Find all entities with given property
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        IEnumerable<T> FindAll(Expression<Func<T, bool>> expression);

        /// <summary>
        /// Returns a paged of list of the given type
        /// </summary>
        /// <param name="page"></param>
        /// <param name="orderByColumn">order by column name</param>
        /// <returns></returns>
        PagedList<T> GetPagedList(long page, string orderByColumn = "id");

    }
}
