using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using Infragistics.Win.UltraWinSchedule;
using Infragistics.Win.UltraWinGanttView;
using Infragistics.Win.UltraMessageBox;
using Infragistics.Win;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win.AppStyling;
using Infragistics.Win.UltraWinToolTip;

namespace UltraGanttView_CS
{
    public partial class UltraGanttView : Form
    {
        #region Constructor

        public UltraGanttView()
        {
            InitializeComponent();
            
        }
        #endregion //Constructor

        #region Private Members
      
        ResourceManager rm = UltraGanttView_CS.Properties.Resources.ResourceManager;

        // Create a new Project other than the Unassigned Project
        Project quarterlyProject = null;
        string projectKey = "projkey1";
        // Create Summary or Parent Tasks
        Task requirementsTask = null;
        Task visualDesign = null;
        Task qualityAssurance = null;
        
        // Create a child task, to start with.
        Task budgetTask = null;
        Task teamTask = null;

        // Create child tasks
        Task redesign = null;
        Task mockUp = null;
        Task meetEngineer = null;
        Task specReview = null;
        Task testGrid = null;
        Task proffRead = null;
       
        #endregion //Private Members

        #region Private methods

        #region InitializeSampleControls

        private void InitializeSampleControls()
        {
            this.cmbRowSelectorStyle.DataSource = Enum.GetValues(typeof(Infragistics.Win.UltraWinGrid.RowSelectorHeaderStyle));
            this.cmbDragOptions.DataSource = Enum.GetValues(typeof(BarDragActions));
            this.numEdtBarHeight.MinValue = 6;
            this.numEdtBarHeight.MaxValue = 15;
            this.cmbBarTextRight.DataSource = Enum.GetValues(typeof(Infragistics.Win.UltraWinSchedule.TaskUI.BarTextField));
            this.numEdtPercentCompleteHeight.MinValue = 2;
            this.numEdtPercentCompleteHeight.MaxValue = 12;

            this.cmbAddIntHeaderFormatStyle.DataSource = Enum.GetValues(typeof(TimelineViewHeaderTextFormatStyle));
            this.cmbAddIntHeaderFormatStyle.SelectedItem = TimelineViewHeaderTextFormatStyle.Default;
            
            this.cmbDisplayArea.DataSource = Enum.GetValues(typeof(DisplayAreas));
            this.cmbDisplayArea.SelectedItem = DisplayAreas.GridAndTimeLine;
            
            // To start with, set the following settings

            // Grid section of the control
            this.cmbRowSelectorStyle.SelectedIndex = 0;
            this.cmbGanttViewGridSettings.SelectedIndex = 0;
            this.numEditOverlayBorderThickness.MinValue = 0;
            this.numEditOverlayBorderThickness.MaxValue = 8;
            this.cmbSelectionOverlayColor.SelectedIndex = 0;
                     
            // Chart Section of the control
            this.cmbDragOptions.SelectedItem = BarDragActions.Default;
            this.numEdtBarHeight.Value = 10;
            this.numEdtPercentCompleteHeight.Value = 5;
            this.cmbBarTextRight.SelectedItem = Infragistics.Win.UltraWinSchedule.TaskUI.BarTextField.Default;
            this.cmbBarAppearances.SelectedIndex = 0;
            this.chkDrawDependencyLines.Checked = true;
            

            // Timeline settings of chart section
            this.chkAddInterval.Checked = true;
            this.cmbAddIntHeaderTextFormat.SelectedIndex = 3;
            this.nmColumnWidth.Minimum = 35;
            this.nmColumnWidth.Maximum = 315;
            this.cmbTimelineAppearances.SelectedIndex = 0;

            // Show the following fields along with the default fields shown in  Grid area of GanttView
            this.ultraGanttView1.GridSettings.ColumnSettings[TaskField.Constraint].Visible = Infragistics.Win.DefaultableBoolean.True;
            this.ultraGanttView1.GridSettings.ColumnSettings[TaskField.ConstraintDateTime].Visible = Infragistics.Win.DefaultableBoolean.True;
            this.ultraGanttView1.GridSettings.ColumnSettings[TaskField.PercentComplete].Visible = Infragistics.Win.DefaultableBoolean.True;
            this.ultraGanttView1.GridSettings.ColumnSettings[TaskField.Deadline].Visible = Infragistics.Win.DefaultableBoolean.True;
            this.ultraGanttView1.GridSettings.ColumnSettings[TaskField.Milestone].Visible = Infragistics.Win.DefaultableBoolean.True;

            UltraToolTipInfo tooltipPercentBarHeight = new UltraToolTipInfo();
            tooltipPercentBarHeight.ToolTipText = rm.GetString("PercentBarHeightToolTip");
            tooltipPercentBarHeight.Enabled = DefaultableBoolean.True;
            this.ultraToolTipManager1.SetUltraToolTip(this.numEdtPercentCompleteHeight, tooltipPercentBarHeight);

            UltraToolTipInfo tooltipAllowedDragOption = new UltraToolTipInfo();
            tooltipAllowedDragOption.ToolTipText = rm.GetString("AllowedDragOptionToolTip");
            tooltipAllowedDragOption.Enabled = DefaultableBoolean.True;
            this.ultraToolTipManager1.SetUltraToolTip(this.cmbDragOptions, tooltipAllowedDragOption);

            UltraToolTipInfo tooltipDependencyLines = new UltraToolTipInfo();
            tooltipDependencyLines.ToolTipText = rm.GetString("ShowDependencyLinesToolTip");
            tooltipDependencyLines.Enabled = DefaultableBoolean.True;
            this.ultraToolTipManager1.SetUltraToolTip(this.chkDrawDependencyLines, tooltipDependencyLines);

            
            UltraToolTipInfo tooltipBarTextRight = new UltraToolTipInfo();
            tooltipBarTextRight.ToolTipText = rm.GetString("BarTextRightToolTip");
            tooltipBarTextRight.Enabled = DefaultableBoolean.True;
            this.ultraToolTipManager1.SetUltraToolTip(this.cmbBarTextRight, tooltipBarTextRight);
        }

        #endregion //InitializeSampleControls

        #region Create a new Project

        private void CreateProject()
        {
            quarterlyProject = this.ultraCalendarInfo1.Projects.Add("QuartlerlyProject", DateTime.Today);
            quarterlyProject.Key = projectKey;
                        
            this.ultraGanttView1.CalendarInfo = this.ultraCalendarInfo1;
            // Assign the new Project to GanttView so that this Project is shown in GanttView and not the unassigned Project.
            this.ultraGanttView1.Project = this.ultraGanttView1.CalendarInfo.Projects[1];
                       
        }

        #endregion //Create a new Project

        #region Create a Parent and Child Task to start with

        private void CreateTasks()
        {
            requirementsTask = this.ultraCalendarInfo1.Tasks.Add(DateTime.Today,TimeSpan.FromDays(6),rm.GetString("RequirementsTaskName"),projectKey);
            budgetTask = requirementsTask.Tasks.Add(DateTime.Today, TimeSpan.FromDays(2), rm.GetString("BudgetTaskName"));
            teamTask = requirementsTask.Tasks.Add(DateTime.Today.AddDays(3), TimeSpan.FromDays(2), rm.GetString("TeamTaskName"));
            
            visualDesign = this.ultraCalendarInfo1.Tasks.Add(DateTime.Today, TimeSpan.FromDays(8), rm.GetString("VisualDesignTaskName"), projectKey);
            redesign = visualDesign.Tasks.Add(DateTime.Today, TimeSpan.FromDays(3), rm.GetString("RedesignWebsiteTaskName"));
            mockUp = visualDesign.Tasks.Add(DateTime.Today.AddDays(3), TimeSpan.FromDays(3), rm.GetString("MockUpTaskName"));
            meetEngineer = mockUp.Tasks.Add(DateTime.Today.AddDays(3), TimeSpan.FromDays(1), rm.GetString("MeetEngTaskName"));
            specReview = mockUp.Tasks.Add(DateTime.Today.AddDays(4), TimeSpan.FromDays(1), rm.GetString("SpecReviewTaskName"));
                   
        }

        #endregion // Create a Parent and Child Task to start with

        #region Create Summary Tasks

        private void CreateSummaryTasks()
        {
            // A single summary task is already added to the GanttView, to start with
            if (this.ultraGanttView1.CalendarInfo.Tasks.Count == 2)
            {
                qualityAssurance = this.ultraCalendarInfo1.Tasks.Add(DateTime.Today, TimeSpan.FromDays(10), rm.GetString("QualityAssTaskName"), projectKey);
                                     
            }
        }

        #endregion //Create Summary Tasks

        #region Create Child tasks

        private void CreateChildTasks()
        {
            // Two summary Tasks with child tasks are already added to the GanttView, to start with
           // Add child tasks for the 3rd Summary Task('Quality Assurance' Task)
            if (this.ultraGanttView1.CalendarInfo.Tasks.Count == 3)
            {
                testGrid = qualityAssurance.Tasks.Add(DateTime.Today, TimeSpan.FromDays(5), rm.GetString("TestGridTaskName"));
                proffRead = qualityAssurance.Tasks.Add(DateTime.Today.AddDays(6), TimeSpan.FromDays(1), rm.GetString("ProofReadTaskName"));
                     
            }

        }

        #endregion // Create Child tasks

        #region Add Resources

        private void AddResources()
        {
                Owner budgetOwner = this.ultraCalendarInfo1.Owners.Add("budgetOwner", rm.GetString("Philips"));
                budgetTask.Resources.Add(budgetOwner);
                Owner teamOwner = this.ultraCalendarInfo1.Owners.Add("teamTaskOwner", rm.GetString("Mathew"));
                teamTask.Resources.Add(teamOwner);
                Owner redisgnOwner = this.ultraCalendarInfo1.Owners.Add("redisgnOwner", rm.GetString("Maria"));
                redesign.Resources.Add(redisgnOwner);
                Owner meetEngineerOwner = this.ultraCalendarInfo1.Owners.Add("MeetEngOwner", rm.GetString("Robert"));
                meetEngineer.Resources.Add(meetEngineerOwner);
                  
        }

        #endregion

        #region Add Dependencies

        private void AddDependencies()
        {           
                teamTask.Dependencies.Add(budgetTask, TaskDependencyType.StartToStart);
                mockUp.Dependencies.Add(teamTask, TaskDependencyType.StartToStart);
                mockUp.Dependencies.Add(redesign, TaskDependencyType.FinishToStart);
            
                        
        }
        #endregion //Add Dependencies

        #region Constraints

        private void AddConstraints()
        {
            
            budgetTask.Constraint = TaskConstraint.MustStartOn;
                    
            meetEngineer.Constraint = TaskConstraint.MustStartOn;
          
        }
        #endregion

        #region Percent Complete

        private void AddPercentComplete()
        {
            budgetTask.PercentComplete = 85;
            teamTask.PercentComplete = 60;
            redesign.PercentComplete = 35;

        }
        
        #endregion //Percent Complete

        #region Deadlne

        private void AddDeadline()
        {
            redesign.Deadline = DateTime.Today.AddDays(5);
            specReview.Deadline = DateTime.Today.AddDays(7);

        }
        #endregion //Deadline

        #region Milestone

        private void AddMilestone()
        {
            teamTask.Milestone = true;
            redesign.Milestone = true;
        }
        #endregion //Milestone

        #region Timeline Appearances

        private AppearanceBase RetrieveAppearanceBasedOnIndex(int index, out string description)
        {
            // Retrieve the appropriate Appearance object (and description) based on the provided index.
            switch (index)
            {
                case 0 : // "Column Header Appearance"
                    description = rm.GetString("DescriptionColumnHeaderApp"); 
                    return this.ultraGanttView1.TimelineSettings.ColumnHeaderAppearance;
                                                
                case 1: // "Non-Working Hour Appearance"
                    description = rm.GetString("DescriptionNonWorkingHourApp");
                    return this.ultraGanttView1.TimelineSettings.NonWorkingHourAppearance;
                   
                case 2: // "Working Appearance"
                    description = rm.GetString("DescriptionWorkingHourApp");
                    return this.ultraGanttView1.TimelineSettings.WorkingHourAppearance;
                   
                default:
                    description = rm.GetString("DescriptionDefault"); 
                    return null;
            }
        }

        #endregion // Timeline Appearances

        #region GanttView Grid Settings Appearances

        private AppearanceBase RetrieveGanttViewGridSettingsAppearance(int index, out string description)
        {
            // Retrieve the appropriate Appearance object (and description) based on the provided index.
            switch (index)
            {
                case 0: // "Cell Appearance"
                    description = rm.GetString("DescriptionCellAppearance");
                    return this.ultraGanttView1.GridSettings.CellAppearance;
               
                case 1: // Cell Appearance for a specific Task
                    description = rm.GetString("DescriptionSpecificCellAppearance");
                    return this.ultraGanttView1.CalendarInfo.Tasks[0].Tasks[0].GridSettings.CellSettings[TaskField.Name].Appearance;

                case 2: //"Column Header Appearance"
                    description = rm.GetString("DescriptionColumnHeaderAppearance");
                    this.ultraGanttView1.GridSettings.ColumnHeaderAppearance.ThemedElementAlpha = Alpha.Transparent;
                    return this.ultraGanttView1.GridSettings.ColumnHeaderAppearance;

                case 3: //"Row Appearance"
                    description = rm.GetString("DescriptionRowAppearance");
                    return this.ultraGanttView1.GridSettings.RowAppearance;

                case 4: //"Row Selector Appearance"
                    description = rm.GetString("DescriptionRowSelectorAppearance");
                    return this.ultraGanttView1.GridSettings.RowSelectorAppearance;

                default:
                    description = rm.GetString("DescriptionDefaultGanttViewGridSettings");
                    return null;

            }

        }
                       
        #endregion // GanttView Grid Settings Appearances
       
        #region Appearance settings for Bar UI Elements in Chart section of the control

        private AppearanceBase RetrieveChartBarUIAppearance(int index, out string description)
        {
            // Retrieve the appropriate Appearance object (and description) based on the provided index.
            switch (index)
            {
                case 0: // "Bar Appearance"
                    description = rm.GetString("DescriptionBarAppearance");
                    return this.ultraGanttView1.TimelineSettings.BarSettings.BarAppearance;

                case 1: //"Percent Complete Bar Appearance"
                    description = rm.GetString("DescriptionPercentCompleteBarAppearance");
                    return this.ultraGanttView1.TimelineSettings.BarSettings.PercentCompleteBarAppearance;

                case 2: //"Start Indicator Bar Appearance"
                    description = rm.GetString("DescriptionStartIndicatorBarAppearance");
                    this.ultraGanttView1.TimelineSettings.BarSettings.StartIndicatorVisible = DefaultableBoolean.True;
                    return this.ultraGanttView1.TimelineSettings.BarSettings.StartIndicatorAppearance;

                case 3: //"End Indicator Bar Appearance"
                    description = rm.GetString("DescriptionEndIndicatorBarAppearance");
                    this.ultraGanttView1.TimelineSettings.BarSettings.EndIndicatorVisible = DefaultableBoolean.True;
                    return this.ultraGanttView1.TimelineSettings.BarSettings.EndIndicatorAppearance;
                
                case 4: //"Deadline Indicator Appearance"
                    description = rm.GetString("DescriptionDeadlineIndicatorAppearance");
                    return this.ultraGanttView1.TimelineSettings.BarSettings.DeadlineIndicatorAppearance;
                
                default:
                    description = rm.GetString("DescriptionDefaultBarAppearances");
                    return null;

            }

        }

        #endregion // Appearance settings for UI Elements in Chart section of the control

        #endregion //Private methods

        #region Event Handlers

        #region Form Load Event

        private void Form1_Load(object sender, EventArgs e)
        {
           // StyleManager.Load("LucidDream.isl");
            InitializeSampleControls();
            CreateProject();
            CreateTasks();
            

        }

#endregion //Form Load Event
        
        #region Summary tasks Button Click Event

        private void btnAddSummaryTasks_Click(object sender, EventArgs e)
        {
            //Add Paren/Summary Tasks to GanttView
            CreateSummaryTasks();
            this.btnAddSummaryTasks.Enabled = false;
            
        }
        #endregion //Summary tasks

        #region Child Tasks Button Click Event
        
        private void btnAddChildtasks_Click(object sender, EventArgs e)
        {
            // Add child tasks only if the number of summary tasks is 3( 2 Parent tasks added by default(Requirement and Visual Design tasks) and the 3rd Task-Quality Assurance)
            if (this.ultraGanttView1.CalendarInfo.Tasks.Count <= 2)
            {
                // Invoke a message box to indicate that parent tasks must be added before adding child tasks
                UltraMessageBoxInfo info = new UltraMessageBoxInfo(MessageBoxStyle.Vista, Owner, rm.GetString("MsgBoxText"), rm.GetString("MsgBoxCaption"), MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, Infragistics.Win.DefaultableBoolean.False);
                this.ultraMessageBoxManager1.ShowMessageBox(info);
            }
            else
            CreateChildTasks();
            this.btnAddChildtasks.Enabled = false;

        }

        #endregion //Child Tasks Button Click Event
        
        #region Task Details Checked List Box Event

        private void clbTaskDetails_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            switch (e.Index)
            {
                case 0://Resources
                    
                    if(e.NewValue == CheckState.Checked)
                    {
                        this.lblTaskDetailsDescription.Text = rm.GetString("ResourcesDescription");
                       AddResources();
                       
                    }
                    else if(e.NewValue == CheckState.Unchecked)
                    {
                        this.ultraGanttView1.CalendarInfo.Owners.Clear();
                        this.lblTaskDetailsDescription.Text = rm.GetString("DefaultDescription");
                    }
                                    
                    break;
                case 1: //Dependencies
                    
                    if (e.NewValue == CheckState.Checked)
                    {
                        
                        this.lblTaskDetailsDescription.Text = rm.GetString("DependenciesDescription");
                        AddDependencies();
                    }
                    else if (e.NewValue == CheckState.Unchecked)
                    {
                        teamTask.Dependencies.Clear();
                        mockUp.Dependencies.Clear();
                        this.lblTaskDetailsDescription.Text = rm.GetString("DefaultDescription");
                    }
                    
                    break;
                case 2: //Constraints
                    if (e.NewValue == CheckState.Checked)
                    {
                        this.lblTaskDetailsDescription.Text = rm.GetString("ConstraintsDescription");
                       AddConstraints();
                    }
                    else if (e.NewValue == CheckState.Unchecked)
                    {
                        budgetTask.Constraint = TaskConstraint.AsSoonAsPossible;
                                                
                        meetEngineer.Constraint = TaskConstraint.StartNoEarlierThan;
                        
                        this.lblTaskDetailsDescription.Text = rm.GetString("DefaultDescription");
                                                                        
                    }
                   break;
                case 3: // Percent Complete
                    if (e.NewValue == CheckState.Checked)
                    {
                        this.lblTaskDetailsDescription.Text = rm.GetString("PercentCompleteDescription");
                        AddPercentComplete();
                    }
                    else if (e.NewValue == CheckState.Unchecked)
                    {
                        budgetTask.PercentComplete = 0;
                        teamTask.PercentComplete = 0;
                        redesign.PercentComplete = 0;
                        this.lblTaskDetailsDescription.Text = rm.GetString("DefaultDescription");
                                                
                    }
                    break;
                case 4: // Deadline
                    if (e.NewValue == CheckState.Checked)
                    {                        
                        this.lblTaskDetailsDescription.Text = rm.GetString("DeadlineDescription");
                        AddDeadline();
                    }
                    else if(e.NewValue == CheckState.Unchecked)
                    {
                        redesign.Deadline = DateTime.MinValue;
                        specReview.Deadline = DateTime.MinValue;
                        this.lblTaskDetailsDescription.Text = rm.GetString("DefaultDescription");
                    }
                    break;
                case 5: //Milestone
                    if (e.NewValue == CheckState.Checked)
                    {                        
                        this.lblTaskDetailsDescription.Text = rm.GetString("MilestoneDescription");
                        AddMilestone();
                    }
                    else if (e.NewValue == CheckState.Unchecked)
                    {
                        teamTask.Milestone = false;
                        redesign.Milestone = false;
                        this.lblTaskDetailsDescription.Text = rm.GetString("DefaultDescription");
                    }
                    break;

            }
        }

        #endregion // Task Details Checked List Box Event handler

        #region Edit Task Information Checked List Box Event
        
        private void clbEditTasks_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            switch (e.Index)
            {
                case 0:// Editing of Deadline
                    if (e.NewValue == CheckState.Checked)
                    {
                        // Disable editing of Deadline for all Tasks added to GanttView
                        this.ultraGanttView1.TaskSettings.AllowEditDeadline = DefaultableBoolean.False;
                    }
                    else if (e.NewValue == CheckState.Unchecked)
                    {
                        // Allow editing of Deadline for all Tasks added to GanttView
                        this.ultraGanttView1.TaskSettings.AllowEditDeadline = DefaultableBoolean.True;
                    }
                    break;
                case 1:// Editing of  Duration
                    if (e.NewValue == CheckState.Checked)
                    {
                        // Disable editing of Duration for all Tasks added to GanttView
                        this.ultraGanttView1.TaskSettings.AllowEditDuration = DefaultableBoolean.False;
                    }
                    else if (e.NewValue == CheckState.Unchecked)
                    {
                        // Allow editing of Duration for all Tasks added to GanttView
                        this.ultraGanttView1.TaskSettings.AllowEditDuration = DefaultableBoolean.True;
                    }
                    break;
                case 2:// Editing of  Percent Complete
                    if (e.NewValue == CheckState.Checked)
                    {
                        // Disable editing of Percent Complete for all Tasks added to GanttView
                        this.ultraGanttView1.TaskSettings.AllowEditPercentComplete = DefaultableBoolean.False;
                    }
                    else if (e.NewValue == CheckState.Unchecked)
                    {
                        // Allow editing of Percent Complete for all Tasks added to GanttView
                        this.ultraGanttView1.TaskSettings.AllowEditPercentComplete = DefaultableBoolean.True;
                    }
                    break;
                case 3:// Editing of  Start Date Time
                    if (e.NewValue == CheckState.Checked)
                    {
                        // Disable editing of start date time for all Tasks added to GanttView
                        this.ultraGanttView1.TaskSettings.AllowEditStartDateTime = DefaultableBoolean.False;
                    }
                    else if (e.NewValue == CheckState.Unchecked)
                    {
                        // Allow editing of start date time for all Tasks added to GanttView
                        this.ultraGanttView1.TaskSettings.AllowEditStartDateTime = DefaultableBoolean.True;
                    }
                    break;
            }
        }

        #endregion // Edit Task Information Checked List Box Event

        #region Disable/Inactivate Tasks CheckBox Event

        private void chkDisableTasks_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox currentcheckstate = (CheckBox)sender;
            if (currentcheckstate.Checked)
            {
                // Disable Tasks
                this.ultraGanttView1.TaskSettings.Enabled = DefaultableBoolean.False;

            }
            else
            {
                // Enable Tasks
                this.ultraGanttView1.TaskSettings.Enabled = DefaultableBoolean.True;
            }
        }
        #endregion // Disable/Inactivate Tasks CheckBox Event

        #region Expand/Collapse Tasks Button Event

        private void btnExpandTasks_Click(object sender, EventArgs e)
        {
            // Expand all Tasks in GanttView
            this.ultraGanttView1.CalendarInfo.Tasks.ExpandAll();
        }

        private void btnCollpaseTasks_Click(object sender, EventArgs e)
        {
            // Collapse all Tasks in GanttView
            this.ultraGanttView1.CalendarInfo.Tasks.CollapseAll();
        }


        #endregion // Expand/Collapse Tasks Button Event

        #region GanttView Grid Settings ComboBox Event
        private void cmbGanttViewGridSettings_ValueChanged(object sender, EventArgs e)
        {
            string description;
            UltraComboEditor combo = (UltraComboEditor)sender;

            // Retrieve the Appearance object based on the selected index, and set it to the PropertyGrid
            this.pgGanttViewGridSettings.SelectedObject = RetrieveGanttViewGridSettingsAppearance(combo.SelectedIndex, out description);

            // Update the Appearance Description label
            this.lblGanttViewGridSettings.Text = description;
            
        }
        
        #endregion // GanttView Grid Settings ComboBox Event

        #region Disable Column Moving CheckBox Event
        private void chkColumnMoving_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox currentState = (CheckBox)sender;
            if (currentState.Checked)
            {
                // Disable Column moving in Grid area of GanttView
                this.ultraGanttView1.GridSettings.AllowColumnMoving = false;
            }
            else
            {
                // Enable Column moving in Grid area of GanttView
                this.ultraGanttView1.GridSettings.AllowColumnMoving = true;
            }
        }

        #endregion //Disable COlumn Moving CheckBox Event

        #region Selection Overlay Color ComboBox Event

        private void cmbSelectionOverlayColor_ValueChanged(object sender, EventArgs e)
        {
            UltraComboEditor activeItem = (UltraComboEditor)sender;
            if (activeItem.SelectedIndex == 0)
            {
                this.lblSelectionOverlay.Text = rm.GetString("DescriptionSelectionOverlayBorderColor");
            }
            else if (activeItem.SelectedIndex == 1)
            {
                this.lblSelectionOverlay.Text = rm.GetString("DescriptionSelectionOverlayColor");
            }
        }

       
        #region // Selection Overlay Border Thickness Numeric Editor Event

        private void numEditOverlayBorderThickness_ValueChanged(object sender, EventArgs e)
        {
            UltraNumericEditor activevalue = (UltraNumericEditor)sender;
            this.ultraGanttView1.GridSettings.SelectionOverlayBorderThickness = (int)activevalue.Value;
        }

        #endregion // Selection Overlay Border Thickness Numeric Editor Event

       

        #endregion //Selection Overlay Color ComboBox Event

        #region Select overlay Color UltraColorPicker Event

        private void ultraColorPicker1_ColorChanged(object sender, EventArgs e)
        {
            if (cmbSelectionOverlayColor.SelectedIndex == 0)
            {
                // Set the Border color when a row is selected
                this.ultraGanttView1.GridSettings.SelectionOverlayBorderColor = this.ultraColorPicker1.Color;
            }
            else if (cmbSelectionOverlayColor.SelectedIndex == 1)
            {
                // Set the Overlay color when a row is selected
                this.ultraGanttView1.GridSettings.SelectionOverlayColor = this.ultraColorPicker1.Color;

            }
        }
        
        #endregion // Select overlay Color UltraColorPicker Event

        #region Reset all grid Appearances Button Event
       
        private void btnResetGridAppearances_Click(object sender, EventArgs e)
        {
            this.ultraGanttView1.GridSettings.CellAppearance.Reset();
            this.ultraGanttView1.CalendarInfo.Tasks[0].Tasks[0].GridSettings.CellSettings[TaskField.Name].Appearance.Reset();
            this.ultraGanttView1.GridSettings.ColumnHeaderAppearance.Reset();
            this.ultraGanttView1.GridSettings.RowAppearance.Reset();
            this.ultraGanttView1.GridSettings.RowSelectorAppearance.Reset();
            cmbGanttViewGridSettings.SelectedIndex = 0;
            pgGanttViewGridSettings.Refresh();
            this.ultraGanttView1.GridSettings.SelectionOverlayBorderColor = Color.Black;
            this.ultraGanttView1.GridSettings.SelectionOverlayColor = Color.Blue;
            cmbSelectionOverlayColor.SelectedIndex = 0;
            this.ultraColorPicker1.ResetColor();
            this.ultraGanttView1.GridSettings.SelectionOverlayBorderThickness = 3;
            numEditOverlayBorderThickness.Value = 3;
            
        }

        #endregion //Reset all grid Appearances Button Event

        #region Allowed Drag Options ComboBox Event

        private void cmbDragOptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox activeitem = (ComboBox)sender;
            // Set the drag option that is available within the Chart section of the control
            this.ultraGanttView1.TimelineSettings.AllowedDragActions = (BarDragActions)activeitem.SelectedItem;
        }

       

       
        #endregion // Allowed Drag Options ComboBox Event

        #region Bar Height Numeric Editor Event

        private void numEdtBarHeight_ValueChanged(object sender, EventArgs e)
        {
            UltraNumericEditor activevalue = (UltraNumericEditor)sender;
            // Set the bar height for child tasks shown in the chart section of the control
            this.ultraGanttView1.TimelineSettings.BarSettings.BarHeight = (int)activevalue.Value;

        }

        #endregion // Bar Height Numeric Editor Event

        #region PercentComplete Numeric Editor Event

        private void numEdtPercentCompleteHeight_ValueChanged(object sender, EventArgs e)
        {
            
            UltraNumericEditor activevalue = (UltraNumericEditor)sender;
            // Set the percent complete bar height for child tasks shown in the chart section of GanttView
            this.ultraGanttView1.TimelineSettings.BarSettings.PercentCompleteBarHeight = (int)activevalue.Value;

        }

        #endregion // PercentCOmplete Numeric Editor Event

        #region Bar Text Right ComboBox Event

        private void cmbBarTextRight_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox activeitem = (ComboBox)sender;
            // Set the text that should be visible to the right of the child task bars.
            this.ultraGanttView1.TimelineSettings.BarSettings.BarTextRight = (Infragistics.Win.UltraWinSchedule.TaskUI.BarTextField)activeitem.SelectedItem;
           
        }

        #endregion // Bar Text Roght ComboBox Event

        #region Bar Appearance ComboBox Event

        private void cmbBarAppearances_SelectedIndexChanged(object sender, EventArgs e)
        {            
            string description;
            ComboBox combo = (ComboBox)sender;

            // Retrieve the Appearance object based on the selected index, and set it to the PropertyGrid
            this.pgBarAppearances.SelectedObject = RetrieveChartBarUIAppearance(combo.SelectedIndex, out description);

            // Update the Appearance Description label
            this.lblBarAppearances.Text = description;
        }

        #endregion // Bar Appearance ComboBox Event

        #region Reset all Chart Appearances to default-Button Event

        private void btnResetChartAppearances_Click(object sender, EventArgs e)
        {
            this.ultraGanttView1.TimelineSettings.AllowedDragActions = BarDragActions.Default;
            cmbDragOptions.SelectedIndex = 6;
            this.ultraGanttView1.TimelineSettings.BarSettings.BarHeight = 0;
            numEdtBarHeight.Value = 10;
            this.ultraGanttView1.TimelineSettings.BarSettings.PercentCompleteBarHeight = 0;
            numEdtPercentCompleteHeight.Value = 5;
            this.ultraGanttView1.TimelineSettings.BarSettings.BarTextRight = Infragistics.Win.UltraWinSchedule.TaskUI.BarTextField.Default;
            cmbBarTextRight.SelectedIndex = 0;
            this.ultraGanttView1.TimelineSettings.BarSettings.BarAppearance.Reset();
            cmbBarAppearances.SelectedIndex = 0;
            pgBarAppearances.Refresh();
            this.ultraGanttView1.TimelineSettings.BarSettings.PercentCompleteBarAppearance.Reset();
            this.ultraGanttView1.TimelineSettings.BarSettings.StartIndicatorAppearance.Reset();
            this.ultraGanttView1.TimelineSettings.BarSettings.StartIndicatorVisible = DefaultableBoolean.False;
            this.ultraGanttView1.TimelineSettings.BarSettings.EndIndicatorAppearance.Reset();
            this.ultraGanttView1.TimelineSettings.BarSettings.EndIndicatorVisible = DefaultableBoolean.False;
            this.ultraGanttView1.TimelineSettings.BarSettings.DeadlineIndicatorAppearance.Reset();
            this.ultraGanttView1.TimelineSettings.DependencyLinkColor = Color.DarkBlue;
                        
        }

        #endregion //Reset all Chart Appearances to default-Button Event

        #region Additional Interval Check Box Event

        private void chkAddInterval_CheckedChanged(object sender, EventArgs e)
        {
            UltraCheckEditor currentstate = (UltraCheckEditor)sender;
            if (currentstate.Checked)
            {
                // Define DateInterval object and pass Interval and IntervalUnits as  parameters 
                // to set additional interval
                DateInterval ganttAddInterval1 = new DateInterval(1, DateIntervalUnits.Days);
                this.ultraGanttView1.TimelineSettings.AdditionalIntervals.Add(ganttAddInterval1);
                
                // Apply Header text Format and Header text Format style for additional interval.
                this.ultraGanttView1.TimelineSettings.AdditionalIntervals[0].HeaderTextFormat =(string) cmbAddIntHeaderTextFormat.SelectedItem;
                this.ultraGanttView1.TimelineSettings.AdditionalIntervals[0].HeaderTextFormatStyle = (TimelineViewHeaderTextFormatStyle)cmbAddIntHeaderFormatStyle.SelectedItem;

            }
            else
            {
                // Remove date interval from the additional Intervals collection
                this.ultraGanttView1.TimelineSettings.AdditionalIntervals.Clear();



            }
        }

        #endregion //Additional Interval Check Box Event

        #region Additional Interval Header Text Format Event

        private void cmbAddIntHeaderTextFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox activeItem = (ComboBox)sender;
            if (this.ultraGanttView1.TimelineSettings.AdditionalIntervals.Count == 1)
            {
                // Set the header text format for the additional intervals added to the timeline column headers in chart area
                this.ultraGanttView1.TimelineSettings.AdditionalIntervals[0].HeaderTextFormat = (string)activeItem.SelectedItem;

            }
        }
                
        #endregion //Additional Interval Header Format Event

        #region Additional Interval Header Format Style

        private void cmbAddIntHeaderFormatStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox activeItem = (ComboBox)sender;
            if (this.ultraGanttView1.TimelineSettings.AdditionalIntervals.Count == 1)
            {
                this.ultraGanttView1.TimelineSettings.AdditionalIntervals[0].HeaderTextFormatStyle = (TimelineViewHeaderTextFormatStyle)activeItem.SelectedItem;

            }

        }
        #endregion // Additional Interval Header Format Style

        #region Timeline Appearances ComboBox Event

        private void cmbTimelineAppearances_SelectedIndexChanged(object sender, EventArgs e)
        {
            string description;
            ComboBox combo = (ComboBox)sender;

            // Retrieve the Appearance object based on the selected index, and set it to the PropertyGrid
            this.pgAppearances.SelectedObject = RetrieveAppearanceBasedOnIndex(combo.SelectedIndex, out description);

            // Update the Appearance Description label
            this.lblAppearanceDescription.Text = description;

        }

        #endregion // Timeline Appearances ComboBox Event

        #region Column Width Numeric Up Down Event

        private void nmColumnWidth_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;
            // Set the column Width property
            this.ultraGanttView1.TimelineSettings.ColumnWidth = Convert.ToInt32(numericUpDown.Value);
        }

        #endregion // Column Width Numeric Up Down Event

        #region Row Selector Style ComboBox Event

        private void cmbRowSelectorStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox activeItem = (ComboBox)sender;
            this.ultraGanttView1.GridSettings.RowSelectorHeaderStyle = (Infragistics.Win.UltraWinGrid.RowSelectorHeaderStyle)activeItem.SelectedItem;

        }

        #endregion // Row Selector Style ComboBox Event

        #region Draw Dependency Lines CheckBox

        private void chkDrawDependencyLines_CheckedChanged(object sender, EventArgs e)
        {
            UltraCheckEditor activeItem = (UltraCheckEditor)sender;
            if (activeItem.Checked)
            {
                this.ultraGanttView1.TimelineSettings.DrawDependencyLines = true;
            }
            else
            {
                this.ultraGanttView1.TimelineSettings.DrawDependencyLines = false;
            }
        }
        #endregion //Draw Dependency Lines CheckBox

        #region Reset Timeline Appearances to default

        private void btnResetTimelineAppearances_Click(object sender, EventArgs e)
        {
            this.ultraGanttView1.TimelineSettings.ColumnHeaderAppearance.Reset();
            this.ultraGanttView1.TimelineSettings.WorkingHourAppearance.Reset();
            this.ultraGanttView1.TimelineSettings.NonWorkingHourAppearance.Reset();
        }
        #endregion //Reset Timeline Appearances to default

        #region cmbDisplayArea_SelectedIndexChanged
        private void cmbDisplayArea_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox activeItem = (ComboBox)sender;
            this.ultraGanttView1.DisplayAreas = (DisplayAreas)activeItem.SelectedItem;
        }
        #endregion cmbDisplayArea_SelectedIndexChanged

        #endregion //Event Handlers

    }
}