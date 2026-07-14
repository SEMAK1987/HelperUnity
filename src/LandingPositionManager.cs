using UnityEngine;
using System.Collections;

namespace FateContinent
{
    /// <summary>
    /// Разработчик: Fate Continent (Континент Судьбы) • Версия v18.11.7
    /// Менеджер позиционирования высадки (Landing Position Manager).
    /// Считывает выбор точки высадки из DialogueSystem_Manager или сохранений SaveGameSystem,
    /// находит вручную заданные 3D координаты на вашей высокополигональной карте
    /// и плавно перемещает туда камеру или десантирует фигурку выбранного героя.
    /// </summary>
    public class LandingPositionManager : MonoBehaviour
    {
        public static LandingPositionManager Instance { get; private set; }

        [System.Serializable]
        public class LandingPoint
        {
            [Tooltip("Техническое имя зоны (для удобства)")]
            public string zoneID = "Wastes";
            
            [Tooltip("Красивое отображаемое имя зоны")]
            public string zoneName = "Кровавые Пустоши";
            
            [Tooltip("Точка в 3D мире (сюда перенесется игрок/камера)")]
            public Transform spawnAnchor;
            
            [Tooltip("Аффилированные 3D объекты этой локации, которые активируются при высадке")]
            public GameObject[] localObjectsToActivate;
        }

        [Header("📍 Настройка Физических Точек Высадки в 3D Мире")]
        [Tooltip("Список ручных точек высадки на 3D карте Континента")]
        public LandingPoint[] landingPoints = new LandingPoint[4];

        [Header("🎥 Ссылки на объекты сцены")]
        [Tooltip("Объект игрока, который будет десантирован")]
        public Transform playerTransform;

        [Tooltip("3D Модель Континента (будет автоматически скрыта во время диалога и включена при десантировании)")]
        public GameObject continentObject;

        [Tooltip("Главная камера для плавной фокусировки")]
        public Transform mainCameraTransform;

        [Header("✨ Настройки плавного перемещения камеры")]
        public bool smoothCameraMove = true;
        public float cameraMoveSpeed = 3.0f;
        public Vector3 cameraOffset = new Vector3(0, 2.5f, -2.0f); // Ракурс сверху на точку

        [Header("👤 Настройки Импорта 3D-моделей Героев")]
        [Tooltip("Коэффициент масштаба для импортируемой 3D-фигурки (если модель слишком большая или маленькая)")]
        public float customHeroModelScale = 1.0f;

        [Tooltip("Слот под префаб или модель Воина (Warrior)")]
        public GameObject warriorModelPrefab;
        [Tooltip("Слот под префаб или модель Стрелка (Streloc)")]
        public GameObject archerModelPrefab;
        [Tooltip("Слот под префаб или модель Мага (Mage)")]
        public GameObject mageModelPrefab;

        [Header("🛠️ Тестирование в редакторе (Edit Mode)")]
        [Tooltip("Выберите класс для предпросмотра 3D-модели прямо в Редакторе")]
        public EditorPreviewClass editorPreviewClass = EditorPreviewClass.Warrior;

        public enum EditorPreviewClass { Warrior, Archer, Mage }

        private System.Collections.Generic.Dictionary<string, Material> originalRegionMaterials = new System.Collections.Generic.Dictionary<string, Material>();
        private Mesh originalPlaceholderMesh;
        private bool isPlaceholderMeshCached = false;

        private void CachePlaceholderMesh()
        {
            if (isPlaceholderMeshCached || playerTransform == null) return;
            var mf = playerTransform.GetComponent<MeshFilter>();
            if (mf != null)
            {
                originalPlaceholderMesh = mf.sharedMesh;
                isPlaceholderMeshCached = true;
            }
        }

        private void OnValidate()
        {
            InitializeDefaultPoints();

            // Автоматическое обновление превью персонажа в редакторе (Edit Mode)
            if (playerTransform != null && !Application.isPlaying)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.delayCall += () => {
                    if (this != null && playerTransform != null)
                    {
                        ApplyPlayerVisualClass();
                    }
                };
#endif
            }
        }

        private void InitializeDefaultPoints()
        {
            // Автоматическое расширение и инициализация массива до 4 элементов (под 4 кнопки диалога)
            if (landingPoints == null || landingPoints.Length != 4)
            {
                landingPoints = new LandingPoint[4];
            }

            for (int i = 0; i < 4; i++)
            {
                if (landingPoints[i] == null)
                {
                    landingPoints[i] = new LandingPoint();
                }
            }

            // Настройка имен по умолчанию
            landingPoints[0].zoneID = "Wastes";
            landingPoints[0].zoneName = "Кровавые Пустоши";

            landingPoints[1].zoneID = "Peak";
            landingPoints[1].zoneName = "Ледяной Пик";

            landingPoints[2].zoneID = "Ruins";
            landingPoints[2].zoneName = "Древние Руины";

            landingPoints[3].zoneID = "Sanctuary";
            landingPoints[3].zoneName = "Святилище Зенита";

            // Автоматический поиск пустышек, чтобы ничего не сбрасывалось и не требовалось перетаскивать вручную
            AutoFindSpawnAnchors();
        }

        private void AutoFindSpawnAnchors()
        {
            if (landingPoints == null) return;

            string[] defaultAnchorNames = new string[] { 
                "Oasis_SpawnPoint", 
                "Outpost_SpawnPoint", 
                "Shore_SpawnPoint", 
                "Citadel_SpawnPoint" 
            };

            // Принудительно корректируем индекс 3 (Грозовые Кряжи) под 8-й регион (Shore_SpawnPoint / Древние Руины)
            // по требованию игрока, чтобы избежать высадки в 11-й регион.
            defaultAnchorNames[3] = "Shore_SpawnPoint";

            for (int i = 0; i < landingPoints.Length; i++)
            {
                if (landingPoints[i] == null) continue;

                // Для надежности, если i == 3, всегда принудительно переназначаем на Shore_SpawnPoint
                if (i == 3)
                {
                    landingPoints[3].spawnAnchor = null;
                }

                if (landingPoints[i].spawnAnchor == null)
                {
                    string targetName = defaultAnchorNames[i];
                    GameObject foundObj = GameObject.Find(targetName);
                    
                    // Если не нашли напрямую, ищем по всей иерархии (включая неактивные или вложенные объекты)
                    if (foundObj == null)
                    {
                        var transforms = Resources.FindObjectsOfTypeAll<Transform>();
                        foreach (var t in transforms)
                        {
                            if (t.name == targetName && t.gameObject.scene.isLoaded)
                            {
                                foundObj = t.gameObject;
                                break;
                            }
                        }
                    }

                    if (foundObj != null)
                    {
                        landingPoints[i].spawnAnchor = foundObj.transform;
                        Debug.Log($"<color=#00FFCC>[LANDING SYS]</color> Успешно авто-связали точку: <b>{landingPoints[i].zoneName}</b> с объектом <b>{targetName}</b> в сцене!");
                    }
                }
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

#if UNITY_EDITOR
            // Если мы запустили геймплейную сцену напрямую в редакторе, принудительно сбрасываем кампанию для тестирования новой игры
            if (!SaveGameSystem.IsStartedFromMenu)
            {
                Debug.Log("<color=yellow>[FATE EDITOR DEBUG]</color> Прямой запуск GameScene в редакторе! Сбрасываем кампанию для чистоты тестирования...");
                PlayerPrefs.SetInt("ContinentGameplayActive", 0);
                PlayerPrefs.SetInt("LandedZoneIndex", -1);
                PlayerPrefs.SetInt("Fate_Current_Day", 1);
                PlayerPrefs.Save();

                // Полностью очищаем старый инвентарь, экипировку и прогресс игрока для чистоты теста
                SaveGameSystem.ClearCampaignAndPlayerProgression();

                // Заполняем чистые дефолтные характеристики (используем выбранный в инспекторе класс для удобства тестов)
                SaveGameSystem.ResetData();
                SaveGameSystem.CurrentData.characterClass = editorPreviewClass.ToString();
                SaveGameSystem.CurrentData.playerLevel = 1;
                SaveGameSystem.CurrentData.gold = 500;
                SaveGameSystem.CurrentData.strength = 15;
                SaveGameSystem.CurrentData.agility = 10;
                SaveGameSystem.CurrentData.intelligence = 4;
                SaveGameSystem.CurrentData.stamina = 15;
                SaveGameSystem.Save(0);

                SaveGameSystem.IsStartedFromMenu = true;
            }
#endif

            // Гарантируем корректное заполнение при старте
            InitializeDefaultPoints();

            // Создаем красивую плоскость океана под континентом для фонового ландшафта, если она отсутствует
            SetupOceanPlane();

            // Автоматически понижаем ракурс камеры, если значение имеет устаревший или завышенный дефолт в инспекторе (например, y > 5.0f)
            if (cameraOffset.y > 5.0f)
            {
                cameraOffset = new Vector3(0, 2.5f, -2.0f);
                Debug.Log($"<color=#00FFCC>[LANDING SYS]</color> Скорректировали слишком высокое смещение камеры ({cameraOffset.y}f) до идеального ракурса 2.5f для детального масштаба континента.");
            }

            // Автоматическое нахождение 3D-карты в Hierarchy на старте
            if (continentObject == null)
            {
                continentObject = GameObject.Find("Континент");
                if (continentObject == null)
                {
                    continentObject = GameObject.Find("New_Kontinent");
                }

                if (continentObject == null)
                {
                    // Ищем по части имени
                    foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
                    {
                        if ((go.name.Contains("Континент") || go.name.Contains("New_Kontinent") || go.name.Contains("Continent")) && go.scene.isLoaded)
                        {
                            continentObject = go;
                            break;
                        }
                    }
                }

                if (continentObject != null)
                {
                    Debug.Log("<color=#00FFCC>[LANDING SYS]</color> Скрипт автоматически нашел 3D-модель Континента: " + continentObject.name);
                }
            }

            // Автоматическое нахождение синей сферы игрока в Hierarchy на старте
            if (playerTransform == null)
            {
                GameObject pObj = GameObject.Find("Player_Placeholder");
                if (pObj == null)
                {
                    foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
                    {
                        if (go.name.Contains("Player_Placeholder") && go.scene.isLoaded)
                        {
                            pObj = go;
                            break;
                        }
                    }
                }

                if (pObj != null)
                {
                    playerTransform = pObj.transform;
                    Debug.Log("<color=#00FFCC>[LANDING SYS]</color> Скрипт автоматически нашел Player_Placeholder.");
                }
            }

            // Автоматическое нахождение главной камеры, если она не задана в Инспекторе
            if (mainCameraTransform == null)
            {
                if (Camera.main != null)
                {
                    mainCameraTransform = Camera.main.transform;
                    Debug.Log("<color=#00FFCC>[LANDING SYS]</color> Скрипт автоматически нашел Главную Камеру по тегу MainCamera.");
                }
                else
                {
                    Camera cam = FindFirstObjectByType<Camera>();
                    if (cam != null)
                    {
                        mainCameraTransform = cam.transform;
                        Debug.Log("<color=#00FFCC>[LANDING SYS]</color> Скрипт автоматически нашел первую попавшуюся Камеру в сцене.");
                    }
                }
            }

            // Кэшируем оригинальные красивые материалы регионов в самом начале, пока сцена полностью активна
            CacheOriginalMaterials();

            // Динамически загружаем 3D-фигурку выбранного класса вместо стандартной синей сферы
            ApplyPlayerVisualClass();
        }

        private void Start()
        {
            // СВЕРХВАЖНО: Кэшируем оригинальные красивые материалы регионов перед их деактивацией или перекраской!
            CacheOriginalMaterials();

            // Автоматически скрываем любые 3D-рендеры на анкорах высадки, чтобы они не отображались на континенте как лишние цветные шары/точки!
            if (landingPoints != null)
            {
                foreach (var pt in landingPoints)
                {
                    if (pt != null && pt.spawnAnchor != null)
                    {
                        var rends = pt.spawnAnchor.GetComponentsInChildren<Renderer>(true);
                        foreach (var r in rends)
                        {
                            r.enabled = false;
                        }
                    }
                }
            }

            // Гарантируем, что визуальная фигурка героя соответствует выбранному классу
            ApplyPlayerVisualClass();

            bool isGameplayActive = PlayerPrefs.GetInt("ContinentGameplayActive", 0) == 1 && PlayerPrefs.GetInt("LandedZoneIndex", -1) != -1;

            if (isGameplayActive)
            {
                // Если геймплей на континенте уже активен (загрузка сохранения / возвращение из битвы)
                if (continentObject != null)
                {
                    continentObject.SetActive(true);
                    Debug.Log("[LANDING SYS] Геймплей активен: Включили 3D Континент.");
                }

                // Активируем океан обратно
                GameObject oceanObj = null;
                foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    if (go.name == "Fate_Ocean_Plane" && go.scene.isLoaded)
                    {
                        oceanObj = go;
                        break;
                    }
                }
                if (oceanObj != null)
                {
                    oceanObj.SetActive(true);
                    Debug.Log("[LANDING SYS] Геймплей активен: Включили океан Fate_Ocean_Plane.");
                }

                // Восстанавливаем сохраненную точку высадки
                int landedZone = PlayerPrefs.GetInt("LandedZoneIndex", 0);
                RepaintRegionsBasedOnLanding(landedZone);

                // Позиционируем игрока на выбранную точку высадки с учетом кастомных смещений
                if (playerTransform != null)
                {
                    playerTransform.gameObject.SetActive(true);

                    Quaternion anchorRot;
                    Vector3 anchorPos = GetLandingAnchorPosition(landedZone, out anchorRot);

                    float ox = PlayerPrefs.GetFloat($"PlayerOffset_X_{landedZone}", 0f);
                    float oy = PlayerPrefs.GetFloat($"PlayerOffset_Y_{landedZone}", 0.8f); // По умолчанию приподнят на 0.8 метра, чтобы не утонуть в замке!
                    float oz = PlayerPrefs.GetFloat($"PlayerOffset_Z_{landedZone}", 0f);

                    playerTransform.position = anchorPos + new Vector3(ox, oy, oz);
                    playerTransform.rotation = anchorRot;
                    Debug.Log($"[LANDING SYS] Геймплей активен: Игрок успешно восстановлен по координатам с оффсетом {new Vector3(ox, oy, oz)}: {playerTransform.position}");

                    // Направляем и фокусируем камеру на этой точке
                    if (StrategicCameraController.Instance != null)
                    {
                        StrategicCameraController.Instance.FocusOnPoint(anchorPos, cameraOffset);
                        StrategicCameraController.Instance.isControlEnabled = true;
                    }
                    else if (mainCameraTransform != null)
                    {
                        mainCameraTransform.position = anchorPos + cameraOffset;
                        mainCameraTransform.LookAt(anchorPos);
                    }
                }

                // Разрешаем свободное управление камерой
                if (StrategicCameraController.Instance != null)
                {
                    StrategicCameraController.Instance.isControlEnabled = true;
                }
            }
            else
            {
                // Изначально полностью выключаем 3D-карту, океан и героя, чтобы они не лезли на задний план вступительных диалогов Аэлиссы!
                if (continentObject != null)
                {
                    continentObject.SetActive(false);
                    Debug.Log("[LANDING SYS] Изначально деактивировали 3D Континент, чтобы очистить задний план диалога.");
                }

                if (playerTransform != null)
                {
                    playerTransform.gameObject.SetActive(false);
                    Debug.Log("[LANDING SYS] Изначально деактивировали Player_Placeholder.");
                }

                // Изначально скрываем океан, чтобы не мешал во время разговора
                GameObject oceanObj = null;
                foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    if (go.name == "Fate_Ocean_Plane" && go.scene.isLoaded)
                    {
                        oceanObj = go;
                        break;
                    }
                }
                if (oceanObj != null)
                {
                    oceanObj.SetActive(false);
                    Debug.Log("[LANDING SYS] Изначально деактивировали океан Fate_Ocean_Plane.");
                }
            }
        }

        /// <summary>
        /// Вызывается автоматически, когда игрок выбирает точку высадки и завершает диалог!
        /// </summary>
        /// <param name="zoneIndex">Индекс выбранной зоны (0: Кровавые Пустоши, 1: Ледяной Пик, 2: Древние Руины, 3: Грозовые Кряжи)</param>
        public void DispatchLanding(int zoneIndex)
        {
            // Скрываем полноэкранный фоновый канвас высадки Аэлиссы, чтобы он не перекрывал карты в момент высадки
            if (FateDialogueBackgroundController.Instance != null)
            {
                FateDialogueBackgroundController.Instance.HideBackground();
                Debug.Log("<color=#00FFCC>[LANDING SYS]</color> Досрочно скрываем полноэкранный диалоговый фон перед полетом камеры!");
            }

            // Включаем 3D-карту и героя обратно при начале перемещения камеры
            if (continentObject != null)
            {
                continentObject.SetActive(true);
                Debug.Log("<color=#00FFCC>[LANDING SYS]</color> Активируем 3D-модель Континента!");
            }

            // Перекрашиваем регионы высадки: выбранный оставляем цветным, невыбранные делаем серыми
            RepaintRegionsBasedOnLanding(zoneIndex);

            if (playerTransform != null)
            {
                playerTransform.gameObject.SetActive(true);
                Debug.Log("<color=#00FFCC>[LANDING SYS]</color> Активируем Player_Placeholder!");
                
                // Гарантируем, что модель героя обновлена и правильно отображается при десантировании
                ApplyPlayerVisualClass();
            }

            // Активируем океан обратно при старте полета
            GameObject oceanObj = null;
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "Fate_Ocean_Plane" && go.scene.isLoaded)
                {
                    oceanObj = go;
                    break;
                }
            }
            if (oceanObj != null)
            {
                oceanObj.SetActive(true);
                Debug.Log("<color=#00FFCC>[LANDING SYS]</color> Активируем океан Fate_Ocean_Plane!");
            }

            if (landingPoints == null || landingPoints.Length == 0)
            {
                Debug.LogWarning("[LANDING SYS] Точки высадки не настроены в LandingPositionManager!");
                return;
            }

            // Безопасное ограничение индекса в пределах массива
            int targetIndex = Mathf.Clamp(zoneIndex, 0, landingPoints.Length - 1);
            LandingPoint point = landingPoints[targetIndex];

            Debug.Log($"<color=#00FFCC>[LANDING SYS]</color> Выполняем десантирование на точку: <b>{point.zoneName}</b> (Индекс: {zoneIndex})");

            // 1. Активируем локальные объекты этой зоны
            if (point.localObjectsToActivate != null)
            {
                foreach (var obj in point.localObjectsToActivate)
                {
                    if (obj != null) obj.SetActive(true);
                }
            }

            // 2. Телепортируем игрока на физические координаты 3D Континента с учетом оффсетов
            Quaternion anchorRot;
            Vector3 anchorPos = GetLandingAnchorPosition(zoneIndex, out anchorRot);

            if (playerTransform != null)
            {
                float ox = PlayerPrefs.GetFloat($"PlayerOffset_X_{zoneIndex}", 0f);
                float oy = PlayerPrefs.GetFloat($"PlayerOffset_Y_{zoneIndex}", 0.8f); // По умолчанию приподнят на 0.8 метра, чтобы не утонуть в замке!
                float oz = PlayerPrefs.GetFloat($"PlayerOffset_Z_{zoneIndex}", 0f);

                playerTransform.position = anchorPos + new Vector3(ox, oy, oz);
                playerTransform.rotation = anchorRot;
                Debug.Log($"[LANDING SYS] Игрок успешно перенесен в 3D координаты с оффсетом {new Vector3(ox, oy, oz)}: {playerTransform.position}");
            }

            // Блокируем свободное перемещение камеры на время полета
            if (StrategicCameraController.Instance != null)
            {
                StrategicCameraController.Instance.isControlEnabled = false;
            }

            // 3. Перемещаем камеру
            if (mainCameraTransform != null)
            {
                Vector3 targetCameraPos = anchorPos + cameraOffset;
                if (smoothCameraMove)
                {
                    StartCoroutine(SmoothMoveCameraCoroutine(targetCameraPos, anchorPos, targetIndex));
                }
                else
                {
                    mainCameraTransform.position = targetCameraPos;
                    mainCameraTransform.LookAt(anchorPos);
                    CompleteLandingAndStartDialogue(anchorPos);
                }
            }
        }

        private void CompleteLandingAndStartDialogue(Vector3 lookAtPoint)
        {
            if (StrategicCameraController.Instance != null)
            {
                StrategicCameraController.Instance.FocusOnPoint(lookAtPoint, cameraOffset);
                StrategicCameraController.Instance.isControlEnabled = false;
            }
            if (GamePause_Manager.Instance != null)
            {
                GamePause_Manager.Instance.isPauseBlockedManually = true;
            }
            if (FateMapManager.Instance != null)
            {
                FateMapManager.Instance.SetMapVisible(false);
            }

            if (DialogueSystem_Manager.Instance != null)
            {
                DialogueSystem_Manager.Instance.StartDialogue(8);
            }
            Debug.Log("[LANDING SYS] Десантирование завершено. Пауза заблокирована. Запускаем Инструктаж о замках (шаг 8).");
        }

        /// <summary>
        /// Возвращает физическое положение точки высадки на основе spawnAnchor, объектов сцены или жестких координат.
        /// Обеспечивает 100% точность для Грозовых Кряжей (индекс 3) на Region_08 (Древние Руины).
        /// </summary>
        public Vector3 GetLandingAnchorPosition(int zoneIndex, out Quaternion rotation)
        {
            rotation = Quaternion.identity;
            
            // Если для Грозовых Кряжей (индекс 3) принудительно задаем 8 регион
            if (zoneIndex == 3)
            {
                // Поищем Region_08 в New_Kontinent или Континент
                GameObject newContinent = GameObject.Find("New_Kontinent") ?? GameObject.Find("Континент");
                if (newContinent != null)
                {
                    Transform r8 = newContinent.transform.Find("Region_08");
                    if (r8 == null)
                    {
                        foreach (Transform child in newContinent.GetComponentsInChildren<Transform>(true))
                        {
                            if (child.name == "Region_08")
                            {
                                r8 = child;
                                break;
                            }
                        }
                    }
                    if (r8 != null)
                    {
                        Debug.Log("[LANDING SYS] Найдена физическая модель Region_08 для Грозовых Кряжей!");
                        rotation = r8.rotation;
                        return r8.position;
                    }
                }
                
                // Вторым приоритетом ищем Shore_SpawnPoint или Ruins_SpawnPoint в сцене
                GameObject shore = GameObject.Find("Shore_SpawnPoint") ?? GameObject.Find("Ruins_SpawnPoint");
                if (shore != null)
                {
                    rotation = shore.transform.rotation;
                    return shore.transform.position;
                }
                
                // Третий приоритет - жесткие координаты Region_08 из FateCastleManager
                return new Vector3(-12.4f, -0.3f, -10.2f);
            }

            int targetIndex = Mathf.Clamp(zoneIndex, 0, landingPoints.Length - 1);
            LandingPoint point = (landingPoints != null && landingPoints.Length > targetIndex) ? landingPoints[targetIndex] : null;
            
            if (point != null && point.spawnAnchor != null)
            {
                rotation = point.spawnAnchor.rotation;
                return point.spawnAnchor.position;
            }
            
            // Если spawnAnchor равен null, ищем по умолчанию по именам
            string[] defaultAnchorNames = new string[] { 
                "Oasis_SpawnPoint", 
                "Outpost_SpawnPoint", 
                "Shore_SpawnPoint", 
                "Citadel_SpawnPoint" 
            };
            
            string targetName = defaultAnchorNames[Mathf.Clamp(zoneIndex, 0, 3)];
            GameObject foundObj = GameObject.Find(targetName);
            if (foundObj == null)
            {
                string altName = zoneIndex == 0 ? "Wastes_SpawnPoint" :
                                 zoneIndex == 1 ? "Peak_SpawnPoint" :
                                 zoneIndex == 2 ? "Ruins_SpawnPoint" : "Crags_SpawnPoint";
                foundObj = GameObject.Find(altName);
            }
            
            if (foundObj != null)
            {
                rotation = foundObj.transform.rotation;
                return foundObj.transform.position;
            }
            
            // Фолбек на координаты по индексам замков
            if (FateCastleManager.Instance != null && FateCastleManager.Instance.customCastlePositions != null)
            {
                int rIdx = 11;
                if (zoneIndex == 0) rIdx = 11;
                else if (zoneIndex == 1) rIdx = 6;
                else if (zoneIndex == 2) rIdx = 8;
                else if (zoneIndex == 3) rIdx = 8;
                
                if (rIdx >= 0 && rIdx < FateCastleManager.Instance.customCastlePositions.Length)
                {
                    return FateCastleManager.Instance.customCastlePositions[rIdx];
                }
            }
            
            // Окончательный хардкод фолбек
            switch (zoneIndex)
            {
                case 0: return new Vector3(9.9f, 0.8f, -4.5f); // Region_11
                case 1: return new Vector3(14.8f, 1.2f, 12.5f); // Region_06
                case 2: return new Vector3(-12.4f, -0.3f, -10.2f); // Region_08
                case 3: return new Vector3(-12.4f, -0.3f, -10.2f); // Region_08 (Грозовые Кряжи)
                default: return Vector3.zero;
            }
        }

        private void EnableStrategicCamera(Vector3 anchorPosition)
        {
            if (StrategicCameraController.Instance != null)
            {
                StrategicCameraController.Instance.FocusOnPoint(anchorPosition, cameraOffset);
                StrategicCameraController.Instance.isControlEnabled = true;
                Debug.Log("[LANDING SYS] Свободный режим стратегической камеры успешно активирован!");
            }
        }

        private IEnumerator SmoothMoveCameraCoroutine(Vector3 targetPosition, Vector3 lookAtPoint, int zoneIndex)
        {
            float elapsed = 0f;
            Vector3 startPos = mainCameraTransform.position;
            Quaternion startRot = mainCameraTransform.rotation;
            Quaternion targetRot = Quaternion.LookRotation(lookAtPoint - targetPosition);

            while (elapsed < 2.0f) // Длительность плавного полета 2 секунды
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 2.0f;
                
                // Плавное ускорение и замедление (Smooth Step)
                t = t * t * (3f - 2f * t);

                mainCameraTransform.position = Vector3.Lerp(startPos, targetPosition, t);
                mainCameraTransform.rotation = Quaternion.Slerp(startRot, targetRot, t);
                yield return null;
            }

            mainCameraTransform.position = targetPosition;
            mainCameraTransform.rotation = targetRot;
            Debug.Log("[LANDING SYS] Камера успешно наведена на новую зону высадки.");

            // Активируем свободное перемещение камеры игроком после приземления
            CompleteLandingAndStartDialogue(lookAtPoint);
        }

        private void CacheOriginalMaterials()
        {
            if (originalRegionMaterials == null)
            {
                originalRegionMaterials = new System.Collections.Generic.Dictionary<string, Material>();
            }

            GameObject newContinent = GameObject.Find("New_Kontinent") ?? GameObject.Find("Континент");
            if (newContinent != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    string regionName = "Region_" + i.ToString("D2");
                    Transform regTrans = newContinent.transform.Find(regionName);
                    if (regTrans != null)
                    {
                        Renderer mr = regTrans.GetComponent<Renderer>();
                        if (mr != null && mr.sharedMaterial != null)
                        {
                            if (!originalRegionMaterials.ContainsKey(regionName))
                            {
                                originalRegionMaterials.Add(regionName, mr.sharedMaterial);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Создает красивую плоскость океана под континентом для фонового ландшафта, если она отсутствует.
        /// </summary>
        public void SetupOceanPlane()
        {
            GameObject oceanObj = GameObject.Find("Fate_Ocean_Plane");
            if (oceanObj == null)
            {
                foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    if (go.name == "Fate_Ocean_Plane" && go.scene.isLoaded)
                    {
                        oceanObj = go;
                        break;
                    }
                }
            }

            if (oceanObj == null)
            {
                oceanObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
                oceanObj.name = "Fate_Ocean_Plane";
                oceanObj.transform.position = new Vector3(0f, -5f, 0f);
                oceanObj.transform.localScale = new Vector3(300f, 1f, 300f);

                MeshRenderer mr = oceanObj.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    mr.receiveShadows = true;

                    Material oceanMat = Resources.Load<Material>("M_Ocean_Background");
                    if (oceanMat == null) oceanMat = Resources.Load<Material>("Materials/M_Ocean_Background");
                    if (oceanMat == null) oceanMat = Resources.Load<Material>("Ocean/M_Ocean_Background");

                    if (oceanMat != null)
                    {
                        mr.sharedMaterial = oceanMat;
                        Debug.Log("[LANDING SYS] Успешно загрузили оригинальный материал океана: M_Ocean_Background");
                    }
                    else
                    {
                        Shader targetShader = Shader.Find("Universal Render Pipeline/Lit");
                        if (targetShader == null) targetShader = Shader.Find("Standard");
                        if (targetShader == null) targetShader = Shader.Find("Diffuse");
                        if (targetShader == null) targetShader = Shader.Find("Mobile/Diffuse");

                        Material fallbackMat = new Material(targetShader != null ? targetShader : Shader.Find("Standard"));
                        fallbackMat.name = "M_Ocean_Background_Fallback";
                        
                        Color deepOceanColor = new Color(0.04f, 0.12f, 0.25f, 1.0f);
                        if (fallbackMat.HasProperty("_Color")) fallbackMat.SetColor("_Color", deepOceanColor);
                        if (fallbackMat.HasProperty("_BaseColor")) fallbackMat.SetColor("_BaseColor", deepOceanColor);
                        
                        if (fallbackMat.HasProperty("_MainTex")) fallbackMat.SetTextureScale("_MainTex", new Vector2(40f, 40f));
                        if (fallbackMat.HasProperty("_BaseMap")) fallbackMat.SetTextureScale("_BaseMap", new Vector2(40f, 40f));

                        if (fallbackMat.HasProperty("_Glossiness")) fallbackMat.SetFloat("_Glossiness", 0.7f);
                        if (fallbackMat.HasProperty("_Smoothness")) fallbackMat.SetFloat("_Smoothness", 0.7f);
                        if (fallbackMat.HasProperty("_Metallic")) fallbackMat.SetFloat("_Metallic", 0.2f);

                        mr.sharedMaterial = fallbackMat;
                        Debug.Log("[LANDING SYS] Создан красивый URP/Standard fallback материал океана темно-синего цвета с UV тайлингом 40x40.");
                    }
                }
            }

            if (SettingsManager.Instance != null)
            {
                int currentQuality = PlayerPrefs.GetInt("Fate_Graphics_Quality", 2);
                SettingsManager.Instance.ApplyOceanQuality(currentQuality);
            }
        }

        /// <summary>
        /// Перекрашивает 12 регионов 3D-карты в зависимости от выбранной зоны высадки игрока.
        /// </summary>
        public void RepaintRegionsBasedOnLanding(int activeZoneIndex)
        {
            CacheOriginalMaterials();

            int actualPlayerRegion = 11;
            try
            {
                actualPlayerRegion = FateCastleManager.GetActualRegionIndexFromLanding(activeZoneIndex);
            }
            catch
            {
                switch (activeZoneIndex)
                {
                    case 0: actualPlayerRegion = 11; break;
                    case 1: actualPlayerRegion = 6; break;
                    case 2: actualPlayerRegion = 8; break;
                    case 3: actualPlayerRegion = 3; break;
                }
            }

            GameObject newContinent = GameObject.Find("New_Kontinent") ?? GameObject.Find("Континент");
            if (newContinent != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    string regionName = "Region_" + i.ToString("D2");
                    Transform regTrans = newContinent.transform.Find(regionName);
                    if (regTrans == null)
                    {
                        foreach (Transform child in newContinent.GetComponentsInChildren<Transform>(true))
                        {
                            if (child.name == regionName)
                            {
                                regTrans = child;
                                break;
                            }
                        }
                    }

                    if (regTrans != null)
                    {
                        Renderer mr = regTrans.GetComponent<Renderer>();
                        if (mr != null)
                        {
                                // Обязательно работаем через .material (инстанс), чтобы не модифицировать sharedMaterial ассеты на диске
                                if (mr.material != null)
                                {
                                    Color targetColor = FateCastleManager.GetRegionColor(i, actualPlayerRegion);

                                    mr.material.color = targetColor;
                                    if (mr.material.HasProperty("_BaseColor"))
                                    {
                                        mr.material.SetColor("_BaseColor", targetColor);
                                    }
                                    if (mr.material.HasProperty("_Color"))
                                    {
                                        mr.material.SetColor("_Color", targetColor);
                                    }
                                }
                        }
                    }
                }
                Debug.Log($"[LANDING SYS] Успешно перекрасили регионы. Активный регион высадки: Region_{actualPlayerRegion:D2} (яркий синий). Остальные регионы сброшены на исходные текстуры и материалы.");
            }
        }

        private void Update()
        {
            // Динамическое обновление масштаба 3D-модели на лету в режиме Play Mode
            if (playerTransform != null)
            {
                Transform visualChild = playerTransform.Find("Player_Model_Visual");
                if (visualChild != null)
                {
                    visualChild.localScale = Vector3.one * customHeroModelScale;
                }
            }

            bool isGameplayActive = PlayerPrefs.GetInt("ContinentGameplayActive", 0) == 1 && PlayerPrefs.GetInt("LandedZoneIndex", -1) != -1;
            if (isGameplayActive && playerTransform != null && landingPoints != null && landingPoints.Length > 0)
            {
                int landedZone = PlayerPrefs.GetInt("LandedZoneIndex", 0);
                
                Quaternion anchorRot;
                Vector3 anchorPos = GetLandingAnchorPosition(landedZone, out anchorRot);
                
                // Считываем текущую позицию игрока относительно anchorPos
                Vector3 currentOffset = playerTransform.position - anchorPos;
                
                // Сверяем с сохраненным оффсетом
                float savedOx = PlayerPrefs.GetFloat($"PlayerOffset_X_{landedZone}", 0f);
                float savedOy = PlayerPrefs.GetFloat($"PlayerOffset_Y_{landedZone}", 0.8f);
                float savedOz = PlayerPrefs.GetFloat($"PlayerOffset_Z_{landedZone}", 0f);

                // Если игрок переместил объект вручную в редакторе (порог 0.01м), автоматически сохраняем его!
                if (Mathf.Abs(currentOffset.x - savedOx) > 0.01f ||
                    Mathf.Abs(currentOffset.y - savedOy) > 0.01f ||
                    Mathf.Abs(currentOffset.z - savedOz) > 0.01f)
                {
                    PlayerPrefs.SetFloat($"PlayerOffset_X_{landedZone}", currentOffset.x);
                    PlayerPrefs.SetFloat($"PlayerOffset_Y_{landedZone}", currentOffset.y);
                    PlayerPrefs.SetFloat($"PlayerOffset_Z_{landedZone}", currentOffset.z);
                    PlayerPrefs.Save();
                    Debug.Log($"[LANDING SYS] Автоматически обнаружено ручное изменение положения игрока в Play Mode! Сохранён новый кастомный оффсет для зоны {landedZone}: {currentOffset}");
                }
            }
        }

        /// <summary>
        /// Динамически заменяет круг-плейсхолдер на выбранную 3D-модель Героя.
        /// Скрывает оригинальный синий/красный круг, чтобы он не отображался вокруг фигурки персонажа.
        /// </summary>
        public void ApplyPlayerVisualClass()
        {
            if (playerTransform == null)
            {
                Debug.LogWarning("[LANDING SYS] playerTransform не назначен. Невозможно обновить визуальную модель.");
                return;
            }

            // Кэшируем оригинальный меш плейсхолдера, если не кэшировали ранее
            CachePlaceholderMesh();

            // 1. Очищаем любые ранее инстанцированные временные 3D-модели (Player_Model_Visual), если они есть
            var childrenToDestroy = new System.Collections.Generic.List<GameObject>();
            foreach (Transform child in playerTransform)
            {
                if (child.name == "Player_Model_Visual")
                {
                    childrenToDestroy.Add(child.gameObject);
                }
            }

            foreach (var childGo in childrenToDestroy)
            {
                if (Application.isPlaying)
                {
                    Destroy(childGo);
                }
                else
                {
                    DestroyImmediate(childGo);
                }
            }

            // 2. Определяем выбранный класс игрока (в зависимости от playmode или editor preview)
            string charClass = "warrior";
            if (Application.isPlaying && SaveGameSystem.CurrentData != null && !string.IsNullOrEmpty(SaveGameSystem.CurrentData.characterClass))
            {
                charClass = SaveGameSystem.CurrentData.characterClass.ToLower();
            }
            else
            {
                charClass = editorPreviewClass.ToString().ToLower();
            }

            // 3. ПОЛНОСТЬЮ СКРЫВАЕМ ВИЗУАЛ ОРИГИНАЛЬНОГО КРУГА (Player_Placeholder сам по себе не должен отображаться)
            var mainRenderer = playerTransform.GetComponent<Renderer>();
            if (mainRenderer != null)
            {
                mainRenderer.enabled = false; // Отключаем рендеринг шара/круга
            }

            var mainMeshRenderer = playerTransform.GetComponent<MeshRenderer>();
            if (mainMeshRenderer != null)
            {
                mainMeshRenderer.enabled = false; // Отключаем рендеринг шара/круга
            }

            // 4. Если в инспекторе назначены префабы 3D-моделей классов, инстанцируем нужный префаб
            GameObject activePrefab = null;
            if (charClass.Contains("warrior") || charClass.Contains("воин") || charClass.Contains("voin"))
                activePrefab = warriorModelPrefab;
            else if (charClass.Contains("archer") || charClass.Contains("стрелок") || charClass.Contains("strelok"))
                activePrefab = archerModelPrefab;
            else if (charClass.Contains("mage") || charClass.Contains("маг") || charClass.Contains("mag"))
                activePrefab = mageModelPrefab;

            bool hasModel = false;

            if (activePrefab != null)
            {
                GameObject instantiated = Instantiate(activePrefab, playerTransform);
                instantiated.name = "Player_Model_Visual";
                instantiated.transform.localPosition = Vector3.zero;
                instantiated.transform.localRotation = Quaternion.identity;
                instantiated.transform.localScale = Vector3.one * customHeroModelScale;
                Debug.Log($"[LANDING SYS] Создали 3D-фигурку класса {charClass} на месте Player_Placeholder из назначенного префаба.");
                hasModel = true;
            }

            // 5. Также проверяем встроенные дочерние объекты (на случай, если пользователь закинул фигурки прямо под Player_Placeholder)
            foreach (Transform child in playerTransform)
            {
                if (child.name == "Player_Model_Visual") continue;

                string cName = child.name.ToLower();
                bool isWarriorModel = cName.Contains("warrior") || cName.Contains("voin") || cName.Contains("warrior_") || cName.Contains("voin_");
                bool isArcherModel = cName.Contains("archer") || cName.Contains("strelok") || cName.Contains("лучник") || cName.Contains("streloc") || cName.Contains("archer_");
                bool isMageModel = cName.Contains("mage") || cName.Contains("mag") || cName.Contains("маг") || cName.Contains("mage_") || cName.Contains("mag_");

                if (isWarriorModel || isArcherModel || isMageModel)
                {
                    bool shouldBeActive = false;
                    if (isWarriorModel && (charClass.Contains("warrior") || charClass.Contains("воин") || charClass.Contains("voin")))
                    {
                        shouldBeActive = true;
                        hasModel = true;
                    }
                    else if (isArcherModel && (charClass.Contains("archer") || charClass.Contains("стрелок") || charClass.Contains("strelok")))
                    {
                        shouldBeActive = true;
                        hasModel = true;
                    }
                    else if (isMageModel && (charClass.Contains("mage") || charClass.Contains("маг") || charClass.Contains("mag")))
                    {
                        shouldBeActive = true;
                        hasModel = true;
                    }

                    child.gameObject.SetActive(shouldBeActive);
                    Debug.Log($"[LANDING SYS] Нашли дочерний 3D объект {child.name}. Установили активность: {shouldBeActive} на основе класса {charClass}.");
                }
                else if (cName.Contains("placeholder") || cName.Contains("circle") || cName.Contains("ring") || cName.Contains("sphere") || cName.Contains("glow"))
                {
                    // Скрываем дочерние кольца/свечения/плейсхолдеры, которые мешают просмотру фигурок
                    child.gameObject.SetActive(false);
                    var childRends = child.GetComponentsInChildren<Renderer>(true);
                    foreach (var cr in childRends)
                    {
                        cr.enabled = false;
                    }
                    Debug.Log($"[LANDING SYS] Скрыли дочерний визуальный элемент плейсхолдера: {child.name}");
                }
            }

            // Если не нашли ни префаба, ни активных дочерних моделей, создаем красивейший процедурный аватар героя
            if (!hasModel)
            {
                CreateProceduralHeroVisual(playerTransform, charClass);
            }

            Debug.Log($"[LANDING SYS] Успешно применили визуальный 3D класс: {charClass}. Исходный круг-плейсхолдер скрыт.");
        }

        private void CreateProceduralHeroVisual(Transform parent, string charClass)
        {
            GameObject container = new GameObject("Player_Model_Visual");
            container.transform.SetParent(parent);
            container.transform.localPosition = Vector3.zero;
            container.transform.localRotation = Quaternion.identity;
            container.transform.localScale = Vector3.one * customHeroModelScale;

            // Цветовые константы
            Color primaryColor = new Color(0.95f, 0.75f, 0.1f);   // Воин: Золотой
            Color secondaryColor = new Color(0.6f, 0.65f, 0.7f);  // Воин: Серебряный
            Color weaponColor = new Color(0.9f, 0.9f, 0.9f);

            bool isWarrior = charClass.Contains("warrior") || charClass.Contains("воин") || charClass.Contains("voin");
            bool isArcher = charClass.Contains("archer") || charClass.Contains("стрелок") || charClass.Contains("strelok");
            bool isMage = charClass.Contains("mage") || charClass.Contains("маг") || charClass.Contains("mag");

            if (isArcher)
            {
                primaryColor = new Color(0.12f, 0.75f, 0.35f);    // Лучник: Лесной зеленый
                secondaryColor = new Color(0.55f, 0.38f, 0.22f);  // Лучник: Кожаный коричневый
                weaponColor = new Color(0.1f, 0.8f, 0.75f);       // Бирюзовый лук
            }
            else if (isMage)
            {
                primaryColor = new Color(0.6f, 0.2f, 0.85f);      // Маг: Космический фиолетовый
                secondaryColor = new Color(0.15f, 0.1f, 0.35f);   // Маг: Темно-синяя мантия
                weaponColor = new Color(0f, 0.85f, 1f);           // Светящийся синий кристалл
            }

            // 1. Пьедестал под фигурку
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            DestroyImmediate(pedestal.GetComponent<Collider>());
            pedestal.transform.SetParent(container.transform);
            pedestal.transform.localPosition = new Vector3(0f, 0.04f, 0f);
            pedestal.transform.localScale = new Vector3(0.7f, 0.04f, 0.7f);
            pedestal.GetComponent<Renderer>().material = CreateProceduralMaterial(new Color(0.2f, 0.22f, 0.25f), 0.8f, 0.8f);

            // 2. Туловище (броня/роба)
            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            DestroyImmediate(torso.GetComponent<Collider>());
            torso.transform.SetParent(container.transform);
            torso.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            torso.transform.localScale = new Vector3(0.35f, 0.4f, 0.35f);
            torso.GetComponent<Renderer>().material = CreateProceduralMaterial(primaryColor, 0.5f, 0.6f);

            // 3. Наплечники
            GameObject lShoulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            DestroyImmediate(lShoulder.GetComponent<Collider>());
            lShoulder.transform.SetParent(container.transform);
            lShoulder.transform.localPosition = new Vector3(-0.25f, 0.75f, 0f);
            lShoulder.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            lShoulder.GetComponent<Renderer>().material = CreateProceduralMaterial(secondaryColor, 0.9f, 0.8f);

            GameObject rShoulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            DestroyImmediate(rShoulder.GetComponent<Collider>());
            rShoulder.transform.SetParent(container.transform);
            rShoulder.transform.localPosition = new Vector3(0.25f, 0.75f, 0f);
            rShoulder.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            rShoulder.GetComponent<Renderer>().material = CreateProceduralMaterial(secondaryColor, 0.9f, 0.8f);

            // 4. Голова
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            DestroyImmediate(head.GetComponent<Collider>());
            head.transform.SetParent(container.transform);
            head.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            head.transform.localScale = new Vector3(0.26f, 0.26f, 0.26f);
            head.GetComponent<Renderer>().material = CreateProceduralMaterial(new Color(0.96f, 0.8f, 0.68f), 0.1f, 0.2f); // Телесный цвет

            // 5. Корона / Шлем / Шляпа мага
            if (isWarrior)
            {
                // Золотая корона
                GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                DestroyImmediate(crown.GetComponent<Collider>());
                crown.transform.SetParent(container.transform);
                crown.transform.localPosition = new Vector3(0f, 1.22f, 0f);
                crown.transform.localScale = new Vector3(0.22f, 0.05f, 0.22f);
                crown.GetComponent<Renderer>().material = CreateProceduralMaterial(primaryColor, 0.95f, 0.9f);
            }
            else if (isArcher)
            {
                // Капюшон следопыта
                GameObject hood = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                DestroyImmediate(hood.GetComponent<Collider>());
                hood.transform.SetParent(container.transform);
                hood.transform.localPosition = new Vector3(0f, 1.15f, -0.05f);
                hood.transform.localScale = new Vector3(0.28f, 0.22f, 0.28f);
                hood.GetComponent<Renderer>().material = CreateProceduralMaterial(primaryColor, 0.1f, 0.1f);
            }
            else if (isMage)
            {
                // Остроконечная шляпа
                GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                DestroyImmediate(cone.GetComponent<Collider>());
                cone.transform.SetParent(container.transform);
                cone.transform.localPosition = new Vector3(0f, 1.26f, 0f);
                cone.transform.localScale = new Vector3(0.18f, 0.15f, 0.18f);
                cone.GetComponent<Renderer>().material = CreateProceduralMaterial(secondaryColor, 0.1f, 0.2f);
            }

            // 6. Оружие
            if (isWarrior)
            {
                // Меч
                GameObject sword = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                DestroyImmediate(sword.GetComponent<Collider>());
                sword.transform.SetParent(container.transform);
                sword.transform.localPosition = new Vector3(0.32f, 0.65f, 0.22f);
                sword.transform.localRotation = Quaternion.Euler(30f, 0f, -15f);
                sword.transform.localScale = new Vector3(0.025f, 0.35f, 0.025f);
                sword.GetComponent<Renderer>().material = CreateProceduralMaterial(weaponColor, 0.95f, 0.9f);

                // Щит
                GameObject shield = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                DestroyImmediate(shield.GetComponent<Collider>());
                shield.transform.SetParent(container.transform);
                shield.transform.localPosition = new Vector3(-0.32f, 0.55f, 0.15f);
                shield.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                shield.transform.localScale = new Vector3(0.22f, 0.02f, 0.22f);
                shield.GetComponent<Renderer>().material = CreateProceduralMaterial(new Color(0.4f, 0.25f, 0.1f), 0.5f, 0.4f);
            }
            else if (isArcher)
            {
                // Лук
                GameObject bow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                DestroyImmediate(bow.GetComponent<Collider>());
                bow.transform.SetParent(container.transform);
                bow.transform.localPosition = new Vector3(0.28f, 0.55f, 0.15f);
                bow.transform.localRotation = Quaternion.Euler(15f, 0f, 45f);
                bow.transform.localScale = new Vector3(0.02f, 0.4f, 0.02f);
                bow.GetComponent<Renderer>().material = CreateProceduralMaterial(weaponColor, 0.7f, 0.8f);
            }
            else if (isMage)
            {
                // Посох
                GameObject staff = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                DestroyImmediate(staff.GetComponent<Collider>());
                staff.transform.SetParent(container.transform);
                staff.transform.localPosition = new Vector3(0.28f, 0.7f, 0.15f);
                staff.transform.localRotation = Quaternion.Euler(10f, 0f, -5f);
                staff.transform.localScale = new Vector3(0.022f, 0.6f, 0.022f);
                staff.GetComponent<Renderer>().material = CreateProceduralMaterial(new Color(0.45f, 0.3f, 0.15f), 0.1f, 0.1f);

                // Светящийся Кристалл
                GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                DestroyImmediate(crystal.GetComponent<Collider>());
                crystal.transform.SetParent(container.transform);
                crystal.transform.localPosition = new Vector3(0.28f, 1.34f, 0.15f);
                crystal.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
                crystal.GetComponent<Renderer>().material = CreateProceduralMaterial(weaponColor, 0.3f, 0.9f, true);
            }

            Debug.Log($"[LANDING SYS] Создана великолепная процедурная 3D-модель класса {charClass} для героя!");
        }

        private Material CreateProceduralMaterial(Color color, float metallic, float smoothness, bool emissive = false)
        {
            Shader targetShader = Shader.Find("Universal Render Pipeline/Lit");
            if (targetShader == null) targetShader = Shader.Find("Standard");
            if (targetShader == null) targetShader = Shader.Find("Diffuse");
            if (targetShader == null) targetShader = Shader.Find("Mobile/Diffuse");

            Material mat = new Material(targetShader != null ? targetShader : Shader.Find("Standard"));
            mat.name = "M_Procedural_Hero_" + ColorUtility.ToHtmlStringRGBA(color);

            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);

            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);

            if (emissive)
            {
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", color * 2.0f);
                    mat.EnableKeyword("_EMISSION");
                }
                if (mat.HasProperty("_EmissiveColor"))
                {
                    mat.SetColor("_EmissiveColor", color * 2.0f);
                }
            }

            return mat;
        }
    }
}
