using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core)
/// Глобальная система локализации на 9 языков для проекта "Алхимический Кот".
/// Обеспечивает перевод интерфейса, синглтон-структуру и поддержку азиатских шрифтов.
/// </summary>
public class Translator : MonoBehaviour
{
    public static Translator Instance { get; private set; }

    private static int _languageID = 0; // 0 = RU, 1 = EN, 2 = DE, 3 = FR, 4 = ES, 5 = PT, 6 = JA, 7 = KO, 8 = ZH
    public static int LanguageID
    {
        get { return _languageID; }
        set { _languageID = value; }
    }

    private static List<Transtable_Text> listId = new List<Transtable_Text>();
    private static List<Transtable_Dropdown> listDropdowns = new List<Transtable_Dropdown>();

    [Header("Шрифты Локализации")]
    public TMP_FontAsset defaultFont;
    public TMP_FontAsset chineseFont;
    public TMP_FontAsset koreanFont;

    [Header("Интервалы букв")]
    public float russianCharacterSpacing = -8f;

    // Специфичные строки для Алхимического Кота
    private static string[][] LineText = 
    {
        // 0 - Russian
        new string[] {
            "Старт", "Продолжить", "Опции", "Разработчики", "Выход", "Звуки", "Музыка", "Инверсия", "Лаборатория", "Графика",
            "Разрешение", "Весь экран", "Язык", "Загрузка...", "Перезаписать сейв?", "Новая игра", "Сохранено", "Сброс", "Управление", "Назад",
            "Качество", "Рейтинг", "Магазин", "Привет, Кот-Алхимик!", "Слот ", "Выбор сохранения", "Перезапись", "(Пусто)",
            "Мяу! Начнем варку?", "Мыши пойманы!", "Улучшить котел", "Книга рецептов", "Поймать мышь", "Играть в Дартс", "Далее", "Сохранить", "Мышей в амбаре: ",
            "Очень Низкое", "Низкое", "Среднее", "Высокое", "Очень Высокое", "Ультра",
            "Вы уверены?", "Да", "Нет", "Загрузить", "Меню кота", "Золото: ", "Кристаллы: ", "Уровень Кота: ", "Зелья: "
        },
        // 1 - English
        new string[] {
            "Start", "Continue", "Options", "Credits", "Exit", "Sounds", "Music", "Inversion", "Laboratory", "Graphics",
            "Resolution", "Full Screen", "Language", "Loading...", "Overwrite save?", "New Game", "Saved", "Reset", "Controls", "Back",
            "Quality", "Rating", "Shop", "Welcome, Alchemist Cat!", "Slot ", "Select Save Slot", "Overwrite", "(Empty)",
            "Meow! Start brewing?", "Mice caught!", "Upgrade Cauldron", "Recipe Book", "Catch Mice", "Play Darts", "Continue", "Save", "Mice in Barn: ",
            "Very Low", "Low", "Medium", "High", "Very High", "Ultra",
            "Are you sure?", "Yes", "No", "Load", "Cat Menu", "Gold: ", "Crystals: ", "Cat Level: ", "Potions: "
        },
        // 2 - Deutsch (German)
        new string[] {
            "Start", "Weiter", "Optionen", "Credits", "Beenden", "Töne", "Musik", "Inversion", "Labor", "Grafik",
            "Auflösung", "Vollbild", "Sprache", "Laden...", "Überschreiben?", "Neues Spiel", "Gespeichert", "Zurücksetzen", "Steuerung", "Zurück",
            "Qualität", "Bewertung", "Laden", "Willkommen, Alchemist Cat!", "Slot ", "Slot wählen", "Überschreiben", "(Leer)",
            "Miau! Brauen starten?", "Mäuse gefangen!", "Kessel verbessern", "Rezeptbuch", "Mäuse fangen", "Darts spielen", "Weiter", "Speichern", "Mäuse im Stall: ",
            "Sehr niedrig", "Niedrig", "Mittel", "Hoch", "Sehr hoch", "Ultra",
            "Sind Sie sicher?", "Ja", "Nein", "Laden", "Katzenmenü", "Gold: ", "Kristalle: ", "Katzenstufe: ", "Tränke: "
        },
        // 3 - Français (French)
        new string[] {
            "Démarrer", "Continuer", "Options", "Crédits", "Quitter", "Sons", "Musique", "Inversion", "Laboratoire", "Graphiques",
            "Résolution", "Plein écran", "Langue", "Chargement...", "Écraser?", "Nouvelle partie", "Sauvegardé", "Réinitialiser", "Commandes", "Retour",
            "Qualité", "Classement", "Boutique", "Bienvenue, Chat Alchimiste!", "Slot ", "Choisir un Slot", "Écraser", "(Vide)",
            "Miaou! Brasser?", "Souris attrapées!", "Améliorer chaudron", "Grimoire", "Attraper souris", "Jouer aux fléchettes", "Continuer", "Sauvegarder", "Souris: ",
            "Très bas", "Bas", "Moyen", "Haut", "Très haut", "Ultra",
            "Êtes-vous sûr?", "Oui", "Non", "Charger", "Menu du Chat", "Or: ", "Cristaux: ", "Niveau: ", "Potions: "
        },
        // 4 - Español (Spanish)
        new string[] {
            "Iniciar", "Continuar", "Opciones", "Créditos", "Salir", "Sonidos", "Música", "Inversión", "Laboratorio", "Gráficos",
            "Resolución", "Pantalla completa", "Idioma", "Cargando...", "Sobrescribir?", "Nuevo juego", "Guardado", "Restablecer", "Controles", "Atrás",
            "Calidad", "Clasificación", "Tienda", "¡Bienvenido, Gato Alquimista!", "Slot ", "Elegir ranura", "Sobrescribir", "(Vacío)",
            "¡Miau! ¿Empezamos?", "¡Ratones atrapados!", "Mejorar caldero", "Libro de recetas", "Atrapar ratones", "Jugar dardos", "Continuar", "Guardar", "Ratones: ",
            "Muy bajo", "Bajo", "Medio", "Alto", "Muy alto", "Ultra",
            "¿Está seguro?", "Sí", "No", "Cargar", "Menú de Gato", "Oro: ", "Cristales: ", "Nivel: ", "Pociones: "
        },
        // 5 - Português (Portuguese)
        new string[] {
            "Iniciar", "Continuar", "Opções", "Créditos", "Sair", "Sons", "Música", "Inversão", "Laboratório", "Gráficos",
            "Resolução", "Tela cheia", "Idioma", "Carregando...", "Sobrescrever?", "Novo jogo", "Salvo", "Redefinir", "Controles", "Voltar",
            "Qualidade", "Classificação", "Loja", "Bem-vindo, Gato Alquimista!", "Slot ", "Selecionar Slot", "Sobrescrever", "(Vazio)",
            "Miau! Começar poção?", "Ratos pegos!", "Melhorar caldeirão", "Livro de receitas", "Pegar ratos", "Jogar dardos", "Continuar", "Salvar", "Ratos: ",
            "Muito baixo", "Baixo", "Médio", "Alto", "Muito alto", "Ultra",
            "Tem certeza?", "Sim", "Não", "Carregar", "Menu do Gato", "Ouro: ", "Cristais: ", "Nível: ", "Poções: "
        },
        // 6 - 日本語 (Japanese)
        new string[] {
            "スタート", "再開", "設定", "クレジット", "終了", "効果音", "音楽", "反転", "実験室", "グラフィック",
            "解像度", "全画面", "言語", "ロード中...", "セーブを上書きしますか？", "ニューゲーム", "保存完了", "リセット", "操作", "戻る",
            "品質", "評価", "ショップ", "ようこそ、錬金術師の猫！", "スロット ", "スロット選択", "上書き", "(空き)",
            "ニャー！調合を始めますか？", "ネズミ捕獲完了！", "大釜を強化", "レシピブック", "ネズミ捕り", "ダーツを遊ぶ", "進む", "保存", "ネズミ: ",
            "非常に低い", "低い", "中程度", "高い", "非常に高い", "ウルトラ",
            "本当によろしいですか？", "はい", "いいえ", "ロード", "猫メニュー", "ゴールド: ", "クリスタル: ", "猫のレベル: ", "ポーション: "
        },
        // 7 - 한국어 (Korean)
        new string[] {
            "시작", "계속", "옵션", "크레딧", "종료", "효과음", "음악", "반전", "연구실", "그래픽",
            "해상도", "전체 화면", "언어", "로딩 중...", "덮어쓰기?", "새 게임", "저장됨", "초기화", "조작 방법", "뒤로",
            "화질", "랭킹", "상점", "환영합니다, 연금술사 고양이!", "슬롯 ", "슬롯 선택", "덮어쓰기", "(비어있음)",
            "야옹! 포션을 만들까요?", "쥐를 잡았습니다!", "가마솥 업그레이드", "레시피 북", "쥐 잡기 게임", "다트 게임", "계속", "저장", "쥐 개수: ",
            "매우 낮음", "낮음", "중간", "높음", "매우 높음", "울트라",
            "진짜입니까?", "예", "아니오", "로드", "고양이 메뉴", "골드: ", "크리스탈: ", "고양이 레벨: ", "물약: "
        },
        // 8 - 简体中文 (Chinese)
        new string[] {
            "开始", "继续", "选项", "制作团队", "退出", "音效", "音乐", "反转", "炼金工坊", "画面设置",
            "分辨率", "全屏", "语言", "加载中...", "确定覆盖存档吗？", "新游戏", "已保存", "重置", "控制", "返回",
            "画质", "排行榜", "商店", "欢迎你，炼金猫！", "存档槽 ", "选择存档位", "覆盖", "(空)",
            "喵！开始炼制吗？", "老鼠抓到了！", "升级炼金釜", "配方大全", "抓老鼠", "玩飞镖", "继续", "保存", "仓库里的老鼠: ",
            "极低", "低", "中", "高", "超高", "极高",
            "你确定吗？", "是", "否", "加载", "猫咪菜单", "金币: ", "水晶: ", "猫咪等级: ", "药水: "
        }
    };

    private void Awake()
    {
        if (Instance == null)
        {
            if (gameObject.name != "ALCHEMIST_TRANSLATOR")
            {
                GameObject translatorObject = new GameObject("ALCHEMIST_TRANSLATOR");
                Translator customTranslator = translatorObject.AddComponent<Translator>();
                
                customTranslator.defaultFont = this.defaultFont;
                customTranslator.chineseFont = this.chineseFont;
                customTranslator.koreanFont = this.koreanFont;
                customTranslator.russianCharacterSpacing = this.russianCharacterSpacing;
                
                Instance = customTranslator;
                DontDestroyOnLoad(translatorObject);
                
                _languageID = PlayerPrefs.GetInt("Alchemist_Language", 0);
                Update_texts();
                
                Destroy(this);
                return;
            }

            Instance = this;
            _languageID = PlayerPrefs.GetInt("Alchemist_Language", 0);
            Update_texts();
        }
        else if (Instance != this)
        {
            Instance.defaultFont = this.defaultFont;
            Instance.chineseFont = this.chineseFont;
            Instance.koreanFont = this.koreanFont;
            Instance.russianCharacterSpacing = this.russianCharacterSpacing;
            Update_texts();
            Destroy(this);
        }
    }

    public static void SelectLanguage(int id)
    {
        _languageID = id;
        PlayerPrefs.SetInt("Alchemist_Language", _languageID);
        Update_texts();
    }

    public static string GetText(int textKey)
    {
        int lang = _languageID;
        if (lang < 0 || lang >= LineText.Length) lang = 1;

        if (textKey >= 0 && textKey < LineText[lang].Length)
        {
            return LineText[lang][textKey];
        }
        return "ID:" + textKey;
    }

    public static string GetText9(string ru, string en, string de, string fr, string es, string pt, string ja, string ko, string zh)
    {
        switch (_languageID)
        {
            case 0: return ru;
            case 1: return en;
            case 2: return de;
            case 3: return fr;
            case 4: return es;
            case 5: return pt;
            case 6: return ja;
            case 7: return ko;
            case 8: return zh;
            default: return en;
        }
    }

    public static void Add(Transtable_Text idtext) { if (!listId.Contains(idtext)) listId.Add(idtext); }
    public static void Delete(Transtable_Text idtext) { listId.Remove(idtext); }
    public static void AddDropdown(Transtable_Dropdown dd) { if (!listDropdowns.Contains(dd)) listDropdowns.Add(dd); }
    public static void DeleteDropdown(Transtable_Dropdown dd) { listDropdowns.Remove(dd); }

    public static void FormatText(Transtable_Text text)
    {
        if (text == null || text.UIText == null) return;
        
        text.UIText.text = GetText(text.TextID);
        text.UIText.characterSpacing = 0f;
        text.UIText.wordSpacing = 0f;
        text.UIText.lineSpacing = 0f;

        if (Instance == null) return;

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

            if (_languageID == 0)
            {
                text.UIText.characterSpacing = Instance.russianCharacterSpacing;
            }
        }
    }

    public static void Update_texts()
    {
        if (Instance == null) return;
        foreach (var text in listId)
        {
            if (text != null) FormatText(text);
        }
        foreach (var dd in listDropdowns)
        {
            if (dd != null) dd.UpdateDropdown();
        }
    }
}
