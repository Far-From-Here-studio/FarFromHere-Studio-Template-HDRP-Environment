using System.Collections.Generic;
using UnityEngine;

namespace FFH.PackageManager
{
    [System.Serializable]
    public class PackageActive
    {
        public bool InstalledPackages { get; set; }
        public string Name;
        public string Address;
        public string FolderGroupLabel;
        [HideInInspector]
        public bool GUIState;
        [HideInInspector]
        public bool EmbededPackages;
    }

    [CreateAssetMenu(fileName = "FFH Packages", menuName = "FFH/PackageData")]
    public class FFHPackagesData : ScriptableObject
    {
        public bool ShowAtStart;

        public string PackageListDefineSymbols;
        public bool AddedSymbols { get; set; }

        public PackageActive[] Packages;

        public PackageActive[] ResourcesPackages;

    }
}