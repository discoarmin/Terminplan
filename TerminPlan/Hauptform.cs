// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HauptForm.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Definiert die Hauptform.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  06.01.17  br      Grundversion
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Terminplan
{
    using System;
    using System.Data;
    using System.IO;
    using System.Windows.Forms;
    using System.Resources;

    using Infragistics.Win.UltraWinSchedule;
    using Infragistics.Win.UltraWinToolbars;
    using Infragistics.Win.UltraWinSchedule.TaskUI;
    using Infragistics.Win.UltraWinGanttView;

    using System.Threading;

    /// <summary>
    /// Klasse TerminPlanForm (Hauptformular).
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class TerminPlanForm
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

        /// <summary> Delegate zum Melden, dass der Begrüßungsbildschirm geschlossen werden kann </summary>
        public delegate void SplashScreenCloseDelegate();

        /// <summary> Delegate zum Schließen des Begrüßungsbildschrms</summary>
        public delegate void closeDelagate();

        /// <summary> Merker für rekursive Zellaktivierung </summary>
        private bool cellActivationRecursionFlag; // Merker rekursive Zellaktivierung

        /// <summary> Index des momentanen Farbschemas </summary>
        private int currentThemeIndex;

        /// <summary> Der ResourceManager </summary>
        private ResourceManager rm = Properties.Resources.ResourceManager;

        /// <summary> Zeilenhöhe einer Arbeitsaufgabe </summary>
        private const int TaskRowHeight = 30;

        /// <summary> Höhe der Aufgabenleiste </summary>
        private const int TaskBarHeight = 20;

        /// <summary> Pfad zu den Farbeinstallungen </summary>
        private string[] themePaths;

        #endregion Variablen

        #region Konstruktor

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="TerminPlanForm" /> Klasse.
        /// </summary>
        public TerminPlanForm()
        {
            // Minimieren der Initialisierungszeit durch Laden der Stilbibliothek
            // vor InitializeComponent(),
            // andernfalls werden alle Metriken nach dem Ändern des Themas neu berechnet.            
            this.themePaths = DienstProgramme.GetStyleLibraryResourceNames();
            for (var i = 0; i < this.themePaths.Length; i++)
            {
                if (!this.themePaths[i].Contains("04"))
                {
                    continue;
                }

                this.currentThemeIndex = i;
                break;
            }

            // Eingebettete Ressourcen laden
            Infragistics.Win.AppStyling.StyleManager.Load(DienstProgramme.GetEmbeddedResourceStream(this.themePaths[this.currentThemeIndex]));
            this.InitializeComponent();
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

                this.components.Dispose();
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
            this.OnChangeIcon(); // Farbe anhand des ausgewählten Themes einstellen

            this.OnInitializationStatusChanged(Properties.Resources.Loading); // Anzeige im Splashscreen aktualisieren
            base.OnLoad(e);

            // Ruft die Daten aus der bereitgestellten XML-Datei ab
            this.OnInitializationStatusChanged(Properties.Resources.Retrieving); // Daten im Splashscreen aktualisieren
            var dataset = DienstProgramme.GetData(Path.Combine(Application.StartupPath, @"Data.TestDaten.XML")); // Testdaten laden

            // Die eingelesenen Daten an die ultraCalendarInfo anbinden. 
            this.OnInitializationStatusChanged(Properties.Resources.Binding); // Anzeige im Splashscreen aktualisieren
            this.OnBindArbInhaltData(dataset); // Daten an ultraCalendarInfo anbinden

            // Initialisiert die Kontrols auf dem Formular
            this.OnInitializationStatusChanged(Properties.Resources.Initializing); // Anzeige im Splashscreen aktualisieren
            this.OnColorizeImages(); // Farbe der Bilder an das eingestellte Farbschema anpassen
            this.OnInitializeUI(); // Oberfläche initialisieren

            // Ereignisprozedur zum Ändern des Schemas festlegen
            Infragistics.Win.AppStyling.StyleManager.StyleChanged += this.OnApplicationStyleChanged;
        }

        #endregion OnLoad

        #region OnShown

        /// <summary>
        /// Löst das <see cref="E:System.Windows.Forms.Form.Shown" /> Ereignis aus.
        /// </summary>
        /// <param name="e">Ein <see cref="T:System.EventArgs" /> welches die Ereignisdaten enthält.</param>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e); // Hauptfenster anzeigen

            // Andere Ereignisse vor dem Auslösen dieses Ereignisses bearbeiten,
            // andernfalls wird das Formular nicht vollständig gezeichnet, 
            // bevor der Splash-Screen geschlossen wird.
            Application.DoEvents();
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

        #region OnBindArbInhaltData

        /// <summary>
        /// Bindet die Daten an die UltraCalendarInfo
        /// </summary>
        /// <param name="data">Die Daten.</param>
        private void OnBindArbInhaltData(DataSet data)
        {
            this.BindArbInhaltData(data);
        }

        #endregion OnBindArbInhaltData

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
        /// </summary>
        private void OnColorizeImages()
        {
            this.ColorizeImages();
        }

        #endregion OnColorizeImages

        #region OnDeleteTask

        /// <summary>
        /// Löscht den aktiven Arbeitsinhalt, oder die aktive Aufgabe
        /// </summary>
        private void OnDeleteTask()
        {
            this.DeleteTask();
        }

        #endregion OnDeleteTask

        #region OnInitializeUI

        /// <summary>
        /// Initialisiert die Oberfläche.
        /// </summary>
        private void OnInitializeUI()
        {
            this.InitializeUi(); // Oberfläche initialisieren
        }

        #endregion OnInitializeUI

        #region OnMoveTask

        /// <summary>
        /// Verschiebt Start- und Enddatum der Aufgabe rückwärts oder vorwärts um eine bestimmte Zeitspanne
        /// </summary>
        /// <param name="action">Aufzählung der unterstützten ganttView-Aktionen</param>
        /// <param name="moveTimeSpan">Zeitspanne zum Verschieben des Start- und Enddatums der Aufgabe</param>
        private void OnMoveTask(GanttViewAction action, TimeSpanForMoving moveTimeSpan)
        {
            this.MoveTask(action, moveTimeSpan);
        }

        #endregion  OnMoveTask

        #region OnPerformIndentOrOutdent

        /// <summary>
        /// Führt Einrückung oder Auslagerung der aktiven Aufgabe oder des aktivev Arbeitsinhalts durch
        /// </summary>
        /// <param name="action">die auszuführende Aktion(Einrückung oder Auslagerung)</param>
        private void OnPerformIndentOrOutdent(GanttViewAction action)
        {
            this.PerformIndentOrOutdent(action);
        }

        #endregion OnPerformIndentOrOutdent

        #region OnPopulateFontSizeValueList

        /// <summary>
        /// Liste mit den Schriftgrößen erstellen
        /// </summary>
        private void OnPopulateFontSizeValueList()
        {
            this.PopulateFontSizeValueList();
        }

        #endregion OnPopulateFontSizeValueList

        #region OnSetTaskPercentage

        /// <summary>
        /// Weist der aktiven Aufgabe oder der aktiven Arbeitsanweisung einen Prozentsatz des Fertigungsgrads zu       
        /// </summary>
        /// <param name="prozentSatz">der zuzuweisende Prozentsatz</param>
        private void OnSetTaskPercentage(float prozentSatz)
        {
            this.SetTaskPercentage(prozentSatz);
        }

        #endregion OnSetTaskPercentage

        #region OnSetTextBackColor

        /// <summary>
        /// Aktualisiert den Wert der Hintergrundfarbe des Textes in der aktiven Zelle abhängig von der 
        /// im PopupColorPickerTool ausgewählten Farbe.
        /// </summary>
        private void OnSetTextBackColor()
        {
            this.SetTextBackColor();
        }

        #endregion OnSetTextBackColor

        #region OnSetTextForeColor

        /// <summary>
        /// Aktualisiert den Wert der Vordergrundfarbe des Textes in der aktiven Zelle abhängig von der 
        /// im PopupColorPickerTool ausgewählten Farbe.
        /// </summary>
        private void OnSetTextForeColor()
        {
            this.SetTextForeColor();
        }

        #endregion OnSetTextForeColor

        #region OnUpdateFontToolsState

        /// <summary>
        /// Aktualisiert die Enabled-Eigenschaft für Werkzeuge in der RibbonGruppe "RibbonGrp_Font"
        /// </summary>
        /// <param name="enabled">falls auf <c>true</c> gesetzt ist, freigeben.</param>
        private void OnUpdateFontToolsState(bool enabled)
        {
            DienstProgramme.SetRibbonGroupToolsEnabledState(this.ultraToolbarsManager1.Ribbon.Tabs[0].Groups["RibbonGrp_Font"], enabled);
        }

        #endregion OnUpdateFontToolsState

        #region OnUpdateFontName

        /// <summary> Aktualisiert den Namen der Schriftart je nach dem im FontListTool ausgewählten Wert. </summary>
        private void OnUpdateFontName()
        {
            this.UpdateFontName();
        }

        #endregion OnUpdateFontName

        #region OnUpdateFontSize

        /// <summary> Aktualisiert die Schriftgröße je nach dem im ComboBoxTool ausgewählten Wert. </summary>
        private void OnUpdateFontSize()
        {
            this.UpdateFontSize();
        }

        #endregion OnUpdateFontSize

        #region OnUpdateFontProperty

        /// <summary>Methode, um verschiedene Eigenschaften der Schriftart zu aktualisieren</summary>
        /// <param name="propertyToUpdate">Aufzählung von Eigenschaften, welche von der Schriftart abhängig sind</param>
        private void OnUpdateFontProperty(FontProperties propertyToUpdate)
        {
            this.UpdateFontProperty(propertyToUpdate);
        }

        #endregion OnUpdateFontProperty

        #region OnUpdateTasksToolsState

        /// <summary> Überprüft den Status der Werkzeuge in der RibbonGruppe "RibbonGrp_Tasks" </summary>
        /// <param name="activeTask">Der aktive Arbeitsinhalt oder die aktive Aufgabe.</param>
        private void OnUpdateTasksToolsState(Task activeTask)
        {
            this.UpdateTasksToolsState(activeTask);
        }

        #endregion OnUpdateTasksToolsState

        #region OnUpdateToolsRequiringActiveTask

        /// <summary> Überprüft den Status aller Werkzeuge, die eine aktiven Arbeitsinhalt erfordern. </summary>
        /// <param name="enabled">falls auf <c>true</c>gesetzt, wird Werkzeug freigegeben, sonst gesperrt.</param>
        private void OnUpdateToolsRequiringActiveTask(bool enabled)
        {
            this.UpdateToolsRequiringActiveTask(enabled);
        }

        #endregion OnUpdateToolsRequiringActiveTask

        #endregion Methoden

        #region SplashScreen Ereignisse

        #region Ereignisse

        /// <summary> Wird ausgelöst, wenn sich der Status der Initialisierung der Hauptform geändert hat. </summary>
        internal static event Terminplan.SplashScreen.InitializationStatusChangedEventHandler InitializationStatusChanged;

        #endregion OnInitializationStatusChanged

        #region OnInitializationStatusChanged

        /// <summary> Wird aufgerufen, wenn sich der Status der Initialisierung der Hauptform ändert </summary>
        /// <param name="status">Der Status.</param>
        protected virtual void OnInitializationStatusChanged(string status)
        {
            // Nur bearbeiten, falls das Ereignis existier
            if (TerminPlanForm.InitializationStatusChanged != null)
            {
                // Ereignis auslösen
                TerminPlanForm.InitializationStatusChanged(this, new Terminplan.SplashScreen.InitializationStatusChangedEventArgs(status));
            }
        }

        /// <summary>
        /// Called when [initialization status changed].
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="showProgressBar">if set to <c>true</c> [show progress bar].</param>
        /// <param name="percentComplete">The percent complete.</param>
        protected virtual void OnInitializationStatusChanged(string status, bool showProgressBar, int percentComplete)
        {
            if (TerminPlanForm.InitializationStatusChanged != null) TerminPlanForm.InitializationStatusChanged(this, new Terminplan.SplashScreen.InitializationStatusChangedEventArgs(status, showProgressBar, percentComplete));
        }
        #endregion OnInitializationStatusChanged
        #endregion Ereignisse		
    }
}
