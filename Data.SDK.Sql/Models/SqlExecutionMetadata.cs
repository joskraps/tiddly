using System.Collections.Concurrent;

namespace Tiddly.Sql.Models
{
    public abstract class SqlExecutionMetadata
    {
        public ConcurrentBag<ExecutionEvent> Events = new ConcurrentBag<ExecutionEvent>();
    }
}