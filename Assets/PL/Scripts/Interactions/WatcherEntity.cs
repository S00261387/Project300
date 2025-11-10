using UnityEngine;

public class WatcherEntity : MonoBehaviour
{
    public Transform player;
    public float lookSpeed = 2f;
    public float activationDistance = 20f;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null)
            return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > activationDistance)
            return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        Quaternion targetRot = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * lookSpeed);
    }
}