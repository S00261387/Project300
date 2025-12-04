using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SanityManager : MonoBehaviour
{
    public static SanityManager Instance { get; private set; }

    public SanityBar sanityBar;

    [Header("Sanity Settings")]
    public float sanity = 10f;          // current sanity
    public float maxSanity = 10f;       // max sanity cap (increases over time)
    public float drainRateInDarkness = 2f;
    public float recoverRateInLight = 1f;
    public bool isInDarkness = true;

    [Header("Visual Effects")]
    public Volume postProcessVolume;

    ChromaticAberration chromatic;
    Vignette vignette;

    float vignettePulseSpeed = 2f;
    float vignetteBaseIntensity = 0.25f;
    float vignettePulseRange = 0.15f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out chromatic);
            postProcessVolume.profile.TryGet(out vignette);
        }
    }

    void Update()
    {
        UpdateSanity();
        UpdateVisualEffects();
        sanityBar.SetSanity(sanity / 100f);
    }

    void UpdateSanity()
    {
        if (isInDarkness)
            sanity -= drainRateInDarkness * Time.deltaTime;
        else
            sanity += recoverRateInLight * Time.deltaTime;

        sanity = Mathf.Clamp(sanity, 0f, maxSanity);
    }

    void UpdateVisualEffects()
    {
        float normalizedSanity = sanity / 100f; // visual scale still assumes full range for chromatic

        if (chromatic != null)
            chromatic.intensity.value = Mathf.Lerp(0f, 1f, 1f - normalizedSanity);

        if (vignette != null)
        {
            if (sanity < 15f)
            {
                float pulse = vignetteBaseIntensity + Mathf.Sin(Time.time * vignettePulseSpeed) * vignettePulseRange;
                vignette.intensity.value = Mathf.Clamp01(pulse);
            }
            else
            {
                vignette.intensity.value = 0f;
            }
        }
    }

    // Public method to increase the sanity cap
    public void IncreaseMaxSanity(float amount)
    {
        maxSanity = Mathf.Clamp(maxSanity + amount, 10f, 100f);
        sanity = Mathf.Clamp(sanity, 0f, maxSanity);
        Debug.Log("New max sanity: " + maxSanity);
    }

    // Optional helper to fully restore sanity to current max
    public void RestoreSanity()
    {
        sanity = maxSanity;
    }

    public void DrainSanity(float amount)
    {
        sanity = Mathf.Max(0, sanity - amount);
    }
}
