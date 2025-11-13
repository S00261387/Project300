using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;          
    public float bulletSpeed = 10f;          //Bullet speed

    [Header("Fire Rates (bullets/sec)")]
    public float semiAutoFireRate = 5f;      //Semi-auto fire rate
    public float fullAutoFireRate = 10f;     //Full-auto fire rate

    [Header("Audio")]
    public AudioSource gunAudio;             //Gunshot sound

    private int fireMode = 1;                 //Firenode Bindigs
    private float fireCooldown = 0f;          //Cooldown for fire rate

    void Update()
    {
        //Switch fire mode
        if (Input.GetKeyDown(KeyCode.Alpha1)) fireMode = 1;
        if (Input.GetKeyDown(KeyCode.Alpha2)) fireMode = 2;

        //Semi-auto: one bullet per click
        if (fireMode == 1 && Input.GetMouseButtonDown(0))
        {
            Shoot();
            fireCooldown = 1f / semiAutoFireRate;
        }
        //Full-auto: hold mouse to fire continuously
        else if (fireMode == 2 && Input.GetMouseButton(0))
        {
            if (fireCooldown <= 0f)
            {
                Shoot();
                fireCooldown = 1f / fullAutoFireRate;
            }
        }

        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;
    }

    void Shoot()
    {
        if (bulletPrefab == null) return; 

        //Play gunshot sound
        if (gunAudio != null)
            gunAudio.PlayOneShot(gunAudio.clip);

        //Get mouse position in world space
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.transform.position.y;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.y = transform.position.y;             //Keep bullet on player height

        Vector3 dir = (worldPos - transform.position).normalized; //Direction to mouse

        //Spawn bullet
        GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.speed = bulletSpeed;
            bullet.Initialize(dir);                    //Set bullet direction (on mouse)
        }
    }
}
