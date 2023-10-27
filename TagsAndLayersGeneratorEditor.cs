// (c) Copyright North Metal Creative, 2023. All rights reserved.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NorthMetalCreative.UnityEngine
{
    public class TagsAndLayersGeneratorEditor : EditorWindow
    {
        private Dictionary<string, int> _Layers = new Dictionary<string, int>();
        private Dictionary<string, string> _Tags = new Dictionary<string, string>();
        private static SettingsJson _Settings;

        #region Editor Window
        [MenuItem("Tools/NorthMetalCreative/Tags and Layers Generator")]
        public static void ShowEditor()
        {
            EditorWindow window = GetWindow<TagsAndLayersGeneratorEditor>();
            window.titleContent = new GUIContent("Tags and Layers Generator");
            window.minSize = new Vector2(500, 700);
        }

        public void CreateGUI()
        {
            _Settings = TagsAndLayersGeneratorSettings.ReadSettings();
            RenderControls(rootVisualElement);
            RenderGeneratedListsView(rootVisualElement);
        }

        private void RenderControls(VisualElement elem)
        {
            elem.Add(new Button(SetAutoRegnerateScripts)
            {
                name = "passiveReloadToggle",
                text = "Passively Generate Regenerate Scripts - " + (_Settings.PassivelyGenerateScripts ? "ON" : "OFF"),
                tooltip = "This will have Unity regenerate Layers.cs and Tags.cs anytime you press play, tab back into Unity or trigger any script to be recompiled."
            });

            elem.Add(new Button(Generate)
            {
                name = "GenerateBtn",
                text = "Generate",
                visible = !_Settings.PassivelyGenerateScripts,
                tooltip = "This will generate Layers.cs and Tags.cs containing appropriate strings and enums to reference in code"
            });

            elem.Add(new Button(DeleteScripts)
            {
                name = "DeleteListsBtn",
                text = "Delete Generated Files",
                tooltip = "This will delete the generated Layers.cs and Tags.cs files"
            });
        }

        private void RenderGeneratedListsView(VisualElement elem)
        {
            var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
            elem.Add(splitView);

            var leftPane = new VisualElement();
            var rightPane = new VisualElement();

            splitView.Add(leftPane);
            splitView.Add(rightPane);

            leftPane.Add(new Label("\nTags\n"));
            var leftList = new ListView(RenderList(ScrapeTags, _Tags));
            leftPane.Add(leftList);

            rightPane.Add(new Label("\nLayers\n"));
            var rightList = new ListView(RenderList(ScrapeLayers, _Layers));
            rightPane.Add(rightList);
        }

        private List<string> RenderList<T>(Action Scraper, Dictionary<string, T> ListData)
        {
            List<string> items = new List<string>();

            Scraper();
            foreach (var data in ListData)
            {
                items.Add($"{data.Key} -> {data.Value}");
            }

            return items;
        }

        // Small hack for re-rendering everything in the editor window
        // Fine for now as editor wise there isn't many components
        private void RefreshLists() 
        {
            rootVisualElement.Clear();
            CreateGUI();
        }
        #endregion Editor Window

        #region Tags And Layer File Management Logic
        private void Generate()
        {
            if (!_Settings.PassivelyGenerateScripts)
            {
                TagsAndLayersFileGenerator.TriggerGeneration();
            }

            AssetDatabase.Refresh();
            RefreshLists();
        }

        private void DeleteScripts()
        {
            DeleteScript(TagsAndLayersGeneratorConstants.TagsFileName);
            DeleteScript(TagsAndLayersGeneratorConstants.LayersFileName);
            AssetDatabase.Refresh();
        }

        private void DeleteScript(string fileName, bool refreshAssetDatabaseForFile = false)
        {
            string file = TagsAndLayersGeneratorSettings.GetFilePathFromFileName(fileName);

            if (!string.IsNullOrEmpty(file))
            {
                File.Delete(file);

                if (refreshAssetDatabaseForFile)
                {
                    AssetDatabase.Refresh();
                }
            }
        }

        private void ScrapeTags()
        {
            _Tags?.Clear();
            Type tags = Type.GetType(TagsAndLayersGeneratorConstants.TagsFileNamespace);

            if (tags == null) return;
            var props = tags.GetMembers(BindingFlags.Public | BindingFlags.Static);

            foreach (var prop in props)
                _Tags[prop.Name] = (string)((FieldInfo)prop).GetRawConstantValue();
        }

        private void ScrapeLayers()
        {
            _Layers?.Clear();
            Type layers = Type.GetType(TagsAndLayersGeneratorConstants.LayersFileNamespace);

            if (layers == null) return;
            string[] names = Enum.GetNames(layers);
            Array vals = Enum.GetValues(layers);

            for (int i = 0; i < names.Length; i++)
                _Layers[names[i]] = (int)vals.GetValue(i);
        }
        #endregion Tags And Layer File Management Logic

        #region Misc
        public static SettingsJson GetSettings() => _Settings;
 
        private void SetAutoRegnerateScripts() 
        { 
            _Settings.PassivelyGenerateScripts = !_Settings.PassivelyGenerateScripts;
            TagsAndLayersGeneratorSettings.WriteSettings(_Settings);
            RefreshLists();
        }
        #endregion Misc
    }
}
#endif