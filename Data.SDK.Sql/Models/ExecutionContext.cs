namespace Tiddly.Sql.Models
{
    using System;
    using System.Collections.Generic;

    public class ExecutionContext
    {
        public ExecutionContext()
        {
            this.TableSchema = "dbo";
            this.ParameterMappingFunctionCollection = new Dictionary<string, CustomMappingFunction>(StringComparer.OrdinalIgnoreCase);
            this.CustomColumnMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.ParameterMappings = new Dictionary<string, ParameterMapping>(StringComparer.OrdinalIgnoreCase);
            this.ExecutionEvent = new ExecutionEvent();
        }

        public Dictionary<string, CustomMappingFunction> ParameterMappingFunctionCollection { get; }

        public Dictionary<string, ParameterMapping> ParameterMappings { get; }

        public string TableSchema { get; set; }

        public DataAccessActionType ActionType { get; set; }

        public Dictionary<string, string> CustomColumnMappings { get; set; }

        public DataActionRetrievalType DataRetrievalType { get; set; }

        public ExecutionEvent ExecutionEvent { get; set; }

        public string ProcedureName { get; set; }

        public string ProcedureSchema { get; set; }

        public string Statement { get; set; }

        public int Timeout { get; set; }
    }
}