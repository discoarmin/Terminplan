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
    using Infragistics.Win;
    using Infragistics.Win.AppStyling;
    using Infragistics.Win.UltraWinGanttView;
    using Infragistics.Win.UltraWinSchedule;
    using Infragistics.Win.UltraWinSchedule.TaskUI;
    using Infragistics.Win.UltraWinToolbars;
    using System;
    using System.Data;
    using System.IO;
    using System.Resources;
    using System.Windows.Forms;

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

        /// <summary>Delegate zum Melden, dass der Begrüßungsbildschirm geschlossen werden kann </summary>
        public delegate void SplashScreenCloseDelegate();

        /// <summary>Delegate zum Schließen des Begrüßungsbildschrms</summary>
        public delegate void CloseDelagate();

        /// <summary>Dataset zur Aufnahme der Daten des Terminplans</summary>
        public DataSet datasetTp;

        /// <summary> Merker für rekursive Zellaktivierung </summary>
        private bool cellActivationRecursionFlag; // Merker rekursive Zellaktivierung

        /// <summary> Index des momentanen Farbschemas </summary>
        private int currentThemeIndex;

        /// <summary> Der ResourceManager </summary>
        private ResourceManager rm = Properties.Resources.ResourceManager;

        /// <summary> Zeilenhöhe einer Arbeitsaufgabe </summary>
        private const int TaskRowHeight = 30;

        /// <summary> Höhe der Aufgabenleiste </summary>
        // ReSharper disable once UnusedMember.Local
        private const int TaskBarHeight = 20;

        /// <summary> Pfad zu den Farbeinstallungen </summary>
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private string[] themePaths;

        public StyleManager StyleManagerIntern;
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
                if (!this.themePaths[i].Contains(@"04"))
                {
                    continue;
                }

                this.currentThemeIndex = i;
                break;
            }

            // Eingebettete Ressourcen laden
            Infragistics.Win.AppStyling.StyleManager.Load(DienstProgramme.GetEmbeddedResourceStream(this.themePaths[this.currentThemeIndex]));
            this.SetResourceStrings();
            this.InitializeComponent();
        }

        #endregion Konstruktor

        #region Überschreibungen der Basisklasse

        #region Dispose

        /// <summary> Bereinigung aller verwendeter Ressourcen </summary>
        /// <param name="disposing">true, falls verwaltete Ressourcen entsorgt werden sollen; sonst false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                // Deaktivieren der Ereignisprozedur OnApplicationStyleChanged()
                Infragistics.Win.AppStyling.StyleManager.StyleChanged -= this.OnApplicationStyleChanged;

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
            this.OnInitializationStatusChanged(Properties.Resources.Loading);   // Anzeige im Splashscreen aktualisieren
            base.OnLoad(e);

            var splitterWeite = 10;                                             // Zum Einstellen des Splitters
            var col = this.ultraGanttView1.GridSettings.ColumnSettings.Values;
            var schluessel = string.Empty;
             
            // Überschriften einstellen
            foreach (var de in col)
            {
                // Arbeitsinhalt oder Aufgabe
                if (de.Key.ToLower() == @"name")                        
                {
                    de.Text = "Arbeitsinhalt/Aufgabe";
                    de.Visible = DefaultableBoolean.True;                    
                    splitterWeite += de.Width;                                  // Breite der Spalte hinzuaddieren
                }

                // Dauer
                if (de.Key.ToLower() == @"duration")
                {
                    de.Text = "Dauer";
                    de.Visible = DefaultableBoolean.True;
                    splitterWeite += de.Width;                                  // Breite der Spalte hinzuaddieren
                }

                // Start
                if (de.Key.ToLower() == @"start")
                {
                    de.Text = "Start";
                    de.Visible = DefaultableBoolean.True;
                    splitterWeite += de.Width;                                  // Breite der Spalte hinzuaddieren
                }

                // Ende
                if (de.Key.ToLower() == @"enddatetime")
                {
                    de.Text = "Ende";
                    de.Visible = DefaultableBoolean.True;
                    splitterWeite += de.Width;                                  // Breite der Spalte hinzuaddieren
                }

                // Fertig in %
                if (de.Key.ToLower() == @"percentcomplete")
                {
                    de.Text = "Status";
                    de.Visible = DefaultableBoolean.True;
                    splitterWeite += de.Width;                                  // Breite der Spalte hinzuaddieren
                }
            }

            // Ruft die Daten aus der bereitgestellten XML-Datei ab
            this.OnInitializationStatusChanged(Properties.Resources.Retrieving); // Daten im Splashscreen aktualisieren
            //datasetTp = DienstProgramme.GetData(Path.Combine(Application.StartupPath, @"Data.TestDatenEST.XML")); // Testdaten laden
            //datasetTp = DienstProgramme.GetData(Path.Combine(Application.StartupPath, @"Data.TestDaten1EST.XML")); // Testdaten laden
            //datasetTp = DienstProgramme.GetData(Path.Combine(Application.StartupPath, @"Data.TestDaten2EST.XML")); // Testdaten laden
            datasetTp = DienstProgramme.GetData(Path.Combine(Application.StartupPath, @"Data.DatenNeuEST.XML")); // Testdaten laden

            // Die eingelesenen Daten an die ultraCalendarInfo anbinden. 
            this.OnInitializationStatusChanged(Properties.Resources.Binding);   // Anzeige im Splashscreen aktualisieren
            this.OnBindArbInhaltData(datasetTp);                                // Daten an ultraCalendarInfo anbinden

            // Initialisiert die Kontrols auf dem Formular
            this.OnInitializationStatusChanged(Properties.Resources.Initializing); // Anzeige im Splashscreen aktualisieren
            this.OnColorizeImages();                                            // Farbe der Bilder an das eingestellte Farbschema anpassen
            this.OnInitializeUi();                                              // Oberfläche initialisieren

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
        private void OnUltraGanttView1CellDeactivating(object sender, CellDeactivatingEventArgs e)
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
        // ReSharper disable once UnusedMember.Local
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

        #region OnInitializeUI                            
        /// <summary>
        /// Initialisiert die Oberfläche.
        /// </summary>
        private void OnInitializeUi()
        {
            this.InitializeUi(); // Oberfläche initialisieren
        }
        #endregion OnInitializeUI

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
            DienstProgramme.SetRibbonGroupToolsEnabledState(this.ultraToolbarsManager1.Ribbon.Tabs[0].Groups[@"RibbonGrp_Font"], enabled);
        }
        #endregion OnUpdateFontToolsState

        #region Datei laden
        private void LadeDatei()
        {
            // Dialog zum Öffnen einer Datei anzeigen
            OpenFileDialog openFileDialog1 = new OpenFileDialog()
            {
                Filter = "XML Dateien|*.xml",
                Title = "Terminplan öffnen"
            };

            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.components = new System.ComponentModel.Container();
                this.ultraCalendarInfo1 = new Infragistics.Win.UltraWinSchedule.UltraCalendarInfo(this.components);

                // Ruft die Daten aus der bereitgestellten XML-Datei ab
                datasetTp = new DataSet();
                datasetTp = DienstProgramme.GetData(openFileDialog1.FileName);      // ausgewählte Daten ladenn laden

                // Die eingelesenen Daten an die ultraCalendarInfo anbinden. 
                this.OnBindArbInhaltData(datasetTp);                                // Daten an ultraCalendarInfo anbinden
            }

        }

        #endregion Datei ladedn
        #endregion Methoden

        #region SplashScreen Ereignisse
        #region OnInitializationStatusChanged                
        /// <summary> Wird ausgelöst, wenn sich der Status der Initialisierung der Hauptform geändert hat. </summary>
        internal static event SplashScreen.InitializationStatusChangedEventHandler InitializationStatusChanged;
        #endregion OnInitializationStatusChanged

        #region OnInitializationStatusChanged
        /// <summary> Wird aufgerufen, wenn sich der Status der Initialisierung der Hauptform ändert </summary>
        /// <param name="status">Der Status.</param>
        protected virtual void OnInitializationStatusChanged(string status)
        {
            // Nur bearbeiten, falls das Ereignis existiert
            // Ereignis auslösen
            if (InitializationStatusChanged != null)
            {
                InitializationStatusChanged.Invoke(this, new SplashScreen.InitializationStatusChangedEventArgs(status));
            }
       }
        #endregion OnInitializationStatusChanged

        /// <summary>
        /// Wird aufgerufen, wenn die Größe des Formulars geändert wurde.
        /// </summary>
        /// <param name="sender">Das aufrufende Element</param>
        /// <param name="e">Die <see cref="EventArgs"/> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnTerminPlanFormClientSizeChanged(object sender, EventArgs e)
        {
            var breite = this.ultraGanttView1.GridAreaWidth;
            var gesamtbreite = this.Width;
            var teiler = Math.Abs((float)breite / (float)gesamtbreite);
            var splitterWeite = (float)gesamtbreite * teiler;
        }

        /// <summary>
        /// Wird aufgerufen, wenn das Panel mit den Daten neu gezeichnet wird.
        /// </summary>
        /// <param name="sender">Das aufrufende Element</param>
        /// <param name="e">Die <see cref="PaintEventArgs"/> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnForm1FillPanelPaint(object sender, PaintEventArgs e)
        {
            // Bildschirmauflösung ermitteln. Dazu muss ermittelt werden, auf welchem Monitor
            // die Anwendung läuft
            var currentScreen = Screen.FromControl(this);                       // momentan benutzter Monitor ermitteln
            var resBreite = currentScreen.Bounds.Width;                         // Breite des Monitors
            var resHoehe = currentScreen.Bounds.Height;                         // Höhe des Monitors

            var fontGroesse = this.ultraGanttView1.GridSettings.RowAppearance.FontData.SizeInPoints;
            var headerGroesse = this.ultraGanttView1.GridSettings.ColumnHeaderAppearance.FontData.SizeInPoints;

            if (resBreite < 1024)
            {
                fontGroesse = 8;
                headerGroesse = 9;

                this.ultraGanttView1.GridSettings.RowAppearance.FontData.SizeInPoints = fontGroesse;
                this.ultraGanttView1.GridSettings.ColumnHeaderAppearance.FontData.SizeInPoints = this.FontHeight;
            }

            this.ultraGanttView1.Appearance.FontData.SizeInPoints = fontGroesse;
           var breite = this.ultraGanttView1.GridAreaWidth;

            var col = this.ultraGanttView1.GridSettings.ColumnSettings.Values;
            var schluessel = string.Empty;
            var panalWeite = 0;

            // Überschriften einstellen
            foreach (var de in col)
            {
                var headerBreite = de.Text.Length * fontGroesse;
                if ((de.Visible == DefaultableBoolean.True) && (de.Width < headerBreite))
                {
                    de.Width = Convert.ToInt32(headerBreite);
                }

                // Alle vorhandenen Spalten analysieren
                switch (de.Key.ToLower())
                {
                    case @"name":                                               // Name (Arbeitsinhalt oder Aufgabe)
                        panalWeite += de.Width;                                 // Breite der Spalte hinzuaddieren
                        break;
                    case @"duration":                                           // Dauer
                        panalWeite += de.Width;                                 // Breite der Spalte hinzuaddieren
                        break;
                    case @"start":                                              // Startdatum
                        panalWeite += de.Width;                                 // Breite der Spalte hinzuaddieren
                        break;
                    case @"enddatetime":                                        // Endedatum
                        panalWeite += de.Width;                                 // Breite der Spalte hinzuaddieren
                        break;
                    case @"percentcomplete":                                    // Fetiggestellt (Status)
                        panalWeite += de.Width;                                 // Breite der Spalte hinzuaddieren
                        break;
                    default:                                                    // Alle sonstigen Spalten
                        if (de.Visible == DefaultableBoolean.True)
                        {
                            panalWeite += de.Width;                             // Breite der Spalte hinzuaddieren
                        }
                        break;
                }
            }

            this.ultraGanttView1.GridAreaWidth = panalWeite;
            var splitterWeite = (float)panalWeite * 1.5;
            this.ultraGanttView1.GridAreaWidth = Convert.ToInt32(splitterWeite);

            var gesamtbreite = this.Width;
            var teiler = Math.Abs((float)breite / (float)gesamtbreite);
        }

        /// <summary>
        /// Behandelt das ToolClick-Ereignis of the ultraToolbarsManager1 control.
        /// </summary>
        /// <remarks>
        /// Die jeweilige Aktion wird nur durchgeführt, wenn sie nicht schon durchgeführt wurde
        /// </remarks>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.UltraWinToolbars.ToolClickEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void UltraToolbarsManagerToolClick(object sender, ToolClickEventArgs e)
        {
            this.OnUltraToolbarsManagerToolClick(sender, e);
        }
        #endregion Ereignisse		
    }
}
