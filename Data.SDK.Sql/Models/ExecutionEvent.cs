using System;
using System.Collections.Generic;

namespace Tiddly.Sql.Models
{
    public class ExecutionEvent
    {
        public ExecutionEvent()
        {
            ExecutionErrors = new List<Exception>();
        }

        public long DataExecutionTiming { get; set; }
        public long DataMappingTiming { get; set; }
        public List<Exception> ExecutionErrors { get; internal set; }
        public long GeneratePropertiesTiming { get; set; }
        public long GeneratePropertyIndicesTiming { get; set; }
    }
}