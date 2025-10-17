using UnityEngine;
using Unity.AI.Navigation;

public class DungeonBuilder : MonoBehaviour
{
    //all accessible parameters for map gen
    public int width = 80;
    public int height = 40;
    public int seed = 12345;
    public int probaDiv = 85;
    public int maxRooms = 25;
    public int minSplitSize = 5;
    public int minRoomClamp = 3;

    public GameObject floorPrefab;
    public GameObject wallVertical;
    public GameObject wallHorizontal;
    public GameObject wallCorner;
    public GameObject doorHorizontal;
    public GameObject doorVertical;
    public GameObject wallVerticalTorch;
    public GameObject wallHorizontalTorch;

    [Range(0, 100)] public int torchChanceVertical = 20;
    [Range(0, 100)] public int torchChanceHorizontal = 20;

    public GameObject navMeshCube;
    private NavMeshSurface navMeshSurface;

    void Start()
    {
        int[,] map = DungeonGenerator.GenerateInt(width, height, seed, probaDiv, maxRooms, minSplitSize, minRoomClamp); //generate the matrix map
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
                Debug.Log("NavMesh built, size: " + width + "x" + height);
            }
            else
            {
                Debug.LogWarning("NavMeshSurface Null");
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
                if (pf != null)
                {
                    Instantiate(pf, new Vector3(x, 0f, y), Quaternion.identity, transform); //spawn prefab at coordinate x,y
                }
                x++;
            }
            y++;
        }
    }
}
