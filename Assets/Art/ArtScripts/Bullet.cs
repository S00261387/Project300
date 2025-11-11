using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;           //Bullet speed
    private Vector3 direction;           //Direction bullet will travel

    public void Initialize(Vector3 dir)
    {
        direction = dir.normalized;     //Set bullet travel direction
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime; //Move bullet
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);            //Destroy bullet if off screen
    }

    void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);            //Destroy bullet on collision
    }
}
