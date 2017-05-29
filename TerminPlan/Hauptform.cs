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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms;
    using Infragistics.Win;
    using Infragistics.Win.AppStyling;
    using Infragistics.Win.UltraWinGanttView;
    using Infragistics.Win.UltraWinGrid;
    using Infragistics.Win.UltraWinSchedule;
    using Infragistics.Win.UltraWinSchedule.TaskUI;
    using Infragistics.Win.UltraWinToolbars;
    using PropertyChangedEventArgs = Infragistics.Win.PropertyChangedEventArgs;
    using Resources = Properties.Resources;

    /// <summary>
    /// Klasse TerminPlanForm (Hauptformular).
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class TerminPlanForm
    {
        #region Konstruktor

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="TerminPlanForm" /> Klasse.
        /// </summary>
        public TerminPlanForm()
        {
            // Minimieren der Initialisierungszeit durch Laden der Stilbibliothek
            // vor InitializeComponent(),
            // andernfalls werden alle Metriken nach dem Ändern des Themas neu berechnet.
            this.ThemePaths = DienstProgramme.GetStyleLibraryResourceNames();
            for (var i = 0; i < this.ThemePaths.Length; i++)
            {
                if (!this.ThemePaths[i].Contains(@"04"))
                {
                    continue;
                }

                this.CurrentThemeIndex = i;
                break;
            }

            var culture = new CultureInfo("de-DE");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

            // Eingebettete Ressourcen laden
            StyleManager.Load(DienstProgramme.GetEmbeddedResourceStream(this.ThemePaths[CurrentThemeIndex]));
            SetResourceStrings();
            InitializeComponent();
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
                StyleManager.StyleChanged -= this.OnApplicationStyleChanged;

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
            OnInitializationStatusChanged(Resources.Loading);   // Anzeige im Splashscreen aktualisieren
            //base.OnLoad(e);

            var splitterWeite = 10;                                             // Zum Einstellen des Splitters
            var col = ultraGanttView1.GridSettings.ColumnSettings.Values;
            var schluessel = string.Empty;

            // Überschriften einstellen
            foreach (var de in col)
            {
                // Arbeitsinhalt oder Aufgabe
                if (de.Key.ToLower() == @"name")
                {
                    de.Text = @"Arbeitsinhalt/Aufgabe";
                    de.Visible = DefaultableBoolean.True;
                    splitterWeite += de.Width;                                  // Breite der Spalte hinzuaddieren
                }

                // Dauer
                if (de.Key.ToLower() == @"duration")
                {
                    de.Text = @"Dauer";
                    de.Visible = DefaultableBoolean.True;
                    splitterWeite += de.Width;                                  // Breite der Spalte hinzuaddieren
                }

                // Start
                if (de.Key.ToLower() == @"start")
                {
                    de.Text = @"Start";
                    de.Visible = DefaultableBoolean.True;
                    splitterWeite += de.Width;                                  // Breite der Spalte hinzuaddieren
                }

                // Ende
                if (de.Key.ToLower() == @"enddatetime")
                {
                    de.Text = @"Ende";
                    de.Visible = DefaultableBoolean.True;
                    splitterWeite += de.Width;                                  // Breite der Spalte hinzuaddieren
                }

                // Fertig in %
                if (de.Key.ToLower() == @"percentcomplete")
                {
                    de.Text = @"Status";
                    de.Visible = DefaultableBoolean.True;
                    splitterWeite += de.Width;                                  // Breite der Spalte hinzuaddieren
                }
            }

            // Ruft die Daten aus der bereitgestellten XML-Datei ab
            OnInitializationStatusChanged(Resources.Retrieving);                // Daten im Splashscreen aktualisieren
            var wasLaden = new WasLaden();
            if (wasLaden.ShowDialog() == DialogResult.OK)
            {
                var dateiName = string.Empty;
                if (wasLaden.Auswahl == 1)
                {
                    // Bestehenden Terminplan laden
                    dateiName = DienstProgramme.OeffneXmlDatei();               // Öffnen-Dialog anzeigen
                    if (dateiName != string.Empty)
                    {
                        this.DatasetTp = DienstProgramme.GetData(dateiName);    // bestehenden Terminplan laden
                        PrjName = Path.GetFileNameWithoutExtension(dateiName);  // Der Dateiname ist der Projektname
                    }
                }
                else
                {
                    ErstelleNeuesProjekt();                                     // Neuen Terminplan erzeugen
                    OnInitializationStatusChanged(Resources.Binding);           // Anzeige im Splashscreen aktualisieren
                }
            }
            //DatasetTp = DienstProgramme.GetData(Path.Combine(Application.StartupPath, @"Data.TestDatenEST.XML")); // Testdaten laden
            //DatasetTp = DienstProgramme.GetData(Path.Combine(Application.StartupPath, @"Data.TestDaten1EST.XML")); // Testdaten laden
            //DatasetTp = DienstProgramme.GetData(Path.Combine(Application.StartupPath, @"Data.TestDaten2EST.XML")); // Testdaten laden
            //DatasetTp = DienstProgramme.GetData(Path.Combine(Application.StartupPath, @"Data.DatenNeuEST.XML")); // Testdaten laden
            //DatasetTp = DienstProgramme.GetData(Path.Combine(Application.StartupPath, @"ProjectTerminplan.XML")); // Testdaten laden

            if (!this.prjHinzugefuegt)
            {
                // Die eingelesenen Daten an die ultraCalendarInfo anbinden.
                OnInitializationStatusChanged(Resources.Binding);                   // Anzeige im Splashscreen aktualisieren
                OnBindArbInhaltData(DatasetTp);                                 // Daten an ultraCalendarInfo anbinden
            }

            // Initialisiert die Kontrols auf dem Formular
            OnInitializationStatusChanged(Resources.Initializing);              // Anzeige im Splashscreen aktualisieren
            OnColorizeImages();                                                 // Farbe der Bilder an das eingestellte Farbschema anpassen
            OnInitializeUi();                                                   // Oberfläche initialisieren

            // Ereignisprozedur zum Ändern des Schemas festlegen
            StyleManager.StyleChanged += this.OnApplicationStyleChanged;

            // Startdatum des Projekts ermitteln
            AnzeigeDatumEinstellen();
            //            if (DatasetTp != null)
            //            {
            //                DatasetTp.AcceptChanges();
            //                var datum = this.DatasetTp.Tables[0].Rows[0].ItemArray[2].ToString();
            //                ultraGanttView1.EnsureDateTimeVisible(datum);                   // Zeitleiste so verschieben, dass das Startdatum angezeit wird
            //            }
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
        private void OnApplicationStyleChanged(object sender, StyleChangedEventArgs e)
        {
            ApplicationStyleChanged(sender, e);
        }

        #endregion OnApplicationStyleChanged

        #region OnUltraCalendarInfo1CalendarInfoChanged

        /// <summary> Behandelt das CalendarInfoChanged-Ereignis des ultraCalendarInfo1 Kontrols. </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="CalendarInfoChangedEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void OnUltraCalendarInfo1CalendarInfoChanged(object sender, CalendarInfoChangedEventArgs e)
        {
            UltraCalendarInfo1CalendarInfoChanged(sender, e);
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
            UltraGanttView1ActiveTaskChanging(sender, e);
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
            UltraGanttView1CellActivating(sender, e);
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
            UltraGanttView1CellDeactivating(sender, e);
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
            // Vorgaben für den neuen Task machen
            DateTime startDatum;                                                // Startdatum des hinzugefügten Verfahrens
            DateTime endeDatum;                                                 // Endedatum des hinzugefügten Verfahrens
            var prjStart = Convert.ToDateTime(StammDaten.prjStartDatum.ToShortDateString()); // Projektstart aus den Stammdaten
            startDatum = e.Task.Parent != null ? e.Task.Parent.StartDateTime : prjStart;
            if (startDatum < prjStart) startDatum = prjStart;                   // Falls das Startdatum vor dem Projektstart liegt, dieses auf den Projektstart setzen
            e.Task.StartDateTime = startDatum;

            endeDatum = startDatum.AddDays( e.Task.Duration.Days);              // Standarddauar zum Startdatum hinzuzählen für das Endedatum
            e.Task.EndDateTime = endeDatum;
            UltraGanttView1TaskAdded(sender, e);
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
            UltraGanttView1TaskDeleted(sender, e);
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
        private void OnUltraToolbarsManager1PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UltraToolbarsManager1PropertyChanged(sender, e);
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
            UltraToolbarsManager1ToolValueChanged(sender, e);
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
        private void OnUltraTouchProvider1PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UltraTouchProvider1PropertyChanged(sender, e);
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
            BindArbInhaltData(data);
        }

        #endregion OnBindArbInhaltData

        #region OnChangeIcon

        /// <summary>
        /// Ändert das Symbol.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private void OnChangeIcon()
        {
            ChangeIcon();
        }

        #endregion OnChangeIcon

        #region OnColorizeImages

        /// <summary>
        /// Färbt die Bilder in den großen und kleinen Bildlisten mit den Standardbildern
        /// und platziert die neuen Bilder in den farbigen Bildlisten.
        /// </summary>
        private void OnColorizeImages()
        {
            ColorizeImages();
        }

        #endregion OnColorizeImages

        #region OnInitializeUI

        /// <summary>
        /// Initialisiert die Oberfläche.
        /// </summary>
        private void OnInitializeUi()
        {
            InitializeUi();                                                     // Oberfläche initialisieren
            ultraGridDaten.DisplayLayout.Bands[0].Columns[0].AutoSizeMode = Infragistics.Win.UltraWinGrid.ColumnAutoSizeMode.AllRowsInBand;
            splitContainerHauptDaten.SplitterDistance = 125;
        }

        #endregion OnInitializeUI

        #region OnSetTextForeColor

        /// <summary>
        /// Aktualisiert den Wert der Vordergrundfarbe des Textes in der aktiven Zelle abhängig von der
        /// im PopupColorPickerTool ausgewählten Farbe.
        /// </summary>
        private void OnSetTextForeColor()
        {
            SetTextForeColor();
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
            var dateiName = DienstProgramme.OeffneXmlDatei();
            if (dateiName == string.Empty) return;                              // Abbruch, da keine Datei ausgewählt wurde

            this.ultraGanttView1.Project = null;
            this.components = new Container();
            this.DatasetTp = new DataSet();
            this.ultraCalendarInfo1 = new UltraCalendarInfo(components);

            // Ruft die Daten aus der bereitgestellten XML-Datei ab
            this.DatasetTp = DienstProgramme.GetData(dateiName);                // ausgewählte Daten laden
            this.GeladeneDatei = dateiName;                                     // ausgewählte Daten-Datei merken

            // Die eingelesenen Daten an die ultraCalendarInfo anbinden.
            OnBindArbInhaltData(DatasetTp);                                     // Daten an ultraCalendarInfo anbinden
        }

        #endregion Datei laden

        #region Neues Projekt

        private void ErstelleNeuesProjekt()
        {
            this.prjHinzugefuegt = false;                                       // Es wurde kein neues Projekt hinzugefügt
            components = new Container();
            ultraCalendarInfo1 = null;
            GC.Collect();                                                       // Speicher bereinigen

            ultraCalendarInfo1 = new UltraCalendarInfo(components);
            var prjNeu = new NeuesProjekt(ref ultraCalendarInfo1);              // Neuen Dialog zur Eingabe der Projektdaten
            var result = prjNeu.ShowDialog();                                   // Gedrückte Taste des Dialogs

            if (result == DialogResult.Cancel)
            {
                var meldung = @"Sie haben das Neuanlegen abgebrochen." + Environment.NewLine +
                    @"Soll ein bestehender Terminplan geladen werten?";
                const string Ueberschrift = @"Frage";
                var erg = MessageBox.Show(this, meldung, Ueberschrift, MessageBoxButtons.YesNo);

                // Falls ein bestehender Terminplan geladen werden soll, muss der Öffnen-Dialog angezeigt werden
                if (erg == DialogResult.Yes)
                {
                    LadeDatei();                                                // Anderen Terminplan laden
                    return;                                                     // Bearbeitung beenden
                }
            }

            if (prjNeu.PrjName != null)
            {
                // Listen erzeugen. Die Liste se
                // TODO: Daten aus Stammdaten holen
                var listBesitzer = new List<Besitzer>
                {
                    new Besitzer
                    {
                        Key = @"Martin Knoblauch",
                        Name = @"kn",
                        Sichtbar = true,
                        EmailAddresse = string.Empty
                    },

                    new Besitzer
                    {
                        Key = @"Martin Kaiser",
                        Name = @"ka",
                        Sichtbar = true,
                        EmailAddresse = string.Empty
                    },

                    new Besitzer
                    {
                        Key = @"Wolfgang Roth",
                        Name = @"ro",
                        Sichtbar = true,
                        EmailAddresse = string.Empty
                    },

                    new Besitzer
                    {
                        Key = @"Armin Brenner",
                        Name = @"br",
                        Sichtbar = true,
                        EmailAddresse = string.Empty
                    },

                    new Besitzer
                    {
                        Key = @"C. Klute-Lang",
                        Name = @"kl",
                        Sichtbar = true,
                        EmailAddresse = string.Empty
                    },

                    new Besitzer
                    {
                        Key = @"Martin Jetter",
                        Name = @"je",
                        Sichtbar = true,
                        EmailAddresse = string.Empty
                    },

                    new Besitzer
                    {
                        Key = @"J. Echsler-Kull",
                        Name = @"ku",
                        Sichtbar = true,
                        EmailAddresse = string.Empty
                    },

                    new Besitzer
                    {
                        Key = @"Benjamin Kittlaus",
                        Name = @"ks",
                        Sichtbar = true,
                        EmailAddresse = string.Empty
                    },

                    new Besitzer
                    {
                        Key = @"Andreas Held",
                        Name = @"hd",
                        Sichtbar = true,
                        EmailAddresse = string.Empty
                    },

                    new Besitzer
                    {
                        Key = @"Xue Yang",
                        Name = @"ya",
                        Sichtbar = true,
                        EmailAddresse = string.Empty
                    }
                };

                // TODO: Falls nicht EST, Daten aus den Stammdaten holen
                var listAufgaben = new List<string> { @"Aufgabe 1" };           // Liste der Aufgaben

                if (AddNewProjekt(prjNeu.PrjName,
                    prjNeu.PrjStart,
                    prjNeu.StartPrj,
                    prjNeu.Kommission,
                    listAufgaben,
                    listBesitzer))
                {
                    prjHinzugefuegt = true;                                     // Es wurde ein neues Projekt hinzugefügt
                }
            }

            //if (!prjHinzugefuegt) return;                                       // Falls kein neues Projekt hinzugefügt wurde, kann hier abgebrochen werden
            return;

            // Erzeugte Projekt speichern
            //var speicherPfad = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //this.ultraCalendarInfo1.SaveAsXml(Path.Combine(speicherPfad, @"Data.DatenNeuEST.XML"),
            //    CalendarInfoCategories.All);

            //DatasetTp = DienstProgramme.GetData(Path.Combine(speicherPfad, @"Data.DatenNeuEST.XML")); // Neue Daten laden

            //components = new Container();
            //ultraCalendarInfo1 = null;
            //GC.Collect();                                                       // Speicher bereinigen

            //ultraCalendarInfo1 = new UltraCalendarInfo(components);

            // Ruft die Daten aus der bereitgestellten XML-Datei ab
            //DatasetTp = new DataSet();
            //DatasetTp = DienstProgramme.GetData(Path.Combine(Application.StartupPath, @"ProjectTerminplan.XML")); // Testdaten laden
            // Die eingelesenen Daten an die ultraCalendarInfo anbinden.
            //this.DatasetTp.AcceptChanges();
            //OnBindArbInhaltData(DatasetTp);                                     // Daten an ultraCalendarInfo anbinden
        }

        #endregion Neues Projekt

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
            var breite = ultraGanttView1.GridAreaWidth;
            var gesamtbreite = Width;
            var teiler = Math.Abs(breite / (float)gesamtbreite);
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

            var fontGroesse = ultraGanttView1.GridSettings.RowAppearance.FontData.SizeInPoints;
            var headerGroesse = ultraGanttView1.GridSettings.ColumnHeaderAppearance.FontData.SizeInPoints;

            if (resBreite < 1024)
            {
                fontGroesse = 8;
                headerGroesse = 9;

                ultraGanttView1.GridSettings.RowAppearance.FontData.SizeInPoints = fontGroesse;
                ultraGanttView1.GridSettings.ColumnHeaderAppearance.FontData.SizeInPoints = FontHeight;
            }

            ultraGanttView1.Appearance.FontData.SizeInPoints = fontGroesse;
            var breite = ultraGanttView1.GridAreaWidth;

            var col = ultraGanttView1.GridSettings.ColumnSettings.Values;
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

            ultraGanttView1.GridAreaWidth = panalWeite;
            var splitterWeite = (float)panalWeite * 1.5;
            ultraGanttView1.GridAreaWidth = Convert.ToInt32(splitterWeite);

            var gesamtbreite = Width;
            var teiler = Math.Abs((float)breite / (float)gesamtbreite);
        }

        /// <summary>Behandelt das ToolClick-Ereignis of the ultraToolbarsManager1 control.</summary>
        /// <remarks>
        /// Die jeweilige Aktion wird nur durchgeführt, wenn sie nicht schon durchgeführt wurde
        /// </remarks>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.UltraWinToolbars.ToolClickEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void UltraToolbarsManagerToolClick(object sender, ToolClickEventArgs e)
        {
            OnUltraToolbarsManagerToolClick(sender, e);
        }

        #endregion SplashScreen Ereignisse

        /// <summary>Behandelt das BeforeApplicationMenu2010Displayed-Ereignis of the ultraToolbarsManager1 control.</summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.UltraWinToolbars.BeforeApplicationMenu2010DisplayedEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraToolbarsManager1BeforeApplicationMenu2010Displayed(object sender, BeforeApplicationMenu2010DisplayedEventArgs e)
        {
            // Falls ein neues Projekt hinzugefügt wurde, muss die Auswahl 'Speichern' gesperrt
            // werden, da noch kein Dateiname angegeben wurde, ansonsten ist die
            // Auswahl 'Speichern' freigeschaltet
            ultraToolbarsManager1.Tools[@"Speichern"].SharedProps.Enabled = !prjHinzugefuegt;
        }

        private void UltraGanttView1BindingContextChanged(object sender, EventArgs e)
        {
        }

        private void ultraGanttView1_TaskAdding(object sender, TaskAddingEventArgs e)
        {
        }

        private void ultraGanttView1_ContextMenuStripChanged(object sender, EventArgs e)
        {
        }

        /// <summary>Initialisiert das UltraGridDaten Control.</summary>
        /// <remarks>Dient zur Anzeige der Stammdaten im Terminplan</remarks>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.UltraWinToolbars.InitializeLayoutEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraGridDatenInitializeLayout(object sender, Infragistics.Win.UltraWinGrid.InitializeLayoutEventArgs e)
        {
            // Die Spalten zur Aufnahme des Firmenlogos und des Controls zur Anzeige der Meilensteine sind verbundene Spalten
            e.Layout.Bands[0].Columns["meilenSteine"].MergedCellStyle = MergedCellStyle.Always;
            e.Layout.Bands[0].Columns["firma"].MergedCellStyle = MergedCellStyle.Always;
        }
    }
}