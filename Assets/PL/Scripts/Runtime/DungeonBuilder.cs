using UnityEngine;

public class DungeonBuilder : MonoBehaviour
{
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

    void Start()
    {
        int[,] map = DungeonGenerator.GenerateInt(width, height, seed, probaDiv, maxRooms, minSplitSize, minRoomClamp);
        Build(map);
    }

    void Build(int[,] map)
    {
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

                if (t == 1)
                {
                    pf = floorPrefab;
                }
                else if (t == 2)
                {
                    if (wallVerticalTorch != null && torchChanceVertical > 0 && Random.Range(0, 100) < torchChanceVertical)
                        pf = wallVerticalTorch;
                    else
                        pf = wallVertical;
                }
                else if (t == 3)
                {
                    if (wallHorizontalTorch != null && torchChanceHorizontal > 0 && Random.Range(0, 100) < torchChanceHorizontal)
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
                    Instantiate(pf, new Vector3(x, 0f, y), Quaternion.identity, transform);
                }

                x++;
            }
            y++;
        }
    }
}
