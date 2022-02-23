using UnityEngine.Profiling;
using UnityEngine;
using TMPro;

public class DebugMenuScript : MonoBehaviour
{
    public TextMeshProUGUI FPSDisplay;
    [Space]
    public TextMeshProUGUI CPUDisplay;
    public TextMeshProUGUI GPUDisplay;
    public TextMeshProUGUI RAMDisplay;
    [Space]
    public TextMeshProUGUI PlayerPositionDisplay;
    public TextMeshProUGUI PlayerChunkPosition;
    public TextMeshProUGUI PlayerBlockFacing;
    public TextMeshProUGUI PlayerBiome;
    [Space]
    [SerializeField] private Movement PlayerMovementClass;
    [SerializeField] private PlayerBlockEditor PlayerBlockEditorClass;
    private void Awake()
    {
        GPUDisplay.SetText($"GPU-INFO: {SystemInfo.graphicsDeviceName}");
        CPUDisplay.SetText($"CPU-INFO: {SystemInfo.processorType}");
        InvokeRepeating("UpdateStats", 0f, 0.1f);
    }

    private void Update()
    {
        PlayerBlockFacing.SetText($"BLOCK-FACING: \n WORLD: {PlayerBlockEditorClass.WorldBlockPos} \n LOCAL: {PlayerBlockEditorClass.LocalBlockPos}");
        PlayerPositionDisplay.SetText($"PLAYER-POSITION: {PlayerMovementClass.transform.position}");
        PlayerChunkPosition.SetText($"CURR-CHUNK {PlayerMovementClass.CurrentCoord.x} {PlayerMovementClass.CurrentCoord.y}");
        PlayerBiome.SetText($"CURR-BIOME {PlayerBlockEditorClass.GetBlockBiome()}");
    }

    private void UpdateStats()
    {
        RAMDisplay.SetText($"RAM-INFO: {Profiler.GetTotalAllocatedMemoryLong() / 1000000} MB / {Profiler.GetTotalReservedMemoryLong() / 1000000} MB");
        FPSDisplay.SetText($"EST. FPS: {Mathf.CeilToInt(1f / Time.unscaledDeltaTime)}");
    }
}
