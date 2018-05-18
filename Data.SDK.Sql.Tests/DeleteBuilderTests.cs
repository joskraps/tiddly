using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tiddly.Sql.DataAccess;
using Tiddly.Sql.QueryComponents.Clauses;
using Tiddly.Sql.QueryComponents.Enums;

namespace Tiddly.Sql.Tests
{
    [TestClass]
    public class DeleteBuilderTests
    {
        [TestMethod]
        public void GenerateDeleteWithNoFlag()
        {
            var dah = new SqlDataAccessHelper();

            dah.DeleteBuilder.Table = "boom";

            try
            {
                dah.DeleteBuilder.BuildQuery();
                Assert.Fail(
                    "Because no conditions were set, this statement would delete the entire table. Set EnableClear to allow this.");
            }
            catch (Exception)
            {
            }
        }

        [TestMethod]
        public void GenerateDeleteWithFlag()
        {
            var dah = new SqlDataAccessHelper();

            dah.DeleteBuilder.Table = "boom";
            dah.DeleteBuilder.EnableClear = true;

            var generatedQuery = dah.DeleteBuilder.BuildQuery();
            Assert.AreEqual(generatedQuery, "DELETE FROM [boom]");
        }

        [TestMethod]
        public void GenerateDeleteWithCondition()
        {
            var dah = new SqlDataAccessHelper();

            dah.DeleteBuilder.Table = "boom";
            dah.DeleteBuilder.AddWhere(new WhereClause("shocka", Comparison.Equals, "locka"));

            var generatedQuery = dah.DeleteBuilder.BuildQuery();

            Assert.AreEqual(generatedQuery, @"DELETE FROM [boom] WHERE  (shocka = 'locka')  ");
        }
    }
}
