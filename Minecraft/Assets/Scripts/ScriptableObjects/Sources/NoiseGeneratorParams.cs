using static FastNoiseLite;
using UnityEngine;

[CreateAssetMenu(fileName = "New Noise Parameters", menuName = "Create Noise Parameters")]
public class NoiseGeneratorParams : ScriptableObject
{
    public int mSeed = 1337;
    public float mFrequency = 0.01f;
    public NoiseType mNoiseType = NoiseType.OpenSimplex2;
    public RotationType3D mRotationType3D = RotationType3D.None;

    public FractalType mFractalType = FractalType.None;
    public int Octaves = 3;
    public float Lacunarity = 2.0f;
    public float Gain = 0.5f;
    public float WeightedStrength = 0.0f;
    public float PingPongStength = 2.0f;

    public float FractalBounding = 1 / 1.75f;

    public CellularDistanceFunction CellularDistanceFunction = CellularDistanceFunction.EuclideanSq;
    public CellularReturnType CellularReturnType = CellularReturnType.Distance;
    public float CellularJitterModifier = 1.0f;

    public DomainWarpType DomainWarpType = DomainWarpType.OpenSimplex2;
    public float DomainWarpAmp = 1.0f;

    public float Intensity;
}
