using System;
using System.Collections.Generic;

namespace Game.Data
{
    // ==========================================
    // データモデルの定義 (Schemas)
    // ==========================================

    [Serializable]
    public class CardDto
    {
        public string card_id;
        public int amount;
    }

    [Serializable]
    public class DeckDto
    {
        public string deck_id;
        public string name;
        public List<string> primary_cards = new List<string>();
        public List<string> secondary_cards = new List<string>();
    }

    [Serializable]
    public class UserGameProfileDto
    {
        // ユーザー名
        public string display_name;
        // チケット (ガチャなど)
        public int ticket;

        // 所持カード一覧
        public List<CardDto> cards = new List<CardDto>();
        // デッキ一覧
        public List<DeckDto> decks = new List<DeckDto>();
        // 現在使用中のデッキID (Optional)
        public string current_deck_id;

        // 管理情報 (基本サーバー側設定だが受信時に使用)
        public string created_at; // DateTime string
        public string updated_at; // DateTime string
    }

    [Serializable]
    public class UserSaveRequestDto
    {
        public UserGameProfileDto profile;
    }

    [Serializable]
    public class UserResponseDto
    {
        public string status;
        public string user_id;
        public UserGameProfileDto profile;
    }

    [Serializable]
    public class StatusResponseDto
    {
        public string status;
        public string updated_at;
    }
}
