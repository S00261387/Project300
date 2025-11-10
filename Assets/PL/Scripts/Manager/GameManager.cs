using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentLevel = 1;
    public DungeonBuilder builder;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void NextLevel()
    {
        currentLevel++;
        Debug.Log($"Generating Level {currentLevel}");

        if (builder != null)
        {
            builder.GenerateNewMap();
        }
        else
        {
            Debug.LogWarning("builder is null");
        }
    }
}
