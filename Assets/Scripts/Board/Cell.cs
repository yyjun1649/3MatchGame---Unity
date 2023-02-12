using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Cell
{
    private int _x;
    private int _y;
    private CellType _type;
    public int X { get { return _x; } set { _x = value; } }
    public int Y { get { return _y; } set { _y = value; } }

    public CellType cellType
    {
        get { return _type; }
        set { _type = value; }
    }


    public Cell(float x, float y, CellType cell)
    {
        _x = (int)x;
        _y = (int)y;
        _type = cell;
    }

    public Cell(Cell other)
    {
        _x = other.X;
        _y = other.Y;
        _type = other.cellType;
    }
}
