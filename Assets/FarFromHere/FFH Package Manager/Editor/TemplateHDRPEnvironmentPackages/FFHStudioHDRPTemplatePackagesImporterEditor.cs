using UnityEditor;
using UnityEngine;

namespace FFH.PackageManager
{
    [InitializeOnLoad]
    public class FFHStudioHDRPTemplatePackagesImporterEditor : FFHPackageImporterWindow
    {
        private const string PACKAGE_DATA_NAME = "FFH_Template_HDRP_Demo_Resources";
        private float progressValue = 0;

        static FFHStudioHDRPTemplatePackagesImporterEditor()
        {
            EditorApplication.delayCall += Startup;
        }


        [MenuItem("FarFromHereStudio/Packages Installer/HDRP: Template Environment")]
        public static void Init()
        {
            LoadAndInitializePackageData();
            var window = GetWindow<FFHStudioHDRPTemplatePackagesImporterEditor>();
            window.titleContent = new GUIContent("FFH Package Installer");
            window.Show();
        }

        static void Startup()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

            LoadAndInitializePackageData();
            if (packageData != null && packageData.ShowAtStart)
            {
                Init();
            }
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
                FFHPackageManagerUtilities.DrawPackageGUI(package, packageData);
                if (!package.InstalledPackages) allPackagesInstalled = false;
            }

            EditorGUILayout.Space(25f);
            GUILayout.Label("Project's Resouces Packages", EditorStyles.boldLabel);
            foreach (var resourcepackage in packageData.ResourcesPackages)
            {
                FFHPackageManagerUtilities.DrawPackageGUI(resourcepackage, packageData);
            }

            EditorGUILayout.Space(25f);
            if (FFHPackageManagerUtilities.addRequest == null) progressValue = 0;
            if (FFHPackageManagerUtilities.addRequest != null)
            {
                progressValue += Time.deltaTime;
                EditorGUI.ProgressBar(new Rect(3, position.height - 50, position.width - 6, 25), progressValue / 50, "Installation Progress");
            }
            FFHPackageManagerUtilities.DrawDefineSymbolsGUI(packageData, allPackagesInstalled);

            GUILayout.FlexibleSpace();
            packageData.ShowAtStart = EditorGUILayout.Toggle("Show On Startup", packageData.ShowAtStart);
        }

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
    }
}