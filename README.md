# SqlQueryTools

SqlQueryTools is a visual studio extension that generates code behind files for sql files.
The code behinde file contains a static class with the same name as the sql file.
That class has a const called sql, with the content of the sql file.
It also contains a const for every parameter that is declared in the sql file.

Before SqlQueryTools start generating the code behind file it validates the content of the sql file with the database.

When the query in the sql file returns a result set the code behind file will also contain a dto class.

The generated cde can be used to execute the query with an ORM like [Dapper](https://github.com/DapperLib/Dapper)

### Download

> Download not available yet, download sources and build visx file

### Getting started


### License

MIT
