using System;

public class Room
{
    public int xPos;
    public int yPos;
    public int xSize;
    public int ySize;

    public Room() { }

    public Room(int x, int y, int w, int h)
    {
        xPos = x; yPos = y; xSize = w; ySize = h;
    }
}
