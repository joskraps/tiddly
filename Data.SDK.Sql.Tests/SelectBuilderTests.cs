using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tiddly.Sql.DataAccess;
using Tiddly.Sql.QueryComponents.Clauses;
using Tiddly.Sql.QueryComponents.Enums;

namespace Tiddly.Sql.Tests
{
    [TestClass]
    public class SelectBuilderTests
    {
        [TestMethod]
        public void GenerateSelect()
        {
            var dah = new SqlDataAccessHelper();

            dah.SelectBuilder.SelectFromTable("BOOM");
            dah.SelectBuilder.SelectColumns("Shocka");

            var generatedQuery = dah.SelectBuilder.BuildQuery();

            Assert.AreEqual(generatedQuery, "SELECT Shocka  FROM BOOM ");
        }

        [TestMethod]
        public void GenerateSelectLargeColumnList()
        {
            var dah = new SqlDataAccessHelper();

            dah.SelectBuilder.SelectFromTable("BOOM");
            dah.SelectBuilder.SelectColumns("Shocka", "Locka");

            var generatedQuery = dah.SelectBuilder.BuildQuery();

            Assert.AreEqual(generatedQuery, "SELECT Shocka,Locka  FROM BOOM ");
        }


        [TestMethod]
        public void GenerateSelectWithJoin()
        {
            var dah = new SqlDataAccessHelper();

            dah.SelectBuilder.SelectFromTable("BOOM b");
            dah.SelectBuilder.SelectColumns("Shocka", "Locka");

            dah.SelectBuilder.AddJoin(new JoinClause(JoinType.InnerJoin, "sauce", "index", Comparison.Equals, "b",
                "Shocka"));

            var generatedQuery = dah.SelectBuilder.BuildQuery();

            Assert.AreEqual(generatedQuery,
                "SELECT Shocka,Locka  FROM BOOM b INNER JOIN sauce ON b.Shocka = sauce.index ");
        }

        [TestMethod]
        public void GenerateSelectConditions()
        {
            var dah = new SqlDataAccessHelper();

            dah.SelectBuilder.SelectFromTable("BOOM b");
            dah.SelectBuilder.SelectColumns("Shocka", "Locka");

            dah.SelectBuilder.AddWhere("Locka", Comparison.Equals, "Joel");

            var generatedQuery = dah.SelectBuilder.BuildQuery();

            Assert.AreEqual(generatedQuery, "SELECT Shocka,Locka  FROM BOOM b  WHERE  (Locka = \'Joel\')  ");
        }
    }
}
