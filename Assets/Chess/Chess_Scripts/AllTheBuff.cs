using System.Collections.Generic;
using UnityEngine;

/// <summary>キングに適用できるバフの種類を表します。</summary>
public enum KingBuff { None, MadKing, SageKing }
/// <summary>クイーンに適用できるバフの種類を表します。</summary>
public enum QueenBuff { None, Witcher, Beauty }
/// <summary>ビショップに適用できるバフの種類を表します。</summary>
public enum BishopBuff { None, Sorcerer, Monk };
/// <summary>ルークに適用できるバフの種類を表します。</summary>
public enum RookBuff { None, Rusher, Guardian };
/// <summary>ナイトに適用できるバフの種類を表します。</summary>
public enum KnightBuff { None, Charger, Skirmisher }
/// <summary>ポーンに適用できるバフの種類を表します。</summary>
public enum PawnBuff { None, Scout, Substitute }

/// <summary>
/// プレイヤーが使用するすべての駒バフを一元管理します。
/// 駒種ごとの選択状態とバフインスタンスを保持し、初期化、選択、
/// レベルアップ、Guardian の保護範囲更新、Substitute の身代わり判定を行います。
/// </summary>
[System.Serializable]
public class AllTheBuff
{
    #region King
    [Header("King Buff")]
    /// <summary>現在選択されているキングのバフです。</summary>
    public KingBuff kingBuffType;
    /// <summary>MadKing バフの状態と処理を保持します。</summary>
    public MadKing madKing = new MadKing();
    /// <summary>SageKing バフの状態と処理を保持します。</summary>
    public SageKing sageKing = new SageKing();
    /// <summary>指定されたキングバフに対応するインスタンスを取得します。</summary>
    /// <param name="buff">取得するキングバフの種類です。</param>
    /// <returns>対応するバフです。未選択または不明な値の場合は <see langword="null" /> です。</returns>
    private BuffBasic TargetBuff_King(KingBuff buff)
    {
        return buff switch
        {
            KingBuff.MadKing => madKing,
            KingBuff.SageKing => sageKing,
            _ => null
        };
    }

    #endregion

    #region Queen
    [Header("Queen Buff")]
    /// <summary>現在選択されているクイーンのバフです。</summary>
    public QueenBuff queenBuffType;
    /// <summary>Witcher バフの状態と処理を保持します。</summary>
    public Witcher witcher = new Witcher();
    /// <summary>Beauty バフの状態と処理を保持します。</summary>
    public Beauty beauty = new Beauty();
        /// <summary>指定されたクイーンバフに対応するインスタンスを取得します。</summary>
    /// <param name="buff">取得するクイーンバフの種類です。</param>
    /// <returns>対応するバフです。未選択または不明な値の場合は <see langword="null" /> です。</returns>
    private BuffBasic TargetBuff_Queen(QueenBuff buff)
    {
        return buff switch
        {
            QueenBuff.Witcher => witcher,
            QueenBuff.Beauty => beauty,
            _ => null
        };
    }

    #endregion

    #region Bishop
    [Header("Bishop Buff")]
    /// <summary>現在選択されているビショップのバフです。</summary>
    public BishopBuff bishopBuffType;
    /// <summary>Sorcerer バフの状態と処理を保持します。</summary>
    public Sorcerer sorcerer = new Sorcerer();
    /// <summary>Monk バフの状態と処理を保持します。</summary>
    public Monk monk = new Monk();
    /// <summary>指定されたビショップバフに対応するインスタンスを取得します。</summary>
    /// <param name="buff">取得するビショップバフの種類です。</param>
    /// <returns>対応するバフです。未選択または不明な値の場合は <see langword="null" /> です。</returns>
    private BuffBasic TargetBuff_Bishop(BishopBuff buff)
    {
        return buff switch
        {
            BishopBuff.Sorcerer => sorcerer,
            BishopBuff.Monk => monk,
            _ => null
        };
    }
    #endregion

    #region Rook
    [Header("Rook Buff")]
    /// <summary>現在選択されているルークのバフです。</summary>
    public RookBuff rookBuffType;
    /// <summary>Rusher バフの状態と処理を保持します。</summary>
    public Rusher rusher = new Rusher();
    /// <summary>Guardian バフの状態と処理を保持します。</summary>
    public Guardian guardian = new Guardian();
    /// <summary>指定されたルークバフに対応するインスタンスを取得します。</summary>
    /// <param name="buff">取得するルークバフの種類です。</param>
    /// <returns>対応するバフです。未選択または不明な値の場合は <see langword="null" /> です。</returns>
    private BuffBasic TargetBuff_Rook(RookBuff buff)
    {
        return buff switch
        {
            RookBuff.Rusher => rusher,
            RookBuff.Guardian => guardian,
            _ => null
        };
    }

    /// <summary>Guardian によって追加のライフを付与されている駒を取得します。</summary>
    public HashSet<ChessBasic> protectedByGuardianChesses { get; private set; } = new();
    /// <summary>指定した駒を Guardian の保護対象へ追加します。</summary>
    /// <param name="chess">保護対象へ追加する駒です。</param>
    public void AddGuardianProtectedChess(ChessBasic chess)
    {
        if (chess == null) return;
        // 新しく登録された駒にだけ追加ライフを付与します。
        if (protectedByGuardianChesses.Add(chess)) chess.GotExtraLife(true);
    }
    /// <summary>
    /// Guardian の保護を一度解除し、現在のルーク配置に基づいて保護範囲を再計算します。
    /// </summary>
    /// <param name="rookList">盤面上に存在するルークの一覧です。</param>
    public void UpdateGuardianProtectArea(List<ChessBasic> rookList)
    {
        if (rookBuffType != RookBuff.Guardian)
            return;
        // 前回の更新で保護されていた駒から追加ライフを取り除きます。
        foreach (ChessBasic chess in protectedByGuardianChesses)
        {
            if (chess != null)
            {
                chess.GotExtraLife(false);
            }
        }

        protectedByGuardianChesses.Clear();

        // 各ルークから Guardian の効果範囲を再適用します。
        foreach (ChessBasic chess in rookList)
        {
            if (!chess.TryGetComponent(out Rook rook))
            {
                Debug.LogError("NonRook stored in the Rook List");
                continue;
            }
            rook.GuardianBuff();
        }
    }

    #endregion

    #region Knight

    [Header("Knight Buff")]
    /// <summary>現在選択されているナイトのバフです。</summary>
    public KnightBuff knightBuffType;
    /// <summary>Charger バフの状態と処理を保持します。</summary>
    public Charger charger = new Charger();
    /// <summary>Skirmisher バフの状態と処理を保持します。</summary>
    public Skirmisher skirmisher = new Skirmisher();
    /// <summary>指定されたナイトバフに対応するインスタンスを取得します。</summary>
    /// <param name="buff">取得するナイトバフの種類です。</param>
    /// <returns>対応するバフです。未選択または不明な値の場合は <see langword="null" /> です。</returns>
    private BuffBasic TargetBuff_Knight(KnightBuff buff)
    {
        return buff switch
        {
            KnightBuff.Charger => charger,
            KnightBuff.Skirmisher => skirmisher,
            _ => null
        };
    }

    #endregion

    #region Pawn
    [Header("Pawn Buff")]
    /// <summary>現在選択されているポーンのバフです。</summary>
    public PawnBuff pawnBuffType;
    /// <summary>Scout バフの状態と処理を保持します。</summary>
    public Scout scout = new Scout();
    /// <summary>Substitute バフの状態と処理を保持します。</summary>
    public Substitute substitute = new Substitute();
    /// <summary>指定されたポーンバフに対応するインスタンスを取得します。</summary>
    /// <param name="buff">取得するポーンバフの種類です。</param>
    /// <returns>対応するバフです。未選択または不明な値の場合は <see langword="null" /> です。</returns>
    private BuffBasic TargetBuff_Pawn(PawnBuff buff)
    {
        return buff switch
        {
            PawnBuff.Scout => scout,
            PawnBuff.Substitute => substitute,
            _ => null
        };
    }

    /// <summary>
    /// Substitute の効果でキングを守れるか判定し、身代わりになるポーンを選択します。
    /// </summary>
    /// <param name="pawnList">身代わり候補となるポーンの一覧です。</param>
    /// <param name="chess">成功した場合に選択された身代わりの駒です。</param>
    /// <returns>身代わりにできる駒が見つかった場合は <see langword="true" /> です。</returns>
    public bool IsProtectbySubstitute(List<ChessBasic> pawnList, out ChessBasic chess)
    {
        chess = null;

        // 効果が無効、または候補のポーンが存在しない場合は保護できません。
        if (substitute == null|| !substitute.cantKillKingWhenPawnExist
            || pawnList == null || pawnList.Count == 0)
            return false;
        // 候補の中から身代わりになるポーンをランダムに選択します。
        chess = pawnList[Random.Range(0, pawnList.Count)];
        return chess != null;
    }



    #endregion

    /// <summary>カードの種類から対応するバフを取得するための辞書です。</summary>
    public Dictionary<AllBuffCard, BuffBasic> cardBuffMap { get; private set; } = new();
    /// <summary>プレイヤーが選択済みのバフカード一覧です。</summary>
    public List<AllBuffCard> choseBuffs { get; private set; } = new();
    /// <summary>指定した駒種に属する2種類のバフを初期化します。</summary>
    /// <param name="player">バフを所有するプレイヤーです。</param>
    /// <param name="chessType">初期化するバフの対象駒種です。</param>
    /// <param name="buffBasic1">対象駒種に属する1つ目のバフです。</param>
    /// <param name="buffBasic2">対象駒種に属する2つ目のバフです。</param>
    private void BuffInit(Player player,ChessType chessType, BuffBasic buffBasic1, BuffBasic buffBasic2)
    {
        // 渡されたバフと対象駒種の組み合わせが正しいことを確認します。
        if (buffBasic1.buffChess != chessType || buffBasic2.buffChess != chessType)
        {
            Debug.LogError($"{chessType.ToString()} : {buffBasic1.buffName} , {buffBasic2.buffName}");
            return;
        }
        // 対象駒種の選択状態を未選択へ戻します。
        switch (chessType)
        {
            case ChessType.King:    kingBuffType    = KingBuff.None;        break;
            case ChessType.Queen:   queenBuffType   = QueenBuff.None;       break;
            case ChessType.Knight:  knightBuffType  = KnightBuff.None;      break;
            case ChessType.Bishop:  bishopBuffType  = BishopBuff.None;      break;
            case ChessType.Rook:    rookBuffType    = RookBuff.None;        break;
            case ChessType.Pawn:    pawnBuffType    = PawnBuff.None;        break;
        }
        buffBasic1.BuffInit(player);
        buffBasic2.BuffInit(player);
    }
    /// <summary>
    /// すべての駒バフを初期化し、カードとバフの対応表を再構築します。
    /// </summary>
    /// <param name="player">バフを所有するプレイヤーです。</param>
    public void AllTheBuffInit(Player player)
    {
        // 駒種ごとに選択状態とバフ固有の状態を初期化します。
        BuffInit(player,ChessType.King, madKing, sageKing);
        BuffInit(player, ChessType.Queen, witcher, beauty);
        BuffInit(player, ChessType.Knight, charger, skirmisher);
        BuffInit(player, ChessType.Bishop, sorcerer, monk);
        BuffInit(player, ChessType.Rook, rusher, guardian);
        BuffInit(player, ChessType.Pawn, scout, substitute);
        // バフカードから実際のバフインスタンスを取得できるように登録します。
        cardBuffMap = new Dictionary<AllBuffCard, BuffBasic>
        {
            { AllBuffCard.SageKing, sageKing },
            { AllBuffCard.MadKing, madKing },

            { AllBuffCard.Witcher, witcher },
            { AllBuffCard.Beauty, beauty },

            { AllBuffCard.Charger, charger },
            { AllBuffCard.Skirmisher, skirmisher },

            { AllBuffCard.Sorcerer, sorcerer },
            { AllBuffCard.Monk, monk },

            { AllBuffCard.Rusher, rusher },
            { AllBuffCard.Guardian, guardian },

            { AllBuffCard.Scout, scout },
            { AllBuffCard.Substitute, substitute },
        };
        choseBuffs.Clear();
    }

    /// <summary>
    /// 現在選択されているすべての駒バフを1段階上げます。
    /// </summary>
    public void AllBuffLevelUp()
    {
        // 各駒種で現在選択されているバフだけを収集します。
        BuffBasic[] buffs =
        {
            TargetBuff_King(kingBuffType),
            TargetBuff_Queen(queenBuffType),
            TargetBuff_Bishop(bishopBuffType),
            TargetBuff_Rook(rookBuffType),
            TargetBuff_Knight(knightBuffType),
            TargetBuff_Pawn(pawnBuffType),
        };

        foreach (BuffBasic buff in buffs)
        {
            // バフが未選択の駒種は処理を省略します。
            if (buff == null) continue;
            buff.LevelUp(out bool success);
            //Debug.Log(buff.buffName + "'s Level :" + buff.nowBuffLevel);
            if (!success)
            {
                Debug.LogWarning($"{buff.buffName} already at max level.");
                continue;
            }
        }

    }

    /// <summary>
    /// 選択されたバフを有効化し、初期レベルを設定します。
    /// </summary>
    /// <param name="choseBuff">プレイヤーが選択したバフカードです。</param>
    public void ChooseBuff(AllBuffCard choseBuff)
    {
        // 対応するバフを選択済みにし、レベル1まで上げます。
        cardBuffMap[choseBuff].Choose();
        cardBuffMap[choseBuff].LevelUpToTargetLevel(1, out bool success);
        choseBuffs.Add(choseBuff);
        if (!success)
        {
            Debug.LogError(cardBuffMap[choseBuff].buffName + " LevelUp No success");
            return;
        }
    }

}
