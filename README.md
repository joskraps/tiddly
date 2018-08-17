# tiddly sql

[<img
src="https://joskraps.visualstudio.com/_apis/public/build/definitions/e59eb71d-cb8a-4975-a09a-982754e10894/3/badge"/>](https://joskraps.visualstudio.com/Tiddly/_build/index?definitionId=2)
[![NuGet Badge](https://buildstats.info/nuget/Tiddly.Sql)](https://www.nuget.org/packages/Tiddly.Sql/)

Tiddly is a lightweight SQL ORM that handles convention based property mapping convert data results into domain objects or simple primitive types. Tiddly can be used without the mapping functionality and simply used to eliminate boilerplate command object code for data retrieval. 

Performance comparisons...

## How to use

### Data access helper
```csharp
var helper = new SqlDataAccessHelper();
```

The helper is a settings object that define how the transaction will execute. 

#### AddProcedure
```csharp
SqlDataAccessHelper AddProcedure(string procedureName, string schema = "dbo")
```

Adds the stored procedure to execute and optionally the schema to be used.

#### AddStatement
```csharp
SqlDataAccessHelper AddStatement(string stringStatement)
```

Adds a sql statement to be executed. This query can be parameterized.

#### AddParameter

```csharp
SqlDataAccessHelper AddParameter(string name, object value, SqlDbType dataType, bool scrubValue = false)
```

Adds a parameter that will be used in the supplied stored procedure or parameterized query. If scrub value is set to true, the string value will be used in a reg ex replace using a generic pattern. see here:

#### Property
```csharp
ParameterMapping Property(string name)
```

Retrieves the propery mapping for the provided string property name.

#### AddCustomMapping
```csharp
SqlDataAccessHelper AddCustomMapping(string alias, string targetProperty)
```

Supplies an alias for the target property. This will allow you to map properties to columns when the names do not match.

#### SetPostProcessFunction
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

#### SetRetrievalMode
```csharp
SqlDataAccessHelper SetRetrievalMode(DataActionRetrievalType type)
```

Allows you to switch between using a data reader or data set.

#### SetTimeout
```csharp
SqlDataAccessHelper SetTimeout(int timeoutValue)
```

Sets the connection timeout

### Collection fills

#### Fill
Fills a list with the supplied object or primitive value. Unless property mappings are supplied, this will attempt to match properties to column names. 

```csharp
IList<T> Fill<T>(SqlDataAccessHelper helper)
```
```csharp
var da = new SqlDataAccess(connectionString);
var helper = new SqlDataAccessHelper();

helper.AddStatement("select name from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
helper.AddParameter("master", "master", SqlDbType.VarChar);
helper.AddParameter("model", "model", SqlDbType.VarChar);
helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

var values = da.Fill<string>(helper);
```

```csharp
private class DbHelpModel
{
    public string Name { get; set; }
    public string Owner { get; set; }
    public string ObjectType { get; set; }
}

var da = new SqlDataAccess(ConnectionString);
var helper = new SqlDataAccessHelper();

helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
helper.AddProcedure("sp_help");

var values = da.Fill<DbHelpModel>(helper);
```

#### FillToDictionary
Same as filling to a list but allows you to specify a property that will key a dictionary. An exception will be thrown if the property does not exist, the property type does not match, or duplicate values are found for the key. Duplicate key behaviour can be overriden so that the key value will overwrite instead of throwing an exception.

```csharp
Dictionary<TKey, TObjType> FillToDictionary<TKey, TObjType>(string keyPropertyName, SqlDataAccessHelper helper, bool overwriteOnDupe = false)
```
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

### Single object/value fill
Returns an instance of type T that you specify: can be an object or primitive type. Does not support nested objects

#### Get - primitive
```csharp
T Get<T>(SqlDataAccessHelper helper)

var da = new SqlDataAccess(ConnectionString);
var helper = new SqlDataAccessHelper();

helper.SetRetrievalMode(DataActionRetrievalType.DataSet);
helper.AddStatement(
    "select top 1 name from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
helper.AddParameter("master", "master", SqlDbType.VarChar);
helper.AddParameter("model", "model", SqlDbType.VarChar);
helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

var returnValue = da.Get<string>(helper);
```

#### Get - object
```csharp
var da = new SqlDataAccess(ConnectionString);
var helper = new SqlDataAccessHelper();

helper.SetRetrievalMode(DataActionRetrievalType.DataReader);
helper.AddStatement(
    "select top 1 name,database_id [Id],is_read_only [ReadOnly],service_broker_guid [BrokerGuid],create_date [CreateDate] from sys.databases where name in (@master,@model,@msdb) order by 1 asc");
helper.AddParameter("master", "master", SqlDbType.VarChar);
helper.AddParameter("model", "model", SqlDbType.VarChar);
helper.AddParameter("msdb", "msdb", SqlDbType.VarChar);

var returnValue = da.Get<DatabaseModel>(helper);
```

### GetDataReader / GetDataSet

```csharp
IDataReader GetDataReader(SqlDataAccessHelper helper)
DataSet GetDataSet(SqlDataAccessHelper helper)
```

### Transactions

Using a unit of work object, you can capture multiple requests into a transaction, and the transactions can span multiple databases

## Development
### Building the repo
Prereqs:
- .NET SDK (can be used via command line or visual studio)
- Local sql server instance. Tests utilize system dbs so no specific database is needed. Express can be used

Build
```csharp
dotnet build Tiddly.Sql.Core\Tiddly.Sql.csproj --configuration release
```

Test
```csharp
dotnet test Tiddly.Sql.Core.Tests\Tiddly.Sql.Tests.csproj --configuration release
```

### Contributing

Submit a pull request and I will review any enhancements. 