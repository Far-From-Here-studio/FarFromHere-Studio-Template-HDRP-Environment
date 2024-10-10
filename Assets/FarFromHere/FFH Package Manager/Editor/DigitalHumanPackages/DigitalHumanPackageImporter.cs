using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace FFH.PackageManager
{
    [InitializeOnLoad]
    class DigitalHumanPackageImporter : FFHPackageImporterWindow
    {
        private const string PACKAGE_DATA_NAME = "DigitalHumanPackageData";

        [MenuItem("FarFromHereStudio/Packages Installer/Digital Human")]
        protected static void Init()
        {
            var window = CreateInstance<DigitalHumanPackageImporter>();
            // Sets instance-specific packageData
            window.InitializePackageData(PACKAGE_DATA_NAME);
            window.titleContent = new GUIContent("Digital Human Packages Installer");
            window.Show();
        }
        public void OnEnable()
        {
            if (listRequest == null) ListPackages();
        }
        protected void OnGUI()
        {
            if (listRequest != null) GUILayout.Label("Listing Packages...", EditorStyles.boldLabel);

            if (PackagesNames != null) CheckAllPackages();
            if (PackagesNames != null && packageData != null && listRequest == null)
            {
                allPackagesInstalled = true;
                GUILayout.Label("Project's Core Packages", EditorStyles.boldLabel);

                foreach (var package in packageData.Packages)
                {
                    DrawPackageGUI(package);

                    if (!package.InstalledPackages) allPackagesInstalled = false;
                }
                EditorGUILayout.Space(25f);
                GUILayout.Label("Project's Resouces Packages", EditorStyles.boldLabel);
                foreach (var resourcepackage in packageData.ResourcesPackages)
                {
                    DrawPackageGUI(resourcepackage);
                }
                EditorGUILayout.Space(25f);

                DrawDefineSymbolsGUI();
            }
        }
    }
}