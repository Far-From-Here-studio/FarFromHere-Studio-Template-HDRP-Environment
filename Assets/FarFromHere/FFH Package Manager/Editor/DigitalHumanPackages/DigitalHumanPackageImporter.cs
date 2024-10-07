using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace FFH.PackageManager
{
    [InitializeOnLoad]
    public class DigitalHumanPackageImporter : FFHPackageImporterWindow
    {
        private const string PACKAGE_DATA_NAME = "DigitalHumanPackageData";
        public static DigitalHumanPackageImporter window;

        protected override void OnEnable()
        {
            base.OnEnable();
            if(!packageData) InitializePackageData(PACKAGE_DATA_NAME);
        }


        [MenuItem("FarFromHereStudio/Packages Installer/Digital Human")]
        public static void Init()
        {
            //LoadAndInitializePackageData();
            window = GetWindow<DigitalHumanPackageImporter>();
            window.titleContent = new GUIContent("Digital Human Packages Installer");
            window.Show();
        }

        void OnGUI()
        {
            if (packageData == null)
            {
                EditorGUILayout.HelpBox("Package data not loaded. Please check the console for errors.", MessageType.Error);
                return;
            }

            allPackagesInstalled = true;
            GUILayout.Label("Project's Core Packages", EditorStyles.boldLabel);

            foreach (var package in packageData.Packages)
            {
                packageManagerUtilities.DrawPackageGUI( package, packageData);
                
                if (!package.InstalledPackages) allPackagesInstalled = false;
            }

            EditorGUILayout.Space(25f);
            GUILayout.Label("Project's Resouces Packages", EditorStyles.boldLabel);
            foreach (var resourcepackage in packageData.ResourcesPackages)
            {
                packageManagerUtilities.DrawPackageGUI(resourcepackage, packageData);
            }

            EditorGUILayout.Space(25f);
            if (packageManagerUtilities.addRequest == null) progressValue = 0;
            if (packageManagerUtilities.addRequest != null)
            {
                progressValue += Time.deltaTime;
                EditorGUI.ProgressBar(new Rect(3, position.height - 50, position.width - 6, 25), progressValue / 50, "Installation Progress");
            }
            FFHPackageManagerUtilities.DrawDefineSymbolsGUI(packageData, allPackagesInstalled);

            GUILayout.FlexibleSpace();
            packageData.ShowAtStart = EditorGUILayout.Toggle("Show On Startup", packageData.ShowAtStart);
        }

        /*
        private static void LoadAndInitializePackageData()
        {
            if (packageData == null)
            {
                packageData = FFHPackageManagerUtilities.LoadPackageData(PACKAGE_DATA_NAME);
                if (packageData != null)
                {
                    FFHPackageManagerUtilities.Initialize(packageData);
                    FFHPackageManagerUtilities.ListPackages();
                }
                else
                {
                    Debug.LogError($"Failed to load package data: {PACKAGE_DATA_NAME}");
                }
            }
        }
        */
    }
}