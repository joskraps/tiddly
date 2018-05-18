using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tiddly.Sql.DataAccess;

namespace Tiddly.Sql.Tests
{
    [TestClass]
    public class InsertBuilderTests
    {
        [TestMethod]
        public void GenerateBaseInsert()
        {
            var dah = new SqlDataAccessHelper();

            dah.InsertBuilder.Table = "boom";
            dah.InsertBuilder.SetField("shocka", "locka");

            var generatedQuery = dah.InsertBuilder.BuildQuery();

            Assert.AreEqual(generatedQuery, @"INSERT INTO boom(shocka) VALUES ('locka')");
        }

        [TestMethod]
        public void GenerateInsertWithIdentitySelect()
        {
            var dah = new SqlDataAccessHelper();
            dah.InsertBuilder.SelectIdentity = true;
            dah.InsertBuilder.Table = "boom";
            dah.InsertBuilder.SetField("shocka", "locka");

            var generatedQuery = dah.InsertBuilder.BuildQuery();

            Assert.AreEqual(generatedQuery, @"INSERT INTO boom(shocka) VALUES ('locka'); SELECT SCOPE_IDENTITY()");
        }
    }
}
