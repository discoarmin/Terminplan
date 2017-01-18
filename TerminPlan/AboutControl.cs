﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utilities.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Dienstprogramme für Terminplan.
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
    using System.Drawing;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.Reflection;
    using System.IO;
    
    public partial class AboutControl : UserControl
    {
        #region Konstruktor

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="TerminPlanForm" /> Klasse.
        /// </summary>
        public AboutControl()
        {
            InitializeComponent();
            this.OnInitializeUi();                                              // Oberfläche initialisieren
        }

        #endregion Konstruktor

        #region Methoden
        #region OnInitializeUi
        /// <summary>
        /// Initialisiert die Oberfläche.
        /// </summary>
        private void OnInitializeUi()
        {
            // Namen der Anwendung aus den Ressourcen laden und in das Textfeld eintragen
            this.lblAppName.Text = Properties.Resources.Title;
            
            // Beschreibungn der Anwendung aus den Ressourcen laden und in das Textfeld eintragen
            this.lblDescription.Text = Properties.Resources.ShortDescription;
            
            // Firmennamen aus des Ressourcen laden und in das Textfeld eintragen
            this.lblCompany.Text = string.Format("{0}: {1}", DienstProgramme.GetLocalizedString(@"Publisher"), Firma);
            
            // Copyright-Info aus den Ressourcen laden und in das Textfeld eintragen
            this.lblCopyright.Text = DienstProgramme.GetLocalizedString(Copyright);
            
            // Versionsnummer in das Textfeld eintragen
            this.lblVersion.Text = string.Format("{0}: {1}", DienstProgramme.GetLocalizedString(@"Version"), AboutControl.Version);

            //Stream stream = DienstProgramme.GetEmbeddedResourceStream(@"TerminPlan.Images.Logo.PNG");
            var stream = DienstProgramme.GetEmbeddedResourceStream(@"Logo.PNG");
            Image logo = Image.FromStream(stream);
            this.pbLogo.Size = logo.Size;
            this.pbLogo.Image = logo;
        }
        #endregion OnInitializeUi
        #endregion Methoden

        #region Eigenschaften
        #region ApplicationName
        /// <summary> Holt den Namen der Anwendung </summary>
        internal static string ApplicationName
        {
            get { return DienstProgramme.GetAssemblyAttribute<AssemblyTitleAttribute>(a => a.Title); }
        }
        #endregion ApplicationName

        #region Beschreibung
        /// <summary> Holt die Beschreibung der Anwendung </summary>
        internal static string Beschreibung
        {
            get { return DienstProgramme.GetAssemblyAttribute<AssemblyDescriptionAttribute>(a => a.Description); }
        }
        #endregion Beschreibung

        #region Firma
        /// <summary> Holt die Firma </summary>
        internal static string Firma
        {
            get { return DienstProgramme.GetAssemblyAttribute<AssemblyCompanyAttribute>(a => a.Company); }
        }
        #endregion Firma

        #region Copyright
        /// <summary> Holt die Copyright-Information. </summary>
        internal static string Copyright
        {
            get { return DienstProgramme.GetAssemblyAttribute<AssemblyCopyrightAttribute>(a => a.Copyright); }
        }
        #endregion Copyright

        #region Version
        /// <summary> Holt die Versionsinformation. </summary>
        internal static string Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }
        #endregion Version
        #endregion Eigenschaften
    }
}
