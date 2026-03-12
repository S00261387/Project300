using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartGame : MonoBehaviour
{
    public void LoadGameScene()
    {
        SceneManager.LoadScene("StartMenu");
    }
}