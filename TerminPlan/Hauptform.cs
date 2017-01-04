using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using Infragistics.Win;
using Infragistics.Win.UltraWinSchedule;
using Infragistics.Win.UltraWinGanttView.Grid;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win.UltraWinGanttView.Internal;
using Infragistics.Win.UltraWinToolbars;
using Infragistics.Win.UltraWinSchedule.TaskUI;
using Infragistics.Win.UltraWinGanttView;
using Infragistics.Win.UltraMessageBox;
using Infragistics.Win.UltraWinListView;
using System.Diagnostics;
using Infragistics.Shared;
using Infragistics.Win.Printing;
using System.Threading;

namespace Terminplan
{
    /// <summary>
    /// Class TerminplanForm.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class TerminplanForm : Form
    {
        #region Aufzählungen
        #region GanttViewAktion
        /// <summary> Aufzählung der Aktionen, die durchgeführt werden können. </summary>
        private enum GanttViewAction
        {
            /// <summary> Aufgabe nach rechts einziehen </summary>
            IndentTask,
            /// <summary> Aufgabe nach links harausziehen </summary>
            OutdentTask,
            /// <summary> Termin für die Aufgabe später </summary>
            MoveTaskDateForward,
            /// <summary> Termin für die Aufgabe früher </summary>
            MoveTaskDateBackward,
        }
        #endregion GanttViewAktion

        #region Stardatum verschieben
        /// <summary> Aufzählung Zeitspannen, die ausgewählt werden können. </summary>
        private enum TimeSpanForMoving
        {
            /// <summary> Ein Tag </summary>
            OneDay,
            /// <summary> Eine Woche </summary>
            OneWeek,
            /// <summary> Einen Monat (4 Wochen) </summary>
            FourWeeks,
        }
        #endregion  Stardatum verschieben

        #region Eigenschaften Schriftart
        /// <summary>
        /// Enumeration of font related properties.
        /// </summary>
        private enum FontProperties
        {
            /// <summary> Fettschrift </summary>
            Bold,
            /// <summary> Schrägschrift </summary>
            Italics,
            /// <summary> Unterstrichen </summary>
            Underline,
        }
        #endregion Eigenschaften Schriftart
        #endregion Aufzählungen

        #region Variablen
        /// <summary> Merker für rekursive Zellaktivierung </summary>
        private bool cellActivationRecursionFlag = false;                       // keine rekursive Zellaktivierung
        
        /// <summary> Index des momentanen Farbschemas </summary>
        private int currentThemeIndex;
        
        /// <summary> Der ResourceManager </summary>
        private ResourceManager rm = Terminplan.Properties.Resources.ResourceManager;
        
        /// <summary> Ereignis, wenn der Begrüßungsbildschrm geladen wurde </summary>
        private static ManualResetEvent splashLoadedEvent;
        
        /// <summary> Zeilenhöhe einer Arbeitsaufgabe </summary>
        private const int TaskRowHeight = 30;
        
        /// <summary> Höhe der Aufgabenleiste </summary>
        private const int TaskBarHeight = 20;
        
        /// <summary> Pfad zu den Farbeinstallungen </summary>
        private string[] themePaths;
        #endregion Variablen

        #region Konstruktor
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminplanForm" /> class.
        /// </summary>
        public TerminplanForm()
        {
            splashLoadedEvent = new ManualResetEvent(false);

            ThreadStart threadStart = new ThreadStart(this.ShowSplashScreen);
            Thread thread = new Thread(threadStart);
            thread.Name = "Splash Screen";
            thread.Start();
            splashLoadedEvent.WaitOne();

            // Minimieren der Initialisierungszeit durch Laden der Stilbibliothek
            // vor InitializeComponent(),
            // andernfalls werden alle Metriken nach dem Ändern des Themas neu berechnet.            this.themePaths = Utilities.GetStyleLibraryResourceNames();
            for (int i = 0; i < this.themePaths.Length; i++)
            {
                if (this.themePaths[i].Contains("02"))
                {
                    this.currentThemeIndex = i;
                    break;
                }
            }

            // Eingebettete Ressourcen laden
            Infragistics.Win.AppStyling.StyleManager.Load(Utilities.GetEmbeddedResourceStream(this.themePaths[this.currentThemeIndex]));
            InitializeComponent();
        }
        #endregion Konstruktor

        #region Überschreibungen der Basisklasse
        #region Dispose
        /// <summary> Bereinigung aller verwendeter Ressourcen </summary>
        /// <param name="disposing">true, falls verwaltete Ressourcen entsorgt werden sollen; sonst false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                // Deaktivieren der Ereignisprozedur OnApplicationStyleChanged()
                Infragistics.Win.AppStyling.StyleManager.StyleChanged -= new Infragistics.Win.AppStyling.StyleChangedEventHandler(OnApplicationStyleChanged);

                components.Dispose();
            }
            
            base.Dispose(disposing);
        }
        #endregion Dispose

        #region OnLoad
        /// <summary>
        /// Löst das <see cref="E:System.Windows.Forms.Form.Load" /> Ereignis aus.
        /// </summary>
        /// <param name="e">Ein <see cref="T:System.EventArgs" /> welches die Ereignisdaten enthält.</param>
        protected override void OnLoad(EventArgs e)
        {
            this.OnChangeIcon();                                                  // Farbe anhand des ausgewählten Themes einstellen

            this.OnInitializationStatusChanged(Properties.Resources.Loading);   // Anzeige im Splashscreen aktualisieren
            base.OnLoad(e);

            // Ruft die Daten aus der bereitgestellten XML-Datei ab
            this.OnInitializationStatusChanged(Properties.Resources.Retrieving);    // Daten im Splashscreen aktualisieren
            DataSet dataset = Utilities.GetData("Terminplan.Data.TestDaten.XML");   // Testdaten laden

            // Die eingelesenen Daten an die ultraCalendarInfo anbinden. 
            this.OnInitializationStatusChanged(Properties.Resources.Binding);   // Anzeige im Splashscreen aktualisieren
            this.OnBindProjectData(dataset);                                      // Daten an ultraCalendarInfo anbinden

            // Initialisiert die Kontrols auf dem Formular
            this.OnInitializationStatusChanged(Properties.Resources.Initializing); // Anzeige im Splashscreen aktualisieren
            this.OnColorizeImages();                                              // Farbe der Bilder an das eingestellte Farbschema anpassen
            this.InitializeUI();                                                // Oberfläche initialisieren

            // Ereignisprozedur zum Ändern des Schemas festlegen
            Infragistics.Win.AppStyling.StyleManager.StyleChanged += new Infragistics.Win.AppStyling.StyleChangedEventHandler(OnApplicationStyleChanged);
        }
        #endregion OnLoad

        #region OnShown
        /// <summary>
        /// Löst das <see cref="E:System.Windows.Forms.Form.Shown" /> Ereignis aus.
        /// </summary>
        /// <param name="e">Ein <see cref="T:System.EventArgs" /> welches die Ereignisdaten enthält.</param>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);                                                    // Hauptfenster anzeigen

            // Andere Ereignisse vor dem Auslösen dieses Ereignisses bearbeiten,
            // andernfalls wird das Formular nicht vollständig gezeichnet, 
            // bevor der Splash-Screen geschlossen wird.
            Application.DoEvents();
            
            // Das InitializationComplete-Ereignis auslösen, so dass der SplashScreen geschlossen wird.
            this.OnInitializationComplete();
        }
        #endregion OnShown
        #endregion Überschreibungen der Basisklasse
        
        #region Ereignisprozeduren
        #region OnApplicationStyleChanged
        /// <summary> Behandelt das StyleChanged-Ereignis des Application Styling Managers </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.AppStyling.StyleChangedEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void OnApplicationStyleChanged(object sender, Infragistics.Win.AppStyling.StyleChangedEventArgs e)
        {
            this.ApplicationStyleChanged(sender, e);
        }
        #endregion OnApplicationStyleChanged

        #region OnUltraCalendarInfo1CalendarInfoChanged
        /// <summary> Behandelt das CalendarInfoChanged-Ereignis des ultraCalendarInfo1 Kontrols. </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="CalendarInfoChangedEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void OnUltraCalendarInfo1CalendarInfoChanged(object sender, CalendarInfoChangedEventArgs e)
        {
            this.UltraCalendarInfo1CalendarInfoChanged(sender, e);
        }
        #endregion OnUltraCalendarInfo1CalendarInfoChanged

        #region OnUltraGanttView1ActiveTaskChanging
        /// <summary>
        /// Behandelt das ActiveTaskChanging-Ereignis des ultraGanttView1 Kontrols.
        /// Wird ausgelöst, wenn auf einen anderen Arbeitsinhalt gewechselt wird.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="ActiveTaskChangingEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void OnUltraGanttView1ActiveTaskChanging(object sender, ActiveTaskChangingEventArgs e)
        {
            this.UltraGanttView1ActiveTaskChanging(sender, e);
        }

        #endregion OnUltraGanttView1ActiveTaskChanging

        #region OnUltraGanttView1CellActivating
        /// <summary> 
        /// Behandelt das CellActivating-Ereignis des ultraGanttView1 Kontrols. 
        /// Wird aufgerufen, wenn eine Zelle aktiviert wird.
        /// </summary>  
        /// <remarks>
        /// Es werden die Buttons für die Einstellung der Schrift an die ausgewählte Zelle angepasst.
        /// </remarks>    
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="CellActivatingEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void OnUltraGanttView1CellActivating(object sender, CellActivatingEventArgs e)
        {
            this.UltraGanttView1CellActivating(sender, e);
        }
        #endregion OnUltraGanttView1CellActivating

        #region OnUltraGanttView1CellDeactivating
        /// <summary>
        /// Behandelt das CellDeactivating-Ereignis des ultraGanttView1 Kontrols.
        /// Wird aufgerufen, wenn eine Zelle deaktiviert wird
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.UltraWinGanttView.CellDeactivatingEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void OnUltraGanttView1CellDeactivating(object sender, Infragistics.Win.UltraWinGanttView.CellDeactivatingEventArgs e)
        {
            this.UltraGanttView1CellDeactivating(sender, e);
        }
        #endregion OnUltraGanttView1CellDeactivating

        #region OnUltraGanttView1TaskAdded
        /// <summary>
        /// Behandelt das TaskAdded-Ereignis des ultraGanttView1 Kontrols.
        /// Wird aufgerufen, wenn ein neuer Arbeitsinhalt hinzugefügt wurde.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="TaskAddedEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void OnUltraGanttView1TaskAdded(object sender, TaskAddedEventArgs e)
        {
            this.UltraGanttView1TaskAdded(sender, e);
        }
        #endregion OnUltraGanttView1TaskAdded

        #region OnUltraGanttView1TaskDeleted
        /// <summary>
        /// Behandelt das TaskDeleted-Ereignis des ultraGanttView1 Kontrols.
        /// Wird aufgerufen, wenn ein Arbeitsinhalt gelöscht wurde.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="TaskDeletedEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void OnUltraGanttView1TaskDeleted(object sender, TaskDeletedEventArgs e)
        {
            this.UltraGanttView1TaskDeleted(sender, e);
        }
        #endregion OnUltraGanttView1TaskDeleted

        #region OnUltraGanttView1TaskDialogDisplaying
        /// <summary>
        /// Anzeige des Dialogs für Arbeitsinhalte.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TaskDialogDisplayingEventArgs" /> instance containing the event data.</param>
        private void OnUltraGanttView1TaskDialogDisplaying(object sender, TaskDialogDisplayingEventArgs e)
        {
            // Zeigt die Seite mit den Arbeitsinhalten an, wenn der Dialog für Arbeitsinhalte
            // angezeigt wird
            e.Dialog.SelectPage(TaskDialogPage.Notes);
        }
        #endregion OnUltraGanttView1TaskDialogDisplaying

        #region OnUltraToolbarsManager1PropertyChanged
        /// <summary>
        /// Behandelt das PropertyChanged-Ereignis des ultraToolbarsManager1 Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.PropertyChangedEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void OnUltraToolbarsManager1PropertyChanged(object sender, Infragistics.Win.PropertyChangedEventArgs e)
        {
            this.UltraToolbarsManager1PropertyChanged(sender, e);
        }
        #endregion OnUltraToolbarsManager1PropertyChanged

        #region OnUltraToolbarsManagerToolClick
        /// <summary>
        /// Behandelt das ToolClick-Ereignis of the ultraToolbarsManager1 control.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.UltraWinToolbars.ToolClickEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraToolbarsManagerToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            this.UltraToolbarsManagerToolClick(sender, e);
        }
        #endregion OnUltraToolbarsManagerToolClick

        #region OnUltraToolbarsManager1ToolValueChanged
        /// <summary>
        /// Behandelt das ToolValueChanged-Ereignis des ultraToolbarsManager1 Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">The <see cref="ToolEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraToolbarsManager1ToolValueChanged(object sender, ToolEventArgs e)
        {
            this.UltraToolbarsManager1ToolValueChanged(sender, e);
        }
        #endregion OnUltraToolbarsManager1ToolValueChanged

        #region OnUltraTouchProvider1PropertyChanged
        /// <summary>
        /// Behandelt das PropertyChanged-Ereignis des ultraTouchProvider1 Kontrols.
        /// </summary>
        /// <remarks>
        /// Dient zum automatischen Größeneinstellung der Gridspalten
        /// </remarks>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.PropertyChangedEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraTouchProvider1PropertyChanged(object sender, Infragistics.Win.PropertyChangedEventArgs e)
        {
            this.UltraTouchProvider1PropertyChanged(sender, e);
        }
        #endregion OnUltraTouchProvider1PropertyChanged
        #endregion Ereignisprozeduren

        #region Eigenschaften
        #region SplashLoadedEvent
        /// <summary>
        /// Holt das Ereignis, dass der Splashscreen geladen wurde.
        /// </summary>
        /// <value>Das Ereignis des SplashScreens.</value>
        internal static ManualResetEvent SplashLoadedEvent
        {
            get
            {
                return splashLoadedEvent;
            }
        }
        #endregion SplashLoadedEvent
        #endregion Eigenschaften

        #region Methoden
        #region OnAddNewTask
        /// <summary>
        /// Fügt dem GanttView einen neuen Arbeitsinhalt hinzu
        /// </summary>
        /// <param name="addAtSelectedRow">
        /// Füget bei true einen neuen Arbeitsinhalt an der ausgewählten Zeile ein, 
        /// bei false am unteren Rand des ganttViews
        /// </param>
        private void OnAddNewTask(bool addAtSelectedRow)
        {
            this.AddNewTask(addAtSelectedRow);
        }
        #endregion OnAddNewTask

        #region OnBindProjectData
        /// <summary>
        /// Bindet die Daten an die UltraCalendarInfo
        /// </summary>
        /// <param name="data">Die Daten.</param>
        private void OnBindProjectData(DataSet data)
        {
            this.BindProjectData(data);
        }
        #endregion OnBindProjectData

        #region OnChangeIcon
        /// <summary>
        /// Ändert das Symbol.
        /// </summary>
        private void OnChangeIcon()
        {
            this.ChangeIcon();
        }
        #endregion OnChangeIcon

        #region OnColorizeImages
        /// <summary>
        /// Färbt die Bilder in den großen und kleinen Bildlisten mit den Standardbildern 
        /// und platziert die neuen Bilder in den farbigen Bildlisten.
,        /// </summary>
        private void OnColorizeImages()
        {
            this.ColorizeImages();
        }
        #endregion OnColorizeImages

        #region OnDeleteTask
        /// <summary>
        /// Löscht den aktiven Arbeitsinhalt
        /// </summary>
        private void OnDeleteTask()
        {
            this.DeleteTask();
        }
        #endregion OnDeleteTask

        #region InitializeUI

        /// <summary>
        /// Initializes the UI.
        /// </summary>
        private void InitializeUI()
        {
            // Populate the themes list
            int selectedIndex = 0;
            ListTool themeTool = (ListTool)this.ultraToolbarsManager1.Tools["ThemeList"];
            foreach (string resourceName in this.themePaths)
            {
                ListToolItem item = new ListToolItem(resourceName);
                string libraryName = resourceName.Replace(".isl", string.Empty);
                item.Text = libraryName.Remove(0, libraryName.LastIndexOf('.') + 1);
                themeTool.ListToolItems.Add(item);

                if (item.Text.Contains("02"))
                    selectedIndex = item.Index;
            }
            themeTool.SelectedItemIndex = selectedIndex;

            // Select the proper touch mode list item.
            ((ListTool)this.ultraToolbarsManager1.Tools["TouchMode"]).SelectedItemIndex = 0;

            // Creates a valueList with various font sizes
            this.PopulateFontSizeValueList();
            ((ComboBoxTool)(this.ultraToolbarsManager1.Tools["FontSize"])).SelectedIndex = 0;
            ((FontListTool)this.ultraToolbarsManager1.Tools["FontList"]).SelectedIndex = 0;
            this.UpdateFontToolsState(false);

            Control control = new AboutControl();
            control.Visible = false;
            control.Parent = this;
            ((PopupControlContainerTool)this.ultraToolbarsManager1.Tools["About"]).Control = control;

            // Autosize the columns so all the data is visible.
            this.ultraGanttView1.PerformAutoSizeAllGridColumns();

            // Colorize the images to match the current theme.
            this.OnColorizeImages();

            this.ultraToolbarsManager1.Ribbon.FileMenuButtonCaption = Properties.Resources.ribbonFileTabCaption;
        }

        #endregion //InitializeUI

        #region MoveTask

        /// <summary>
        /// Moves start and end dates of the task backward or foward by a specific timespan
        /// </summary>
        /// <param name="action">Enumeration of ganttView actions supported</param>
        /// <param name="moveTimeSpan">TimeSpan for moving the start and end dates of the task</param>
        private void MoveTask(GanttViewAction action, TimeSpanForMoving moveTimeSpan)
        {
            Task activeTask = this.ultraGanttView1.ActiveTask;

            if (activeTask != null && activeTask.IsSummary == false)
            {
                switch (action)
                {
                    case GanttViewAction.MoveTaskDateForward:
                        {
                            switch (moveTimeSpan)
                            {
                                case TimeSpanForMoving.OneDay:
                                    activeTask.StartDateTime = activeTask.StartDateTime.AddDays(1);
                                    break;
                                case TimeSpanForMoving.OneWeek:
                                    activeTask.StartDateTime = activeTask.StartDateTime.AddDays(7);
                                    break;
                                case TimeSpanForMoving.FourWeeks:
                                    activeTask.StartDateTime = activeTask.StartDateTime.AddDays(28);
                                    break;
                            }
                        }
                        break;

                    case GanttViewAction.MoveTaskDateBackward:
                        {
                            switch (moveTimeSpan)
                            {
                                case TimeSpanForMoving.OneDay:
                                    activeTask.StartDateTime = activeTask.StartDateTime.Subtract(TimeSpan.FromDays(1));
                                    break;
                                case TimeSpanForMoving.OneWeek:
                                    activeTask.StartDateTime = activeTask.StartDateTime.Subtract(TimeSpan.FromDays(7));
                                    break;
                                case TimeSpanForMoving.FourWeeks:
                                    activeTask.StartDateTime = activeTask.StartDateTime.Subtract(TimeSpan.FromDays(28));
                                    break;
                            }
                        }
                        break;
                }
            }
        }
        #endregion  // MoveTask

        #region PerformIndentOrOutdent

        /// <summary>
        /// Perfoms indent or outdent on active task
        /// </summary>
        /// <param name="action">Action to be performed(indent or outdent)</param>
        private void PerformIndentOrOutdent(GanttViewAction action)
        {
            Task activeTask = this.ultraGanttView1.ActiveTask;

            try
            {
                if (activeTask != null)
                {
                    switch (action)
                    {
                        case GanttViewAction.IndentTask:
                            if (activeTask.CanIndent())
                                activeTask.Indent();
                            break;
                        case GanttViewAction.OutdentTask:
                            if (activeTask.CanOutdent())
                                activeTask.Outdent();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                UltraMessageBoxManager.Show(ex.Message, rm.GetString("MessageBox_Error"));
            }
        }
        #endregion // PerformIndentOrOutdent

        #region PopulateFontSizeValueList

        /// <summary>
        /// Populates the list of font sizes
        /// </summary>
        private void PopulateFontSizeValueList()
        {
            List<float> fontSizeList = new List<float> ( new float [] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72}); 
            foreach(float i in fontSizeList)
            {
               ((ComboBoxTool)(this.ultraToolbarsManager1.Tools["FontSize"])).ValueList.ValueListItems.Add(i);
            }
        }
        #endregion // PopulateFontSizeValueList

        #region SetTaskPercentage

        /// <summary>
        /// Assigns a percentage complete value to the active task
        /// </summary>
        /// <param name="percent">Percentage to be assigned to the active task</param>
        private void SetTaskPercentage(float percent)
        {
            Task activeTask = this.ultraGanttView1.ActiveTask;
            try
            {
                if (activeTask != null)
                {
                    activeTask.PercentComplete = percent;
                }
            }
            catch (TaskException ex)
            {
                UltraMessageBoxManager.Show(ex.Message, rm.GetString("MessageBox_Error"));
            }
        }
        #endregion // SetTaskPercentage

        #region SetTextBackColor

        /// <summary>
        /// Updates size of the background color of the active cell depending upon the color
        /// selected from the PopupColorPickerTool.
        /// </summary>
        private void SetTextBackColor()
        {
            Color fontBGColor = ((PopupColorPickerTool)this.ultraToolbarsManager1.Tools["Font_BackColor"]).SelectedColor;
            Task activeTask = this.ultraGanttView1.ActiveTask;
            if (activeTask != null)
            {
                TaskField? activeField = this.ultraGanttView1.ActiveField;
                if (activeField.HasValue)
                {
                    activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance.BackColor = fontBGColor;
                }
            }
        }
        #endregion //SetTextBackColor

        #region SetTextForeColor

        /// <summary>
        /// Updates fore color of the text in the active cell depending upon the color
        /// selected from the PopupColorPickerTool.
        /// </summary>
        private void SetTextForeColor()
        {
            Color fontColor = ((PopupColorPickerTool)this.ultraToolbarsManager1.Tools["Font_ForeColor"]).SelectedColor;
            Task activeTask = this.ultraGanttView1.ActiveTask;
            if (activeTask != null)
            {
                TaskField? activeField = this.ultraGanttView1.ActiveField;
                if (activeField.HasValue)
                {
                    activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance.ForeColor = fontColor;
                }
            }
        }
        #endregion // SetTextForeColor

        #region ShowSplashScreen

        /// <summary>
        /// Shows the splash screen.
        /// </summary>
        private void ShowSplashScreen()
        {
            SplashScreen splashScreen = new SplashScreen();
            Application.Run(splashScreen);
            Application.ExitThread();
        }
        #endregion //ShowSplashScreen

        #region UpdateFontToolsState

        /// <summary>
        /// Updates the Enabled property for tools in the Font RibbonGroup
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        private void UpdateFontToolsState(bool enabled)
        {
            Utilities.SetRibbonGroupToolsEnabledState(this.ultraToolbarsManager1.Ribbon.Tabs[0].Groups["RibbonGrp_Font"], enabled);
        }

        #endregion // UpdateFontToolsState

        #region UpdateFontName

        /// <summary>
        /// Updates the font depending upon the value selected from the FontListTool.
        /// </summary>
        private void UpdateFontName()
        {
            string fontName = ((FontListTool)this.ultraToolbarsManager1.Tools["FontList"]).Text;
            Task activeTask = this.ultraGanttView1.ActiveTask;
            if (activeTask != null)
            {
                TaskField? activeField = this.ultraGanttView1.ActiveField;
                if (activeField.HasValue)
                {
                    activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance.FontData.Name = fontName;
                }
            }
        }
        #endregion // UpdateFontName

        #region UpdateFontSize

        /// <summary>
        /// Updates size of the font depending upon the value selected from the ComboBoxTool.
        /// </summary>
        private void UpdateFontSize()
        {
            ValueListItem item = (ValueListItem)((ComboBoxTool)(this.ultraToolbarsManager1.Tools["FontSize"])).SelectedItem;
            if (item != null)
            {
                float fontSize = (float)item.DataValue;
                Task activeTask = this.ultraGanttView1.ActiveTask;
                if (activeTask != null)
                {
                    TaskField? activeField = this.ultraGanttView1.ActiveField;
                    if (activeField.HasValue)
                    {
                        activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance.FontData.SizeInPoints = fontSize;
                    }
                }
            }
        }
        #endregion // UpdateFontSize

        #region UpdateFontProperty

        /// <summary>
        /// Method to update various font properties.
        /// </summary>
        /// <param name="propertyToUpdate">Enumeration of font related properties</param>
        private void UpdateFontProperty(FontProperties propertyToUpdate)
        {
            Task activeTask = this.ultraGanttView1.ActiveTask;
            if (activeTask != null)
            {
                TaskField? activeField = this.ultraGanttView1.ActiveField;
                if (activeField.HasValue)
                {
                    FontData activeTaskActiveCellFontData = activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance.FontData;
                    switch (propertyToUpdate)
                    {
                        case FontProperties.Bold:
                            activeTaskActiveCellFontData.Bold = Utilities.ToggleDefaultableBoolean(activeTaskActiveCellFontData.Bold);
                            break;
                        case FontProperties.Italics:
                            activeTaskActiveCellFontData.Italic = Utilities.ToggleDefaultableBoolean(activeTaskActiveCellFontData.Italic);
                            break;
                        case FontProperties.Underline:
                            activeTaskActiveCellFontData.Underline = Utilities.ToggleDefaultableBoolean(activeTaskActiveCellFontData.Underline);
                            break;
                    }
                }
            }
            this.cellActivationRecursionFlag = false;
        }
        #endregion // UpdateFontProperty

        #region UpdateTasksToolsState

        /// <summary>
        /// Verifies the state of the tools in the Tasks RibbonGroup.
        /// </summary>
        /// <param name="activeTask">The active task.</param>
        private void UpdateTasksToolsState(Task activeTask)
        {
            RibbonGroup group = this.ultraToolbarsManager1.Ribbon.Tabs["Ribbon_Task"].Groups["RibbonGrp_Tasks"];

            if (activeTask != null)
            {
                // For summary tasks, the completion percentage is based on it's child tasks
                Utilities.SetRibbonGroupToolsEnabledState(group, !activeTask.IsSummary);

                group.Tools["Tasks_MoveLeft"].SharedProps.Enabled = activeTask.CanOutdent();
                group.Tools["Tasks_MoveRight"].SharedProps.Enabled = activeTask.CanIndent();
                group.Tools["Tasks_Delete"].SharedProps.Enabled = true;
            }
            else
            {
                Utilities.SetRibbonGroupToolsEnabledState(group, false);
            }
        }

        #endregion // UpdateTasksToolsState

        #region UpdateToolsRequiringActiveTask

        /// <summary>
        /// Überprüft den Status aller Werkzeuge, die eine aktiven Arbeitsinhalt erfordern.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        private void UpdateToolsRequiringActiveTask(bool enabled)
        {
            this.ultraToolbarsManager1.Tools["Tasks_Delete"].SharedProps.Enabled = enabled;
            this.ultraToolbarsManager1.Tools["Insert_Milestone"].SharedProps.Enabled = enabled;
            this.ultraToolbarsManager1.Tools["Properties_TaskInformation"].SharedProps.Enabled = enabled;
            this.ultraToolbarsManager1.Tools["Properties_Notes"].SharedProps.Enabled = enabled;
            this.ultraToolbarsManager1.Tools["Insert_Task_TaskAtSelectedRow"].SharedProps.Enabled = enabled;
        }

        #endregion // UpdateToolsRequiringActiveTask

        #endregion // Methods

        #region SplashScreen Events

        #region Events

        /// <summary>
        /// Fired when the staus of the form initialization has changed.
        /// </summary>
        internal static event Terminplan.SplashScreen.InitializationStatusChangedEventHandler InitializationStatusChanged;

        /// <summary>
        /// Fired when the staus of the form initialization has completed.
        /// </summary>
        internal static event EventHandler InitializationComplete;

        /// <summary>
        /// The initialization completed
        /// </summary>
        bool initializationCompleted = false;

        #region OnInitializationComplete
        /// <summary>
        /// Called when [initialization complete].
        /// </summary>
        protected virtual void OnInitializationComplete()
        {
            if (this.initializationCompleted == false)
            {
                this.initializationCompleted = true;

                if (TerminplanForm.InitializationComplete != null)
                    TerminplanForm.InitializationComplete(this, EventArgs.Empty);
            }
        }
        #endregion OnInitializationStatusChanged

        #region OnInitializationStatusChanged
        /// <summary>
        /// Called when [initialization status changed].
        /// </summary>
        /// <param name="status">The status.</param>
        protected virtual void OnInitializationStatusChanged(string status)
        {
            if (TerminplanForm.InitializationStatusChanged != null)
                TerminplanForm.InitializationStatusChanged(this, new Terminplan.SplashScreen.InitializationStatusChangedEventArgs(status));
        }

        /// <summary>
        /// Called when [initialization status changed].
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="showProgressBar">if set to <c>true</c> [show progress bar].</param>
        /// <param name="percentComplete">The percent complete.</param>
        protected virtual void OnInitializationStatusChanged(string status, bool showProgressBar, int percentComplete)
        {
            if (TerminplanForm.InitializationStatusChanged != null)
                TerminplanForm.InitializationStatusChanged(this, new Terminplan.SplashScreen.InitializationStatusChangedEventArgs(status, showProgressBar, percentComplete));
        }
        #endregion OnInitializationStatusChanged

        #endregion Events		

        #endregion //SplashScreen Events
    }
}
