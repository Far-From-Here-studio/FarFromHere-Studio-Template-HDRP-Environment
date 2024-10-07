using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LODGroupRecalculate))]
public class LODGroupRecalculateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LODGroupRecalculate lodGroupRecalculate = (LODGroupRecalculate)target;

        if (GUILayout.Button("Recalculate LODGroup Bounds"))
        {
            lodGroupRecalculate.OnButtonClick();
        }

        DrawDefaultInspector();
    }
}