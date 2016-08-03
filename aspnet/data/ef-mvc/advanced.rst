Advanced topics
===============

The Contoso University sample web application demonstrates how to create ASP.NET Core 1.0 MVC web applications using Entity Framework Core 1.0 and Visual Studio 2015. For information about the tutorial series, see [the first tutorial in the series](todo).

In the previous tutorial you implemented table-per-hierarchy inheritance. This tutorial includes introduces several topics that are useful to be aware of when you go beyond the basics of developing ASP.NET web applications that use Entity Framework Code First. Step-by-step instructions walk you through the code and using Visual Studio for the following topics:

* Performing raw SQL queries
* Performing no-tracking queries
* Examining SQL sent to the database

The tutorial introduces several topics with brief introductions followed by links to resources for more information:

* Repository and unit of work patterns
* Proxy classes
* Automatic change detection
* Automatic validation
* Entity Framework source code
* Reverse engineer from existing database
* Database Providers
* Visualization tools

The tutorial also includes the following sections:

* Summary
* Acknowledgments
* A note about VB
* Common errors, and solutions or workarounds for them

For most of these topics, you'll work with pages that you already created. To use raw SQL to do bulk updates you'll create a new page that updates the number of credits of all courses in the database:

![Update Course Credits page](todo)

## Performing Raw SQL Queries

One of the advantages of using the Entity Framework is that it avoids tying your code too closely to a particular method of storing data. It does this by generating SQL queries and commands for you, which also frees you from having to write them yourself. But there are exceptional scenarios when you need to run specific SQL queries that you have manually created. For these scenarios, the Entity Framework Code First API includes methods that enable you to pass SQL commands directly to the database. You have the following options:

* Use the DbSet.FromSql method for queries that return entity types. The returned objects must be of the type expected by the DbSet object, and they are automatically tracked by the database context unless you turn tracking off. (See the following section about the AsNoTracking method.)
* Use the Database.ExecuteSqlCommand for non-query commands.

If you need to run a query that returns types that aren't entities, you can use ADO.NET with the database connection provided by EF. The returned data isn't tracked by the database context, even if you use this method to retrieve entity types.

As is always true when you execute SQL commands in a web application, you must take precautions to protect your site against SQL injection attacks. One way to do that is to use parameterized queries to make sure that strings submitted by a web page can't be interpreted as SQL commands. In this tutorial you'll use parameterized queries when integrating user input into a query.

### Calling a Query that Returns Entities

The `DbSet<TEntity>` class provides a method that you can use to execute a query that returns an entity of type TEntity. To see how this works you'll change the code in the Details method of the Department controller.

In DepartmentController.cs, in the Details method, replace the db.Departments.FindAsync method call with a db.Departments.SqlQuery method call, as shown in the following highlighted code:

```
       public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var department = await _context.Departments.Include(d => d.Administrator).SingleOrDefaultAsync(m => m.DepartmentID == id);
            string query = "SELECT * FROM Departments WHERE DepartmentID = {0}";
            var department = await _context.Departments
                .FromSql(query, id)
                .Include(d => d.Administrator)
                .SingleOrDefaultAsync(d => d.DepartmentID == id);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }
```

To verify that the new code works correctly, select the Departments tab and then Details for one of the departments.

![Department Details](todo)

### Calling a Query that Returns Other Types of Objects

Earlier you created a student statistics grid for the About page that showed the number of students for each enrollment date. The code that does this in HomeController.cs uses LINQ:

```
var data = from student in db.Students
           group student by student.EnrollmentDate into dateGroup
           select new EnrollmentDateGroup()
           {
               EnrollmentDate = dateGroup.Key,
               StudentCount = dateGroup.Count()
           };
```

Suppose you want to write the code that retrieves this data directly in SQL rather than using LINQ. To do that you need to run a query that returns something other than entity objects, which means you need to use ADO.NET.

In HomeController.cs, replace the LINQ statement in the About method with a SQL statement, as shown in the following highlighted code:

```
        public async Task<ActionResult> About()
        {
            List<EnrollmentDateGroup> groups = new List<EnrollmentDateGroup>();
            using (var conn = _context.Database.GetDbConnection())
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    string query = "SELECT EnrollmentDate, COUNT(*) AS StudentCount "
                    + "FROM Students "
                    + "GROUP BY EnrollmentDate";
                    command.CommandText = query;
                    DbDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var row = new EnrollmentDateGroup { EnrollmentDate = reader.GetDateTime(0), StudentCount = reader.GetInt32(1) };
                            groups.Add(row);
                        }
                    }
                    reader.Dispose();
                }
            }
            return View(groups);
        }
```

Add using statements:

```
using System.Collections.Generic;
using System.Data.Common;
```

Run the About page. It displays the same data it did before.

![About page](todo)

### Calling an Update Query

Suppose Contoso University administrators want to be able to perform bulk changes in the database, such as changing the number of credits for every course. If the university has a large number of courses, it would be inefficient to retrieve them all as entities and change them individually. In this section you'll implement a web page that enables the user to specify a factor by which to change the number of credits for all courses, and you'll make the change by executing a SQL UPDATE statement. The web page will look like the following illustration:

![Update Course Credits page](todo)

In CoursesContoller.cs, add UpdateCourseCredits methods for HttpGet and HttpPost:

```
public ActionResult UpdateCourseCredits()
{
    return View();
}

[HttpPost]
public ActionResult UpdateCourseCredits(int? multiplier)
{
    if (multiplier != null)
    {
        ViewBag.RowsAffected = db.Database.ExecuteSqlCommand("UPDATE Course SET Credits = Credits * {0}", multiplier);
    }
    return View();
}
```

When the controller processes an HttpGet request, nothing is returned in ViewData["RowsAffected"], and the view displays an empty text box and a submit button, as shown in the preceding illustration.

When the Update button is clicked, the HttpPost method is called, and multiplier has the value entered in the text box. The code then executes the SQL that updates courses and returns the number of affected rows to the view in the ViewBag.RowsAffected variable. When the view gets a value in that variable, it displays the number of rows updated  instead of the text box and submit button, as shown in the following illustration:

![Update Course Credits rows affected](todo)

In Solution Explorer, right-click the Views\Controllers folder, and then click Add > New Item.

In the Add New Item dialog, click ASP.NET under Installed in the left pane, click MVC View Page, and name the new view UpdateCourseCredits.cshtml.

![Add New Item dialog](todo)

In Views\Course\UpdateCourseCredits.cshtml, replace the template code with the following code:

```
@{
    ViewBag.Title = "UpdateCourseCredits";
}

<h2>Update Course Credits</h2>

@if (ViewData["RowsAffected"] == null)
{
    <form asp-action="UpdateCourseCredits">
        <div class="form-actions no-color">
            <p>
                Enter a number to multiply every course's credits by: @Html.TextBox("multiplier")
            </p>
            <p>
                <input type="submit" value="Update" class="btn btn-default" />
            </p>
        </div>
    </form>
}
@if (ViewData["RowsAffected"] != null)
{
    <p>
        Number of rows updated: @ViewData["RowsAffected"]
    </p>
}
<div>
    @Html.ActionLink("Back to List", "Index")
</div>
```

Run the UpdateCourseCredits method by selecting the Courses tab, then adding "/UpdateCourseCredits" to the end of the URL in the browser's address bar (for example: http://localhost:50205/Course/UpdateCourseCredits). Enter a number in the text box:

![Update Course Credits page](todo)

Click Update. You see the number of rows affected:

![Update Course Credits page rows affected](todo)

Click Back to List to see the list of courses with the revised number of credits.

![Courses Index page](todo)

For more information about raw SQL queries, see Raw SQL Queries on MSDN.todo update link

## Examining SQL sent to the database

Sometimes it's helpful to be able to see the actual SQL queries that are sent to the database. Built-in logging functionality for ASP.NET Core is automatically used by EF Core to write logs that contain the SQL for queries and updates. In this section you'll see some examples of SQL logging.

todo 		? By default, EF Core will log a warning when client evaluation is performed. Show for Single method


todo  go to Students, Details see paging query and Top(2) for SingleOrDefault.


## No-Tracking Queries

When a database context retrieves table rows and creates entity objects that represent them, by default it keeps track of whether the entities in memory are in sync with what's in the database. The data in memory acts as a cache and is used when you update an entity. This caching is often unnecessary in a web application because context instances are typically short-lived (a new one is created and disposed for each request) and the context that reads an entity is typically disposed before that entity is used again.

You can disable tracking of entity objects in memory by using the AsNoTracking method. Typical scenarios in which you might want to do that include the following:

* A query retrieves such a large volume of data that turning off tracking might noticeably enhance performance.
* You want to attach an entity in order to update it, but you earlier retrieved the same entity for a different purpose. Because the entity is already being tracked by the database context, you can't attach the entity that you want to change. One way to handle this situation is to use the AsNoTracking option with the earlier query.

For an example that demonstrates how to use the AsNoTracking method, see the earlier version of this tutorial. This version of the tutorial doesn't set the Modified flag on a model-binder-created entity in the Edit method, so it doesn't need AsNoTracking.todo see the concurrency example code


## Repository and unit of work patterns

Many developers write code to implement the repository and unit of work patterns as a wrapper around code that works with the Entity Framework. These patterns are intended to create an abstraction layer between the data access layer and the business logic layer of an application. Implementing these patterns can help insulate your application from changes in the data store and can facilitate automated unit testing or test-driven development (TDD). However, writing additional code to implement these patterns is not always the best choice for applications that use EF, for several reasons:

* The EF context class itself insulates your code from data-store-specific code.
* The EF context class can act as a unit-of-work class for database updates that you do using EF.
* EF includes features for implementing TDD without writing repository code.

For more information about how to implement the repository and unit of work patterns, see the Entity Framework 5 version of this tutorial series. For information about ways to implement TDD in Entity Framework Core, see the following resources:

todo update this list
* How EF6 Enables Mocking DbSets more easily
* Testing with a mocking framework
* Testing with your own test doubles

## Automatic change detection

The Entity Framework determines how an entity has changed (and therefore which updates need to be sent to the database) by comparing the current values of an entity with the original values. The original values are stored when the entity is queried or attached. Some of the methods that cause automatic change detection are the following:

todo verify list
DbSet.Find
DbSet.Local
DbSet.Remove
DbSet.Add
DbSet.Attach
DbContext.SaveChanges
DbContext.GetValidationErrors
DbContext.Entry
DbChangeTracker.Entries

If you're tracking a large number of entities and you call one of these methods many times in a loop, you might get significant performance improvements by temporarily turning off automatic change detection using the AutoDetectChangesEnabled property.todo verify up-to-date For more information, see Automatically Detecting Changes on MSDN.todo update link

## Automatic validation

When you call the SaveChanges method, by default the Entity Framework validates the data in all properties of all changed entities before updating the database. If you've updated a large number of entities and you've already validated the data, this work is unnecessary and you could make the process of saving the changes take less time by temporarily turning off validation. You can do that using the ValidateOnSaveEnabled property. For more information, see Validation on MSDN.todo validate and update link

Entity Framework source code

The source code for Entity Framework Core is available at http://entityframework.codeplex.com/. todo update linkBesides source code, you can get nightly builds, issue tracking, feature specs, design meeting notes, and more. You can file bugs, and you can contribute your own enhancements to the EF source code.

Although the source code is open, Entity Framework is fully supported as a Microsoft product. The Microsoft Entity Framework team keeps control over which contributions are accepted and tests all code changes to ensure the quality of each release. 

## Reverse engineer from existing database

todo 	Reverse Engineer with Scaffold-DbContext
	
	From <https://msdn.microsoft.com/en-us/magazine/mt614250.aspx> 
	


## Database providers

todo link to EF docs

## Visualization tools

todo Coming in November

## Summary

This completes this series of tutorials on using the Entity Framework in an ASP.NET MVC application. For more information about how to work with data using the Entity Framework, see the EF documentation.todo update link

For more information about how to deploy your web application after you've built it, see todo add azure link

For information about other topics related to ASP.NET Core MVC, such as authentication and authorization, see the ASP.NET Core documentation.todo update link

## Acknowledgments

Tom Dykstra and Rick Anderson (twitter @RickAndMSFT) wrote this tutorial.
Rowan Miller and other members of the Entity Framework team assisted with code reviews and helped debug issues that arose while we were writing code for the tutorials.

## VB

When the tutorial was originally produced for EF 4.1, we provided both C# and VB versions of the completed download project. Due to time limitations and other priorities we have not done that for this version. If you build a VB project using these tutorials and would be willing to share that with others, please let us know.

## Common errors, and solutions or workarounds for them

todo validate these are still issues

### Cannot create/shadow copy

Error Message:

Cannot create/shadow copy '<filename>' when that file already exists.

Solution

 Wait a few seconds and refresh the page.

### Update-Database not recognized

Error Message (from the Update-Database command in the PMC):

The term 'Update-Database' is not recognized as the name of a cmdlet, function, script file, or operable program. Check the spelling of the name, or if a path was included, verify that the path is correct and try again.

Solution

 Exit Visual Studio. Reopen project and try again.

### Validation failed

Error Message (from the Update-Database command in the PMC):

Validation failed for one or more entities. See 'EntityValidationErrors' property for more details.

Solution

One cause of this problem is validation errors when the Seed method runs.  See Seeding and Debugging Entity Framework (EF) DBs for tips on debugging the Seed method.

### HTTP 500.19 error

Error Message:

HTTP Error 500.19 - Internal Server Error
The requested page cannot be accessed because the related configuration data for the page is invalid.

Solution

One way you can get this error is from having multiple copies of the solution, each of them using the same port number. You can usually solve this problem by exiting all instances of Visual Studio, then restarting the project you're working on. If that doesn't work, try changing the port number. Right click on the project file and then click properties. Select the Web tab and then change the port number in the Project Url text box.

### Error locating SQL Server instance

Error Message:

A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: SQL Network Interfaces, error: 26 - Error Locating Server/Instance Specified)

Solution

Check the connection string. If you have manually deleted the database, change the name of the database in the construction string.

## SQL Login fail error

Solution to Login fail error. If you delete the DB file, it still stays registered with SqlLocalDB. If LocalDb gets hung up with can’t attach (after manually deleting the DB file) or Login failed see JSobell’s and CodingwithSpike’s answers here: ef5-cannot-attach-the-file-0-as-database-1 http://stackoverflow.com/questions/13275054/ef5-cannot-attach-the-file-0-as-database-1/16339164#16339164 Run ‘sqllocaldb.exe stop v11.0’ and ‘sqllocaldb.exe delete v11.0’ from the PM Console

## SQL open error

o	You must stop IIS Express before you update the database. If IIS-Express is running, you’ll get the error CS2012: Cannot open ‘MvcMovie/bin/Debug/netcoreapp1.0/MvcMovie.dll’ for writing – ‘The process cannot access the file ‘MvcMovie/bin/Debug/netcoreapp1.0/MvcMovie.dll’ because it is being used by another process.’