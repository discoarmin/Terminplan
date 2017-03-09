using System;
using System.Windows.Forms;
using System.Resources;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinSchedule;
using Infragistics.Win.UltraWinGanttView;

namespace TaskWorkingHourMode_CS
{
    public partial class Form1 : Form
    {
        #region Private Memebers
        private readonly ResourceManager _resourceManager = Properties.Resources.ResourceManager;
        #endregion

        #region Constructor
        public Form1()
        {
            InitializeComponent();
        }
        #endregion //Constructor

        #region Event Handlers

        #region Form Load Event
        private void Form1Load(object sender, EventArgs e)
        {
            // Create a new Project other than the Unassigned Project
            Project quarterlyProject = ultraCalendarInfo1.Projects.Add(_resourceManager.GetString("ProjectName"), DateTime.Today);
            quarterlyProject.Key = _resourceManager.GetString("ProjectKey");

            // Create a Summary or Parent Task
            Task requirementsTask = ultraCalendarInfo1.Tasks.Add(DateTime.Today, TimeSpan.FromDays(5), _resourceManager.GetString("RequirementsTask"), _resourceManager.GetString("ProjectKey"));
            
            // Create a child task
            Task budgetTask = requirementsTask.Tasks.Add(DateTime.Today, TimeSpan.FromDays(2), _resourceManager.GetString("BudgetAnalysisTask"));
            Task teamTask = requirementsTask.Tasks.Add(DateTime.Today.AddDays(3), TimeSpan.FromDays(3), _resourceManager.GetString("TeamAllocationTask"));

            // Create a Summary or Parent Task
            Task implemetationTask = ultraCalendarInfo1.Tasks.Add(DateTime.Today.AddDays(5), TimeSpan.FromDays(1), _resourceManager.GetString("ImplementationTask"), _resourceManager.GetString("ProjectKey"));
            
            // Create a child task
            Task frontendTask = implemetationTask.Tasks.Add(DateTime.Today.AddDays(5), TimeSpan.FromDays(1), _resourceManager.GetString("GUIDesignTask"));
            
            //** Adding a deadline to a task.
            frontendTask.Deadline = frontendTask.StartDateTime.AddDays(7);

            ultraGanttView1.CalendarInfo = ultraCalendarInfo1;

            // Assign the new Project to GanttView so that this Project is shown in GanttView and not the unassigned Project.
            ultraGanttView1.Project = ultraGanttView1.CalendarInfo.Projects[1];

            ultraGanttView1.GridSettings.ColumnSettings[TaskField.StartDateTime].Format = Properties.Resources.TaskWorkingHoursMode_FullDateTime_Format;
            ultraGanttView1.GridSettings.ColumnSettings[TaskField.StartDateTime].Width = 120;

            ultraGanttView1.GridSettings.ColumnSettings[TaskField.EndDateTime].Format = Properties.Resources.TaskWorkingHoursMode_FullDateTime_Format;
            ultraGanttView1.GridSettings.ColumnSettings[TaskField.EndDateTime].Width = 120;

            ultraGanttView1.GridSettings.ColumnSettings[TaskField.Deadline].Format = Properties.Resources.TaskWorkingHoursMode_FullDateTime_Format;
            ultraGanttView1.GridSettings.ColumnSettings[TaskField.Deadline].Width = 120;

            rdbAutoAdjust.Checked = true;
            rdbFillTimeSlots.Checked = true;

            // To start with, set the Format to Days
            cmbTimeSpanFormat.SelectedIndex = 3;
            cmbTaskDragIncrement.SelectedIndex = 0;
        }
        #endregion

        #region TaskWorkingHourModeCheckedChanged (Task Duration Working Hour Mode)
        private void TaskWorkingHourModeCheckedChanged(object sender, EventArgs e)
        {
            var workingHourMode = (RadioButton)sender;

            ultraCalendarInfo1.TaskWorkingHourMode = 
                workingHourMode == rdbManual ? TaskWorkingHourMode.Manual : TaskWorkingHourMode.AutoAdjust;
        }
        #endregion

        #region CmbTimeSpanFormatValueChanged (Duration - Time Span Format)
        private void CmbTimeSpanFormatValueChanged(object sender, EventArgs e)
        {
            var durationFormat = (UltraComboEditor)sender;

            for (int i = 0; i < ultraCalendarInfo1.Tasks.Count; i++)
            {
                foreach (Task childTask in ultraCalendarInfo1.Tasks[i].Tasks)
                {
                    if (!childTask.IsSummary)
                    {
                        if (durationFormat.SelectedItem.ToString() == _resourceManager.GetString("MinutesFormat"))
                        {
                            childTask.SetDuration(childTask.Duration, Infragistics.Win.TimeSpanFormat.Minutes);

                            // Set up per minute PrimaryInterval.
                            ultraGanttView1.TimelineSettings.PrimaryInterval =
                                new TimeInterval(1, TimeIntervalUnits.Minutes);
                        }
                        else if (durationFormat.SelectedItem.ToString() == _resourceManager.GetString("HoursFormat"))
                        {
                            childTask.SetDuration(childTask.Duration, Infragistics.Win.TimeSpanFormat.Hours);

                            // Set up hourly PrimaryInterval.
                            ultraGanttView1.TimelineSettings.PrimaryInterval =
                                new TimeInterval(1, TimeIntervalUnits.Hours);
                        }
                        else if (durationFormat.SelectedItem.ToString() == _resourceManager.GetString("DaysFormat"))
                        {
                            childTask.SetDuration(childTask.Duration, Infragistics.Win.TimeSpanFormat.Days);

                            // Set up daily PrimaryInterval.
                            ultraGanttView1.TimelineSettings.PrimaryInterval =
                                new DateInterval(1, DateIntervalUnits.Days);
                        }
                        else if (durationFormat.SelectedItem.ToString() == _resourceManager.GetString("WeeksFormat"))
                        {
                            childTask.SetDuration(childTask.Duration, Infragistics.Win.TimeSpanFormat.Weeks);

                            // Set up weekly PrimaryInterval.
                            ultraGanttView1.TimelineSettings.PrimaryInterval =
                                new DateInterval(1, DateIntervalUnits.Weeks);
                        }
                    }
                }
            }
        }
        #endregion

        #region TaskPositioningCheckedChanged
        private void TaskPositioningCheckedChanged(object sender, EventArgs e)
        {
            var taskPositioning = (RadioButton)sender;

            ultraGanttView1.TimelineSettings.TaskPositioning =
                taskPositioning == rdbProportional ? TimelineTaskPositioning.Proportional : TimelineTaskPositioning.FillTimeSlots;
        }
        #endregion

        #region CmbTaskDragIncrementValueChanged
        private void CmbTaskDragIncrementValueChanged(object sender, EventArgs e)
        {
            var taskDragIncrement = (UltraComboEditor)sender;
            string[] incrementString = taskDragIncrement.SelectedItem.ToString().Split(' ');

            if (incrementString[1].Contains("minute"))
            {
                ultraGanttView1.TimelineSettings.TaskDragIncrement =
                    TimeSpan.FromMinutes(Convert.ToDouble(incrementString[0]));
            }
            else if (incrementString[1].Contains("hour"))
            {
                ultraGanttView1.TimelineSettings.TaskDragIncrement =
                    TimeSpan.FromHours(Convert.ToDouble(incrementString[0]));
            }
            else if (incrementString[1].Contains("day"))
            {
                ultraGanttView1.TimelineSettings.TaskDragIncrement =
                    TimeSpan.FromDays(Convert.ToDouble(incrementString[0]));
            }
        }
        #endregion

        #region BtnEnableEasyTouchClick
        private void BtnEnableEasyTouchClick(object sender, EventArgs e)
        {
            ultraTouchProvider1.Enabled = !ultraTouchProvider1.Enabled;
            btnEnableEasyTouch.Text =
                ultraTouchProvider1.Enabled ? Properties.Resources.Button_Disable_Touch : Properties.Resources.Button_Enable_Touch;
        } 
        #endregion

        #endregion

    }
}