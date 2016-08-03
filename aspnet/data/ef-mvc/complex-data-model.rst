Creating a complex data model
=============================

The Contoso University sample web application demonstrates how to create ASP.NET Core 1.0 MVC web applications using Entity Framework Core 1.0 and Visual Studio 2015. For information about the tutorial series, see [the first tutorial in the series](todo).

In the previous tutorials you worked with a simple data model that was composed of three entities. In this tutorial you'll add more entities and relationships and you'll customize the data model by specifying formatting, validation, and database mapping rules. You'll see two ways to customize the data model:  by adding attributes to entity classes and by adding code to the database context class.

When you're finished, the entity classes will make up the completed data model that's shown in the following illustration:

![Entity diagram](todo)

Customize the Data Model by Using Attributes

In this section you'll see how to customize the data model by using attributes that specify formatting, validation, and database mapping rules. Then in several of the following sections you'll create the complete School data model by adding attributes to the classes you already created and creating new classes for the remaining entity types in the model.

## The DataType Attribute

For student enrollment dates, all of the web pages currently display the time along with the date, although all you care about for this field is the date. By using data annotation attributes, you can make one code change that will fix the display format in every view that shows the data. To see an example of how to do that, you'll add an attribute to the EnrollmentDate property in the Student class.

In Models\Student.cs, add a using statement for the System.ComponentModel.DataAnnotations namespace and add DataType and DisplayFormat attributes to the EnrollmentDate property, as shown in the following example:

```
using System.ComponentModel.DataAnnotations;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]

```

The DataType attribute is used to specify a data type that is more specific than the database intrinsic type. In this case we only want to keep track of the date, not the date and time. The  DataType Enumeration provides for many data types, such as Date, Time, PhoneNumber, Currency, EmailAddress and more. The DataType attribute can also enable the application to automatically provide type-specific features. For example, a mailto: link can be created for DataType.EmailAddress, and a date selector can be provided for DataType.Date in browsers that support HTML5. The DataType attributes emits HTML 5 data- (pronounced data dash) attributes that HTML 5 browsers can understand. The DataType attributes do not provide any validation. 

DataType.Date does not specify the format of the date that is displayed. By default, the data field is displayed according to the default formats based on the server's CultureInfo.

The DisplayFormat attribute is used to explicitly specify the date format:

 [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
The ApplyFormatInEditMode setting specifies that the specified formatting should also be applied when the value is displayed in a text box for editing. (You might not want that for some fields — for example, for currency values, you might not want the currency symbol in the text box for editing.)

You can use the DisplayFormat attribute by itself, but it's generally a good idea to use the DataType attribute also. The DataType attribute conveys the semantics of the data as opposed to how to render it on a screen, and provides the following benefits that you don't get with DisplayFormat:

The browser can enable HTML5 features (for example to show a calendar control, the locale-appropriate currency symbol, email links, some client-side input validation, etc.).
By default, the browser will render data using the correct format based on your locale.
The DataType attribute can enable MVC to choose the right field template to render the data (the DisplayFormat uses the string template). todo does this still apply? For more information, see Brad Wilson's ASP.NET MVC 2 Templates. (Though written for MVC 2, this article still applies to the current version of ASP.NET MVC.)
todo does this still apply If you use the DataType attribute with a date field, you have to specify the DisplayFormat attribute also in order to ensure that the field renders correctly in Chrome browsers. For more information, see this StackOverflow thread.

todo update linkn For more information about how to handle other date formats in MVC, todo does this still apply go to MVC 5 Introduction: Examining the Edit Methods and Edit View and search in the page for "internationalization".
See Localization note: https://docs.asp.net/en/latest/tutorials/first-mvc-app/adding-model.html#test-the-app

Run the Student Index page again and notice that times are no longer displayed for the enrollment dates. The same will be true for any view that uses the Student model.

![Students index page showing dates without times](todo)

## The StringLengthAttribute

You can also specify data validation rules and validation error messages using attributes. The StringLength attribute sets the maximum length  in the database and provides client side and server side validation for ASP.NET MVC. You can also specify the minimum string length in this attribute, but the minimum value has no impact on the database schema.

Suppose you want to ensure that users don't enter more than 50 characters for a name. To add this limitation, add StringLength attributes to the LastName and FirstMidName properties, as shown in the following example:

```
        [StringLength(50)]

       [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
```


todo 		? EF doesn't validate maximum string length
https://ef.readthedocs.io/en/latest/modeling/max-length.html

The StringLength attribute won't prevent a user from entering white space for a name. You can use the RegularExpression attribute to apply restrictions to the input. For example the following code requires the first character to be upper case and the remaining characters to be alphabetical:

```
[RegularExpression(@"^[A-Z]+[a-zA-Z''-'\s]*$")]
```

The MaxLength attribute provides similar functionality to the StringLength attribute but doesn't provide client side validation.

Run the application and click the Students tab. You get the following error:

todo why doesn't this error happen?  The model backing the 'SchoolContext' context has changed since the database was created. Consider using Code First Migrations to update the database (http://go.microsoft.com/fwlink/?LinkId=238269).

The database model  has changed in a way that requires a change in the database schema. You'll use migrations to update the schema without losing any data that you added to the database by using the application UI.

In the Package Manager Console (PMC), enter the following commands:

```
add-migration MaxLengthOnNames
update-database
```

(If you've closed and reopened Visual Studio since the last time you ran PMC commands, you'll have to rerun the `use-dbcontext SchoolContext` command first.)

The add-migration command creates a file named <timeStamp>_MaxLengthOnNames.cs. This file contains code in the Up method that will update the database to match the current data model. The update-database command ran that code.

The timestamp prepended to the migrations file name is used by Entity Framework to order the migrations. You can create multiple migrations before running the update-database command, and then all of the migrations are applied in the order in which they were created.

Run the Create page, and enter either name longer than 50 characters. When you click Create, client side validation shows an error message.

![Students index page showing string length errors](todo)

## The Column Attribute

You can also use attributes to control how your classes and properties are mapped to the database. Suppose you had used the name FirstMidName for the first-name field because the field might also contain a middle name. But you want the database column to be named FirstName, because users who will be writing ad-hoc queries against the database are accustomed to that name. To make this mapping, you can use the Column attribute.

The Column attribute specifies that when the database is created, the column of the Student table that maps to the FirstMidName property will be named FirstName. In other words, when your code refers to Student.FirstMidName, the data will come from or be updated in the FirstName column of the Student table. If you don't specify column names, they are given the same name as the property name.

In the Student.cs file, add a using statement for System.ComponentModel.DataAnnotations.Schema and add the column name attribute to the FirstMidName property, as shown in the following highlighted code:

```
using System.ComponentModel.DataAnnotations.Schema;

       [Column("FirstName")]
```

The addition of the Column attribute changes the model backing the SchoolContext, so it won't match the database. Enter the following commands in the PMC to create another migration:

```
add-migration ColumnFirstName
update-database
```

In SQL Server Object Explorer, open the Student table designer by double-clicking the Student table.

![Students table in SSOX after migrations](todo)

The following image shows the original column name as it was before you applied the first two migrations. In addition to the column name changing from FirstMidName to FirstName, the two name columns have changed from MAX length to 50 characters.

![Students table in SSOX before migrations](todo)

You can also make database mapping changes using the Fluent API, as you'll see later in this tutorial.

todo note format
Note If you try to compile before you finish creating all of the entity classes in the following sections, you might get compiler errors.

## Complete Changes to the Student Entity

![Student entity diagram](todo)

In Models\Student.cs, replace the code you added earlier with the following code. The changes are highlighted.

```
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
    public class Student
    {
        public int ID { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        [Column("FirstName")]
        [Display(Name = "First Name")]
        public string FirstMidName { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Enrollment Date")]
        public DateTime EnrollmentDate { get; set; }

        [Display(Name = "Full Name")]
        public string FullName
        {
            get
            {
                return LastName + ", " + FirstMidName;
            }
        }

        public ICollection<Enrollment> Enrollments { get; set; }
    }
}
```

### The Required Attribute

The Required attribute makes the name properties required fields. The Required attribute is not needed for non-nullable types such as value types (DateTime, int, double, float, etc.). Types that can't be null are automatically treated as required fields. 

You could remove the Required attribute and replace it with a minimum length parameter for the StringLength attribute:

```
      [Display(Name = "Last Name")]
      [StringLength(50, MinimumLength=1)]
      public string LastName { get; set; }
```

### The Display Attribute

The Display attribute specifies that the caption for the text boxes should be "First Name", "Last Name", "Full Name", and "Enrollment Date" instead of the property name in each instance (which has no space dividing the words).

### The FullName Calculated Property

FullName is a calculated property that returns a value that's created by concatenating two other properties. Therefore it has only a get accessor, and no FullName column will be generated in the database. 

## Create the Instructor Entity

![Instructor entity diagram](todo)

Create Models\Instructor.cs, replacing the template code with the following code:

```
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
    public class Instructor
    {
        public int ID { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [Column("FirstName")]
        [Display(Name = "First Name")]
        [StringLength(50)]
        public string FirstMidName { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Hire Date")]
        public DateTime HireDate { get; set; }

        [Display(Name = "Full Name")]
        public string FullName
        {
            get { return LastName + ", " + FirstMidName; }
        }

        public ICollection<CourseInstructor> Courses { get; set; }
        public OfficeAssignment OfficeAssignment { get; set; }
    }
}
```

Notice that several properties are the same in the Student and Instructor entities. In the Implementing Inheritance tutorial later in this series, you'll refactor this code to eliminate the redundancy.

You can put multiple attributes on one line, so you could also write the HireDate attributes as follows:

```
[DataType(DataType.Date),Display(Name = "Hire Date"),DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
```

### The Courses and OfficeAssignment Navigation Properties

The Courses and OfficeAssignment properties are navigation properties. If a navigation property can hold multiple entities, its type must implement the `ICollection<T>` Interface. For example `IList<T>` qualifies but not `IEnumerable<T>` because IEnumerable<T> doesn't implement `Add`.

An instructor can teach any number of courses, so Courses is defined as a collection of courses. The reason why these are InstructorCourse entities is explained below where we talk about many-to-many relationships.

```
public ICollection<InstructorCourse> Courses { get; set; }
```

Our business rules state an instructor can only have at most one office, so OfficeAssignment is defined as a single OfficeAssignment entity (which may be null if no office is assigned).

```
public virtual OfficeAssignment OfficeAssignment { get; set; }
```

## Create the OfficeAssignment Entity

![OfficeAssignment entity diagram](todo)

Create Models\OfficeAssignment.cs with the following code:

```
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
    public class OfficeAssignment
    {
        [Key]
        [ForeignKey("Instructor")]
        public int InstructorID { get; set; }
        [StringLength(50)]
        [Display(Name = "Office Location")]
        public string Location { get; set; }

        public virtual Instructor Instructor { get; set; }
    }
}
```

### The Key Attribute

There's a one-to-zero-or-one relationship  between the Instructor and the OfficeAssignment entities. An office assignment only exists in relation to the instructor it's assigned to, and therefore its primary key is also its foreign key to the Instructor entity. But the Entity Framework can't automatically recognize InstructorID as the primary key of this entity because its name doesn't follow the ID or classnameID naming convention. Therefore, the Key attribute is used to identify it as the key:

```
[Key]
[ForeignKey("Instructor")]
public int InstructorID { get; set; }
```

You can also use the Key attribute if the entity does have its own primary key but you want to name the property something different than classnameID or ID. By default EF treats the key as non-database-generated because the column is for an identifying relationship.

### The ForeignKey Attribute

When there is a  one-to-zero-or-one relationship or a  one-to-one relationship between two entities (such as between OfficeAssignment and Instructor), EF can't work out which end of the relationship is the principal and which end is dependent.  One-to-one relationships have a reference navigation property in each class to the other class. The ForeignKey Attribute can be applied to the dependent class to establish the relationship. todo does this still happen If you omit the ForeignKey Attribute, you get the following error when you try to create the migration:

Unable to determine the principal end of an association between the types 'ContosoUniversity.Models.OfficeAssignment' and 'ContosoUniversity.Models.Instructor'. The principal end of this association must be explicitly configured using either the relationship fluent API or data annotations.

todo are we still going to do that Later in the tutorial you'll see how to configure this relationship with the fluent API.

### The Instructor Navigation Property

The Instructor entity has a nullable OfficeAssignment navigation property (because an instructor might not have an office assignment), and the OfficeAssignment entity has a non-nullable Instructor navigation property (because an office assignment can't exist without an instructor -- InstructorID is non-nullable). When an Instructor entity has a related OfficeAssignment entity, each entity will have a reference to the other one in its navigation property.

You could put a [Required] attribute on the Instructor navigation property to specify that there must be a related instructor, but you don't have to do that because the InstructorID foreign key (which is also the key to this table) is non-nullable.

## Modify the Course Entity

![Course entity diagram](todo)

In Models\Course.cs, replace the code you added earlier with the following code:

```
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
    public class Course
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Number")]
        public int CourseID { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string Title { get; set; }

        [Range(0, 5)]
        public int Credits { get; set; }

        public int DepartmentID { get; set; }

        public Department Department { get; set; }
        public  ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<CourseInstructor> Instructors { get; set; }
    }
}
```

The course entity has a foreign key property DepartmentID which points to the related Department entity and it has a Department navigation property. The Entity Framework doesn't require you to add a foreign key property to your data model when you have a navigation property for a related entity.  EF automatically creates foreign keys in the database wherever they are needed and creates shadow properties todo add link for them. todo https://ef.readthedocs.io/en/latest/modeling/shadow-properties.html

But having the foreign key in the data model can make updates simpler and more efficient. For example, when you fetch a course entity to edit, the  Department entity is null if you don't load it, so when you update the course entity, you would have to first fetch the Department entity. When the foreign key property DepartmentID is included in the data model, you don't need to fetch the Department entity before you update.

### The DatabaseGenerated Attribute

The DatabaseGenerated attribute with the None parameter on the CourseID property specifies that primary key values are provided by the user rather than generated by the database.

```
[DatabaseGenerated(DatabaseGeneratedOption.None)]
[Display(Name = "Number")]
public int CourseID { get; set; }
```

By default, the Entity Framework assumes that primary key values are generated by the database. That's what you want in most scenarios. However, for Course entities, you'll use a user-specified course number such as a 1000 series for one department, a 2000 series for another department, and so on.

DBGEN can be used for default values
		? For insert date use [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		? For update date use [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		? Fluent api names are better - ValueGeneratedOnAddOrUpdate
https://ef.readthedocs.io/en/latest/modeling/generated-properties.html


### Foreign Key and Navigation Properties

The foreign key properties and navigation properties in the Course entity reflect the following relationships:

A course is assigned to one department, so there's a DepartmentID foreign key and a Department navigation property for the reasons mentioned above.

```
public int DepartmentID { get; set; }
public Department Department { get; set; }
```

A course can have any number of students enrolled in it, so the Enrollments navigation property is a collection:

```
public ICollection<Enrollment> Enrollments { get; set; }
```

A course may be taught by multiple instructors, so the Instructors navigation property is a collection:

```
public ICollection<Instructor> Instructors { get; set; }
```

## Create the Department Entity

![Department entity diagram](todo)

Create Models\Department.cs with the following code:

```
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
    public class Department
    {
        public int DepartmentID { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal Budget { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        public int? InstructorID { get; set; }

        public virtual Instructor Administrator { get; set; }
        public virtual ICollection<Course> Courses { get; set; }
    }
}
```

### The Column Attribute

Earlier you used the Column attribute to change column name mapping. In the code for the Department entity, the Column attribute is being used to change SQL data type mapping so that the column will be defined using the SQL Server money type in the database:

```
[Column(TypeName="money")]
public decimal Budget { get; set; }
```

Column mapping is generally not required, because the Entity Framework usually chooses the appropriate SQL Server data type based on the CLR type that you define for the property. The CLR decimal type maps to a SQL Server decimal type. But in this case you know that the column will be holding currency amounts, and the money data type is more appropriate for that. For more information about CLR data types and how they match to SQL Server data types, see SqlClient for Entity FrameworkTypes.todo is this still current

### Foreign Key and Navigation Properties

The foreign key and navigation properties reflect the following relationships:

A department may or may not have an administrator, and an administrator is always an instructor. Therefore the InstructorID property is included as the foreign key to the Instructor entity, and a question mark is added after the int type designation to mark the property as nullable. The navigation property is named Administrator but holds an Instructor entity:

```
public int? InstructorID { get; set; }
public virtual Instructor Administrator { get; set; }
```

A department may have many courses, so there's a Courses navigation property:

```
public ICollection<Course> Courses { get; set; }
```

Note By convention, the Entity Framework enables cascade delete for non-nullable foreign keys and for many-to-many relationships. This can result in circular cascade delete rules, which will cause an exception when you try to add a migration. For example, if you didn't define the Department.InstructorID property as nullable, you'd get the following exception message: todo check this out"The referential relationship will result in a cyclical reference that's not allowed." If your business rules required InstructorID property to be non-nullable, you would have to use the following fluent API statement to disable cascade delete on the relationship:

``` 
todo verify this code
modelBuilder.Entity().HasRequired(d => d.Administrator).WithMany().WillCascadeOnDelete(false);
Modify the Enrollment Entity
```

## Enrollment_entity

In Models\Enrollment.cs, replace the code you added earlier with the following code

```
using System.ComponentModel.DataAnnotations;

namespace ContosoUniversity.Models
{
    public enum Grade
    {
        A, B, C, D, F
    }

    public class Enrollment
    {
        public int EnrollmentID { get; set; }
        public int CourseID { get; set; }
        public int StudentID { get; set; }
        [DisplayFormat(NullDisplayText = "No grade")]
        public Grade? Grade { get; set; }

        public virtual Course Course { get; set; }
        public virtual Student Student { get; set; }
    }
}
```

### Foreign Key and Navigation Properties

The foreign key properties and navigation properties reflect the following relationships:

An enrollment record is for a single course, so there's a CourseID foreign key property and a Course navigation property:

```
public int CourseID { get; set; }
public Course Course { get; set; }
```

An enrollment record is for a single student, so there's a StudentID foreign key property and a Student navigation property:

```
public int StudentID { get; set; }
public Student Student { get; set; }
```

## Many-to-Many Relationships

There's a many-to-many relationship between the Student and Course entities, and the Enrollment entity functions as a many-to-many join table with payload in the database. This means that the Enrollment table contains additional data besides foreign keys for the joined tables (in this case, a primary key and a Grade property).

The following illustration shows what these relationships look like in an entity diagram. (This diagram was generated using the Entity Framework Power Tools for EF 6.x; creating the diagram isn't part of the tutorial, it's just being used here as an illustration.)

![Student-Course many to many relationship](todo)

Each relationship line has a 1 at one end and an asterisk (*) at the other, indicating a one-to-many relationship.

If the Enrollment table didn't include grade information, it would only need to contain the two foreign keys CourseID and StudentID. In that case, it would correspond to a many-to-many join table without payload (or a pure join table) in the database. The Instructor and Course entities have that kind of many-to-many relationship, and your next step is to create an entity class to function as a join table without payload. 

## The CourseInstructor entity

A join table is required in the database for the Instructor-Courses many-to-many relationship, as shown in the following database diagram:

![Instructor-Course many to many relationship](todo)

![Instructor-Course many to many relationship](todo)

To create an entity for this taable, create Models\CourseInstructor.cs, and replace the template code with the following code.

```
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
    public class CourseInstructor
    {
        public int InstructorID { get; set; }
        public int CourseID { get; set; }
        public Instructor Instructor { get; set; }
        public Course Course { get; set; }
    }
}
```

To specify that the InstructorID and CourseID properties function as a composite primary key, add the following code to Data/SchoolContext.cs, immediately after the last DbSet property.

```
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CourseInstructor>()
                .HasKey(c => new { c.CourseID, c.InstructorID });
        }
```

You can't specify composite primary keys by using data annotations attributes.

## Entity Diagram Showing Relationships

The following illustration shows the diagram that the Entity Framework Power Tools create for the completed School model. todo doesn't show CourseInstructor entity

![Data model diagram](todo)

Besides the many-to-many relationship lines (* to *) and the one-to-many relationship lines (1 to *), you can see here the one-to-zero-or-one relationship line (1 to 0..1) between the Instructor and OfficeAssignment entities and the zero-or-one-to-many relationship line (0..1 to *) between the Instructor and Department entities.

## Customize the Data Model by adding Code to the Database Context

Next you'll add the new entities to the SchoolContext class and customize some of the mapping using fluent API calls. The API is "fluent" because it's often used by stringing a series of method calls together into a single statement, as in the following example:

```
todo new example code and link to EF core doc on fluent API
```

In this tutorial you're usinf the fluent API only for database mapping that you can't do with attributes. (Specifically, you used it to specify a composite key for the Instructor-t-Course many-to-many mapping entity.) However, you can also use the fluent API to specify most of the formatting, validation, and mapping rules that you can do by using attributes. Some attributes such as MinimumLength can't be applied with the fluent API.todo is this still true As mentioned previously, MinimumLength doesn't change the schema, it only applies a client and server side validation rule.

Some developers prefer to use the fluent API exclusively so that they can keep their entity classes "clean." You can mix attributes and fluent API if you want, and there are a few customizations that can only be done by using fluent API, but in general the recommended practice is to choose one of these two approaches and use that consistently as much as possible. If you do use both, note that wherever there is a conflict, Fluent API overrides attributes.



To add the new entities to the data model, add the following DbSet properties to Data\SchoolContext.cs:

```
public DbSet<Department> Departments { get; set; }
public DbSet<Instructor> Instructors { get; set; }
public DbSet<OfficeAssignment> OfficeAssignments { get; set; }
public DbSet<CourseInstructor> CourseInstructors { get; set; }
```

## Seed the Database with Test Data

Replace the code in the Data\DbInitializer.cs file with the following code in order to provide seed data for the new entities you've created.

```
dbinitializer code
```

As you saw in the first tutorial, most of this code simply updates or creates new entity objects and loads sample data into properties as required for testing. However, notice how the many-to-many relationship between the Course entity and the Instructor entity is handled:

```
var courseInstructors = new List<CourseInstructor>
{
    new CourseInstructor {
        CourseID = courses.Single(c => c.Title == "Chemistry" ).CourseID,
        InstructorID = instructors.Single(i => i.LastName == "Kapoor").ID
        },
        // other lines like this one have been omitted here
};
courseInstructors.ForEach(c => context.CourseInstructors.Add(c));
context.SaveChanges();
```

## Add a Migration and Update the Database

From the PMC, enter the add-migration command (don't do the update-database command yet):

```
add-Migration ComplexDataModel
```

If you tried to run the update-database command at this point (don't do it yet), you would get the following error:

todo is this still currrent
The ALTER TABLE statement conflicted with the FOREIGN KEY constraint "FK_dbo.Course_dbo.Department_DepartmentID". The conflict occurred in database "ContosoUniversity", table "dbo.Department", column 'DepartmentID'.

Sometimes when you execute migrations with existing data, you need to insert stub data into the database to satisfy foreign key constraints, and that's what you have to do now. The generated code in the ComplexDataModel Up method adds a non-nullable DepartmentID foreign key to the Course table. Because there are already rows in the Course table when the code runs, the AddColumn operation will fail because SQL Server doesn't know what value to put in the column that can't be null. Therefore have to change the code to give the new column a default value, and create a stub department named "Temp" to act as the default department. As a result, existing Course rows will all be related to the "Temp" department after the Up method runs.  You can relate them to the correct departments in the Seed method.

Edit the `<timestamp>_ComplexDataModel.cs` file, comment out the line of code that adds the DepartmentID column to the Course table, and add before it the following highlighted code (the commented line is also highlighted):

```
           migrationBuilder.Sql("INSERT INTO dbo.Departments (Name, Budget, StartDate) VALUES ('Temp', 0.00, GETDATE())");
            // Default value for FK points to department created above, with
            // defaultValue changed to 1 in following AddColumn statement.

            migrationBuilder.AddColumn<int>(
                name: "DepartmentID",
                table: "Courses",
                nullable: false,
                defaultValue: 1);
```

When the DbInitializer.Initialize method runs, it will insert rows in the Department table and it will relate existing Course rows to those new Department rows. You will then no longer need the "Temp" department or the default value on the Course.DepartmentID column.

After you have finished editing the `<timestamp>_ComplexDataModel.cs` file, build the project, and then enter the update-database command in the PMC to execute the migration.

```
update-database
```

todo start sidebar
Note: It's possible to get other errors when migrating data and making schema changes. If you get migration errors you can't resolve, you can either change the database name in the connection string or delete the database. The simplest approach is to rename the database in appsettings.json. The following example shows the name changed to ContosoUniversity3:

```
   "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=aspnet-ContosoUniversity3;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
```

With a new database, there is no data to migrate, and the update-database command is much more likely to complete without errors. For instructions on how to delete the database, see How to Drop a Database from Visual Studio 2012.todo update link

If that fails, another thing you can try is re-initialize the database by entering the following command in the PMC:

todo is this still valid
```
update-database -TargetMigration:0
```

todo end of sidebar

Run the app to cause the DbInitializer.Initialize method to run and populate the new database.

Open the database in SSOX as you did earlier, and expand the Tables node to see that all of the tables have been created. (If you still have Server Explorer open from the earlier time, click the Refresh button.)

![Tables in SSOX](todo)

Right-click the CourseInstructors table and select Show Table Data to verify that it has data in it.

![CourseInstructors in SSOX](todo)

## Summary

You now have a more complex data model and corresponding database. In the following tutorial you'll learn more about different ways to access related data.

