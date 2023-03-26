using UnityEngine;
using UnityEditor;

#nullable enable
namespace ConnieSDK.Editor
{
    [EditorWindowTitle(title ="ConnieSDK Object Serialization")]
    public class UnitSerEditor : EditorWindow
    {
        public static UnitSerEditor? Window { get; protected set; }
        public static UnitSerSettings? Settings { get; protected set; }

        protected static string OutputDir
        {
            get => Settings?.OutputDir ?? System.IO.Path.Join(System.Environment.CurrentDirectory, "Output");
            set
            {
                if (Settings is null) return;

                Settings.OutputDir = value;
            }
        }

        public static GameObject? selectedPrefab { get; protected set; }
        public static ObjectType type = ObjectType.Generic;
        public static string outputName = string.Empty;

        [MenuItem("Window/ConnieSDK/Object Serializer")]
        static void Init()
        {
            Window = (UnitSerEditor)GetWindow(typeof(UnitSerEditor));
            Window.Show();
        }

        private void OnGUI()
        {
            if (Settings is null)
            {
                Settings = FindSettings();
            }

            GUILayout.Label("Unit Serializer", EditorStyles.boldLabel);

            string newDir = EditorGUILayout.DelayedTextField("Output Directory", OutputDir);
            GameObject newselection = (GameObject)EditorGUILayout.ObjectField("Prefab", selectedPrefab, typeof(GameObject), true);
            type = (ObjectType)EditorGUILayout.EnumPopup("Object Type", type);

            EditorGUILayout.Space(10, true);

            EditorGUILayout.BeginHorizontal();
            bool serialize = GUILayout.Button("Serialize", GUILayout.MinWidth(50), GUILayout.MaxWidth(100));
            string newName = GUILayout.TextField(outputName, GUILayout.MinWidth(50));
            EditorGUILayout.EndHorizontal();

            if (!newDir.Equals(OutputDir))
            {
                OutputDir = newDir;
                ConnieSerializer.SetAssetDirectory(OutputDir);
                return;
            }

            if (newselection is GameObject go && !go.Equals(selectedPrefab))
            {
                outputName = go.name;
                selectedPrefab = newselection;
                return;
            }
            else if (newselection is null)
            {
                outputName = string.Empty;
                selectedPrefab = newselection;
                return;
            }

            if (!newName.Equals(outputName))
            {
                outputName = newName;
                return;
            }

            if (serialize && selectedPrefab is not null)
            {
                ConnieSerializer.SetAssetDirectory(OutputDir);
                ConnieSerializer.SerializeObject(selectedPrefab, ObjectType.Generic, outputName, "");
            }
        }

        protected static UnitSerSettings FindSettings()
        {
            if (AssetDatabase.LoadMainAssetAtPath("Assets/ConnieSDK/UnitSerSettings.asset") is UnitSerSettings uss)
            {
                //Debug.Log("Found existing Serializer settings");

                return uss;
            }

            AssetDatabase.CreateFolder("Assets", "ConnieSDK");

            UnitSerSettings instance = ScriptableObject.CreateInstance<UnitSerSettings>();

            AssetDatabase.CreateAsset(instance, "Assets/ConnieSDK/UnitSerSettings.asset");

            return instance;
        }
    }
}
