using UnityEditor;
using UnityEngine;

namespace FFH.PackageManager
{
    [InitializeOnLoad]
    public class DigitalHumanPackageImporter : FFHPackageImporterWindow
    {
        private const string PACKAGE_DATA_NAME = "DigitalHumanPackageData";
        private const string SESSION_STATE_KEY = "DigitalHuman_ShowAtStart";
        static DigitalHumanPackageImporter FFHPackageImporterWindow;
        static FFHPackagesData data;
        private bool Listed;
        static DigitalHumanPackageImporter()
        {
            EditorApplication.delayCall += DelayedStart;
        }
        static void DelayedStart()
        {
            var isAlreadyShown = SessionState.GetBool(SESSION_STATE_KEY, false);

            if (!data)
            {
                data = LoadPackageData(PACKAGE_DATA_NAME);
            }

            if (data && !isAlreadyShown && data.ShowAtStart)
            {
                Init();
                SessionState.SetBool(SESSION_STATE_KEY, true);
                EditorApplication.update -= DelayedStart;
            }
        }


        [MenuItem("FarFromHereStudio/Packages Installer/Digital Human")]
        protected static void Init()
        {
            FFHPackageImporterWindow = GetWindow<DigitalHumanPackageImporter>();
            FFHPackageImporterWindow.Show();
        }

        protected void OnInspectorUpdate()
        {

            if (listRequest != null)
            {
                IsListing = true;
                Listed = IsListing;
            }
            else
            {
                IsListing = false;
                if (Listed != IsListing)
                {
                    if (PackagesNames != null && packageData != null) CheckAllPackages();
                    Repaint();
                    Listed = IsListing;
                }
            }
        }
        private void OnEnable()
        {
            if (listRequest == null) ListPackages();
            if (packageData == null) InitializePackageData(PACKAGE_DATA_NAME);
        }
        protected void OnGUI()
        {
            if (IsListing)
            {
                GUILayout.Label("Listing Packages...", EditorStyles.boldLabel);
            }
            else
            {
                if (PackagesNames != null && packageData != null) CheckAllPackages();
                if (PackagesNames != null && packageData != null)
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

                    GUILayout.FlexibleSpace();
                    packageData.ShowAtStart = EditorGUILayout.Toggle("Show On Startup", packageData.ShowAtStart);
                }
            }
        }
    }
}