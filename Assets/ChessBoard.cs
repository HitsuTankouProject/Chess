using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using static ChessBlock;

[Serializable]
public class Col
{
    public List<ChessBlock> chessBlocks;
}
[Serializable]
public class ChessObject
{
    public List<GameObject> chessObjects;
}

[Serializable]
public enum ChessAction
{
    Move,
    GotEat
}

public class ChessBoard : MonoBehaviour
{
    public static ChessBoard Instance { get; private set; }
    private PoolManager _poolManager => PoolManager.Instance;
    private InGame _InGame => InGame.Instance;
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

    public List<Col> cols;
    public ChessObject chessPrefab;
    private readonly Vector2Int chessBoard_max = new Vector2Int(8, 8);
    private HashSet<Vector2Int> nowShowing = new HashSet<Vector2Int>();


    public ChessBlock black_KingChessSpawn { get; private set; }
    public ChessBlock white_KingChessSpawn { get; private set; }


    private readonly Dictionary<Vector2Int, Pair<ChessColor, ChessType>> chessStartMap = new()
    {
            { new Vector2Int(0,0), new Pair<ChessColor, ChessType> (ChessColor.White, ChessType.Rook)},
            { new Vector2Int(1,0), new Pair<ChessColor, ChessType> (ChessColor.White, ChessType.Knight )},
            { new Vector2Int(2,0), new Pair<ChessColor, ChessType> (ChessColor.White, ChessType.Bishop)},
            { new Vector2Int(3,0), new Pair<ChessColor, ChessType> (ChessColor.White, ChessType.Queen)},
            { new Vector2Int(4,0), new Pair<ChessColor, ChessType> (ChessColor.White, ChessType.King)},
            { new Vector2Int(5,0), new Pair<ChessColor, ChessType> (ChessColor.White, ChessType.Bishop)},
            { new Vector2Int(6,0), new Pair<ChessColor, ChessType> (ChessColor.White, ChessType.Knight)},
            { new Vector2Int(7,0), new Pair<ChessColor, ChessType> (ChessColor.White, ChessType.Rook)},

            { new Vector2Int(0,1), new Pair<ChessColor, ChessType> (ChessColor.White, ChessType.Pawn )},
            //{ new Vector2Int(1,1), new Pair<ChessColor, ChessType> (ChessColor.White, ChessType.Pawn  )},
            //{ new Vector2Int(2,1), new Pair<ChessColor, ChessType> (ChessColor.White, ChessType.Pawn )},
            //{ new Vector2Int(3,1), new Pair<ChessColor, ChessType> (ChessColor.White, ChessType.Pawn )},
            //{ new Vector2Int(4,1), new Pair<ChessColor, ChessType> (ChessColor.White, ChessType.Pawn )},
            //{ new Vector2Int(5,1), new Pair<ChessColor, ChessType> (ChessColor.White, ChessType.Pawn )},
            //{ new Vector2Int(6,1), new Pair<ChessColor, ChessType> (ChessColor.White, ChessType.Pawn )},
            { new Vector2Int(7,1), new Pair<ChessColor, ChessType> (ChessColor.White, ChessType.Pawn )},

            { new Vector2Int(0,6), new Pair<ChessColor, ChessType> (ChessColor.Black, ChessType.Pawn )},
            //{ new Vector2Int(1,6), new Pair<ChessColor, ChessType> (ChessColor.Black, ChessType.Pawn  )},
            //{ new Vector2Int(2,6), new Pair<ChessColor, ChessType> (ChessColor.Black, ChessType.Pawn )},
            //{ new Vector2Int(3,6), new Pair<ChessColor, ChessType> (ChessColor.Black, ChessType.Pawn )},
            //{ new Vector2Int(4,6), new Pair<ChessColor, ChessType> (ChessColor.Black, ChessType.Pawn )},
            //{ new Vector2Int(5,6), new Pair<ChessColor, ChessType> (ChessColor.Black, ChessType.Pawn )},
            //{ new Vector2Int(6,6), new Pair<ChessColor, ChessType> (ChessColor.Black, ChessType.Pawn )},
            { new Vector2Int(7,6), new Pair<ChessColor, ChessType> (ChessColor.Black, ChessType.Pawn )},

            { new Vector2Int(0,7), new Pair<ChessColor, ChessType> (ChessColor.Black, ChessType.Rook)},
            { new Vector2Int(1,7), new Pair<ChessColor, ChessType> (ChessColor.Black, ChessType.Knight )},
            { new Vector2Int(2,7), new Pair<ChessColor, ChessType> (ChessColor.Black, ChessType.Bishop)},
            { new Vector2Int(3,7), new Pair<ChessColor, ChessType> (ChessColor.Black, ChessType.Queen)},
            { new Vector2Int(4,7), new Pair<ChessColor, ChessType> (ChessColor.Black, ChessType.King)},
            { new Vector2Int(5,7), new Pair<ChessColor, ChessType> (ChessColor.Black, ChessType.Bishop)},
            { new Vector2Int(6,7), new Pair<ChessColor, ChessType> (ChessColor.Black, ChessType.Knight)},
            { new Vector2Int(7,7), new Pair<ChessColor, ChessType> (ChessColor.Black, ChessType.Rook)},

    };

    public Dictionary<Vector2Int, ChessBasic> board { get; private set; } = new Dictionary<Vector2Int, ChessBasic>();

    public Dictionary<Pair<ChessColor, ChessType>, GameObject> chessPrefabDictionary { get; private set; }

    #region Game Initialization
    private bool PrefabDictionaryInit()
    {
        chessPrefabDictionary = new Dictionary<Pair<ChessColor, ChessType>, GameObject>();
        foreach (ChessColor chessColor in Enum.GetValues(typeof(ChessColor)))
        {
            foreach (ChessType chessType in Enum.GetValues(typeof(ChessType)))
            {
                string prefabName = $"Chess_{chessColor}_{chessType}";
                GameObject prefab = chessPrefab.chessObjects.Find(obj => obj.name == prefabName);
                if (prefab != null)
                {
                    chessPrefabDictionary.Add(new Pair<ChessColor, ChessType>(chessColor, chessType), prefab);
                }
                else
                {
                    Debug.LogError($"Prefab not found for {prefabName}");
                    return false;
                }
            }
        }

        return true;
    }

    private void ChessBlockInit()
    {
        for (int x = 0; x < cols.Count; x++)
        {
            for (int y = 0; y < cols[x].chessBlocks.Count; y++)
            {
                cols[x].chessBlocks[y].position = new Vector2Int(x, y);
            }
        }
    }

    private void CheckChessBoardError()
    {
        if (cols.Count != chessBoard_max.x)
        {
            Debug.LogError($"Chess board has incorrect number of columns. Expected: 8, Actual: {cols.Count}");
            return;
        }

        for (int x = 0; x < cols.Count; x++)
        {
            int y = cols[x].chessBlocks.Count;
            if (y != chessBoard_max.y) Debug.LogError($"Column {x} has insufficient number of chess blocks. Expected: 8, Actual: {y}");
            for (y = 0; y < chessBoard_max.y; y++)
            {
                if (cols[x].chessBlocks[y] == null)
                {
                    Debug.LogError($"Chess block at position ({x},{y}) is null.");
                }
                else if (cols[x].chessBlocks[y].name != $"Pos_{x}_{y}")
                {
                    Debug.LogError($"Chess block at position ({x},{y}) has incorrect name. Expected: ChessBlock_{x}_{y}, Actual: {cols[x].chessBlocks[y].name}");
                }
            }

        }
    }

    #endregion

    #region Turn Initialization
    private bool GenChessAtStart()
    {
        board.Clear();
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
                if (block.color == ChessBoardColor.Black) blackChessBlocks.Add(block);
                else whiteChessBlocks.Add(block);
            }
        }

        black_KingChessSpawn = blackChessBlocks[UnityEngine.Random.Range(0, blackChessBlocks.Count)];
        white_KingChessSpawn = whiteChessBlocks[UnityEngine.Random.Range(0, whiteChessBlocks.Count)];

        if (black_KingChessSpawn.color != ChessBoardColor.Black || white_KingChessSpawn.color != ChessBoardColor.White) return false;

        return true;

        // For Debug
        //black_KingChessSpawn.gameObject.SetActive(false);
        //white_KingChessSpawn.gameObject.SetActive(false);
    }

    #endregion

    #region Normal Function
    private bool BoardCheckError(ChessBasic chessBasic)
    {
        if (chessBasic == null)
        {
            Debug.LogError("Chess piece is null.");
            return false;
        }
        if (board[chessBasic.position] != chessBasic)
        {
            Debug.LogError("Chess piece mismatch.");
            return false;
        }
        return true;
    }
    public void GenChess(Vector2Int position, Pair<ChessColor, ChessType> pair)
    {
        if (chessPrefabDictionary[pair] == null)
        {
            Debug.LogError($"Prefab not found for {pair.first}_{pair.second}");
            return;
        }

        GameObject chess = _poolManager.Release(chessPrefabDictionary[pair]);
        chess.transform.position = ReturnChessBlockPosition(position); // Example position, replace with actual logic
        ChessBasic target = chess.GetComponent<ChessBasic>();
        BoardUpdate(target, position, ChessAction.Move);

        //Debug.Log(target.ChessName() + $"___{target.position}");
    }


    public Vector3 ReturnChessBlockPosition(Vector2Int position)
    {
        if (position.x < 0 || position.x >= cols.Count || position.y < 0 || position.y >= cols[position.x].chessBlocks.Count)
        {
            Debug.LogError($"Invalid position: ({position.x},{position.y})");
            return Vector3.zero;
        }
        return cols[position.x].chessBlocks[position.y].transform.position;
    }
    public void BoardUpdate(ChessBasic chessBasic, Vector2Int position, ChessAction chessAction)
    {
        if (chessBasic == null) return;
        switch (chessAction)
        {
            case ChessAction.Move:
                if (board.ContainsKey(chessBasic.position)) board.Remove(chessBasic.position);
                board[position] = chessBasic;
                chessBasic.SetPosition(position);
                break;
            case ChessAction.GotEat:
                board.Remove(chessBasic.position);
                break;

        }

    }

    public void ShowCanGo(HashSet<Vector2Int> canGoPos)
    {
        if (canGoPos.Count == 0) return;
        nowShowing = canGoPos;
        foreach (var pos in canGoPos)
        {
            cols[pos.x].chessBlocks[pos.y].ShowCanGo();
        }
    }

    public void ReSetCanGo()
    {
        if (nowShowing.Count == 0) return;
        foreach (var pos in nowShowing)
        {
            cols[pos.x].chessBlocks[pos.y].ResetNormal();
        }
        nowShowing.Clear();
    }

    #endregion


    public void ChessBoard_Init()
    {
        CheckChessBoardError();
        _poolManager.AllPoolInit();
        ChessBlockInit();
    }

    public IEnumerator ChessBoard_TurnInit()
    {
        bool prefabDictionaryInitSeccess =  PrefabDictionaryInit();
        yield return null;
        bool genChessAtStartSeccess =  GenChessAtStart();
        yield return null;
        bool findRandomKingChessSpawnSeccess = FindRandomKingChessSpawn();
        yield return null;

        if (!prefabDictionaryInitSeccess||!genChessAtStartSeccess||!findRandomKingChessSpawnSeccess)
        {
            Debug.LogError(
                $"PrefabDictionaryInit : {prefabDictionaryInitSeccess}, " +
                $"GenChessAtStart : {genChessAtStartSeccess}, " +
                $"FindRandomKingChessSpawn : {findRandomKingChessSpawnSeccess}");
        }
        

    }




}
