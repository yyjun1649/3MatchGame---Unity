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
		Apple = 1,
		Banana = 2,
		Grape = 3,
		Orange = 4,
		PineApple = 5,
		Count = 6,
	}

	public enum PieceType
	{
		None = 0,
		Munchkin = 1,
		HorClear = 2,
		VerClear = 3,
		Bomb = 4,
		RainBow = 5,
	}

	public enum CellType
	{
		EMPTY = 0, // �ǽ��� ������ �� ����
		BASIC = 1, // �ǽ��� ������ �� �ִ� �⺻����
		BLOCKER = 2, // ��ֹ�
		COUNT = 3
	}

    public enum BlockType
    {
        EMPTY = 0,
        BASIC = 1,
		PIECE = 2,
		NEVER = 3,
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
  
}
