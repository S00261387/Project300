using UnityEngine;

public class DynamicLightController : MonoBehaviour
{
    [Header("Light Settings")]
    public Light lightSource;
    public float activeDistance = 15f;   // Distance to activate light
    public float fadeSpeed = 2f;         // How fast the light fades in/out

    private Transform player;
    private float baseIntensity;
    private float targetIntensity;
    private bool initialized = false;
    

    void Awake()
    {
        if (!lightSource)
            lightSource = GetComponentInChildren<Light>();

        if (lightSource)
            baseIntensity = lightSource.intensity; // Save the prefab’s true intensity
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Start turned off (for fading logic)
        if (lightSource)
            lightSource.intensity = 0f;

        initialized = true;
    }

    void Update()
    {
        if (!initialized || !lightSource) return;

        // If player not yet found (e.g. spawned later)
        if (!player)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj)
                player = playerObj.transform;
            else
                return;
        }

        float dist = Vector3.Distance(player.position, transform.position);
        bool shouldBeOn = dist < activeDistance;

        targetIntensity = shouldBeOn ? baseIntensity : 0f;

        // Smooth fade
        lightSource.intensity = Mathf.Lerp(
            lightSource.intensity,
            targetIntensity,
            Time.deltaTime * fadeSpeed
        );
    }

    void OnDisable()
    {
        // Reset to base intensity if script removed or object disabled
        if (lightSource)
            lightSource.intensity = baseIntensity;
    }
}