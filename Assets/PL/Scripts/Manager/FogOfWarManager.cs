using UnityEngine;
using System.Collections.Generic;

public class FogOfWarManager : MonoBehaviour
{
    [Header("References")]
    public DungeonBuilder dungeonBuilder;
    public Transform player;
    public Transform lookSource;
    public Renderer fogRenderer;

    [Header("Texture Size")]
    public int textureWidth = 512;
    public int textureHeight = 256;

    [Header("Torch Vision")]
    public float torchRadius = 3.5f;

    [Header("Player Vision")]
    public float playerRadius = 2.5f;
    public float flashlightRange = 7f;
    [Range(1f, 179f)] public float flashlightAngle = 70f;

    [Header("Fog")]
    [Range(0f, 1f)] public float fogAlpha = 0.9f;
    public float overlayY = 2.5f;

    private Texture2D torchMask;
    private Texture2D playerMask;
    private Color32[] torchPixels;
    private Color32[] playerPixels;
    private Material fogMaterial;

    private Vector3 mapOrigin;
    private Vector2 mapSize;

    void Start()
    {
        RefreshReferences();

        if (dungeonBuilder == null || player == null || fogRenderer == null)
        {
            Debug.LogWarning("FogOfWarManager missing references.");
            enabled = false;
            return;
        }

        fogMaterial = fogRenderer.material;

        mapOrigin = dungeonBuilder.GetMapOrigin();
        mapSize = dungeonBuilder.GetMapSize();

        CreateTextures();
        SetupFogObject();
        ApplyMaterialData();
        RebuildStaticMask();
        UpdatePlayerMask();
        ApplyMaterialData();
    }

    void LateUpdate()
    {
        if (!enabled)
            return;

        if (player == null)
            RefreshReferences();

        if (player == null || fogMaterial == null)
            return;

        UpdatePlayerMask();
        ApplyMaterialData();
    }

    void RefreshReferences()
    {
        if (dungeonBuilder == null)
            dungeonBuilder = FindFirstObjectByType<DungeonBuilder>();

        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        if (fogRenderer == null)
            fogRenderer = GetComponent<Renderer>();
    }

    public void RefreshAllMasks()
    {
        RefreshReferences();

        if (dungeonBuilder == null || player == null || fogRenderer == null)
            return;

        if (fogMaterial == null)
            fogMaterial = fogRenderer.material;

        mapOrigin = dungeonBuilder.GetMapOrigin();
        mapSize = dungeonBuilder.GetMapSize();

        if (torchMask == null || playerMask == null)
            CreateTextures();

        SetupFogObject();
        RebuildStaticMask();
        UpdatePlayerMask();
        ApplyMaterialData();
    }

    public void RebuildStaticMask()
    {
        if (dungeonBuilder == null)
            return;

        mapOrigin = dungeonBuilder.GetMapOrigin();
        mapSize = dungeonBuilder.GetMapSize();

        if (torchMask == null || playerMask == null)
            CreateTextures();

        ClearPixels(torchPixels);

        List<GameObject> torches = dungeonBuilder.GetTorchWalls();

        for (int i = 0; i < torches.Count; i++)
        {
            if (torches[i] == null)
                continue;

            Vector3 pos = torches[i].transform.position;
            DrawCircle(torchPixels, pos, torchRadius);
        }

        torchMask.SetPixels32(torchPixels);
        torchMask.Apply(false, false);

        SetupFogObject();
        ApplyMaterialData();
    }

    void CreateTextures()
    {
        torchMask = new Texture2D(textureWidth, textureHeight,
            TextureFormat.R8, false);
        playerMask = new Texture2D(textureWidth, textureHeight,
            TextureFormat.R8, false);

        torchMask.wrapMode = TextureWrapMode.Clamp;
        torchMask.filterMode = FilterMode.Bilinear;

        playerMask.wrapMode = TextureWrapMode.Clamp;
        playerMask.filterMode = FilterMode.Bilinear;

        torchPixels = new Color32[textureWidth * textureHeight];
        playerPixels = new Color32[textureWidth * textureHeight];

        ClearPixels(torchPixels);
        ClearPixels(playerPixels);

        torchMask.SetPixels32(torchPixels);
        torchMask.Apply(false, false);

        playerMask.SetPixels32(playerPixels);
        playerMask.Apply(false, false);
    }

    void SetupFogObject()
    {
        if (fogRenderer == null)
            return;

        Transform t = fogRenderer.transform;

        t.position = new Vector3(mapOrigin.x + mapSize.x * 0.5f,
            overlayY,
            mapOrigin.z + mapSize.y * 0.5f);

        t.rotation = Quaternion.Euler(90f, 0f, 0f);
        t.localScale = new Vector3(mapSize.x, mapSize.y, 1f);
    }

    void ApplyMaterialData()
    {
        if (fogMaterial == null)
            return;

        fogMaterial.SetTexture("_TorchMask", torchMask);
        fogMaterial.SetTexture("_PlayerMask", playerMask);
        fogMaterial.SetVector("_MapOrigin",
            new Vector4(mapOrigin.x, mapOrigin.y, mapOrigin.z, 0f));
        fogMaterial.SetVector("_MapSize",
            new Vector4(mapSize.x, mapSize.y, 0f, 0f));
        fogMaterial.SetFloat("_FogAlpha", fogAlpha);
    }

    void UpdatePlayerMask()
    {
        if (playerMask == null || playerPixels == null || player == null)
            return;

        ClearPixels(playerPixels);

        Vector3 p = player.position;
        DrawCircle(playerPixels, p, playerRadius);
        DrawCone(playerPixels, p, GetForwardOnXZ(), flashlightRange,
            flashlightAngle);

        playerMask.SetPixels32(playerPixels);
        playerMask.Apply(false, false);
    }

    Vector3 GetForwardOnXZ()
    {
        Transform source = lookSource != null ? lookSource : player;

        if (source == null)
            return Vector3.forward;

        Vector3 dir = source.forward;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector3.forward;

        return dir.normalized;
    }

    void DrawCircle(Color32[] pixels, Vector3 worldPos, float radiusWorld)
    {
        WorldToPixel(worldPos, out int cx, out int cy);

        int radiusX = Mathf.CeilToInt(radiusWorld / mapSize.x * textureWidth);
        int radiusY = Mathf.CeilToInt(radiusWorld / mapSize.y * textureHeight);

        int minX = Mathf.Max(0, cx - radiusX);
        int maxX = Mathf.Min(textureWidth - 1, cx + radiusX);
        int minY = Mathf.Max(0, cy - radiusY);
        int maxY = Mathf.Min(textureHeight - 1, cy + radiusY);

        float r2 = radiusWorld * radiusWorld;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector3 wp = PixelToWorld(x, y);
                Vector2 d = new Vector2(wp.x - worldPos.x, wp.z - worldPos.z);

                if (d.sqrMagnitude <= r2)
                {
                    pixels[y * textureWidth + x] = new Color32(255, 255, 255,
                        255);
                }
            }
        }
    }

    void DrawCone(Color32[] pixels, Vector3 worldPos, Vector3 forward,
        float rangeWorld, float angleDeg)
    {
        WorldToPixel(worldPos, out int cx, out int cy);

        int radiusX = Mathf.CeilToInt(rangeWorld / mapSize.x * textureWidth);
        int radiusY = Mathf.CeilToInt(rangeWorld / mapSize.y * textureHeight);

        int minX = Mathf.Max(0, cx - radiusX);
        int maxX = Mathf.Min(textureWidth - 1, cx + radiusX);
        int minY = Mathf.Max(0, cy - radiusY);
        int maxY = Mathf.Min(textureHeight - 1, cy + radiusY);

        float cosHalf = Mathf.Cos(angleDeg * 0.5f * Mathf.Deg2Rad);
        float r2 = rangeWorld * rangeWorld;
        Vector2 fwd = new Vector2(forward.x, forward.z).normalized;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector3 wp = PixelToWorld(x, y);
                Vector2 dir = new Vector2(wp.x - worldPos.x, wp.z - worldPos.z);
                float dist2 = dir.sqrMagnitude;

                if (dist2 > r2)
                    continue;

                if (dist2 < 0.0001f)
                {
                    pixels[y * textureWidth + x] = new Color32(255, 255, 255,
                        255);
                    continue;
                }

                dir.Normalize();

                if (Vector2.Dot(fwd, dir) >= cosHalf)
                {
                    pixels[y * textureWidth + x] = new Color32(255, 255, 255,
                        255);
                }
            }
        }
    }

    void WorldToPixel(Vector3 worldPos, out int px, out int py)
    {
        float u = Mathf.InverseLerp(mapOrigin.x, mapOrigin.x + mapSize.x,
            worldPos.x);
        float v = Mathf.InverseLerp(mapOrigin.z, mapOrigin.z + mapSize.y,
            worldPos.z);

        px = Mathf.Clamp(Mathf.RoundToInt(u * (textureWidth - 1)), 0,
            textureWidth - 1);
        py = Mathf.Clamp(Mathf.RoundToInt(v * (textureHeight - 1)), 0,
            textureHeight - 1);
    }

    Vector3 PixelToWorld(int px, int py)
    {
        float u = px / (float)(textureWidth - 1);
        float v = py / (float)(textureHeight - 1);

        float x = Mathf.Lerp(mapOrigin.x, mapOrigin.x + mapSize.x, u);
        float z = Mathf.Lerp(mapOrigin.z, mapOrigin.z + mapSize.y, v);

        return new Vector3(x, 0f, z);
    }

    void ClearPixels(Color32[] pixels)
    {
        Color32 c = new Color32(0, 0, 0, 255);

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = c;
    }

    public void SetTopDownFogVisible(bool value)
    {
        if (fogRenderer != null)
            fogRenderer.enabled = value;
    }
}