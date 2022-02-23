using UnityEngine;

[CreateAssetMenu(fileName = "New Generation Feature", menuName = "Create Generation Feature")]
public class GenerationFeature : ScriptableObject
{

    public enum GenerationType
    {
        Chance
    }

    public BlockInfo[] StructuralBlockPositions;
    public GenerationType MethodToGenerate;
    [Tooltip("1 = Forest \n 2 = Desert \n 3 = Mountain")]
    public int HousingBiome;
    [Space]
    public int yOffset;
    [Range(0.0f, 1.0f)]
    public float ChanceToGenerate = 1;
    public float GenDistance;
    public int ExpectedNumberPerChunk = 3;
}
