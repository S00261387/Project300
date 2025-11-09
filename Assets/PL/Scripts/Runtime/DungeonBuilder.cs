using UnityEngine;
using Unity.AI.Navigation;
using System.Collections.Generic;

public class DungeonBuilder : MonoBehaviour
{
    //all accessible parameters for map gen
    public int width = 80;
    public int height = 40;
    public int seed = 0; //for generation (0 is random, other numbers generate specfic maps)
    public int probaDiv = 85; //proba of dividing a subdivision again or selecting it to create a room
    public int maxRooms = 25; //max amount of rooms in the map
    public int minSplitSize = 5; //min size of subdivision
    public int minRoomSize = 3; //min size of a room

    static Vector2Int playerSpawn = Vector2Int.zero; //spawnPoint
    public GameObject Player; //999
    public GameObject EnemyPrefab; //88


    //all prefabs to put in the map
    public GameObject floorPrefab; //1
    public GameObject wallPrefab; //2 for vertical and 3 for horizontal
    public GameObject wallCorner; //4
    public GameObject doorPrefab; //5 vertical and 6 horizontal
    public GameObject decorationPrefab; //7
    public GameObject libraryPrefab; //8
    public GameObject wallTorch; //no specific number because of proba to exchange with a 2 or 3

    public GameObject wallHospital; //to replace the basic dungeon prefabs
    public GameObject wallTorchHospital;

    public GameObject EscapePrefab; //777

    //proba of spawning torches
    [Range(0, 100)] public int torchChance = 5;
    [Range(0f, 100f)] public float sanity = 0f; //for testing, I have to move that in a player controller when we merge branches

    private List<GameObject> normalWalls = new List<GameObject>(); //to know all the walls i have
    private List<GameObject> torchWalls = new List<GameObject>(); //same with light walls
    private bool hospitalWallsActive = false; //to make things appear and disappear
    private bool hospitalTorchsActive = false; //same

    //NavMesh management
    public GameObject navMeshCube;
    private NavMeshSurface navMeshSurface;

    void Start()
    {
        int[,] map = DungeonGenerator.GenerateMatrix(width, height, seed, probaDiv, maxRooms, minSplitSize, minRoomSize); //generate the matrix map
        if (map != null)
        {
            Build(map); //spawns all the prefabs of walls, floors ect
        }
        if (navMeshCube != null)
        {
            Vector3 newSize = navMeshCube.transform.localScale;
            newSize.x = width;
            newSize.z = height;
            navMeshCube.transform.localScale = newSize;
            Vector3 newPos = navMeshCube.transform.position;
            newPos.x = width / 2f; //middle of map is at width / 2
            newPos.z = height / 2f; //same
            navMeshCube.transform.position = newPos;
            navMeshSurface = navMeshCube.GetComponent<NavMeshSurface>();
            if (navMeshSurface != null)
            {
                navMeshSurface.BuildNavMesh();

                MeshRenderer rend = navMeshCube.GetComponent<MeshRenderer>(); //so cube doesnt appear on top of the floor tiles
                if (rend != null)
                    rend.enabled = false;
            }
            else
            {
                Debug.LogWarning("NavMeshSurface Null"); //testing
            }
        }
        PlacePlayer(playerSpawn); //tp player to spawn
    }
    void Update()
    {
        UpdateMapWithSanity();
    }

    void Build(int[,] map) //for each tile (number in the matrix map) we spawn the coresponding game object in the 3D map
    {
        //bool PlayerSpawned = false;
        //need to create a random to spawn player?____________________________________________________________________________________________________________________still in progress
        int h = map.GetLength(0);
        int w = map.GetLength(1);
        int y = 0;
        while (y < h)
        {
            int x = 0;
            while (x < w)
            {
                int t = map[y, x]; //current tile
                GameObject pf = null; //the choosen prefab to spawn at the end of the loop
                Quaternion rot = Quaternion.identity; //for rotation of prefabs
                if (t == 1)
                {
                    pf = floorPrefab;
                }
                else if (t == 2)
                {
                    rot = Quaternion.Euler(0, 90, 0);
                    if (wallTorch != null && Random.Range(0, 100) < torchChance)
                        pf = wallTorch;
                    else
                        pf = wallPrefab;
                }
                else if (t == 3)
                {
                    rot = Quaternion.identity;
                    if (wallTorch != null && Random.Range(0, 100) < torchChance)
                        pf = wallTorch;
                    else
                        pf = wallPrefab;
                }
                else if (t == 4)
                {
                    pf = wallCorner;
                }
                else if (t == 5)
                {

                    rot = Quaternion.Euler(0, 90, 0);
                    pf = doorPrefab;
                }
                else if (t == 6)
                {

                    rot = Quaternion.identity;
                    pf = doorPrefab;
                }
                else if (t == 7)
                {
                    pf = decorationPrefab;
                }
                else if (t == 8)
                {
                    pf = libraryPrefab;
                }
                else if (t == 999 || t == 88 || t == 777)
                {
                    pf = floorPrefab; //so entities dont fall in the void when spawned lol

                    if (t == 999)
                        playerSpawn = new Vector2Int(x, y); //we will teleport the player there, not instanciate it
                    else if (t == 88)
                        Instantiate(EnemyPrefab, new Vector3(x, 0f, y), Quaternion.identity);
                    else if (t == 777)
                        Instantiate(EscapePrefab, new Vector3(x, 0f, y), Quaternion.identity);
                }
                if (pf != null)
                {
                    GameObject obj = Instantiate(pf, new Vector3(x, 0f, y), rot, transform); //spawn chosen prefab at coordinate x,0,y with rotation for vertical or horizontal
                    if (pf == wallPrefab)
                        normalWalls.Add(obj);
                    else if (pf == wallTorch)
                        torchWalls.Add(obj);
                }
                x++;
            }
            y++;
        }
    }

    void SwapPrefabs(List<GameObject> list, GameObject newPrefab)
    {
        for (int i = 0; i < list.Count; i++)
        {
            GameObject old = list[i];
            if (old == null) continue;

            Vector3 pos = old.transform.position;
            Quaternion rot = old.transform.rotation;
            Transform parent = old.transform.parent;

            GameObject newObj = Instantiate(newPrefab, pos, rot, parent);
            Destroy(old);
            list[i] = newObj;
        }
    }

    void UpdateMapWithSanity()
    {
        if (sanity > 50 && !hospitalWallsActive)
        {
            SwapPrefabs(normalWalls, wallHospital);
            hospitalWallsActive = true;
        }
        else if (sanity <= 50 && hospitalWallsActive)
        {
            SwapPrefabs(normalWalls, wallPrefab);
            hospitalWallsActive = false;
        }

        if (sanity > 70 && !hospitalTorchsActive)
        {
            SwapPrefabs(torchWalls, wallTorchHospital);
            hospitalTorchsActive = true;
        }
        else if (sanity <= 70 && hospitalTorchsActive)
        {
            SwapPrefabs(torchWalls, wallTorch);
            hospitalTorchsActive = false;
        }
    }
    void PlacePlayer(Vector2Int spawnTile) //teleport player to spawn position
    {
        if (Player == null)
        {
            Debug.LogWarning("Player is null");
            return;
        }

        Vector3 pos = new Vector3(spawnTile.x + 0.5f, 0f, spawnTile.y + 0.5f);
        Player.transform.position = pos;
    }
}
