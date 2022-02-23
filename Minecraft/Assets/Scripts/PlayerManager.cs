using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    private Movement[] PlayerMovementClasses;

    private void Awake()
    {
        Instance = this;
        PlayerMovementClasses = FindObjectsOfType<Movement>();
    }

    public void Init()
    {
        foreach(Movement m in PlayerMovementClasses)
        {
            m.SpawnPlayer();
        }
    }

}
