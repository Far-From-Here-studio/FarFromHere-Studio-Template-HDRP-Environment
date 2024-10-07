// 2023-12-15 AI-Tag 
// This was created with assistance from Muse, a Unity Artificial Intelligence product
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[ExecuteAlways]
public class RayTracingCheck : MonoBehaviour
{
    public GameObject targetObject; // The GameObject to enable or disable

    void FixedUpdate()
    {
        // Get the current Render Pipeline Asset
        var activeRenderPipeline = GraphicsSettings.currentRenderPipeline;
        // Cast to HDRP asset
        var hdrpAsset = activeRenderPipeline as HDRenderPipelineAsset;

        if (hdrpAsset != null && targetObject != null)
        {
            // Access the current platform render pipeline settings
            RenderPipelineSettings settings = hdrpAsset.currentPlatformRenderPipelineSettings;

            // Check if ray tracing is enabled
            if (settings.supportRayTracing)
            {
                // If ray tracing is enabled, deactivate the GameObject
                if(targetObject.activeSelf) targetObject.SetActive(false);
            }
            else
            {
                // If ray tracing is disabled, activate the GameObject
                if(!targetObject.activeSelf) targetObject.SetActive(true);
            }
        }
    }
}
