using System.Collections.Generic;

namespace NDbPortal.Command
{
    public interface ICommand<T, TPrimary> where T : class
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
        bool Remove(TPrimary id);
        /// <summary>
        /// Remove <see cref="T"/> from database
        /// </summary>
        /// <param name="obj">Object to be removed</param>
        /// <returns></returns>
        bool Remove(T obj);

        /// <summary>
        /// Removes a list of object using their primary ids
        /// </summary>
        /// <param name="idsList"></param>
        /// <returns></returns>
        List<int> RemoveRange(List<TPrimary> idsList);

        /// <summary>
        /// Removes a list of object
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        List<int> RemoveRange(List<T> objs);

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
        //
        // Summary:
        //     Commits the database transaction.
        //
        // Exceptions:
        //   T:System.Exception:
        //     An error occurred while trying to commit the transaction.
        //
        //   T:System.InvalidOperationException:
        //     The transaction has already been committed or rolled back. -or- The connection
        //     is broken.
        void CommitTransaction();
        //
        // Summary:
        //     Rolls back a transaction from a pending state.
        //
        // Exceptions:
        //   T:System.Exception:
        //     An error occurred while trying to commit the transaction.
        //
        //   T:System.InvalidOperationException:
        //     The transaction has already been committed or rolled back. -or- The connection
        //     is broken.
        void RollbackTransaction();

    }
}
