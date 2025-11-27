using UnityEngine;
using System.Linq;

public class GhostDash : MonoBehaviour
{
    public float Lifetime = 0.3f; // Time before destruction
    private float _timer;

    private Renderer[] _renderers;
    private Material[][] _materials; // Array of material arrays for each renderer

    private void Awake()
    {
        // Get all renderers in the object
        _renderers = GetComponentsInChildren<Renderer>();

        // Create unique material instances per renderer
        _materials = new Material[_renderers.Length][];
        for (int i = 0; i < _renderers.Length; i++)
        {
            _materials[i] = _renderers[i].materials.Select(m => new Material(m)).ToArray();
            _renderers[i].materials = _materials[i]; // Assign unique instances
        }
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, _timer / Lifetime);

        // Fade all materials
        for (int i = 0; i < _materials.Length; i++)
        {
            foreach (var mat in _materials[i])
            {
                if (mat.HasProperty("_Color"))
                {
                    Color c = mat.color;
                    c.a = alpha;
                    mat.color = c;
                }
            }
        }

        if (_timer >= Lifetime)
        {
            Destroy(gameObject); // Destroy after fading
        }
    }
}
