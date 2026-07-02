
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class BoardInitData
    {
        public string[] boardStartData;

        public Dictionary<Vector2Int, Pair<ChessColor, ChessType>> boardStartMap = new();
    }
    public class AllBoardInitData
    {
        public Dictionary<Language, string[]> allDescriptionCsvLines { get; private set; } = new();
        public bool AddToDescriptionCsvLines(Language language, string[] csvLine)
        {
            if(allDescriptionCsvLines.ContainsKey(language)) return false;
            allDescriptionCsvLines[language] = csvLine;
            return true;
        }


    }

    [System.Serializable]
    public class CardData
    {
        public Sprite sp_CardCover;
        public Material m_CardCover;
        public string name { get; set; }
        public string buffLevel01Description { get; set; }
        public string buffLevel02Description { get; set; }
        public string buffLevel03Description { get; set; }
    }

    [System.Serializable]
    public class AllCradData
    {
        [Header("Buff Card Data")]
        public CardData sageKing;
        public CardData madKing;
        public CardData witcher;
        public CardData beauty;
        public CardData sorcerer;
        public CardData monk;
        public CardData rusher;
        public CardData guardian;
        public CardData charger;
        public CardData skirmisher;
        public CardData scout;
        public CardData substitute;

        [Header("Card's Back Materials")]
        public Sprite sp_CardBack;
        public Material m_CardBack;

        public Dictionary<AllBuffCard, CardData> cardDataDict = new();
        public void CardDataDictInit()
        {
            cardDataDict = new Dictionary<AllBuffCard, CardData>()
            {
                // King
                { AllBuffCard.SageKing, sageKing },
                { AllBuffCard.MadKing, madKing },

                // Queen
                { AllBuffCard.Witcher, witcher },
                { AllBuffCard.Beauty, beauty },

                // Bishop
                { AllBuffCard.Sorcerer, sorcerer },
                { AllBuffCard.Monk, monk },

                // Rook
                { AllBuffCard.Rusher, rusher },
                { AllBuffCard.Guardian, guardian },

                // Knight
                { AllBuffCard.Charger, charger },
                { AllBuffCard.Skirmisher, skirmisher },

                // Pawn
                { AllBuffCard.Scout, scout },
                { AllBuffCard.Substitute, substitute },
            };
        }

    }

    [System.Serializable]
    public class ChessData
    {
        public GameObject prefab;
        public Mesh model;
        [Header("Chess Effect -- Got Eat")]
        public GameObject chessEffect;
        public AllBuffCard buff01 { get; private set; }
        public AllBuffCard buff02 { get; private set; }
        public void SetBuff(AllBuffCard first, AllBuffCard secondy)
        {
            buff01 = first;
            buff02 = secondy;
        }

    }
    [System.Serializable]
    public class AllChessModel
    {
        public ChessData king;
        public ChessData queen;
        public ChessData bishop;
        public ChessData rook;
        public ChessData knight;
        public ChessData pawn;

        public Dictionary<ChessType, ChessData> chessModelDict { get; private set; } = new();

        public void ChessDataInit()
        {
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
    [System.Serializable]
    public struct AllMaterial
    {
        [Header("BoardBlock's Chess Material")]
        public Material m_BoardBlockCanGo;
        public Material m_BoardBlockCanEat;
        [Header("BoardBlock's Effect Material")]
        public Material e_BoardBlockGotCurse;
        public Material e_BoardBlockKingSpawn;

        [Header("BoardBlock Material")]
        public Material m_BoardBlockKingSpawn;
        [Header("Chess Material")]
        public Material m_ChessHaveExtraLife;

        [Header("Public Material")]
        public Material m_White;
        public Material m_Black;
        public Material m_GotCurse;

    }
    [System.Serializable]
    public struct AllSprite
    {
        [Header("Player Name Sprite")]
        public Sprite sp_WhiteColor;
        public Sprite sp_BlackColor;
        [Header("Button Sprite")]
        public Sprite sp_Confirm;
        public Sprite sp_Return;
        public Sprite sp_canDraw;
        public Sprite sp_cantDraw;
        public Sprite sp_Ready;
        public Sprite sp_NonReady;
    }

}