using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core)
/// Глобальная система локализации на 3 языка для проекта "Алхимический Кот".
/// Обеспечивает перевод интерфейса, синглтон-структуру и поддержку языков для Яндекс Игр (RU, EN, TR).
/// </summary>
public class Translator : MonoBehaviour
{
    public static Translator Instance { get; private set; }
    private static int _languageID = 0; // 0 = RU, 1 = EN, 2 = TR
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
            "Вы уверены?", "Да", "Нет", "Загрузить", "Меню кота", "Золото: ", "Кристаллы: ", "Уровень Кота: ", "Зелья: ", "Настройки",
            "Гардероб Аватарок", "Ур. ", "Выбрано", "Надеть", "Категория Аватарок", "Простые Аватарки (до 100 Ур.)", "Покупные Аватарки (с 5 Ур.)", "Премиум Аватарки (с 3 Ур.)", "Рамки Профиля", "С 5 Ур.", "С 3 Ур.", "Закрыто"
        },
        // 1 - English
        new string[] {
            "Start", "Continue", "Options", "Credits", "Exit", "Sounds", "Music", "Inversion", "Laboratory", "Graphics",
            "Resolution", "Full Screen", "Language", "Loading...", "Overwrite save?", "New Game", "Saved", "Reset", "Controls", "Back",
            "Quality", "Rating", "Shop", "Welcome, Alchemist Cat!", "Slot ", "Select Save Slot", "Overwrite", "(Empty)",
            "Meow! Start brewing?", "Mice caught!", "Upgrade Cauldron", "Recipe Book", "Catch Mice", "Play Darts", "Continue", "Save", "Mice in Barn: ",
            "Very Low", "Low", "Medium", "High", "Very High", "Ultra",
            "Are you sure?", "Yes", "No", "Load", "Cat Menu", "Gold: ", "Crystals: ", "Cat Level: ", "Potions: ", "Settings",
            "Avatar Wardrobe", "Lvl. ", "Selected", "Equip", "Avatar Category", "Free Avatars (up to Lvl 100)", "Shop Avatars (from Lvl 5)", "Premium Avatars (from Lvl 3)", "Profile Frames", "From Lvl 5", "From Lvl 3", "Locked"
        },
        // 2 - Turkish (TR) - Идеально для Яндекс Игр!
        new string[] {
            "Başlat", "Devam Et", "Seçenekler", "Yapımcılar", "Çıkış", "Sesler", "Müzik", "Ters Çevir", "Laboratuvar", "Grafik",
            "Çözünürlük", "Tam Ekran", "Dil", "Yükleniyor...", "Kayıt üzerine yazılsın mı?", "Yeni Oyun", "Kaydedildi", "Sıfırla", "Kontroller", "Geri",
            "Kalite", "Derecelendirme", "Mağaza", "Hoş geldin, Simyacı Kedi!", "Yuva ", "Kayıt Yuvası Seç", "Üzerine Yaz", "(Boş)",
            "Miyav! İksir yapmaya başla?", "Fareler yakalandı!", "Kazanı Geliştir", "Tarif Kitabı", "Fare Yakala", "Dart Oyna", "Devam Et", "Kaydet", "Barn'daki Fareler: ",
            "Çok Düşük", "Düşük", "Orta", "Yüksek", "Çok Yüksek", "Ultra",
            "Emin misiniz?", "Evet", "Hayır", "Yükle", "Kedi Menüsü", "Altın: ", "Kristaller: ", "Kedi Seviyesi: ", "İksirler: ", "Ayarlar",
            "Avatar Gardırobu", "Seviye ", "Seçildi", "Kuşan", "Avatar Kategorisi", "Ücretsiz Avatarlar (100 Seviyeye Kadar)", "Mağaza Avatarları (5. Seviyeden)", "Premium Avatarlar (3. Seviyeden)", "Profil Çerçeveleri", "5. Seviyeden", "3. Seviyeden", "Kilitli"
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
            case 2: return en; // Турецкий мапим на английский для сторонних скриптов без TR локали
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
        text.UIText.textWrappingMode = TextWrappingModes.NoWrap;

        // Если включена опция boldForRussian и активный язык русский, делаем текст жирным (Bold)
        if (text.boldForRussian && _languageID == 0)
        {
            text.UIText.fontStyle = FontStyles.Bold;
        }
        else
        {
            text.UIText.fontStyle = FontStyles.Normal;
        }

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
                text.UIText.characterSpacing = 0f;
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

    public static void TranslateAll()
    {
        Update_texts();
    }
}
