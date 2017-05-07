// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LaufendeAnwendungen.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Excel-Daten lesen und schreiben.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  �nderung
//                  --------  ------  ------------------------------------
//                  05.05.17  br      Grundversion
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Terminplan
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using Microsoft.Office.Interop.Excel;

    /// <summary>
    /// Erm�glicht .NET-Anwendungen direkten Zugriff auf die Running Object Table (Tabelle mit allen momentan laufenden COM-Objekte)
    /// </summary>
    public class LaufendeAnwendungen
    {
        /// <summary>
        /// Standardkonstruktor.
        /// </summary>
        private LaufendeAnwendungen() { }

        // Win32-API-Aufruf zum lesen der ROT
        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(uint reserved, out IRunningObjectTable pprot);

        // Win32-API-Aufruf zum Erstellen von Bindungen
        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx pctx);

        // Win32-API-Aufruf zum Ermitteln der Prozess-ID
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);

        /// <summary>
        /// Gibt einen Verweis auf eine laufendes COM-Objekt anhand ihres Anzeigenamens zur�ck.
        /// </summary>
        /// <remarks>Die Ermittlung erfolgt �ber Moniker</remarks>
        /// <param name="objectDisplayName">Anzeigename einer COM-Instanz</param>
        /// <returns>Verweis auf COM-Objekt, oder null, wenn kein COM-Objekt mit dem angegbenen Namen l�uft</returns>
        public static object GetRunningCOMObjectByName(string objectDisplayName)
        {
            IRunningObjectTable runningObjectTable = null;                      // ROT-Schnittstelle
            IEnumMoniker monikerList = null;                                    // Moniker-Auflistung

            try
            {
                // Running Object Table abfragen und nichts zur�ckgeben, wenn keine COM-Objekte laufen
                if (GetRunningObjectTable(0, out runningObjectTable) != 0 || runningObjectTable == null) return null;

                runningObjectTable.EnumRunning(out monikerList);                // Moniker abfragen
                monikerList.Reset();                                            // An den Anfang der Auflistung springen
                var monikerContainer = new IMoniker[1];                         // Array f�r Moniker-Abfrage erzeugen

                // Zeiger auf die Anzahl der tats�chlich abgefragten Moniker erzeugen
                var pointerFetchedMonikers = IntPtr.Zero;

                // Alle Moniker durchlaufen
                while (monikerList.Next(1, monikerContainer, pointerFetchedMonikers) == 0)
                {
                    IBindCtx bindInfo;                                          // Objekt f�r Bindungsinformationen
                    string displayName;                                         // Variable f�r den Anzeigenamen des aktuellen COM-Objekts
                    Guid classId;
                    CreateBindCtx(0, out bindInfo);                             // Bindungsobjekt erzeugen

                    // Anzeigename des COM-Objekts �ber den Moniker abfragen
                    monikerContainer[0].GetDisplayName(bindInfo, null, out displayName);

                    monikerContainer[0].GetClassID(out classId);

                    Marshal.ReleaseComObject(bindInfo);                         // Bindungsobjekt wird nicht mehr ben�tigt, also entsorgen

                    // Wenn der Anzeigename mit dem gesuchten �bereinstimmt ...
                    // �berpr�fen, ob die gesuchte Instanz der Anwendung vorhanden ist
                    if (displayName.IndexOf(objectDisplayName) != -1)
                    {
                        object comInstance;                                     // Variable f�r COM-Objekt = R�ckgabewert

                        // COM-Objekt �ber den Anzeigenamen abfragen
                        runningObjectTable.GetObject(monikerContainer[0], out comInstance);
                        return comInstance;                                     // gefundenes COM-Objekt zur�ckgeben
                    }
                }
            }
            catch
            {
                // Nichts zur�ckgeben, wenn ein Fehler aufgetreten ist
                return null;
            }
            finally
            {
                // Ggf. COM-Verweise entsorgen
                if (runningObjectTable != null) Marshal.ReleaseComObject(runningObjectTable);
                if (monikerList != null) Marshal.ReleaseComObject(monikerList);
            }
            // Nichts zur�ckgeben
            return null;
        }

        /// <summary>
        /// Gibt eine Liste mit Anzeigenamen aller momentan laufenden COM-Objekte zur�ck.
        /// </summary>
        /// <returns>Liste mit Anzeigenamen</returns>
        public static IList<string> GetRunningCOMObjectNames()
        {
            // Auflistung der Anzeigenamen erzeugen
            IList<string> result = new List<string>();

            IRunningObjectTable runningObjectTable = null;                      // Tabelle zur Aufnahme von Informationen der laufenden COM-Instanzen
            IEnumMoniker monikerList = null;                                    // Moniker-Auflistung

            try
            {
                // Running Object Table abfragen und nichts zur�ckgeben, wenn keine COM-Objekte laufen
                if (GetRunningObjectTable(0, out runningObjectTable) != 0 || runningObjectTable == null) return null;

                runningObjectTable.EnumRunning(out monikerList);                // Moniker abfragen
                monikerList.Reset();                                            // An den Anfang der Auflistung springen

                // Array f�r Moniker-Abfrage erzeugen
                var monikerContainer = new IMoniker[1];                         // Array f�r Moniker-Abfrage erzeugen

                // Zeiger auf die Anzahl der tats�chlich abgefragten Moniker erzeugen
                var pointerFetchedMonikers = IntPtr.Zero;

                // Alle Moniker durchlaufen
                while (monikerList.Next(1, monikerContainer, pointerFetchedMonikers) == 0)
                {
                    IBindCtx bindInfo;                                          // Objekt f�r Bindungsinformationen
                    string displayName;                                         // Variable f�r den Anzeigenamen des aktuellen COM-Objekts

                    CreateBindCtx(0, out bindInfo);                             // Bindungsobjekt erzeugen

                    // Anzeigename des COM-Objekts �ber den Moniker abfragen
                    monikerContainer[0].GetDisplayName(bindInfo, null, out displayName);
                    Marshal.ReleaseComObject(bindInfo);                         // Bindungsobjekt wird nicht mehr ben�tigt, also entsorgen

                    result.Add(displayName);                                    // Anzeigenamen der Auflistung zuf�gen
                }

                return result;                                                  // Auflistung zur�ckgeben
            }
            catch
            {
                // Nichts zur�ckgeben, wenn ein Fehler aufgetreten ist
                return null;
            }
            finally
            {
                // Ggf. COM-Verweise entsorgen
                if (runningObjectTable != null) Marshal.ReleaseComObject(runningObjectTable);
                if (monikerList != null) Marshal.ReleaseComObject(monikerList);
            }
        }

        public static void KillExcelInstanceById(ref Application xlsApp)
        {
            var processId = default(IntPtr);

            // API Funktion, out val: processId
            GetWindowThreadProcessId((IntPtr)xlsApp.Hwnd, processId);           // Prozess-ID ermitteln
            var excelProcess = Process.GetProcessById(processId.ToInt32());     // Prozess erstellen

            Debug.WriteLine(processId);

            excelProcess.Kill();                                                // Excel beenden
        }
    }
}