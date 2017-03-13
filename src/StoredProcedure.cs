using System.Collections.Generic;

namespace NDbPortal
{
    public class StoredProcedure : IStoredProcedure
    {
        private readonly ICommandFactory _commandFactory;
        private readonly INamingConvention _namingConvention;

        public StoredProcedure(ICommandFactory commandFactory, INamingConvention namingConvention)
        {
            _commandFactory = commandFactory;
            _namingConvention = namingConvention;
        }
        /// <summary>
        /// Invokes a stored procedure
        /// </summary>
        /// <param name="name">name of the stored procedure</param>
        /// <param name="prms">parameters object</param>
        public void Invoke(string name, object prms = null)
        {
            using (var cmd = _commandFactory.Create(name, prms, true))
            {
                Mapper.ExecuteNonQuery(cmd);
            }
        }

        public T Get<T>(object prm = null)
        {

            var attr = ReflectionUtilities.GetEntityAttribute(typeof(T));
            using (var cmd = _commandFactory.Create(GetTableInfo<T>().FullTableName, prm, true))
            {
                return Mapper.GetObject<T>(cmd);

            }
        }

        private TableInfo GetTableInfo<T>()
        {
            return new TableInfoBuilder<T>(_namingConvention)
                            .SetColumnInfos()
                            .SetPrimaryKey()
                            .SetTableName()
                            .Build();
        }

        public IEnumerable<T> GetList<T>(object prm = null)
        {
            var attr = ReflectionUtilities.GetEntityAttribute(typeof(T));
            using (var cmd = _commandFactory.Create(GetTableInfo<T>().TableName, prm, true))
            {
                return Mapper.GetObjects<T>(cmd);

            }
        }

        public IEnumerable<T> GetPaged<T>(string pageProcName, int pageSize, int skip, string searchText,
            object prm = null)
        {
            var attr = ReflectionUtilities.GetEntityAttribute(typeof(T));
            using (var cmd = _commandFactory.Create(pageProcName, ReflectionUtilities.MergeObjects(new { sch = attr.Schema, tblName = attr.Name, pageSize, skip, searchText }, prm), true))
            {
                return Mapper.GetObjects<T>(cmd);
            }
        }

        public T Get<T>(string name, object prm = null)
        {
            using (var cmd = _commandFactory.Create(name, prm, true))
            {
                return Mapper.GetObject<T>(cmd);
            }
        }

        public IEnumerable<T> GetList<T>(string name, object prm = null)
        {
            using (var cmd = _commandFactory.Create(name, prm, true))
            {
                return Mapper.GetObjects<T>(cmd);

            }
        }
    }
}
