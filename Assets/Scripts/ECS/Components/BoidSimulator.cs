using Unity.Entities;

public struct BoidSimulator : IComponentData
{
    public float Value;

    public void Reset()
    {
    }
}

public struct SimulationSettings : IComponentData
{
    public float BoidDensity;
    public int RoundWorldSizeToMultiplesOf;
}