using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[InitializeOnLoad]
#endif

[RequireComponent(typeof(Terrain))]
[ExecuteAlways]
public class ExposeTerrainHeight : MonoBehaviour
{
    private Terrain terrain;
    public Texture HeightMap;
    public Mesh Quad;
    public GameObject InstanceQuad;
    public Shader TerrainShader;

    private Vector3 size;
    private Vector3 position;
    public Material newmat;
    public int layer;
    MaterialPropertyBlock props;
    private void Awake()
    {
        terrain = GetComponent<Terrain>();
        terrain.drawTreesAndFoliage = false;

        HeightMap = terrain.terrainData.heightmapTexture;
        if (!HeightMap) return;
        size = terrain.terrainData.size;
        position = terrain.GetPosition();
        layer = LayerMask.NameToLayer("Default");
        //Tools.visibleLayers &= ~(1 << layer);

        if (!InstanceQuad && TerrainShader)
        {
            var elements = GetComponentsInChildren<TerrainElement>();
            foreach (var m in elements)
            {
                DestroyImmediate(m.gameObject);
            }
            var go = new GameObject();
            go.hideFlags = HideFlags.HideAndDontSave;

            go.AddComponent<TerrainElement>();
            go.AddComponent<MeshFilter>().mesh = Quad;

            var renderer = go.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            newmat = new Material(TerrainShader);
            renderer.material = newmat;
            go.layer = layer;
            go.transform.parent = transform;
            go.transform.position = position + new Vector3(size.x / 2, 0, size.x / 2);
            go.transform.rotation = Quaternion.Euler(90, 0, 0);
            go.transform.localScale = new Vector3(size.x, size.x, 0);

            InstanceQuad = go;
        }
        //terrain.materialTemplate.SetVector("_TerrainPosition", new Vector4(terrain.gameObject.transform.position.x, terrain.gameObject.transform.position.z, 0, 0));
        props = new MaterialPropertyBlock();
        terrain.GetSplatMaterialPropertyBlock(props);
        props.SetVector("_TerrainPosition", new Vector4(terrain.gameObject.transform.position.x, terrain.gameObject.transform.position.z, 0, 0));
        terrain.SetSplatMaterialPropertyBlock(props);

    }
    private void OnEnable()
    {
        if(!terrain.drawTreesAndFoliage) terrain.drawTreesAndFoliage = true;
    }
    void LateUpdate()
    {
        if (!InstanceQuad && TerrainShader)
        {
            var elements = GetComponentsInChildren<TerrainElement>();
            foreach (var m in elements)
            {
                DestroyImmediate(m.gameObject);
            }
            var go = new GameObject();
            go.hideFlags = HideFlags.HideAndDontSave;

            go.AddComponent<TerrainElement>();
            go.AddComponent<MeshFilter>().mesh = Quad;

            var renderer = go.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            newmat = new Material(TerrainShader);
            renderer.material = newmat;
            go.layer = layer;
            go.transform.parent = transform;
            go.transform.position = position + new Vector3(size.x / 2, 0, size.x / 2);
            go.transform.rotation = Quaternion.Euler(90, 0, 0);
            go.transform.localScale = new Vector3(size.x, size.x, 0);

            InstanceQuad = go;
        }

        HeightMap = terrain.terrainData.heightmapTexture;
        if (!HeightMap) return;
        newmat.SetTexture("_HeightMap", HeightMap);
        newmat.SetFloat("_TerrainHeightMax", terrain.terrainData.heightmapScale.y);
        newmat.SetFloat("_TerrainYpos", terrain.gameObject.transform.position.y / 20);
    }



#if UNITY_EDITOR
    private void OnDestroy()
    {
        DestroyImmediate(InstanceQuad);
    }
#else
    private void OnDestroy()
    {
        Destroy(InstanceQuad);
    }
#endif
}
