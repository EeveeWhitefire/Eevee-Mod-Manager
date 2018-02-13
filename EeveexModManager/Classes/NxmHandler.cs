﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Win32;

using EeveexModManager.Classes.JsonClasses;
using EeveexModManager.Classes.DatabaseClasses;
using EeveexModManager.Interfaces;
using EeveexModManager.Services;

namespace EeveexModManager.Classes
{
    public class NxmHandler
    {
        private Json_Config _config;
        private Service_JsonParser _jsonParser;
        private DatabaseContext _db;

        public NxmHandler(Json_Config jsc, Service_JsonParser jsonParser, CheckBox IsAssociated_CheckBox, DatabaseContext db)
        {
            _jsonParser = jsonParser;
            _config = jsc;
            _db = db;

            if (IsUrlAssociated("nxm") != _config.Nxm_Handled)
                AssociationManagement(IsUrlAssociated("nxm"), IsAssociated_CheckBox, _db.GetCollection<Db_Game>("games").FindAll());
        }

        void AssociateNxmFile(string Extension, string KeyName, string OpenWith, string FileDescription)
        {
            string strFileId = Extension + "_File_Type";

            try
            {
                Registry.SetValue(@"HKEY_CLASSES_ROOT\" + "." + Extension, null, strFileId);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\" + strFileId, null, FileDescription, RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\" + strFileId + @"\DefaultIcon", null, OpenWith + ",0", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\" + strFileId + @"\shell\open\command", null, "\"" + OpenWith + "\" \"%1\"", RegistryValueKind.String);
            }
            catch (UnauthorizedAccessException)
            {
                throw new InvalidOperationException("Something (usually your antivirus) is preventing the program from interacting with the registry and changing the file associations.");
            }
        }

        void UnassociateNxmFile(string name)
        {
            string strFileId = name + "_File_Type";
            string[] strKeys = Registry.ClassesRoot.GetSubKeyNames();
            if (Array.IndexOf<string>(strKeys, strFileId) != -1)
            {
                Registry.ClassesRoot.DeleteSubKeyTree(strFileId);
                Registry.ClassesRoot.DeleteSubKeyTree("." + name);
            }
        }

        /// <summary>
        /// Determines if the specified URL protocol is associated with the client.
        /// </summary>
        /// <param name="p_strUrlProtocol">The protocol of the URL for which it is to be determined
        /// whether it is associated with the client.</param>
        /// <returns><c>true</c> if the URL protocol is associated with the client;
        /// <c>false</c> otherwise.</returns>
        bool IsUrlAssociated(string p_strUrlProtocol)
        {
            string key = Registry.GetValue(@"HKEY_CLASSES_ROOT\" + p_strUrlProtocol, null, null) as string;
            return !String.IsNullOrEmpty(key);
        }

        void AssociateNxmUrl(string name, string desc, string exePath)
        {
            string strUrlId = "URL:" + desc;
            Registry.SetValue(@"HKEY_CLASSES_ROOT\" + name, null, strUrlId, RegistryValueKind.String);
            Registry.SetValue(@"HKEY_CLASSES_ROOT\" + name, "URL Protocol", "", RegistryValueKind.String);
            Registry.SetValue(@"HKEY_CLASSES_ROOT\" + name + @"\DefaultIcon", null, exePath + ",101", RegistryValueKind.String);
            Registry.SetValue(@"HKEY_CLASSES_ROOT\" + name + @"\shell\open\command", null, "\"" + exePath + "\" \"%1\"", RegistryValueKind.String);
        }
        void UnassociateNxmUrl(string name)
        {
            string[] strKeys = Registry.ClassesRoot.GetSubKeyNames();
            if (Array.IndexOf(strKeys, name) != -1)
                Registry.ClassesRoot.DeleteSubKeyTree(name);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        public void AssociationManagement(bool isAssociated, CheckBox cbx, IEnumerable<Db_Game> games)
        {
            bool newState = !isAssociated;
            if (!isAssociated)
            {
                //AssociateNxmFile(".nxm", "NXM_File_Handler", Directory.GetCurrentDirectory() + @"\" + "EeveexModManager.exe", "NXM File");
                foreach (var item in games)
                {
                    AssociateNxmUrl($"nxm://{item.Name_Nexus}", "Nexus Url", _config.Installation_Path + @"\" + "EeveexModManager.exe");
                }
                cbx.IsChecked = newState;
            }
            else
            {
                foreach (var item in games)
                {
                    UnassociateNxmUrl($"nxm://{item.Name_Nexus}");
                }
                cbx.IsChecked = newState;

            }
            _config.Nxm_Handled = newState;
            _jsonParser.UpdateJson(_config);
        }

    }
}
