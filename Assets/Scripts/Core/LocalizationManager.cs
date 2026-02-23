using System;
using System.Collections.Generic;
using UnityEngine;

namespace ComBoom.Core
{
    public static class LocalizationManager
    {
        public static event Action OnLanguageChanged;

        private static string currentLanguage = "en";
        private static Dictionary<string, Dictionary<string, string>> translations;
        private static bool initialized;

        public static string CurrentLanguage => currentLanguage;

        public static readonly string[] LanguageCodes = { "en", "tr", "de", "es", "it", "fr" };
        public static readonly string[] LanguageDisplayNames = { "ENGLISH", "TÜRKÇE", "DEUTSCH", "ESPAÑOL", "ITALIANO", "FRANÇAIS" };

        public static void Init()
        {
            if (initialized) return;
            InitTranslations();
            currentLanguage = PlayerPrefs.GetString("ComBoom_Language", "en");
            initialized = true;
        }

        public static void SetLanguage(string code)
        {
            Init();
            if (currentLanguage == code) return;
            currentLanguage = code;
            PlayerPrefs.SetString("ComBoom_Language", code);
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke();
        }

        public static string Get(string key)
        {
            Init();
            if (translations != null && translations.TryGetValue(key, out var langDict))
            {
                if (langDict.TryGetValue(currentLanguage, out var text))
                    return text;
                if (langDict.TryGetValue("en", out var fallback))
                    return fallback;
            }
            return key.ToUpper();
        }

        public static int GetLanguageIndex()
        {
            Init();
            int idx = System.Array.IndexOf(LanguageCodes, currentLanguage);
            return idx >= 0 ? idx : 0;
        }

        private static void Add(string key, string en, string tr, string de, string es, string it, string fr)
        {
            translations[key] = new Dictionary<string, string>
            {
                ["en"] = en, ["tr"] = tr, ["de"] = de, ["es"] = es, ["it"] = it, ["fr"] = fr
            };
        }

        private static void InitTranslations()
        {
            translations = new Dictionary<string, Dictionary<string, string>>();

            // === GAME UI ===
            Add("score",        "SCORE",        "SKOR",         "PUNKTE",       "PUNTOS",       "PUNTI",        "SCORE");
            Add("best",         "BEST",         "EN İYİ",       "BESTE",        "MEJOR",        "MIGLIORE",     "MEILLEUR");
            Add("best_prefix",  "BEST:",        "EN İYİ:",      "BESTE:",       "MEJOR:",       "MIGLIORE:",    "MEILLEUR:");
            Add("undo",         "UNDO",         "GERİ AL",      "RÜCKGÄNGIG",   "DESHACER",     "ANNULLA",      "ANNULER");
            Add("bomb",         "BOMB",         "BOMBA",        "BOMBE",        "BOMBA",        "BOMBA",        "BOMBE");
            Add("shuffle",      "SHUFFLE",      "KARIŞTIR",     "MISCHEN",      "MEZCLAR",      "MESCOLA",      "MÉLANGER");

            // === GAME OVER ===
            Add("game_over",    "GAME OVER",    "OYUN BİTTİ",  "SPIEL VORBEI", "FIN DEL JUEGO","GIOCO FINITO", "PARTIE TERMINÉE");
            Add("game_over_title","GAME OVER!",  "OYUN BİTTİ!", "SPIEL VORBEI!","¡FIN DEL JUEGO!","GIOCO FINITO!","PARTIE TERMINÉE!");

            // === CONTINUE ===
            Add("continue_watch_ad","WATCH AD & CONTINUE","REKLAM İZLE VE DEVAM ET","WERBUNG ANSEHEN & WEITERSPIELEN","VER ANUNCIO Y CONTINUAR","GUARDA PUBBLICITÀ E CONTINUA","REGARDER PUB ET CONTINUER");
            Add("continue_desc","Clear 2 rows and keep playing","2 satır temizle ve oynamaya devam et","2 Reihen löschen und weiterspielen","Elimina 2 filas y sigue jugando","Cancella 2 righe e continua a giocare","Effacer 2 lignes et continuer");
            Add("skip",         "SKIP",         "GEÇI",        "ÜBERSPRINGEN", "SALTAR",       "SALTA",        "PASSER");

            // === PAUSE MENU ===
            Add("paused",       "PAUSED",       "DURAKLATILDI", "PAUSIERT",     "PAUSADO",      "IN PAUSA",     "EN PAUSE");
            Add("resume",       "RESUME",       "DEVAM ET",     "FORTSETZEN",   "REANUDAR",     "RIPRENDI",     "REPRENDRE");
            Add("restart",      "RESTART",      "YENİDEN",      "NEUSTART",     "REINICIAR",    "RICOMINCIA",   "RECOMMENCER");
            Add("home",         "HOME",         "ANA MENÜ",     "STARTSEITE",   "INICIO",       "HOME",         "ACCUEIL");

            // === MAIN MENU ===
            Add("level",        "LEVEL",        "SEVİYE",       "LEVEL",        "NIVEL",        "LIVELLO",      "NIVEAU");
            Add("play",         "PLAY",         "OYNA",         "SPIELEN",      "JUGAR",        "GIOCA",        "JOUER");
            Add("best_score",   "BEST SCORE",   "EN İYİ SKOR",  "BESTPUNKTZAHL","MEJOR PUNTUACIÓN","MIGLIOR PUNTEGGIO","MEILLEUR SCORE");

            // === SETTINGS ===
            Add("settings",     "SETTINGS",     "AYARLAR",      "EINSTELLUNGEN","AJUSTES",      "IMPOSTAZIONI", "PARAMÈTRES");
            Add("sound",        "SOUND",        "SES",          "TON",          "SONIDO",       "SUONO",        "SON");
            Add("music",        "MUSIC",        "MÜZİK",       "MUSIK",        "MÚSICA",       "MUSICA",       "MUSIQUE");
            Add("vibration",    "VIBRATION",    "TİTREŞİM",    "VIBRATION",    "VIBRACIÓN",    "VIBRAZIONE",   "VIBRATION");
            Add("vibe",         "VIBE",         "TİTREŞİM",    "VIBRATION",    "VIBRACIÓN",    "VIBRAZIONE",   "VIBRATION");
            Add("share_friends","SHARE WITH FRIENDS","ARKADAŞLARINLA PAYLAŞ","MIT FREUNDEN TEILEN","COMPARTIR CON AMIGOS","CONDIVIDI CON AMICI","PARTAGER AVEC DES AMIS");
            Add("language",     "LANGUAGE",     "DİL",          "SPRACHE",      "IDIOMA",       "LINGUA",       "LANGUE");
            Add("terms",        "TERMS OF SERVICE","KULLANIM KOŞULLARI","NUTZUNGSBEDINGUNGEN","TÉRMINOS DE SERVICIO","TERMINI DI SERVIZIO","CONDITIONS D'UTILISATION");
            Add("privacy_policy","PRIVACY POLICY","GİZLİLİK POLİTİKASI","DATENSCHUTZRICHTLINIE","POLÍTICA DE PRIVACIDAD","INFORMATIVA SULLA PRIVACY","POLITIQUE DE CONFIDENTIALITÉ");
            Add("contact_us",   "CONTACT US",   "BİZE ULAŞIN",  "KONTAKTIERE UNS","CONTÁCTENOS","CONTATTACI",  "CONTACTEZ-NOUS");

            // === RANKS ===
            Add("ranks",        "RANKS",        "SIRALAMA",     "RANGLISTE",    "CLASIFICACIÓN","CLASSIFICA",   "CLASSEMENT");
            Add("you",          "You",          "Sen",          "Du",           "Tú",           "Tu",           "Toi");
            Add("your_rank",    "Your Rank",    "Sıralaman",    "Dein Rang",    "Tu Rango",     "Il Tuo Rango", "Ton Rang");
            Add("ranks_no_play_games_title",
                "Google Play Games Required",
                "Google Play Games Gerekli",
                "Google Play Games erforderlich",
                "Se requiere Google Play Games",
                "Google Play Games richiesto",
                "Google Play Games requis");
            Add("ranks_no_play_games_desc",
                "Sign in with Google Play Games to see the leaderboard and compete with other players.",
                "Skor tablosunu görmek ve diğer oyuncularla yarışmak için Google Play Games ile giriş yapın.",
                "Melden Sie sich bei Google Play Games an, um die Bestenliste zu sehen und gegen andere Spieler anzutreten.",
                "Inicia sesión en Google Play Games para ver la clasificación y competir con otros jugadores.",
                "Accedi a Google Play Games per vedere la classifica e competere con altri giocatori.",
                "Connectez-vous à Google Play Games pour voir le classement et rivaliser avec d'autres joueurs.");
            Add("ranks_empty_title",
                "Be the First!",
                "İlk Sen Ol!",
                "Sei der Erste!",
                "¡Sé el Primero!",
                "Sii il Primo!",
                "Soyez le Premier!");
            Add("ranks_empty_desc",
                "No scores on the leaderboard yet. Play a game and set the first record!",
                "Skor tablosunda henüz skor yok. Bir oyun oyna ve ilk rekoru kır!",
                "Noch keine Punkte in der Bestenliste. Spiele ein Spiel und stelle den ersten Rekord auf!",
                "Aún no hay puntuaciones en la clasificación. ¡Juega y establece el primer récord!",
                "Nessun punteggio in classifica. Gioca e stabilisci il primo record!",
                "Pas encore de scores au classement. Jouez et établissez le premier record!");
        }
    }
}
