using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeDoorInteract : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log("Player reached escape door");

        if (SanityManager.Instance != null &&
            SanityManager.Instance.maxSanity >= 100f)
        {
            Debug.Log("Game completed");

            SceneManager.sceneLoaded += CleanAfterLoad;
            SceneManager.LoadScene("EndScene");

            return;
        }

        Debug.Log("Going to next dungeon floor");

        if (GameManager.Instance != null)
            GameManager.Instance.currentLevel++;

        DungeonBuilder builder = FindFirstObjectByType<DungeonBuilder>();
        if (builder != null)
            builder.GenerateNewMap();
    }

    void CleanAfterLoad(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= CleanAfterLoad;

        GameObject temp = new GameObject("Temp");
        DontDestroyOnLoad(temp);

        Scene dontDestroyScene = temp.scene;

        foreach (GameObject obj in dontDestroyScene.GetRootGameObjects())
        {
            if (obj != temp)
                Destroy(obj);
        }

        Destroy(temp);
    }
}