using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Build;

[InitializeOnLoad]
public class FFHStudioHDRPTemplateDemoResourcesImporterEditor : EditorWindow
{
    private const string SHOW_ON_START_PREF = "FFHStudio_ShowOnStart";
    private const string PACKAGE_DATA_NAME = "FFH_Template_HDRP_Demo_Resources";

    private static FFHPackagesData packageData;
    private static bool allPackagesInstalled;

    private static ListRequest listRequest;
    private static AddRequest addRequest;
    private static RemoveRequest removeRequest;
    private static EmbedRequest embedRequest;

    [MenuItem("FarFromHereStudio/Packages Installer/HDRP: Template Environment")]
    public static void Init()
    {
        var window = GetWindow<FFHStudioHDRPTemplateDemoResourcesImporterEditor>();
        window.titleContent = new GUIContent("FFH Package Installer");
        window.Show();
    }

    static FFHStudioHDRPTemplateDemoResourcesImporterEditor()
    {
        EditorApplication.delayCall += Startup;
    }

    static void Startup()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

        LoadPackageData();
        if (packageData != null)
        {
            if (EditorPrefs.GetBool(SHOW_ON_START_PREF, false)) Init();
            ListPackages();
        }
    }

    static void LoadPackageData()
    {
        string[] assetPaths = AssetDatabase.FindAssets(PACKAGE_DATA_NAME);
        if (assetPaths.Length > 0)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetPaths[0]);
            packageData = AssetDatabase.LoadAssetAtPath<FFHPackagesData>(assetPath);
            if (packageData != null)
            {
                EditorUtility.SetDirty(packageData);
                packageData.ShowAtStart = EditorPrefs.GetBool(SHOW_ON_START_PREF, false);
                packageData.OnShowAtStartChanged += HandleShowAtStartChanged;
            }
        }
    }

    static void HandleShowAtStartChanged(bool newValue)
    {
        EditorPrefs.SetBool(SHOW_ON_START_PREF, newValue);
    }

    void OnGUI()
    {
        if (packageData == null) return;

        GUILayout.Label("Package Settings", EditorStyles.boldLabel);

        allPackagesInstalled = true;

        foreach (var package in packageData.Packages)
        {
            DrawPackageGUI(package);
            if (!package.InstalledPackages) allPackagesInstalled = false;
        }

        DrawDefineSymbolsGUI();

        GUILayout.FlexibleSpace();
        packageData.ShowAtStart = EditorGUILayout.Toggle("Show On Startup", packageData.ShowAtStart);
    }

    void DrawPackageGUI(PackageActive package)
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        package.GUIState = EditorGUILayout.BeginFoldoutHeaderGroup(package.GUIState, package.FolderGroupLabel);

        GUILayout.FlexibleSpace();
        EditorGUI.BeginDisabledGroup(true);
        //EditorGUILayout.TextField(package.FolderGroupLabel);
        EditorGUILayout.Toggle("Installed", package.InstalledPackages);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        if (package.GUIState)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Import Package"))
            {
                AddPackage(package);
            }

            if (GUILayout.Button("Remove Package"))
            {
                RemovePackage(package);
            }

            if (GUILayout.Button("Embed Package"))
            {
                EmbedPackage(package.Name);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    void DrawDefineSymbolsGUI()
    {
        if (string.IsNullOrEmpty(packageData.PackageListDefineSymbols)) return;

        if (!allPackagesInstalled && packageData.AddedSymbols)
        {
            if (GUILayout.Button("Remove Define Symbol"))
            {
                RemoveSymbols();
            }
        }
        else if (allPackagesInstalled && !packageData.AddedSymbols)
        {
            if (GUILayout.Button("Add Define Symbol"))
            {
                AddSymbols();
            }
        }
    }

    static void ListPackages()
    {
        listRequest = Client.List(false, true);
        EditorApplication.update += ListProgress;
    }

    static void ListProgress()
    {
        if (listRequest.IsCompleted)
        {
            if (listRequest.Status == StatusCode.Success)
            {
                packageData.PackagesNames = listRequest.Result.Select(p => p.name).ToList();
                CheckAllPackages();
            }
            else if (listRequest.Status >= StatusCode.Failure)
            {
                Debug.LogError($"Error listing packages: {listRequest.Error.message}");
            }

            EditorApplication.update -= ListProgress;
        }
    }

    static void AddPackage(PackageActive package)
    {
        Debug.Log($"{package.Name} Installation...");
        addRequest = Client.Add(package.Address);
        EditorApplication.update += AddProgress;
    }

    static void AddProgress()
    {
        if (addRequest.IsCompleted)
        {
            if (addRequest.Status == StatusCode.Success)
            {
                Debug.Log($"Installed: {addRequest.Result.packageId}");
                ListPackages();
            }
            else if (addRequest.Status >= StatusCode.Failure)
            {
                Debug.LogError($"Error adding package: {addRequest.Error.message}");
            }

            EditorApplication.update -= AddProgress;
        }
    }

    static void RemovePackage(PackageActive package)
    {
        Debug.Log($"{package.Name} Uninstallation...");
        RemoveSymbols();
        removeRequest = Client.Remove(package.Name);
        EditorApplication.update += RemoveProgress;
    }

    static void RemoveProgress()
    {
        if (removeRequest.IsCompleted)
        {
            if (removeRequest.Status == StatusCode.Success)
            {
                Debug.Log($"Uninstalled: {removeRequest.PackageIdOrName}");
                ListPackages();
            }
            else if (removeRequest.Status >= StatusCode.Failure)
            {
                Debug.LogError($"Error removing package: {removeRequest.Error.message}");
            }

            EditorApplication.update -= RemoveProgress;
        }
    }

    static void EmbedPackage(string packageName)
    {
        Debug.Log($"{packageName} Embedding...");
        embedRequest = Client.Embed(packageName);
        EditorApplication.update += EmbedProgress;
    }

    static void EmbedProgress()
    {
        if (embedRequest.IsCompleted)
        {
            if (embedRequest.Status == StatusCode.Success)
            {
                Debug.Log($"Embedded: {embedRequest.Result.packageId}");
                ListPackages();
            }
            else if (embedRequest.Status >= StatusCode.Failure)
            {
                Debug.LogError($"Error embedding package: {embedRequest.Error.message}");
            }

            EditorApplication.update -= EmbedProgress;
        }
    }

    static void CheckAllPackages()
    {
        foreach (var package in packageData.Packages)
        {
            package.InstalledPackages = packageData.PackagesNames.Any(p => p.Contains(package.Name));
        }
    }

    static void AddSymbols()
    {
        var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var existingSymbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup));

        var symbols = new HashSet<string>(existingSymbols.Split(';'));
        symbols.Add(packageData.PackageListDefineSymbols);

        var newSymbols = string.Join(";", symbols);
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup), newSymbols);
        Debug.Log($"Added scripting define symbol: {packageData.PackageListDefineSymbols}");

        packageData.AddedSymbols = true;
    }

    static void RemoveSymbols()
    {
        var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var existingSymbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup));

        var symbols = new HashSet<string>(existingSymbols.Split(';'));
        symbols.Remove(packageData.PackageListDefineSymbols);

        var newSymbols = string.Join(";", symbols);
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup), newSymbols);
        Debug.Log($"Removed scripting define symbol: {packageData.PackageListDefineSymbols}");

        packageData.AddedSymbols = false;
    }
}