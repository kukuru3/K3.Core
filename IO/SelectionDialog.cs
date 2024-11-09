#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace K3.IO {
    public static class DialogUtilities {

        public static string OpenFilePanel(string title, string initialFolder, params (string desc, string filter)[] filters) {
            #if UNITY_EDITOR
            
            var path = string.Empty;

            #if UNITY_STANDALONE_WIN
            var crunchedFilters = new List<string>();
            foreach ( (var desc, var f) in filters ) { crunchedFilters.Add(desc); crunchedFilters.Add(f); }

            path = EditorUtility.OpenFilePanelWithFilters(title, initialFolder, crunchedFilters.ToArray());
            
            if (!string.IsNullOrWhiteSpace(path)) {
                var fi = new FileInfo(path);
                if (!fi.Exists) throw new System.InvalidOperationException("File does not exist");
                return fi.FullName;
            } else {
                return null;
            }
            #else
            throw new System.NotImplementedException();
            #endif

            #else
            throw new System.NotImplementedException("Editor only!");
            #endif
        }
    }
}