using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
#pragma warning disable UDR0001

public class DungeonBuilder : MonoBehaviour
{
    public static DungeonBuilder Instance;

    public int width = 80;
    public int height = 40;
    public int seed = 0;
    public int probaDiv = 85;
    public int maxRooms = 25;
    public int minSplitSize = 5;
    public int minRoomSize = 3;

    private Vector2Int playerSpawn = new Vector2Int(-1, -1);
    public GameObject Player;
    public GameObject EnemyPrefab;
    public GameObject StalkerPrefab;

    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject wallTorch;
    public GameObject wallCorner;
    public GameObject doorPrefab;

    [Header("Decorations")]
    public List<GameObject> decorationPrefabs;
    public GameObject libraryPrefab;

    public GameObject wallHospital;
    public GameObject wallTorchHospital;

    public GameObject EscapePrefab;

    [Range(0, 100)] public int torchChance = 5;

    private List<GameObject> normalWalls = new List<GameObject>();
    private List<GameObject> torchWalls = new List<GameObject>();

    public GameObject navMeshCube;
    private NavMeshSurface navMeshSurface;

    private List<Vector3> enemySpawnPoints = new List<Vector3>();
    private List<Vector3> escapeSpawnPoints = new List<Vector3>();
    private List<Vector3> stalkerTeleportPoints = new List<Vector3>();

    private GameObject currentStalker;
    private GameObject currentPatrolEnemy;

    private int[,] map;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (Player == null)
            Player = GameObject.FindWithTag("Player");

        map = DungeonGenerator.GenerateMatrix(width, height, seed, probaDiv,
            maxRooms, minSplitSize, minRoomSize);

        if (map != null)
            Build(map);

        if (navMeshCube != null)
        {
            Vector3 newSize = navMeshCube.transform.localScale;
            newSize.x = width;
            newSize.z = height;
            navMeshCube.transform.localScale = newSize;

            Vector3 newPos = navMeshCube.transform.position;
            newPos.x = width / 2f;
            newPos.z = height / 2f;
            navMeshCube.transform.position = newPos;

            navMeshSurface = navMeshCube.GetComponent<NavMeshSurface>();

            if (navMeshSurface != null)
                StartCoroutine(BuildNavMeshAndSpawn());
            else
                Debug.LogWarning("NavMeshSurface Null");
        }

        StartCoroutine(PlacePlayerAndRefreshFogNextFrame());
    }

    void Update() { }

    IEnumerator BuildNavMeshAndSpawn()
    {
        yield return null;

        navMeshSurface.BuildNavMesh();

        MeshRenderer rend = navMeshCube.GetComponent<MeshRenderer>();
        if (rend != null)
            rend.enabled = false;

        SpawnPatrolEnemy();
        SpawnEscapeDoors();
        SpawnStalker();
    }

    void Build(int[,] map)
    {
        bool isHospitalTheme = false;

        if (SanityManager.Instance != null)
            isHospitalTheme = SanityManager.Instance.sanity > 50f;

        normalWalls.Clear();
        torchWalls.Clear();
        enemySpawnPoints.Clear();
        escapeSpawnPoints.Clear();
        stalkerTeleportPoints.Clear();
        playerSpawn = new Vector2Int(-1, -1);

        int h = map.GetLength(0);
        int w = map.GetLength(1);

        int y = 0;
        while (y < h)
        {
            int x = 0;
            while (x < w)
            {
                int t = map[y, x];

                GameObject pf = null;
                Quaternion rot = Quaternion.identity;

                if (t == 1)
                {
                    pf = floorPrefab;
                    stalkerTeleportPoints.Add(new Vector3(x + 0.5f, 0f,
                        y + 0.5f));
                }
                else if (t == 2)
                {
                    rot = Quaternion.Euler(0, 90, 0);

                    if (wallTorch != null && Random.Range(0, 100) < torchChance)
                        pf = isHospitalTheme ? wallTorchHospital : wallTorch;
                    else
                        pf = isHospitalTheme ? wallHospital : wallPrefab;
                }
                else if (t == 3)
                {
                    if (wallTorch != null && Random.Range(0, 100) < torchChance)
                        pf = isHospitalTheme ? wallTorchHospital : wallTorch;
                    else
                        pf = isHospitalTheme ? wallHospital : wallPrefab;
                }
                else if (t == 4) pf = wallCorner;
                else if (t == 5)
                {
                    rot = Quaternion.Euler(0, 90, 0);
                    pf = doorPrefab;
                }
                else if (t == 6) pf = doorPrefab;
                else if (t == 7)
                {
                    if (decorationPrefabs != null && decorationPrefabs.Count > 0)
                        pf = decorationPrefabs[Random.Range(0,
                            decorationPrefabs.Count)];

                    stalkerTeleportPoints.Add(new Vector3(x + 0.5f, 0f,
                        y + 0.5f));
                }
                else if (t == 8)
                {
                    pf = libraryPrefab;
                    stalkerTeleportPoints.Add(new Vector3(x + 0.5f, 0f,
                        y + 0.5f));
                }
                else if (t == 999 || t == 88 || t == 777)
                {
                    pf = floorPrefab;
                    stalkerTeleportPoints.Add(new Vector3(x + 0.5f, 0f,
                        y + 0.5f));

                    if (t == 999)
                        playerSpawn = new Vector2Int(x, y);
                    else if (t == 88)
                        enemySpawnPoints.Add(new Vector3(x + 0.5f, 0f,
                            y + 0.5f));
                    else if (t == 777)
                        escapeSpawnPoints.Add(new Vector3(x + 0.5f, 0f,
                            y + 0.5f));
                }

                if (pf != null)
                {
                    GameObject obj = Instantiate(pf,
                        new Vector3(x, 0f, y),
                        rot,
                        transform);

                    if (pf == wallPrefab || pf == wallHospital)
                        normalWalls.Add(obj);
                    else if (pf == wallTorch || pf == wallTorchHospital)
                        torchWalls.Add(obj);
                }

                x++;
            }

            y++;
        }

        if (playerSpawn.x < 0 || playerSpawn.y < 0)
        {
            Debug.LogWarning("No player spawn found in generated map.");
            playerSpawn = new Vector2Int(width / 2, height / 2);
        }
    }

    void SpawnPatrolEnemy()
    {
        if (EnemyPrefab == null)
            return;

        if (currentPatrolEnemy != null)
            Destroy(currentPatrolEnemy);

        if (escapeSpawnPoints == null || escapeSpawnPoints.Count == 0)
            return;

        Vector3 pos = escapeSpawnPoints[0];

        currentPatrolEnemy = Instantiate(EnemyPrefab, pos,
            Quaternion.identity);

        EnemyAi ai = currentPatrolEnemy.GetComponent<EnemyAi>();

        if (ai != null)
        {
            Vector2Int tile = new Vector2Int(Mathf.FloorToInt(pos.x),
                Mathf.FloorToInt(pos.z));
            var roomTiles = GetRoomTiles(tile);
            var patrol = PickPatrolPoints(roomTiles, 3);
            ai.InitializePatrol(patrol);
        }
    }

    void SpawnEscapeDoors()
    {
        foreach (Vector3 pos in escapeSpawnPoints)
            Instantiate(EscapePrefab, pos, Quaternion.identity);
    }

    void SpawnStalker()
    {
        if (StalkerPrefab == null)
            return;

        if (stalkerTeleportPoints == null || stalkerTeleportPoints.Count == 0)
            return;

        if (currentStalker != null)
            Destroy(currentStalker);

        Vector3 spawnPos = stalkerTeleportPoints[
            Random.Range(0, stalkerTeleportPoints.Count)];

        currentStalker = Instantiate(StalkerPrefab, spawnPos,
            Quaternion.identity);

        StalkerEnemy ai = currentStalker.GetComponent<StalkerEnemy>();
        if (ai != null)
            ai.SetTeleportPoints(stalkerTeleportPoints);
    }

    void PlacePlayer(Vector2Int spawnTile)
    {
        if (Player == null)
            Player = GameObject.FindWithTag("Player");

        if (Player == null)
        {
            Debug.LogWarning("Player is null");
            return;
        }

        Vector3 pos = new Vector3(spawnTile.x + 0.5f, 0f, spawnTile.y + 0.5f);

        CharacterController cc = Player.GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = false;

        Player.transform.position = pos;

        if (cc != null)
            cc.enabled = true;
    }

    public void TeleportPlayerToCurrentSpawn()
    {
        PlacePlayer(playerSpawn);

        FogOfWarManager fog = FindFirstObjectByType<FogOfWarManager>();
        if (fog != null)
            fog.RefreshAllMasks();
    }

    public Vector3 GetCurrentSpawnWorldPosition()
    {
        return new Vector3(playerSpawn.x + 0.5f, 0f, playerSpawn.y + 0.5f);
    }

    public void GenerateNewMap()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject != navMeshCube)
                Destroy(child.gameObject);
        }

        GameObject[] oldEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in oldEnemies)
            Destroy(enemy);

        GameObject[] oldEscapeDoor =
            GameObject.FindGameObjectsWithTag("EscapeDoor");
        foreach (GameObject escapeDoor in oldEscapeDoor)
            Destroy(escapeDoor);

        if (currentStalker != null)
            Destroy(currentStalker);

        if (currentPatrolEnemy != null)
            Destroy(currentPatrolEnemy);

        StartCoroutine(RebuildNavMeshAfterCleanup());
    }

    private IEnumerator RebuildNavMeshAfterCleanup()
    {
        for (int i = 0; i < 5; i++)
            yield return null;

        NavMeshAgent[] agents = FindObjectsByType<NavMeshAgent>(
            FindObjectsSortMode.None);
        foreach (var a in agents)
            a.enabled = false;

        NavMesh.RemoveAllNavMeshData();

        map = DungeonGenerator.GenerateMatrix(width, height, seed, probaDiv,
            maxRooms, minSplitSize, minRoomSize);

        Build(map);

        if (navMeshCube != null)
        {
            Vector3 newScale = new Vector3(width, 1f, height);
            navMeshCube.transform.localScale = newScale;

            navMeshCube.transform.position =
                new Vector3(width / 2f - 0.5f, 0f, height / 2f - 0.5f);

            /*
            BoxCollider col = navMeshCube.GetComponent<BoxCollider>();

            if (col == null)
                col = navMeshCube.AddComponent<BoxCollider>();
            */

            if (navMeshSurface == null)
                navMeshSurface = navMeshCube.GetComponent<NavMeshSurface>();

            if (navMeshSurface == null)
                navMeshSurface = navMeshCube.AddComponent<NavMeshSurface>();

            navMeshSurface.collectObjects = CollectObjects.All;
            navMeshSurface.layerMask = ~0;

            yield return null;

            navMeshSurface.BuildNavMesh();

            SpawnPatrolEnemy();
            SpawnEscapeDoors();
            SpawnStalker();

            MeshRenderer rend = navMeshCube.GetComponent<MeshRenderer>();
            if (rend != null)
                rend.enabled = false;
        }
        else
        {
            Debug.LogWarning("navMesh is null");
        }

        yield return null;
        PlacePlayer(playerSpawn);
        StartCoroutine(PlacePlayerAndRefreshFogNextFrame());
    }

    IEnumerator PlacePlayerAndRefreshFogNextFrame()
    {
        yield return null;

        PlacePlayer(playerSpawn);

        FogOfWarManager fog = FindFirstObjectByType<FogOfWarManager>();
        if (fog != null)
            fog.RefreshAllMasks();
    }

    List<Vector2Int> GetRoomTiles(Vector2Int start)
    {
        List<Vector2Int> r = new List<Vector2Int>();
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        HashSet<Vector2Int> v = new HashSet<Vector2Int>();

        q.Enqueue(start);
        v.Add(start);

        while (q.Count > 0)
        {
            var t = q.Dequeue();
            r.Add(t);

            Vector2Int[] d = new Vector2Int[]
            {
                Vector2Int.right,
                Vector2Int.left,
                Vector2Int.up,
                Vector2Int.down
            };

            for (int i = 0; i < d.Length; i++)
            {
                Vector2Int n = t + d[i];

                if (n.x >= 0 && n.x < width && n.y >= 0 && n.y < height)
                {
                    int tile = map[n.y, n.x];

                    if (!v.Contains(n) &&
                       (tile == 1 || tile == 7 || tile == 8 || tile == 999 ||
                        tile == 777))
                    {
                        v.Add(n);
                        q.Enqueue(n);
                    }
                }
            }
        }

        return r;
    }

    List<Vector3> PickPatrolPoints(List<Vector2Int> tiles, int count)
    {
        List<Vector3> pts = new List<Vector3>();

        if (tiles.Count == 0) return pts;

        for (int i = 0; i < count; i++)
        {
            Vector2Int t = tiles[Random.Range(0, tiles.Count)];
            pts.Add(new Vector3(t.x + 0.5f, 0f, t.y + 0.5f));
        }

        return pts;
    }

    public List<GameObject> GetTorchWalls()
    {
        return torchWalls;
    }

    public Vector2 GetMapSize()
    {
        return new Vector2(width, height);
    }

    public Vector3 GetMapOrigin()
    {
        return Vector3.zero;
    }

    public List<Vector3> GetTeleportPoints()
    {
        return stalkerTeleportPoints;
    }
}