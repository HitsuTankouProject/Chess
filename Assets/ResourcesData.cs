using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum AllBuffCard
{
    None = 0,
    SageKing, MadKing,
    Witcher, Beauty,
    Sorcerer, Monk,
    Rusher, Guardian,
    Charger, Skirmisher,
    Scout, Substitute
}

public class ResourcesData : MonoBehaviour
{
    [Header("Card Materials")]
    public Material m_Card_King_SageKing;
    public Material m_Card_King_MadKing;
    public Material m_Card_Queen_Witcher;
    public Material m_Card_Queen_Beauty;
    public Material m_Card_Bishop_Sorcerer;
    public Material m_Card_Bishop_Monk;
    public Material m_Card_Rook_Rusher;
    public Material m_Card_Rook_Guardian;
    public Material m_Card_Knight_Charger;
    public Material m_Card_Knight_Skirmisher;
    public Material m_Card_Pawn_Scout;
    public Material m_Card_Pawn_Substitute;

    public Material m_CardBack;
    public Dictionary<AllBuffCard, Material> cardMaterialDict {  get; private set; }
    public void CardMaterialDictInit()
    {
        cardMaterialDict = new Dictionary<AllBuffCard, Material>()
            {
                {AllBuffCard.None, null },

                {AllBuffCard.SageKing, m_Card_King_SageKing },
                {AllBuffCard.MadKing, m_Card_King_MadKing },

                {AllBuffCard.Witcher, m_Card_Queen_Witcher },
                {AllBuffCard.Beauty, m_Card_Queen_Beauty },

                {AllBuffCard.Sorcerer, m_Card_Bishop_Sorcerer },
                {AllBuffCard.Monk, m_Card_Bishop_Monk },

                {AllBuffCard.Rusher, m_Card_Rook_Rusher },
                {AllBuffCard.Guardian, m_Card_Rook_Guardian },

                {AllBuffCard.Charger, m_Card_Knight_Charger },
                {AllBuffCard.Skirmisher, m_Card_Knight_Skirmisher },

                {AllBuffCard.Scout, m_Card_Pawn_Scout },
                {AllBuffCard.Substitute, m_Card_Pawn_Substitute }
            };
    }

}
