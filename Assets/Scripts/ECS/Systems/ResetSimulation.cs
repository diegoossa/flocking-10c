using Unity.Entities;
using UnityEngine;

public partial class ResetSimulation : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<BoidSimulator>();
    }
    
    protected override void OnUpdate()
    {
        var boidSimulator = GetSingleton<SimulationSettings>();
        Debug.Log(boidSimulator.BoidDensity);
    }
}