using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro; 
using UnityEngine.SceneManagement; 

public class Translator : MonoBehaviour
{
    public static Translator Instance { get; private set; } 

    private static int _languageID = 1; 
    public static int LanguageID => _languageID; 
    private static List<Transtable_Text> listId = new List<Transtable_Text>();
    private static List<Transtable_Dropdown> listDropdowns = new List<Transtable_Dropdown>();

    public TMP_FontAsset defaultFont; 
    public TMP_FontAsset chineseFont;   // SimHei
    public TMP_FontAsset koreanFont;    // Malgun Gothic / Noto Sans KR

    private static string[,] LineText = 
    {
        #region 0 - Russian
        {
            "Старт", "Продолжить", "Опции", "Разработчик", "Выход", "Громкость звука", "Громкость музыки", "Инверсия мыши", "Чувствительность мыши", "Графика",
            "Разрешение экрана", "На весь экран", "Язык", "Загрузка ...", "Предыдущее сохранение будет перезаписано!", "Начать новую игру", "Игра сохранена", "Перезагрузка", "Управление", "Назад",
            "Качество", "Инверсия мыши", "Чувствительность мыши", "Здравствуй друг...", "Слот ", "Выберите слот...", "(Перезаписать)", "(Пусто)",
            "Привет ..., меня зовут Дружок...", "Мыши все еще бегают...", "Спасибо ..., что помог нам...", "Здравствуй ..., меня зовут Хитрец...", "Я сейчас занят.", "Нажмите E", "Далее", "Сохранить и продолжить", "Мышей поймано: ",
            "Очень Низкое", "Низкое", "Среднее", "Высокое", "Очень Высокое", "Ультра",
            "Вы уверены, что хотите начать новую игру?", "Да", "Нет"
        },
        #endregion
        #region 1 - English
        {
            "Start", "Continue", "Options", "Credits", "Exit", "Sound volume", "Music volume", "Mouse inversion", "Mouse sensitivity", "Graphic", // 0-9
            "Screen resolution", "Full screen", "Language", "Loading ...", "Previous save will be overwritten!", "Start New Game", "Game saved", "Restart", "Controls", "Back", // 10-19
            "Quality", "Mouse Inversion", "Mouse Sensitivity", "Welcome, friend...", "Slot ", "Select Save Slot", "(Overwrite)", "(Empty)", // 20-27
            "Hi ..., my name is Druzhok...", "The mice are still running around!", "Thanks ..., you caught them!", "Hello ..., my name is Khitrets...", "I'm busy right now.", "Press E", "Continue", "Save & Continue", "Mice collected: ", // 28-36
            "Very Low", "Low", "Medium", "High", "Very High", "Ultra", // 37-42
            "Are you sure you want to start a new game?", "Yes", "No" // 43, 44, 45
        },
        #endregion
        #region 2 - Deutsch
        { 
            "Start", "Weiter", "Optionen", "Credits", "Beenden", "Ton", "Musik", "Inversion", "Sensibilität", "Grafik", "Auflösung", "Vollbild", "Sprache", "Laden...", "Speicher überschreiben?", "Neues Spiel", "Gespeichert", "Neustart", "Steuerung", "Zurück", 
            "Qualität", "Maus Inversion", "Sensibilität", "Willkommen...", "Slot ", "Slot wählen", "(Überschreiben)", "(Leer)", 
            "Hallo...", "Mäuse...", "Danke...", "Hallo...", "Besetzt...", "E drücken", "Weiter", "Speichern", "Mäuse: ", 
            "Sehr Niedrig", "Niedrig", "Mittel", "Hoch", "Sehr Hoch", "Ultra", 
            "Sind Sie sicher?", "Ja", "Nein" 
        },
        #endregion
        #region 3 - Français
        { 
            "Démarrer", "Continuer", "Options", "Crédits", "Quitter", "Son", "Musique", "Inversion", "Sensibilité", "Graphiques", "Résolution", "Plein écran", "Langue", "Chargement...", "Écraser?", "Nouvelle partie", "Sauvegardé", "Relancer", "Commandes", "Retour", 
            "Qualité", "Inversion", "Sensibilité", "Bienvenue...", "Slot ", "Slot", "(Écraser)", "(Vide)", 
            "Salut...", "Souris...", "Merci...", "Salut...", "Occupé...", "E", "Continuer", "Sauvegarder", "Souris: ", 
            "Très Bas", "Bas", "Moyen", "Haut", "Très Haut", "Ultra", 
            "Êtes-vous sûr?", "Oui", "Non" 
        },
        #endregion
        #region 4 - Español
        { 
            "Inicio", "Continuar", "Opciones", "Créditos", "Salir", "Sonido", "Música", "Inversión", "Sensibilidad", "Gráficos", "Resolución", "Pantalla completa", "Idioma", "Cargando...", "Sobreescribir?", "Nueva partida", "Guardado", "Reiniciar", "Controles", "Atrás", 
            "Calidad", "Inversión", "Sensibilidad", "Bienvenido...", "Slot ", "Slot", "(Sobreescribir)", "(Vacío)", 
            "Hola...", "Ratones...", "Gracias...", "Hola...", "Ocupado...", "E", "Continuar", "Guardar", "Ratones: ", 
            "Muy Bajo", "Bajo", "Medio", "Alto", "Muy Alto", "Ultra", 
            "¿Estás seguro?", "Sí", "No" 
        },
        #endregion
        #region 5 - Português
        { 
            "Iniciar", "Continuar", "Opções", "Créditos", "Sair", "Som", "Música", "Inversão", "Sensibilidade", "Gráficos", "Resolução", "Tela cheia", "Idioma", "Carregando...", "Sobrescrever?", "Novo jogo", "Gravado", "Reiniciar", "Controles", "Voltar", 
            "Qualidade", "Inversão", "Sensibilidade", "Bem-vindo...", "Slot ", "Slot", "(Sobrescrever)", "(Vazio)", 
            "Olá...", "Ratos...", "Obrigado...", "Olá...", "Ocupado...", "E", "Continuar", "Salvar", "Ratos: ", 
            "Muito Baixo", "Bajo", "Médio", "Alto", "Muito Alto", "Ultra", 
            "Tem certeza?", "Sim", "Não" 
        },
        #endregion
        #region 6 - 日本語
        { 
            "スタート", "続行", "設定", "クレジット", "終了", "音量", "音楽", "反転", "感度", "グラフィック", "解像度", "全画面", "言語", "読み込み中...", "上書きしますか？", "新しく始める", "保存完了", "再開", "操作", "戻る", 
            "品質", "反転", "感度", "ようこそ...", "スロット ", "スロット選択", "(上書き)", "(空き)", 
            "こんにちは...", "ネズ미가...", "ありがとう...", "こんにちは...", "忙しい...", "Eを押す", "次へ", "保存", "ネズミ: ", 
            "最低", "低い", "中程度", "高い", "最高", "ウルトラ", 
            "よろしいですか？", "はい", "いいえ" 
        },
        #endregion
        #region 7 - 한국어
        { 
            "시작", "계속", "옵션", "크레딧", "종료", "사운드", "음악", "반전", "감도", "그래픽", "해상도", "전체 화면", "언어", "로딩 중...", "덮어쓰기?", "새 게임", "저장됨", "재시작", "조작", "뒤로", 
            "품질", "반전", "감도", "환영합니다...", "슬롯 ", "슬롯 선택", "(덮어쓰기)", "(비어있음)", 
            "안녕...", "쥐가...", "고마워...", "안녕...", "바쁨...", "E 누르기", "계속", "저장", "쥐: ", 
            "매우 낮음", "낮음", "중간", "높음", "매우 높음", "울트라", 
            "진짜 시작할까요?", "네", "아니요" 
        },
        #endregion
        #region 8 - 简体中文
        { 
            "开始", "继续", "选项", "制作人员", "退出", "音量", "音乐", "反转", "灵敏度", "画面", "分辨率", "全屏", "语言", "载入中...", "覆盖存档？", "新游戏", "已保存", "重启", "控制", "返回", 
            "质量", "反转", "灵敏度", "欢迎...", "槽位 ", "选择槽位", "(覆盖)", "(空)", 
            "你好...", "老鼠...", "谢谢...", "你好...", "正忙...", "按E", "继续", "保存", "老鼠: ", 
            "极低", "低", "中", "高", "超高", "极高", 
            "你确定吗？", "是", "否" 
        }
        #endregion
    };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this; 
        DontDestroyOnLoad(gameObject); 

        _languageID = PlayerPrefs.GetInt("Language", 0); // 0 = Russian
        Update_texts(); 
    }

    static public void SelectLanguage(int id) 
    {
        _languageID = id; 
        PlayerPrefs.SetInt("Language", _languageID); 
        Update_texts(); 
    }

    static public string GetText(int textKey) 
    {
        if (_languageID >= 0 && _languageID < LineText.GetLength(0))
        {
            if (textKey >= 0 && textKey < LineText.GetLength(1))
            {
                string t = LineText[_languageID, textKey];
                return string.IsNullOrEmpty(t) ? ("ID:" + textKey) : t;
            }
        }
        return "ID:" + textKey; 
    }

    static public void Add(Transtable_Text idtext) { if (!listId.Contains(idtext)) listId.Add(idtext); }
    static public void Delete(Transtable_Text idtext) { listId.Remove(idtext); }
    static public void AddDropdown(Transtable_Dropdown dd) { if (!listDropdowns.Contains(dd)) listDropdowns.Add(dd); }
    static public void DeleteDropdown(Transtable_Dropdown dd) { listDropdowns.Remove(dd); }

    static public void Update_texts() 
    {
        if (Instance == null) return;
        foreach (var text in listId)
        {
            if (text != null && text.UIText != null)
            {
                text.UIText.text = GetText(text.TextID);
                
                // Reset spacing to prevent "vertical" or "thin" look
                text.UIText.characterSpacing = 0;
                text.UIText.wordSpacing = 0;
                text.UIText.lineSpacing = 0;

                // Font mapping
                if (_languageID == 7) // Korean
                {
                    if (Instance.koreanFont != null) text.UIText.font = Instance.koreanFont;
                    else if (Instance.chineseFont != null) text.UIText.font = Instance.chineseFont;
                }
                else if (_languageID == 8 || _languageID == 6) // Chinese or Japanese
                {
                    if (Instance.chineseFont != null) text.UIText.font = Instance.chineseFont;
                }
                else // Russian/European
                {
                    if (Instance.defaultFont != null) text.UIText.font = Instance.defaultFont;
                }

                // Global fallback if everything fails
                if (text.UIText.font == null && Instance.chineseFont != null) 
                    text.UIText.font = Instance.chineseFont;
            }
        }

        foreach (var dd in listDropdowns)
        {
            if (dd != null) dd.UpdateDropdown();
        }
    }
}
