namespace NDbPortal
{
    public class DbEnums
    {
        public enum NamingConventions
        {
            UnderScoreCase = 1,
            PascalCase = 2
        }

        public enum DbType
        {
           Postgres=1,
           MsSql=2,
           MySql=3
        }
    }
}
