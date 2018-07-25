# tiddly

[<img
src="https://joskraps.visualstudio.com/_apis/public/build/definitions/e59eb71d-cb8a-4975-a09a-982754e10894/3/badge"/>](https://joskraps.visualstudio.com/Tiddly/_build/index?definitionId=2)
[![NuGet Badge](https://buildstats.info/nuget/tiddly.sql)](https://www.nuget.org/packages/tiddly.sql/)

Tiddly is a lightweight sql ORM that handles convention based property mappings to domain objects.


A data access helper object is used to setup the transaction:

## Data access helper

### AddParameter

```csharp
SqlDataAccessHelper AddParameter(string name, object value, SqlDbType dataType, bool scrubValue = false)
```

### AddProcedure
```csharp
SqlDataAccessHelper AddProcedure(string procedureName, string schema = "dbo")
```

### AddStatement
```csharp
SqlDataAccessHelper AddStatement(string stringStatement)
```

### Property
```csharp
ParameterMapping Property(string name)
```

### SetPostProcessFunction
```csharp
SqlDataAccessHelper SetPostProcessFunction<T>(string targetProperty,
            Func<string, object> mappingFunction)
```

### SetRetrievalMode
```csharp
SqlDataAccessHelper SetRetrievalMode(DataActionRetrievalType type)
```

### SetTimeout
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

### Get
```csharp
T Get<T>(SqlDataAccessHelper helper)
```

### Send
```csharp
int Send(SqlDataAccessHelper helper, bool overrideCount = false)
```

## Dataset/DataReader return

### GetDataReader
```csharp
IDataReader GetDataReader(SqlDataAccessHelper helper)
```

### GetDataSet
```csharp
DataSet GetDataSet(SqlDataAccessHelper helper)
```

