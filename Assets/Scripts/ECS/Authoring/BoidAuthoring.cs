using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring
{
    public class BoidAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<Boid>(entity);
            dstManager.AddComponent<Velocity>(entity);
        }
    }
}