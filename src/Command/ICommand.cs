using System;
using System.Collections.Generic;

namespace NDbPortal.Command
{
    public interface ICommand<in T> where T : class
    {
        /// <summary>
        /// Adds an entity
        /// </summary>
        /// <param name="obj">the object to be added</param>
        /// <returns>the id of the object</returns>
        long Add(T obj);

        /// <summary>
        /// Adds a range of entities
        /// </summary>
        /// <param name="entities">Ienumrable of entities</param>
        /// <returns>boolean indicating the success status of the task</returns>
        IList<long> AddRange(IEnumerable<T> entities);

        /// <summary>
        /// Updates an entity
        /// </summary>
        /// <param name="obj">the updated entity</param>
        /// <returns>no. of records affected</returns>
        long Update(T obj);

        /// <summary>
        /// Updates an entity
        /// </summary>
        /// <param name="entities"></param>
        /// <returns>no. of records affected</returns>
        long UpdateRange(IEnumerable<T> entities);


        /// <summary>
        /// Deletes an entity by its id
        /// </summary>
        /// <param name="id">id of the entity</param>
        /// <returns></returns>
        bool Remove(object id);
        
        /// <summary>
        /// Removes a list of object using their primary ids
        /// </summary>
        /// <param name="idsList"></param>
        /// <returns></returns>
        bool RemoveRange<TKey>(IEnumerable<TKey> idsList);

        /// <summary>
        /// Experimental method that updates entity if exists already otherwise creates a new one.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        long Upsert(T t);

    }
}
