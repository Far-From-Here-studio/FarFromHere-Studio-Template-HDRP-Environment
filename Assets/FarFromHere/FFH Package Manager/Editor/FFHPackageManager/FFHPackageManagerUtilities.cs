
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
    public class FFHPackageManagerUtilities
    {

        public static AddRequest addRequest;
        public static EmbedRequest embedRequest;
        public static ListRequest listRequest;
        public static RemoveRequest removeRequest;

        private static FFHPackagesData _packageData;

        public static void Initialize(FFHPackagesData packageData)
        {
            _packageData = packageData;
        }

        public static FFHPackagesData LoadPackageData(string packageDataName)
        {
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

        public static void DrawPackageGUI(PackageActive package, FFHPackagesData packageData)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            package.GUIState = EditorGUILayout.BeginFoldoutHeaderGroup(package.GUIState, package.FolderGroupLabel);
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Toggle("Installed", package.InstalledPackages);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            if (package.GUIState && listRequest == null)
            {
                EditorGUILayout.BeginHorizontal();

                if (package.InstalledPackages == false && addRequest == null)
                {
                    if (GUILayout.Button("Import Package"))
                    {
                        AddPackage(package);
                    }
                }

                if (package.InstalledPackages == true)
                {
                    if (GUILayout.Button("Remove Package"))
                    {
                        RemovePackage(packageData, package);
                    }
                }

                if (package.EmbededPackages == false && package.InstalledPackages == true)
                {
                    if (GUILayout.Button("Embed Package"))
                    {
                        Debug.Log(package.EmbededPackages);
                        EmbedPackage(package);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        public static void CheckAllPackages(FFHPackagesData packageData)
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

        public static void ListPackages()
        {
            if (_packageData == null)
            {
                Debug.LogError("Package data is null. Make sure to call Initialize before using FFHPackageManagerUtilities.");
                return;
            }

            listRequest = Client.List(false, true);
            EditorApplication.update += ListProgress;
        }
        static void ListProgress()
        {
            if (listRequest.IsCompleted)
            {
                if (listRequest.Status == StatusCode.Success)
                {
                    _packageData.PackagesNames = listRequest.Result.Select(p => p.name).ToList();
                    CheckAllPackages(_packageData);
                }
                else if (listRequest.Status >= StatusCode.Failure)
                {
                    Debug.LogError($"Error listing packages: {listRequest.Error.message}");
                }
                listRequest = null;
                EditorApplication.update -= ListProgress;
            }
        }

        static void AddPackage(PackageActive package)
        {
            if (addRequest != null)
            {
                return;
            }
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

                    // Check if the added package is a ResourcesPackage
                    if (_packageData.ResourcesPackages.Any(p => p.Name == addRequest.Result.name))
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
                EditorApplication.update -= AddProgress;
            }
        }

        static void RemovePackage(FFHPackagesData packageData, PackageActive package)
        {
            Debug.Log($"{package.Name} Uninstallation...");

            RemoveSymbols(packageData);

            bool isEmbedded = package.EmbededPackages;

            if (isEmbedded)
            {
                if (EditorUtility.DisplayDialog("Remove Embedded Package",
                    $"The package '{package.Name}' is embedded. Removing it will delete the package folder and update the manifest. This action cannot be undone. Are you sure you want to proceed?",
                    "Yes",
                    "No"))
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
                EditorApplication.update += RemoveProgress;
            }
        }

        static void RemoveEmbeddedPackage(PackageActive package)
        {
            removeRequest = Client.Remove(package.Name);
            EditorApplication.update += RemoveProgress;

            // Check if the package is embedded
            string projectpath = Path.GetDirectoryName(Application.dataPath);
            Debug.Log("project path " + projectpath);
            string packagePath = Path.Combine(projectpath, "Packages", package.Name);
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
                AssetDatabase.Refresh();
                package.EmbededPackages = false;
                Debug.Log($"Successfully removed embedded package: {package.Name}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error removing embedded package {package.Name}: {e.Message}");
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
                EditorApplication.update -= RemoveProgress;
            }
        }

        static void EmbedPackage(PackageActive package)
        {
            Debug.Log($"{package.Name} Embedding...");
            embedRequest = Client.Embed(package.Name);
            package.EmbededPackages = true;
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
        public static void DrawDefineSymbolsGUI(FFHPackagesData packageData, bool allPackagesInstalled)
        {
            if (string.IsNullOrEmpty(packageData.PackageListDefineSymbols)) return;

            if (!allPackagesInstalled && packageData.AddedSymbols)
            {
                if (GUILayout.Button("Remove Define Symbol"))
                {
                    RemoveSymbols(packageData);
                }
            }
            else if (allPackagesInstalled && !packageData.AddedSymbols)
            {
                if (GUILayout.Button("Add Define Symbol"))
                {
                    AddSymbols(packageData);
                }
            }
        }
        static void AddSymbols(FFHPackagesData packageData)
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

        static void RemoveSymbols(FFHPackagesData packageData)
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