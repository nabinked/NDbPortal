using NDbPortal.Names;

namespace NDbPortal
{
    public interface ITableInfoBuilder<T>
    {

        ITableInfoBuilder<T> SetPrimaryKey();
        ITableInfoBuilder<T> SetTableName();
        ITableInfoBuilder<T> SetColumnInfos();
        TableInfo Build();
    }
}
