using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public void LoadGameScene()
    {
        SceneManager.LoadScene("Level_1_scene", LoadSceneMode.Single);
    }
}