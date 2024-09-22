using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine;

class NoCullRenderPass : CustomPass
{
    public LayerMask HiddenRuntimeLayerMask;
    public Camera NoCullCamera;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        if(!NoCullCamera)
        {
            NoCullCamera.gameObject.SetActive(false);
        }
    }
    protected override void Execute(CustomPassContext ctx)
    {
        if (NoCullCamera)
        {
            if (NoCullCamera.TryGetCullingParameters(out var cullingParams))
            {
                cullingParams.cullingOptions = CullingOptions.None;
                ctx.cullingResults = ctx.renderContext.Cull(ref cullingParams);
            }
        }
    }
    protected override void Cleanup()
    {
    }
}