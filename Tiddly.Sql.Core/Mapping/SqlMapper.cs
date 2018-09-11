namespace Tiddly.Sql.Mapping
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;

    using Models;

    public static class SqlMapper
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, ObjectPropertyMapping>>
            ObjectMappings = new ConcurrentDictionary<Type, ConcurrentDictionary<string, ObjectPropertyMapping>>();

        public static Dictionary<TKeyType, TObjType> KeyedMap<TKeyType, TObjType>(
            string propertyToKey,
            IList<TObjType> initialReturn,
            ExecutionContext executionContext,
            bool overrideKeyOnDupe)
        {
            var timer = new Stopwatch();
            timer.Start();
            try
            {
                var propertyToKeyToLower = propertyToKey.ToLower();
                var returnO = new Dictionary<TKeyType, TObjType>();
                var baseType = typeof(TObjType);

                GenerateProperties(baseType, executionContext);

                var typeCheck = ObjectMappings[typeof(TObjType)];

                if (!typeCheck.ContainsKey(propertyToKeyToLower))
                {
                    throw new ArgumentException("Property to be used to key is not on the supplied object");
                }

                if (typeCheck[propertyToKeyToLower].ObjectPropertyInfo.PropertyType != typeof(TKeyType))
                {
                    throw new ArgumentException("Property type to key does not match");
                }

                var keyedProperty = typeCheck[propertyToKeyToLower].ObjectPropertyInfo;

                foreach (var item in initialReturn)
                {
                    var keyValue = (TKeyType)keyedProperty.GetValue(item, null);

                    if (!returnO.ContainsKey(keyValue))
                    {
                        returnO.Add(keyValue, item);
                    }
                    else if (overrideKeyOnDupe)
                    {
                        returnO[keyValue] = item;
                    }
                    else
                    {
                        throw new ArgumentException("Duplicate key found and no override supplied.");
                    }
                }

                return returnO;
            }
            finally
            {
                timer.Stop();
                executionContext.ExecutionEvent.DataMappingTiming = timer.ElapsedTicks;
            }
        }

        public static List<T> Map<T>(DataTable dataTable, ExecutionContext executionContext)
        {
            var timer = new Stopwatch();
            timer.Start();
            try
            {
                var returnO = new List<T>();

                if (dataTable.Rows.Count <= 0)
                {
                    return returnO;
                }

                var baseType = typeof(T);

                if (IsPrimitiveValue<T>())
                {
                    returnO = GeneratePrimitives<T>(dataTable, executionContext);
                }
                else if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    returnO = GenerateNullablePrimitives<T>(dataTable, executionContext);
                }
                else
                {
                    GenerateProperties(baseType, executionContext);

                    var columns = Enumerable.Range(0, dataTable.Columns.Count)
                        .Select(i => dataTable.Columns[i].ColumnName).ToList();
                    var indexer = GetPropertyIndices(
                        columns,
                        ObjectMappings[baseType],
                        executionContext,
                        executionContext.TableSchema,
                        executionContext.CustomColumnMappings);

                    returnO = GenerateObjects<T>(indexer, dataTable, executionContext);
                }

                return returnO;
            }
            finally
            {
                timer.Stop();
                executionContext.ExecutionEvent.DataMappingTiming = timer.ElapsedTicks;
            }
        }

        public static List<T> Map<T>(IDataReader reader, ExecutionContext executionContext)
        {
            var timer = new Stopwatch();
            timer.Start();
            try
            {
                List<T> returnO;

                var baseType = typeof(T);

                if (IsPrimitiveValue<T>())
                {
                    returnO = GeneratePrimitives<T>(reader, executionContext);
                }
                else if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    returnO = GenerateNullablePrimitives<T>(reader, executionContext);
                }
                else
                {
                    GenerateProperties(baseType, executionContext);

                    var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();

                    var indexer = GetPropertyIndices(
                        columns,
                        ObjectMappings[baseType],
                        executionContext,
                        executionContext.TableSchema,
                        executionContext.CustomColumnMappings);

                    returnO = GenerateObjects<T>(indexer, reader, executionContext);
                }

                return returnO;
            }
            finally
            {
                timer.Stop();
                executionContext.ExecutionEvent.DataMappingTiming = timer.ElapsedTicks;
            }
        }

        public static T MapSingle<T>(DataTable dataTable, ExecutionContext helperExecutionContext)
        {
            var returnO = Map<T>(dataTable, helperExecutionContext);

            return returnO.Count > 0 ? returnO[0] : default(T);
        }

        public static void GenerateProperties(Type objectType, ExecutionContext context)
        {
            var timer = new Stopwatch();
            timer.Start();
            try
            {
                if (ObjectMappings.ContainsKey(objectType))
                {
                    return;
                }

                ObjectMappings.TryAdd(objectType, new ConcurrentDictionary<string, ObjectPropertyMapping>());

                // Need to look at attributes on properties for type conversions
                foreach (var pi in objectType.GetProperties())
                {
                    if (pi.PropertyType.IsEnum)
                    {
                        var wimp = new ObjectPropertyMapping
                        {
                            Name = pi.Name,
                            IsEnum = true,
                            ObjectType = pi.PropertyType,
                            ObjectPropertyInfo = pi,
                            IsNullable = true
                        };
                        ObjectMappings[objectType].TryAdd(pi.Name.ToLower(), wimp);
                    }
                    else if (pi.PropertyType.IsGenericType &&
                             pi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var underlyingType = Nullable.GetUnderlyingType(pi.PropertyType);

                        var objectPropertyMapping = new ObjectPropertyMapping
                        {
                            // ReSharper disable once PossibleNullReferenceException
                            Name = underlyingType.Name,
                            ObjectType = underlyingType,
                            ObjectPropertyInfo = pi,
                            IsNullable = true
                        };

                        ObjectMappings[objectType].TryAdd(pi.Name.ToLower(), objectPropertyMapping);
                    }
                    else if (pi.PropertyType.IsValueType
                             || pi.PropertyType.Name == "String"
                             || pi.PropertyType.Name == "Byte[]")
                    {
                        var objectPropertyMapping = new ObjectPropertyMapping
                        {
                            Name = pi.Name,
                            ObjectType = pi.PropertyType,
                            ObjectPropertyInfo = pi
                        };
                        ObjectMappings[objectType].TryAdd(pi.Name.ToLower(), objectPropertyMapping);
                    }
                }
            }
            finally
            {
                timer.Stop();
                context.ExecutionEvent.GeneratePropertiesTiming = timer.ElapsedTicks;
            }
        }

        private static void SetProperty<T>(
            ref T newO,
            ObjectPropertyMapping indexerVal,
            string tempValue,
            CustomMappingFunction customMappingFunction)
        {
            if (customMappingFunction != null)
            {
                indexerVal.ObjectPropertyInfo.SetValue(newO, customMappingFunction.Action.Invoke(tempValue));
            }
            else
            {
                if (indexerVal.ObjectType == typeof(Guid))
                {
                    indexerVal.ObjectPropertyInfo.SetValue(newO, new Guid(tempValue), null);
                }
                else if (indexerVal.ObjectPropertyInfo.PropertyType.IsEnum)
                {
                    indexerVal.ObjectPropertyInfo.SetValue(
                        newO,
                        Enum.Parse(indexerVal.ObjectPropertyInfo.PropertyType, tempValue),
                        null);
                }
                else
                {
                    indexerVal.ObjectPropertyInfo.SetValue(
                        newO,
                        Convert.ChangeType(tempValue, indexerVal.ObjectType),
                        null);
                }
            }
        }

        private static T GenerateObject<T>(
            DataRecordAdapter row,
            Dictionary<int, ObjectPropertyMapping> indexer,
            ExecutionContext context)
        {
            var newO = Activator.CreateInstance<T>();

            foreach (var key in indexer.Keys)
            {
                if (row[key] == null || row[key] == DBNull.Value)
                {
                    continue;
                }

                var indexerVal = indexer[key];
                var columnName = indexerVal.Name.ToLower();
                var tempValue = row[key];

                var customMapping = context.ParameterMappingFunctionCollection.ContainsKey(columnName)
                                        ? context.ParameterMappingFunctionCollection[columnName]
                                        : null;
                SetProperty(
                    ref newO,
                    indexerVal,
                    tempValue.ToString(),
                    customMapping);
            }

            return newO;
        }

        private static List<T> GenerateObjects<T>(
            Dictionary<int, ObjectPropertyMapping> indexer,
            DataTable data,
            ExecutionContext context)
        {
            var timer = new Stopwatch();
            timer.Start();
            var returnList = new List<T>();
            try
            {
                foreach (DataRow dataRow in data.Rows)
                {
                    var adapter = new DataRecordAdapter(dataRow);

                    returnList.Add(GenerateObject<T>(adapter, indexer, context));
                }
            }
            finally
            {
                timer.Stop();
                context.ExecutionEvent.DataMappingTiming = timer.ElapsedTicks;
            }

            return returnList;
        }

        private static List<T> GenerateObjects<T>(
            Dictionary<int, ObjectPropertyMapping> indexer,
            IDataReader data,
            ExecutionContext context)
        {
            var timer = new Stopwatch();
            timer.Start();
            try
            {
                var returnList = new List<T>();

                using (data)
                {
                    while (data.Read())
                    {
                        var adapter = new DataRecordAdapter(data);

                        returnList.Add(GenerateObject<T>(adapter, indexer, context));
                    }
                }

                return returnList;
            }
            finally
            {
                timer.Stop();
                context.ExecutionEvent.DataMappingTiming = timer.ElapsedTicks;
            }
        }

        private static List<T> GeneratePrimitives<T>(DataTable dataTable, ExecutionContext context)
        {
            var timer = new Stopwatch();
            timer.Start();
            try
            {
                var returnList = new List<T>();

                foreach (DataRow dr in dataTable.Rows)
                {
                    returnList.Add((T)Convert.ChangeType(dr[0], typeof(T)));
                }

                return returnList;
            }
            finally
            {
                timer.Stop();
                context.ExecutionEvent.DataMappingTiming = timer.ElapsedTicks;
            }
        }

        private static List<T> GeneratePrimitives<T>(IDataReader reader, ExecutionContext context)
        {
            var timer = new Stopwatch();
            timer.Start();
            try
            {
                var returnList = new List<T>();

                using (reader)
                {
                    while (reader.Read())
                    {
                        returnList.Add((T)Convert.ChangeType(reader[0], typeof(T)));
                    }
                }

                return returnList;
            }
            finally
            {
                timer.Stop();
                context.ExecutionEvent.DataMappingTiming = timer.ElapsedTicks;
            }
        }

        private static List<T> GenerateNullablePrimitives<T>(DataTable dataTable, ExecutionContext context)
        {
            var timer = new Stopwatch();
            timer.Start();
            try
            {
                var returnList = new List<T>();
                var underlyingType = Nullable.GetUnderlyingType(typeof(T));
                foreach (DataRow dr in dataTable.Rows)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    var safeValue = dr.IsNull(0) ? null : Convert.ChangeType(dr[0], underlyingType);

                    returnList.Add((T)safeValue);
                }

                return returnList;
            }
            finally
            {
                timer.Stop();
                context.ExecutionEvent.DataMappingTiming = timer.ElapsedTicks;
            }
        }

        private static List<T> GenerateNullablePrimitives<T>(IDataReader reader, ExecutionContext context)
        {
            var timer = new Stopwatch();
            timer.Start();
            try
            {
                var returnList = new List<T>();
                var underlyingType = Nullable.GetUnderlyingType(typeof(T));

                using (reader)
                {
                    while (reader.Read())
                    {
                        // ReSharper disable once AssignNullToNotNullAttribute
                        var safeValue = reader.IsDBNull(0) ? null : Convert.ChangeType(reader[0], underlyingType);

                        returnList.Add((T)safeValue);
                    }
                }

                return returnList;
            }
            finally
            {
                timer.Stop();
                context.ExecutionEvent.DataMappingTiming = timer.ElapsedTicks;
            }
        }

        private static Dictionary<int, ObjectPropertyMapping> GetPropertyIndices(
            IReadOnlyList<string> columns,
            IDictionary<string, ObjectPropertyMapping> propertyList,
            ExecutionContext context,
            string tablePrefix = null,
            IDictionary<string, string> customMappings = null)
        {
            var timer = new Stopwatch();
            timer.Start();
            try
            {
                var returnDictionary = new Dictionary<int, ObjectPropertyMapping>();

                for (var i = 0; i < columns.Count; i++)
                {
                    var colName = columns[i].ToLower();
                    var propertyListCheck = propertyList.ContainsKey(colName);

                    if (propertyListCheck)
                    {
                        returnDictionary.Add(i, propertyList[colName]);
                        continue;
                    }

                    if (!string.IsNullOrEmpty(tablePrefix))
                    {
                        var prefixColName = columns[i].ToLower().Replace(tablePrefix, string.Empty);
                        var customPropertyListCheck = propertyList.ContainsKey(prefixColName);
                        if (customPropertyListCheck)
                        {
                            returnDictionary.Add(i, propertyList[prefixColName]);
                            continue;
                        }
                    }

                    // ReSharper disable once PossibleNullReferenceException
                    var customListCheck = customMappings.ContainsKey(columns[i].ToLower());

                    if (!customListCheck)
                    {
                        continue;
                    }

                    var customMap = customMappings[columns[i].ToLower()];

                    returnDictionary.Add(i, propertyList[customMap.ToLower()]);
                }

                return returnDictionary;
            }
            finally
            {
                timer.Stop();
                context.ExecutionEvent.GeneratePropertyIndicesTiming = timer.ElapsedTicks;
            }
        }

        private static bool IsPrimitiveValue<T>()
        {
            var baseType = typeof(T);

            return baseType.IsPrimitive

                   // This treats these types as primitives even though they are technically objects.
                   // Something else we can do is check the namespace - the types we care about all all in System.x
                   || baseType == typeof(decimal)
                   || baseType == typeof(string)
                   || baseType == typeof(DateTime)
                   || baseType == typeof(Guid);
        }
    }
}