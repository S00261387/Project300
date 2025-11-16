using UnityEngine;

public class Distratable : MonoBehaviour
{
    private GameObject[] Enemies;
    private Rigidbody rb;

    private void Awake()
    {
        Enemies = GameObject.FindGameObjectsWithTag("Enemy");
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
       rb.isKinematic = true;

        gameObject.layer = 8;
        if (collision.gameObject.tag != ("Enemy"))
        {
           
            foreach (GameObject enemy in Enemies)
            {
                EnemyAi ai = enemy.GetComponent<EnemyAi>();
                if (Vector3.Distance(transform.position, enemy.transform.position) <= ai.DistractRadius)
                {
                    ai.Alert(transform.position);
                }
            }
        }
        else
        {

        }
    }
}
