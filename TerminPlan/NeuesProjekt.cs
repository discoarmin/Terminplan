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
        private string prjName = null;
        private DateTime prjStart;

        public NeuesProjekt()
        {
            this.InitializeComponent();
        }

        #region Eigenschaften
        /// <summary>Holt den Namen des Projekts</summary>
        public string PrjName
        {
            get
            {
                return prjName;
            }

            private set
            {
                prjName = value;
            }
        }


        /// <summary>Holt das Startdatum des Projekts</summary>
        public DateTime PrjStart
        {
            get
            {
                return prjStart;
            }

            private set
            {
                prjStart = value;
            }
        }

        #endregion Eigenschaften

        /// <summary>Behandelt das Click-Ereignis des btnOk Kontrols.</summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="EventArgs"/> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnBtnOkClick(object sender, EventArgs e)
        {
            this.PrjName = this.ultraTextEditor1.Text;                          // eingeebener Projektname
            this.PrjStart = this.ultraDateTimeEditor1.DateTime;                 // eingegebes Startdatum
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

 
    }
}
