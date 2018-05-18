using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tiddly.Sql.DataAccess;
using Tiddly.Sql.QueryComponents.Clauses;
using Tiddly.Sql.QueryComponents.Enums;

namespace Tiddly.Sql.Tests
{
    [TestClass]
    public class UpdateBuilderTests
    {
        [TestMethod]
        public void GenerateBaseUpdate()
        {
            var dah = new SqlDataAccessHelper();

            dah.UpdateBuilder.Table = "boom";
            dah.UpdateBuilder.SetField("shocka", "locka");

            var generatedQuery = dah.UpdateBuilder.BuildQuery();

            Assert.AreEqual(generatedQuery, @"UPDATE boom SET shocka='locka' ");
        }


        [TestMethod]
        public void GenerateUpdateWithCondition()
        {
            var dah = new SqlDataAccessHelper();

            dah.UpdateBuilder.Table = "boom";
            dah.UpdateBuilder.SetField("shocka", "locka");
            dah.UpdateBuilder.AddWhere(new WhereClause("test", Comparison.Equals, 10));
            var generatedQuery = dah.UpdateBuilder.BuildQuery();

            Assert.AreEqual(generatedQuery, @"UPDATE boom SET shocka='locka'  WHERE  (test = 10)  ");
        }

        [TestMethod]
        public void GenerateUpdateWithConditions()
        {
            var dah = new SqlDataAccessHelper();

            dah.UpdateBuilder.Table = "boom";
            dah.UpdateBuilder.SetField("shocka", "locka");
            dah.UpdateBuilder.AddWhere(new WhereClause("test1", Comparison.Equals, 10));
            dah.UpdateBuilder.AddWhere(new WhereClause("test2", Comparison.GreaterOrEquals, 99));
            var generatedQuery = dah.UpdateBuilder.BuildQuery();

            Assert.AreEqual(generatedQuery, @"UPDATE boom SET shocka='locka'  WHERE  ((test1 = 10) AND (test2 >= 99))  ");
        }

        [TestMethod]
        public void GenerateUpdateWithNestedConditions()
        {
            var dah = new SqlDataAccessHelper();

            dah.UpdateBuilder.Table = "boom";
            dah.UpdateBuilder.SetField("shocka", "locka");
            // Can think of the WhereClause as a single grouping of comparisons
            var baseWhere = new WhereClause("test1", Comparison.Equals, 10);
            baseWhere.AddClause(LogicOperator.Or, Comparison.Equals, 11);

            dah.UpdateBuilder.AddWhere(baseWhere, 1);
            dah.UpdateBuilder.AddWhere(new WhereClause("test2", Comparison.GreaterOrEquals, 99), 1);
            var generatedQuery = dah.UpdateBuilder.BuildQuery();

            Assert.AreEqual(generatedQuery,
                @"UPDATE boom SET shocka='locka'  WHERE  ((test1 = 10 OR test1 = 11) AND (test2 >= 99))  ");
        }
    }
}
