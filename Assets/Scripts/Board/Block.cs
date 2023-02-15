using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Block 
{
    private int _x;
    private int _y;
    private BlockType _blockType;
    private ColorType _colorType;
    private PieceType _pieceType;
    private int _index;
    
    public int X { get { return _x; } set { _x = value; _index = _x + _y * 9; } }
    public int Y { get { return _y; } set { _y = value; _index = _x + _y * 9; } }
    public int index { get { return _index; } }

    public BlockType blockType
    {
        get { return _blockType; }
        set { _blockType = value; }
    }

    public ColorType colorType
    {
        get { return _colorType; }
        set { _colorType = value; }
    }
    public PieceType pieceType
    {
        get { return _pieceType; }
        set { _pieceType = value; }
    }

    public Block(float x, float y)
    {
        _x = (int)x;
        _y = (int)y;
        _index = _x + _y * 9;
        _colorType = (ColorType)Random.Range(1, (int)ColorType.Count);
        _blockType = BlockType.BASIC;
        _pieceType = PieceType.None;
    }

    public Block(float x, float y, BlockType blockType)
    {
        _x = (int)x;
        _y = (int)y;
        _index = _x + _y * 9;
        _colorType = ColorType.None;
        _blockType = BlockType.NEVER;
        _pieceType = PieceType.None;
    }

    public Block(Block block)
    {
        _x = block.X;
        _y = block.Y;
        _index = _x + _y * 9;
        _colorType = block.colorType;
        _blockType = block.blockType;
        _pieceType = PieceType.None;
    }

    public Block(Block block, Block other)
    {
        _x = block.X;
        _y = block.Y;
        _index = _x + _y * 9;
        _colorType = other.colorType;
        _blockType = other.blockType;
        _pieceType = PieceType.None;
    }
}
