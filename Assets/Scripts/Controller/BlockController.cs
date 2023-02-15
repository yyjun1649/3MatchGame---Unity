using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting;

public class BlockController : MonoBehaviour
{ 

    public Block info;
    [SerializeField]
    private Image image;
    [SerializeField]
    private Button button;

    [SerializeField]
    private Sprite EmptyImage;

    [SerializeField]
    private Sprite AppleImage;
    [SerializeField]
    private Sprite BananaImage;
    [SerializeField]
    private Sprite GrapeImage;
    [SerializeField]
    private Sprite OrangeImage;
    [SerializeField]
    private Sprite PineappleImage;

    [SerializeField]
    private Sprite MunchikinImage;
    [SerializeField]
    private Sprite RainbowImage;
    [SerializeField]
    private Sprite BombImage;
    [SerializeField]
    private Sprite StraightImage;
    [SerializeField]
    private Sprite fireEffect;

    [SerializeField]
    private GameObject ItemViewObject;


    private BoardController boardController;

    public BlockController(BlockController other)
    {
        Init(other.info.X,other.info.Y);
        info = other.info;
        image = other.image;
        button = other.button;
        boardController = other.boardController;
        ChangeColor();
    }

    public void Init(float x, float y)
    {
        boardController = FindObjectOfType<BoardController>();
        InitEventTrigger();
        info = new Block(x,y);
        ChangeColor();
    }

    public void NoBlockInit(float x, float y, BlockType blockType)
    {
        boardController = FindObjectOfType<BoardController>();
        InitEventTrigger();
        info = new Block(x, y, blockType);
        ChangeColor();
    }
    #region Input
    void InitEventTrigger()
    {
        EventTrigger eventTrigger = transform.GetComponent<EventTrigger>();

        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => { OnPointerDown((PointerEventData)data); });
        eventTrigger.triggers.Add(pointerDown);

        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => { OnPointerUp((PointerEventData)data); });
        eventTrigger.triggers.Add(pointerUp);

        EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((data) => { OnPointerEnter((PointerEventData)data); });
        eventTrigger.triggers.Add(pointerEnter);
    }

    void OnPointerDown(PointerEventData data)
    {
        boardController.SelectBlock(this);
    }

    void OnPointerUp(PointerEventData data)
    {
        boardController.UnSelectBlock(this);
    }

    void OnPointerEnter(PointerEventData data)
    {
        boardController.PointBlock(this);
    }
    #endregion

    public void ChangeColor()
    {
        switch (info.colorType)
        {
            case ColorType.None:
                image.sprite = EmptyImage;
                break;
            case ColorType.Apple:
                image.sprite = AppleImage;
                break;
            case ColorType.Banana:
                image.sprite = BananaImage;
                break;
            case ColorType.Grape:
                image.sprite = GrapeImage;
                break;
            case ColorType.Orange:
                image.sprite = OrangeImage;
                break;
            case ColorType.PineApple:
                image.sprite = PineappleImage;
                break;
        }
    }
    public void ChangeColor(ColorType colorType)
    {
        info.colorType = colorType;
        ChangeColor();
    }
    public void ChangeType(PieceType pt, BlockType bt, ColorType ct)
    {
        ItemViewObject.GetComponent<Image>().sprite = EmptyImage;
        info.pieceType = pt;
        info.blockType = bt;
        ChangeColor(ct);


        if (pt == PieceType.HorClear)
        {
            ItemViewObject.GetComponent<Image>().sprite = StraightImage;
            ItemViewObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if (pt == PieceType.VerClear)
        {
            ItemViewObject.GetComponent<Image>().sprite = StraightImage;
            ItemViewObject.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        }
        else if (pt == PieceType.Bomb)
        {
            ItemViewObject.GetComponent<Image>().sprite = BombImage;
        }
        else if (pt == PieceType.Munchkin)
        {
            ChangeColor(ColorType.None);
            GetComponent<Image>().sprite = MunchikinImage;
        }
        else if (pt == PieceType.RainBow)
        {
            ChangeColor(ColorType.None);
            GetComponent<Image>().sprite = RainbowImage;
        }

    }

    public IEnumerator FireEffectOn()
    {
        if (BoardController.board._cells[info.X, info.Y].GetComponent<CellController>().info.cellType == CellType.BASIC)
        {
            Managers.Sound.Play(Define.Sound.Effect, "Sound_Fire",0.3f);
            ItemViewObject.GetComponent<Image>().sprite = fireEffect;
            ItemViewObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            yield return new WaitForSeconds(2f);
        }
    }

    public void MoveToTargetDirect(Vector3 Target, Block other)
    { 
        info.X = other.X;
        info.Y = other.Y;
        transform.position = Target;
    }

    public IEnumerator MoveToTarget(Vector3 Target, Block other, float speed = 2)
    {
        BoardController.isCoroutineAcitve = true;

        info.X = other.X;
        info.Y = other.Y;
        while ((Target - transform.position).sqrMagnitude > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, Target, speed * 500 * Time.deltaTime);
            yield return null;
        }
        transform.position = Target;
        BoardController.isCoroutineAcitve = false;
    }

    public void Respawn(int MoveDistance)
    {
        Vector3 PrevPosition = BoardController.board._cells[info.X, info.Y].transform.position;
        transform.position = new Vector3(transform.position.x, transform.position.y + (MoveDistance * 50), transform.position.z);
        ChangeType(PieceType.None, BlockType.BASIC, (ColorType)Random.Range(1, (int)ColorType.Count));
        ChangeColor();
        StartCoroutine(MoveToTarget(PrevPosition, info));
    }
}
