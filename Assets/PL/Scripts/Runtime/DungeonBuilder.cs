using UnityEngine;
using Unity.AI.Navigation;

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

    //all prefabs to put in the map
    public GameObject floorPrefab; //1
    public GameObject wallVertical; //2
    public GameObject wallHorizontal; //3
    public GameObject wallCorner; //4
    public GameObject doorHorizontal; //5
    public GameObject doorVertical; //6
    public GameObject decorationPrefab; //7
    public GameObject libraryPrefab; //8

    public GameObject wallVerticalTorch; //no specific number because of proba to exchange with a 2
    public GameObject wallHorizontalTorch; //no specific number because of proba to exchange with a 3

    //proba of spawning torches
    [Range(0, 100)] public int torchChanceVertical = 20;
    [Range(0, 100)] public int torchChanceHorizontal = 20;

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
                Debug.Log("NavMesh built, size: " + width + "x" + height); //testing

                MeshRenderer rend = navMeshCube.GetComponent<MeshRenderer>(); //so cube doesnt appear on top of the floor tiles
                if (rend != null)
                    rend.enabled = false;
            }
            else
            {
                Debug.LogWarning("NavMeshSurface Null"); //testing
            }
        }
    }

    void Build(int[,] map) //for each tile (number in the matrix map) we spawn the coresponding game object in the 3D map
    {
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
                if (t == 1)
                {
                    pf = floorPrefab;
                }
                else if (t == 2)
                {
                    if (wallVerticalTorch != null && torchChanceVertical > 0 && Random.Range(0, 100) < torchChanceVertical) //might become wall with lights
                        pf = wallVerticalTorch;
                    else
                        pf = wallVertical;
                }
                else if (t == 3)
                {
                    if (wallHorizontalTorch != null && torchChanceHorizontal > 0 && Random.Range(0, 100) < torchChanceHorizontal) //same
                        pf = wallHorizontalTorch;
                    else
                        pf = wallHorizontal;
                }
                else if (t == 4)
                {
                    pf = wallCorner;
                }
                else if (t == 5)
                {
                    pf = doorHorizontal;
                }
                else if (t == 6)
                {
                    pf = doorVertical;
                }
                else if (t == 7)
                {
                    pf = decorationPrefab;
                }
                else if (t == 8)
                {
                    pf = libraryPrefab;
                }
                if (pf != null)
                {
                    Instantiate(pf, new Vector3(x, 0f, y), Quaternion.identity, transform); //spawn chosen prefab at coordinate x,0,y
                }
                x++;
            }
            y++;
        }
    }
}
