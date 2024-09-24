using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Build;
using System.IO;

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

    private float progressValue = 0;

    [MenuItem("FarFromHereStudio/Packages Installer/HDRP: Template Environment")]
    public static void Init()
    {
        ListPackages();
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

        GUILayout.Label("Packages", EditorStyles.boldLabel);

        allPackagesInstalled = true;

        foreach (var package in packageData.Packages)
        {
            DrawPackageGUI(package);
            if (!package.InstalledPackages) allPackagesInstalled = false;
        }

        EditorGUILayout.Space(25f);
        GUILayout.Label("Resouces", EditorStyles.boldLabel);
        foreach (var resourcepackage in packageData.ResourcesPackages)
        {
            DrawPackageGUI(resourcepackage);
        }


        EditorGUILayout.Space(25f);
        if (addRequest == null) progressValue = 0;
        if (addRequest != null)
        {
            progressValue += Time.deltaTime;
            EditorGUI.ProgressBar(new Rect(3, position.height - 50, position.width - 6, 25), progressValue / 50, "Installation Progress");
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

        // Check if the package is embedded
        string packagePath = Path.Combine(Application.dataPath, "..", "Packages", package.Name);
        bool isEmbedded = Directory.Exists(packagePath);

        if (isEmbedded)
        {
            if (EditorUtility.DisplayDialog("Remove Embedded Package",
                $"The package '{package.Name}' is embedded. Removing it will delete the package folder and update the manifest. This action cannot be undone. Are you sure you want to proceed?",
                "Yes", "No"))
            {
                RemoveEmbeddedPackage(package.Name);
            }
        }
        else
        {
            removeRequest = Client.Remove(package.Name);
            EditorApplication.update += RemoveProgress;

        }
    }

    static void RemoveEmbeddedPackage(string packageName)
    {
        removeRequest = Client.Remove(packageName);
        EditorApplication.update += RemoveProgress;

        string packagePath = Path.Combine(Application.dataPath, "..", "Packages", packageName);
        Debug.Log("packagePath" + packagePath);
        string manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
        Debug.Log("manifest" + manifestPath);

        try
        {
            // Delete the package folder
            if (Directory.Exists(packagePath))
            {
                Directory.Delete(packagePath, true);
            }
            Debug.Log($"Successfully removed embedded package: {packageName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error removing embedded package {packageName}: {e.Message}");
        }
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


            AssetDatabase.Refresh();
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
        foreach (var package in packageData.ResourcesPackages)
        {
            package.InstalledPackages = packageData.PackagesNames.Any(p => p.Contains(package.Name));
        }
    }

    static void AddSymbols()
    {
        if (string.IsNullOrEmpty(packageData.PackageListDefineSymbols)) return;

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
        if (string.IsNullOrEmpty(packageData.PackageListDefineSymbols)) return;

        var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var existingSymbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup));

        var symbols = new HashSet<string>(existingSymbols.Split(';'));
        symbols.Remove(packageData.PackageListDefineSymbols);

        var newSymbols = string.Join(";", symbols);
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup), newSymbols);
        Debug.Log($"Removed scripting define symbol: {packageData.PackageListDefineSymbols}");

        packageData.AddedSymbols = false;
    }


    [System.Serializable]
    private class ManifestJson
    {
        public Dictionary<string, string> dependencies = new Dictionary<string, string>();
    }
}