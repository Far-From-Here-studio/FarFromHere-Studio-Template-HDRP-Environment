using System.Linq;
using UnityEditor;
using UnityEngine;
using FFH.PackageImporter.Editor;

[InitializeOnLoad]
public class FFHExtensionPackages : FFHPackageImporterWindow
{
    private const string PACKAGE_DATA_NAME = "FFHExtensionPackagesData";
    private const string SESSION_STATE_KEY = "FFHExtensionPackages_ShowAtStart";
    static FFHExtensionPackages FFHPackageImporterWindow;
    static FFHPackagesData data;
    private bool Listed;
    static FFHExtensionPackages()
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


    [MenuItem("FarFromHereStudio/Packages Installer/HDRP Extension Packages")]
    protected static void Init()
    {
        FFHPackageImporterWindow = GetWindow<FFHExtensionPackages>();
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
    private void OnValidate()
    {
        if (PackagesNames != null && packageData != null) CheckAllPackages();
        Repaint();
    }
    private void OnEnable()
    {
        if (listRequest == null) ListPackages();
        if (packageData == null) InitializePackageData(PACKAGE_DATA_NAME);
        if (PackagesNames != null && packageData != null) CheckAllPackages();
    }
    protected void OnGUI()
    {
        if (IsListing)
        {
            GUILayout.Label("Listing Packages...", EditorStyles.boldLabel);
        }
        else
        {
            if (PackagesNames != null && packageData != null)
            {
                allPackagesInstalled = true;
                GUILayout.Label(" Core Packages ", EditorStyles.boldLabel);

                foreach (var package in packageData.Packages)
                {
                    DrawPackageGUI(package);

                    if (!package.InstalledPackages) allPackagesInstalled = false;
                }


                if (packageData.ResourcesPackages.FirstOrDefault() != null)
                {
                    EditorGUILayout.Space(25f);
                    GUILayout.Label("Resouces Packages", EditorStyles.boldLabel);
                    foreach (var resourcepackage in packageData.ResourcesPackages)
                    {
                        DrawPackageGUI(resourcepackage);
                    }
                }
                EditorGUILayout.Space(25f);
                DrawDefineSymbolsGUI();

                GUILayout.FlexibleSpace();
                packageData.ShowAtStart = EditorGUILayout.Toggle("Show On Startup", packageData.ShowAtStart);
            }
        }
    }
}