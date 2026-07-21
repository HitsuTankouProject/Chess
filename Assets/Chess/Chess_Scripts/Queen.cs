using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// クイーンへ Witcher の能力を付与するバフです。
/// 移動距離を制限する代わりに、レベルに応じて呪いへの耐性、
/// 現在マスへの呪い、移動可能範囲への呪いを開放します。
/// </summary>
public class Witcher : BuffBasic
{
    /// <summary>このバフの対象となる駒種を取得します。</summary>
    public override ChessType buffChess => ChessType.Queen;
    /// <summary>バフの表示名を取得します。</summary>
    public override string buffName => "Witcher";
    /// <summary>プレイヤーが使用するクイーンバフを Witcher に設定します。</summary>
    public override void Choose() => _player.allTheBuff.queenBuffType = QueenBuff.Witcher;
    /// <summary>Witcher 使用中にクイーンが移動できる最大距離を取得します。</summary>
    public int canGoRange { get; private set; } = 2;
    /// <summary>クイーンが呪いを受けない能力が有効かどうかを取得します。</summary>
    public bool cantGotCurse { get; private set; } = false;
    /// <summary>盤面マスへ呪いを付与できるかどうかを取得します。</summary>
    public bool canCurseBlock { get; private set; } = false;
    /// <summary>移動可能範囲へ呪いを付与できるかどうかを取得します。</summary>
    public bool canCurseAllTheBlockCanGo { get; private set; } = false;
    /// <summary>移動可能範囲へ呪いを付与する際の対象座標を保持します。</summary>
    private HashSet<Vector2Int> curseAllTheBlockCanGoPos = new HashSet<Vector2Int>();
    /// <summary>Witcher 固有の能力をすべて無効化します。</summary>
    public override void ResetBuff()
    {
        cantGotCurse = false;
        canCurseBlock = false;
        canCurseAllTheBlockCanGo = false;
    }
    /// <summary>クイーンが呪いを受けない能力を開放します。</summary>
    public override void FirstLevel()
    {
        cantGotCurse = true;
    }
    /// <summary>盤面マスへ呪いを付与する能力を開放します。</summary>
    public override void SecondLevel()
    {
        canCurseBlock = true;
    }
    /// <summary>移動可能範囲へ呪いを付与する能力を開放します。</summary>
    public override void ThirdLevel()
    {
        canCurseAllTheBlockCanGo = true;
    }
    /// <summary>移動可能範囲の呪い対象座標を初期化します。</summary>
    public void CurseAllTheBlockCanGo()
    {
        curseAllTheBlockCanGoPos.Clear();
    }



}

/// <summary>
/// クイーンへ Beauty の能力を付与するバフです。
/// レベルに応じてナイトによる護衛、護衛範囲制限の解除、
/// 捕獲した駒を味方ナイトとして生成する魅了能力を開放します。
/// </summary>
public class Beauty : BuffBasic
{
    /// <summary>このバフの対象となる駒種を取得します。</summary>
    public override ChessType buffChess => ChessType.Queen;
    /// <summary>バフの表示名を取得します。</summary>
    public override string buffName => "Beauty";
    /// <summary>プレイヤーが使用するクイーンバフを Beauty に設定します。</summary>
    public override void Choose() => _player.allTheBuff.queenBuffType = QueenBuff.Beauty;
    /// <summary>ナイトがクイーンの身代わりになれるかどうかを示します。</summary>
    public bool canProtectByKnight = false;
    /// <summary>護衛するナイトの移動範囲条件を解除するかどうかを示します。</summary>
    public bool removeTheAreaLimit = false;
    /// <summary>捕獲した駒を味方ナイトとして魅了できるかどうかを示します。</summary>
    public bool canCharmChess = false;
    /// <summary>Beauty 固有の能力をすべて無効化します。</summary>
    public override void ResetBuff()
    {
        canProtectByKnight = false;
        removeTheAreaLimit = false;
        canCharmChess = false;
    }
    /// <summary>ナイトによるクイーンの護衛を開放します。</summary>
    public override void FirstLevel()
    {
        canProtectByKnight = true;
    }
    /// <summary>護衛するナイトの移動範囲条件を解除します。</summary>
    public override void SecondLevel()
    {
        removeTheAreaLimit = true;
    }
    /// <summary>捕獲した駒を味方ナイトとして生成する魅了能力を開放します。</summary>

    public override void ThirdLevel()
    {
        canCharmChess = true;
    }

}

/// <summary>
/// 縦、横、斜め方向へ移動するクイーンの駒を管理します。
/// Witcher 選択時は呪いへの耐性と盤面マスへの呪いを処理し、
/// Beauty 選択時はナイトによる護衛と捕獲した駒への魅了を処理します。
/// </summary>
public class Queen : ChessBasic
{
    /// <summary>この駒の種類を取得します。</summary>
    public override ChessType type => ChessType.Queen;
    /// <summary>駒の表示名を取得します。</summary>
    /// <returns>クイーンを示す文字列を返します。</returns>
    public override string ChessName() { return "Queen"; }
    /// <summary>選択中のバフに応じた移動探索距離を取得します。</summary>
    public override int findRange
    {
        get
        {
            if(_player.queenBuffType == QueenBuff.Witcher)
                return _player.witcher.canGoRange;
             else return 8;
        }
    }
    /// <summary>クイーンが通常移動できる縦、横、斜めの8方向を取得します。</summary>
    public override HashSet<Vector2Int> directions => new HashSet<Vector2Int>()
    { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };
    /// <summary>この駒へ呪いを適用します。Witcher 選択中は、代わりに現在の呪いを浄化します。</summary>
    public override void CurseThisChess()
    {
        if (_player.queenBuffType == QueenBuff.Witcher) PurifyThisChess();
        else base.CurseThisChess();
    }
    /// <summary>指定駒を捕獲し、選択中のクイーンバフの追加効果を発動します。</summary>
    /// <param name="chess">捕獲する敵駒です。</param>
    public override void EatChess(ChessBasic chess)
    {
        if (chess == null) return;
        // 捕獲前の座標と駒種を、バフ効果で使用するため保持します。
        Vector2Int thisPos = position;
        Vector2Int chessPos = chess.position;
        ChessType chessType = chess.type;

        base.EatChess(chess);

        if (_player.queenBuffType == QueenBuff.Witcher) CurseBlock();
        else if (_player.queenBuffType == QueenBuff.Beauty) CharmChess(chessType, thisPos);

        QueenBuff queenBuff = _player.queenBuffType;
        
    }
    /// <summary>捕獲した駒をナイトとして魅了する基本確率です。</summary>
    private const float canCharmPercent = 50.0f;
    /// <summary>クイーンを身代わりで守れる味方ナイトを検索します。</summary>
    /// <param name="knight">護衛に使用するナイトです。</param>
    /// <returns>護衛できるナイトが見つかった場合は <see langword="true" /> です。</returns>
    private bool CanProtectByKnight(out ChessBasic knight)
    {
        knight = null;

        if (_player.queenBuffType != QueenBuff.Beauty) return false;
        List<ChessBasic> knightList = _player.ChessListByType(ChessType.Knight);
        if (knightList.Count == 0) return false;

        if(_player.beauty.nowBuffLevel == 1)
        {
            // レベル1ではクイーンの位置へ移動可能なナイトだけが護衛できます。
            foreach (ChessBasic chess in knightList)
            {
                HashSet<Vector2Int> knightPossibleMove = chess.PossibleMove(false);

                if (knightPossibleMove.Contains(position))
                {
                    knight = chess;
                    return true;
                }
            }
        }
        else
        {
            // 範囲制限の解除後は、味方ナイトからランダムに護衛役を選択します。
            knight = knightList[Random.Range(0, knightList.Count)];
            return true;
        }
        return false;

    }
    /// <summary>指定ナイトと位置を交換し、クイーンの代わりに捕獲させます。</summary>
    /// <param name="knight">クイーンの身代わりになる味方ナイトです。</param>
    private void ProtectByKnight(ChessBasic knight)
    {
        SwapPosition(knight);
        knight.GotEaten();
    }
    /// <summary>捕獲した駒の種類と確率に応じて、元の位置へ味方ナイトを生成します。</summary>
    /// <param name="chessType">捕獲した駒の種類です。</param>
    /// <param name="spawnKnightPos">魅了したナイトを生成する盤面座標です。</param>
    private void CharmChess(ChessType chessType, Vector2Int spawnKnightPos)
    {
        if (_player.queenBuffType != QueenBuff.Beauty ||
            !_player.beauty.canCharmChess
            || _chessBoard.IsKingChessSpawn(spawnKnightPos)) return;

        // 捕獲対象がナイト以外の場合は、50%の確率で魅了を成功させます。
        if (chessType != ChessType.Knight)
        {
            float isCanCharm = Random.Range(0.0f, 100.0f);
            if (isCanCharm > canCharmPercent) return;
        }

        Pair<ChessColor, ChessType> promotionInfo = new Pair<ChessColor, ChessType>(color, ChessType.Knight);
        _chessBoard.StartGenChessProcess(spawnKnightPos, promotionInfo, _player);


    }
    /// <summary>Witcher の現在レベルに応じて盤面マスへ呪いを付与します。</summary>
    private void CurseBlock()
    {
        if (_player.queenBuffType != QueenBuff.Witcher) return;
        if (_player.witcher.nowBuffLevel == 2)
        {
            _chessBoard.CurseTheBlock(position,this);
        }
        else if(_player.witcher.nowBuffLevel == 3)
        {
            // 移動可能範囲を取得し、各候補について呪い処理を実行します。
            HashSet<Vector2Int> cursePossibleMove = PossibleMove(false);
            foreach (Vector2Int pos in cursePossibleMove)
            {
                _chessBoard.CurseTheBlock(position, this);

            }
        }

    }

    /// <summary>捕獲時にナイトの護衛を試し、護衛できない場合は通常の捕獲処理を行います。</summary>
    public override void GotEaten()
    {
        if (!CanProtectByKnight(out ChessBasic knight)) base.GotEaten();
        else ProtectByKnight(knight);

    }



}
