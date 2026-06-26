using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class Col
{
    public List<ChessBlock> chessBlocks;
}

public class ChessBoard : MonoBehaviour
{
    public static ChessBoard Instance { get; private set; }
    private PoolManager _poolManager => PoolManager.Instance;
    private InGame _InGame => InGame.Instance;
    private ResourcesData _resourcesData => GameManager.Instance.resourcesData;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    [Header("Players")]

    public Vector2Int black_KingChessSpawn { get; private set; } = new Vector2Int(-1, -1);
    public Vector2Int white_KingChessSpawn { get; private set; } = new Vector2Int(-1, -1);
    public bool IsKingChessSpawn(Vector2Int pos) => pos == black_KingChessSpawn || pos == white_KingChessSpawn;


    public Vector2Int playerChoseBlock { get; private set; } = new Vector2Int(-1, -1);

    [Header("Chess Board")]
    public List<Col> cols;
    private readonly Vector2Int chessBoard_max = new Vector2Int(8, 8);
    private HashSet<Vector2Int> nowShowing = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, Pair<ChessColor, ChessType>> GetBoardStartMap(string mapFileName)
    {
        string fileName = mapFileName + ".csv";

        return _resourcesData.GetBcoardInitData(fileName);
    }

    public ChessBlock ChessBlock(Vector2Int targetPos)
    {
        return cols[targetPos.x].chessBlocks[targetPos.y];
    }

    public Dictionary<Vector2Int, ChessBasic> board { get; private set; } = new Dictionary<Vector2Int, ChessBasic>();

    #region Game Initialization

    private void ChessBlockInit()
    {
        for (int x = 0; x < cols.Count; x++)
        {
            for (int y = 0; y < cols[x].chessBlocks.Count; y++)
            {
                cols[x].chessBlocks[y].Init(new Vector2Int(x, y));
            }
        }
    }

    //private void CheckChessBoardError()
    //{
    //    if (cols.Count != chessBoard_max.x)
    //    {
    //        Debug.LogError($"Chess board has incorrect number of columns. Expected: 8, Actual: {cols.Count}");
    //        return;
    //    }

    //    for (int x = 0; x < cols.Count; x++)
    //    {
    //        int y = cols[x].chessBlocks.Count;
    //        if (y != chessBoard_max.y) Debug.LogError($"Column {x} has insufficient number of chess blocks. Expected: 8, Actual: {y}");
    //        for (y = 0; y < chessBoard_max.y; y++)
    //        {
    //            if (cols[x].chessBlocks[y] == null)
    //            {
    //                Debug.LogError($"Chess block at position ({x},{y}) is null.");
    //            }
    //            else if (cols[x].chessBlocks[y].name != $"Pos_{x}_{y}")
    //            {
    //                Debug.LogError($"Chess block at position ({x},{y}) has incorrect name. Expected: ChessBlock_{x}_{y}, Actual: {cols[x].chessBlocks[y].name}");
    //            }
    //        }

    //    }
    //}

    #endregion

    #region Turn Initialization
    private bool GenChessAtStart()
    {
        if (board.Count > 0)
        {
            foreach (var chess in board.Values)
            {
                if (chess.poolObject != null) chess.poolObject.pool.Return(chess.gameObject);
            }
        }

        board.Clear();
        Dictionary<Vector2Int, Pair<ChessColor, ChessType>> chessStartMap = GetBoardStartMap("BoardInitData_Base");
        foreach (var entry in chessStartMap)
        {
            GenChess(entry.Key, entry.Value);
        }

        foreach(Vector2Int pos in board.Keys)
        {
            if (board[pos].chessInfo != chessStartMap[pos]) return false;
        }

        return true;
    }

    private bool FindRandomKingChessSpawn()
    {
        List<ChessBlock> blackChessBlocks = new List<ChessBlock>();
        List<ChessBlock> whiteChessBlocks = new List<ChessBlock>();

        foreach (var col in cols)
        {
            foreach (var block in col.chessBlocks)
            {
                if (block.color == ChessColor.Black) blackChessBlocks.Add(block);
                else whiteChessBlocks.Add(block);
            }
        }

        black_KingChessSpawn = blackChessBlocks[UnityEngine.Random.Range(0, blackChessBlocks.Count)].position;
        ChessBlock(black_KingChessSpawn).ChangeBlockStage(BlockStage.KingSpawn);
        white_KingChessSpawn = whiteChessBlocks[UnityEngine.Random.Range(0, whiteChessBlocks.Count)].position;
        ChessBlock(white_KingChessSpawn).ChangeBlockStage(BlockStage.KingSpawn);

        return true;
    }

    #endregion

    #region Normal Function

    public Dictionary<Vector2Int, ChessBasic> ColorChessDict(ChessColor chessColor)
    {
        Dictionary<Vector2Int, ChessBasic> result = new();
        foreach (Vector2Int pos in board.Keys)
        {
            if (board[pos].color == chessColor)
            {
                result[pos] = board[pos];
            }
        }
        return result;


    }

    //private bool BoardCheckError(ChessBasic chessBasic)
    //{
    //    if (chessBasic == null)
    //    {
    //        Debug.LogError("Chess piece is null.");
    //        return false;
    //    }
    //    if (board[chessBasic.position] != chessBasic)
    //    {
    //        Debug.LogError("Chess piece mismatch.");
    //        return false;
    //    }
    //    return true;
    //}
    public void GenChess(Vector2Int position, Pair<ChessColor, ChessType> pair)
    {
        GameObject chess = _poolManager.Release(_resourcesData.chessModelDict[pair.second].prefab);

        chess.transform.position = ReturnChessBlockPosition(position);
        ChessBasic target = chess.GetComponent<ChessBasic>();
        target.ChangeChessColor(pair.first);
        MoveTo(target, position);

    }
    public void GenChess(Vector2Int position, Pair<ChessColor, ChessType> pair, out ChessBasic genChess)
    {
        genChess = null;
        GameObject chess = _poolManager.Release(_resourcesData.chessModelDict[pair.second].prefab);

        chess.transform.position = ReturnChessBlockPosition(position);
        ChessBasic target = chess.GetComponent<ChessBasic>();
        target.ChangeChessColor(pair.first);
        MoveTo(target, position);

        genChess = target;
    }

    public Vector3 ReturnChessBlockPosition(Vector2Int position)
    {
        if (IsOutOfBoard(position))
        {
            Debug.LogError($"Invalid position: ({position.x},{position.y})");
            return Vector3.zero;
        }

        return ChessBlock(position).transform.position;
    }

    public void MoveTo(ChessBasic chess, Vector2Int moveTo)
    {
        if (chess == null|| moveTo == new Vector2Int(-1, -1))
        {
            Debug.LogError(chess.gameObject.name + " : " + moveTo);
            return;
        }


        board.Remove(chess.position);
        board[moveTo] = chess;
        chess.SetPosition(moveTo);
        chess.transform.position = ReturnChessBlockPosition(moveTo);


    }
    public void GotEat(ChessBasic chess) => board.Remove(chess.position);
    public void Swap(ChessBasic aChess, ChessBasic bChess)
    {
        if(aChess == null || bChess == null)
        {
            Debug.LogError("aChess == null|| bChess == null ");
            return;
        }

        Vector2Int aPos = aChess.position;
        Vector2Int bPos = bChess.position;
        board[aPos] = bChess;
        board[bPos] = aChess;

        aChess.SetPosition(bPos);
        bChess.SetPosition(aPos);

        aChess.transform.position = ReturnChessBlockPosition(bPos);
        bChess.transform.position = ReturnChessBlockPosition(aPos);

    }

    public void ShowActive(ChessBlockStage activeStage, ChessType chessType, HashSet<Vector2Int> canGoPos)
    {
        if (canGoPos.Count == 0) return;
        nowShowing.AddRange(canGoPos);
        foreach (var pos in canGoPos) ChessBlock(pos).ChangeChessBlockEffect(activeStage, chessType);
    }

    public void ReSetActive()
    {
        if (nowShowing.Count == 0) return;
        foreach (var pos in nowShowing) ChessBlock(pos).ChangeChessBlockEffect(ChessBlockStage.Normal);
        nowShowing.Clear();
    }

    public void CurseTheBlock(Vector2Int position, ChessBasic chess)
        => ChessBlock(position).ChangeBlockStage(BlockStage.GotCurse, chess);

    public bool IsOutOfBoard(Vector2Int position)
    {
        if (position.x < 0 || position.x >= chessBoard_max.x ||
                   position.y < 0 || position.y >= chessBoard_max.y)
            return true;

        return false;
    }

    private readonly Vector2Int invalidPosition = new Vector2Int(-1, -1);
    public void UpdatePlayerChose(Vector2Int position)
    {
        bool hasNewChoice = position != invalidPosition;
        bool hasOldChoice = playerChoseBlock != invalidPosition;

        if (hasOldChoice) ChessBlock(playerChoseBlock)?.ShowChoseEffect(false);
        if (hasNewChoice)
        {
            playerChoseBlock = position;
            ChessBlock(playerChoseBlock)?.ShowChoseEffect(true);
        }
        else playerChoseBlock = invalidPosition;
    }


    #endregion


    public void ChessBoard_Init()
    {
        //CheckChessBoardError();
        _poolManager.AllPoolInit();
        ChessBlockInit();
    }

    public IEnumerator ChessBoard_TurnInit()
    {
        bool genChessAtStartSeccess =  GenChessAtStart();
        yield return null;
        bool findRandomKingChessSpawnSeccess = FindRandomKingChessSpawn();
        yield return null;

        if (!genChessAtStartSeccess||!findRandomKingChessSpawnSeccess)
        {
            Debug.LogError(
                $"GenChessAtStart : {genChessAtStartSeccess}, " +
                $"FindRandomKingChessSpawn : {findRandomKingChessSpawnSeccess}");
        }
        
    }




}
