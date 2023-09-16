using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
            static Editor() {
                _unityProjectRoot = new DirectoryInfo(Application.dataPath).Parent.FullName;
                _masterRoot = new DirectoryInfo(Application.dataPath).Parent.Parent.FullName;
            }
            static string _unityProjectRoot;
            static string _masterRoot;
            /// <summary>"Project root"is the folder that CONTAINS THE ASSETS folder</summary>
            static public string GetProjectRoot() => _unityProjectRoot;

            static public string GetMasterPathOfEnitreProject() => _masterRoot;
            
            static public string GetPathInUnityProject(params string[] relativePath) {
                var l = new List<string>();
                l.Add(GetProjectRoot());
                l.AddRange(relativePath);
                return Path.Combine(l.ToArray());
            }

            static public string MetricsFolder => Path.Combine(GetMasterPathOfEnitreProject(), "Metrics");

            [UnityEditor.MenuItem("K3 Tools/Dump folders")]
            static void DumpFolders() {
                Debug.Log($"editor project root : {GetPathInUnityProject("Data")}");
                Debug.Log($"editor logfile: {GetLogFilePath()}");
            }
            
            [UnityEditor.MenuItem("K3 Tools/Navigate to editor logfile")]
            static public void HiglightEditorLogFileInExplorer() {
                var finalPath = GetLogFilePath();
                System.Diagnostics.Process.Start("explorer.exe", $"/select,{finalPath}");
            }

            [UnityEditor.MenuItem("K3 Tools/Navigate to build logfile")]
            static public void HiglightBuildLogFileInExplorer() {                
                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var finalPath = $"{userProfile}\\AppData\\LocalLow\\{Application.companyName}\\{Application.productName}\\Player.log";
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{finalPath}\"");
            }

            [UnityEditor.MenuItem("K3 Tools/Navigate to Application.persistentDataPath(editor)")]
            static public void GetAppDataPath() {
                var p = Application.persistentDataPath;
                p = p.Replace("/", "\\");
                System.Diagnostics.Process.Start("explorer.exe", $"select,\"{p}\"");
            }
        }
    }
    #endif
}