using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using UnityEngine.UI;

public class CellController : MonoBehaviour
{
    public Cell info;

    [SerializeField]
    private Image image;

    [SerializeField]
    private Sprite blackhole;

    public void Init(float x, float y, CellType cellType)
    {
        info = new Cell(x, y, cellType);

        Vector4 vec4 = info.Y % 2 == info.X % 2 ? new Vector4(0.0f, 0.0f, 0.2f, 1f) : new Vector4(0.0f, 0.0f, 0.4f, 1f);

        GetComponent<Image>().color = vec4;

        if (cellType == CellType.BLOCKER)
            image.sprite = blackhole;

    }
}
