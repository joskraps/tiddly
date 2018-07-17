namespace Tiddly.Sql.DataAccess
{
    using System;
    using System.Data;

    using Tiddly.Sql.Models;

    public sealed class SqlDataAccessHelper
    {
        public readonly ExecutionContext ExecutionContext;

        public SqlDataAccessHelper()
        {
            this.ExecutionContext = new ExecutionContext
            {
                DataRetrievalType = DataActionRetrievalType.DataReader,
                ExecutionEvent = new ExecutionEvent()
            };
        }

        public SqlDataAccessHelper AddParameter(string name, object value, SqlDbType dataType, bool scrubValue = false)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"{nameof(name)} cannot be null or empty.");
            }

            if (scrubValue && (dataType == SqlDbType.VarChar
                               || dataType == SqlDbType.Char
                               || dataType == SqlDbType.Text
                               || dataType == SqlDbType.NChar
                               || dataType == SqlDbType.NVarChar
                               || dataType == SqlDbType.NText))
            {
                value = SqlScrubber.ScrubString(value.ToString());
            }

            if (dataType == SqlDbType.Image)
            {
                dataType = SqlDbType.Binary;
            }

            if (dataType == SqlDbType.Text)
            {
                dataType = SqlDbType.VarChar;
            }

            this.ExecutionContext.ParameterMappings.Add(
                name.ToLower(),
                new ParameterMapping
                    {
                        DataType = dataType,
                Name = name.ToLower(),
                ScrubValue = scrubValue,
                Value = value
            });

            return this;
        }

        public SqlDataAccessHelper AddProcedure(string procedureName, string schema = "dbo")
        {
            this.ExecutionContext.ProcedureSchema = schema;
            this.ExecutionContext.ProcedureName = string.Join(".", schema, procedureName);
            this.ExecutionContext.ActionType = DataAccessActionType.Procedure;

            return this;
        }

        public SqlDataAccessHelper AddStatement(string stringStatement)
        {
            this.ExecutionContext.Statement = stringStatement;
            this.ExecutionContext.ActionType = DataAccessActionType.Statement;

            return this;
        }

        public ParameterMapping Property(string name)
        {
            return this.ExecutionContext.ParameterMappings.ContainsKey(name)
                ? this.ExecutionContext.ParameterMappings[name]
                : null;
        }

        public SqlDataAccessHelper SetPostProcessFunction<T>(
            string targetProperty,
            Func<string, object> mappingFunction)
        {
            targetProperty = targetProperty.ToLower();

            var mappingFunctionWrapper = new CustomMappingFunction
            {
                Action = mappingFunction,
                TargetProperty = targetProperty,
                TargetType = typeof(T)
            };

            if (this.ExecutionContext.ParameterMappingFunctionCollection.ContainsKey(targetProperty))
            {
                this.ExecutionContext.ParameterMappingFunctionCollection[targetProperty] = mappingFunctionWrapper;
            }
            else
            {
                this.ExecutionContext.ParameterMappingFunctionCollection.Add(targetProperty, mappingFunctionWrapper);
            }

            return this;
        }

        public SqlDataAccessHelper SetRetrievalMode(DataActionRetrievalType type)
        {
            this.ExecutionContext.DataRetrievalType = type;

            return this;
        }

        public SqlDataAccessHelper SetTimeout(int timeoutValue)
        {
            this.ExecutionContext.Timeout = timeoutValue;

            return this;
        }

        #region CustomMappings

        public SqlDataAccessHelper AddCustomMapping(string alias, string targetProperty)
        {
            if (!this.ExecutionContext.CustomColumnMappings.ContainsKey(alias))
            {
                this.ExecutionContext.CustomColumnMappings.Add(alias, string.Empty);
            }

            this.ExecutionContext.CustomColumnMappings[alias] = targetProperty;

            return this;
        }

        #endregion
    }
}