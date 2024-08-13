using Unity.Entities;
using UnityEngine;

 struct TreeShared : IComponentData
{
}

public class TreesAutoring : MonoBehaviour
{
    public class Baker : Baker<TreesAutoring>
    {
        public override void Bake(TreesAutoring authoring)
        {
            var xx = new TreeShared();

            var entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, xx);
             
        }
    }
}
