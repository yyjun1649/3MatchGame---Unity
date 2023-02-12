using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CellController : MonoBehaviour
{
    public Cell info;

    public void Init(float x, float y, CellType cellType)
    {
        info = new Cell(x, y, cellType);
    }
}
