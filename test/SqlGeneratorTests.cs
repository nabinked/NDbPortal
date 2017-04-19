using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NDbPortal.Names;

namespace NDbPortal.Test
{
    [TestClass]
    public class SqlGeneratorTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var namingConvention = new NamingConvention()
            {
                DbNamingConvention = DbEnums.NamingConventions.UnderScoreCase,
                PocoNamingConvention = DbEnums.NamingConventions.PascalCase
            };
            var storedProcName = "core.test";
            var sqlGenerator = new SqlGenerator(new TableInfo(storedProcName), namingConvention);
            var s = sqlGenerator.GetStoredProcQuery(new { param1 = 1, param2 = "two" });
            Assert.IsTrue(s.Contains("param1 => 1"));
            Assert.IsTrue(s.Contains("param2 => \"two\""));
        }
    }
}
