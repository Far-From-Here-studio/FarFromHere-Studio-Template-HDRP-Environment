using UnityEditor;
using UnityEngine;

namespace FFH.PackageManager
{
    public class FFHPackageImporterWindow : EditorWindow
    {
        protected FFHPackagesData packageData;
        protected bool allPackagesInstalled;
        protected float progressValue = 0;
        protected FFHPackageManagerUtilities packageManagerUtilities;

        protected virtual void OnEnable()
        {
            packageManagerUtilities = new FFHPackageManagerUtilities();
        }

        protected void InitializePackageData(string packageDataName)
        {
            if (packageData == null)
            {
                packageData = FFHPackageManagerUtilities.LoadPackageData(packageDataName);
                if (packageData != null)
                {
                    packageManagerUtilities.Initialize(packageData);
                    packageManagerUtilities.ListPackages();
                }
                else
                {
                    Debug.LogError($"Failed to load package data: {packageDataName}");
                }
            }
        }
    }
}