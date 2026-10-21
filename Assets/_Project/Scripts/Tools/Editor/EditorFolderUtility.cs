using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LightShadow.Tools
{
    internal static class EditorFolderUtility
    {
        // Creates "Assets/A/B/C" style folders recursively. Safe to call repeatedly.
        public static void EnsureFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder)) return;

            string parent = Path.GetDirectoryName(folder);
            string leaf = Path.GetFileName(folder);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf)) return;

            EnsureFolder(parent.Replace('\\', '/'));
            AssetDatabase.CreateFolder(parent.Replace('\\', '/'), leaf);
        }
    }
}
