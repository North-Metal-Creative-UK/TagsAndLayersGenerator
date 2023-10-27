// (c) Copyright North Metal Creative, 2023. All rights reserved.

#if UNITY_EDITOR

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NorthMetalCreative.UnityEngine
{
    [Serializable]
    public class SettingsJson
    {
        public bool PassivelyGenerateScripts;
    }

    public static class TagsAndLayersGeneratorSettings
    {
        public static SettingsJson _Settings;
        private static string SettingsJsonFilePath = GetFilePathFromFileName(TagsAndLayersGeneratorConstants.SettingsJsonFileName);

        public static SettingsJson ReadSettings()
        {
            string json = null;

            if (!string.IsNullOrEmpty(SettingsJsonFilePath) && SettingsJsonFilePath != string.Empty)
                json = File.ReadAllText(SettingsJsonFilePath);

            if (string.IsNullOrEmpty(json) || json == "{}") // If the file is empty
            {
                var newSettings = new SettingsJson();
                string newJson = JsonUtility.ToJson(newSettings);

                string relativePathId = AssetDatabase.FindAssets($"t:Script {nameof(TagsAndLayersGeneratorSettings)}")[0];
                string path = AssetDatabase.GUIDToAssetPath(relativePathId);
                path = path.Remove(path.LastIndexOf("/"));

                File.WriteAllText(path + $"/{TagsAndLayersGeneratorConstants.SettingsJsonFileName}", newJson);

                newSettings.PassivelyGenerateScripts = false;
                AssetDatabase.Refresh();

                return newSettings;
            }

            return JsonUtility.FromJson<SettingsJson>(json);
        }

        public static void WriteSettings(SettingsJson settings) => File.WriteAllText(SettingsJsonFilePath, JsonUtility.ToJson(settings));

        public static string GetFilePathFromFileName(string fileName) 
        {
            string[] search = Directory.GetFiles(Application.dataPath, fileName, SearchOption.AllDirectories);

            if (search.Length == 0)
                return string.Empty;

            return search[0];
        }

        private static string GetCurrentFileName([System.Runtime.CompilerServices.CallerFilePath] string fileName = null)
        {
            return fileName;
        }
    }
}

#endif