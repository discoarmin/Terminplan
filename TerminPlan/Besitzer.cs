using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Besitzer.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Verwaltet die Daten derjenigen Personen, welche Aufgaben oder Arbeitsinhalte bearbeiten dürfen.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  10.03.17  br      Grundversion
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Terminplan
{
    class Besitzer
    {
        /// <summary>Holt oder setzt den vollen Namen des Besitzers</summary>
        public string Key { get; set; }

        /// <summary>Holt oder setzt die Kurzbezeichnung des Besitzers</summary>
        public string Name { get; set; }

        /// <summary>Holt oder setzt die EMail-Addresse des Besitzers</summary>
        public string EmailAddresse { get; set; }

        /// <summary>Holt oder setzt einen Wert, der angibt, ob der Besitzer ichtbar ist</summary>
        public bool Sichtbar { get; set; }
    }
}
