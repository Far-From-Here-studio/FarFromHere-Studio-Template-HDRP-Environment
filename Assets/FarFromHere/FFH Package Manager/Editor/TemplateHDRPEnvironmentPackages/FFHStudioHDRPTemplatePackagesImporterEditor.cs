using UnityEditor;
using UnityEngine;

namespace FFH.PackageManager
{
    [InitializeOnLoad]
    class FFHStudioHDRPTemplatePackagesImporterEditor : FFHPackageImporterWindow
    {
        private const string PACKAGE_DATA_NAME = "FFH_Template_HDRP_Demo_Resources";

        [MenuItem("FarFromHereStudio/Packages Installer/Template HDRP Environment")]
        protected static void Init()
        {
            var window = CreateInstance<FFHStudioHDRPTemplatePackagesImporterEditor>();
            window.InitializePackageData(PACKAGE_DATA_NAME);
            window.titleContent = new GUIContent("Template Packages Installer");
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

                //GUILayout.FlexibleSpace();
                //window.packageData.ShowAtStart = EditorGUILayout.Toggle("Show On Startup", window.packageData.ShowAtStart);
            }


        }
    }
}