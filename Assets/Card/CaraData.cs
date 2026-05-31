using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaraData
{
    public enum AllBuffCara
    {
        None = 0,
        SageKing, MadKing,
        Witcher, Beauty,
        Sorcerer, Monk,
        Rusher, Guardian,
        Charger, Skirmisher,
        Scout, Substitute
    }

    [Serializable]
    public static class CaraData
    {
        #region King_Material
        private static bool card_King_Material_Init = false;
        public static Material m_Card_King_Back { get; private set; }
        public static Material m_Card_King_Front_SageKing { get; private set; }
        public static Material m_Card_King_Front_MadKing { get; private set; }
        private static void Card_King_Material_Init()
        {
            if (card_King_Material_Init) return;
            card_King_Material_Init = true;
            m_Card_King_Back = Resources.Load<Material>("Card_Materials/m_Card_King_Back");
            m_Card_King_Front_SageKing = Resources.Load<Material>("Card_Materials/m_Card_King_Front_SageKing");
            m_Card_King_Front_MadKing = Resources.Load<Material>("Card_Materials/m_Card_King_Front_MadKing");
        }
        #endregion

        #region Queen_Material
        private static bool card_Queen_Material_Init = false;
        public static Material m_Card_Queen_Back { get; private set; }
        public static Material m_Card_Queen_Front_Witcher { get; private set; }
        public static Material m_Card_Queen_Front_Beauty { get; private set; }
        private static void Card_Queen_Material_Init()
        {
            if (card_Queen_Material_Init) return;
            card_Queen_Material_Init = true;
            m_Card_Queen_Back = Resources.Load<Material>("Card_Materials/m_Card_Queen_Back");
            m_Card_Queen_Front_Witcher = Resources.Load<Material>("Card_Materials/m_Card_Queen_Front_Witcher");
            m_Card_Queen_Front_Beauty = Resources.Load<Material>("Card_Materials/m_Card_Queen_Front_Beauty");
        }
        #endregion

        #region Bishop_Material
        private static bool card_Bishop_Material_Init = false;
        public static Material m_Card_Bishop_Back { get; private set; }
        public static Material m_Card_Bishop_Front_Sorcerer { get; private set; }
        public static Material m_Card_Bishop_Front_Monk { get; private set; }
        private static void Card_Bishop_Material_Init()
        {
            if (card_Bishop_Material_Init) return;
            card_Bishop_Material_Init = true;
            m_Card_Bishop_Back = Resources.Load<Material>("Card_Materials/m_Card_Bishop_Back");
            m_Card_Bishop_Front_Sorcerer = Resources.Load<Material>("Card_Materials/m_Card_Bishop_Front_Sorcerer");
            m_Card_Bishop_Front_Monk = Resources.Load<Material>("Card_Materials/m_Card_Bishop_Front_Monk");
        }
        #endregion

        #region Rook_Material
        private static bool card_Rook_Material_Init = false;
        public static Material m_Card_Rook_Back { get; private set; }
        public static Material m_Card_Rook_Front_Rusher { get; private set; }
        public static Material m_Card_Rook_Front_Guardian { get; private set; }
        private static void Card_Rook_Material_Init()
        {
            if (card_Rook_Material_Init) return;
            card_Rook_Material_Init = true;
            m_Card_Rook_Back = Resources.Load<Material>("Card_Materials/m_Card_Rook_Back");
            m_Card_Rook_Front_Rusher = Resources.Load<Material>("Card_Materials/m_Card_Rook_Front_Rusher");
            m_Card_Rook_Front_Guardian = Resources.Load<Material>("Card_Materials/m_Card_Rook_Front_Guardian");
        }
        #endregion

        

        #region Knight_Material
        private static bool card_Knight_Material_Init = false;
        public static Material m_Card_Knight_Back { get; private set; }
        public static Material m_Card_Knight_Front_Charger { get; private set; }
        public static Material m_Card_Knight_Front_Skirmisher { get; private set; }
        private static void Card_Knight_Material_Init()
        {
            if (card_Knight_Material_Init) return;
            card_Knight_Material_Init = true;
            m_Card_Knight_Back = Resources.Load<Material>("Card_Materials/m_Card_Knight_Back");
            m_Card_Knight_Front_Charger = Resources.Load<Material>("Card_Materials/m_Card_Knight_Front_Charger");
            m_Card_Knight_Front_Skirmisher = Resources.Load<Material>("Card_Materials/m_Card_Knight_Front_Skirmisher");
        }
        #endregion

        #region Pawn_Material
        private static bool card_Pawn_Material_Init = false;
        public static Material m_Card_Pawn_Back { get; private set; }
        public static Material m_Card_Pawn_Front_Scout { get; private set; }
        public static Material m_Card_Pawn_Front_Substitute { get; private set; }
        private static void Card_Pawn_Material_Init()
        {
            if (card_Pawn_Material_Init) return;
            card_Pawn_Material_Init = true;
            m_Card_Pawn_Back = Resources.Load<Material>("Card_Materials/m_Card_Pawn_Back");
            m_Card_Pawn_Front_Scout = Resources.Load<Material>("Card_Materials/m_Card_Pawn_Front_Scout");
            m_Card_Pawn_Front_Substitute = Resources.Load<Material>("Card_Materials/m_Card_Pawn_Front_Substitute");
        }
        #endregion
        public static bool isCardMaterialInit { get; private set; } = false;
        public static Dictionary<AllBuffCara, Pair<Material, Material>> cardMaterialDict;
        public static void CardMaterialDictInit()
        {
            cardMaterialDict = new Dictionary<AllBuffCara, Pair<Material, Material>>()
            {
                 {AllBuffCara.None, new Pair<Material, Material>(null, null) },

                {AllBuffCara.SageKing, new Pair<Material, Material>(m_Card_King_Front_SageKing, m_Card_King_Back) },
                {AllBuffCara.MadKing, new Pair<Material, Material>(m_Card_King_Front_MadKing, m_Card_King_Back) },

                {AllBuffCara.Witcher, new Pair<Material, Material>(m_Card_Queen_Front_Witcher, m_Card_Queen_Back) },
                {AllBuffCara.Beauty, new Pair<Material, Material>(m_Card_Queen_Front_Beauty, m_Card_Queen_Back) },

                {AllBuffCara.Sorcerer, new Pair<Material, Material>(m_Card_Bishop_Front_Sorcerer, m_Card_Bishop_Back) },
                {AllBuffCara.Monk, new Pair<Material, Material>(m_Card_Bishop_Front_Monk, m_Card_Bishop_Back) },

                {AllBuffCara.Rusher, new Pair<Material, Material>(m_Card_Rook_Front_Rusher, m_Card_Rook_Back) },
                {AllBuffCara.Guardian, new Pair<Material, Material>(m_Card_Rook_Front_Guardian, m_Card_Rook_Back) },

                {AllBuffCara.Charger, new Pair<Material, Material>(m_Card_Knight_Front_Charger, m_Card_Knight_Back) },
                {AllBuffCara.Skirmisher, new Pair<Material, Material>(m_Card_Knight_Front_Skirmisher, m_Card_Knight_Back) },

                {AllBuffCara.Scout, new Pair<Material, Material>(m_Card_Pawn_Front_Scout, m_Card_Pawn_Back) },
                {AllBuffCara.Substitute, new Pair<Material, Material>(m_Card_Pawn_Front_Substitute, m_Card_Pawn_Back) }

            };
        }

        public static void CaraDataInit()
        {
            if(isCardMaterialInit) return;
            isCardMaterialInit = true;

            Card_King_Material_Init();
            Card_Queen_Material_Init();
            Card_Rook_Material_Init();
            Card_Bishop_Material_Init();
            Card_Knight_Material_Init();
            Card_Pawn_Material_Init();

            CardMaterialDictInit();
        }


    }
}
