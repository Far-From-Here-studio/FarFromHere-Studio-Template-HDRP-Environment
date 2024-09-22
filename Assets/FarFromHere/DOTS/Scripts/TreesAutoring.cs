using Unity.Entities;
using UnityEngine;
using Unity.Entities.Graphics;
using Unity.Rendering;

struct TreeShared : IComponentData
{
}

public class TreesAutoring : MonoBehaviour
{
    public class Baker : Baker<TreesAutoring>
    {
        public override void Bake(TreesAutoring authoring)
        {
            var treeShared = new TreeShared();

            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, treeShared);
             
        }
    }
}
[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateBefore(typeof(EntitiesGraphicsSystem))]
public partial class ECSSystem : SystemBase
{
    protected override void OnUpdate()
    {
        

        Entities
            .ForEach(
                (ref Entity entity, ref TreeShared treeShared) =>
                {
                    
                }
            )
            .ScheduleParallel();
    }
    
}

