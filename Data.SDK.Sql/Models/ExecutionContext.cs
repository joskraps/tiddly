using System.Collections.Generic;

namespace Tiddly.Sql.Models
{
    public class ExecutionContext
    {
        public Dictionary<string, CustomMappingFunction> ParameterMappingFunctionCollection;
        public Dictionary<string, ParameterMapping> ParameterMappings;
        public string ProcedureSchema = "dbo";
        public SqlQueryBuilder QueryBuilder;
        public string Statement = string.Empty;
        public string TableSchema = "dbo";
        public int Timeout = 60;


        public ExecutionContext()
        {
            ParameterMappingFunctionCollection = new Dictionary<string, CustomMappingFunction>();
            CustomColumnMappings = new Dictionary<string, string>();
            ParameterMappings = new Dictionary<string, ParameterMapping>();
            QueryBuilder = new SqlQueryBuilder();
            ExecutionEvent = new ExecutionEvent();
        }

        public DataAccessActionType ActionType { get; set; }
        public Dictionary<string, string> CustomColumnMappings { get; set; }
        public DataActionRetrievalType DataRetrievalType { get; set; }
        public string ProcedureName { get; set; }
        public ExecutionEvent ExecutionEvent { get; set; }
    }
}