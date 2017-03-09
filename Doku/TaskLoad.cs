 using Infragistics.Win.UltraWinSchedule;
 using Infragistics.Win.UltraWinGanttView;

 private void Tasks_Programmatically_Load(object sender, EventArgs e)
 {
					// Create a new Project other than the Unassigned Project
            Project quarterlyProject = this.ultraCalendarInfo1.Projects.Add("QuartlerlyProject", DateTime.Today);
            quarterlyProject.Key = "projkey1";
            
            // Create a Summary or Parent Task
            Task requirementsTask = this.ultraCalendarInfo1.Tasks.Add(DateTime.Today, TimeSpan.FromDays(5), "Requirements", "projkey1");
           
            // Create a child task
            Task budgetTask = requirementsTask.Tasks.Add(DateTime.Today, TimeSpan.FromDays(2), "Budget Analysis");
            // Set Deadline
            budgetTask.Deadline = DateTime.Today.AddDays(3); 
            //Assign a Resource for this task
            Owner budgetOwner = this.ultraCalendarInfo1.Owners.Add("BudgetOwner", "Bill Isacky");
            budgetTask.Resources.Add(budgetOwner);
           
            // Create another child task
            Task teamTask = requirementsTask.Tasks.Add(DateTime.Today.AddDays(3), TimeSpan.FromDays(2), "Team Allocation");
            // Set a Constraint for this Task
            teamTask.ConstraintDateTime = DateTime.Today.AddDays(4);
            teamTask.Constraint = TaskConstraint.FinishNoLaterThan;
            
           
            
            // Create a Summary or Parent Task
            Task implemetationTask = this.ultraCalendarInfo1.Tasks.Add(DateTime.Now.AddDays(7), TimeSpan.FromDays(3), "Implementation", "projkey1");
           
            // Create a child task
            Task frontendTask = implemetationTask.Tasks.Add(DateTime.Now.AddDays(7), TimeSpan.FromDays(3), "GUI Design");
            // Set this task as a Milestone
            frontendTask.Milestone = true;
            // Set Percent Complete for this Task
            frontendTask.PercentComplete = 40;
            frontendTask.Dependencies.Add(budgetTask, TaskDependencyType.StartToStart);
            frontendTask.Dependencies.Add(teamTask, TaskDependencyType.FinishToStart);

            this.ultraGanttView1.CalendarInfo = this.ultraCalendarInfo1;
            // Assign the new Project to GanttView so that this Project is shown in GanttView and not the unassigned Project.
            this.ultraGanttView1.Project = this.ultraGanttView1.CalendarInfo.Projects[1];

 }