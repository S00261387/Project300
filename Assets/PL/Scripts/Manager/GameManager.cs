using UnityEngine;
using UnityEngine.SceneManagement;

#pragma warning disable UDR0001

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentLevel = 1;

    private DungeonBuilder builder;

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
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        builder = Object.FindFirstObjectByType<DungeonBuilder>();
    }

    public void NextLevel()
    {
        currentLevel++;
        Debug.Log($"Accessing Floor {currentLevel}");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
