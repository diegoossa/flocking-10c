using Unity.Entities;
using UnityEngine;

public class WorldSettingsAuthoring : MonoBehaviour
{
    [Header("Boid World Settings")] 
    public float initialVelocity = 2.0f;
    public float boidDensity = 4f;
    public int roundWorldSizeToMultiplesOf = 5;
}

public class WorldSettingsAuthoringBaker : Baker<WorldSettingsAuthoring>
{
    public override void Bake(WorldSettingsAuthoring authoring)
    {
        Entity entity = GetEntity(authoring.gameObject, TransformUsageFlags.None);
        AddComponent(entity, new WorldSettings
        {
            BoidDensity = authoring.boidDensity,
            RoundWorldSizeToMultiplesOf = authoring.roundWorldSizeToMultiplesOf,
            InitialVelocity = authoring.initialVelocity
        });
    }
}