# SqlQueryTools
[![Build Status](https://marginet.visualstudio.com/SqlQueryTools/_apis/build/status/SqlQueryTools-CI?branchName=master&jobName=Build%20latest%20SqlQueryTools%20commit)](https://marginet.visualstudio.com/SqlQueryTools/_build/latest?definitionId=16&branchName=master)

SqlQueryTools is a visual studio extension that generates code behind files for sql files.
The code behinde file contains a static class with the same name as the sql file.
That class has a const called query, with the content of the sql file.
It also contains a const for every parameter that is declared in the sql file.

Before SqlQueryTools start generating the code behind file it validates the content of the sql file with a SQL Server database.

The generated code can be used to execute the query with an ORM like [Dapper](https://github.com/DapperLib/Dapper)

### Download

Download from [releases](https://github.com/gmarginet/SqlQueryTools/releases).

Or install it from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=GertMarginet.SqlQueryTools).

### Getting started

You can add a new sql file by right clicking on a .cs file and select 'Sql Query Tools/Add Sql File'.
This will add a nested sql file below the selected csharp file.

![image](https://github.com/gmarginet/SqlQueryTools/blob/master/art/AddSqlFile.png?raw=true)

The following dialog will pop up, enter the filename and click 'Add file'.

![image](https://github.com/gmarginet/SqlQueryTools/blob/master/art/AddNewSqlFileDialog.png?raw=true)

The added sql file will have the following content.

![image](https://github.com/gmarginet/SqlQueryTools/blob/master/art/SqlFileDefaultContent.png?raw=true)

Before you start writing your query you should set the SQL server connection string that Sql Query Tools should use to validate the query.
The connection string can be set in the .csproj file you just added the sql file to.

![image](https://github.com/gmarginet/SqlQueryTools/blob/master/art/ConnectionString.png?raw=true)

As of version 1.4 you can use the the properties window to edit the connection string, you just need to select the project file in the sollution explorer and open the properties window.

![image](https://github.com/gmarginet/SqlQueryTools/blob/master/art/ConnectionString_PropertiesWindow.png?raw=true)


Now you can start writing your sql query.
You can remove all comments but if you use parameters in your query you should put the parameter declarations above the 'End Of Parameter Declaration' marker.
And your actual query below the marker.
The generated const string will only contain the lines below the marker, if the sql file doesn't contain a marker line all lines will be put in the const string.

All comments that you keep in your sql file will not be put in the const string.

Below you can find an example of a query.

![image](https://github.com/gmarginet/SqlQueryTools/blob/master/art/ExampleSql.png?raw=true)

When you save the sql file, Sql Query Tools will start generating the code behind.
The progress can be found in the Sql Query Tools output panel.
Here you will also find more info when something went wrong.

![image](https://github.com/gmarginet/SqlQueryTools/blob/master/art/OutputPanel.png?raw=true)

And the generated code for the example query from above will look like this.

![image](https://github.com/gmarginet/SqlQueryTools/blob/master/art/ExampleCode.png?raw=true)

The solution explorer will now look like this.

![image](https://github.com/gmarginet/SqlQueryTools/blob/master/art/SolutionExplorer.png?raw=true)

If you don't like the file suffixes or the parameter declaration marker.
These can be changed via 'Tools/Options/SqlQueryTools'

![image](https://github.com/gmarginet/SqlQueryTools/blob/master/art/Options.png?raw=true)

### Parameters

By default SqlQueryTools was generating const fields for every parameter that you declared in the parameter declaration section.

As of version 1.4 this can be turned of in the properties window while the sql file is selected in the solution explorer.

![image](https://github.com/gmarginet/SqlQueryTools/blob/master/art/Sql_ParametersWindow.png?raw=true)

In 'Tools/Options/SqlQueryTools' you can change the default for new files that are created with the SqlQueryTools 'Add Sql File' context menu item.

![image](https://github.com/gmarginet/SqlQueryTools/blob/master/art/Options_Extra.png?raw=true)

### Poco class

From version 1.4 you can also ask SqlQueryTools to generate a poco class for the result that you get back from your query.

Lets use the following sql as an example

![image](https://github.com/gmarginet/SqlQueryTools/blob/master/art/ExampleSql_Poco.png?raw=true)

The code generated for this sql with 'Generate Poco' turned on would be the following:

![image](https://github.com/gmarginet/SqlQueryTools/blob/master/art/ExampleCode_Poco.png?raw=true)

You can turn on this feature at sql file level by using the properties window of the file, or change the default for new files via 'Tools/Options/SqlQueryTools'.

### License

MIT
