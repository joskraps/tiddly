# tiddly

Tiddly is a lightweight sql ORM that handles convention based property mappings to domain objects.


A data access helper object is used to setup the transaction:

## Data access helper
```csharp
SqlDataAccessHelper AddParameter(string name, object value, SqlDbType dataType, bool scrubValue = false)
```

```csharp
SqlDataAccessHelper AddProcedure(string procedureName, string schema = "dbo")
```

```csharp
SqlDataAccessHelper AddStatement(string stringStatement)
```

```csharp
ParameterMapping Property(string name)
```

```csharp
SqlDataAccessHelper SetPostProcessFunction<T>(string targetProperty,
            Func<string, object> mappingFunction)
```

```csharp
SqlDataAccessHelper SetRetrievalMode(DataActionRetrievalType type)
```

```csharp
SqlDataAccessHelper SetTimeout(int timeoutValue)
```

## Collection fills

### Fill
```csharp
List<T> Fill<T>(SqlDataAccessHelper helper)
```

Fills a list with the supplied object or primitive value.

Example:

```csharp
            var da = new SqlDataAccess(connectionString);
            var helper = new SqlDataAccessHelper();

            helper.AddStatement("select name from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

            var values = da.Fill<string>(helper);
```
### FillToDictionary
```csharp
Dictionary<TKey, TObjType> FillToDictionary<TKey, TObjType>(string keyPropertyName, SqlDataAccessHelper helper, bool overwriteOnDupe = false)
```

Fills a list with the supplied object or primitive value.

Example:

```csharp
    public class DatabaseModel
    {
        public string Name { get; set; }
        public Guid BrokerGuid { get; set; }
        public DateTime CreateDate { get; set; }
        public int Id { get; set; }
        public bool ReadOnly { get; set; }

    }
    
    var da = new SqlDataAccess(connectionString);
    var helper = new SqlDataAccessHelper();

    helper.AddStatement("select name,row_number() over (ORDER BY name ASC)[Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate] from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
    helper.AddParameter("master", "master", SqlDbType.VarChar);
    helper.AddParameter("model", "model", SqlDbType.VarChar);
    helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

    var values = da.FillToDictionary<int,DatabaseModel>("Id",helper);
```

## Single object/value fill
```csharp
T Get<T>(SqlDataAccessHelper helper)
```

```csharp
int Send(SqlDataAccessHelper helper, bool overrideCount = false)
```

## Dataset/DataReader return
```csharp
IDataReader GetDataReader(SqlDataAccessHelper helper)
```

```csharp
DataSet GetDataSet(SqlDataAccessHelper helper)
```

