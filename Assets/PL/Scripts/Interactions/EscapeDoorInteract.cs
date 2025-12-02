using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeDoorInteract : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player escapes current level");
            GameManager.Instance.currentLevel++;

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
