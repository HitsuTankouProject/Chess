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
    public static ResourcesData Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    [Header("Card Materials")]
    public Pair<Material, Material> m_Card_King_SageKing;
    public Pair<Material, Material> m_Card_King_MadKing;
    public Pair<Material, Material> m_Card_Queen_Witcher;
    public Pair<Material, Material> m_Card_Queen_Beauty;
    public Pair<Material, Material> m_Card_Bishop_Sorcerer;
    public Pair<Material, Material> m_Card_Bishop_Monk;
    public Pair<Material, Material> m_Card_Rook_Rusher;
    public Pair<Material, Material> m_Card_Rook_Guardian;
    public Pair<Material, Material> m_Card_Knight_Charger;
    public Pair<Material, Material> m_Card_Knight_Skirmisher;
    public Pair<Material, Material> m_Card_Pawn_Scout;
    public Pair<Material, Material> m_Card_Pawn_Substitute;
    public Dictionary<AllBuffCard, Pair<Material, Material>> cardMaterialDict;
    public void CardMaterialDictInit()
    {
        cardMaterialDict = new Dictionary<AllBuffCard, Pair<Material, Material>>()
            {
                {AllBuffCard.None, new Pair<Material, Material>(null, null) },

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
