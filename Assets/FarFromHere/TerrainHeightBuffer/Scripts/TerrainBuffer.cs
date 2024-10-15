using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine;
using static UnityEngine.Rendering.HighDefinition.CustomPassUtils;


#if UNITY_EDITOR
using UnityEditor.Rendering.HighDefinition;

[CustomPassDrawer(typeof(TerrainBuffer))]
class TerrainBufferEditor : CustomPassDrawer
{
    protected override PassUIFlag commonPassUIFlags => PassUIFlag.Name;
}
#endif
public class TerrainBuffer : CustomPass
{
    public LayerMask DefaultLayerMask;
    public RenderTexture TerrainRT;
    public Vector2 BufferSize;

    public Transform BufferSourceAreaTranform;

    public float OrthographicSize = 50;

    private RTHandle terrainRTHandle;

    //[HideInInspector]
    public GameObject BufferSourceCamera;
    //[HideInInspector]
    public Camera ProjectionCamera;
    private ShaderTagId[] TerrainElementshaderPasses { get; set; }

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        terrainRTHandle = RTHandles.Alloc(TerrainRT);



        var TerrainElementPass = new ShaderTagId("TerrainElement");
        TerrainElementshaderPasses = new ShaderTagId[]
        {
            TerrainElementPass
        };

        ProjectionCamera = GhostCameraSetup();
    }

    protected override void Execute(CustomPassContext ctx)
    {
        //Perform CustomCulling from our GhostCamera 

        CoreUtils.SetRenderTarget(ctx.cmd, terrainRTHandle, ClearFlag.Color);

        if (ProjectionCamera != null)
        {
            CameraCulling(ctx, ProjectionCamera);
            RenderFromCamera(ctx, TerrainElementshaderPasses, ProjectionCamera, DefaultLayerMask);
        }

    }

    private void CameraCulling(CustomPassContext ctx, Camera camera)
    {
        camera.TryGetCullingParameters(out var cullingParams);
        cullingParams.cullingOptions = CullingOptions.None;
        ctx.cullingResults = ctx.renderContext.Cull(ref cullingParams);
    }
    protected override void Cleanup()
    {
        CoreUtils.Destroy(ProjectionCamera);
        base.Cleanup();
    }


    private Camera GhostCameraSetup()
    {
        var cameras = Object.FindObjectsByType<TerrainGhostCamera>(FindObjectsSortMode.None);
        foreach (var camera in cameras) { Object.DestroyImmediate(camera.gameObject); }
        if (!BufferSourceCamera)
        {
            BufferSourceCamera = new GameObject();
            BufferSourceCamera.name = "CullingCameraAnchor";
            BufferSourceCamera.transform.parent = BufferSourceAreaTranform;
            BufferSourceCamera.transform.localPosition = new Vector3(0, 100, 0);
            BufferSourceCamera.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
            //BufferSourceCamera.hideFlags = HideFlags.HideAndDontSave;
            BufferSourceCamera.AddComponent<TerrainGhostCamera>();

            var cam = BufferSourceCamera.AddComponent<Camera>();
            cam.targetTexture = terrainRTHandle;
            cam.orthographic = true;
            cam.orthographicSize = OrthographicSize;
            cam.cullingMask = DefaultLayerMask.value;
            cam.depth = -49;
            cam.enabled = false;

            return cam;

        }
        else
        { 
            return BufferSourceCamera.GetComponent<Camera>();
        }
    }
}

public class TerrainGhostCamera : MonoBehaviour
{

}

