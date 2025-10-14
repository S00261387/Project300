using System;
using System.Collections.Generic;

public static class DungeonGenerator
{
    const int EMPTY = 0;
    const int FLOOR = 1;
    const int WALL_V = 2;
    const int WALL_H = 3;
    const int CORNER = 4;
    const int DOOR_H = 5;
    const int DOOR_V = 6;

    static int col;
    static int row;

    public static int[,] GenerateInt(int width, int height, int seed, int probaDiv, int maxRooms, int minSplitSize, int minRoomClamp)
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

        Queue<Room> rooms = GenerateSubDivisions(probaDiv, maxRooms, minSplitSize, minRoomClamp, seed);
        List<Node> nodes = GenerateNodes(rooms);
        DrawRooms(map, nodes);
        GenerateCorridors(nodes, map);
        ClassifyWalls(map);
        PlaceDoors(map);

        return map;
    }

    static Queue<Room> GenerateSubDivisions(int probaDiv, int maxRooms, int minSplitSize, int minRoomClamp, int seed)
    {
        Queue<Room> rooms = new Queue<Room>();
        Queue<SubDivision> subDivisions = new Queue<SubDivision>();
        Random rand = (seed == 0) ? new Random() : new Random(seed);

        subDivisions.Enqueue(new SubDivision(1, 1, col - 1, row - 1));
        int numRoom = 0;

        while (subDivisions.Count > 0 && numRoom < maxRooms)
        {
            SubDivision sub = subDivisions.Dequeue();

            if (sub.Width < minSplitSize && sub.Height < minSplitSize)
            {
                int minW = Math.Max(1, Math.Min(minRoomClamp, sub.Width));
                int minH = Math.Max(1, Math.Min(minRoomClamp, sub.Height));
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

            if (ind1 != -1) list[i].adjacentNodes.Add(list[ind1]);
            if (ind2 != -1) list[i].adjacentNodes.Add(list[ind2]);
            if (ind3 != -1) list[i].adjacentNodes.Add(list[ind3]);

            i++;
        }

        return list;
    }

    static void DrawRooms(int[,] map, List<Node> nodes)
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
    {
        if (nodes.Count == 0) return;

        Queue<Node> visited = new Queue<Node>();
        Random rand = new Random();
        int probaConnect = 30;

        int z = 0;
        while (z < nodes.Count) { nodes[z].connected = false; z++; }

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

    static void ClassifyWalls(int[,] map)
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
                    bool nF = y - 1 >= 0 && map[y - 1, x] == FLOOR;
                    bool eF = x + 1 < w && map[y, x + 1] == FLOOR;
                    bool sF = y + 1 < h && map[y + 1, x] == FLOOR;
                    bool wF = x - 1 >= 0 && map[y, x - 1] == FLOOR;

                    bool neF = y - 1 >= 0 && x + 1 < w && map[y - 1, x + 1] == FLOOR;
                    bool seF = y + 1 < h && x + 1 < w && map[y + 1, x + 1] == FLOOR;
                    bool swF = y + 1 < h && x - 1 >= 0 && map[y + 1, x - 1] == FLOOR;
                    bool nwF = y - 1 >= 0 && x - 1 >= 0 && map[y - 1, x - 1] == FLOOR;

                    if (nF || eF || sF || wF || neF || seF || swF || nwF)
                    {
                        bool cornerConvex = (nF || sF) && (eF || wF);
                        bool cornerConcave = (neF && !nF && !eF) || (seF && !sF && !eF) || (swF && !sF && !wF) || (nwF && !nF && !wF);

                        if (cornerConvex || cornerConcave)
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
                    bool nF = map[y - 1, x] == FLOOR;
                    bool sF = map[y + 1, x] == FLOOR;
                    bool eF = map[y, x + 1] == FLOOR;
                    bool wF = map[y, x - 1] == FLOOR;

                    bool nW = map[y - 1, x] >= WALL_V && map[y - 1, x] <= CORNER;
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
}
