# tiddly sql

[<img
src="https://joskraps.visualstudio.com/_apis/public/build/definitions/e59eb71d-cb8a-4975-a09a-982754e10894/3/badge"/>](https://joskraps.visualstudio.com/Tiddly/_build/index?definitionId=2)
[![NuGet Badge](https://buildstats.info/nuget/Tiddly.Sql)](https://www.nuget.org/packages/Tiddly.Sql/)

Tiddly is a lightweight SQL ORM that handles convention based property mapping convert data results into domain objects or simple primitive types. Tiddly can be used without the mapping functionality and simply used to eliminate boilerplate command object code for data retrieval. 

## How to use

A data access helper object is used to define the transaction. It includes what statement or procedure will be executed, which parameters should be used, column mappings, and post processing functions.

```csharp
var helper = new SqlDataAccessHelper().AddStatement("select 0 [EnumType] union all select 1");
var values = new SqlDataAccess(ConnectionString).Fill<TestClassWithEnum>(helper);
```

That's all it takes to retrieve data from your instance!

### Data access helper

The helper is a setting object that define how the transaction will execute. It is required to utilize the data access object. 

```csharp
var helper = new SqlDataAccessHelper();
```

The helper has a fluent api that exposes the following functions for configuring the helper object:

#### AddProcedure

Adds the stored procedure to execute and optionally the schema to be used.

```csharp
SqlDataAccessHelper AddProcedure(string procedureName, string schema = "dbo")
```

#### AddStatement

Adds a sql statement to be executed. This query can be parameterized.

```csharp
SqlDataAccessHelper AddStatement(string stringStatement)
```

#### AddParameter

Adds a parameter that will be used in the supplied stored procedure or parameterized query. If scrub value is set to true, the string value will be used in a reg ex replace using a generic pattern. See here:

```csharp
SqlDataAccessHelper AddParameter(string name, object value, SqlDbType datatype, bool scrubValue = false)
```

#### Property

Retrieves the propery mapping for the provided string property name.

```csharp
ParameterMapping Property(string name)
```

#### AddCustomMapping

Supplies an alias for the target property. This will allow you to map properties to columns when the names do not match.

```csharp
SqlDataAccessHelper AddCustomMapping(string alias, string targetProperty)
```

#### SetPostProcessFunction

This allows a function to be defined that will execute after the value has been set. The value is supplied as a string and will be cast to the target property type from the function return.

```csharp
SqlDataAccessHelper SetPostProcessFunction(string targetProperty,
 Func mappingFunction)
```

```csharp
var da = new SqlDataAccess(ConnectionString);
var helper = new SqlDataAccessHelper();

helper. SetRetrievalMode(DataActionRetrievalType. DataSet);
helper. AddStatement(
 "select top 1 name, database_id [I'd], is_read_only [ReadOnly], service_broker_guid [BrokerGuid], create_date [CreateDate] from sys.databases where name in (@master, @model, @msdb) order by 1 asc");
helper. AddParameter("master", "master", SqlDbType. VarChar);
helper. AddParameter("model", "model", SqlDbType. VarChar);
helper. AddParameter("msdb", "msdb", SqlDbType. VarChar);
helper. SetPostProcessFunction("name", s => s == "master" ? "MAPPING FUNCTION" : s);

var returnable = da.Get<DatabaseModel>(helper);

this. OutputTestTimings(helper. ExecutionContext);

Assert. AreEqual(returnable. Name, "MAPPING FUNCTION");
Assert. IsTrue(returnable.I'd != 0);
Assert. AreEqual(returnable. BrokerGuid, Guid. Empty);
```

#### SetRetrievalMode

Allows you to switch between using a data reader or data set. Data reader should be the most perform ant of the two.

```csharp
SqlDataAccessHelper SetRetrievalMode(DataActionRetrievalType type)
```

#### SetTimeout

Sets the connection timeout on the underlying command object.

```csharp
SqlDataAccessHelper SetTimeout(int timeoutValue)
```
### SqlDataAccess object

The data access object is what actually creates the command and opens the connection/retrieves the data. 

### Collection fills

#### Fill
Fills a list with the supplied object or primitive value. Unless property mappings are supplied, this will attempt to match properties to column names. 

```csharp
IList<T> Fill<T>(SqlDataAccessHelper helper)
```
```csharp
var da = new SqlDataAccess(connectionString);
var helper = new SqlDataAccessHelper();

helper. AddStatement("select name from sys.databases where name in (@master, @model, @msdb) order by 1 asc");
helper. AddParameter("master", "master", SqlDbType. VarChar);
helper. AddParameter("model", "model", SqlDbType. VarChar);
helper. AddParameter("msdb", "msdb", SqlDbType. VarChar);

var values = da.Fill<string>(helper);
```

```csharp
private class DbHelpModel
{
 public string Name {get; set;}
 public string Owner {get; set;}
 public string ObjectType {get; set;}
}

var da = new SqlDataAccess(ConnectionString);
var helper = new SqlDataAccessHelper();

helper. SetRetrievalMode(DataActionRetrievalType. DataReader);
helper. AddProcedure("sp_help");

var values = da.Fill<DbHelpModel>(helper);
```

#### FillToDictionary
Same as filling to a list but allows you to specify a property that will key a dictionary. An exception will be thrown if the property does not exist, the property type does not match, or duplicate values are found for the key. Duplicate key behaviour can be overridden so that the key value will overwrite instead of throwing an exception.

```csharp
Dictionary<TKey, TObjType> FillToDictionary<TKey, TObjType>(string keyPropertyName, SqlDataAccessHelper helper, bool overwriteOnDupe = false)
```
```csharp
public class DatabaseModel
{
 public string Name {get; set;}
 public Guid BrokerGuid {get; set;}
 public DateTime CreateDate {get; set;}
 public int Id {get; set;}
 public bool ReadOnly {get; set;}

}
 
var da = new SqlDataAccess(connectionString);
var helper = new SqlDataAccessHelper();

helper. AddStatement("select name, row_number() over (ORDER BY name ASC)[I'd], is_read_only [ReadOnly], service_broker_guid [BrokerGuid], create_date [CreateDate] from sys.databases where name in (@master, @model, @msdb) order by 1 asc");
helper. AddParameter("master", "master", SqlDbType. VarChar);
helper. AddParameter("model", "model", SqlDbType. VarChar);
helper. AddParameter("msdb", "msdb", SqlDbType. VarChar);

var values = da.FillToDictionary<int,DatabaseModel>("Id",helper);
```

### Single object/value fill
Returns an instance of type T that you specify: can be an object or primitive type. Does not support nested objects

#### Execute

Returns the scalar int value that is produced from executing the statement. This is more useful for DML type actions where the return is the number of rows effected or status code.

```csharp
int Execute(SqlDataAccessHelper helper)
```

#### Get - primitive
```csharp
T Get<T>(SqlDataAccessHelper helper)

var da = new SqlDataAccess(ConnectionString);
var helper = new SqlDataAccessHelper();

helper. SetRetrievalMode(DataActionRetrievalType. DataSet);
helper. AddStatement(
 "select top 1 name from sys.databases where name in (@master, @model, @msdb) order by 1 asc");
helper. AddParameter("master", "master", SqlDbType. VarChar);
helper. AddParameter("model", "model", SqlDbType. VarChar);
helper. AddParameter("msdb", "msdb", SqlDbType. VarChar);

var returnValue = da.Get<string>(helper);
```

#### Get - object
```csharp
var da = new SqlDataAccess(ConnectionString);
var helper = new SqlDataAccessHelper();

helper. SetRetrievalMode(DataActionRetrievalType. DataReader);
helper. AddStatement(
 "select top 1 name, database_id [I'd], is_read_only [ReadOnly], service_broker_guid [BrokerGuid], create_date [CreateDate] from sys.databases where name in (@master, @model, @msdb) order by 1 asc");
helper. AddParameter("master", "master", SqlDbType. VarChar);
helper. AddParameter("model", "model", SqlDbType. VarChar);
helper. AddParameter("msdb", "msdb", SqlDbType. VarChar);

var returnValue = da.Get<DatabaseModel>(helper);
```

### GetDataReader / GetDataSet

Can be used to get the raw data structures if you just want something to get the raw data for you.

```csharp
IDataReader GetDataReader(SqlDataAccessHelper helper)
DataSet GetDataSet(SqlDataAccessHelper helper)
```

### Performance 

Using some quick performance comparisons, Tiddly is on par with mainstream libraries like Dapper. At a minimum, I wanted to make sure it was not drastically slower than what's available and not a lot of time was spent on spinning up the tests.

|Library| Method | Mean | Error | StdDev | Median | Min | Max |
|-------|:------------------------------------------------------------------ |---------:|----------:|----------:|---------:|---------:|---------:|
|Tiddly | Get string from data reader | 225.2 us | 1.334 us | 1.182 us | 225.6 us | 222.4 us | 226.3 us |
|Tiddly | Get object from data reader | 252.5 us | 2.113 us | 1.977 us | 253.0 us | 247.5 us | 254.4 us |
|Tiddly | Fill string collection from data reader | 307.8 us | 4.318 us | 4.039 us | 307.6 us | 302.4 us | 315.1 us |
|Tiddly | Fill object collection from data reader | 396.1 us | 4.519 us | 4.006 us | 396.5 us | 386.5 us | 402.9 us |
|Dapper | Get string from data reader(unbuf) | 254.2 us | 4.907 us | 5.651 us | 252.0 us | 244.7 us | 268.6 us |
|Dapper | Get string from data reader(buf) | 225.1 us | 3.096 us | 2.896 us | 225.4 us | 219.4 us | 229.8 us |
|Dapper | Get object from data reader | 379.1 us | 33.078 us | 95.965 us | 351.5 us | 236.4 us | 611.8 us |
|Dapper | Fill string collection from data reader | 322.6 us | 5.694 us | 5.326 us | 322.5 us | 315.8 us | 331.3 us |
|Dapper | Fill object collection from data reader | 477.1 us | 27.851 us | 81.243 us | 460.4 us | 322.9 us | 695.4 us |
|Base | Get string | 217.5 us | 3.066 us | 2.868 us | 217.0 us | 214.1 us | 222.7 us |

#### Legends
 - Mean : Arithmetic mean of all measurements
 - Error : Half of 99.9% confidence interval
 - StdDev : Standard deviation of all measurements
 - Median : Value separating the higher half of all measurements (50th percentile)
 - Min : Minimum
 - Max : Maximum
 - 1 us : 1 Microsecond (0.000001 sec)


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
