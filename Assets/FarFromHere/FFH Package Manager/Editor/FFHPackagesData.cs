using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class packageActive
{
    public bool InstalledPackages{ get; set; }
    public bool GUIState { get; set; }
    public string Name;
    public string Adress;
    public string folderGroupLabel;
}

[CreateAssetMenu(fileName = "FFH Packages", menuName = "FFH/PackageData")]
public class FFHPackagesData : ScriptableObject
{
    public bool ShowAtStart { get; set; }

    public string PackageListDefineSymbols;
    public bool AddedSymbols { get; set; }

    public packageActive[] Packages;
    public List<string> PackagesNames { get; set; }

}
