using Unity.Entities;
using UnityEngine;

public class BoidSimulatorAuthoring : MonoBehaviour
{
    [Header("Simulation Settings")] 
    public float matchVelocityRate = 1.0f;
    public float avoidanceRange = 2.0f;
    public float avoidanceRate = 5.0f;
    public float coherenceRate = 2.0f;
    public float viewRange = 3.0f;
}

public class BoidSimulatorAuthoringBaker : Baker<BoidSimulatorAuthoring>
{
    public override void Bake(BoidSimulatorAuthoring authoring)
    {
        AddComponent(new BoidSimulationSettings
        {
            MatchVelocityRate = authoring.matchVelocityRate,
            AvoidanceRange = authoring.avoidanceRange,
            AvoidanceRate = authoring.avoidanceRate,
            CoherenceRate = authoring.coherenceRate,
            ViewRange = authoring.viewRange
        });
    }
}