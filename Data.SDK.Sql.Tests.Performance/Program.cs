using BenchmarkDotNet.Running;
using Tiddly.Sql.Tests.Performance.DataAccess;

namespace Tiddly.Sql.Tests.Performance
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Get_Performance_DataSDK>();
            BenchmarkRunner.Run<Get_Performance_Dapper>();
        }
    }
}
