// (c) Copyright North Metal Creative, 2023. All rights reserved.

#if UNITY_EDITOR

using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace NorthMetalCreative.UnityEngine
{
    public class TagsAndLayersFileGenerator : IDisposable
    {
        private static string PathToThisFile;
        private static string PreviousFileContent;

        [InitializeOnLoadMethod] 
        private static void Init() => EditorApplication.update += OnEditorUpdate;

        private static void OnEditorUpdate()
        {
            if (TagsAndLayersGeneratorEditor.GetSettings() != null 
                && TagsAndLayersGeneratorEditor.GetSettings().PassivelyGenerateScripts) 
                TriggerGeneration();
        }

        public static void TriggerGeneration()
        {
            if (!TrySetPathToThisFile()) return;

            UpdateFile(TagsAndLayersGeneratorConstants.TagsFileName, 
                BuildFileContentForTags(TagsAndLayersGeneratorConstants.TagsFileName, UnityEditorInternal.InternalEditorUtility.tags));
            
            UpdateFile(TagsAndLayersGeneratorConstants.LayersFileName, 
                BuildFileContentForLayers(TagsAndLayersGeneratorConstants.LayersFileName, UnityEditorInternal.InternalEditorUtility.layers));
        }

        private static bool TrySetPathToThisFile()
        {
            if (string.IsNullOrEmpty(PathToThisFile))
            {
                string className = nameof(TagsAndLayersFileGenerator) + ".cs";
                var resource = Directory.GetFiles(Application.dataPath, className, SearchOption.AllDirectories)[0];

                PathToThisFile = (resource.Length != 0) ?
                    resource.Replace(className, "").Replace("\\", "/")
                    : null;

                if (PathToThisFile == null)
                {
                    Debug.LogError("Couldn't find path to TagsAndLayersFileGenerator.cs");
                    return false;
                }
            }
            return true;
        }

        private static void UpdateFile(string fileName, string fileContent)
        {
            if (PreviousFileContent == fileContent) return;
            
            File.WriteAllText(Path.Combine(PathToThisFile, fileName), fileContent);
            PreviousFileContent = fileContent;   
        }


        private static string BuildFileContentForTags(string fileName, string[] items)
        {
            string classDefDecl = "namespace NorthMetalCreative.UnityEngine \n{\n";
            classDefDecl += $"    public static class {fileName.Remove(fileName.Length - 3)}\n    {{";
            for (int i = 0; i < items.Length; i++)
                classDefDecl += "\n        public const string Tag_" + Regex.Replace(items[i], "[^a-zA-Z0-9]", "") + " = @\"" + items[i] + "\";\n";
            classDefDecl += "    }\n}\n";
            return classDefDecl;
        }

        private static string BuildFileContentForLayers(string fileName, string[] items)
        {
            string enumDefDecl = "namespace NorthMetalCreative.UnityEngine \n{\n";
            enumDefDecl += $"    public enum {fileName.Remove(fileName.Length - 3)}\n    {{";

            bool hasUserLayer3Missing = false;
            for (int i = 0; i < items.Length; i++)
            {
                if (hasUserLayer3Missing)
                    enumDefDecl += "\n        Layer_" + Regex.Replace(items[i], "[^a-zA-Z0-9]", "") + " = " + (i+1) + ",\n";
                else 
                    enumDefDecl += "\n        Layer_" + Regex.Replace(items[i], "[^a-zA-Z0-9]", "") + " = " + i + ",\n";

                // Annoying - Unity likes to leave User layer 3 empty, then skip builtin layer 4, 5 which causes the index to shift, so here's a duck-tape fix to skip and adjust the value moving forward
                if (items[i] == "Ignore Raycast" && items[i + 1] == "Water")
                    hasUserLayer3Missing = true;
            }
            enumDefDecl += "    }\n}\n";
            return enumDefDecl;
        }

        public void Dispose()
        {
            EditorApplication.update -= OnEditorUpdate;
        }
    }
}
#endif
