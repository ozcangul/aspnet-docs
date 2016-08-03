Migrations
==========

The Contoso University sample web application demonstrates how to create ASP.NET Core 1.0 MVC web applications using Entity Framework Core 1.0 and Visual Studio 2015. For information about the tutorial series, see [the first tutorial in the series](todo).

So far the application has been running locally in IIS Express on your development computer. To make a real application available for other people to use over the Internet, you have to deploy it to a web hosting provider. In this tutorial you'll deploy the Contoso University application to the cloud in Azure.

The tutorial contains the following sections:

* Set up Code First Migrations. The Migrations feature enables you to change the data model and deploy your changes to production by updating the database schema without having to drop and re-create the database.
* Deploy to Azure. This step is optional; you can continue with the remaining tutorials without having deployed the project.

We recommend that you use a continuous integration process with source control for deployment, but this tutorial does not cover those topics. For more information, see the source control and continuous integration chapters of the Building Real-World Cloud Apps with Azure e-book.

## Code First Migrations

When you develop a new application, your data model changes frequently, and each time the model changes, it gets out of sync with the database. You started by configuring the Entity Framework to automatically create the database if it doesn't exist. Then each time you change the data model -- add, remove, or change entity classes or change your DbContext class -- you can delete the database and EF creates a new one that matches the model, and seeds it with test data.

This method of keeping the database in sync with the data model works well until you deploy the application to production. When the application is running in production it is usually storing data that you want to keep, and you don't want to lose everything each time you make a change such as adding a new column. The Code First Migrations feature solves this problem by enabling Code First to update the database schema instead of creating  a new database. In this tutorial, you'll deploy the application, and to prepare for that you'll set up Migrations.  

In the appsettings.json file, change the name of the database in the connection string to ContosoUniversity2.

```
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=aspnet-ContosoUniversity2;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
```

This change sets up the project so that the first migration will create a new database. This isn't required but you'll see later why it's a good idea.

From the Tools menu, click Library Package Manager and then Package Manager Console.

![PMC in menu]()

At the PM> prompt enter the following commands:

```
use-dbcontext SchoolContext
add-migration InitialCreate
```

![PMC command execution]()

todo use statement is because there's an identity context. can add -context SchoolContext to each command

When you executed the add-migration command, Migrations generated the code that would create the database from scratch. This code is in the Migrations folder, in the file named `<timestamp>_InitialCreate.cs`. The Up method of the InitialCreate class creates the database tables that correspond to the data model entity sets, and the Down method deletes them. 

```
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ContosoUniversity.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    CourseID = table.Column<int>(nullable: false),
                    Credits = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.CourseID);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EnrollmentDate = table.Column<DateTime>(nullable: false),
                    FirstMidName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    EnrollmentID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseID = table.Column<int>(nullable: false),
                    Grade = table.Column<int>(nullable: true),
                    StudentID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.EnrollmentID);
                    table.ForeignKey(
                        name: "FK_Enrollments_Courses_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Courses",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Enrollments_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CourseID",
                table: "Enrollments",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentID",
                table: "Enrollments",
                column: "StudentID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
```

Migrations calls the Up method to implement the data model changes for a migration. When you enter a command to roll back the update, Migrations calls the Down method.

This is the initial migration that was created when you entered the add-migration InitialCreate command. The parameter (InitialCreate in the example) is used for the file name and can be whatever you want; you typically choose a word or phrase that summarizes what is being done in the migration. For example, you might name a later migration "AddDepartmentTable".

If you created the initial migration when the database already exists, the database creation code is generated but it doesn't have to run because the database already matches the data model. When you deploy the app to another environment where the database doesn't exist yet, this code will run to create your database, so it's a good idea to test it first. That's why you changed the name of the database in the connection string earlier -- so that migrations can create a new one from scratch.

In the Package Manager Console window, enter the following command:

```
update-database
```

![PMC command execution]()

The update-database command runs the Up method to create the database. The same process will run automatically in production after you deploy the application, as you'll see in the following section.

Use SQL Server Object Explorer to inspect the database as you did in the first tutorial, todo see Migrations history table.  

todo Migrations also creates model snapshot 	Because of the model snapshot that EF7 stores in the project, you shouldn’t just delete a migration file from your project because the snapshot would then be incorrect. So, along with the change to the migrations workflow comes a new command: remove-migration. 
	
	From <https://msdn.microsoft.com/en-us/magazine/mt614250.aspx> 


Run the application to verify that everything still works the same as before. 


## Command line vs. PMC

todo
		? Command line alternative to PMC 
		dotnet ef migrations add Initial
		dotnet ef database update
Intro to dotnet ef statements https://docs.asp.net/en/latest/tutorials/first-mvc-app/adding-model.html#dotnet-ef-commands
https://ef.readthedocs.io/en/latest/miscellaneous/cli/powershell.html


## Summary

In this tutorial you've seen how to enable migrations. todo link to EF docs for more info
In the next tutorial you'll begin looking at more advanced topics by expanding the data model.
