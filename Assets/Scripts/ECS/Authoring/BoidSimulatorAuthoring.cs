using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring
{
    public class BoidSimulatorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Header("Simulation Settings")] [SerializeField]
        private float boidDensity = 4f;

        [SerializeField] private int roundWorldSizeToMultiplesOf = 5;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<BoidSimulator>(entity);
            dstManager.AddComponentData(entity, new SimulationSettings
            {
                BoidDensity = boidDensity,
                RoundWorldSizeToMultiplesOf = roundWorldSizeToMultiplesOf
            });
        }
    }
}