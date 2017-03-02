// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuesProjekt.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Formular zur Eingabe von neuen Projektdaten.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  11.02.17  br      Grundversion
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Terminplan
{
    using System;
    using System.Windows.Forms;

    public partial class NeuesProjekt : Form
    {
        public NeuesProjekt()
        {
            this.InitializeComponent();
        }

        #region Eigenschaften

        /// <summary>Holt das formatiwerte Startdatum des Projekts</summary>
        public string StartPrj { get; private set; }

        /// <summary>Holt den Namen des Projekts</summary>
        public string PrjName { get; private set; } = null;

        /// <summary>Holt das Startdatum des Projekts</summary>
        public DateTime PrjStart { get; private set; }

        /// <summary>Holt das Startdatum des Projekts</summary>
        public string Kommission { get; private set; }

        #endregion Eigenschaften

        #region Ereignisse
        /// <summary>Behandelt das Click-Ereignis des btnOk Kontrols.</summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="EventArgs"/> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnBtnOkClick(object sender, EventArgs e)
        {
            this.PrjName = this.ultraTextEditorPrjName.Text;                    // eingeebener Projektname
            this.PrjStart = this.ultraDateTimeEditor1.DateTime;                 // eingegebes Startdatum
            this.Kommission = this.ultraMaskedEditKommission.Text;              // eingegebene Kommissionsnummer
            this.StartPrj = this.ErstelleStartDatum();                          // Erstellt das 
            this.DialogResult = DialogResult.OK;                                // Ergebnis des Dialogs ist OK
            this.Close();                                                       // Dialog beenden
        }

        /// <summary>Behandelt das Click-Ereignis des btnCancel Kontrols.</summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="EventArgs"/> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnBtnCancelClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;                            // Ergebnis des Dialogs ist Cancel
            this.Close();                                                       // Dialog beenden
        }
        #endregion Ereignisse

        /// <summary>Erstellt das Startdatum in dem Format jjjj-mm-ttThh:mm:ss+01:00</summary>
        /// <remarks>Das Datum liegt in folgendem Fornat vor: dd.mm.jjjj</remarks>
        /// <returns>Das erstellte Startdatum.</returns>
        private string ErstelleStartDatum()
        {
            var testWert = this.PrjStart.ToString();                            // Startdatum zum Aufspalten in Zeichenkette umwandeln
            string[] separators = { ".", " " };
            var splitWert = testWert.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            var tag = splitWert[0];
            var monat = splitWert[1];
            var jahr = splitWert[2];
            var zeit = splitWert[3];

            var retWert = jahr + @"-" + monat + @"-" + tag + @"T" + zeit + @"+01:00";

            return retWert;
        }

        /// <summary>
        /// Behandelt das CheckedChanged Ereignis eines RadioButtons.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="EventArgs"/>Instanz, welche die Ereignisdaten enthält.</param>
        private void OnRadioButtonChanged(object sender, EventArgs e)
        {
            var rb = (RadioButton)sender;                                       // die Quelle ist ein RadioButton                                        
            
            // Falls der RadioButton abgewählt ist, wird die zugehörige Textbox usichtbar geschaltet, sonst sichtbar
            if (rb.Checked)
            {
                // Ermitteln, welcher RadioButton betätigt wurde
                if(rb.Tag.ToString() == @"Text")
                {
                    // Der Projektschlüssel besteht aus reinem Text
                    this.ultraTextEditorNormalerText.Visible = true;            // Texteditor ausblenden
                    this.ultraMaskedEditKommission.Visible = false;             // maskierte Eingabe für EST-Kommissionsnummeer einblenden
                }
                else
                {
                    // Der Projektschlüssel besteht aus einem 
                    this.ultraTextEditorNormalerText.Visible = false;           // Texteditor einblenden
                    this.ultraMaskedEditKommission.Visible = true;              // maskierte Eingabe für EST-Kommissionsnummeer ausblenden
                }
            }
        }
    }
}
