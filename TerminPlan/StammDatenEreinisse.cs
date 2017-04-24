// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StammDatenEreignisse.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Ereignisbehandlung des Formulars.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  06.01.17  br      Grundversion
//                  31.03.17  br      Einfärben 2. Toolbarmanager
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Terminplan
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;
    using Infragistics.Win;
    using Infragistics.Win.AppStyling;
    using Infragistics.Win.UltraWinGrid;
    using Infragistics.Win.UltraWinToolbars;
    using PropertyIds = Infragistics.Win.UltraWinToolbars.PropertyIds;

    /// <summary>
    /// Klasse StammDaten.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
    public partial class StammDaten : Form
    {
        #region Ereignisprozeduren

        #region ApplicationStyleChanged

        /// <summary> Behandelt das StyleChanged-Ereignis des Application Styling Managers </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.AppStyling.StyleChangedEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void ApplicationStyleChanged(object sender, StyleChangedEventArgs e)
        {
            // Bilder an das ausgewählte Farbschema anpassen.
            ColorizeImages();
        }

        #endregion ApplicationStyleChanged

        #region this.ultraToolbarsManagerStammPropertyChanged

        /// <summary>
        /// Behandelt das PropertyChanged-Ereignis des this.ultraToolbarsManagerStamm Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="PropertyChangedEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void UltraToolbarsManagerStammPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var trigger = e.ChangeInfo.FindTrigger(null);                       // Ermitteln, welche Eigenschaft geändert wurde

            // Nur bearbeiten, wenn es eine Eigenschaft ist, welche vom Toolbars-Manager verwaltet wird
            if (trigger == null || !(trigger.Source is SharedProps) || !(trigger.PropId is PropertyIds))

            {
                return;
            }

            // ID auswerten
            switch ((PropertyIds)trigger.PropId)
            {
                case PropertyIds.Enabled:                                       // Nur freigegebene Eigenschaften bearbeiten
                    var sharedProps = (SharedProps)trigger.Source;              // Kontrol ermitteln

                    // Falls mehrere Instanzen des Kontrols vorhanden sind, die erste Instanz nehmen,
                    // bei nur einer Instanz muss diese genommen werden
                    var tool = (sharedProps.ToolInstances.Count > 0) ? sharedProps.ToolInstances[0] : sharedProps.RootTool; // Name desSchlüssels zusammenstellen
                    var imageKey = string.Format(@"{0}_{1}", tool.Key, tool.EnabledResolved ? @"Normal" : @"Disabled");

                    // Schlüssel des Bildes in die entsprechende Appearance-Eigenschaft eintragen
                    if (this.ilColorizedImagesLarge.Images.ContainsKey(imageKey))
                    {
                        sharedProps.AppearancesLarge.Appearance.Image = imageKey; // Für große Bilder
                    }

                    if (this.ilColorizedImagesSmall.Images.ContainsKey(imageKey))
                    {
                        sharedProps.AppearancesSmall.Appearance.Image = imageKey; // Für kleine Bilder
                    }

                    break;
            }
        }

        #endregion this.ultraToolbarsManagerStammPropertyChanged

        #region UltraToolbarsManagerToolClick

        /// <summary>
        /// Behandelt das ToolClick-Ereignis of the this.ultraToolbarsManagerStamm control.
        /// </summary>
        /// <remarks>
        /// Die jeweilige Aktion wird nur durchgeführt, wenn sie nicht schon durchgeführt wurde
        /// </remarks>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.UltraWinToolbars.ToolClickEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraToolbarsManagerToolClick(object sender, ToolClickEventArgs e)
        {
            // Ermitteln, auf welches Tool geklickt wurde
            switch (e.Tool.Key)
            {
                case "Font_Bold":                                               // Fettschrift
                    if (this.cellActivationRecursionFlag == false)
                    {
                        this.UpdateFontProperty(FontProperties.Bold);
                    }
                    break;

                case "Font_Italic":                                             // Kursivschrift
                    if (this.cellActivationRecursionFlag == false)
                    {
                        this.UpdateFontProperty(FontProperties.Italics);
                    }
                    break;

                case "Font_Underline":
                    if (this.cellActivationRecursionFlag == false)              // Unterstrichene Schrift
                    {
                        this.UpdateFontProperty(FontProperties.Underline);
                    }
                    break;

                case "Font_BackColor":                                          // Hintergrundfarbe
                    SetTextBackColor();
                    break;

                case "Font_ForeColor":                                          // Vordergrundfarbe
                    SetTextForeColor();
                    break;

                case "FontList":                                                // Liste mit den Schriftarten
                    UpdateFontName();
                    break;

                case "FontSize":                                                // Schriftgröße
                    UpdateFontSize();
                    break;
            }
        }

        #endregion UltraToolbarsManagerToolClick

        #region ultraToolbarsManagerStammToolValueChanged

        /// <summary>
        /// Behandelt das ToolValueChanged-Ereignis des this.ultraToolbarsManagerStamm Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">The <see cref="ToolEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void UltraToolbarsManagerStammToolValueChanged(object sender, ToolEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "Font_BackColor":
                    SetTextBackColor();
                    break;

                case "Font_ForeColor":
                    SetTextForeColor();
                    break;

                case "FontList":
                    UpdateFontName();
                    break;

                case "FontSize":
                    UpdateFontSize();
                    break;
            }
        }

        #endregion ultraToolbarsManagerStammToolValueChanged

        #endregion Ereignisprozeduren

        #region Methoden

        #region SetTextBackColor

        /// <summary>
        /// Aktualisiert den Wert der Hintergrundfarbe des Textes in der aktiven Zelle abhängig von der
        /// im PopupColorPickerTool ausgewählten Farbe.
        /// </summary>
        private void SetTextBackColor()
        {
            // Ausgewählte Farbe aus dem ColorPicker ermitteln
            var fontBgColor = ((PopupColorPickerTool)this.ultraToolbarsManagerStamm.Tools[@"Font_BackColor"]).SelectedColor;
            var activeZelle = this.ultraGridStammDaten.ActiveCell;              // Aktive Zelle ermitteln

            // Nur bearbeiten, falls eine aktive Zelle existiert
            if (activeZelle == null)
            {
                return;
            }

            activeZelle.Appearance.BackColor = fontBgColor;                     // Hintergrundfarbe der Zelle setzen
        }

        #endregion SetTextBackColor

        #region SetTextForeColor

        /// <summary>
        /// Aktualisiert den Wert der Vordergrundfarbe des Textes in der aktiven Zelle abhängig von der
        /// im PopupColorPickerTool ausgewählten Farbe.
        /// </summary>
        private void SetTextForeColor()
        {
            // Ausgewählte Farbe aus dem ColorPicker ermitteln
            var fontColor = ((PopupColorPickerTool)this.ultraToolbarsManagerStamm.Tools[@"Font_ForeColor"]).SelectedColor;
            var activeZelle = this.ultraGridStammDaten.ActiveCell;              // Aktive Zelle ermitteln

            // Nur bearbeiten, falls eine aktive Zelle existiert
            if (activeZelle == null)
            {
                return;
            }

            activeZelle.Appearance.ForeColor = fontColor;                       // Vordergrundfarbe der Schrift setzen
        }

        #endregion SetTextForeColor

        #region UpdateFontName

        /// <summary> Aktualisiert den Namen der Schriftart je nach dem im FontListTool ausgewählten Wert. </summary>
        private void UpdateFontName()
        {
            // Namen der ausgewählte Schriftart aus der Fontliste ermitteln
            var fontName = ((FontListTool)ultraToolbarsManagerStamm.Tools[@"FontList"]).Text;
            var activeZelle = this.ultraGridStammDaten.ActiveCell;              // Aktive Zelle ermitteln

            // Nur bearbeiten, falls eine aktive Zelle existiert
            if (activeZelle == null)
            {
                return;
            }

            activeZelle.Appearance.FontData.Name = fontName;                    // Font der Schrift der Zelle zuweisen
        }

        #endregion UpdateFontName

        #region UpdateFontSize

        /// <summary> Aktualisiert die Schriftgröße je nach dem im ComboBoxTool ausgewählten Wert. </summary>
        private void UpdateFontSize()
        {
            // Größe der ausgewählte Schriftart aus dem ComboBoxTool ermitteln
            var item = (ValueListItem)((ComboBoxTool)(this.ultraToolbarsManagerStamm.Tools[@"FontSize"])).SelectedItem;

            // Nur bearbeiten, falls ein Wert vorhanden ist
            if (item == null)
            {
                return;
            }

            var fontSize = (float)item.DataValue;                               // Schriftgröße
            var activeZelle = this.ultraGridStammDaten.ActiveCell;              // Aktive Zelle ermitteln

            // Nur bearbeiten, falls eine aktive Zelle existiert
            if (activeZelle == null)
            {
                return;
            }

            activeZelle.Appearance.FontData.SizeInPoints = fontSize;            // Schriftgröße der Zelle zuweisen
        }

        #endregion UpdateFontSize

        #region UpdateFontProperty

        /// <summary>Methode, um verschiedene Eigenschaften der Schriftart zu aktualisieren</summary>
        /// <remarks> Die Eigenschaften werden umgeschaltet.</remarks>
        /// <param name="propertyToUpdate">Aufzählung von Eigenschaften, welche von der Schriftart abhängig sind</param>
        private void UpdateFontProperty(FontProperties propertyToUpdate)
        {
            var activeCell = this.ultraGridStammDaten.ActiveCell;               // Aktive Zelle im Grid ermitteln ermitteln
            UltraGridCell zelle;
            // Nur bearbeiten, falls keine aktive Zelle existiert
            if (activeCell == null)
            {
                // Falls es kein aktive Zelle gibt, können auch Zellen selektiert sein
                var anzZeilen = this.ultraGridStammDaten.Rows.Count;           // Anzahl Zeilen im Grid
                var anzSpalten = this.ultraGridStammDaten.DisplayLayout.Bands[0].Columns.Count; // Anzahl Spalten des Grids

                // Alle Zeilen und Spalten durchgehen und selektierte Zellen ermitteln
                for (var z = 0; z < anzZeilen; z++)
                {
                    for (var s = 0; s < anzSpalten; s++)
                    {
                        zelle = this.ultraGridStammDaten.Rows[z].Cells[s];      // Zelle im Grid
                        if (zelle.Selected)
                        {
                            // Die Zelle ist ausgewählt, Fontdaten einstellen
                            this.StelleFontEigenschaftEin(ref zelle, propertyToUpdate);
                        }
                    }
                }
            }
            else
            {
                // Die Zelle ist aktiv, Fontdaten einstellen
                this.StelleFontEigenschaftEin(ref activeCell, propertyToUpdate);
            }

            this.cellActivationRecursionFlag = false;                           // Zellen müssen nicht rekursiv bearbeitet werden, gilt also nur für eine Zelle
        }

        /// <summary>
        /// Methode, um verschiedene Eigenschaften der Schriftart zu aktualisieren
        /// </summary>
        /// <param name="zelle">Die zu bearbeitende Zelle.</param>
        /// <param name="propertyToUpdate">Aufzählung von Eigenschaften, welche von der Schriftart abhängig sind</param>
        /// <remarks>
        /// Die Eigenschaften werden umgeschaltet.
        /// </remarks>
        private void StelleFontEigenschaftEin(ref UltraGridCell zelle, FontProperties propertyToUpdate)
        {
            // Art der Daten auswerten
            switch (propertyToUpdate)
            {
                case FontProperties.Bold:                                       // Fettschrift
                    zelle.Appearance.FontData.Bold = DienstProgramme.ToggleDefaultableBoolean(zelle.Appearance.FontData.Bold); // Fettschrift aus- oder einschalten
                    break;

                case FontProperties.Italics:                                    // Kursiv
                    zelle.Appearance.FontData.Italic = DienstProgramme.ToggleDefaultableBoolean(zelle.Appearance.FontData.Italic); // Kursivschrift umschalten
                    break;

                case FontProperties.Underline:                                  // Unterstrichen
                    zelle.Appearance.FontData.Underline = DienstProgramme.ToggleDefaultableBoolean(zelle.Appearance.FontData.Underline); // unterstrichene Schrift umschalten
                    break;
            }
        }

        #endregion UpdateFontProperty

        #region ChangeIcon

        /// <summary>
        /// Ändert das Symbol.
        /// </summary>
        private void ChangeIcon()
        {
            // Anhand des Farbschemas den Namen des zum Farbschema gehörenden Icons zusammensetzen
            var iconPath = this.FrmTerminPlan.ThemePaths[this.FrmTerminPlan.CurrentThemeIndex].Replace(@"StyleLibraries.", @"Images.AppIcon - ").Replace(@".isl", @".ico");

            var stream = DienstProgramme.GetEmbeddedResourceStream(iconPath);   // Zum Laden des Farbschemas

            // Falls Farbschema existiert, kann Icon geladen werden
            if (stream != null)
            {
                this.Icon = new Icon(stream);                                   // Icon laden
            }
        }

        #endregion ChangeIcon

        #region ColorizeImages

        /// <summary>
        /// Färbt die Bilder in den großen und kleinen Bildlisten mit den Standardbildern
        /// und platziert die neuen Bilder in den farbigen Bildlisten.
        /// </summary>
        private void ColorizeImages()
        {
            // Unterbindet das Zeichnen im UltraToolbarsManager,
            // damit die neuen Farben eingestellt werden können-
            var shouldSuspendPainting = !this.ultraToolbarsManagerStamm.IsUpdating; // Ermitteln, ob gerade gezeichnet wird

            // Neue Farben können eingestellt werden, falls nicht gerade aufgefrischt wird
            if (shouldSuspendPainting)
            {
                this.ultraToolbarsManagerStamm.BeginUpdate();                       // Auffrischen starten
            }

            // Bildlisten mit den neuen Bildern setzen
            var largeImageList = this.ultraToolbarsManagerStamm.ImageListLarge;
            var smallImageList = this.ultraToolbarsManagerStamm.ImageListSmall;

            try
            {
                // Bildlisten im UltraToolbarsManager löschen, damit weiter andere
                // Farben eingestellt werden können
                this.ultraToolbarsManagerStamm.ImageListLarge = null;
                this.ultraToolbarsManagerStamm.ImageListSmall = null;

                ToolBase resolveTool = null;                                    // gefundenes Tool löschen, damit neues erstellt werden kann

                // Nur bearbeiten, falls das Tool "Paste" existiert
                if (this.ultraToolbarsManagerStamm.Tools.Exists(@"Paste"))
                {
                    resolveTool = this.ultraToolbarsManagerStamm.Tools[@"Paste"]; // Tool merken

                    // Alle Instanzen auf der Suche nach dem Tool in der RibbonGroup durchsuchen
                    foreach (var instanceTool in resolveTool.SharedProps.ToolInstances.Cast<ToolBase>().Where(instanceTool => instanceTool.OwnerIsRibbonGroup))
                    {
                        resolveTool = instanceTool;                             // Tool gefunden, Suche kann abgebrochen werden
                        break;
                    }
                }

                // Nur weiter bearbeiten, wenn ein Tool gefunden wurde
                if (resolveTool == null)
                {
                    return;
                }

                // Holt die eingestellten Farben
                var colors = new Dictionary<string, Color>();                   // Neue Liste mit den Farben erzeugen

                // Standard-Vordergrundfarbe einstellen
                var appData = new AppearanceData();                             // Neue Einstellungen für Infragistics
                var requestedProps = AppearancePropFlags.ForeColor;             // Vordergrundfarbe soll eingestellt werden

                // Das aktuelle Erscheinungsbild des Tools löschen
                resolveTool.ResolveAppearance(ref appData, ref requestedProps);
                colors[@"Normal"] = appData.ForeColor;                          // Standard-Vordergrundfarbe in Liste eintragen

                // Aktive Vordergrundfarbe einstellen
                appData = new AppearanceData();                                 // Neue Einstellungen für Infragistics
                requestedProps = AppearancePropFlags.ForeColor | AppearancePropFlags.BackColor; // Ermitteln, ob Vorder- oder Hintergrundfarbe bearbeiteet werden soll

                // Das aktuelle Erscheinungsbild des Tools löschen
                resolveTool.ResolveAppearance(ref appData, ref requestedProps, true, false);
                colors[@"Active"] = appData.ForeColor;                          // Aktive Vordergrundfarbe in Liste eintragen

                // Hintergrundfarbe einstellen
                if (appData.BackColor.IsEmpty || appData.BackColor.Equals(Color.Transparent))
                {
                    // Hintergrundfabe festlegen, falls keine Farbe oder 'Transparent' angegeben ist
                    appData = new AppearanceData();                             // Neue Einstellungen für Infragistics
                    requestedProps = AppearancePropFlags.BackColor;             // Hintergrundfarbe soll eingestellt werden

                    // Löscht das aktuelle Erscheinungsbild für die Registerkarte des RibbonTab
                    this.ultraToolbarsManagerStamm.Ribbon.Tabs[0].ResolveTabItemAppearance(ref appData, ref requestedProps);
                    colors[@"Disabled"] = appData.BackColor;                    // Hintergrundfarbe für 'gesperrt' in Liste eintragen
                }
                else
                {
                    // Es ist eine Farbe angegeben, diese dann in die Liste eintragen
                    colors[@"Disabled"] = appData.BackColor;
                }

                // Die Standardbilder haben die Farbe 'Magenta'. Diese Farbe muss ersetzt werden
                var replacementColor = Color.Magenta;

                // Die Bilder in den großen und kleinen Bildlisten mit den Standardbildern
                // an das ausgewählte Farbschema anpassen
                DienstProgramme.ColorizeImages(replacementColor, colors, ref this.ilDefaultImagesLarge, ref this.ilColorizedImagesLarge);
                DienstProgramme.ColorizeImages(replacementColor, colors, ref this.ilDefaultImagesSmall, ref this.ilColorizedImagesSmall);

                // Sicherstellen, dass der UltraToolbarsManager die neuen farbigen Bilder verwendet
                largeImageList = this.ilColorizedImagesLarge;
                smallImageList = this.ilColorizedImagesSmall;
            }
            catch
            {
                // Sicherstellen, dass der UltraToolbarsManager die neuen farbigen Bilder verwendet
                largeImageList = this.ilDefaultImagesLarge;
                smallImageList = this.ilDefaultImagesSmall;
            }
            finally
            {
                this.ultraToolbarsManagerStamm.ImageListLarge = largeImageList;
                this.ultraToolbarsManagerStamm.ImageListSmall = smallImageList;

                // Zeichnen im UltraToolbarsManager fortsetzen, falls es unterbrochen war
                if (shouldSuspendPainting)
                {
                    this.ultraToolbarsManagerStamm.EndUpdate();                 // Auffrischen ist fertig
                }
            }
        }

        #endregion ColorizeImages

        #region InitializeUi

        /// <summary> Initialisiert die Oberfläche. </summary>
        private void InitializeUi()
        {
            var culture = CultureInfo.InstalledUICulture;                       // Sprache des Betriebssystems ermitteln
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            var col = this.ultraGridStammDaten.DisplayLayout.Bands[0].Columns;

            // Spaltenbreite einstellen
            foreach (var de in col)
            {
                var colNumber = Convert.ToInt32(DienstProgramme.ZahlenInString(de.Header.Caption)); //Spaltennummer extrahieren
                var colBez = DienstProgramme.IntConvertToExcelHeadLine(colNumber); // Spaltennummer in Spaltenbuchstabe(n) umwandeln

                de.Header.Caption = colBez;                                     // Spaltenbuchstabe eintragen
                de.Header.Appearance.TextHAlign = HAlign.Center;
                de.Header.Appearance.TextVAlign = VAlign.Middle;
                de.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                de.Header.Appearance.FontData.Name = @"Arial";
                de.Header.Appearance.FontData.SizeInPoints = 10;

                // Spaltenbreite je nach Spaltenummer einstellen
                switch (colNumber)
                {
                    case 1:                                                     // Spalte 1
                        de.Width = 14;
                        break;

                    case 2:
                        de.Width = 320;
                        break;

                    case 3:
                    case 5:
                    case 8:
                    case 13:
                    case 18:
                    case 20:
                    case 26:
                    case 32:
                    case 34:
                        de.Width = 32;
                        break;

                    case 4:
                    case 22:
                    case 24:
                    case 30:
                        de.Width = 22;
                        break;

                    case 6:
                        de.Width = 86;
                        break;

                    case 7:
                        de.Width = 173;
                        break;

                    case 9:
                        de.Width = 166;
                        break;

                    case 14:
                        de.Width = 155;
                        break;

                    case 19:
                        de.Width = 100;
                        break;

                    case 21:
                    case 23:
                    case 25:
                        de.Width = 220;
                        break;

                    case 27:
                        de.Width = 280;
                        break;

                    case 28:
                        de.Width = 26;
                        break;

                    case 29:
                        de.Width = 116;
                        break;

                    case 31:
                    case 38:
                    case 39:
                    case 40:
                    case 41:
                        de.Width = 80;
                        break;

                    case 33:
                        de.Width = 170;
                        break;

                    case 35:
                        de.Width = 35;
                        break;

                    case 36:
                        de.Width = 286;
                        break;

                    case 37:
                        de.Width = 52;
                        break;

                    case 42:
                        de.Width = 440;
                        break;

                    default:                                                    // Alle anderen Spalten werden ausgeblendet
                        if ((colNumber <= 57) && (colNumber >= 43))
                        {
                            // Zwischen den Spalten 43 und 57 ist die Spaltenbreite gleich
                            de.Width = 80;
                        }
                        else
                        {
                            de.Hidden = true;
                        }
                        break;
                }
            }

            // Füllt die Liste mit den Farbschematas
            var selectedIndex = 0;                                              // Index des ausgewählten Farbschemas (1. Element)
            var themeTool = (ListTool)this.ultraToolbarsManagerStamm.Tools[@"ThemeList"];

            // Alle vorhandenen Farbschematas durchgehen
            foreach (var resourceName in this.FrmTerminPlan.ThemePaths)
            {
                var item = new ListToolItem(resourceName);                      // Eintrag aus der liste

                // In der Liste erscheint nur der Name des Farbschemas ohne Endung in Dateinamen
                var libraryName = resourceName.Replace(@".isl", string.Empty);
                item.Text = libraryName.Remove(0, libraryName.LastIndexOf('.') + 1);
                themeTool.ListToolItems.Add(item);                              // Name des Farbschemas der Liste hinzufügen

                // Farbschema 4 (dunkle Farbe Excel) auswählen
                if (item.Text.Contains(@"04"))
                {
                    selectedIndex = item.Index;
                }
            }

            themeTool.SelectedItemIndex = selectedIndex;                        // Ausgewähltes Farbschema als Standard setzen

            // Das richtigen Listenelement für den Touch-Modus auswählen
            ((ListTool)this.ultraToolbarsManagerStamm.Tools[@"TouchMode"]).SelectedItemIndex = 0; // Erstes Element als Auswahl

            // Erstellt eine Liste mit verschiedenen Schriftgrößen
            PopulateFontSizeValueList();                                        // Fontliste füllen
            ((ComboBoxTool)(this.ultraToolbarsManagerStamm.Tools[@"FontSize"])).SelectedIndex = 0;
            ((FontListTool)this.ultraToolbarsManagerStamm.Tools[@"FontList"]).SelectedIndex = 0;
            OnUpdateFontToolsState(false);                                      // Font ist nicht auswählbar

            // Aboutbox initialisieren
            Control control = new AboutControl()
            {
                Visible = false,                                                // Aboutbox ist nicht sichtbar
                Parent = this                                                   // Das Hauptformular ist das Elternformular
            };                                                                  // Neue Instanz der Aboutbox erzeugen

            ((PopupControlContainerTool)this.ultraToolbarsManagerStamm.Tools[@"About"]).Control = control; // Aboutbox in die Tools für den UltraToolbarsManager setzen

            // Die Bilder entsprechend dem aktuellen Farbschema einfärben.
            ColorizeImages();
            this.ultraToolbarsManagerStamm.Ribbon.FileMenuButtonCaption = Properties.Resources.ribbonFileTabCaption; // Beschriftung des Datei-Menüs-Button eintragen

            this.StelleArbeitsBlattEin();
        }

        #endregion InitializeUi

        #region PopulateFontSizeValueList

        /// <summary> Liste mit den Schriftgrößen erstellen </summary>
        private void PopulateFontSizeValueList()
        {
            // Schriftgrößen für die Liste vorgeben und neue Liste erstellen
            var fontSizeList = new List<float>(new float[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 });

            // Jeden Eintrag der Liste in das Tool für die Schriftgröße des UltraToolbarsManager eintragen
            foreach (var i in fontSizeList)
            {
                ((ComboBoxTool)(this.ultraToolbarsManagerStamm.Tools[@"FontSize"])).ValueList.ValueListItems.Add(i);
            }
        }

        #endregion PopulateFontSizeValueList

        #region OnUpdateFontToolsState

        /// <summary>
        /// Aktualisiert die Enabled-Eigenschaft für Werkzeuge in der RibbonGruppe "RibbonGrp_Font"
        /// </summary>
        /// <param name="enabled">falls auf <c>true</c> gesetzt ist, freigeben.</param>
        private void OnUpdateFontToolsState(bool enabled)
        {
            DienstProgramme.SetRibbonGroupToolsEnabledState(this.ultraToolbarsManagerStamm.Ribbon.Tabs[0].Groups[@"RibbonGrp_Font"], enabled);
        }

        #endregion OnUpdateFontToolsState

        #endregion Methoden
    }
}