using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class Define
{
	public const int MAX_COLLECTION_COUNT = 100;

	public enum UIEvent
	{
		Click,
		Pressed,
		PointerDown,
		PointerUp,
	}

	public enum Scene
	{
		Unknown,
		Dev,
		Game,
	}

	public enum Sound
	{
		Bgm,
		Effect,
		Speech,
		Max,
	}

	public enum ColorType
	{
		None = 0,
		Blue = 1,
		Green = 2,
		Red = 3,
		Yellow = 4,
		Gray = 5,
		Cyan = 6,
		Count = 7
	}

	public enum PieceType
	{
		Munchkin = 0,
		RowClear = 1,
		ColumnClear = 2,
		Bomb = 3,
		RainBow = 4,
	}

	public enum CellType
	{
		EMPTY = 0, // 피스가 존재할 수 없음
		BASIC = 1, // 피스가 존재할 수 있는 기본상태
		BLOCKER = 2, // 장애물
		HOLE=3,
	}

    public enum BlockType
    {
        EMPTY = 0,
        BASIC = 1,
		PIECE = 2,
	}

	public enum MoveType
	{
		TOP,
		BOTTOM,
		LEFT,
		RIGHT,
	}

	public enum BFSMode
	{ 
		SUFFLE,
		Match,
	}


	public struct CheckNode
    {
		public int x;
		public int y;
        public int Hfloor;
		public int Vfloor;

        public CheckNode(int _x, int _y, int f, int vf)
         {
			x = _x;
			y = _y;
			Hfloor = f;
			Vfloor = vf;
         }
    }   
}
