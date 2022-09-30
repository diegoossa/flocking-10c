using Unity.Entities;
using UnityEngine;

public class WorldSettingsAuthoring : MonoBehaviour
{
    [Header("Boid World Settings")] 
    public float InitialVelocity = 2.0f;
    public float BoidDensity = 4f;
    public int RoundWorldSizeToMultiplesOf = 5;
}

public class WorldSettingsAuthoringBaker : Baker<WorldSettingsAuthoring>
{
    public override void Bake(WorldSettingsAuthoring authoring)
    {
        AddComponent(new WorldSettings
        {
            BoidDensity = authoring.BoidDensity,
            RoundWorldSizeToMultiplesOf = authoring.RoundWorldSizeToMultiplesOf,
            InitialVelocity = authoring.InitialVelocity
        });
    }
}