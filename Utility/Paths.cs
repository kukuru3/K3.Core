using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace K3 {
    static public partial class Paths {

        /// <summary>Aware of whether we are checking for a build or editor</summary>
        static public string GetLogFilePath() {
            #if UNITY_EDITOR
            return Path.Combine(GetLogDirectoryPath(), "Editor.log");
            #else
            return Path.Combine(GetLogDirectoryPath(), "Player.log");
            #endif
        }

        public static string GetFolderOf(string path) {
            var di = new DirectoryInfo(path);
            if (di.Exists) return path;
            var fi = new FileInfo(path);
            if (fi.Exists) return fi.Directory.FullName;
            throw new InvalidOperationException($"Cannot get folder of path {path} because it doesn't exist");
        }

        static public string GetLogDirectoryPath() {
            #if UNITY_EDITOR
            return Path.GetFullPath(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "..", "Local", "Unity", "Editor"
            ));
            #else
            #if UNITY_STANDALONE_WIN
            return Path.GetFullPath(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "..",
                "LocalLow",
                Application.companyName,
                Application.productName
            ));
            #endif
            #endif
            throw new NotImplementedException("Unsupported on this platform");
        }
    }


    #if UNITY_EDITOR
    static public partial class Paths {
        static public class Editor { 
        /// <summary>"Project root"is the folder that CONTAINS THE ASSETS folder</summary>
        static public string GetProjectRoot() => new DirectoryInfo(Application.dataPath).Parent.FullName;

        static public string GetProjectFolder(params string[] relativeFolders) {
            var l = new List<string>();
            l.Add(GetProjectRoot());
            l.AddRange(relativeFolders);
            return Path.Combine(l.ToArray());
        }

        //static public string EditorGetAssetsFolder

        [UnityEditor.MenuItem("K3 Tools/Dump folders")]
        static void DumpFolders() {
            Debug.Log($"editor project root : {GetProjectFolder("Data")}");
            Debug.Log($"editor logfile: {GetLogFilePath()}");
        }
        }
    }
    #endif
}