using System.Text;

namespace NDbPortal
{
    public static class Utils
    {
        public static string GetSchemaQualifiedName(string tbName, string schema)
        {
            if (!string.IsNullOrWhiteSpace(schema))
            {
                return $"{schema}.{tbName}";

            }
            else{
                return tbName;
            }
        }


        public static string RemoveAll(string str, string[] stringToReplace)
        {
            var sb = new StringBuilder(str);

            foreach (var s in stringToReplace)
            {
                if (string.IsNullOrEmpty(s)) continue;
                sb.Replace(s, "");
            }

            return sb.ToString();
        }
    }
}
