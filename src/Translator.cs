using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro; 

public class Translator : MonoBehaviour
{
    public static Translator Instance { get; private set; } 

    private static int _languageID = 0; // 0 = Russian, 1 = English
    public static int LanguageID => _languageID; 
    private static List<Transtable_Text> listId = new List<Transtable_Text>();
    private static List<Transtable_Dropdown> listDropdowns = new List<Transtable_Dropdown>();

    public TMP_FontAsset defaultFont; 
    public TMP_FontAsset chineseFont;   
    public TMP_FontAsset koreanFont;    

    private static string[][] LineText = 
    {
        // 0 - Russian
        new string[] {
            "Старт", "Продолжить", "Опции", "Разработчик", "Выход", "Громкость звука", "Громкость музыки", "Инверсия", "", "Графика",
            "Разрешение", "Весь экран", "Язык", "Загрузка ...", "Слот будет перезаписан!", "Новая игра", "Сохранено", "Рестарт", "Управление", "Назад",
            "Качество", "Инверсия", "", "Приветствую...", "Слот ", "Выбор слота", "(Перезапись)", "(Пусто)",
            "Привет...", "Мыши бегают!", "Спасибо!", "Привет...", "Я занят.", "Нажми E", "Далее", "Сохранить", "Мышей: ",
            "Очень низко", "Низко", "Средне", "Высоко", "Очень высоко", "Ультра",
            "Вы уверены?", "Да", "Нет", "Загрузить"
        },
        // 1 - English
        new string[] {
            "Start", "Continue", "Options", "Credits", "Exit", "Sound Volume", "Music Volume", "Inversion", "", "Graphics",
            "Resolution", "Full Screen", "Language", "Loading ...", "Slot will be overwritten!", "New Game", "Saved", "Restart", "Controls", "Back",
            "Quality", "Inversion", "", "Welcome...", "Slot ", "Select Slot", "(Overwrite)", "(Empty)",
            "Hi...", "Mice are running!", "Thanks!", "Hello...", "I'm busy.", "Press E", "Continue", "Save", "Mice: ",
            "Very Low", "Low", "Medium", "High", "Very High", "Ultra",
            "Are you sure?", "Yes", "No", "Load"
        },
        // 2 - Deutsch (German)
        new string[] { "Start", "Weiter", "Optionen", "Credits", "Beenden", "Ton", "Musik", "Inversion", "", "Grafik", "Auflösung", "Vollbild", "Sprache", "Laden...", "Speichern?", "Neues Spiel", "Gespeichert", "Neustart", "Steuerung", "Zurück", "Qualität", "Inversion", "", "Willkommen...", "Slot ", "Slot wählen", "(Überschreiben)", "(Leer)", "Hallo...", "Mäuse...", "Danke...", "Hallo...", "Besetzt...", "E drücken", "Weiter", "Speichern", "Mäuse: ", "Sehr Niedrig", "Niedrig", "Mittel", "Hoch", "Sehr Hoch", "Ultra", "Sind Sie sicher?", "Ja", "Nein", "Laden" },
        // 3 - Français (French)
        new string[] { "Démarrer", "Continuer", "Options", "Crédits", "Quitter", "Son", "Musique", "Inversion", "", "Graphiques", "Résolution", "Plein écran", "Langue", "Chargement...", "Écraser?", "Nouvelle partie", "Sauvegardé", "Relancer", "Commandes", "Retour", "Qualité", "Inversion", "", "Bienvenue...", "Slot ", "Slot", "(Écraser)", "(Vide)", "Salut...", "Souris...", "Merci...", "Salut...", "Occupé...", "E", "Continuer", "Sauvegarder", "Souris: ", "Très Bas", "Bas", "Moyen", "Haut", "Très Haut", "Ultra", "Êtes-vous sûr?", "Oui", "Non", "Charger" },
        // 4 - Español (Spanish)
        new string[] { "Inicio", "Continuar", "Opciones", "Créditos", "Salir", "Sonido", "Música", "Inversión", "", "Gráficos", "Resolución", "Pantalla completa", "Idioma", "Cargando...", "Sobreescribir?", "Nueva partida", "Guardado", "Reiniciar", "Controles", "Atrás", "Calidad", "Inversión", "", "Bienvenido...", "Slot ", "Slot", "(Sobreescribir)", "(Vacío)", "Hola...", "Ratones...", "Gracias...", "Hola...", "Ocupado...", "E", "Continuar", "Guardar", "Ratones: ", "Muy Bajo", "Bajo", "Medio", "Alto", "Muy Alto", "Ultra", "¿Estás seguro?", "Sí", "No", "Cargar" },
        // 5 - Português (Portuguese)
        new string[] { "Iniciar", "Continuar", "Opções", "Créditos", "Sair", "Som", "Música", "Inversão", "", "Gráficos", "Resolução", "Tela cheia", "Idioma", "Carregando...", "Sobrescrever?", "Novo jogo", "Gravado", "Reiniciar", "Controles", "Voltar", "Qualidade", "Inversão", "", "Bem-vindo...", "Slot ", "Slot", "(Sobrescrever)", "(Vazio)", "Olá...", "Ratos...", "Obrigado...", "Olá...", "Ocupado...", "E", "Continuar", "Salvar", "Ratos: ", "Muito Baixo", "Baixo", "Médio", "Alto", "Muito Alto", "Ultra", "Tem certeza?", "Sim", "Não", "Carregar" },
        // 6 - 日本語 (Japanese)
        new string[] { "スタート", "続行", "設定", "クレジット", "終了", "音量", "音楽", "反転", "", "グラフィック", "解像度", "全画面", "言語", "読み込み中...", "上書きしますか？", "新しく始める", "保存完了", "再開", "操作", "戻る", "品質", "反転", "", "ようこそ...", "スロット ", "スロット選択", "(上書き)", "(空き)", "こんにちは...", "ネズミが...", "ありがとう...", "こんにちは...", "忙しい...", "Eを押す", "次へ", "保存", "ネズミ: ", "最低", "低い", "中程度", "高い", "最高", "ウルトラ", "よろしいですか？", "はい", "いいえ", "ロード" },
        // 7 - 한국어 (Korean)
        new string[] { "시작", "계속", "옵션", "크레딧", "종료", "사운드", "음악", "반전", "", "그래픽", "해상도", "전체 화면", "언어", "로딩 중...", "덮어쓰기?", "새 게임", "저장됨", "재시작", "조작", "뒤로", "품질", "반전", "", "환영합니다...", "슬롯 ", "슬롯 선택", "(덮어쓰기)", "(비어있음)", "안녕...", "쥐가...", "고마워...", "안녕...", "바쁨...", "E 누르기", "계속", "저장", "쥐: ", "매우 낮음", "낮음", "중간", "높음", "매우 낮음", "울트ра", "진짜 시작할까요?", "네", "아니요", "로드" },
        // 8 - 简体中文 (Chinese)
        new string[] { "开始", "继续", "选项", "制作人员", "退出", "音量", "音乐", "反转", "", "画面", "分辨率", "全屏", "语言", "载入中...", "覆盖存档？", "新游戏", "已保存", "重启", "控制", "返回", "质量", "反转", "", "欢迎...", "槽位 ", "选择槽位", "(覆盖)", "(空)", "你好...", "老鼠...", "谢谢...", "你好...", "正忙...", "按E", "继续", "保存", "老鼠: ", "极低", "低", "中", "高", "超高", "极高", "你确定吗？", "是", "否", "加载" }
    };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this; 
        DontDestroyOnLoad(gameObject); 

        _languageID = PlayerPrefs.GetInt("Language", 0); 
        Update_texts(); 
    }

    static public void SelectLanguage(int id) 
    {
        _languageID = id; 
        PlayerPrefs.SetInt("Language", _languageID); 
        Update_texts(); 
    }

    // Alias for compatibility
    static public void ChangeLanguage(int id) => SelectLanguage(id);

    static public string GetText(int textKey) 
    {
        int lang = _languageID;
        int maxLang = LineText.Length;

        if (lang < 0 || lang >= maxLang)
        {
            lang = 1; // Fallback to English to prevent ID:X from ever showing
        }

        if (textKey >= 0 && textKey < LineText[lang].Length)
        {
            return LineText[lang][textKey];
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
                
                // Фикс сдвига и вертикального текста
                text.UIText.characterSpacing = 0;
                text.UIText.wordSpacing = 0;
                text.UIText.lineSpacing = 0;
                text.UIText.textWrappingMode = TextWrappingModes.NoWrap; 
                text.UIText.overflowMode = TextOverflowModes.Overflow; 

                // Font mapping (restores original font for non-asian languages to maintain beautiful styling)
                if (_languageID == 7) 
                { 
                    if (Instance.koreanFont != null) text.UIText.font = Instance.koreanFont; 
                }
                else if (_languageID == 8 || _languageID == 6) 
                { 
                    if (Instance.chineseFont != null) text.UIText.font = Instance.chineseFont; 
                }
                else 
                { 
                    if (text.originalFont != null) text.UIText.font = text.originalFont;
                    else if (Instance.defaultFont != null) text.UIText.font = Instance.defaultFont; 
                }
            }
        }

        foreach (var dd in listDropdowns) { if (dd != null) dd.UpdateDropdown(); }
    }
}
