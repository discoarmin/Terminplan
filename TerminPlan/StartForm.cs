// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StartForm.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   MIDI-Fenster zur Aufnahme des Terminplans und der Stammdaten.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  08.03.17  br      Grundversion
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Terminplan
{
    using System;
    using System.Globalization;
    using System.Windows.Forms;

    public partial class StartForm : Form
    {
        #region Konstruktor
        /// <summary> Initialisiert eine neue Instanz der <see cref="StartForm"/> Klasse.
        /// </summary>
        public StartForm()
        {
            var culture = new CultureInfo("de-DE");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

            InitializeComponent();
        }

        #endregion Konstruktor

        /// <summary>
        /// Löst das <see cref="System.Windows.Forms.Form.Load" /> Ereignis aus.
        /// </summary>
        /// <param name="e">Ein <see cref="T:System.EventArgs" /> welches die Ereignisdaten enthält.</param>
        protected override void OnLoad(EventArgs e)
        {
            // Hier müssen die beiden Formulare geladen werden
            // a) Stammdaten
            var fs = new StammDaten
            {
                Text = @"Stammdaten",
                MdiParent = this
            };

            // b) Formular für den Terminplan
            var ft = new TerminPlanForm
            {
                Text = @"Terminplan",
                MdiParent = this
            };

            fs.FrmTerminPlan = ft;                                              // Dem Projektplan die Stammdaten bekannt machen
            ft.FrmStammDaten = fs;                                              // Den Stammdaten den Projektplan bekant machen

            // Formulare anzeigen
            fs.Show();
            ft.Show();

            base.OnLoad(e);
        }
    }
}
