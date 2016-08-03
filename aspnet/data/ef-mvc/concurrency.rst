Handling concurrency conflicts
==============================

The Contoso University sample web application demonstrates how to create ASP.NET Core 1.0 MVC web applications using Entity Framework Core 1.0 and Visual Studio 2015. For information about the tutorial series, see [the first tutorial in the series](todo).

In earlier tutorials you learned how to update data. This tutorial shows how to handle conflicts when multiple users update the same entity at the same time.

You'll change the web pages that work with the Department entity so that they handle concurrency errors. The following illustrations show the Index and Delete pages, including some messages that are displayed if a concurrency conflict occurs.

![Department Index page]()

![Department Edit page]()

## Concurrency Conflicts

A concurrency conflict occurs when one user displays an entity's data in order to edit it, and then another user updates the same entity's data before the first user's change is written to the database. If you don't enable the detection of such conflicts, whoever updates the database last overwrites the other user's changes. In many applications, this risk is acceptable: if there are few users, or few updates, or if isn't really critical if some changes are overwritten, the cost of programming for concurrency might outweigh the benefit. In that case, you don't have to configure the application to handle concurrency conflicts.

### Pessimistic Concurrency (Locking)

If your application does need to prevent accidental data loss in concurrency scenarios, one way to do that is to use database locks. This is called pessimistic concurrency. For example, before you read a row from a database, you request a lock for read-only or for update access. If you lock a row for update access, no other users are allowed to lock the row either for read-only or update access, because they would get a copy of data that's in the process of being changed. If you lock a row for read-only access, others can also lock it for read-only access but not for update.

Managing locks has disadvantages. It can be complex to program. It requires significant database management resources, and it can cause performance problems as the number of users of an application increases. For these reasons, not all database management systems support pessimistic concurrency. The Entity Framework provides no built-in support for it, and this tutorial doesn't show you how to implement it.

### Optimistic Concurrency

The alternative to pessimistic concurrency is optimistic concurrency. Optimistic concurrency means allowing concurrency conflicts to happen, and then reacting appropriately if they do. For example, John runs the Departments Edit page, changes the Budget amount for the English department from $350,000.00 to $0.00.

![Changing budget to 0]()

Before John clicks Save, Jane runs the same page and changes the Start Date field from 9/1/2007 to 8/8/2013.

![Changing start date to 2013]()

John clicks Save first and sees his change when the browser returns to the Index page, then Jane clicks Save. What happens next is determined by how you handle concurrency conflicts. Some of the options include the following:

* You can keep track of which property a user has modified and update only the corresponding columns in the database. In the example scenario, no data would be lost, because different properties were updated by the two users. The next time someone browses the English department, they'll see both John's and Jane's changes — a start date of 8/8/2013 and a budget of Zero dollars.

	This method of updating can reduce the number of conflicts that could result in data loss, but it can't avoid data loss if competing changes are made to the same property of an entity. Whether the Entity Framework works this way depends on how you implement your update code. todo, with read first edit is this still valid? It's often not practical in a web application, because it can require that you maintain large amounts of state in order to keep track of all original property values for an entity as well as new values. Maintaining large amounts of state can affect application performance because it either requires server resources or must be included in the web page itself (for example, in hidden fields) or in a cookie.

* You can let Jane's change overwrite John's change. The next time someone browses the English department, they'll see 8/8/2013 and the restored $350,000.00 value. This is called a Client Wins or Last in Wins scenario. (All values from the client take precedence over what's in the data store.) As noted in the introduction to this section, if you don't do any coding for concurrency handling, this will happen automatically.

* You can prevent Jane's change from being updated in the database. Typically, you would display an error message, show her the current state of the data, and allow her to reapply her changes if she still wants to make them. This is called a Store Wins scenario. (The data-store values take precedence over the values submitted by the client.) You'll implement the Store Wins scenario in this tutorial. This method ensures that no changes are overwritten without a user being alerted to what's happening.

### Detecting Concurrency Conflicts

You can resolve conflicts by handling DbConcurrencyException exceptions that the Entity Framework throws. In order to know when to throw these exceptions, the Entity Framework must be able to detect conflicts. Therefore, you must configure the database and the data model appropriately. Some options for enabling conflict detection include the following:

* In the database table, include a tracking column that can be used to determine when a row has been changed. You can then configure the Entity Framework to include that column in the Where clause of SQL Update or Delete commands.

	The data type of the tracking column is typically rowversion. The rowversion value is a sequential number that's incremented each time the row is updated. In an Update or Delete command, the Where clause includes the original value of the tracking column (the original row version) . If the row being updated has been changed by another user, the value in the rowversion column is different than the original value, so the Update or Delete statement can't find the row to update because of the Where clause. When the Entity Framework finds that no rows have been updated by the Update or Delete command (that is, when the number of affected rows is zero), it interprets that as a concurrency conflict.

* Configure the Entity Framework to include the original values of every column in the table in the Where clause of Update and Delete commands.

	As in the first option, if anything in the row has changed since the row was first read, the Where clause won't return a row to update, which the Entity Framework interprets as a concurrency conflict. For database tables that have many columns, this approach can result in very large Where clauses, and can require that you maintain large amounts of state. As noted earlier, maintaining large amounts of state can affect application performance. Therefore this approach is generally not recommended, and it isn't the method used in this tutorial.

	If you do want to implement this approach to concurrency, you have to mark all non-primary-key properties in the entity you want to track concurrency for by adding the ConcurrencyCheck attribute to them. todo is this still accurate? That change enables the Entity Framework to include all columns in the SQL WHERE clause of UPDATE statements.

In the remainder of this tutorial you'll add a rowversion tracking property to the Department entity, create a controller and views, and test to verify that everything works correctly.

## Add a tracking property to the Department entity

In Models\Department.cs, add a tracking property named RowVersion:

```
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

    [Display(Name = "Administrator")]
    public int? InstructorID { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }

    public Instructor Administrator { get; set; }
    public ICollection<Course> Courses { get; set; }
}
```

The Timestamp attribute specifies that this column will be included in the Where clause of Update and Delete commands sent to the database. The attribute is called Timestamp because previous versions of SQL Server used a SQL timestamp data type before the SQL rowversion replaced it. The .Net type for rowversion is a byte array. 

If you prefer to use the fluent API, you can use the IsConcurrencyToken method to specify the tracking property, as shown in the following example:

```
modelBuilder.Entity<Department>()
    .Property(p => p.RowVersion).IsConcurrencyToken();
```

By adding a property you changed the database model, so you need to do another migration. In the Package Manager Console (PMC), enter the following commands:

```
Use-DbContext SchoolContext
Add-Migration RowVersion 
Update-Database
```

## Create a Department Controller and Views

Scaffold a Department controller and views as you did earlier for Students, Courses, and Instructors.

![Scaffold Department](todo)

In Controllers\DepartmentController.cs, add a using statement:

using System.Data.Entity.Infrastructure;

In the DepartmentController.cs file, change all four occurrences of "FirstMidName" to "FullName" so that the department administrator drop-down lists will contain the full name of the instructor rather than just the last name.

```
ViewData["InstructorID"] = new SelectList(_context.Instructors, "ID", "FullName", department.InstructorID);
```

In the HttpGet Edit method, do eager loading for the Administrator navigation property.

```
var department = await _context.Departments.Include(i => i.Administrator).SingleOrDefaultAsync(m => m.DepartmentID == id);
```

Replace the existing code for the HttpPost Edit method with the following code:

```
todo add final code
```

If the SingleOrDefaultAsync method returns null, the department was deleted by another user. The code shown uses the posted form values to create a department entity so that the Edit page can be redisplayed with an error message. As an alternative, you wouldn't have to re-create the department entity if you display only an error message without redisplaying the department fields.

The view stores the original RowVersion value in a hidden field, and the method receives it in the rowVersion parameter. Before you call SaveChanges, you have to put that original RowVersion property value in the OriginalValues collection for the entity. Then when the Entity Framework creates a SQL UPDATE command, that command will include a WHERE clause that looks for a row that has the original RowVersion value.

If no rows are affected by the UPDATE command (no rows have the original RowVersion value),  the Entity Framework throws a DbUpdateConcurrencyException exception, and the code in the catch block gets the affected Department entity from the exception object.

```
var exceptionEntry = ex.Entries.Single();
```

This object has the new values entered by the user in its Entity property, and you can get the current database values by using a no-tracking query. todo explain no tracking

```
                    var databaseEntity = _context.Departments.AsNoTracking().Single(d => d.DepartmentID == ((Department)exceptionEntry.Entity).DepartmentID);
                    var databaseEntry = _context.Entry(databaseEntity);

```

This code runs a query for the affected department. Because you already checked for deletion, the Single method rather than SingleOrDefault is used here.

Next, the code adds a custom error message for each column that has database values different from what the user entered on the Edit page.

```
todo add code
```

Finally, the code sets the RowVersion value of the Department object to the new value retrieved from the database. This new RowVersion value will be stored in the hidden field when the Edit page is redisplayed, and the next time the user clicks Save, only concurrency errors that happen since the redisplay of the Edit page will be caught. The ModelState.Remove statement is required because ModelState has the old RowVersion value, and in values passed to the View ModelState values take precedence over the model property values.

todo clean up create Bind attribute - remove RV, ID

In Views\Department\Edit.cshtml, make the following changes:

* Convert the self-closing validation span elements to use closing tags.
* Remove the form-group div that was scaffolded for the RowVersion field.
* Add a hidden field to save the RowVersion property value, immediately following the hidden field for the DepartmentID property.
* Add a "Select Administrator" option to the drop-downlist.

```
@model ContosoUniversity.Models.Department

@{
    ViewData["Title"] = "Edit";
}

<h2>Edit</h2>

<form asp-action="Edit">
    <div class="form-horizontal">
        <h4>Department</h4>
        <hr />
        <div asp-validation-summary="ModelOnly" class="text-danger"></div>
        <input type="hidden" asp-for="DepartmentID" />
        <input type="hidden" asp-for="RowVersion" />
        <div class="form-group">
            <label asp-for="Budget" class="col-md-2 control-label"></label>
            <div class="col-md-10">
                <input asp-for="Budget" class="form-control" />
                <span asp-validation-for="Budget" class="text-danger"></span>
            </div>
        </div>
        <div class="form-group">
            <label asp-for="InstructorID" class="control-label col-md-2">Administrator</label>
            <div class="col-md-10">
                <select asp-for="InstructorID" class="form-control" asp-items="ViewBag.InstructorID">
                    <option value="">-- Select Administrator --</option>
                </select>
                <span asp-validation-for="InstructorID" class="text-danger"></span>
            </div>
        </div>
        <div class="form-group">
            <label asp-for="Name" class="col-md-2 control-label"></label>
            <div class="col-md-10">
                <input asp-for="Name" class="form-control" />
                <span asp-validation-for="Name" class="text-danger"></span>
            </div>
        </div>
        <div class="form-group">
            <label asp-for="StartDate" class="col-md-2 control-label"></label>
            <div class="col-md-10">
                <input asp-for="StartDate" class="form-control" />
                <span asp-validation-for="StartDate" class="text-danger"></span>
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-2 col-md-10">
                <input type="submit" value="Save" class="btn btn-default" />
            </div>
        </div>
    </div>
</form>

<div>
    <a asp-action="Index">Back to List</a>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```


## Testing Optimistic Concurrency Handling

Run the site and click Departments:

![Department Index page before edits](todo)

Right click the Edit hyperlink for the English department and select Open in new tab, then click the Edit hyperlink for the English department. The two tabs display the same information.

![Department Edit page before edits](todo)

Change a field in the first browser tab and click Save.

![Department Edit page 1 after change](todo)

The browser shows the Index page with the changed value.

![Departments Index page after first edit](todo)

Change a field  in the second browser tab and click Save.

![Department Edit page 2 after change](todo)

Click Save in the second browser tab. You see an error message:

![Department Edit page with concurrency error message](todo)

Click Save again. The value you entered in the second browser tab is saved along with the original value of the data you changed in the first browser. You see the saved values when the Index page appears.

![Departments Index page after second edit](todo)

## Updating the Delete Page

For the Delete page, the Entity Framework detects concurrency conflicts caused by someone else editing the department in a similar manner. When the HttpGet Delete method displays the confirmation view, the view includes the original RowVersion value in a hidden field. That value is then available to the HttpPost Delete method that's called when the user confirms the deletion. When the Entity Framework creates the SQL DELETE command, it includes a WHERE clause with the original RowVersion value. If the command results in zero rows affected (meaning the row was changed after the Delete confirmation page was displayed), a concurrency exception is thrown, and the HttpGet Delete method is called with an error flag set to true in order to redisplay the confirmation page with an error message. It's also possible that zero rows were affected because the row was deleted by another user, so in that case no error message is displayed.

In DepartmentController.cs, replace the HttpGet Delete method with the following code:

```
        public async Task<IActionResult> Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments.SingleOrDefaultAsync(m => m.DepartmentID == id);
            if (department == null)
            {
                if (concurrencyError.GetValueOrDefault())
                {
                    return RedirectToAction("Index");
                }
                return NotFound();
            }

            if (concurrencyError.GetValueOrDefault())
            {
                ViewBag.ConcurrencyErrorMessage = "The record you attempted to delete "
                    + "was modified by another user after you got the original values. "
                    + "The delete operation was canceled and the current values in the "
                    + "database have been displayed. If you still want to delete this "
                    + "record, click the Delete button again. Otherwise "
                    + "click the Back to List hyperlink.";
            }

            return View(department);
        }
```

The method accepts an optional parameter that indicates whether the page is being redisplayed after a concurrency error. If this flag is true, an error message is sent to the view using ViewData.

Replace the code in the HttpPost Delete method (named DeleteConfirmed) with the following code:

```
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Department department)
        {
            try
            {
                //department = await _context.Departments.SingleOrDefaultAsync(m => m.DepartmentID == id);
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return RedirectToAction("Delete", new { concurrencyError = true, id = department.DepartmentID });
            }
        }
```

In the scaffolded code that you just replaced, this method accepted only a record ID:

```
        public async Task<IActionResult> DeleteConfirmed(int id)
```

You've changed this parameter to a Department entity instance created by the model binder. This gives you access to the RowVersion property value in addition to the record key.

```
        public async Task<IActionResult> Delete(Department department)
```

You have also changed the action method name from DeleteConfirmed to Delete. The scaffolded code named the HttpPost Delete method DeleteConfirmed to give the HttpPost  method a unique signature. ( The CLR requires overloaded methods to have different method parameters.) Now that the signatures are unique, you can stick with the MVC convention and use the same name for the HttpPost and HttpGet delete methods.

If a concurrency error is caught, the code redisplays the Delete confirmation page and provides a flag that indicates it should display a concurrency error message.

In Views\Department\Delete.cshtml, replace the scaffolded code with the following code that adds an error message field and hidden fields for the DepartmentID and RowVersion properties. The changes are highlighted.

```
@model ContosoUniversity.Models.Department

@{
    ViewData["Title"] = "Delete";
}

<h2>Delete</h2>

<p class="text-danger">@ViewBag.ConcurrencyErrorMessage</p>

<h3>Are you sure you want to delete this?</h3>
<div>
    <h4>Department</h4>
    <hr />
    <dl class="dl-horizontal">
        <dt>
            @Html.DisplayNameFor(model => model.Budget)
        </dt>
        <dd>
            @Html.DisplayFor(model => model.Budget)
        </dd>
        <dt>
            @Html.DisplayNameFor(model => model.Name)
        </dt>
        <dd>
            @Html.DisplayFor(model => model.Name)
        </dd>
        <dt>
            @Html.DisplayNameFor(model => model.Administrator)
        </dt>
        <dd>
            @Html.DisplayFor(model => model.Administrator.FullName)
        </dd>
        <dt>
            @Html.DisplayNameFor(model => model.StartDate)
        </dt>
        <dd>
            @Html.DisplayFor(model => model.StartDate)
        </dd>
    </dl>
    
    <form asp-action="Delete">
        <input type="hidden" asp-for="DepartmentID" />
        <input type="hidden" asp-for="RowVersion" />
        <div class="form-actions no-color">
            <input type="submit" value="Delete" class="btn btn-default" /> |
            <a asp-action="Index">Back to List</a>
        </div>

    </form>
</div>
```

This code adds an error message between the h2 and h3 headings:

```
<p class="error">@ViewBag["ConcurrencyErrorMessage"]</p>
```

It replaces LastName with FullName in the Administrator field:

```
<dt>
  Administrator
</dt>
<dd>
  @Html.DisplayFor(model => model.Administrator.FullName)
</dd>
```

Finally, it adds hidden fields for the DepartmentID and RowVersion properties after the Html.BeginForm statement:

```
@Html.HiddenFor(model => model.DepartmentID)
@Html.HiddenFor(model => model.RowVersion)
```

Run the Departments Index page. Right click the Delete hyperlink for the English department and select Open in new tab, then in the first tab click the Edit hyperlink for the English department.

In the first window, change one of the values, and click Save: 

![Department Edit page after change before delete](todo)

The Index page confirms the change.

![Departments Index page after budget edit before delete](todo)

In the second tab, click Delete.

![Department Delete confirmation page before concurrency error](todo)

 You see the concurrency error message, and the Department values are refreshed with what's currently in the database.

![Department Delete confirmation page with concurrency error](todo)

If you click Delete again, you're redirected to the Index page, which shows that the department has been deleted.

todoChange RowVersion to Administrator in index, details
Add select option in Create

## Summary

This completes the introduction to handling concurrency conflicts. For information about other ways to handle various concurrency scenarios, see todo link to EF Core versionOptimistic Concurrency Patterns and Working with Property Values on MSDN. The next tutorial shows how to implement table-per-hierarchy inheritance for the Instructor and Student entities.