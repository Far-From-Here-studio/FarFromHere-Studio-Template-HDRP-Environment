using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEngine;

namespace FFH.PackageManager
{
    public abstract class FFHPackageImporterWindow : EditorWindow
    {
        protected bool allPackagesInstalled;
        protected float progressValue = 0;
        protected FFHPackagesData packageData;
        protected bool IsListing;

        public static ListRequest listRequest;

        protected static AddRequest addRequest;
        protected static EmbedRequest embedRequest;
        protected static RemoveRequest removeRequest;
        public static List<string> PackagesNames { get; set; }

        protected static FFHPackagesData LoadPackageData(string packageDataName)
        {
            // This can remain static as it's just a utility method
            string[] assetPaths = AssetDatabase.FindAssets(packageDataName);
            if (assetPaths.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetPaths[0]);
                FFHPackagesData loadedPackageData = AssetDatabase.LoadAssetAtPath<FFHPackagesData>(assetPath);
                if (loadedPackageData != null)
                {
                    EditorUtility.SetDirty(loadedPackageData);
                    return loadedPackageData;
                }
            }
            Debug.LogError($"Failed to load package data: {packageDataName}");
            return null;
        }
        // NEW - More encapsulated approach
        protected void InitializePackageData(string packageDataName)
        {
            packageData = LoadPackageData(packageDataName);
        }

        
        protected void DrawPackageGUI(PackageActive package)
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
                if (!package.InstalledPackages)
                {
                    if (GUILayout.Button("Import Package"))
                    {
                        AddPackage(package);
                    }
                }

                if (package.InstalledPackages)
                {
                    if (GUILayout.Button("Remove Package"))
                    {
                        RemovePackage(package);
                    }
                }

                if (!package.EmbededPackages && package.InstalledPackages)
                {
                    if (GUILayout.Button("Embed Package"))
                    {
                        EmbedPackage(package);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        public void ListPackages()
        {
            PackagesNames = new List<string>();
            if (PackagesNames == null)
            {
                Debug.LogError("PackageName List is null.");
            }
            listRequest = Client.List(false, true);
            EditorApplication.update += ListProgress;
        }

        public static void ListProgress()
        {
            if (listRequest != null && listRequest.IsCompleted)
            {
                if (listRequest.Status == StatusCode.Success)
                {
                    PackagesNames = listRequest.Result.Select(p => p.name).ToList();

                }
                else if (listRequest.Status >= StatusCode.Failure)
                {
                    PackagesNames = null;
                    Debug.LogError($"Error listing packages: {listRequest.Error.message}");
                }
                listRequest = null;
                EditorApplication.update -= ListProgress;
            }
        }

        protected void AddPackage(PackageActive package)
        {
            if (addRequest != null) return;
            Debug.Log($"{package.Name} Installation...");
            addRequest = Client.Add(package.Address);
            EditorApplication.update += AddProgressHandler;
        }

        protected void AddProgressHandler()
        {
            if (addRequest != null && addRequest.IsCompleted)
            {
                if (addRequest.Status == StatusCode.Success)
                {
                    Debug.Log($"Installed: {addRequest.Result.packageId}");
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    if (packageData.ResourcesPackages.Any(p => p.Name == addRequest.Result.name))
                    {
                        Debug.Log("ResourcesPackage added. Reloading open scenes...");
                        EditorApplication.delayCall += SceneReloader.ReloadOpenScenes;
                    }
                }
                else if (addRequest.Status >= StatusCode.Failure)
                {
                    Debug.LogError($"Error adding package: {addRequest.Error.message}");
                }
                addRequest = null;
                EditorApplication.update -= AddProgressHandler;
            }
        }

        protected void RemovePackage(PackageActive package)
        {
            Debug.Log($"{package.Name} Uninstallation...");
            RemoveSymbols();

            if (package.EmbededPackages)
            {
                var displayDialog = EditorUtility.DisplayDialog("Remove Embedded Package",
                    $"The package '{package.Name}' is embedded. Removing it will delete the package folder and update the manifest. This action cannot be undone. Are you sure you want to proceed?",
                    "Yes",
                    "No");

                if (displayDialog)
                {
                    RemoveEmbeddedPackage(package);
                }
                else
                {
                    return;
                }
            }
            else
            {
                removeRequest = Client.Remove(package.Name);
                EditorApplication.update += RemoveProgressHandler;
            }
        }

        protected void RemoveProgressHandler()
        {
            if (removeRequest != null && removeRequest.IsCompleted)
            {
                if (removeRequest.Status == StatusCode.Success)
                {
                    Debug.Log($"Uninstalled: {removeRequest.PackageIdOrName}");
                    EditorApplication.delayCall += SceneReloader.ReloadOpenScenes;
                }
                else if (removeRequest.Status >= StatusCode.Failure)
                {
                    Debug.LogError($"Error removing package: {removeRequest.Error.message}");
                }
                removeRequest = null;
                EditorApplication.update -= RemoveProgressHandler;
            }
        }


        protected void EmbedPackage(PackageActive package)
        {
            Debug.Log($"{package.Name} Embedding...");
            embedRequest = Client.Embed(package.Name);
            package.EmbededPackages = true;
            EditorApplication.update += EmbedProgressHandler;
        }


        protected void EmbedProgressHandler()
        {
            if (embedRequest != null && embedRequest.IsCompleted)
            {
                if (embedRequest.Status == StatusCode.Success)
                {
                    Debug.Log($"Embedded: {embedRequest.Result.packageId}");
                    EditorApplication.delayCall += SceneReloader.ReloadOpenScenes;
                }
                else if (embedRequest.Status >= StatusCode.Failure)
                {
                    Debug.LogError($"Error embedding package: {embedRequest.Error.message}");
                }
                embedRequest = null;
                EditorApplication.update -= EmbedProgressHandler;
            }
        }

        protected void RemoveEmbeddedPackage(PackageActive package)
        {
            removeRequest = Client.Remove(package.Name);
            EditorApplication.update += RemoveProgressHandler;
            string projectPath = Path.GetDirectoryName(Application.dataPath);
            string packagePath = Path.Combine(projectPath, "Packages", package.Name);
            try
            {
                if (Directory.Exists(packagePath))
                {
                    Directory.Delete(packagePath, true);
                }
                AssetDatabase.Refresh();
                package.EmbededPackages = false;
                Debug.Log($"Successfully removed embedded package: {package.Name}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error removing embedded package {package.Name}: {e.Message}");
            }
        }

        protected void CheckAllPackages()
        {
            foreach (var package in packageData.Packages)
            {
                package.InstalledPackages = PackagesNames.Any(p => p.Contains(package.Name));
            }
            foreach (var package in packageData.ResourcesPackages)
            {
                package.InstalledPackages = PackagesNames.Any(p => p.Contains(package.Name));
            }
        }

        protected void DrawDefineSymbolsGUI()
        {
            if (string.IsNullOrEmpty(packageData.PackageListDefineSymbols)) return;
            if (!allPackagesInstalled && packageData.AddedSymbols)
            {
                if (GUILayout.Button("Remove Define Symbol"))
                {
                    RemoveSymbols();
                }
            }
            else if (allPackagesInstalled && packageData.AddedSymbols)
            {
                if (GUILayout.Button("Add Define Symbol"))
                {
                    AddSymbols();
                }
            }
        }
        protected virtual void OnDestroy()
        {
            EditorApplication.update -= ListProgress;
            EditorApplication.update -= AddProgressHandler;
            EditorApplication.update -= RemoveProgressHandler;
            EditorApplication.update -= EmbedProgressHandler;
        }

        protected void AddSymbols()
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

        protected void RemoveSymbols()
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
    }
}