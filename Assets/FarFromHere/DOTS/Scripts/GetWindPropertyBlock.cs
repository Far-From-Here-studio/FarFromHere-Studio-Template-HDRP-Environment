using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class GetWindPropertyBlock : MonoBehaviour
{
    public LayerMask layermask;
    public Material[] SharedMaterialsList;
    public BatchMaterialID[] MaterialIDs;

    private EntityManager _manager;
    private EntitiesGraphicsSystem _hybrid;
    private bool initialization = false;

    public GameObject[] SpeedtreePrototypes;
    public Material[] SpeedtreeTempMaterials;
    private Renderer[] renderers;

    //needed to read SpeedTree's wind 
    private MaterialPropertyBlock[] MaterialProperties;

    private EntityQuery rendermesharrayfinder;
    private Entity arraysharedEntity;
    private RenderMeshArray rendermesharraydata;

    public GameObject[] instances;

    [SerializeField]
    private GameObject NoCullPass;


    private void OnEnable()
    {

        arraysharedEntity = new Entity();
        rendermesharraydata = new RenderMeshArray();

        SpeedtreeTempMaterials = new Material[SpeedtreePrototypes.Length];
        renderers = new Renderer[SpeedtreePrototypes.Length];
        MaterialProperties = new MaterialPropertyBlock[SpeedtreePrototypes.Length];
        instances = new GameObject[SpeedtreePrototypes.Length];

        for (int i = 0; i < SpeedtreePrototypes.Length; i++)
        {
            MaterialProperties[i] = new MaterialPropertyBlock();

            //Create a clone of each SpeedtreePrototypes
            var renderer = SpeedtreePrototypes[i].GetComponentInChildren<MeshRenderer>();
            var instance = Instantiate(renderer.gameObject);

            //instance.hideFlags = HideFlags.HideAndDontSave;

            //TO DO : Create a new NoCullElements layers with a new CustomPass to handle the culling to never appen
            var layer = LayerMask.NameToLayer("Default");
            instance.gameObject.layer = 0;
            instance.gameObject.layer |= layer;
            instances[i] = instance;

            renderers[i] = instance.GetComponent<MeshRenderer>();
            //renderers[i].material.name = SpeedtreePrototypes[i].name;

            //Temp materials to store PropertyBlock, as we cannot use the tree's original materials without bug
            SpeedtreeTempMaterials[i] = new Material(renderers[i].material);
            SpeedtreeTempMaterials[i].name = SpeedtreePrototypes[i].name;
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < instances.Length; i++)
        {
            Destroy(instances[i]);
        }
    }
    private void OnDisable()
    {
        for (int i = 0; i < instances.Length; i++)
        {
            Destroy(instances[i]);
        }
    }

    private void LateUpdate()
    {
      

        _manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _hybrid = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EntitiesGraphicsSystem>();
        //Wait a little bit to warmUp wind and the ECS world's creation on new loadings
        if (_manager.World.IsCreated == false && _hybrid.World.IsCreated == false) return;


        if (initialization == false)
        {
            //Look for an entity that old the MaterialReferences
            rendermesharrayfinder = _manager.CreateEntityQuery(typeof(RenderMeshArray));
            arraysharedEntity = rendermesharrayfinder.ToEntityArray(Allocator.Temp).FirstOrDefault();

            //TODO Build a IndexList to not use string.contain to match materials
            if (_manager.IsQueryValid(rendermesharrayfinder) && _manager.Exists(arraysharedEntity))
            {
                //Look for the Entity Shared Material List store in MaterialReferences
                rendermesharraydata = _manager.GetSharedComponentManaged<RenderMeshArray>(arraysharedEntity);

                //Create a equivalent List with Temp materials
                SharedMaterialsList = new Material[rendermesharraydata.MaterialReferences.Length];
                MaterialIDs = new BatchMaterialID[rendermesharraydata.MaterialReferences.Length];

                //Get the BatchMaterialIDs from the EntitiesGraphicsSystem
                for (int spm = 0; spm < SpeedtreeTempMaterials.Length; spm++)
                {
                    MaterialIDs[spm] = _hybrid.RegisterMaterial(SpeedtreeTempMaterials[spm]);
                }

                //Compare SpeedtreeTempMaterials and MaterialsRefences, if the name is the same, fill SharedMaterialsList list
                //In order to know what MaterialReferences to update
                for (int m = 0; m < rendermesharraydata.MaterialReferences.Length; m++)
                {
                    SharedMaterialsList[m] = _hybrid.GetMaterial(MaterialIDs[m]);
                    if(SharedMaterialsList[m] != null) Debug.Log(SharedMaterialsList[m].name + m);
                }

                /*
                foreach (var material in SharedMaterialsList)
                {
                    foreach (var speedtreeTempMat in SpeedtreeTempMaterials)
                    {
                        if (material.name.Contains(speedtreeTempMat.name))
                        {

                        }
                    }

                }
                */
            }
            initialization = true;
        }
        if (initialization == false) return;

        //Debug propertyBlock
        for (int proto = 0; proto < renderers.Length; proto++)
        {
            //Get Tree gamobject's wind PropertyBlock and store it in a list (directly from TreeProptypes renderer's)
            renderers[proto].GetPropertyBlock(MaterialProperties[proto]);
            
        }
        var prop = MaterialProperties.FirstOrDefault().GetVector("_ST_WindTurbulences");
        Debug.Log(prop);

        var material = renderers.FirstOrDefault().material;
        var wind = material.GetVector("_ST_WindTurbulences");
        Debug.Log(wind);

        /*
        //Extract Wind properties from proptotypes to our TempMaterial
        for (int i = 0; i < SpeedtreePrototypes.Length; i++)
        {
            //Get Tree gamobject's wind PropertyBlock and store it in a list (directly from TreeProptypes renderer's)
            renderers[i].GetPropertyBlock(MaterialProperties[i]);

            //Use our stored PropertyBlocks to inject SpeedtreeTempMaterials
            GetSpeedTreeWindParametersFromMaterialBlock(MaterialProperties[i], SpeedtreeTempMaterials[i]);
        }



        //Inject Wind properties to ECS world, by filtering the null materials to loop only the right ones
        for (var x = 0; x < rendermesharraydata.MaterialReferences.Length; x++)
        {
            //Get our Temporary SharedMaterialsList (SharedMaterialsList can point to a SpeedtreeTempMaterials or a null materials)
            //and inject the ECS world Wind properties if a SharedMaterialsList's material is not null, it can transfer the properties to the MaterialReferences
            if (SharedMaterialsList[x] != null) SetSpeedTreeWindParametersForMaterial(SharedMaterialsList[x], rendermesharraydata.MaterialReferences[x]);
        }
        */
    }

    public void GetSpeedTreeWindParametersFromMaterialBlock(MaterialPropertyBlock materialPropertyBlock, Material dstMat)
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
}
