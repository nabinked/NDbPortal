using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDbPortal
{
    public class NamingConvention : INamingConvention
    {


        #region Properties
        public DbEnums.NamingConventions DbNamingConvention { get; set; } = DbEnums.NamingConventions.UnderScoreCase;

        public DbEnums.NamingConventions PocoNamingConvention { get; set; } = DbEnums.NamingConventions.PascalCase;
        #endregion

        public string ConvertToPocoName(string dbName)
        {
            if (DbNamingConvention == PocoNamingConvention) return dbName;

            IList<string> rawNameList;
            switch (DbNamingConvention)
            {
                case DbEnums.NamingConventions.UnderScoreCase:
                    rawNameList = SplitUnderScoreNamingConvention(dbName);
                    break;
                case DbEnums.NamingConventions.PascalCase:
                    rawNameList = SplitPascalNamingConvention(dbName);
                    break;
                default:
                    return null;
            }
            return GetObjectNamesFromRawList(rawNameList);
        }

        public string ConvertToDbName(string objName)
        {
            if (DbNamingConvention == PocoNamingConvention) return objName;

            IEnumerable<string> rawNameList;
            switch (PocoNamingConvention)
            {
                case DbEnums.NamingConventions.UnderScoreCase:
                    rawNameList = SplitUnderScoreNamingConvention(objName);
                    break;
                case DbEnums.NamingConventions.PascalCase:
                    rawNameList = SplitPascalNamingConvention(objName);
                    break;
                default:
                    return null;
            }
            return GetColumnNamesFromRawList(rawNameList);
        }


        #region privates
        private string GetObjectNamesFromRawList(IEnumerable<string> rawNameList)
        {
            var nameList = rawNameList as IList<string> ?? rawNameList.ToList();
            switch (PocoNamingConvention)
            {
                case DbEnums.NamingConventions.PascalCase:
                    var strList = nameList.Select(ToFirstLetterUpper);
                    return string.Join("", strList);
                case DbEnums.NamingConventions.UnderScoreCase:
                    return string.Join("_", nameList);
            }

            return null;
        }

        private string GetColumnNamesFromRawList(IEnumerable<string> rawNameList)
        {
            var nameList = rawNameList as IList<string> ?? rawNameList.ToList();
            switch (DbNamingConvention)
            {
                case DbEnums.NamingConventions.UnderScoreCase:
                    return string.Join("_", nameList);
                case DbEnums.NamingConventions.PascalCase:
                    var tempList = nameList.Select(ToFirstLetterUpper).ToList();
                    return string.Join("", tempList);
                default:
                    return null;
            }
        }


        #region Extensions
        private string ToFirstLetterUpper(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var letters = value.ToCharArray();
            letters[0] = char.ToUpper(letters[0]);
            return new string(letters);
        }
        #endregion
        #region SplitStringsRegion
        private IList<string> SplitUnderScoreNamingConvention(string name)
        {
            return name.ToLower().Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private IList<string> SplitPascalNamingConvention(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var newText = new StringBuilder(name.Length * 2);
            newText.Append(name[0]);
            for (var i = 1; i < name.Length; i++)
            {
                if (char.IsUpper(name[i]) && name[i - 1] != ' ')
                {
                    newText.Append(' ');
                }
                newText.Append(name[i]);
            }
            return newText.ToString().ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }
        #endregion
        #endregion



    }
}
