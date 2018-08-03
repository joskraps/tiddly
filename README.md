# tiddly sql

[<img
src="https://joskraps.visualstudio.com/_apis/public/build/definitions/e59eb71d-cb8a-4975-a09a-982754e10894/3/badge"/>](https://joskraps.visualstudio.com/Tiddly/_build/index?definitionId=2)
[![NuGet Badge](https://buildstats.info/nuget/Tiddly.Sql)](https://www.nuget.org/packages/Tiddly.Sql/)

Tiddly is a lightweight SQL ORM that handles convention based property mapping convert data results into domain objects. 



## Data access helper
```csharp
var helper = new SqlDataAccessHelper();
```

The helper is a settings object that define how the transaction will execute. 

### AddProcedure
```csharp
SqlDataAccessHelper AddProcedure(string procedureName, string schema = "dbo")
```

Adds the stored procedure to execute and optionally the schema to be used.

### AddStatement
```csharp
SqlDataAccessHelper AddStatement(string stringStatement)
```

Adds a sql statement to be executed. This query can be parameterized.

### AddParameter

```csharp
SqlDataAccessHelper AddParameter(string name, object value, SqlDbType dataType, bool scrubValue = false)
```

Adds a parameter that will be used in the supplied stored procedure or parameterized query. If scrub value is set to true, the string value will be used in a reg ex replace using a generic pattern. see here:

### Property
```csharp
ParameterMapping Property(string name)
```

Retrieves the propery mapping for the provided string property name.

### SetPostProcessFunction
```csharp
SqlDataAccessHelper SetPostProcessFunction<T>(string targetProperty,
            Func<string, object> mappingFunction)
```

This allows a function to be defined that will execute after the value has been set. The value is supplied as a string and will be cast to the target property type from the function return.


```csharp
            var da = new SqlDataAccess(ConnectionString);
            var helper = new SqlDataAccessHelper();
            helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
            helper.AddStatement(
                "select top 1 name,database_id [Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate] from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
            helper.AddParameter("master", "master", SqlDbType.VarChar);
            helper.AddParameter("model", "model", SqlDbType.VarChar);
            helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);
            helper.SetPostProcessFunction<string>("name", s => s == "master" ? "MAPPING FUNCTION" : s);

            var returnValue = da.Get<DatabaseModel>(helper);
            this.OutputTestTimings(helper.ExecutionContext);
            Assert.AreEqual(returnValue.Name, "MAPPING FUNCTION");
            Assert.IsTrue(returnValue.Id != 0);
            Assert.AreEqual(returnValue.BrokerGuid, Guid.Empty);
```

//TODO 
Should have two flavors of this - one that fires when the value is set, and one that is fired after the row has been read (providing context to the row/object)
### SetRetrievalMode
```csharp
SqlDataAccessHelper SetRetrievalMode(DataActionRetrievalType type)
```

Allows you to switch between using a data reader or data set.

### SetTimeout
```csharp
SqlDataAccessHelper SetTimeout(int timeoutValue)
```

Sets the connection timeout

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

Returns an instance of type T that you specify: can be an object or primitive type. Does not support nested objects


## Dataset/DataReader return

### GetDataReader
```csharp
IDataReader GetDataReader(SqlDataAccessHelper helper)
```

### GetDataSet
```csharp
DataSet GetDataSet(SqlDataAccessHelper helper)
```

