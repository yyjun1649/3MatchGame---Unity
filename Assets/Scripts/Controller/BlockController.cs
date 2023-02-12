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
                image.color = new Vector4(0, 0, 0, 0);
                break;
            case ColorType.Blue:
                image.color = Color.blue;
                break;
            case ColorType.Red:
                image.color = Color.red;
                break;
            case ColorType.Green:
                image.color = Color.green;
                break;
            case ColorType.Yellow:
                image.color = Color.yellow;
                break;
            case ColorType.Gray:
                image.color = Color.gray;
                break;
            case ColorType.Cyan:
                image.color = Color.cyan;
                break;
        }
    }
    public void ChangeColor(ColorType colorType)
    {
        info.colorType = colorType;
        ChangeColor();
    }
    public void MoveToTargetDirect(Vector3 Target, Block other)
    { 
        info.X = other.X;
        info.Y = other.Y;
        transform.position = Target;
    }

    public IEnumerator MoveToTarget(Vector3 Target, Block other, float speed = 1)
    {
        BoardController.isCoroutineAcitve = true;

        info.X = other.X;
        info.Y = other.Y;
        while ((transform.position - Target).sqrMagnitude > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, Target, speed * 500 * Time.deltaTime);
            yield return null;
        }

        BoardController.isCoroutineAcitve = false;
    }

    public IEnumerator MatchedBlock()
    {
        BoardController.isCoroutineAcitve = true;

        yield return new WaitForSeconds(0.2f);
        ChangeColor(ColorType.None);
        info.blockType = BlockType.EMPTY;

        BoardController.isCoroutineAcitve = false;
    }

    public void Respawn(int MoveDistance)
    {
        Vector3 PrevPosition = transform.position;
        transform.position = new Vector3(transform.position.x, transform.position.y + (MoveDistance * 50), transform.position.z);
        info.blockType = BlockType.BASIC;
        info.colorType = (ColorType)Random.Range(1, (int)ColorType.Count - 1);
        ChangeColor();
        StartCoroutine(MoveToTarget(PrevPosition, info, 2));
    }
}
