using System.Linq;
using System.Reflection;
using NDbPortal.Names.MappingAttributes;

namespace NDbPortal
{
    public class ColumnInfo
    {
        public string ColumnName { get; set; }
        public bool IsDisplayColumn { get; set; }

        public static ColumnInfo FromMemberInfo(MemberInfo memberInfo)
        {
            var colAttr = memberInfo.GetCustomAttributes<ColumnAttribute>().FirstOrDefault();
            return new ColumnInfo()
            {
                ColumnName = colAttr.ColumnName,
                IsDisplayColumn = colAttr is DisplayColumnAttribute  
            };
        }
    }
}
