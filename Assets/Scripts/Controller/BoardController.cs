using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
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
    int[] dx = { 1, -1, 0, 0 };
    int[] dy = { 0, 0, 1, -1 };

    public bool canSwap = true;
    public bool canSelect = true;

    private BlockController selectedBlock = null;
    private BlockController targetBlock = null;
    #endregion
    
    private void SortBoard()
    {
        Array.Sort(board._blocks, delegate(BlockController b1, BlockController b2)
        {
            return b1.info.index.CompareTo(b2.info.index);
        });
    }

    #region Input
    public void SelectBlock(BlockController block)
    {
        if (!canSelect)
            return;

        //Debug.Log(block.info.X + " , " + block.info.Y + " , "+ block.info.colorType.ToString() + " Select!");
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
        Debug.Log(block.info.X + "," + block.info.Y + "UnSelect!");
    }

    public void PointBlock(BlockController block)
    {
        if (selectedBlock == null || !canSwap || canSelect || block.info.blockType == BlockType.EMPTY)
            return;

        Debug.Log(block.info.X + "," + block.info.Y + "Point Select!");

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

        InitBoardBFS(BFSMode.SUFFLE);
        InitBoardBFS(BFSMode.SUFFLE);
    }

    void InstatntiateCell(Vector2 temp)
    {
        GameObject cellInstantiate = Instantiate(cellPrefab, new Vector2(-400 + (temp.x * 100), 300 - (temp.y * 100)), Quaternion.identity, cellParent);
        CellController cellCon = cellInstantiate.GetComponent<CellController>();
        cellCon.Init(temp.x, temp.y, CellType.BASIC);

        cellInstantiate.name = "cell" + cellCon.info.X.ToString() + " , " + cellCon.info.Y.ToString();
        board._cells[cellCon.info.X, cellCon.info.Y] = cellInstantiate.transform;
    }

    void InstatntiateBlock(Vector2 temp)
    {
        GameObject blockInstantiate = Instantiate(blockPrefab, new Vector2(-400 + (temp.x * 100), 300 - (temp.y * 100)), Quaternion.identity, blockParent);
        BlockController blockCon = blockInstantiate.GetComponent<BlockController>();
        blockCon.Init(temp.x, temp.y);

        blockInstantiate.name = "block" + blockCon.info.X.ToString() + " , " + blockCon.info.Y.ToString();
        board._blocks[blockCon.info.index] = blockCon;
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

    bool InitBoardBFS(BFSMode mode)
    {
        bool isMatch = false;
        bool[,] discovered = new bool[WIDTH, HEIGHT];
        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                CheckNode here = new CheckNode(i, j,9,9);
                if(BoardBFS(here, ref discovered, mode))
                    isMatch = true;
            }
        }
        return isMatch;
    }
    // Match 되는 블럭이 있습니까?
    bool BoardBFS(CheckNode here, ref bool[,] discovered, BFSMode mode)
    {
        if (discovered[here.x,here.y] || board._blocks[here.x+ here.y * HEIGHT].GetComponent<BlockController>().info.blockType == BlockType.EMPTY)
            return false;

        List<BlockController> matchBlocks = new List<BlockController>();
        Queue<CheckNode> q = new Queue<CheckNode>();

        int[,] distance = new int[WIDTH, HEIGHT];
        int[] HorCount = new int[WIDTH*2];
        int[] VerCount = new int[HEIGHT*2];

        bool isMatch = false;

        //초기화
        q.Enqueue(here);
        matchBlocks.Add(board._blocks[here.x + here.y * 9]);
        discovered[here.x,here.y] = true;
        distance[here.x, here.y] = 0;
        HorCount[here.Hfloor]++;
        VerCount[here.Vfloor]++;

        while (q.Count != 0)
        {
            here = q.Dequeue();
            int rx = here.x;
            int ry = here.y;
            for (int i = 0; i < 4; i++)
            {
                int x = rx + dx[i];
                int y = ry + dy[i];
                int hf = here.Hfloor;
                int vf = here.Vfloor;

                hf += dx[i];
                vf += dy[i];

                if (x < 0 || y < 0 || x >= WIDTH || y >= HEIGHT) continue;
                // 2X2 체크
                if (discovered[x, y])
                {
                    if (distance[x, y] >= 2 && distance[rx, ry] == distance[x, y])
                    {
                        // 2X2 TODO
                    }
                    continue;
                }
                if (board._blocks[rx + ry * HEIGHT].GetComponent<BlockController>().info.colorType != board._blocks[x + y * HEIGHT].GetComponent<BlockController>().info.colorType) continue;

                distance[x, y] = distance[rx, ry] + 1;
                discovered[x, y] = true;

                HorCount[hf]++;
                VerCount[vf]++;

                matchBlocks.Add(board._blocks[x + y * HEIGHT]);

                q.Enqueue(new CheckNode(x, y, hf, vf));
            }
        }

        for(int i = 0; i < WIDTH*2; i++)
        {
            if (HorCount[i] > 2 || VerCount[i] > 2)
                isMatch = true;
        }

        if(isMatch)
        {
            if (mode == BFSMode.SUFFLE)
                SuffleBoard(distance);
            else if (mode == BFSMode.Match)
                MatchBoard(matchBlocks);

            return true;
        }
        return false;
    }

    void SuffleBoard(int[,] distance)
    {
        for (int i = 0; i < WIDTH; i++)
        {
            for(int j = 0; j < HEIGHT; j++)
            {
                if (distance[i, j] == 2)
                    board._blocks[i + j * HEIGHT].GetComponent<BlockController>().ChangeColor(ColorType.Gray);

                if (distance[i, j] == 3)
                    board._blocks[i + j * HEIGHT].GetComponent<BlockController>().ChangeColor(ColorType.Cyan);
            }
        }
    }

    void MatchBoard(List<BlockController> matchBlocks)
    {
        for (int i = 0; i < matchBlocks.Count; i++)
        {
            StartCoroutine(matchBlocks[i].GetComponent<BlockController>().MatchedBlock());
        }
    }

    IEnumerator SwapBlock()
    {
        Debug.Log("Swap Start");

        ChangeBlockValue(selectedBlock,targetBlock);

        while (isCoroutineAcitve)
            yield return null;

        if(!InitBoardBFS(BFSMode.Match))
        {
            ChangeBlockValue(selectedBlock, targetBlock);
            canSwap = true;
            canSelect = true;
        }
        else
        {
            targetBlock = null;
            selectedBlock = null;
            while (isCoroutineAcitve)
                yield return null;
            StartCoroutine(FillEmptyBlock());
        }
    }
    IEnumerator FillEmptyBlock()
    {
        BlockDown();

        while (isCoroutineAcitve)
            yield return null;

        StartCoroutine(RespawnBlock());
    }

    void BlockDown()
    {
        Debug.Log("BlockDown!!");
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
                if (board._blocks[i + j * HEIGHT].GetComponent<BlockController>().info.blockType == BlockType.BASIC)
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
                        moveDistance[i, (8 - (distance + basicCount))] = distance;
                        basicCount++;
                    }
                }
            }
        }
    }

    void BlockMoveDown(int[,] moveDistance)
    {
        for (int i = WIDTH - 1; i > -1; i--)
        {
            for (int j = HEIGHT - 1; j > -1; j--)
            {
                if (moveDistance[i, j] > 0)
                {
                    Debug.Log("BlockDown");
                    ChangeBlockValue(board._blocks[i + j * HEIGHT].GetComponent<BlockController>(), board._blocks[i + (j + moveDistance[i, j]) * HEIGHT].GetComponent<BlockController>(),true,2);
                }
            }
        }
    }

    IEnumerator RespawnBlock()
    {
        Debug.Log("RespawnBlock!!");
        bool DidRespawn = false;
        for(int i = 0; i < WIDTH; i++)
        {
            int basicCount = 0;
            for (int j = HEIGHT - 1; j > -1; j--)
            {
                if (board._blocks[i + j * HEIGHT].info.blockType == BlockType.EMPTY)
                {

                    board._blocks[i + j * HEIGHT].Respawn(basicCount);
                    DidRespawn = true;
                }
                else
                    basicCount++;
            }
        }

        yield return new WaitForSeconds(1f);

        canSelect = true;
        if (DidRespawn)
        {
            canSelect = false;
            
            InitBoardBFS(BFSMode.Match);
            yield return new WaitForSeconds(0.3f);
            StartCoroutine(FillEmptyBlock());
        }
    }
}
