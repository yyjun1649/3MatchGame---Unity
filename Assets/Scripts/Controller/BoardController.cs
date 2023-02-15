using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using static Define;
using static UnityEngine.GraphicsBuffer;

public class BoardController : MonoBehaviour
{
    public static Board board;
    public static bool isCoroutineAcitve = false;

    #region Initialization var
    public int WIDTH;
    public int HEIGHT;
    #endregion

    #region SerializeField var
    [SerializeField]
    private GameObject cellPrefab;

    [SerializeField]
    private GameObject blockPrefab;

    [SerializeField]
    private Transform cellParent;

    [SerializeField]
    private Transform blockParent;
    #endregion

    #region Necessary var
    int[] dx = {1, 0, -1, 0, 2, 0, -2, 0, 1, 1, -1, -1 };
    int[] dy = {0, 1, 0, -1, 0, 2, 0, -2, -1, 1, 1, -1 };

    public bool canSwap = true;
    public bool canSelect = true;

    private BlockController selectedBlock = null;
    private BlockController targetBlock = null;

    public int CanMove = 50;
    public int Score = 4;
    #endregion

    #region 정렬
    private void SortBoard()
    {
        Array.Sort(board._blocks, delegate(BlockController b1, BlockController b2)
        {
            return b1.info.index.CompareTo(b2.info.index);
        });
    }
    void ChangeBlockValue(BlockController A, BlockController B, bool DirectMove = false, float speed = 1)
    {
        Vector3 APos = board._cells[A.info.X, A.info.Y].transform.position;
        Vector3 BPos = board._cells[B.info.X, B.info.Y].transform.position;

        Block temp = new Block(A.info);

        StartCoroutine(A.MoveToTarget(BPos, B.info, speed));
        if (DirectMove)
            B.MoveToTargetDirect(APos, temp);
        else
            StartCoroutine(B.MoveToTarget(APos, temp, speed));

        SortBoard();

    }
    #endregion

    #region Input
    public void SelectBlock(BlockController block)
    {
        if (!canSelect)
            return;

        //Debug.Log(block.info.X + " , " + block.info.Y + " , "+ block.info.colorType.ToString() + block.info.blockType.ToString() + block.info.pieceType.ToString() + " Select!");
        //Debug.Log(board._blocks[block.info.index].info.X + " , " + board._blocks[block.info.index].info.Y + " Select!");
        
        canSelect = false;
        selectedBlock = block;
        canSwap = true;
    }

    public void UnSelectBlock(BlockController block)
    {
        if (canSelect)
            return;

        canSelect = true;
        //Debug.Log(block.info.X + "," + block.info.Y + "UnSelect!");
    }

    public void PointBlock(BlockController block)
    {
        if (selectedBlock == null || !canSwap || canSelect || block.info.blockType == BlockType.EMPTY || block.info.blockType == BlockType.NEVER)
            return;

        //Debug.Log(block.info.X + "," + block.info.Y + "Point Select!");

        for (int i = 0; i < 4; i++)
        {
            int x = selectedBlock.info.X + dx[i];
            int y = selectedBlock.info.Y + dy[i];

            if(block.info.X == x && block.info.Y == y)
            {
                canSwap = false;
                canSelect = false;
                targetBlock = block;
                StartCoroutine(SwapBlock());
                break;
            }
        }
    }
    #endregion

    #region Init
    public void Init()
    {
        board = new Board(WIDTH, HEIGHT);

        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                Vector2 temp = new Vector2(i, j);
                InstatntiateCell(temp);
                InstatntiateBlock(temp);
            }
        }
        Score = 4;
        board._blocks[4+ 4*HEIGHT].ChangeType(PieceType.Munchkin, BlockType.PIECE, ColorType.None);

        InitBoardMatch(BFSMode.SUFFLE);
    }

    void InstatntiateCell(Vector2 temp)
    {
        GameObject cellInstantiate = Instantiate(cellPrefab, new Vector2(-400 + (temp.x * 100), 300 - (temp.y * 100)), Quaternion.identity, cellParent);
        CellController cellCon = cellInstantiate.GetComponent<CellController>();

        CellType cellType = (CellType)(Managers.Data.Stage6.Cells[(int)temp.x + (int)temp.y * HEIGHT] - '0');
        
        cellCon.Init(temp.x, temp.y, cellType);

        cellInstantiate.name = "cell" + cellCon.info.X.ToString() + " , " + cellCon.info.Y.ToString();
        board._cells[cellCon.info.X, cellCon.info.Y] = cellInstantiate.transform;
    }

    void InstatntiateBlock(Vector2 temp)
    {
        GameObject blockInstantiate = Instantiate(blockPrefab, new Vector2(-400 + (temp.x * 100), 300 - (temp.y * 100)), Quaternion.identity, blockParent);
        BlockController blockCon = blockInstantiate.GetComponent<BlockController>();

        if (board._cells[(int)temp.x,(int)temp.y].GetComponent<CellController>().info.cellType == CellType.BASIC)
            blockCon.Init(temp.x, temp.y);
        else
            blockCon.NoBlockInit(temp.x, temp.y,BlockType.NEVER);

        blockInstantiate.name = "block" + blockCon.info.X.ToString() + " , " + blockCon.info.Y.ToString();
        board._blocks[blockCon.info.index] = blockCon;
    }

    #endregion


    // Logic

    #region Swap
    IEnumerator SwapBlock()
    {
        //Debug.Log("Swap Start");
        CanMove--;

        if (selectedBlock.info.pieceType == PieceType.Munchkin)
            StartCoroutine(MunchkinAction(selectedBlock.info, targetBlock.info));

        else {

            ChangeBlockValue(selectedBlock, targetBlock);

            if (selectedBlock.info.pieceType == PieceType.RainBow)
                StartCoroutine(RainbowAction(targetBlock.info, selectedBlock));

            else if (targetBlock.info.pieceType == PieceType.RainBow)
            {
                StartCoroutine(RainbowAction(selectedBlock.info, targetBlock));
            }
            else
            {
                while (isCoroutineAcitve)
                    yield return null;


                if (InitBoardMatch(BFSMode.Match))
                {
                    while (isCoroutineAcitve)
                        yield return null;
                    targetBlock = null;
                    selectedBlock = null;
                    StartCoroutine(BlockDownAndRespawn());
                }
                else
                {
                    CanMove++;
                    while (isCoroutineAcitve)
                        yield return null;
                    ChangeBlockValue(selectedBlock, targetBlock);
                    canSwap = true;
                    canSelect = true;
                }
            }
        }
        Managers.UI.FindPopup<UI_BoardPopup>().RefreshUI();
    }
    #endregion

    #region Match Behavior
    bool InitBoardMatch(BFSMode mode = BFSMode.Match)
    {
        bool isMatch = false;
        int[,] countBoard = new int[9, 9];
        int[,] count2x2Board = new int[9, 9];

        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                if (board._blocks[i + j * HEIGHT].info.blockType == BlockType.EMPTY || board._blocks[i + j * HEIGHT].info.blockType == BlockType.NEVER) continue;

                if (GetVerticalBlockCount(board._blocks[i + j * HEIGHT].info) > 2)
                    countBoard[i, j]++;
                if (GetHorizontalBlockCount(board._blocks[i + j * HEIGHT].info) > 2)
                    countBoard[i, j]++;
                if (GetVerticalBlockCount(board._blocks[i + j * HEIGHT].info) > 1)
                    count2x2Board[i, j]++;
                if (GetHorizontalBlockCount(board._blocks[i + j * HEIGHT].info) > 1)
                    count2x2Board[i, j]++;
            }
        }

        // ㄴ , ㅜ 검사
        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                if (countBoard[i, j] == 2)
                {
                    if(Find3Match(board._blocks[i + j * HEIGHT].info, ref countBoard, mode))
                        isMatch = true;
                }
            }
        }

        for(int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                if(countBoard[i, j] == 1 && Find5Match(board._blocks[i + j * HEIGHT].info, ref countBoard, mode))
                    isMatch = true;
                if(count2x2Board[i, j] == 2 && Find2X2Match(board._blocks[i + j * HEIGHT].info, ref countBoard, ref count2x2Board, mode))
                    isMatch = true;
                if(countBoard[i, j] == 1 && Find4Match(board._blocks[i + j * HEIGHT].info, ref countBoard, mode))
                    isMatch = true; 
                if(countBoard[i, j] == 1 && Find3Match(board._blocks[i + j * HEIGHT].info, ref countBoard, mode))
                    isMatch = true;
            }
        }


        return isMatch;
    }

    int GetVerticalBlockCount(Block b)
    {
        int count = 0;
        for(int i = b.Y - 2; i <= b.Y + 2; i++)
        {
            if (i < 0) continue;
            if (i >= HEIGHT) break;

            if (b.colorType == board._blocks[b.X + i * HEIGHT].info.colorType)
                count++;
        }
        return count;
    }

    int GetHorizontalBlockCount(Block b)
    {
        int count = 0;
        for (int i = b.X - 2; i <= b.X + 2; i++)
        {
            if (i < 0) continue;
            if (i >= WIDTH) break;

            if (b.colorType == board._blocks[i + b.Y * HEIGHT].info.colorType)
                count++;
        }
        return count;
    }

    // Match 되는 블럭이 있습니까?
    bool Find3Match(Block b, ref int[,] countBoard, BFSMode mode)
    {
        int x = b.X;
        int y = b.Y;
        bool isMatch = false;
        int mask = 0;
        List<BlockController> matchList = new List<BlockController>();

        for (int i = 0; i < 4; i++)
        {
            bool isMatch2 = false;
            int rx = x + dx[i];
            int ry = y + dy[i];

            int rx2 = x + dx[(i + 2) % 4]; // rx의 반대방향
            int ry2 = y + dy[(i + 2) % 4];

            int rx3 = x + dx[i+4]; // rx 보다 한칸 더
            int ry3 = y + dy[i+4];

            if (rx >= 0 && rx < WIDTH && ry >=0 && ry < HEIGHT && board._blocks[rx + ry * HEIGHT].info.colorType == b.colorType)
            {
                // rx 반대칸 (L 으로 매칭되면 안되므로 반대쪽 체크)
                if (rx2 >= 0 && rx2 < WIDTH && ry2 >= 0 && ry2 < HEIGHT && board._blocks[rx2 + ry2 * HEIGHT].info.colorType == b.colorType)
                {
                    isMatch2 = true; 
                }

                // rx 다음 칸
                if (rx3 >= 0 && rx3 < WIDTH && ry3 >= 0 && ry3 < HEIGHT && board._blocks[rx3 + ry3 * HEIGHT].info.colorType == b.colorType)
                {
                    matchList.Add(board._blocks[rx3 + ry3 * HEIGHT]);
                    countBoard[rx3, ry3]--;
                    mask |= 1 << (i + 4);
                    isMatch2 = true;
                }


                if(isMatch2)
                {
                    isMatch = true;
                    matchList.Add(board._blocks[rx + ry * HEIGHT]);
                    countBoard[rx, ry]--;
                    mask |= 1 << i;
                }
            }
        }

        if(isMatch)
        {
            countBoard[x, y]=0;
            if (mode == BFSMode.Match)
                MatchBlock(mask, b, matchList);
           
            else if(mode == BFSMode.SUFFLE)
                SuffleBoard(board._blocks[x + y * HEIGHT]);

            return true;
        }
        
        return false;
    }

    bool Find4Match(Block b, ref int[,] countBoard, BFSMode mode)
    {
        int x = b.X;
        int y = b.Y;
        bool isMatch = false;
        int mask = 0;
        List<BlockController> matchList = new List<BlockController>();


        for (int i = 0; i < 2; i++)
        {
            int rx = x + dx[i];
            int ry = y + dy[i];

            if (((i == 0 && rx >= 0 && rx < WIDTH && board._blocks[rx + ry * HEIGHT].info.colorType == b.colorType) || (i == 1 && ry >= 0 && ry < HEIGHT && board._blocks[rx + ry * HEIGHT].info.colorType == b.colorType)))
            {
                if (((i == 0 && rx + 1 >= 0 && rx + 1 < WIDTH && board._blocks[(rx + 1) + ry * HEIGHT].info.colorType == b.colorType) || (i == 1 && ry + 1 >= 0 && ry + 1 < HEIGHT && board._blocks[rx + (ry+1) * HEIGHT].info.colorType == b.colorType)))
                {
                    if (((i == 0 && rx + 2 >= 0 && rx + 2 < WIDTH && board._blocks[(rx + 2) + ry * HEIGHT].info.colorType == b.colorType) || (i == 1 && ry + 2 >= 0 && ry + 2 < HEIGHT && board._blocks[rx + (ry + 2) * HEIGHT].info.colorType == b.colorType)))
                    {
                        countBoard[x, y]--;
                        if (i == 0)
                        {
                            countBoard[(rx + 0), ry]--;
                            countBoard[(rx + 1), ry]--;
                            countBoard[(rx + 2), ry]--;
                            matchList.Add(board._blocks[(rx + 0) + ry * HEIGHT]);
                            matchList.Add(board._blocks[(rx + 1) + ry * HEIGHT]);
                            matchList.Add(board._blocks[(rx + 2) + ry * HEIGHT]);
                            mask = 21;
                        }
                        else if (i == 1)
                        {
                            countBoard[rx, (ry + 0)]--;
                            countBoard[rx, (ry + 1)]--;
                            countBoard[rx, (ry + 2)]--;
                            matchList.Add(board._blocks[rx + (ry + 0) * HEIGHT]);
                            matchList.Add(board._blocks[rx + (ry + 1) * HEIGHT]);
                            matchList.Add(board._blocks[rx + (ry + 2) * HEIGHT]);
                            mask = 42;
                        }                        
                        isMatch = true;
                    }
                }
            }
        }

        if (isMatch)
        {
            if (mode == BFSMode.Match)
                MatchBlock(mask, b, matchList);

            else if (mode == BFSMode.SUFFLE)
                SuffleBoard(board._blocks[x + y * HEIGHT]);

            return true;
        }

        return false;
    }

    bool Find5Match(Block b, ref int[,] countBoard, BFSMode mode)
    {
        int x = b.X;
        int y = b.Y;
        bool isMatch = false;
        int mask = 0;
        List<BlockController> matchList = new List<BlockController>();


        for (int i = 0; i < 2; i++)
        {
            int rx = x + dx[i];
            int ry = y + dy[i];

            if (((i == 0 && rx >= 0 && rx < WIDTH && board._blocks[rx + ry * HEIGHT].info.colorType == b.colorType) || (i == 1 && ry >= 0 && ry < HEIGHT && board._blocks[rx + ry * HEIGHT].info.colorType == b.colorType)))
            {
                if (((i == 0 && rx + 1 >= 0 && rx + 1 < WIDTH && board._blocks[(rx + 1) + ry * HEIGHT].info.colorType == b.colorType) || (i == 1 && ry + 1 >= 0 && ry + 1 < HEIGHT && board._blocks[rx + (ry + 1) * HEIGHT].info.colorType == b.colorType)))
                {
                    if (((i == 0 && rx + 2 >= 0 && rx + 2 < WIDTH && board._blocks[(rx + 2) + ry * HEIGHT].info.colorType == b.colorType) || (i == 1 && ry + 2 >= 0 && ry + 2 < HEIGHT && board._blocks[rx + (ry + 2) * HEIGHT].info.colorType == b.colorType)))
                    {
                        if (((i == 0 && rx + 3 >= 0 && rx + 3 < WIDTH && board._blocks[(rx + 3) + ry * HEIGHT].info.colorType == b.colorType) || (i == 1 && ry + 3 >= 0 && ry + 3 < HEIGHT && board._blocks[rx + (ry + 3) * HEIGHT].info.colorType == b.colorType)))
                        {
                            countBoard[x, y]--;
                            if (i == 0)
                            {
                                countBoard[(rx + 0), ry]--;
                                countBoard[(rx + 1), ry]--;
                                countBoard[(rx + 2), ry]--;
                                countBoard[(rx + 3), ry]--;
                                matchList.Add(board._blocks[(rx + 0) + ry * HEIGHT]);
                                matchList.Add(board._blocks[(rx + 1) + ry * HEIGHT]);
                                matchList.Add(board._blocks[(rx + 2) + ry * HEIGHT]);
                                matchList.Add(board._blocks[(rx + 3) + ry * HEIGHT]);
                                mask = 85;
                            }
                            else if (i == 1)
                            {
                                countBoard[rx, (ry + 0)]--;
                                countBoard[rx, (ry + 1)]--;
                                countBoard[rx, (ry + 2)]--;
                                countBoard[rx, (ry + 3)]--;
                                matchList.Add(board._blocks[rx + (ry + 0) * HEIGHT]);
                                matchList.Add(board._blocks[rx + (ry + 1) * HEIGHT]);
                                matchList.Add(board._blocks[rx + (ry + 2) * HEIGHT]);
                                matchList.Add(board._blocks[rx + (ry + 3) * HEIGHT]);
                                mask = 170;
                            }
                            isMatch = true;
                        }
                    }
                }
            }
        }

        if (isMatch)
        {
            if (mode == BFSMode.Match)
                MatchBlock(mask, b, matchList);

            else if (mode == BFSMode.SUFFLE)
                SuffleBoard(board._blocks[x + y * HEIGHT]);

            return true;
        }

        return false;
    }

    bool Find2X2Match(Block b, ref int[,] countBoard, ref int[,] count2X2Board, BFSMode mode)
    {
        int x = b.X;
        int y = b.Y;
        bool isMatch = false;
        int mask = 0;
        List<BlockController> matchList = new List<BlockController>();

        for (int i = 0; i < 4; i++)
        {
            bool isMatch2 = false;
            int rx = x + dx[i];
            int ry = y + dy[i];

            int rx2 = x + dx[(i+3)%4]; // 이전 단계
            int ry2 = y + dy[(i+3)%4];

            int rx3 = x + dx[i + 8]; // 이전 대각
            int ry3 = y + dy[i + 8];

            int rx4 = x + dx[8 + ((i + 1) % 4)]; // 다음 대각
            int ry4 = y + dy[8 + ((i + 1) % 4)];

            int rx5 = x + dx[(i + 1) % 4]; // 다음 단계
            int ry5 = y + dy[(i + 1) % 4];

            if (rx >= 0 && rx < WIDTH && ry >= 0 && ry < HEIGHT && board._blocks[rx + ry * HEIGHT].info.colorType == b.colorType)
            {

                if (rx3 >= 0 && rx3 < WIDTH && ry3 >= 0 && ry3 < HEIGHT && board._blocks[rx3 + ry3 * HEIGHT].info.colorType == b.colorType)
                {
                    if (rx2 >= 0 && rx2 < WIDTH && ry2 >= 0 && ry2 < HEIGHT && board._blocks[rx2 + ry2 * HEIGHT].info.colorType == b.colorType)
                    {
                        isMatch2 = true;
                    }
                }

                if (rx4 >= 0 && rx4 < WIDTH && ry4 >= 0 && ry4 < HEIGHT && board._blocks[rx4 + ry4 * HEIGHT].info.colorType == b.colorType)
                {
                    if (rx5 >= 0 && rx5 < WIDTH && ry5 >= 0 && ry5 < HEIGHT && board._blocks[rx5 + ry5 * HEIGHT].info.colorType == b.colorType)
                    {
                        matchList.Add(board._blocks[rx4 + ry4 * HEIGHT]);
                        countBoard[rx4, ry4]--;
                        count2X2Board[rx4,ry4]--;
                        mask |= 1 << 8 + ((i + 1) % 4);
                        isMatch2 = true;
                    }
                }

                if (isMatch2)
                {
                    isMatch = true;
                    matchList.Add(board._blocks[rx + ry * HEIGHT]);
                    count2X2Board[rx, ry]--;
                    countBoard[rx, ry]--;
                    mask |= 1 << i;
                }
            }
        }

        if (isMatch)
        {
            countBoard[x, y] = 0;
            if (mode == BFSMode.Match)
                MatchBlock(mask, b, matchList);

            else if (mode == BFSMode.SUFFLE)
                SuffleBoard(board._blocks[x + y * HEIGHT]);

            return true;
        }

        return false;
    }

    void MatchBlock(int mask, Block b, List<BlockController> matchList)
    {
        ColorType color = board._blocks[b.X + b.Y * HEIGHT].info.colorType;
        switch (mask)
        {
            // 1
            case 0:                
                break;
            // 2
            case 1:case 2:case 4:case 8:
                break;


            // 3
            case 5:case 10:case 17:case 34:case 68:case 136:
                MatchAnimStart(board._blocks[b.X + b.Y * HEIGHT]);
                ListMatchAnimStart(matchList);
                break;

            // 4 Horizontal Line Clear
            case 21:case 69:
                ListMatchAnimStart(matchList);
                PieceMake(board._blocks[b.X + b.Y * HEIGHT], PieceType.HorClear, color);
                break;

            // 4 Vertical Line Clear
            case 42:case 138:
                ListMatchAnimStart(matchList);
                PieceMake(board._blocks[b.X + b.Y * HEIGHT], PieceType.VerClear, color);
                break;

            // 5 Rainbow Hor
            case 85:
                ListMatchAnimStart(matchList);
                PieceMake(board._blocks[(b.X + 2) + b.Y * HEIGHT], PieceType.RainBow, ColorType.None);
                break;

            // 5 Rainbow Ver
            case 170:
                ListMatchAnimStart(matchList);
                PieceMake(board._blocks[b.X + (b.Y + 2) * HEIGHT], PieceType.RainBow, ColorType.None);
                break;

            // 4X3 Bomb
            case 55: case 59: case 110: case 133: case 155: case 157: case 205:case 206:
                ListMatchAnimStart(matchList);
                PieceMake(board._blocks[b.X + b.Y * HEIGHT], PieceType.Bomb, color);
                break;

            // ㄱ ㄴ .. Bomb
            case 51:case 102:case 153:case 204:
                ListMatchAnimStart(matchList);
                PieceMake(board._blocks[b.X + b.Y * HEIGHT], PieceType.Bomb, color);
                break;

            // ㅏ ㅓ ㅗ ㅜ Bomb
            case 27:case 39:case 94:case 141:
                ListMatchAnimStart(matchList);
                PieceMake(board._blocks[b.X + b.Y * HEIGHT], PieceType.Bomb, color);
                break;

            case 265: case 515: case 1054: case 3108:
                ListMatchAnimStart(matchList);
                PieceMake(board._blocks[b.X + b.Y * HEIGHT], PieceType.Munchkin, ColorType.None);
                break;
        }   
            
        
    }

    void ListMatchAnimStart(List<BlockController> matchList)
    {
        for(int i = 0; i < matchList.Count; i++)
        {
            MatchAnimStart(matchList[i]);
        }
    }

    void MatchAnimStart(BlockController b)
    {
        if ((b.info.blockType == BlockType.NEVER))
            return;

        if ((b.info.blockType == BlockType.BASIC || b.info.pieceType == PieceType.Munchkin))
        {
            b.ChangeType(PieceType.None, BlockType.EMPTY, ColorType.None);
            Managers.Sound.Play(Define.Sound.Effect, "Sound_Bomb",0.3f);
        }
        else if (b.info.blockType == BlockType.PIECE)
            StartCoroutine(PieceAction(b.info));
    }

    void SuffleBoard(BlockController b)
    {
        for (int i = 0; i < WIDTH; i++)
        {
            for(int j = 0; j < HEIGHT; j++)
            {
                b.ChangeType(PieceType.None, BlockType.BASIC, ColorType.PineApple);
            }
        }
    }
    #endregion

    #region Piece Logic

    IEnumerator PieceAction(Block info)
    {
        if (info.pieceType != PieceType.Munchkin && info.pieceType != PieceType.Munchkin)
        {
            isCoroutineAcitve = true;
            List<BlockController> Blocks = new List<BlockController>();

            if (info.pieceType == PieceType.HorClear)
                Blocks.AddRange(StraightClear(true, info));
            else if (info.pieceType == PieceType.VerClear)
                Blocks.AddRange(StraightClear(false, info));
            else if (info.pieceType == PieceType.Bomb)
                Blocks.AddRange(BombClear(info));


            board._blocks[info.X + info.Y * HEIGHT].ChangeType(PieceType.None, BlockType.EMPTY, ColorType.None);
            StartCoroutine(board._blocks[info.X + info.Y * HEIGHT].FireEffectOn());
            
            for (int i = 0; i < Blocks.Count; i++)
            {
                MatchAnimStart(Blocks[i]);
                StartCoroutine(Blocks[i].FireEffectOn());
            }
            yield return new WaitForSeconds(1.5f);
            isCoroutineAcitve = false;
        }
    }

    IEnumerator RainbowAction(Block info, BlockController RainbowBlock)
    {
        List<BlockController> Blocks = new List<BlockController>();

        for(int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                if (info.colorType == board._blocks[i + j * HEIGHT].info.colorType)
                    Blocks.Add(board._blocks[i + j * HEIGHT]);
            }
        }

        RainbowBlock.ChangeType(PieceType.None, BlockType.EMPTY, ColorType.None);
        StartCoroutine(RainbowBlock.FireEffectOn());

        for (int i = 0; i < Blocks.Count; i++)
        {
            MatchAnimStart(Blocks[i]);
            StartCoroutine(Blocks[i].FireEffectOn());
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(2f);

        targetBlock = null;
        selectedBlock = null;
        StartCoroutine(BlockDownAndRespawn());
    }

    IEnumerator MunchkinAction(Block info, Block target)
    {
        int x = info.X;
        int y = info.Y;

        int dx = target.X - info.X;
        int dy = target.Y - info.Y;

        int rx = x + dx;
        int ry = y + dy;

        while (rx  < WIDTH && rx  >= 0 && ry  >= 0 && ry  < HEIGHT)
        {
            if (board._cells[rx, ry].GetComponent<CellController>().info.cellType != CellType.BASIC)
            {
                Score--;
                Managers.UI.FindPopup<UI_BoardPopup>().RefreshUI();
                break;
            }
            MatchAnimStart(board._blocks[rx + ry * HEIGHT]);
            ChangeBlockValue(board._blocks[x + y * HEIGHT], board._blocks[rx + ry * HEIGHT], false, 1f);
            StartCoroutine(board._blocks[x + y * HEIGHT].FireEffectOn());
            while (isCoroutineAcitve)
                yield return null;


            x = rx;
            y = ry;

            rx += dx;
            ry += dy;
        }

        MatchAnimStart(board._blocks[x + y * HEIGHT]);
        yield return new WaitForSeconds(1f);

        targetBlock = null;
        selectedBlock = null;
        StartCoroutine(BlockDownAndRespawn());
    }

    List<BlockController> StraightClear(bool isHorizontal, Block info)
    {
        List<BlockController> matchBlock = new List<BlockController>();
        // 가로 클리어
        if (isHorizontal)
        {
            for (int i = 0; i < WIDTH; i++)
            {
                if (info.X == i) continue;
                matchBlock.Add(board._blocks[i + info.Y * HEIGHT]);
            }
        }

        // 세로 클리어
        else
        {
            for (int i = 0; i < HEIGHT; i++)
            {
                if (info.Y == i * HEIGHT) continue;
                matchBlock.Add(board._blocks[info.X + i * HEIGHT]);
            }
        }

        return matchBlock;
    }

    List<BlockController> BombClear(Block here)
    {
        bool[,] discovered = new bool[9, 9];
        int[,] distance = new int[9, 9];
        Queue<Block> q = new Queue<Block>();
        List<BlockController> matchBlock = new List<BlockController>();
        q.Enqueue(here);
        while (q.Count > 0)
        {
            here = q.Dequeue();
            int rx = here.X;
            int ry = here.Y;
            for (int i = 0; i < 4; i++)
            {
                int x = rx + dx[i];
                int y = ry + dy[i];

                if (x < 0 || y < 0 || x >= WIDTH || y >= HEIGHT) continue;
                if (discovered[x, y]) continue;

                distance[x, y] = distance[rx, ry] + 1;
                discovered[x, y] = true;

                if (distance[x, y] < 3)
                {
                    matchBlock.Add(board._blocks[x + y * HEIGHT]);
                    q.Enqueue(board._blocks[x + y * HEIGHT].info);
                }
            }
        }
        
        return matchBlock;
    }

    void PieceMake(BlockController block, PieceType makePiece, ColorType colorType)
    {
        block.ChangeType(makePiece, BlockType.PIECE, colorType);
    }
    #endregion

    IEnumerator BlockDownAndRespawn()
    {
        BlockDown();

        while (isCoroutineAcitve)
            yield return new WaitForSeconds(0.5f);

        StartCoroutine(RespawnBlock());
    }

    #region BlockDown
    void BlockDown()
    {
        //Debug.Log("BlockDown!!");
        List<List<int>> DivideBlocks = new List<List<int>>();
        int[,] moveDistance = new int[WIDTH, HEIGHT];

        DivideSection(ref DivideBlocks);
        CalDistance(ref moveDistance,DivideBlocks);
        BlockMoveDown(moveDistance);
    }

    void DivideSection(ref List<List<int>> DivideBlocks)
    {
        // [BAISC] [EMPTY] [BAISC] [EMPTY] 순으로 값 저장
        for (int i = 0; i < WIDTH; i++)
        {
            int countBasic = 0;
            int countEmpty = 0;

            bool isBasic = true;
            DivideBlocks.Add(new List<int>());
            for (int j = HEIGHT - 1; j > -1; j--)
            {
                if (board._blocks[i + j * HEIGHT].GetComponent<BlockController>().info.blockType != BlockType.EMPTY)
                {
                    if (!isBasic)
                    {
                        isBasic = true;
                        DivideBlocks[i].Add(countEmpty);                        
                        countEmpty = 0;
                    }
                    countBasic++;
                }
                else if (board._blocks[i + j * HEIGHT].GetComponent<BlockController>().info.blockType == BlockType.EMPTY)
                {
                    if (isBasic)
                    {
                        isBasic = false;

                        if (j != HEIGHT - 1)
                            DivideBlocks[i].Add(countBasic);

                        countBasic = 0;
                    }
                    countEmpty++;
                }
            }

            if (isBasic)
                DivideBlocks[i].Add(countBasic);
            else
                DivideBlocks[i].Add(countEmpty);
        }
    }

    void CalDistance(ref int[,] moveDistance, List<List<int>> DivideBlocks)
    {
        for (int i = 0; i < DivideBlocks.Count; i++)
        {
            if (DivideBlocks[i].Count == 1) continue;

            int distance = 0;
            int basicCount = DivideBlocks[i][0];
            for (int j = 1; j < DivideBlocks[i].Count; j++)
            {
                if (j % 2 == 1) distance += DivideBlocks[i][j];

                else
                {
                    for (int r = 0; r < DivideBlocks[i][j]; r++)
                    {
                        if (board._cells[i, (HEIGHT - 1 - (distance + basicCount))].GetComponent<CellController>().info.cellType == CellType.BASIC)
                            moveDistance[i, (HEIGHT -1 - (distance + basicCount))] = distance;
                        basicCount++;
                    }
                }
            }
        }
        
    }

    void BlockMoveDown(int[,] moveDistance)
    {
       bool flag = false;
       for (int i = WIDTH - 1; i > -1; i--)
        {
            for (int j = HEIGHT - 1; j > -1; j--)
            {
                if (moveDistance[i, j] > 0)
                {
                    isCoroutineAcitve = true;
                    flag = true;
                    //Debug.Log("BlockDown");
                    ChangeBlockValue(board._blocks[i + j * HEIGHT].GetComponent<BlockController>(), board._blocks[i + (j + moveDistance[i, j]) * HEIGHT].GetComponent<BlockController>(), true, 3);
                }
            }
        }

        if (flag)
        {
            isCoroutineAcitve = false;
        }
        
    }
    #endregion

    #region Respawn
    IEnumerator RespawnBlock()
    {
        //Debug.Log("RespawnBlock!!");
        bool DidRespawn = false;
        for(int i = 0; i < WIDTH; i++)
        {
            int basicCount = 0;
            for (int j = HEIGHT - 1; j > -1; j--)
            {
                if (board._blocks[i + j * HEIGHT].info.blockType == BlockType.EMPTY)
                {
                    board._blocks[i + j * HEIGHT].Respawn(HEIGHT + 1 - basicCount);
                    DidRespawn = true;
                }
                else if (board._blocks[i + j * HEIGHT].info.blockType == BlockType.BASIC || board._blocks[i + j * HEIGHT].info.blockType == BlockType.PIECE)
                    basicCount++;
            }
        }

        
        yield return new WaitForSeconds(1f);

        canSelect = true;
        if (DidRespawn)
        {
            canSelect = false;

            if (InitBoardMatch(BFSMode.Match))
            {
                while (isCoroutineAcitve)
                    yield return null;
                targetBlock = null;
                selectedBlock = null;
                StartCoroutine(BlockDownAndRespawn());
            }
        }
    }
    #endregion
}
