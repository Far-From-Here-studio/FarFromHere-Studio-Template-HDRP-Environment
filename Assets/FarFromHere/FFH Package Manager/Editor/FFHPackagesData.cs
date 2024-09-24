using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PackageActive
{
    public bool InstalledPackages { get; set; }
    public bool GUIState { get; set; }
    public string Name;
    public string Address;
    public string FolderGroupLabel;
}

[CreateAssetMenu(fileName = "FFH Packages", menuName = "FFH/PackageData")]
public class FFHPackagesData : ScriptableObject
{
    [SerializeField] private bool showAtStart;
    public bool ShowAtStart
    {
        get => showAtStart;
        set
        {
            if (showAtStart != value)
            {
                showAtStart = value;
                OnShowAtStartChanged?.Invoke(showAtStart);
            }
        }
    }

    public event System.Action<bool> OnShowAtStartChanged;

    public string PackageListDefineSymbols;
    public bool AddedSymbols { get; set; }

    public PackageActive[] Packages;

    public PackageActive[] ResourcesPackages;
    public List<string> PackagesNames { get; set; } = new List<string>();
}