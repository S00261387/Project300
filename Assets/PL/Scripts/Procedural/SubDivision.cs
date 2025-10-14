using System;

public class SubDivision
{
    public int startX;
    public int startY;
    public int endX;
    public int endY;

    public SubDivision() { }

    public SubDivision(int sx, int sy, int ex, int ey)
    {
        startX = sx; startY = sy; endX = ex; endY = ey;
    }

    public int Width { get { return endX - startX; } }
    public int Height { get { return endY - startY; } }
}
