using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Scenes;
using UnityEngine;

public class GetWindPropertyBlock : MonoBehaviour
{
    public LayerMask layerMask;
    public SubScene TreesScene;
    private Material[] matsIndex;

    private EntityManager _manager;
    private bool init = false;

    public GameObject[] TreePrototypes;
    private Material[] TreeMaterials;
    private MaterialPropertyBlock[] MaterialProperties;
    private Renderer[] renderers;

    private EntityQuery rendermesharrayfinder;
    private Entity arraysharedEntity;
    private RenderMeshArray rendermesharraydata;
    private bool setindex;

    private void OnEnable()
    {
        arraysharedEntity = new Entity();

        TreeMaterials = new Material[TreePrototypes.Length];
        renderers = new Renderer[TreePrototypes.Length];
        MaterialProperties = new MaterialPropertyBlock[TreePrototypes.Length];
        rendermesharraydata = new RenderMeshArray();
        matsIndex = new Material[0];

        for (int i = 0; i < TreePrototypes.Length; i++)
        {
            var renderer = TreePrototypes[i].GetComponentInChildren<MeshRenderer>();

            var instance = Instantiate(renderer.gameObject);
            instance.hideFlags = HideFlags.HideAndDontSave;
            instance.gameObject.layer = 0;
            instance.gameObject.layer |= LayerMask.NameToLayer("MeteoVFXElements");

            var instancerenderer = instance.GetComponent<MeshRenderer>();
            instancerenderer.renderingLayerMask = (uint)0;

            MaterialProperties[i] = new MaterialPropertyBlock();
            renderers[i] = instancerenderer;
            renderers[i].material.name = TreePrototypes[i].name;
            TreeMaterials[i] = new Material(renderers[i].material);
            TreeMaterials[i].name = TreePrototypes[i].name;
        }
        setindex = false;

    }
    private void LateUpdate()
    {
        TreesScene.AutoLoadScene = true;
        if (TreesScene.gameObject == null) return;

        _manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        if (_manager.World.IsCreated == false) return;
        if (_manager.World.Time.ElapsedTime < 0.5f) return;

        if (init == false)
        {
            rendermesharrayfinder = _manager.CreateEntityQuery(typeof(RenderMeshArray));
            arraysharedEntity = rendermesharrayfinder.ToEntityArray(Allocator.Temp).FirstOrDefault();

            if (_manager.IsQueryValid(rendermesharrayfinder) && _manager.Exists(arraysharedEntity))
            {
                rendermesharraydata = _manager.GetSharedComponentManaged<RenderMeshArray>(arraysharedEntity);

                matsIndex = new Material[rendermesharraydata.Materials.Length];

                foreach (Material mats in TreeMaterials)
                {
                    for (int m = 0; m < rendermesharraydata.Materials.Length; m++)
                    {
                            if (rendermesharraydata.Materials[m].name.Contains(mats.name))
                            {
                                matsIndex[m] = mats;
                            }
                    }              
                }  
            }

            init = true;
        }

        if (init == false) return;

        for (int i = 0; i < TreePrototypes.Length; i++)
        {
            renderers[i].GetPropertyBlock(MaterialProperties[i]);
            SetSpeedTreeWindParametersForMaterialBlock(MaterialProperties[i], TreeMaterials[i]);
        }
        //Build a IndexList to not use string.contain to match materials

            //Use the matching stored index to inject the right Materials
            for (var x = 0; x < rendermesharraydata.Materials.Length; x++)
            {
                    if(matsIndex[x] != null) SetSpeedTreeWindParametersForMaterial(matsIndex[x], rendermesharraydata.Materials[x]);
            }
      
        // _manager.SetSharedComponentManaged<RenderMeshArray>(arraysharedEntity, rendermesharraydata);
    }

    public void SetSpeedTreeWindParametersForMaterial(Material srcMat, Material dstMat)
    {
        dstMat.SetVector("_ST_WindAnimation", srcMat.GetVector("_ST_WindAnimation"));
        dstMat.SetVector("_ST_WindBranch", srcMat.GetVector("_ST_WindBranch"));
        dstMat.SetVector("_ST_WindBranchAdherences", srcMat.GetVector("_ST_WindBranchAdherences"));
        dstMat.SetVector("_ST_WindBranchAnchor", srcMat.GetVector("_ST_WindBranchAnchor"));
        dstMat.SetVector("_ST_WindBranchTwitch", srcMat.GetVector("_ST_WindBranchTwitch"));
        dstMat.SetVector("_ST_WindBranchWhip", srcMat.GetVector("_ST_WindBranchWhip"));
        dstMat.SetVector("_ST_WindFrondRipple", srcMat.GetVector("_ST_WindFrondRipple"));
        dstMat.SetVector("_ST_WindGlobal", srcMat.GetVector("_ST_WindGlobal"));
        dstMat.SetVector("_ST_WindLeaf1Ripple", srcMat.GetVector("_ST_WindLeaf1Ripple"));
        dstMat.SetVector("_ST_WindLeaf1Tumble", srcMat.GetVector("_ST_WindLeaf1Tumble"));
        dstMat.SetVector("_ST_WindLeaf1Twitch", srcMat.GetVector("_ST_WindLeaf1Twitch"));
        dstMat.SetVector("_ST_WindLeaf2Ripple", srcMat.GetVector("_ST_WindLeaf2Ripple"));
        dstMat.SetVector("_ST_WindLeaf2Tumble", srcMat.GetVector("_ST_WindLeaf2Tumble"));
        dstMat.SetVector("_ST_WindLeaf2Twitch", srcMat.GetVector("_ST_WindLeaf2Twitch"));
        dstMat.SetVector("_ST_WindTurbulences", srcMat.GetVector("_ST_WindTurbulences"));
        dstMat.SetVector("_ST_WindVector", srcMat.GetVector("_ST_WindVector"));
    }
    public void SetSpeedTreeWindParametersForMaterialBlock(MaterialPropertyBlock materialPropertyBlock, Material dstMat)
    {
        dstMat.SetVector("_ST_WindAnimation", materialPropertyBlock.GetVector("_ST_WindAnimation"));
        dstMat.SetVector("_ST_WindBranch", materialPropertyBlock.GetVector("_ST_WindBranch"));
        dstMat.SetVector("_ST_WindBranchAdherences", materialPropertyBlock.GetVector("_ST_WindBranchAdherences"));
        dstMat.SetVector("_ST_WindBranchAnchor", materialPropertyBlock.GetVector("_ST_WindBranchAnchor"));
        dstMat.SetVector("_ST_WindBranchTwitch", materialPropertyBlock.GetVector("_ST_WindBranchTwitch"));
        dstMat.SetVector("_ST_WindBranchWhip", materialPropertyBlock.GetVector("_ST_WindBranchWhip"));
        dstMat.SetVector("_ST_WindFrondRipple", materialPropertyBlock.GetVector("_ST_WindFrondRipple"));
        dstMat.SetVector("_ST_WindGlobal", materialPropertyBlock.GetVector("_ST_WindGlobal"));
        dstMat.SetVector("_ST_WindLeaf1Ripple", materialPropertyBlock.GetVector("_ST_WindLeaf1Ripple"));
        dstMat.SetVector("_ST_WindLeaf1Tumble", materialPropertyBlock.GetVector("_ST_WindLeaf1Tumble"));
        dstMat.SetVector("_ST_WindLeaf1Twitch", materialPropertyBlock.GetVector("_ST_WindLeaf1Twitch"));
        dstMat.SetVector("_ST_WindLeaf2Ripple", materialPropertyBlock.GetVector("_ST_WindLeaf2Ripple"));
        dstMat.SetVector("_ST_WindLeaf2Tumble", materialPropertyBlock.GetVector("_ST_WindLeaf2Tumble"));
        dstMat.SetVector("_ST_WindLeaf2Twitch", materialPropertyBlock.GetVector("_ST_WindLeaf2Twitch"));
        dstMat.SetVector("_ST_WindTurbulences", materialPropertyBlock.GetVector("_ST_WindTurbulences"));
        dstMat.SetVector("_ST_WindVector", materialPropertyBlock.GetVector("_ST_WindVector"));
    }
}
