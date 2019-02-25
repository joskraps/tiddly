﻿namespace Tiddly.Sql.DataAccess
{
    using System;
    using System.Data;

    using Models;

    public sealed class SqlDataAccessHelper
    {
        public readonly ExecutionContext ExecutionContext;

        public SqlDataAccessHelper()
        {
            ExecutionContext = new ExecutionContext
            {
                DataRetrievalType = DataActionRetrievalType.DataSet,
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

            ExecutionContext.ParameterMappings.Add(
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
            ExecutionContext.ProcedureSchema = schema;
            ExecutionContext.ProcedureName = string.Join(".", schema, procedureName);
            ExecutionContext.ActionType = DataAccessActionType.Procedure;

            return this;
        }

        public SqlDataAccessHelper AddStatement(string stringStatement)
        {
            ExecutionContext.Statement = stringStatement;
            ExecutionContext.ActionType = DataAccessActionType.Statement;

            return this;
        }

        public ParameterMapping Property(string name)
        {
            return ExecutionContext.ParameterMappings.ContainsKey(name)
                ? ExecutionContext.ParameterMappings[name]
                : null;
        }

        public SqlDataAccessHelper SetPostProcessFunction<T>(
            string targetProperty,
            Func<object, object> mappingFunction)
        {
            targetProperty = targetProperty.ToLower();

            var mappingFunctionWrapper = new CustomMappingFunction
            {
                Action = mappingFunction,
                TargetProperty = targetProperty,
                TargetType = typeof(T)
            };

            if (ExecutionContext.ParameterMappingFunctionCollection.ContainsKey(targetProperty))
            {
                ExecutionContext.ParameterMappingFunctionCollection[targetProperty] = mappingFunctionWrapper;
            }
            else
            {
                ExecutionContext.ParameterMappingFunctionCollection.Add(targetProperty, mappingFunctionWrapper);
            }

            return this;
        }

        public SqlDataAccessHelper SetRetrievalMode(DataActionRetrievalType type)
        {
            ExecutionContext.DataRetrievalType = type;

            return this;
        }

        public SqlDataAccessHelper SetTimeout(int timeoutValue)
        {
            ExecutionContext.Timeout = timeoutValue;

            return this;
        }

        #region CustomMappings

        public SqlDataAccessHelper AddCustomMapping(string alias, string targetProperty)
        {
            if (!ExecutionContext.CustomColumnMappings.ContainsKey(alias))
            {
                ExecutionContext.CustomColumnMappings.Add(alias, string.Empty);
            }

            ExecutionContext.CustomColumnMappings[alias] = targetProperty;

            return this;
        }

        #endregion
    }
}