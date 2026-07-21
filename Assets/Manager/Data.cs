
using System.Collections.Generic;
using UnityEngine;

/// <summary>ゲームで使用する設定データとリソース構造をまとめた名前空間です。</summary>
namespace Data
{
    /// <summary>
    /// 1つのチェス盤開始配置と、解析後の駒情報を保持します。
    /// </summary>
    public class BoardInitData
    {
        /// <summary>CSVから読み込んだ開始配置の各行です。</summary>
        public string[] boardStartData;
        /// <summary>盤面座標と、その位置へ配置する駒の色・種類の対応表です。</summary>
        public Dictionary<Vector2Int, Pair<ChessColor, ChessType>> boardStartMap = new();
    }
    /// <summary>
    /// 言語ごとのゲーム説明CSVデータを一元管理します。
    /// </summary>
    public class AllBoardInitData
    {
        /// <summary>言語と、その言語用のゲーム説明CSV行の対応表を取得します。</summary>
        public Dictionary<Language, string[]> allDescriptionCsvLines { get; private set; } = new();
        /// <summary>指定言語のゲーム説明CSV行を、未登録の場合に追加します。</summary>
        /// <param name="language">登録する説明文の言語です。</param>
        /// <param name="csvLine">CSVから読み込んだ説明文の各行です。</param>
        /// <returns>新しく追加できた場合は <see langword="true" /> です。</returns>
        public bool AddToDescriptionCsvLines(Language language, string[] csvLine)
        {
            // 同じ言語のデータを上書きしないよう、登録済みの場合は失敗とします。
            if (allDescriptionCsvLines.ContainsKey(language)) return false;
            allDescriptionCsvLines[language] = csvLine;
            return true;
        }


    }

    /// <summary>
    /// 1種類のバフカードに必要な画像、素材、表示テキストを保持します。
    /// </summary>
    [System.Serializable]
    public class CardData
    {
        /// <summary>UI上で表示するカード表面の画像です。</summary>
        public Sprite sp_CardCover;
        /// <summary>3Dカード表面へ適用するマテリアルです。</summary>
        public Material m_CardCover;
        /// <summary>カードの表示名を取得または設定します。</summary>
        public string name { get; set; }
        /// <summary>バフレベル1の説明文を取得または設定します。</summary>
        public string buffLevel01Description { get; set; }
        /// <summary>バフレベル2の説明文を取得または設定します。</summary>
        public string buffLevel02Description { get; set; }
        /// <summary>バフレベル3の説明文を取得または設定します。</summary>
        public string buffLevel03Description { get; set; }
    }

    /// <summary>
    /// 全バフカードの表示データとカード裏面素材をまとめて管理します。
    /// </summary>
    [System.Serializable]
    public class AllCradData
    {
        [Header("Buff Card Data")]
        /// <summary>SageKingカードの表示データです。</summary>
        public CardData sageKing;
        /// <summary>MadKingカードの表示データです。</summary>
        public CardData madKing;
        /// <summary>Witcherカードの表示データです。</summary>
        public CardData witcher;
        /// <summary>Beautyカードの表示データです。</summary>
        public CardData beauty;
        /// <summary>Sorcererカードの表示データです。</summary>
        public CardData sorcerer;
        /// <summary>Monkカードの表示データです。</summary>
        public CardData monk;
        /// <summary>Rusherカードの表示データです。</summary>
        public CardData rusher;
        /// <summary>Guardianカードの表示データです。</summary>
        public CardData guardian;
        /// <summary>Chargerカードの表示データです。</summary>
        public CardData charger;
        /// <summary>Skirmisherカードの表示データです。</summary>
        public CardData skirmisher;
        /// <summary>Scoutカードの表示データです。</summary>
        public CardData scout;
        /// <summary>Substituteカードの表示データです。</summary>
        public CardData substitute;

        [Header("Card's Back Materials")]
        /// <summary>UI上で表示するカード裏面の画像です。</summary>
        public Sprite sp_CardBack;
        /// <summary>3Dカード裏面へ適用するマテリアルです。</summary>
        public Material m_CardBack;
        /// <summary>バフカード種別と表示データの対応表です。</summary>
        public Dictionary<AllBuffCard, CardData> cardDataDict = new();
        /// <summary>
        /// すべてのバフカード種別とカード表示データの対応表を再構築します。
        /// </summary>
        public void CardDataDictInit()
        {
            cardDataDict.Clear();

            // 駒種ごとに2種類のバフカードデータを登録します。
            cardDataDict.Add(AllBuffCard.SageKing, sageKing);
            cardDataDict.Add(AllBuffCard.MadKing, madKing);

            cardDataDict.Add(AllBuffCard.Witcher, witcher);
            cardDataDict.Add(AllBuffCard.Beauty, beauty);

            cardDataDict.Add(AllBuffCard.Sorcerer, sorcerer);
            cardDataDict.Add(AllBuffCard.Monk, monk);

            cardDataDict.Add(AllBuffCard.Rusher, rusher);
            cardDataDict.Add(AllBuffCard.Guardian, guardian);

            cardDataDict.Add(AllBuffCard.Charger, charger);
            cardDataDict.Add(AllBuffCard.Skirmisher, skirmisher);

            cardDataDict.Add(AllBuffCard.Scout, scout);
            cardDataDict.Add(AllBuffCard.Substitute, substitute);
        }

    }

    /// <summary>
    /// 1種類のチェス駒に必要なPrefab、モデル、演出、バフ情報を保持します。
    /// </summary>
    [System.Serializable]
    public class ChessData
    {
        /// <summary>盤面へ生成する駒のPrefabです。</summary>
        public GameObject prefab;
        /// <summary>移動候補表示などで使用する駒のメッシュです。</summary>
        public Mesh model;
        [Header("Chess Effect -- Got Eat")]
        /// <summary>駒の生成・捕獲時に使用するエフェクトPrefabです。</summary>
        public GameObject chessEffect;
        /// <summary>この駒種で選択できる1つ目のバフカードを取得します。</summary>
        public AllBuffCard buff01 { get; private set; }
        /// <summary>この駒種で選択できる2つ目のバフカードを取得します。</summary>
        public AllBuffCard buff02 { get; private set; }
        /// <summary>この駒種で選択できる2種類のバフカードを設定します。</summary>
        /// <param name="first">1つ目のバフカードです。</param>
        /// <param name="secondy">2つ目のバフカードです。</param>
        public void SetBuff(AllBuffCard first, AllBuffCard secondy)
        {
            buff01 = first;
            buff02 = secondy;
        }

    }

    /// <summary>
    /// 全駒種のモデルデータと駒種別の検索辞書を管理します。
    /// </summary>
    [System.Serializable]
    public class AllChessModel
    {
        /// <summary>キングのモデルデータです。</summary>
        public ChessData king;
        /// <summary>クイーンのモデルデータです。</summary>
        public ChessData queen;
        /// <summary>ビショップのモデルデータです。</summary>
        public ChessData bishop;
        /// <summary>ルークのモデルデータです。</summary>
        public ChessData rook;
        /// <summary>ナイトのモデルデータです。</summary>
        public ChessData knight;
        /// <summary>ポーンのモデルデータです。</summary>
        public ChessData pawn;
        /// <summary>駒種とモデルデータの対応表を取得します。</summary>
        public Dictionary<ChessType, ChessData> chessModelDict { get; private set; } = new();
        /// <summary>各駒種へバフカードを設定し、駒種とモデルデータの対応表を構築します。</summary>
        public void ChessDataInit()
        {
            // 各駒種で選択できる2種類の専用バフを登録します。
            king.SetBuff(AllBuffCard.SageKing, AllBuffCard.MadKing);
            queen.SetBuff(AllBuffCard.Witcher, AllBuffCard.Beauty);
            bishop.SetBuff(AllBuffCard.Sorcerer, AllBuffCard.Monk);
            rook.SetBuff(AllBuffCard.Rusher, AllBuffCard.Guardian);
            knight.SetBuff(AllBuffCard.Charger, AllBuffCard.Skirmisher);
            pawn.SetBuff(AllBuffCard.Scout, AllBuffCard.Substitute);

            chessModelDict = new()
            {
                [ChessType.King] = king,
                [ChessType.Queen] = queen,
                [ChessType.Bishop] = bishop,
                [ChessType.Rook] = rook,
                [ChessType.Knight] = knight,
                [ChessType.Pawn] = pawn,
            };
        }
    }

    /// <summary>
    /// 盤面、駒、状態エフェクトで共有するすべてのマテリアルを保持します。
    /// </summary>
    [System.Serializable]
    public struct AllMaterial
    {
        [Header("BoardBlock's Chess Material")]
        /// <summary>移動可能マスの駒モデル表示に使用するマテリアルです。</summary>
        public Material m_BoardBlockCanGo;
        /// <summary>捕獲可能マスの駒モデル表示に使用するマテリアルです。</summary>
        public Material m_BoardBlockCanEat;
        [Header("BoardBlock's Effect Material")]
        /// <summary>呪われたマスのパーティクルに使用するマテリアルです。</summary>
        public Material e_BoardBlockGotCurse;
        /// <summary>キング生成地点のパーティクルに使用するマテリアルです。</summary>
        public Material e_BoardBlockKingSpawn;

        [Header("BoardBlock Material")]
        /// <summary>キング生成地点の盤面マスに使用するマテリアルです。</summary>
        public Material m_BoardBlockKingSpawn;
        [Header("Chess Material")]
        /// <summary>追加ライフを持つ駒の表示に使用するマテリアルです。</summary>
        public Material m_ChessHaveExtraLife;
        /// <summary>白い駒へ適用するマテリアルです。</summary>
        public Material m_White;
        /// <summary>黒い駒へ適用するマテリアルです。</summary>
        public Material m_Black;

        [Header("Public Material")]
        /// <summary>駒または盤面マスの呪い表示に使用する共通マテリアルです。</summary>
        public Material m_GotCurse;

    }

    /// <summary>
    /// プレイヤー名や数値表示で使用する共通スプライトを保持します。
    /// </summary>
    [System.Serializable]
    public struct AllSprite
    {
        [Header("Player Name Sprite")]
        /// <summary>白プレイヤーを示す画像です。</summary>
        public Sprite sp_WhiteColor;
        /// <summary>黒プレイヤーを示す画像です。</summary>
        public Sprite sp_BlackColor;
        [Header("Number Sprite")]
        /// <summary>勝利数などの数値表示に使用する画像一覧です。</summary>
        public List<Sprite> sp_NumberSprites;
    }

    /// <summary>
    /// ゲーム内で使用するすべての効果音データを保持します。
    /// </summary>
    [System.Serializable]
    public struct AllSfxData
    {
        /// <summary>ボタン押下時の効果音です。</summary>
        public SfxData sfx_PressButton;
        /// <summary>ゲーム開始時の効果音です。</summary>
        public SfxData sfx_GameStart;
        /// <summary>駒を選択した際の効果音です。</summary>
        public SfxData sfx_PickChess;
        /// <summary>駒を盤面へ置いた際の効果音です。</summary>
        public SfxData sfx_PutChess;
        /// <summary>手番交代時の効果音です。</summary>
        public SfxData sfx_TurnChange;
        /// <summary>リザルト表示時の効果音です。</summary>
        public SfxData sfx_Release;
        /// <summary>表示言語を変更した際の効果音です。</summary>
        public SfxData sfx_ChangeLanguage;
        /// <summary>駒を生成した際の効果音です。</summary>
        public SfxData sfx_ChessSwapn;
    }


}