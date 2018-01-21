using System.Collections.Generic;

namespace NDbPortal.Command
{
    interface ICommand<in T> where T : class
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
        bool AddRange(IEnumerable<T> entities);

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
        /// Deletes an entity
        /// </summary>
        /// <param name="id">id of the entity</param>
        /// <returns></returns>
        bool Remove(long id);

        /// <summary>
        /// Experimental method that updates entity if exists already otherwise creates a new one.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        long Upsert(T t);

        /// <summary>
        /// Begins a transaction
        /// </summary>
        void BeginTransaction();

    }
}
