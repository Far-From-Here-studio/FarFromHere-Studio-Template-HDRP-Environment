using UnityEditor;
using UnityEngine;

public class StampArea : MonoBehaviour
{
#if UNITY_EDITOR
    [ColorUsage(true, true)]
    public Color OutlineColor = Color.grey;
    public float OutlineEdgeSize = 10;
    public bool Outline = true;
    public Vector3 Size = new Vector3(10, 10, 10);
    public Vector3 Offset;
    private void OnDrawGizmos()
    {
        if (!Outline) return;
        Handles.matrix = transform.localToWorldMatrix;
        Handles.color = OutlineColor;

        Vector3 halfSize = Size * 0.5f;
        Vector3 center = Offset;

        Vector3[] corners = new Vector3[]
        {
            center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z),
            center + new Vector3(halfSize.x, -halfSize.y, -halfSize.z),
            center + new Vector3(halfSize.x, -halfSize.y, halfSize.z),
            center + new Vector3(-halfSize.x, -halfSize.y, halfSize.z),
            center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z),
            center + new Vector3(halfSize.x, halfSize.y, -halfSize.z),
            center + new Vector3(halfSize.x, halfSize.y, halfSize.z),
            center + new Vector3(-halfSize.x, halfSize.y, halfSize.z)
        };

        Handles.DrawLines(new Vector3[]
        {
            corners[0], corners[1],
            corners[1], corners[2],
            corners[2], corners[3],
            corners[3], corners[0],
            corners[4], corners[5],
            corners[5], corners[6],
            corners[6], corners[7],
            corners[7], corners[4],
            corners[0], corners[4],
            corners[1], corners[5],
            corners[2], corners[6],
            corners[3], corners[7]
        });
    }
#endif
}
