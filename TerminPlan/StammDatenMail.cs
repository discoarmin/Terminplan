// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StammFatenMail.Cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Klasse zum Versenden von EMails �ber Outlook.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  �nderung
//                  --------  ------  ------------------------------------
//                  30.04.17  br      Grundversion
//      </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Terminplan
{
    using System;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;
    using Microsoft.Office.Interop.Outlook;

    using Outlook = Microsoft.Office.Interop.Outlook;

    public partial class StammDaten : Form
    {
        /// <summary>Erstellt eine Mail mit Outlook.</summary>
        /// <returns><c>true</c>, wenn erfolgreich, sonst <c>false</c></returns>
        private bool MailNachrichtErstellen()
        {
            // Ermitteln, ob Outlook installiert ist. Wenn nicht, wird eine Fehlermeldung ausgegeben
            var retwert = DienstProgramme.IstOutlookInstalliert();

            if (!retwert)
            {
                // Meldung ausgeben, dass Outlook nicht installiert ist
                var meldung = @"Outlook ist nicht installiert." + System.Environment.NewLine +
                    @"Die Mail kann deshalb nicht verschickt werden.";
                const string Ueberschrift = @"EMail-Client nicht gefunden";
                MessageBox.Show(this, meldung, Ueberschrift, MessageBoxButtons.OK);
                return retwert;                                                 // Falls Outlook nicht installiert ist, kann hier abgebrochen werden
            }

            var startForm = (StartForm)MdiParent;                               // Das Elternfenster holen
            startForm.ultraStatusBarStart.Panels[0].Text = @"Lade Outlook";     // Statusmeldung ausgeben

            var outlookApp = new Outlook.Application();                         // Neue Instanz von Outlook erzeugen
            var mailItem = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);

            var currentUser = outlookApp.Session.CurrentUser.AddressEntry;      // Eigene Email-Addresse ermitteln
            //mailItem.GetInspector.Activate();

            var signature = LeseSignature();                                    // Signatur der eigenen Mailadresse ermitteln
            var anschrift = @"Sehr geehrte Damen und Herren,";

            mailItem.Subject = "Hier Betreff eingaben";
            mailItem.To = this.ultraFormattedLinkLabel1.Value.ToString();
            //mailItem.Body = @"Sehr geehrte Damen und Herren,";
            mailItem.Body = anschrift +
                Environment.NewLine +
                Environment.NewLine +
                signature;
            mailItem.Importance = OlImportance.olImportanceNormal;
            mailItem.Display(true);

            startForm.ultraStatusBarStart.Panels[0].Text = @"Bereit ...";
            return retwert;
        }

        /// <summary>Ermittelt die Signatur des Benutzers.</summary>
        /// <returns>Die gefundene Signatur, wenn keine gfunden wurde ein Leerstring</returns>
        private string LeseSignature()
        {
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Signatures"; // Verzeichnis, wo die Signaturern abgelegt sind
            var signature = string.Empty;                                       // Gefundene Sinatur
            var diInfo = new DirectoryInfo(appDataDir);                         // Alle vorhandenen Signaturen ermitteln
            if (!diInfo.Exists) return signature;                               // Falls keine Verzeichnisinfo vorhanden sind, kann abgebrochen werden

            var fiSignature = diInfo.GetFiles("*.txt");                         //ben die Dateiendung '.htm'

            if (fiSignature.Length <= 0) return signature;                      // Falls keine Signaturen vorhanden sind, kann abgebrochen werden
            var sr = new StreamReader(
                fiSignature[0].FullName,
                Encoding.Default);                                              // Es wird die erste vorhandene Signatur genommen
            signature = sr.ReadToEnd();                                         // Signatur laden

            //// �berpr�fen, ob es sich um eine leere Signatur handelt
            //if (!string.IsNullOrEmpty(signature))
            //{
            //    // Signatur enth�lt Text, Signatur zusammenstellen
            //    var fileName = fiSignature[0].Name.Replace(fiSignature[0].Extension, string.Empty);
            //    signature = signature.Replace(fileName + "_files/", appDataDir + "/" + fileName + "_files/");
            //}

            return signature;                                                   // Signatur zur�ckgaben
        }
    }
}