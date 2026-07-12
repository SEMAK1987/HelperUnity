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

                    if (landingPoints != null && landingPoints.Length > 0)
                    {
                        int targetIndex = Mathf.Clamp(landedZone, 0, landingPoints.Length - 1);
                        LandingPoint point = landingPoints[targetIndex];
                        if (point.spawnAnchor != null)
                        {
                            float ox = PlayerPrefs.GetFloat($"PlayerOffset_X_{landedZone}", 0f);
                            float oy = PlayerPrefs.GetFloat($"PlayerOffset_Y_{landedZone}", 0.8f); // По умолчанию приподнят на 0.8 метра, чтобы не утонуть в замке!
                            float oz = PlayerPrefs.GetFloat($"PlayerOffset_Z_{landedZone}", 0f);

                            playerTransform.position = point.spawnAnchor.position + new Vector3(ox, oy, oz);
                            playerTransform.rotation = point.spawnAnchor.rotation;
                            Debug.Log($"[LANDING SYS] Геймплей активен: Игрок успешно восстановлен по координатам с оффсетом {new Vector3(ox, oy, oz)}: {playerTransform.position}");

                            // Направляем и фокусируем камеру на этой точке
                            if (mainCameraTransform != null)
                            {
                                mainCameraTransform.position = point.spawnAnchor.position + cameraOffset;
                                mainCameraTransform.LookAt(point.spawnAnchor.position);
                            }
                        }
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
            if (point.spawnAnchor != null)
            {
                if (playerTransform != null)
                {
                    float ox = PlayerPrefs.GetFloat($"PlayerOffset_X_{zoneIndex}", 0f);
                    float oy = PlayerPrefs.GetFloat($"PlayerOffset_Y_{zoneIndex}", 0.8f); // По умолчанию приподнят на 0.8 метра, чтобы не утонуть в замке!
                    float oz = PlayerPrefs.GetFloat($"PlayerOffset_Z_{zoneIndex}", 0f);

                    playerTransform.position = point.spawnAnchor.position + new Vector3(ox, oy, oz);
                    playerTransform.rotation = point.spawnAnchor.rotation;
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
                    Vector3 targetCameraPos = point.spawnAnchor.position + cameraOffset;
                    if (smoothCameraMove)
                    {
                        StartCoroutine(SmoothMoveCameraCoroutine(targetCameraPos, point.spawnAnchor.position, targetIndex));
                    }
                    else
                    {
                        mainCameraTransform.position = targetCameraPos;
                        mainCameraTransform.LookAt(point.spawnAnchor.position);
                        CompleteLandingAndStartDialogue(point.spawnAnchor.position);
                    }
                }
            }
            else
            {
                Debug.LogError($"[LANDING SYS] Критическая ошибка: Spawn Anchor для точки '{point.zoneName}' равен Null! Укажите пустышку Transform в инспекторе.");
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
                            if (i == actualPlayerRegion)
                            {
                                // Единственный выбранный квадрат высадки игрока красим в яркий синий неон игрока
                                Color targetColor = new Color(0.12f, 0.58f, 0.95f, 1.0f);
                                if (mr.material != null)
                                {
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
                            else
                            {
                                // Восстанавливаем оригинальный красивый текстурированный материал для ВСЕХ остальных регионов!
                                // Благодаря этому континент сохраняет свои оригинальные цвета и текстуры (верните назад как было).
                                if (originalRegionMaterials != null && originalRegionMaterials.ContainsKey(regionName))
                                {
                                    mr.sharedMaterial = originalRegionMaterials[regionName];
                                    if (mr.sharedMaterial != null)
                                    {
                                        // Сбрасываем цвет тинта на стандартный белый, чтобы вернуть оригинальные цвета текстур
                                        mr.sharedMaterial.color = Color.white;
                                        if (mr.sharedMaterial.HasProperty("_BaseColor"))
                                        {
                                            mr.sharedMaterial.SetColor("_BaseColor", Color.white);
                                        }
                                        if (mr.sharedMaterial.HasProperty("_Color"))
                                        {
                                            mr.sharedMaterial.SetColor("_Color", Color.white);
                                        }
                                    }
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
                int targetIndex = Mathf.Clamp(landedZone, 0, landingPoints.Length - 1);
                LandingPoint point = landingPoints[targetIndex];
                if (point.spawnAnchor != null)
                {
                    // Считываем текущую позицию игрока относительно spawnAnchor
                    Vector3 currentOffset = playerTransform.position - point.spawnAnchor.position;
                    
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

            if (activePrefab != null)
            {
                GameObject instantiated = Instantiate(activePrefab, playerTransform);
                instantiated.name = "Player_Model_Visual";
                instantiated.transform.localPosition = Vector3.zero;
                instantiated.transform.localRotation = Quaternion.identity;
                instantiated.transform.localScale = Vector3.one * customHeroModelScale;
                Debug.Log($"[LANDING SYS] Создали 3D-фигурку класса {charClass} на месте Player_Placeholder из назначенного префаба.");
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
                        shouldBeActive = true;
                    else if (isArcherModel && (charClass.Contains("archer") || charClass.Contains("стрелок") || charClass.Contains("strelok")))
                        shouldBeActive = true;
                    else if (isMageModel && (charClass.Contains("mage") || charClass.Contains("маг") || charClass.Contains("mag")))
                        shouldBeActive = true;

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

            Debug.Log($"[LANDING SYS] Успешно применили визуальный 3D класс: {charClass}. Исходный круг-плейсхолдер скрыт.");
        }
    }
}
