using System;
using System.Collections.Generic;

public class Node
{
    public Room room;
    public bool connected;
    public List<Node> adjacentNodes;

    public Node()
    {
        connected = false;
        adjacentNodes = new List<Node>();
    }

    public Node(Room r)
    {
        room = r;
        connected = false;
        adjacentNodes = new List<Node>();
    }
}
