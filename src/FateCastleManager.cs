using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Разработчик: Fate Continent (Континент Судьбы)
/// Zenith Glassmorphism Design System (8K Ultra-High Density)
/// Castle Progression and Income Management System.
/// Coordinates with SaveGameSystem.CurrentData.gold and persists state.
/// </summary>
public class FateCastleManager : MonoBehaviour
{
    public static FateCastleManager Instance { get; private set; }

    [System.Serializable]
    public class CastleInstance
    {
        public int zoneIndex;
        public string nameRU;
        public string nameEN;
        public string owner; // "Player" or "Enemy"
        public int level = 1; // 1 to 5
        public float goldAccumulated;
        [System.NonSerialized] public GameObject visualRoot;
    }

    public List<CastleInstance> castles = new List<CastleInstance>();
    
    // UI Panels tracking
    private bool isDetailsOpen = false;
    private int activeDetailsIndex = -1;
    private string feedbackMessage = "";
    private float messageTimer = 0f;

    private float passiveIncomeTimer = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeCastleStates();
    }

    private void Start()
    {
        // Каждую секунду пассивный доход
        StartCoroutine(PassiveIncomeRoutine());
    }

    private void Update()
    {
        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f)
            {
                feedbackMessage = "";
            }
        }

        // Клик по замкам через луч из камеры
        HandleCastleClicks();
    }

    private void InitializeCastleStates()
    {
        castles.Clear();

        // 4 зоны нашего континента
        string[] zonesRU = { "Кровавые Пустоши", "Ледяной Пик", "Древние Руины", "Святилище Зенита" };
        string[] zonesEN = { "Crimson Wastes", "Ice-Bound Peak", "Ancient Ruins", "Zenith Sanctuary" };

        for (int i = 0; i < 4; i++)
        {
            CastleInstance castle = new CastleInstance
            {
                zoneIndex = i,
                nameRU = zonesRU[i],
                nameEN = zonesEN[i],
                level = PlayerPrefs.GetInt("Castle_Level_" + i, 1),
                owner = PlayerPrefs.GetString("Castle_Owner_" + i, "Enemy")
            };
            castles.Add(castle);
        }
    }

    /// <summary>
    /// Автоматически спавнит 3D замки на тактической карте
    /// </summary>
    public void SpawnAllCastles()
    {
        Debug.Log("<color=#00FFCC>[CASTLE MGR]</color> Генерация 3D объектов замков в тактическом 3D-пространстве...");
        
        int playerZone = 0;
        if (DialogueSystem_Manager.Instance != null)
        {
            playerZone = DialogueSystem_Manager.Instance.selectedZoneIndex;
        }

        LandingPositionManager lpm = LandingPositionManager.Instance;
        if (lpm == null)
        {
            Debug.LogError("[CASTLE MGR] LandingPositionManager не найден. Не могу спавнить замки!");
            return;
        }

        for (int i = 0; i < castles.Count; i++)
        {
            CastleInstance castle = castles[i];
            
            // Если игрок выбрал эту точку в качестве точки высадки, замок принадлежит ему!
            if (i == playerZone)
            {
                castle.owner = "Player";
            }
            else
            {
                castle.owner = "Enemy";
            }

            // Пересохраняем владельца
            PlayerPrefs.SetString("Castle_Owner_" + i, castle.owner);
            PlayerPrefs.Save();

            // Если visual уже создан, уничтожаем старый перед повторным созданием
            if (castle.visualRoot != null)
            {
                Destroy(castle.visualRoot);
            }

            if (i >= lpm.landingPoints.Length || lpm.landingPoints[i].spawnAnchor == null)
            {
                continue;
            }

            Transform anchor = lpm.landingPoints[i].spawnAnchor;
            
            // Спавним замок с небольшим смещением, чтобы он стоял красивой ратушей рядом с лагерем высадки героя!
            Vector3 spawnPos = anchor.position + new Vector3(2.5f, 0f, 2.5f);
            
            // Находим высоту террейна, чтобы замок не парил
            RaycastHit hit;
            if (Physics.Raycast(spawnPos + Vector3.up * 50f, Vector3.down, out hit, 100f))
            {
                spawnPos.y = hit.point.y;
            }

            // Создаем корневой объект
            GameObject root = new GameObject("Castle_" + i);
            root.transform.position = spawnPos;
            root.transform.rotation = Quaternion.identity;

            // Навешиваем скрипт клика
            InteractiveCastle ic = root.AddComponent<InteractiveCastle>();
            ic.zoneIndex = i;

            // Навешиваем BoxCollider на корень всей композиции
            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 1.25f, 0f);
            col.size = new Vector3(2.2f, 3.0f, 2.2f);

            // Безопасно настраиваем совместимый шейдер без "розового" материала
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("URP/Lit") ?? Shader.Find("Standard");
            Material castleMat = new Material(urpShader);
            
            // Игрок - сочно изумрудный неон, Враг - глубокий неоновый рубин
            if (castle.owner == "Player")
            {
                castleMat.color = new Color(0.1f, 0.9f, 0.4f, 1.0f);
            }
            else
            {
                castleMat.color = new Color(0.9f, 0.1f, 0.3f, 1.0f);
            }

            if (castleMat.HasProperty("_Glossiness")) castleMat.SetFloat("_Glossiness", 0.6f);
            if (castleMat.HasProperty("_Smoothness")) castleMat.SetFloat("_Smoothness", 0.6f);
            if (castleMat.HasProperty("_Metallic")) castleMat.SetFloat("_Metallic", 0.3f);

            // 1. Центральная Квадратная Башня
            GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(tower.GetComponent<BoxCollider>()); // Убираем лишние коллайдеры
            tower.transform.SetParent(root.transform);
            tower.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            tower.transform.localScale = new Vector3(0.9f, 2.5f, 0.9f);
            tower.GetComponent<Renderer>().material = castleMat;

            // 2. Четыре угловые оборонительные колонны для величия
            float offset = 0.55f;
            for (float ox = -offset; ox <= offset; ox += offset * 2)
            {
                for (float oz = -offset; oz <= offset; oz += offset * 2)
                {
                    GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(pillar.GetComponent<BoxCollider>());
                    pillar.transform.SetParent(root.transform);
                    pillar.transform.localPosition = new Vector3(ox, 0.8f, oz);
                    pillar.transform.localScale = new Vector3(0.40f, 1.6f, 0.40f);
                    pillar.GetComponent<Renderer>().material = castleMat;
                }
            }

            // 3. Неоновая светящаяся Корона / Цитадель на верхушке башни
            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(crown.GetComponent<BoxCollider>());
            crown.transform.SetParent(root.transform);
            crown.transform.localPosition = new Vector3(0f, 2.75f, 0f);
            crown.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            
            Material crownMat = new Material(urpShader);
            crownMat.color = castle.owner == "Player" ? new Color(0.5f, 1.0f, 0.6f, 1.0f) : new Color(1.0f, 0.6f, 0.7f, 1.0f);
            if (crownMat.HasProperty("_EmissionColor")) crownMat.SetColor("_EmissionColor", crownMat.color * 2.0f);
            crown.GetComponent<Renderer>().material = crownMat;

            castle.visualRoot = root;
        }
    }

    private void HandleCastleClicks()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Проверка: клик поверх UI исключается во избежание закрытия по клику
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 500f))
            {
                InteractiveCastle clicked = hit.collider.GetComponentInParent<InteractiveCastle>();
                if (clicked != null)
                {
                    OpenCastleDetails(clicked.zoneIndex);
                }
            }
        }
    }

    public void OpenCastleDetails(int index)
    {
        if (index >= 0 && index < castles.Count)
        {
            activeDetailsIndex = index;
            isDetailsOpen = true;
            feedbackMessage = "";
            Debug.Log($"[CASTLE MGR] Открыты детали замка зоны {index}: {castles[index].nameRU}");
        }
    }

    private IEnumerator PassiveIncomeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);

            // Каждую секунду вычисляем доход со всех принадлежащих игроку замков
            int income = 0;
            for (int i = 0; i < castles.Count; i++)
            {
                if (castles[i].owner == "Player")
                {
                    // Level 1 = 5 gold/sec, Level 2 = 15 gold/sec
                    income += castles[i].level == 1 ? 5 : 15;
                }
            }

            if (income > 0)
            {
                SaveGameSystem.CurrentData.gold += income;
            }
        }
    }

    private void ShowFeedback(string msg)
    {
        feedbackMessage = msg;
        messageTimer = 3.5f;
    }

    private void OnGUI()
    {
        // 1. Постоянный красивый индикатор кошелька Золота в верхнем правом углу
        int curLang = Translator.LanguageID;
        string goldText = curLang == 0 ? "Золото: " : "Gold: ";
        if (curLang == 8) goldText = "金币: ";
        if (curLang == 7) goldText = "골드: ";

        GUIStyle goldStyle = new GUIStyle(GUI.skin.box);
        goldStyle.fontSize = 17;
        goldStyle.fontStyle = FontStyle.Bold;
        goldStyle.normal.textColor = new Color(1.0f, 0.85f, 0.1f, 1.0f); // Насыщенный золотой
        goldStyle.alignment = TextAnchor.MiddleCenter;

        GUI.Box(new Rect(Screen.width - 230f, 20f, 210f, 45f), $"💰 {goldText}{SaveGameSystem.CurrentData.gold}", goldStyle);

        // 2. Окно деталей выбранного замка
        if (!isDetailsOpen || activeDetailsIndex < 0 || activeDetailsIndex >= castles.Count) return;

        CastleInstance castle = castles[activeDetailsIndex];

        // ZENITH GLASSMORPHISM PANEL LAYOUT
        float panelWidth = 470f;
        float panelHeight = 490f;
        float px = (Screen.width - panelWidth) / 2f;
        float py = (Screen.height - panelHeight) / 2f;

        // Фон панели
        GUI.backgroundColor = new Color(0.04f, 0.08f, 0.2f, 0.98f);
        GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.normal.textColor = Color.white;
        windowStyle.fontSize = 16;
        windowStyle.fontStyle = FontStyle.Bold;

        string windowHeader = curLang == 0 ? "Управление Замком" : "Castle Stronghold";
        if (curLang == 8) windowHeader = "城堡大厅";
        if (curLang == 7) windowHeader = "성채 관리실";

        GUI.Window(99, new Rect(px, py, panelWidth, panelHeight), WindowFunction, windowHeader, windowStyle);
    }

    private void WindowFunction(int windowID)
    {
        int curLang = Translator.LanguageID;
        CastleInstance castle = castles[activeDetailsIndex];

        // Стили текста
        GUIStyle nameStyle = new GUIStyle(GUI.skin.label);
        nameStyle.alignment = TextAnchor.MiddleCenter;
        nameStyle.fontSize = 20;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.normal.textColor = castle.owner == "Player" ? new Color(0.2f, 1.0f, 0.6f) : new Color(1.0f, 0.3f, 0.4f);

        string labelName = curLang == 0 ? castle.nameRU : castle.nameEN;
        GUILayout.Label($"🏰 {labelName.ToUpper()}", nameStyle);

        GUIStyle infoStyle = new GUIStyle(GUI.skin.label);
        infoStyle.alignment = TextAnchor.MiddleCenter;
        infoStyle.fontSize = 14;
        infoStyle.normal.textColor = Color.gray;

        string ownerTxt = castle.owner == "Player" ? 
            (curLang == 0 ? "КОНТРОЛИРУЕТСЯ ИГРОКОМ" : "CONTROLLED BY YOU") : 
            (curLang == 0 ? "КОНТРОЛИРУЕТСЯ ВРАГОМ" : "UNDER ENEMY CONTROL");
        GUILayout.Label(ownerTxt, infoStyle);

        GUILayout.Space(8);

        // Уровень Замка
        GUIStyle levelStyle = new GUIStyle(GUI.skin.label);
        levelStyle.alignment = TextAnchor.MiddleCenter;
        levelStyle.fontSize = 17;
        levelStyle.fontStyle = FontStyle.Bold;
        levelStyle.normal.textColor = Color.white;
        string lvlName = curLang == 0 ? "УРОВЕНЬ" : "LEVEL";
        GUILayout.Label($"{lvlName}: {castle.level} / 5", levelStyle);

        // Пассивный Поток
        int speed = castle.level == 1 ? 5 : 15;
        string flowTxt = curLang == 0 ? 
            $"Пассивная добыча золота: +{speed} 💰 / сек" : 
            $"Passive gold collection: +{speed} 💰 / sec";
        GUILayout.Label(flowTxt, infoStyle);

        GUILayout.Space(12);

        // ФИДБЕК МЕССАДЖИ
        if (!string.IsNullOrEmpty(feedbackMessage))
        {
            GUIStyle feedStyle = new GUIStyle(GUI.skin.box);
            feedStyle.normal.textColor = new Color(0.0f, 1.0f, 1.0f);
            feedStyle.alignment = TextAnchor.MiddleCenter;
            feedStyle.fontSize = 13;
            GUILayout.Box(feedbackMessage, feedStyle, GUILayout.Height(35));
        }
        else
        {
            GUILayout.Space(39);
        }

        GUILayout.Space(5);

        GUILayout.BeginVertical(GUI.skin.box);

        if (castle.owner == "Player")
        {
            // ДЕЙСТВИЯ СОЮЗНОГО ЗАМКА
            string upgradeBtnText = curLang == 0 ? "УЛУЧШИТЬ ЗАМОК (200 💰)" : "UPGRADE CASTLE (200 💰)";
            if (GUILayout.Button(upgradeBtnText, GUILayout.Height(40)))
            {
                if (castle.level >= 2)
                {
                    string restrict = curLang == 0 ? 
                        "Максимум 2 уровень на первом континенте!" : 
                        "Maximum level is 2 on the first continent!";
                    ShowFeedback(restrict);
                }
                else if (SaveGameSystem.CurrentData.gold < 200)
                {
                    string noGold = curLang == 0 ? "Недостаточно золота!" : "Not enough gold!";
                    ShowFeedback(noGold);
                }
                else
                {
                    SaveGameSystem.CurrentData.gold -= 200;
                    castle.level = 2;
                    PlayerPrefs.SetInt("Castle_Level_" + activeDetailsIndex, 2);
                    PlayerPrefs.Save();
                    
                    string upgraded = curLang == 0 ? 
                        "Замок успешно улучшен до 2 уровня! Привилегии увеличены." : 
                        "Castle successfully upgraded to Level 2! Privileges increased.";
                    ShowFeedback(upgraded);
                }
            }

            GUILayout.Space(6);

            // Наём Войск
            string hireHeader = curLang == 0 ? "--- НАЁМ АРМИИ ---" : "--- RECRUIT SOLDIERS ---";
            GUILayout.Label(hireHeader, infoStyle);

            GUILayout.BeginHorizontal();
            
            string wName = curLang == 0 ? "Мечник (50 💰)" : "Warrior (50 💰)";
            if (GUILayout.Button(wName, GUILayout.Height(30)))
            {
                BuyUnit(50, curLang == 0 ? "Великий воин готов к бою!" : "Great warrior recruited!");
            }
            
            string aName = curLang == 0 ? "Лучник (75 💰)" : "Archer (75 💰)";
            if (GUILayout.Button(aName, GUILayout.Height(30)))
            {
                BuyUnit(75, curLang == 0 ? "Снайпер нанят! Лук натянут." : "Expert sniper is ready!");
            }

            string mName = curLang == 0 ? "Маг (120 💰)" : "Mage (120 💰)";
            if (GUILayout.Button(mName, GUILayout.Height(30)))
            {
                BuyUnit(120, curLang == 0 ? "Боевой архимаг вступил в отряд!" : "Battle Archmage is ready!");
            }
            
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // Покупка Зелий и Снаряжения
            string storeHeader = curLang == 0 ? "--- ЛАВКА СНАРЯЖЕНИЯ ---" : "--- EQUIPMENT SHOP ---";
            GUILayout.Label(storeHeader, infoStyle);

            GUILayout.BeginHorizontal();
            string potBtn = curLang == 0 ? "Зелье здоровья (30 💰)" : "Healing Potion (30 💰)";
            if (GUILayout.Button(potBtn, GUILayout.Height(30)))
            {
                BuyItem(30, curLang == 0 ? "Лечебное зелье добавлено герою!" : "Healing potion added!");
            }

            string eqBtn = curLang == 0 ? "Латы & Меч (90 💰)" : "Heavy Armor (90 💰)";
            if (GUILayout.Button(eqBtn, GUILayout.Height(30)))
            {
                BuyItem(90, curLang == 0 ? "Экипировка улучшена! Защита +15." : "Heavy armor customized!");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // Тренировочный плац
            string trainHeader = curLang == 0 ? "--- ТРЕНИРОВОЧНЫЙ ЦЕНТР ---" : "--- TRAINING PLAZA ---";
            GUILayout.Label(trainHeader, infoStyle);

            string trBtn = curLang == 0 ? "⚔️ Начать учения и тренировки (Бесплатно)" : "⚔️ Begin combat training (Free)";
            if (GUILayout.Button(trBtn, GUILayout.Height(30)))
            {
                SaveGameSystem.CurrentData.currentXP += 15;
                if (SaveGameSystem.CurrentData.currentXP >= 100)
                {
                    SaveGameSystem.CurrentData.currentXP = 0;
                    SaveGameSystem.CurrentData.playerLevel++;
                }
                string feed = curLang == 0 ? 
                    $"Герой прошел тренировку! +15 опыта получено. Уровень: {SaveGameSystem.CurrentData.playerLevel}" : 
                    $"Hero finished training! +15 XP. Level: {SaveGameSystem.CurrentData.playerLevel}";
                ShowFeedback(feed);
            }
        }
        else
        {
            // ДЕЙСТВИЯ ВРАЖЕСКОГО ЗАМКА
            GUIStyle warningStyle = new GUIStyle(GUI.skin.label);
            warningStyle.alignment = TextAnchor.MiddleCenter;
            warningStyle.fontStyle = FontStyle.Bold;
            warningStyle.fontSize = 14;
            warningStyle.normal.textColor = new Color(1.0f, 0.4f, 0.4f);

            string warn = curLang == 0 ? 
                "ЭТОТ ЗАМОК ДЕРЖИТ В ОБОРОНЕ ВРАЖЕСКИЙ КЛАН.\nЗахватывайте земли и копите золото для осады!" : 
                "THIS STRONGHOLD IS GUARDED BY AN OPPOSING CLAN.\nConquer territories and prepare resources for siege!";
            GUILayout.Label(warn, warningStyle);
            
            GUILayout.Space(15);
            
            string spyBtn = curLang == 0 ? "🕵️ Отправить лазутчика (Бесплатно)" : "🕵️ Dispatch spy (Free)";
            if (GUILayout.Button(spyBtn, GUILayout.Height(40)))
            {
                int garrison = UnityEngine.Random.Range(3, 12);
                string spyFeed = curLang == 0 ? 
                    $"Шпион докладывает: в замке находится {garrison} воинов противника!" : 
                    $"Spy reports: a garrison of {garrison} enemy soldiers defends the fort!";
                ShowFeedback(spyFeed);
            }
        }

        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();

        // Кнопка Закрыть
        GUI.backgroundColor = new Color(1.0f, 0.3f, 0.3f, 1.0f);
        string closeText = curLang == 0 ? "ЗАКРЫТЬ ДЕТАЛИ" : "CLOSE DETAILS";
        if (GUILayout.Button(closeText, GUILayout.Height(35)))
        {
            isDetailsOpen = false;
        }
        GUI.backgroundColor = Color.white;
    }

    private void BuyUnit(int price, string successMsg)
    {
        if (SaveGameSystem.CurrentData.gold < price)
        {
            string noG = Translator.LanguageID == 0 ? "Недостаточно золота!" : "Not enough gold!";
            ShowFeedback(noG);
        }
        else
        {
            SaveGameSystem.CurrentData.gold -= price;
            ShowFeedback(successMsg);
        }
    }

    private void BuyItem(int price, string successMsg)
    {
        if (SaveGameSystem.CurrentData.gold < price)
        {
            string noG = Translator.LanguageID == 0 ? "Недостаточно золота!" : "Not enough gold!";
            ShowFeedback(noG);
        }
        else
        {
            SaveGameSystem.CurrentData.gold -= price;
            ShowFeedback(successMsg);
        }
    }
}

/// <summary>
/// Скрипт-маркер, вещаемый на 3D замки для улавливания кликов
/// </summary>
public class InteractiveCastle : MonoBehaviour
{
    public int zoneIndex;
}
