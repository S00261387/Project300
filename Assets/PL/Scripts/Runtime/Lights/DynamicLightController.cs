using UnityEngine;

public class DynamicLightController : MonoBehaviour
{
    [Header("Light Settings")]
    public Light lightSource;
    public float activeDistance = 15f;   // Distance to activate light
    public float fadeSpeed = 2f;
    public float flickerIntensity = 0.3f;
    public float flickerSpeed = 5f;

    private Transform player;
    private float baseIntensity;
    private float targetIntensity;

    void Start()
    {
        if (lightSource == null)
            lightSource = GetComponentInChildren<Light>();

        if (lightSource != null)
        {
            baseIntensity = lightSource.intensity;
            lightSource.intensity = 0f;
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null || lightSource == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        bool shouldBeOn = dist < activeDistance;

        float flicker = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f) * flickerIntensity; //perline noise is a type of noise like gradient

        targetIntensity = shouldBeOn ? baseIntensity + flicker : 0f;

        lightSource.intensity = Mathf.Lerp(lightSource.intensity, targetIntensity, Time.deltaTime * fadeSpeed);
    }
}