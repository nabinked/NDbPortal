using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using NDbPortal.Names;

namespace NDbPortal
{
    public interface IRepository<T> where T : class
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
        /// Adds an entity to the repository
        /// </summary>
        /// <param name="obj">the object to be added</param>
        /// <returns>the id of the object</returns>
        long Add(T obj);
        /// <summary>
        /// Adds a range of object
        /// </summary>
        /// <param name="entities">Ienumrable of entities</param>
        /// <returns>boolean indicating the success status of the task</returns>
        bool AddRange(IEnumerable<T> entities);

        /// <summary>
        /// Updates an entity
        /// </summary>
        /// <param name="obj">the updated entity</param>
        /// <returns>no. of records affected</returns>
        long Update(T obj);

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="id">id of the entity</param>
        /// <returns></returns>
        bool Remove(long id);

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

        long Upsert(T t);

        bool IsNew(T t);
    }
}
