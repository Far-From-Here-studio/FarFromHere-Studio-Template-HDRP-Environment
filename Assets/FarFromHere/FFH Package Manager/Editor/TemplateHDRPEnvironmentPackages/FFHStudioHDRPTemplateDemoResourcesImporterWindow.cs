using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEngine.Assertions.Must;

[InitializeOnLoad]
public class FFHStudioHDRPTemplateDemoResourcesImporterEditor : EditorWindow
{
    static SearchRequest searchRequest;
    static AddRequest addRequest;
    static RemoveRequest removeRequest;
    static EmbedRequest embedRequest;
    static ListRequest listRequest;

    private static bool showOnStart;
    [SerializeField]
    static FFHPackagesData data;

    private packageActive package;
    private static bool allPackagesInstalled;

    [MenuItem("FarFromHereStudio/ Packages Installer/ HDRP: Template Environment")]
    public static void Init()
    {

        FFHStudioHDRPTemplateDemoResourcesImporterEditor window = (FFHStudioHDRPTemplateDemoResourcesImporterEditor)GetWindow(typeof(FFHStudioHDRPTemplateDemoResourcesImporterEditor));
        window.Show();
    }
    static FFHStudioHDRPTemplateDemoResourcesImporterEditor()
    {
        if (EditorApplication.isCompiling && EditorApplication.isUpdating) return;
        EditorApplication.update += Startup;
    }
    static void Startup()
    {
        if (EditorApplication.isCompiling && EditorApplication.isUpdating) return;

        if (!data)
        {
            string[] assetPaths = AssetDatabase.FindAssets("FFH_Template_HDRP_Demo_Resources");
            if (assetPaths.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetPaths[0]);
                data = AssetDatabase.LoadAssetAtPath<FFHPackagesData>(assetPath);
            }
            EditorUtility.SetDirty(data);
        }
        if (data)
        {
            if (showOnStart) Init();
            ListPackages();
            showOnStart = GetStartupValue(data);
        }
        EditorApplication.update -= Startup;
    }

    public static bool GetStartupValue(FFHPackagesData FFHData)
    {
        return FFHData.ShowAtStart;
    }

    void OnGUI()
    {
        GUILayout.Label("Package Settings", EditorStyles.boldLabel);

        allPackagesInstalled = true;

        if (!data)
        {
            string[] assetPaths = AssetDatabase.FindAssets("FFH_MeteoVFX_DemoResources");
            if (assetPaths.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetPaths[0]);
                data = AssetDatabase.LoadAssetAtPath<FFHPackagesData>(assetPath);
            }
        }
        if (!data) return;
        EditorUtility.SetDirty(data);
        // Display import/remove buttons for each package
        for (int i = 0; i < data.Packages.Length; i++)
        {
            DrawPackageGUI(ref data.Packages[i], ref data);
            if (data.Packages[i].InstalledPackages == false) allPackagesInstalled = false;
        }


        if (!string.IsNullOrEmpty(data.PackageListDefineSymbols))
        {
            if (!allPackagesInstalled && data.AddedSymbols)
            {
                if (GUILayout.Button("Remove Define Symbol"))
                {
                    RemoveSymbols();
                }
            }
            if (allPackagesInstalled && !data.AddedSymbols)
            {
                if (GUILayout.Button("Add Define Symbol"))
                {
                    AddSymbols();
                }
            }
        }

        // If all packages are installed and symbols haven't been added, add symbols
        GUILayout.FlexibleSpace();
        data.ShowAtStart = GUILayout.Toggle(data.ShowAtStart, "Show On Startup");
    }

    void DrawPackageGUI(ref packageActive package, ref FFHPackagesData data)
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        package.GUIState = EditorGUILayout.BeginFoldoutHeaderGroup(package.GUIState, package.folderGroupLabel);

        GUILayout.FlexibleSpace();
        GUILayout.Toggle(package.InstalledPackages, "Installed");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();

        if (package.GUIState)
        {
            if (GUILayout.Button("Import Package"))
            {
                Debug.Log(package.Name + " Installation..");
                AddPackage(package);
            }

            if (GUILayout.Button("Remove Package"))
            {
                RemoveSymbols();
                Debug.Log(package.Name + " Uninstallation...");

                RemovePackage(package);
            }

            if (GUILayout.Button("Embed Package"))
            {
                Debug.Log(package.Name + " Embedding...");
                EmbedPackage(package.Name);
            }
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndFoldoutHeaderGroup();



    }


    static void ListPackages()
    {
        data.PackagesNames = new List<string>();
        listRequest = Client.List(false, true);
        EditorApplication.update += ListProgress;
    }
    static void ListProgress()
    {
        if (listRequest.IsCompleted)
        {
            if (listRequest.Status == StatusCode.Success)
            {
                foreach (var package in listRequest.Result)
                {
                    data.PackagesNames.Add(package.name);
                }
                CheckAllPackages();
            }
            else if (listRequest.Status >= StatusCode.Failure)
            {
                Debug.Log(listRequest.Error.message);
            }
            EditorApplication.update -= ListProgress;
        }
    }

    static void AddPackage(packageActive package)
    {

        addRequest = Client.Add(package.Adress);
        EditorApplication.update += AddProgress;
        if (addRequest.IsCompleted)
        {
            ListPackages();
        }
    }

    static void AddProgress()
    {
        if (addRequest.IsCompleted)
        {
            if (addRequest.Status == StatusCode.Success)
            {
                Debug.Log("Installed: " + addRequest.Result.packageId);
            }
            else if (addRequest.Status >= StatusCode.Failure)
            {
                Debug.Log(addRequest.Error.message);
            }
            EditorApplication.update -= AddProgress;
        }
    }

    static void EmbedPackage(string packageNameOrID)
    {
        embedRequest = Client.Embed(packageNameOrID);
        EditorApplication.update += EmbedProgress;
    }

    static void EmbedProgress()
    {
        if (embedRequest.IsCompleted)
        {
            if (embedRequest.Status == StatusCode.Success)
            {
                Debug.Log("Embedding Package: " + embedRequest.Result.packageId);
            }
            else if (embedRequest.Status >= StatusCode.Failure)
            {
                Debug.Log(embedRequest.Error.message);
            }
            EditorApplication.update -= EmbedProgress;
        }
    }

    static void RemovePackage(packageActive package)
    {
        RemoveSymbols();
        removeRequest = Client.Remove(package.Name);
        EditorApplication.update += RemoveProgress;
        if (removeRequest.IsCompleted)
        {
            ListPackages();
        }
    }

    static void RemoveProgress()
    {
        if (removeRequest.IsCompleted)
        {
            if (removeRequest.Status == StatusCode.Success)
            {
                Debug.Log("Uninstalled: " + removeRequest.PackageIdOrName);
            }
            else if (removeRequest.Status >= StatusCode.Failure)
            {
                Debug.Log(removeRequest.Error.message);

            }
            EditorApplication.update -= RemoveProgress;
        }
    }

    static void AddSymbols()
    {
        BuildTargetGroup buildTargetGroupSelected = EditorUserBuildSettings.selectedBuildTargetGroup;
        string existingSymbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroupSelected));

        string newSymbols = existingSymbols + ";" + data.PackageListDefineSymbols;
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroupSelected), newSymbols);
        Debug.Log("Added scripting define symbol: " + data.PackageListDefineSymbols);

        data.AddedSymbols = true;
        EditorApplication.update -= AddSymbols;
    }
    static void RemoveSymbols()
    {
        BuildTargetGroup buildTargetGroupSelected = EditorUserBuildSettings.selectedBuildTargetGroup;
        string existingSymbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroupSelected));

        string[] symbols = existingSymbols.Split(';').Where(s => s != data.PackageListDefineSymbols).ToArray();
        string newSymbols = string.Join(";", symbols);
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroupSelected), newSymbols);
        Debug.Log("Removed scripting define symbol: " + data.PackageListDefineSymbols);

        data.AddedSymbols = false;
        EditorApplication.update -= RemoveSymbols;
    }


    static void CheckAllPackages()
    {
        // Loop through each package in data
        for (var p = 0; p < data.Packages.Length; p++)
        {
            data.Packages[p].InstalledPackages = false;
            // Check if the package exists in the installed packages list (PackagesNames)
            foreach (var packagename in data.PackagesNames)
            {
                if (packagename.Contains(data.Packages[p].Name))
                {
                    data.Packages[p].InstalledPackages = true;
                }
            }

        }
        EditorApplication.update -= CheckAllPackages;
    }
}
