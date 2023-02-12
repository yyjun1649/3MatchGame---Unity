using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board 
{
    private int _width;
    private int _height;
    public int WIDTH
    {
        get { return _width; }
        set { _width = value; }
    }
    public int HEIGHT
    {
        get { return _height; }
        set { _height = value; }
    }

    public BlockController[] _blocks;
    public Transform[,] _cells;

    public Board(int w, int h)
    {
        _width = w;
        _height = h;
        _blocks = new BlockController[_width * _height];
        _cells = new Transform[_width, _height];
    }

}
