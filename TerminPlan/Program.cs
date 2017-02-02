// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Startet die Anwendung.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  06.01.17  br      Grundversion
//                  19.01.17  br      Begrüßungsbildschirm hinzugefügt
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Terminplan
{
    using System;
    using System.Windows.Forms;
    using System.Threading;

    public static class Program
    {
        public static SplashScreen StartScreen;   
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Begrüßungsbildschirm anzeigen
            var splashThread = new Thread(new ThreadStart 
                (
                    delegate
                        {
                            StartScreen = new SplashScreen();
                            Application.Run(StartScreen);
                        }
                ));                                                             // Thread zum Starten des Begrüßungsbildschirm

            splashThread.SetApartmentState(ApartmentState.STA);
            splashThread.Start();                                               // Thread zum Anzeigen des BegrüßungsBildschirm starten

            // Hauptanwendung starten
            var mainForm = new TerminPlanForm();
            mainForm.Load += OnMainFormLoad;                                    // Damit vor dem 1. Anzeigen des Hauptfensters der Begrüßungsbildschirm geschlossen wird
            Application.Run(mainForm);                                          // Hauptanwendung starten
        }

        /// <summary>
        /// Wird aufgerufen, wenn bevor das Hauptformular das erste Mal angezeigt wird.
        /// </summary>
        /// <param name="sender">Das aufrufende Element</param>
        /// <param name="e">Die <see cref="EventArgs"/> Instanz, welche die Ereignisdaten enthält.</param>
        public static void OnMainFormLoad(object sender, EventArgs e)
        {
            // Begrüßungsbildaschirm schließen
            if (StartScreen != null)
            {
                StartScreen.Invoke(new Action(StartScreen.Close));
            }
        }
    }
}
