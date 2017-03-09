using System;
using System.Windows.Forms;

namespace Terminplan
{
    public partial class WasLaden : Form
    {
        /// <summary>Holt die Auswahl, welche Daten geladen werden sollen</summary>
        /// <value>die getätigte Ausswahl</value>
        /// <remarks> 1 = vorhandener Terminplan, 2 = neuer Terminplan</remarks>
        public int Auswahl { get; private set; }

        public WasLaden()
        {
            InitializeComponent();
        }

        /// <summary>Behandelt das Click-Ereignis des btnOk Kontrols.</summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="EventArgs"/> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnBtnOkClickClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;                                     // Ergebnis des Dialogs ist OK
            Close();                                                            // Dialog beenden
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
                // Soll ein neuer Terminplan geladen werden ?
                if (rb.Tag.ToString() == @"Neu")
                {
                    Auswahl = 2;                                           // Neuer Terminplan
                }
                else
                {
                    Auswahl = 1;                                           // Vorhandener Terminplan
                }
            }
        }

    }
}
