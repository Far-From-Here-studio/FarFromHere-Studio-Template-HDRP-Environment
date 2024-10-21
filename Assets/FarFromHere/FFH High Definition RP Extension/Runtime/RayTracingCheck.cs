// 2023-12-15 AI-Tag 
// This was created with assistance from Muse, a Unity Artificial Intelligence product
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace FFH.HighDefinition.Extension
{
    public class RayTracingCheckEditor
    {
        static RayTracingCheckEditor()
        {
            CheckRaytracing();
        }
        static void CheckRaytracing()
        {
            ReflectionProbesGroups[] reflectionprobesgroups = Object.FindObjectsByType<ReflectionProbesGroups>(FindObjectsSortMode.None);
            // Get the current Render Pipeline Asset
            var activeRenderPipeline = GraphicsSettings.currentRenderPipeline;
            // Cast to HDRP asset
            var hdrpAsset = activeRenderPipeline as HDRenderPipelineAsset;

            if (hdrpAsset != null)
            {
                // Access the current platform render pipeline settings
                RenderPipelineSettings settings = hdrpAsset.currentPlatformRenderPipelineSettings;

                // Check if ray tracing is enabled
                if (settings.supportRayTracing)
                {
                    // If ray tracing is enabled, deactivate the GameObject
                    foreach (var reflectionProbe in reflectionprobesgroups)
                    {
                        if (reflectionProbe.gameObject.activeSelf) reflectionProbe.gameObject.SetActive(false);
                    }

                }
                else
                {
                    foreach (var reflectionProbe in reflectionprobesgroups)
                    {
                        if (!reflectionProbe.gameObject.activeSelf) reflectionProbe.gameObject.SetActive(true);
                    }
                }
            }
        }
    }


}


