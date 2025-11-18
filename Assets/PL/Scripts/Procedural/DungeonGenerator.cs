using System;
using System.Collections.Generic;
#pragma warning disable UDR0001

public static class DungeonGenerator
{
    const int EMPTY = 0;
    const int FLOOR = 1;
    const int WALL_V = 2; //vertical wall
    const int WALL_H = 3; //horizontal
    const int CORNER = 4;
    const int DOOR_H = 5; //horizontal door
    const int DOOR_V = 6; //vertical

    static int col;
    static int row;

    public static int[,] GenerateMatrix(int width, int height, int seed, int probaDiv, int maxRooms, int minSplitSize, int minRoomSize)
        /*uses all our functions to create a matrix of int with rooms and corridors and specific numbers for floors, walls, doors ect*/
    {
        col = width;
        row = height;

        int[,] map = new int[row, col];
        int y = 0;
        while (y < row)
        {
            int x = 0;
            while (x < col)
            {
                map[y, x] = EMPTY;
                x++;
            }
            y++;
        }

        Queue<Room> rooms = GenerateSubDivisions(probaDiv, maxRooms, minSplitSize, minRoomSize, seed);
        List<Node> nodes = GenerateNodes(rooms);
        DrawRooms(map, nodes);
        GenerateCorridors(nodes, map);

        PlaceWalls(map); //not very opti to do three "place" functions, if i have time ill merge into one to only parcour the matrix once
        PlaceDoors(map); //before i forget, some stuff has to be done before other, doors need to be created before decorations so idk if possible to merge the functions
        PlaceDecorations(map);
        PlaceEntities(map, nodes, 10); //number of ennemy we want

        return map;
    }

    static Queue<Room> GenerateSubDivisions(int probaDiv, int maxRooms, int minSplitSize, int minRoomSize, int seed)
    {
        /*puts the map dimensions in a queue, then we loop doing unqueue, what we get we either split in two and put those parts 
         * in the queue again (depends on max size, min size and a probability number) or we take it and generate a room within the dimensions of the subdivided space.
         We return the list of all the rooms created for the map.*/
        Queue<Room> rooms = new Queue<Room>();
        Queue<SubDivision> subDivisions = new Queue<SubDivision>();
        Random rand;

        if (seed == 0)
        {
            rand = new Random();
        }
        else
        {
            rand = new Random(seed);
        }

        subDivisions.Enqueue(new SubDivision(1, 1, col - 1, row - 1));
        int numRoom = 0;

        while (subDivisions.Count > 0 && numRoom < maxRooms)
        {
            SubDivision sub = subDivisions.Dequeue();
            if (sub.Width < minSplitSize && sub.Height < minSplitSize)
            {
                int minW = Math.Max(1, Math.Min(minRoomSize, sub.Width));
                int minH = Math.Max(1, Math.Min(minRoomSize, sub.Height));
                int rw = rand.Next(minW, sub.Width + 1);
                int rh = rand.Next(minH, sub.Height + 1);
                rooms.Enqueue(new Room(sub.startX, sub.startY, rw, rh));
                numRoom++;
            }
            else
            {
                if (sub.Width < 20 && sub.Height < 20 && rand.Next(0, 100) > probaDiv)
                {
                    int rw2 = rand.Next(Math.Max(1, sub.Width - 2), sub.Width + 1);
                    int rh2 = rand.Next(Math.Max(1, sub.Height - 2), sub.Height + 1);
                    rooms.Enqueue(new Room(sub.startX, sub.startY, rw2, rh2));
                    numRoom++;
                }
                else
                {
                    if (sub.Width > sub.Height)
                    {
                        int cut = sub.startX + sub.Width / 2;
                        subDivisions.Enqueue(new SubDivision(sub.startX, sub.startY, cut, sub.endY));
                        subDivisions.Enqueue(new SubDivision(cut + 1, sub.startY, sub.endX, sub.endY));
                    }
                    else
                    {
                        int cut = sub.startY + sub.Height / 2;
                        subDivisions.Enqueue(new SubDivision(sub.startX, sub.startY, sub.endX, cut));
                        subDivisions.Enqueue(new SubDivision(sub.startX, cut + 1, sub.endX, sub.endY));
                    }
                }
            }
        }
        return rooms;
    }

    static List<Node> GenerateNodes(Queue<Room> rooms)
        /*generates the list of nodes from the queue rooms (with ajacent rooms) so we can use it in our graph algorithme*/
    {
        List<Node> list = new List<Node>();
        while (rooms.Count > 0) list.Add(new Node(rooms.Dequeue()));

        int i = 0;
        while (i < list.Count)
        {
            int x = list[i].room.xPos;
            int y = list[i].room.yPos;

            int ind1 = -1;
            int ind2 = -1;
            int ind3 = -1;

            int j = 0;
            while (j < list.Count)
            {
                if (i != j)
                {
                    int dx = x - list[j].room.xPos;
                    int dy = y - list[j].room.yPos;
                    int d = dx * dx + dy * dy;

                    if (ind1 == -1)
                    {
                        ind1 = j;
                    }
                    else
                    {
                        int dx1 = x - list[ind1].room.xPos;
                        int dy1 = y - list[ind1].room.yPos;
                        int d1 = dx1 * dx1 + dy1 * dy1;
                        if (d < d1)
                        {
                            ind3 = ind2;
                            ind2 = ind1;
                            ind1 = j;
                        }
                        else
                        {
                            if (ind2 == -1)
                            {
                                ind2 = j;
                            }
                            else
                            {
                                int dx2 = x - list[ind2].room.xPos;
                                int dy2 = y - list[ind2].room.yPos;
                                int d2 = dx2 * dx2 + dy2 * dy2;

                                if (d < d2)
                                {
                                    ind3 = ind2;
                                    ind2 = j;
                                }
                                else
                                {
                                    if (ind3 == -1)
                                    {
                                        ind3 = j;
                                    }
                                    else
                                    {
                                        int dx3 = x - list[ind3].room.xPos;
                                        int dy3 = y - list[ind3].room.yPos;
                                        int d3 = dx3 * dx3 + dy3 * dy3;
                                        if (d < d3) ind3 = j;
                                    }
                                }
                            }
                        }
                    }
                }
                j++;
            }

            if (ind1 != -1)
                list[i].adjacentNodes.Add(list[ind1]);
            if (ind2 != -1)
                list[i].adjacentNodes.Add(list[ind2]);
            if (ind3 != -1)
                list[i].adjacentNodes.Add(list[ind3]);

            i++;
        }

        return list;
    }

    static void DrawRooms(int[,] map, List<Node> nodes)
        /*just lace floors inside the room dimensions*/
    {
        int k = 0;
        while (k < nodes.Count)
        {
            if (nodes[k].room != null)
            {
                Room room = nodes[k].room;
                int j = room.yPos;
                while (j < room.yPos + room.ySize)
                {
                    if (j > 0 && j < row - 1)
                    {
                        int i = room.xPos;
                        while (i < room.xPos + room.xSize)
                        {
                            if (i > 0 && i < col - 1) map[j, i] = FLOOR;
                            i++;
                        }
                    }
                    j++;
                }
            }
            k++;
        }
    }

    static void DrawCorridor(Room r1, Room r2, int[,] map)
        /*places floor tiles to link the two rooms centers*/
    {
        int x1 = r1.xPos + r1.xSize / 2;
        int y1 = r1.yPos + r1.ySize / 2;
        int x2 = r2.xPos + r2.xSize / 2;
        int y2 = r2.yPos + r2.ySize / 2;

        if (x1 < 1) x1 = 1;
        if (x1 > col - 2) x1 = col - 2;
        if (y1 < 1) y1 = 1;
        if (y1 > row - 2) y1 = row - 2;

        if (x2 < 1) x2 = 1;
        if (x2 > col - 2) x2 = col - 2;
        if (y2 < 1) y2 = 1;
        if (y2 > row - 2) y2 = row - 2;

        int x = x1;
        int y = y1;

        if (x <= x2)
        {
            while (x <= x2)
            {
                if (y > 0 && y < row - 1 && x > 0 && x < col - 1) map[y, x] = FLOOR;
                x++;
            }
        }
        else
        {
            while (x >= x2)
            {
                if (y > 0 && y < row - 1 && x > 0 && x < col - 1) map[y, x] = FLOOR;
                x--;
            }
        }

        x = x2;
        y = y1;

        if (y <= y2)
        {
            while (y <= y2)
            {
                if (y > 0 && y < row - 1 && x > 0 && x < col - 1) map[y, x] = FLOOR;
                y++;
            }
        }
        else
        {
            while (y >= y2)
            {
                if (y > 0 && y < row - 1 && x > 0 && x < col - 1) map[y, x] = FLOOR;
                y--;
            }
        }
    }

    static void GenerateCorridors(List<Node> nodes, int[,] map)
        /*Take first node, trace a corridor from its center to the height or depth of the 1 or 2 randmly selected room 
         * from the adjacent rooms and then to the depth or height of the center of those rooms and check those rooms 
         * as connected to the main network, then put those rooms in a queue, we do the same for each rooms 
         * of the queue until queue is empty, we then check all the rooms to see if they are connected to the main network, if not we connect them to a room that is connected.*/
    {
        if (nodes.Count == 0) return;

        Queue<Node> visited = new Queue<Node>();
        Random rand = new Random();
        int probaConnect = 30;

        int z = 0;
        while (z < nodes.Count)
        {
            nodes[z].connected = false; z++;
        }

        Node n = nodes[0];
        n.connected = true;
        visited.Enqueue(n);

        while (visited.Count > 0)
        {
            n = visited.Dequeue();
            int i = 0;
            while (i < n.adjacentNodes.Count)
            {
                if (!n.connected)
                {
                    if (n.adjacentNodes[i].connected)
                    {
                        DrawCorridor(n.room, n.adjacentNodes[i].room, map);
                        n.connected = true;
                        break;
                    }
                }
                else
                {
                    if (n.adjacentNodes[i].connected)
                    {
                        if (probaConnect > rand.Next(0, 100))
                        {
                            DrawCorridor(n.room, n.adjacentNodes[i].room, map);
                            n.connected = true;
                        }
                    }
                    else
                    {
                        DrawCorridor(n.room, n.adjacentNodes[i].room, map);
                        n.connected = true;
                        n.adjacentNodes[i].connected = true;
                        visited.Enqueue(n.adjacentNodes[i]);
                    }
                }
                i++;
            }

            if (!n.connected)
            {
                int k = 0;
                while (k < nodes.Count)
                {
                    if (nodes[k].connected)
                    {
                        n.connected = true;
                        DrawCorridor(n.room, nodes[k].room, map);
                        break;
                    }
                    k++;
                }
            }

            if (visited.Count == 0)
            {
                int u = 0;
                bool pushed = false;
                while (u < nodes.Count)
                {
                    if (!nodes[u].connected)
                    {
                        visited.Enqueue(nodes[u]);
                        pushed = true;
                        break;
                    }
                    u++;
                }
                if (!pushed) break;
            }
        }
    }

    static void PlaceWalls(int[,] map)
    {
        int h = map.GetLength(0);
        int w = map.GetLength(1);

        int y = 0;
        while (y < h)
        {
            int x = 0;
            while (x < w)
            {
                if (map[y, x] == EMPTY)
                {
                    bool nF = y - 1 >= 0 && map[y - 1, x] == FLOOR; //check if North tile is floor
                    bool eF = x + 1 < w && map[y, x + 1] == FLOOR; //same with East
                    bool sF = y + 1 < h && map[y + 1, x] == FLOOR; //South
                    bool wF = x - 1 >= 0 && map[y, x - 1] == FLOOR; //West

                    bool neF = y - 1 >= 0 && x + 1 < w && map[y - 1, x + 1] == FLOOR; //North East
                    bool seF = y + 1 < h && x + 1 < w && map[y + 1, x + 1] == FLOOR; //South East
                    bool swF = y + 1 < h && x - 1 >= 0 && map[y + 1, x - 1] == FLOOR; //South West
                    bool nwF = y - 1 >= 0 && x - 1 >= 0 && map[y - 1, x - 1] == FLOOR; //North West

                    if (nF || eF || sF || wF || neF || seF || swF || nwF)
                    {
                        bool cornerConvex = (nF || sF) && (eF || wF);
                        bool cornerConcave = (neF && !nF && !eF) || (seF && !sF && !eF) || (swF && !sF && !wF) || (nwF && !nF && !wF);

                        if (cornerConvex || cornerConcave) //all corners will spawn same prefab for now
                        {
                            map[y, x] = CORNER;
                        }
                        else
                        {
                            if (eF || wF) map[y, x] = WALL_V;
                            else if (nF || sF) map[y, x] = WALL_H;
                        }
                    }
                }
                x++;
            }
            y++;
        }
    }

    static void PlaceDoors(int[,] map) 
        /*put doors when a floor tile has 2 floors on opposing sides and only 2 diagonal walls
        (so we dont make doors everywhere in corridors) */
    {
        int h = map.GetLength(0);
        int w = map.GetLength(1);

        int y = 1;
        while (y < h - 1)
        {
            int x = 1;
            while (x < w - 1)
            {
                if (map[y, x] == FLOOR)
                {
                    bool nF = map[y - 1, x] == FLOOR; //names are same as in PlaceWalls function just scroll a few lines up
                    bool sF = map[y + 1, x] == FLOOR;
                    bool eF = map[y, x + 1] == FLOOR;
                    bool wF = map[y, x - 1] == FLOOR;

                    bool nW = map[y - 1, x] >= WALL_V && map[y - 1, x] <= CORNER; //same names but for walls
                    bool sW = map[y + 1, x] >= WALL_V && map[y + 1, x] <= CORNER;
                    bool eW = map[y, x + 1] >= WALL_V && map[y, x + 1] <= CORNER;
                    bool wW = map[y, x - 1] >= WALL_V && map[y, x - 1] <= CORNER;

                    int diagCount = 0;
                    if (map[y - 1, x + 1] == FLOOR) diagCount++;
                    if (map[y - 1, x - 1] == FLOOR) diagCount++;
                    if (map[y + 1, x + 1] == FLOOR) diagCount++;
                    if (map[y + 1, x - 1] == FLOOR) diagCount++;

                    if (nF && sF && eW && wW && diagCount >= 2)
                    {
                        map[y, x] = DOOR_H;
                    }
                    else if (eF && wF && nW && sW && diagCount >= 2)
                    {
                        map[y, x] = DOOR_V;
                    }
                }
                x++;
            }
            y++;
        }
    }

    static void PlaceDecorations(int[,] map, int probaDecoration = 10, int probaLibrary = 15) 
        /*if a floor tile has max one neighboor tile that isnt a floor it can becore deco
        if a floor tile has only one neighboor that ist floor and it is a deco, then it can become library with that neighboor*/
    {
        int h = map.GetLength(0);
        int w = map.GetLength(1);
        Random rand = new Random();

        int FLOOR = 1;
        int DOOR_H = 5;
        int DOOR_V = 6;

        for (int y = 1; y < h - 1; y++)
        {
            for (int x = 1; x < w - 1; x++)
            {
                if (map[y, x] == FLOOR)
                {
                    bool nearDoor = false;
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if (map[y + dy, x + dx] == DOOR_H || map[y + dy, x + dx] == DOOR_V)
                            {
                                nearDoor = true;
                            }
                        }
                    }
                    if (nearDoor) continue;

                    int floorCount = 0;
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if (map[y + dy, x + dx] == FLOOR)
                            {
                                floorCount++;
                            }
                        }
                    }

                    if (floorCount >= 5 && rand.Next(0, 100) < probaDecoration)
                    {
                        map[y, x] = 7;

                        if (rand.Next(0, 100) < probaLibrary)
                        {
                            if (map[y, x + 1] == FLOOR)
                            {
                                map[y, x] = 8;
                                map[y, x + 1] = 8;
                            }
                            else if (map[y + 1, x] == FLOOR)
                            {
                                map[y, x] = 8;
                                map[y + 1, x] = 8;
                            }
                        }
                    }
                }
            }
        }
    }

    static void PlaceEntities(int[,] map, List<Node> nodes, int enemyCount = 10)
    {
        if (nodes == null || nodes.Count == 0) return;

        Random rand = new Random();
        Node startNode = nodes[rand.Next(0, nodes.Count)];
        Room playerRoom = startNode.room; //random room to spawn player in

        int px = playerRoom.xPos + playerRoom.xSize / 2;
        int py = playerRoom.yPos + playerRoom.ySize / 2;
        map[py, px] = 999; //player number

        int enemiesPlaced = 0;
        while (enemiesPlaced < enemyCount)
        {
            Node n = nodes[rand.Next(0, nodes.Count)];
            if (n == startNode) continue; //no ennemies in player spawn room
            Room r = n.room;

            int ex = rand.Next(r.xPos + 1, r.xPos + r.xSize - 1);
            int ey = rand.Next(r.yPos + 1, r.yPos + r.ySize - 1);

            if (map[ey, ex] == 1)
            {
                map[ey, ex] = 88; //ennemy number
                enemiesPlaced++;
            }
        }

        Node ladderNode = null;
        while (ladderNode == null)
        {
            Node n = nodes[rand.Next(0, nodes.Count)];
            if (n != startNode)
                ladderNode = n;
        }

        Room ladderRoom = ladderNode.room;
        int lx = ladderRoom.xPos + ladderRoom.xSize / 2;
        int ly = ladderRoom.yPos + ladderRoom.ySize / 2;
        map[ly, lx] = 777; //escape number
    }

}
