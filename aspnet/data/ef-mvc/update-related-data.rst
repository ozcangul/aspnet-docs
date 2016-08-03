Updating related data
=====================

The Contoso University sample web application demonstrates how to create ASP.NET Core 1.0 MVC web applications using Entity Framework Core 1.0 and Visual Studio 2015. For information about the tutorial series, see [the first tutorial in the series](todo).

In the previous tutorial you displayed related data; in this tutorial you'll update related data. This can be done by updating either foreign key fields or navigation properties.

The following illustrations show some of the pages that you'll work with.

![Course Create page](todo)

![Course Edit page](todo)

![Instructor Edit page](todo)

## Customize the Create and Edit Pages for Courses

When a new course entity is created, it must have a relationship to an existing department. To facilitate this, the scaffolded code includes controller methods and Create and Edit views that include a drop-down list for selecting the department. The drop-down list sets the Course.DepartmentID foreign key property, and that's all the Entity Framework needs in order to load the Department navigation property with the appropriate Department entity. You'll use the scaffolded code, but change it slightly to add error handling and sort the drop-down list.

In CourseController.cs, delete the four Create and Edit methods and replace them with the following code:

```
        // GET: Courses/Create
        public IActionResult Create()
        {
            PopulateDepartmentsDropDownList();
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseID,Credits,DepartmentID,Title")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            PopulateDepartmentsDropDownList(course.DepartmentID);
            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.SingleOrDefaultAsync(m => m.CourseID == id);
            if (course == null)
            {
                return NotFound();
            }
            PopulateDepartmentsDropDownList(course.DepartmentID);
            return View(course);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseToUpdate = await _context.Courses.SingleOrDefaultAsync(c => c.CourseID == id);
            if (await TryUpdateModelAsync<Course>(courseToUpdate,
                "",
                c => c.Credits, c => c.DepartmentID, c => c.Title))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException /* dex */)
                {
                    //Log the error (uncomment dex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }
            PopulateDepartmentsDropDownList(courseToUpdate.DepartmentID);
            return View(courseToUpdate);
        }

        private void PopulateDepartmentsDropDownList(object selectedDepartment = null)
        {
            var departmentsQuery = from d in _context.Departments
                                   orderby d.Name
                                   select d;
            ViewData["DepartmentID"] = new SelectList(departmentsQuery, "DepartmentID", "Name", selectedDepartment);
        }
```


The PopulateDepartmentsDropDownList method gets a list of all departments sorted by name, creates a SelectList collection for a drop-down list, and passes the collection to the view in ViewData. The method accepts the optional selectedDepartment parameter that allows the calling code to specify the item that will be selected when the drop-down list is rendered. The view will pass the name DepartmentID to the DropDownList helper, and the helper then knows to look in the ViewData object for a SelectList named DepartmentID.

The HttpGet Create method calls the PopulateDepartmentsDropDownList method without setting the selected item, because for a new course the department is not established yet:

```
public IActionResult Create()
{
    PopulateDepartmentsDropDownList();
    return View();
}
```

The HttpGet Edit method sets the selected item, based on the ID of the department that is already assigned to the course being edited:

```
PopulateDepartmentsDropDownList(course.DepartmentID);
```

The HttpPost methods for both Create and Edit also include code that sets the selected item when they redisplay the page after an error. This ensures that when the page is redisplayed to show the error message, whatever department was selected stays selected.

Change the code that reads a course in the Details and HttpGet Delete methods so that it loads the Department navigation property:

```
var course = await _context.Courses.Include(c => c.DepartmentID).SingleOrDefaultAsync(m => m.CourseID == id);
```

The Course views are already scaffolded with drop-down lists for the department field, but you don't want the DepartmentID caption for this field, so make the following highlighted change to the Views\Course\Create.cshtml and Views\Course\Edit.cshtml files to change the caption.

```
            <label asp-for="Department" class="control-label col-md-2"></label>
            <div class="col-md-10">
                <select asp-for="DepartmentID" class="form-control" asp-items="ViewBag.DepartmentID"></select>
                <span asp-validation-for="DepartmentID" class="text-danger" />
            </div>
```

The scaffolder doesn't scaffold a primary key because typically the key value is generated by the database and can't be changed and isn't a meaningful value to be displayed to users. For Course entities you do need a text box in the Create view for the CourseID field because the DatabaseGeneratedOption.None attribute means the user should be able enter the primary key value.

In Views\Course\Create.cshtml, add a field for the course ID before the Credits field:

```
        <div class="form-group">
            <label asp-for="CourseID" class="col-md-2 control-label"></label>
            <div class="col-md-10">
                <input asp-for="CourseID" class="form-control" />
                <span asp-validation-for="CourseID" class="text-danger" />
            </div>
        </div>
```

Also in Create.cshtml, add a "Select Department" option to the Department drop-down list. 

```
        <div class="form-group">
            <label asp-for="Department" class="col-md-2 control-label"></label>
            <div class="col-md-10">
                <select asp-for="DepartmentID" class ="form-control" asp-items="ViewBag.DepartmentID">
                    <option value="">-- Select Department --</option>
                </select>
            </div>
        </div>

```

In Views\Course\Edit.cshtml, add a course number field before the Title field. Because it's the primary key, it's displayed, but it can't be changed.

```
        <div class="form-group">
            <label asp-for="CourseID" class="col-md-2 control-label"></label>
            <div class="col-md-10">
                @Html.DisplayFor(model => model.Credits)
            </div>
        </div>
```

There's already a hidden field (Html.HiddenFor helper) for the course number in the Edit view. Adding an Html.LabelFor helper doesn't eliminate the need for the hidden field because it doesn't cause the course number to be included in the posted data when the user clicks Save on the Edit page.

In Views\Course\Delete.cshtml and Views\Course\Details.cshtml, add a course number field at the top and a department name field before the title field.

```
        <dt>
            @Html.DisplayNameFor(model => model.CourseID)
        </dt>
        <dd>
            @Html.DisplayFor(model => model.CourseID)
        </dd>
        <dt>
            @Html.DisplayNameFor(model => model.Credits)
        </dt>
        <dd>
            @Html.DisplayFor(model => model.Credits)
        </dd>
        <dt>
            @Html.DisplayNameFor(model => model.Department)
        </dt>
        <dd>
            @Html.DisplayFor(model => model.Department.Name)
        </dd>
                <dt>
            @Html.DisplayNameFor(model => model.Title)
        </dt>
        <dd>
            @Html.DisplayFor(model => model.Title)
        </dd>
```


Run the Create page (display the Course Index page and click Create New) and enter data for a new course:

![Course Create page](todo)

Click Create. The Course Index page is displayed with the new course added to the list. The department name in the Index page list comes from the navigation property, showing that the relationship was established correctly.

![Course Index page](todo)

Run the Edit page (display the Course Index page and click Edit on a course).

![Course Edit page](todo)

Change data on the page and click Save. The Course Index page is displayed with the updated course data.

## Adding an Edit Page for Instructors

When you edit an instructor record, you want to be able to update the instructor's office assignment. The Instructor entity has a one-to-zero-or-one relationship with the OfficeAssignment entity, which means you must handle the following situations:

* If the user clears the office assignment and it originally had a value, you must remove and delete the OfficeAssignment entity.
* If the user enters an office assignment value and it originally was empty, you must create a new OfficeAssignment entity.
* If the user changes the value of an office assignment, you must change the value in an existing OfficeAssignment entity.

In InstructorController.cs, change the code in the HttpGet Edit method so that it loads the Instructor entity's OfficeAssignment navigation property:

```
var instructor = await _context.Instructors.Include(i => i.OfficeAssignment).SingleOrDefaultAsync(m => m.ID == id);
```

Replace the HttpPost Edit method with the following code. which handles office assignment updates:

```
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructorToUpdate = await _context.Instructors.Include(i => i.OfficeAssignment).SingleOrDefaultAsync(s => s.ID == id);
            if (await TryUpdateModelAsync<Instructor>(
                instructorToUpdate,
                "",
                i => i.FirstMidName, i => i.LastName, i => i.HireDate, i => i.OfficeAssignment))
            {
                try
                {
                    if (String.IsNullOrWhiteSpace(instructorToUpdate.OfficeAssignment?.Location))
                    {
                        instructorToUpdate.OfficeAssignment = null;
                    }
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException /* dex */)
                {
                    //Log the error (uncomment dex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }
            return View(instructorToUpdate);
        }

```

The code does the following:

Changes the method name to EditPost because the signature is now the same as the HttpGet method (the ActionName attribute specifies that the /Edit/ URL is still used).

Gets the current Instructor entity from the database using eager loading for the OfficeAssignment navigation property. This is the same as what you did in the HttpGet Edit method.

Updates the retrieved Instructor entity with values from the model binder. The TryUpdateModel overload used enables you to whitelist the properties you want to include. This prevents over-posting, as explained in the second tutorial.

```
            if (await TryUpdateModelAsync<Instructor>(
                instructorToUpdate,
                "",
                i => i.FirstMidName, i => i.LastName, i => i.HireDate, i => i.OfficeAssignment))

```

If the office location is blank, sets the Instructor.OfficeAssignment property to null so that the related row in the OfficeAssignment table will be deleted.

```
                    if (String.IsNullOrWhiteSpace(instructorToUpdate.OfficeAssignment?.Location))
                    {
                        instructorToUpdate.OfficeAssignment = null;
                    }

```


Saves the changes to the database.

In Views\Instructor\Edit.cshtml, add a new field for editing the office location:

```
        <div class="form-group">
            <label asp-for="OfficeAssignment.Location" class="col-md-2 control-label"></label>
            <div class="col-md-10">
                <input asp-for="OfficeAssignment.Location" class="form-control" />
                <span asp-validation-for="OfficeAssignment.Location" class="text-danger" />
            </div>
        </div>
```

Run the page (select the Instructors tab and then click Edit on an instructor). Change the Office Location and click Save.

![Instructor Edit page](todo)

## Adding Course Assignments to the Instructor Edit Page

Instructors may teach any number of courses. Now you'll enhance the Instructor Edit page by adding the ability to change course assignments using a group of check boxes, as shown in the following screen shot:

![Instructor Edit page with courses](todo)

The relationship between the Course and Instructor entities is many-to-many. To add and remove relationships, you add and remove entities to and from the InstructorCourses join entity set.

The UI that enables you to change which courses an instructor is assigned to is a group of check boxes. A check box for every course in the database is displayed, and the ones that the instructor is currently assigned to are selected. The user can select or clear check boxes to change course assignments. If the number of courses were much greater, you would probably want to use a different method of presenting the data in the view, but you'd use the same method of manipulating navigation properties in order to create or delete relationships.

To provide data to the view for the list of check boxes, you'll use a view model class. Create AssignedCourseData.cs in the ViewModels folder and replace the existing code with the following code:

```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoUniversity.Models.SchoolViewModels
{
    public class AssignedCourseData
    {
        public int CourseID { get; set; }
        public string Title { get; set; }
        public bool Assigned { get; set; }
    }
}
```

In InstructorController.cs, replace the HttpGet Edit method with the following code. The changes are highlighted.

```
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.Courses).ThenInclude(i => i.Course)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (instructor == null)
            {
                return NotFound();
            }
            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        private void PopulateAssignedCourseData(Instructor instructor)
        {
            var allCourses = _context.Courses;
            var instructorCourses = new HashSet<int>(instructor.Courses.Select(c => c.Course.CourseID));
            var viewModel = new List<AssignedCourseData>();
            foreach (var course in allCourses)
            {
                viewModel.Add(new AssignedCourseData
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    Assigned = instructorCourses.Contains(course.CourseID)
                });
            }
            ViewData["Courses"] = viewModel;
        }
```

The code adds eager loading for the Courses navigation property and calls the new PopulateAssignedCourseData method to provide information for the check box array using the AssignedCourseData view model class.

The code in the PopulateAssignedCourseData method reads through all Course entities in order to load a list of courses using the view model class. For each course, the code checks whether the course exists in the instructor's Courses navigation property. To create efficient lookup when checking whether a course is assigned to the instructor, the courses assigned to the instructor are put into a HashSet collection. The Assigned property  is set to true for courses the instructor is assigned to. The view will use this property to determine which check boxes must be displayed as selected. Finally, the list is passed to the view in ViewData.

Next, add the code that's executed when the user clicks Save. Replace the EditPost method with the following code, which calls a new method that updates the Courses navigation property of the Instructor entity. The changes are highlighted.

```
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,FirstMidName,HireDate,LastName,OfficeAssignment")] Instructor instructor, string[] selectedCourses)
        {
            if (id != instructor.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var instructorToUpdate = await _context.Instructors
                        .Include(i => i.OfficeAssignment)
                        .Include(i => i.Courses).ThenInclude(i => i.Course)
                        .SingleOrDefaultAsync(m => m.ID == id);
                    await TryUpdateModelAsync(instructorToUpdate);
                    if (String.IsNullOrWhiteSpace(instructor.OfficeAssignment?.Location))
                    {
                        instructor.OfficeAssignment = null;
                    }
                    UpdateInstructorCourses(selectedCourses, instructorToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InstructorExists(instructor.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(instructor);
        }

        private void UpdateInstructorCourses(string[] selectedCourses, Instructor instructorToUpdate)
        {
            if (selectedCourses == null)
            {
                instructorToUpdate.Courses = new List<CourseInstructor>();
                return;
            }

            var selectedCoursesHS = new HashSet<string>(selectedCourses);
            var instructorCourses = new HashSet<int>
                (instructorToUpdate.Courses.Select(c => c.Course.CourseID));
            foreach (var course in _context.Courses)
            {
                if (selectedCoursesHS.Contains(course.CourseID.ToString()))
                {
                    if (!instructorCourses.Contains(course.CourseID))
                    {
                        instructorToUpdate.Courses.Add(new CourseInstructor { InstructorID = instructorToUpdate.ID, CourseID = course.CourseID });
                    }
                }
                else
                {

                    if (instructorCourses.Contains(course.CourseID))
                    {
                        CourseInstructor courseToRemove = instructorToUpdate.Courses.SingleOrDefault(i => i.CourseID == course.CourseID);
                        _context.Remove(courseToRemove);
                    }
                }
            }
        }

```

The method signature is now different from the HttpGet Edit method, so the method name changes from EditPost back to Edit.

Since the view doesn't have a collection of Course entities, the model binder can't automatically update the Courses navigation property. Instead of using the model binder to update the Courses navigation property, you'll do that in the new UpdateInstructorCourses method. Therefore you need to exclude the Courses property from model binding. This doesn't require any change to the code that calls TryUpdateModel because you're using the whitelisting overload and Courses isn't in the include list.

If no check boxes were selected, the code in UpdateInstructorCourses initializes the Courses navigation property with an empty collection:


```
           if (selectedCourses == null)
            {
                instructorToUpdate.Courses = new List<CourseInstructor>();
                return;
            }
```

The code then loops through all courses in the database and checks each course against the ones currently assigned to the instructor versus the ones that were selected in the view. To facilitate efficient lookups, the latter two collections are stored in HashSet objects.

If the check box for a course was selected but the course isn't in the Instructor.Courses navigation property, the course is added to the collection in the navigation property.

```
if (selectedCoursesHS.Contains(course.CourseID.ToString()))
{
    if (!instructorCourses.Contains(course.CourseID))
    {
        instructorToUpdate.Courses.Add(new CourseInstructor { InstructorID = instructorToUpdate.ID, CourseID = course.CourseID });
    }
}
```

If the check box for a course wasn't selected, but the course is in the Instructor.Courses navigation property, the course is removed from the navigation property.

```
else
{
    if (instructorCourses.Contains(course.CourseID))
    {
        CourseInstructor courseToRemove = instructorToUpdate.Courses.SingleOrDefault(i => i.CourseID == course.CourseID);
        _context.Remove(courseToRemove);
    }
}
```

In Views\Instructor\Edit.cshtml, add a Courses field with an array of check boxes by adding the following code immediately after the div elements for the OfficeAssignment field and before the div element for the Save button:

Important:  Open the file in a text editor such as Notepad to make this change.  If you use Visual Studio, line breaks will be changed in a way that breaks the code.  If that happens, you will have to manually fix everything so that it looks like what you see here. The indentation doesn't have to be perfect, but the @</tr><tr>, @:<td>, @:</td>, and @</tr> lines must each be on a single line as shown or you'll get a runtime error.

```
        <div class="form-group">
            <div class="col-md-offset-2 col-md-10">
                <table>
                    <tr>
                        @{
                            int cnt = 0;
                            List<ContosoUniversity.Models.SchoolViewModels.AssignedCourseData> courses = ViewBag.Courses;

                            foreach (var course in courses)
                            {
                                if (cnt++ % 3 == 0)
                                {
                                    @:</tr><tr>
                                }
                                @:<td>
                                    <input type="checkbox"
                                           name="selectedCourses"
                                           value="@course.CourseID"
                                           @(Html.Raw(course.Assigned ? "checked=\"checked\"" : "")) />
                                           @course.CourseID @:  @course.Title
                                @:</td>
                            }
                            @:</tr>
                        }
                    </table>
                </div>
            </div>
```

This code creates an HTML table that has three columns. In each column is a check box followed by a caption that consists of the course number and title. The check boxes all have the same name ("selectedCourses"), which informs the model binder that they are to be treated as a group. The value attribute of each check box is set to the value of CourseID. When the page is posted, the model binder passes an array to the controller that consists of the CourseID values for only the check boxes which are selected.

When the check boxes are initially rendered, those that are for courses assigned to the instructor have checked attributes, which selects them (displays them checked).

After changing course assignments, you'll want to be able to verify the changes when the site returns to the Index page. Therefore, you need to add a column to the table in that page. The information you want to display is already in the Courses navigation property of the Instructor entity that you're passing to the page as the model.

In Views\Instructor\Index.cshtml, add a Courses heading immediately following the Office heading, as shown in the following example:

```
<tr> 
    <th>Last Name</th> 
    <th>First Name</th> 
    <th>Hire Date</th> 
    <th>Office</th>
    <th>Courses</th>
    <th></th> 
</tr> 
```

Then add a new detail cell immediately following the office location detail cell:

```
<td>
    @if (item.OfficeAssignment != null)
    {
        @item.OfficeAssignment.Location
    }
</td>
<td>
    @{
        foreach (var course in item.Courses)
        {
            @course.CourseID @:  @course.Title <br />
        }
    }
</td>
```

Run the Instructor Index page to see the courses assigned to each instructor:

![Instructors Index page with courses](todo)

Click Edit on an instructor to see the Edit page.

![Instructor Edit page with courses](todo)

Change some course assignments and click Save. The changes you make are reflected on the Index page.

Note: The approach taken here to edit instructor course data works well when there is a limited number of courses. For collections that are much larger, a different UI and a different updating method would be required.
 
## Update the DeleteConfirmed Method

In InstructorController.cs, delete the DeleteConfirmed method and insert the following code in its place.

```
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Instructor instructor = await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.Courses)
                .SingleAsync(i => i.ID == id);

            var department = await _context.Departments
                .SingleOrDefaultAsync(d => d.InstructorID == id);
            if (department != null)
            {
                department.InstructorID = null;
            }

            _context.Instructors.Remove(instructor);

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
```

This code makes the following changes:

*  Does eager loading for the Courses and OfficeAssignment navigation properties.  You have to include these or EF won't know about them and won't delete them.  To avoid needing to read them here you could configure cascade delete in the database.
* If the instructor is assigned as administrator of any department, removes the instructor assignment from that department. Without this code, you would get a referential integrity error if you tried to delete an instructor who was assigned as administrator for a department. This code doesn't handle the scenario of one instructor assigned as administrator for multiple departments. 

## Add office location and courses to the Create page

In InstructorController.cs, delete the HttpGet and HttpPost Create methods, and then add the following code in their place:

```
       // GET: Instructors/Create
        public IActionResult Create()
        {
            var instructor = new Instructor();
            instructor.Courses = new List<CourseInstructor>();
            PopulateAssignedCourseData(instructor);
            return View();
        }

        // POST: Instructors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstMidName,HireDate,LastName")] Instructor instructor, string[] selectedCourses)
        {
            if (selectedCourses != null)
            {
                instructor.Courses = new List<CourseInstructor>();
                foreach (var course in selectedCourses)
                {
                    var courseToAdd = new CourseInstructor { InstructorID = instructor.ID, CourseID = int.Parse(course) };
                    instructor.Courses.Add(courseToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                _context.Add(instructor);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(instructor);
        }
```

This code is similar to what you saw for the Edit methods except that initially no courses are selected. The HttpGet Create method calls the PopulateAssignedCourseData method not because there might be courses selected but in order to provide an empty collection for the foreach loop in the view (otherwise the view code would throw a null reference exception).

The HttpPost Create method adds each selected course to the Courses navigation property before it checks for validation errors and adds the new instructor to the database. Courses are added even if there are model errors so that when there are model errors (for an example, the user keyed an invalid date), and the page is redisplayed with an error message, any course selections that were made are automatically restored.

Notice that in order to be able to add courses to the Courses navigation property you have to initialize the property as an empty collection:

```
instructor.Courses = new List<Course>();
```

As an alternative to doing this in controller code, you could do it in the Instructor model by changing the property getter to automatically create the collection if it doesn't exist, as shown in the following example:

```
private ICollection<Course> _courses;
public ICollection<Course> Courses 
{ 
    get
    {
        return _courses ?? (_courses = new List<Course>());
    }
    set
    {
        _courses = value;
    } 
}
```

If you modify the Courses property in this way, you can remove the explicit property initialization code in the controller.

In Views\Instructor\Create.cshtml, add an office location text box and course check boxes after the hire date field and before the Submit button. As in the case of the Edit page, this will work better if you do it in a text editor such as Notepad.

```
        <div class="form-group">
            <label asp-for="OfficeAssignment.Location" class="col-md-2 control-label"></label>
            <div class="col-md-10">
                <input asp-for="OfficeAssignment.Location" class="form-control" />
                <span asp-validation-for="OfficeAssignment.Location" class="text-danger" />
            </div>
        </div>

        <div class="form-group">
            <div class="col-md-offset-2 col-md-10">
                <table>
                    <tr>
                        @{
                            int cnt = 0;
                            List<ContosoUniversity.Models.SchoolViewModels.AssignedCourseData> courses = ViewBag.Courses;

                            foreach (var course in courses)
                            {
                                if (cnt++ % 3 == 0)
                                {
                                    @:</tr><tr>
                                }
                                @:<td>
                                    <input type="checkbox"
                                           name="selectedCourses"
                                           value="@course.CourseID"
                                           @(Html.Raw(course.Assigned ? "checked=\"checked\"" : "")) />
                                        @course.CourseID @:  @course.Title
                                        @:</td>
                            }
                            @:</tr>
                        }
                    </table>
                </div>
            </div>


        <div class="form-group">
            <div class="col-md-offset-2 col-md-10">
                <input type="submit" value="Create" class="btn btn-default" />
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

Run the Create page and add an instructor.

## Handling Transactions

As explained in the Basic CRUD Functionality tutorial, by default the Entity Framework implicitly implements transactions. For scenarios where you need more control -- for example, if you want to include operations done outside of Entity Framework in a transaction -- see Working with Transactions on MSDN.todo is this ref current?https://ef.readthedocs.io/en/latest/saving/transactions.html


## Summary

You have now completed this introduction to working with related data. In the next tutorial you'll see how to handle concurrency conflicts.