
using UnityEngine;
using UnityEditor;

namespace FFH.Utilities.Rendering
{
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
}

