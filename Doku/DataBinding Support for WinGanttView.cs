using Infragistics.Win.UltraWinGanttView;
using Infragistics.Win.UltraWinSchedule;

//  Call the method that creates a DataSet
DataSet ds = this.GetSampleData();

//  Set the BindingContextControl property to reference this form
this.ultraCalendarInfo1.DataBindingsForTasks.BindingContextControl = this;
this.ultraCalendarInfo1.DataBindingsForProjects.BindingContextControl = this;

//  Set the DataBinding members for Projects 
this.ultraCalendarInfo1.DataBindingsForProjects.SetDataBinding(ds, "Projects");
this.ultraCalendarInfo1.DataBindingsForProjects.IdMember = "ProjectID";
this.ultraCalendarInfo1.DataBindingsForProjects.KeyMember = "ProjectKey";
this.ultraCalendarInfo1.DataBindingsForProjects.NameMember = "ProjectName";
this.ultraCalendarInfo1.DataBindingsForProjects.StartDateMember = "ProjectStartTime";

//  Set the DataBinding members for Tasks
this.ultraCalendarInfo1.DataBindingsForTasks.SetDataBinding(ds, "Tasks");

// Basic Task properties 
this.ultraCalendarInfo1.DataBindingsForTasks.NameMember = "TaskName";
this.ultraCalendarInfo1.DataBindingsForTasks.DurationMember = "TaskDuration";
this.ultraCalendarInfo1.DataBindingsForTasks.StartDateTimeMember = "TaskStartTime";
this.ultraCalendarInfo1.DataBindingsForTasks.IdMember = "TaskID";
this.ultraCalendarInfo1.DataBindingsForTasks.ProjectKeyMember = "ProjectKey";
this.ultraCalendarInfo1.DataBindingsForTasks.ParentTaskIdMember = "ParentTaskID";

this.ultraCalendarInfo1.DataBindingsForTasks.ConstraintMember = "Constraint";
this.ultraCalendarInfo1.DataBindingsForTasks.PercentCompleteMember = "TaskPercentComplete";
// All other properties
this.ultraCalendarInfo1.DataBindingsForTasks.AllPropertiesMember = "AllProperties";

//  Since we are showing a task that belongs to an explicitly defined
//  project (i.e., not the UnassignedProject), assign that project to
//  the control's Project property so the control knows to display that project.
this.ultraGanttView1.CalendarInfo = this.ultraCalendarInfo1;
this.ultraGanttView1.Project = this.ultraGanttView1.CalendarInfo.Projects[1];

private DataSet GetSampleData()
{
    DataSet theDataSet = new DataSet();
    string projectKey = "projectKey";
    
    DataTable theProjects = theDataSet.Tables.Add("Projects");
    theProjects.Columns.Add("ProjectID");
    theProjects.Columns.Add("ProjectKey");
    theProjects.Columns.Add("ProjectName");
    theProjects.Columns.Add("ProjectStartTime", typeof(DateTime));
    // Assign values for each Project member
    theProjects.Rows.Add(new Object[] { Guid.NewGuid(), projectKey, "QuarterlyProject", DateTime.Today });
    
    DataTable theTasks = theDataSet.Tables.Add("Tasks");
    theTasks.Columns.Add("TaskID");
    theTasks.Columns.Add("ProjectKey");
    theTasks.Columns.Add("TaskName");
    theTasks.Columns.Add("TaskStartTime", typeof(DateTime));
    theTasks.Columns.Add("TaskDuration", typeof(TimeSpan));
    theTasks.Columns.Add("ParentTaskID");
    theTasks.Columns.Add("Constraint", typeof(object));
    theTasks.Columns.Add("TaskPercentComplete");
    //The Task properties are all covered by individual members. But we could save space in the database 
    //By storing data as binary using AllProperties and not binding the other fields.
    theTasks.Columns.Add("AllProperties", typeof(Byte[]));
    
    
    Guid planningTaskid = Guid.NewGuid();
    // Assign values for each Task member
    
    // Parent Task1
    theTasks.Rows.Add(new Object[] { planningTaskid, projectKey, "Planning", DateTime.Now, TimeSpan.FromDays(5), null, TaskConstraint.StartNoEarlierThan, null });
    // Child Task1 of Parent Task1
    theTasks.Rows.Add(new Object[] { Guid.NewGuid(), projectKey, "Prepare Budget", DateTime.Now, TimeSpan.FromDays(2), planningTaskid,TaskConstraint.StartNoEarlierThan, 100 });
    // Child Task2 of Parent Task1
    theTasks.Rows.Add(new Object[] { Guid.NewGuid(), projectKey, "Allocate Teams", DateTime.Now.AddDays(2), TimeSpan.FromDays(3), planningTaskid,TaskConstraint.StartNoEarlierThan, null });
    
    // Parent Task2
    Guid implementationTaskid = Guid.NewGuid();
    theTasks.Rows.Add(new Object[] { implementationTaskid, projectKey, "Implementation", DateTime.Now.AddDays(6), TimeSpan.FromDays(16), null, TaskConstraint.StartNoEarlierThan,
    null });
    
    // Child Task1 of Parent Task2
    Guid installationTaskid2 = Guid.NewGuid();
    theTasks.Rows.Add(new Object[] { installationTaskid2, projectKey, "Installations", DateTime.Now.AddDays(6), TimeSpan.FromDays(2), implementationTaskid, 60 });
    // Child Task2 of Parent Task2
    theTasks.Rows.Add(new Object[] { Guid.NewGuid(), projectKey, "Execution", DateTime.Now.AddDays(8), TimeSpan.FromDays(13), implementationTaskid, TaskConstraint.StartNoEarlierThan,null });
    
    Guid testingTaskid = Guid.NewGuid();
    // Parent Task3
    theTasks.Rows.Add(new Object[] { testingTaskid, projectKey, "Testing", DateTime.Now.AddDays(23), TimeSpan.FromDays(20), null, TaskConstraint.StartNoEarlierThan,null });
    
    // Child Task1 of Parent Task3
    theTasks.Rows.Add(new Object[] { Guid.NewGuid(), projectKey, "TestPhase1", DateTime.Now.AddDays(23), TimeSpan.FromDays(10), testingTaskid,TaskConstraint.StartNoEarlierThan, 20 });
    // Child Task2 of Parent Task3
    theTasks.Rows.Add(new Object[] { Guid.NewGuid(), projectKey, "TestPhase2", DateTime.Now.AddDays(36), TimeSpan.FromDays(9), testingTaskid, TaskConstraint.StartNoEarlierThan,null });
    
    return theDataSet;
}
