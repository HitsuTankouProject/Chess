using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>チェス盤の1列に含まれるマスを保持します。</summary>
[Serializable]
public class Col
{
    /// <summary>この列を構成するチェス盤のマス一覧です。</summary>
    public List<ChessBlock> chessBlocks;
}
/// <summary>
/// チェス盤のマス、駒、座標、表示エフェクトを一元管理します。
/// 開始配置データからの駒生成、キング生成地点の選択、駒の移動・交換・捕獲、
/// 移動候補表示、呪いマス、生成・消滅・浄化エフェクトを制御します。
/// 各ターン開始時には盤面を再構築し、両プレイヤー用のキング生成地点を設定します。
/// </summary>
public class ChessBoard : MonoBehaviour
{
    /// <summary>チェス盤を管理する共有インスタンスを取得します。</summary>
    public static ChessBoard Instance { get; private set; }
    /// <summary>駒の生成効果音を再生するオーディオ管理オブジェクトを取得します。</summary>
    private AudioManager _audioManager => GameManager.Instance.audioManager;
    /// <summary>駒とエフェクトのオブジェクトプールを取得します。</summary>
    private PoolManager _poolManager => PoolManager.Instance;
    /// <summary>駒モデル、マテリアル、開始配置データを取得します。</summary>
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
    /// <summary>有効な座標が設定されていないことを示す値です。</summary>
    private readonly Vector2Int falseVaule = new Vector2Int(-1, -1);
    /// <summary>黒キングの再生成地点を取得します。</summary>
    public Vector2Int black_KingChessSpawn { get; private set; } = new Vector2Int(-1, -1);
    /// <summary>白キングの再生成地点を取得します。</summary>
    public Vector2Int white_KingChessSpawn { get; private set; } = new Vector2Int(-1, -1);
    /// <summary>黒キングの開始地点です。</summary>
    private Vector2Int black_KingStart = new Vector2Int(-1, -1);
    /// <summary>白キングの開始地点です。</summary>
    private Vector2Int white_KingStart = new Vector2Int(-1, -1);
    /// <summary>指定色のキング開始地点を保存します。</summary>
    /// <param name="color">開始地点を設定するキングの色です。</param>
    /// <param name="pos">キングの開始座標です。</param>
    private void SetKingStartPoint(ChessColor color, Vector2Int pos)
    {
        if (color == ChessColor.White) white_KingStart = pos;
        else black_KingStart = pos;
    }
    /// <summary>指定色のキング開始地点を取得します。</summary>
    /// <param name="color">取得するキングの色です。</param>
    /// <returns>指定色のキング開始座標です。</returns>
    public Vector2Int GetKingStartPoint(ChessColor color)
    {
        if (color == ChessColor.White) return white_KingStart;
        else return black_KingStart;
    }
    /// <summary>指定座標が現在のキング再生成地点か判定します。</summary>
    /// <param name="pos">判定する盤面座標です。</param>
    /// <returns>白または黒キングの再生成地点の場合は <see langword="true" /> です。</returns>
    public bool IsKingChessSpawn(Vector2Int pos) => pos == black_KingChessSpawn || pos == white_KingChessSpawn;
    /// <summary>プレイヤーが現在選択している盤面座標を取得します。</summary>
    public Vector2Int playerChoseBlock { get; private set; } = new Vector2Int(-1, -1);

    [Header("Chess Board")]
    /// <summary>盤面を構成する列の一覧です。</summary>
    public List<Col> cols;
    /// <summary>盤面の横幅と縦幅です。</summary>
    private readonly Vector2Int chessBoard_max = new Vector2Int(8, 8);
    /// <summary>現在、移動・捕獲候補を表示している座標です。</summary>
    private HashSet<Vector2Int> nowShowing = new HashSet<Vector2Int>();
    /// <summary>CSVファイルからターン開始時の駒配置を取得します。</summary>
    /// <param name="mapFileName">拡張子を除いた配置ファイル名です。</param>
    /// <returns>盤面座標と駒情報の対応表です。</returns>
    private Dictionary<Vector2Int, Pair<ChessColor, ChessType>> GetBoardStartMap(string mapFileName)
    {
        string fileName = mapFileName + ".csv";

        return _resourcesData.GetBcoardInitData(fileName);
    }
    /// <summary>指定座標に対応する盤面マスを取得します。</summary>
    /// <param name="targetPos">取得するマスの盤面座標です。</param>
    /// <returns>指定座標の <see cref="ChessBlock" /> です。</returns>
    public ChessBlock ChessBlock(Vector2Int targetPos)
    {
        return cols[targetPos.x].chessBlocks[targetPos.y];
    }
    /// <summary>盤面座標と、その位置に存在する駒の対応表を取得します。</summary>
    public Dictionary<Vector2Int, ChessBasic> board { get; private set; } = new Dictionary<Vector2Int, ChessBasic>();

    /// <summary>すべての盤面マスへ座標を割り当て、表示状態を初期化します。</summary>
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

    #region Turn Initialization
    /// <summary>盤面上の全駒をプールへ返し、駒の対応表を空にします。</summary>
    public void CleanTheBoard()
    {
        if (board.Count > 0)
        {
            foreach (var chess in board.Values)
            {
                if (chess.poolObject != null) chess.poolObject.pool.Return(chess.gameObject);
            }
        }

        board.Clear();
    }
    /// <summary>指定ターンの開始配置データに従って盤面上へ駒を生成します。</summary>
    /// <param name="turn">初期化するターン番号です。4以降は3ターン目の配置を使用します。</param>
    private async UniTask GenChessAtStart(int turn)
    {
        CleanTheBoard();
        int index = turn > 3 ? 3 : turn;
        Dictionary<Vector2Int, Pair<ChessColor, ChessType>> chessStartMap = GetBoardStartMap($"BoardInitData_Turn_0{index}");
        int error = 0;

        // 配置データの各座標へ生成演出を再生してから駒を作成します。
        foreach (var entry in chessStartMap)
        {
            error++;
            await GenChessProcess(entry.Key, entry.Value);
            if (entry.Value.second == ChessType.King)
            {
                SetKingStartPoint(entry.Value.first, entry.Key);
            }
        }

        // 実際に生成された駒が開始配置データと一致することを確認します。
        foreach (Vector2Int pos in board.Keys)
        {
            if (board[pos].chessInfo != chessStartMap[pos])
            {
                Debug.LogError($"GenChessAtStart Failed ");
                return;
            }
        }


       
    }

    /// <summary>中央2列から両色のキング再生成地点をランダムに選択します。</summary>
    private void FindRandomKingChessSpawn()
    {
        List<ChessBlock> blackChessBlocks = new List<ChessBlock>();
        List<ChessBlock> whiteChessBlocks = new List<ChessBlock>();

        // 中央2列のマスを色ごとの候補へ分類します。
        for (int x = 0; x < cols.Count; x++) 
        {
            for (int y = 3; y <= 4; y++)
            {
                ChessBlock block = cols[x].chessBlocks[y];

                if (block.color == ChessColor.Black) blackChessBlocks.Add(block);
                else whiteChessBlocks.Add(block);
            }
        }

        black_KingChessSpawn = blackChessBlocks[UnityEngine.Random.Range(0, blackChessBlocks.Count)].position;
        ChessBlock(black_KingChessSpawn).ChangeBlockStage(BlockStage.KingSpawn);
        white_KingChessSpawn = whiteChessBlocks[UnityEngine.Random.Range(0, whiteChessBlocks.Count)].position;
        ChessBlock(white_KingChessSpawn).ChangeBlockStage(BlockStage.KingSpawn);
    }

    #endregion

    #region Normal Function

    /// <summary>指定色に所属する盤面上の駒だけを取得します。</summary>
    /// <param name="chessColor">抽出する駒の色です。</param>
    /// <returns>指定色の駒を座標ごとに格納した辞書です。</returns>
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
    /// <summary>駒色に対応した盤面上の向きを取得します。</summary>
    /// <param name="color">向きを決定する駒色です。</param>
    /// <returns>白は0度、黒はY軸180度の回転です。</returns>
    private Quaternion ChessRotation(ChessColor color)
    {
        Vector3 angle = Vector3.zero;
        if (color == ChessColor.Black) angle = new Vector3(0, 180, 0);
        return Quaternion.Euler(angle); 
    }
    /// <summary>駒生成時の効果音を再生します。</summary>
    private void PlaySwapnSfx()=> _audioManager.PlaySfx(_resourcesData.sfx_ChessSwapn);
    /// <summary>指定位置、色、種類の駒をプールから取得して盤面へ登録します。</summary>
    /// <param name="position">駒を生成する盤面座標です。</param>
    /// <param name="pair">生成する駒の色と種類です。</param>
    /// <returns>生成して盤面へ登録した駒です。</returns>
    private ChessBasic GenChess(Vector2Int position, Pair<ChessColor, ChessType> pair)
    {
        Vector3 chessGenPos = ReturnChessBlockPosition(position);
        Quaternion chessRotation = ChessRotation(pair.first);
        GameObject chess = _poolManager.Release(_resourcesData.chessModelDict[pair.second].prefab, chessGenPos, chessRotation);

        ChessBasic target = chess.GetComponent<ChessBasic>();
        target.ChangeChessColor(pair.first);
        board.Add(position, target);
        target.SetPosition(position);
        chess.transform.position = ReturnChessBlockPosition(position);
        return target;
    }
    /// <summary>生成エフェクトの完了後に駒を作成し、必要に応じて所有者を設定します。</summary>
    /// <param name="position">駒を生成する盤面座標です。</param>
    /// <param name="pair">生成する駒の色と種類です。</param>
    /// <param name="player">生成した駒を所有するプレイヤーです。</param>
    private async UniTask GenChessProcess(Vector2Int position, Pair<ChessColor, ChessType> pair, Player player = null)
    {
        Vector3 boardPosition = ReturnChessBlockPosition(position);
        GameObject chess = _poolManager.Release(_resourcesData.chessModelDict[pair.second].chessEffect, boardPosition);
        ChessEffect chessEffect = chess.gameObject.GetComponent<ChessEffect>();
        chessEffect.PlayEffect(EffectType.Swapn, _resourcesData.allMaterial.m_ChessHaveExtraLife);
        PlaySwapnSfx();

        // エフェクトが完了してプールへ戻るまで、実際の駒生成を待機します。
        while (!chessEffect.isEffectFinish) await UniTask.Yield();

        ChessBasic chessBasic = GenChess(position, pair);
        

        if (player != null) chessBasic.ChessInit(player);
    }

    /// <summary>駒の非同期生成処理を開始します。</summary>
    /// <param name="position">駒を生成する盤面座標です。</param>
    /// <param name="pair">生成する駒の色と種類です。</param>
    /// <param name="player">生成した駒を所有するプレイヤーです。</param>
    public void StartGenChessProcess(Vector2Int position, Pair<ChessColor, ChessType> pair, Player player = null)
    {
        GenChessProcess(position, pair, player).Forget();
    }
    /// <summary>指定駒の位置へ消滅エフェクトを生成します。</summary>
    /// <param name="targetChess">消滅演出の対象となる駒です。</param>
    public void DeadEffect(ChessBasic targetChess)
    {
        Vector3 boardPosition = ReturnChessBlockPosition(targetChess.position);
        GameObject chess = _poolManager.Release(_resourcesData.chessModelDict[targetChess.type].chessEffect, boardPosition);
        ChessEffect chessEffect = chess.gameObject.GetComponent<ChessEffect>();
        chessEffect.PlayEffect(EffectType.Dead, targetChess.color);
    }
    /// <summary>指定駒の位置へ呪いの浄化エフェクトを生成します。</summary>
    /// <param name="targetChess">浄化演出の対象となる駒です。</param>
    public void PurificEffect(ChessBasic targetChess)
    {
        Vector3 boardPosition = ReturnChessBlockPosition(targetChess.position);
        GameObject chess = _poolManager.Release(_resourcesData.chessModelDict[targetChess.type].chessEffect, boardPosition);
        ChessEffect chessEffect = chess.gameObject.GetComponent<ChessEffect>();
        chessEffect.PlayEffect(EffectType.Dead, _resourcesData.allMaterial.m_GotCurse);
    }
    /// <summary>追加ライフの付与または解除に対応するエフェクトを生成します。</summary>
    /// <param name="targetChess">追加ライフ状態が変化する駒です。</param>
    /// <param name="isGot">追加ライフを付与する場合は <see langword="true" /> です。</param>
    public void IsGotExtraLife(ChessBasic targetChess, bool isGot)
    {
        Vector3 boardPosition = ReturnChessBlockPosition(targetChess.position);
        GameObject chess = _poolManager.Release(_resourcesData.chessModelDict[targetChess.type].chessEffect, boardPosition);
        ChessEffect chessEffect = chess.gameObject.GetComponent<ChessEffect>();
        EffectType effectType = isGot ? EffectType.Swapn : EffectType.Dead;
        chessEffect.PlayEffect(effectType, _resourcesData.allMaterial.m_ChessHaveExtraLife);
    }
    /// <summary>盤面座標に対応するワールド座標を取得します。</summary>
    /// <param name="position">変換する盤面座標です。</param>
    /// <returns>対応するマスのワールド座標です。盤面外の場合は原点です。</returns>
     public Vector3 ReturnChessBlockPosition(Vector2Int position)
    {
        if (IsOutOfBoard(position))
        {
            Debug.LogError($"Invalid position: ({position.x},{position.y})");
            return Vector3.zero;
        }

        return ChessBlock(position).transform.position;
    }
    /// <summary>指定駒を新しい盤面座標へ移動し、辞書とTransformを更新します。</summary>
    /// <param name="chess">移動する駒です。</param>
    /// <param name="moveTo">移動先の盤面座標です。</param>
    public void MoveTo(ChessBasic chess, Vector2Int moveTo)
    {
        if (chess == null || moveTo == new Vector2Int(-1, -1))
        {
            Debug.LogError(chess.gameObject.name + " : " + moveTo);
            return;
        }
        board.Remove(chess.position);
        board[moveTo] = chess;
        chess.SetPosition(moveTo);
        chess.transform.position = ReturnChessBlockPosition(moveTo);


    }
    /// <summary>捕獲された駒を盤面の対応表から削除します。</summary>
    /// <param name="chess">盤面から削除する駒です。</param>
    public void GotEat(ChessBasic chess) => board.Remove(chess.position);
    /// <summary>2つの駒の盤面座標とワールド座標を交換します。</summary>
    /// <param name="aChess">交換する1つ目の駒です。</param>
    /// <param name="bChess">交換する2つ目の駒です。</param>
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
    /// <summary>指定座標へ移動または捕獲候補の表示を適用します。</summary>
    /// <param name="activeStage">表示する候補の種類です。</param>
    /// <param name="chessType">候補位置へ表示する駒の種類です。</param>
    /// <param name="canGoPos">候補表示を適用する盤面座標です。</param>
    public void ShowActive(ChessBlockStage activeStage, ChessType chessType, HashSet<Vector2Int> canGoPos)
    {
        if (canGoPos.Count == 0) return;
        nowShowing.AddRange(canGoPos);
        foreach (var pos in canGoPos) ChessBlock(pos).ChangeChessBlockEffect(activeStage, chessType);
    }
    /// <summary>現在表示中のすべての移動・捕獲候補を通常状態へ戻します。</summary>
    public void ReSetActive()
    {
        if (nowShowing.Count == 0) return;
        foreach (var pos in nowShowing) ChessBlock(pos).ChangeChessBlockEffect(ChessBlockStage.Normal);
        nowShowing.Clear();
    }
    /// <summary>指定座標のマスへ呪いを付与します。</summary>
    /// <param name="position">呪いを付与する盤面座標です。</param>
    /// <param name="chess">呪いを付与した駒です。</param>
    public void CurseTheBlock(Vector2Int position, ChessBasic chess) => ChessBlock(position).ChangeBlockStage(BlockStage.GotCurse, chess);
    /// <summary>指定座標が8×8の盤面外か判定します。</summary>
    /// <param name="position">判定する盤面座標です。</param>
    /// <returns>盤面外の場合は <see langword="true" /> です。</returns>
    public bool IsOutOfBoard(Vector2Int position)
    {
        if (position.x < 0 || position.x >= chessBoard_max.x ||
                   position.y < 0 || position.y >= chessBoard_max.y)
            return true;

        return false;
    }
    /// <summary>盤面上の選択が存在しないことを示す座標です。</summary>
    private readonly Vector2Int invalidPosition = new Vector2Int(-1, -1);
    /// <summary>プレイヤーが選択しているマスを変更し、選択マークを更新します。</summary>
    /// <param name="position">新しく選択する座標です。(-1, -1) で選択を解除します。</param>
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

    /// <summary>
    /// すべてのオブジェクトプールと盤面マスを初期化します。
    /// </summary>

    public void ChessBoard_Init()
    {
        _poolManager.AllPoolInit();
        ChessBlockInit();
    }

    /// <summary>
    /// 指定ターンの開始配置を生成し、キング再生成地点を選択します。
    /// </summary>
    public async UniTask ChessBoard_TurnInit(int turn)
    {
        // 前回のキング生成地点表示を解除します。
        if (black_KingChessSpawn != falseVaule)
            ChessBlock(black_KingChessSpawn).ChangeBlockStage(BlockStage.None);
        if (white_KingChessSpawn != falseVaule)
            ChessBlock(white_KingChessSpawn).ChangeBlockStage(BlockStage.None);

        await GenChessAtStart(turn);
        FindRandomKingChessSpawn();
        await UniTask.Delay(1000);


    }


}
