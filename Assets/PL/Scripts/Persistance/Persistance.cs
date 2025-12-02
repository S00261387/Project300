using UnityEngine;

public class Persistence : MonoBehaviour
{
    private static readonly System.Collections.Generic.HashSet<string> persistentObjects
        = new System.Collections.Generic.HashSet<string>();

    private void Awake()
    {
        string id = gameObject.name;

        if (persistentObjects.Contains(id))
        {
            Destroy(gameObject);
        }
        else
        {
            persistentObjects.Add(id);
            DontDestroyOnLoad(gameObject);
        }
    }
}
