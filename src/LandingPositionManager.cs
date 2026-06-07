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
        public Vector3 cameraOffset = new Vector3(0, 15f, -10f); // Ракурс сверху на точку

        private void OnValidate()
        {
            InitializeDefaultPoints();
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

            landingPoints[3].zoneID = "Crags";
            landingPoints[3].zoneName = "Грозовые Кряжи";

            // Автоматический поиск пустышек, чтобы ничего не сбрасывалось и не требовалось перетаскивать вручную
            AutoFindSpawnAnchors();
        }

        private void AutoFindSpawnAnchors()
        {
            if (landingPoints == null) return;

            string[] defaultAnchorNames = new string[] { 
                "Wastes_SpawnPoint", 
                "Peak_SpawnPoint", 
                "Ruins_SpawnPoint", 
                "Crags_SpawnPoint" 
            };

            for (int i = 0; i < landingPoints.Length; i++)
            {
                if (landingPoints[i] == null) continue;

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

            // Гарантируем корректное заполнение при старте
            InitializeDefaultPoints();

            // Автоматическое нахождение 3D-карты в Hierarchy на старте
            if (continentObject == null)
            {
                continentObject = GameObject.Find("Континент");
                if (continentObject == null)
                {
                    // Ищем по части имени
                    foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
                    {
                        if (go.name.Contains("Континент") && go.scene.isLoaded)
                        {
                            continentObject = go;
                            break;
                        }
                    }
                }

                if (continentObject != null)
                {
                    Debug.Log("<color=#00FFCC>[LANDING SYS]</color> Скрипт автоматически нашел 3D-модель Континента.");
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
        }

        private void Start()
        {
            // СВЕРХВАЖНО: Изначально полностью выключаем 3D-карту и героя, чтобы они не лезли на задний план вступительных диалогов Аэлиссы!
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
        }

        /// <summary>
        /// Вызывается автоматически, когда игрок выбирает точку высадки и завершает диалог!
        /// </summary>
        /// <param name="zoneIndex">Индекс выбранной зоны (0: Кровавые Пустоши, 1: Ледяной Пик, 2: Древние Руины, 3: Грозовые Кряжи)</param>
        public void DispatchLanding(int zoneIndex)
        {
            // Включаем 3D-карту и героя обратно при начале перемещения камеры
            if (continentObject != null)
            {
                continentObject.SetActive(true);
                Debug.Log("<color=#00FFCC>[LANDING SYS]</color> Активируем 3D-модель Континента!");
            }

            if (playerTransform != null)
            {
                playerTransform.gameObject.SetActive(true);
                Debug.Log("<color=#00FFCC>[LANDING SYS]</color> Активируем Player_Placeholder!");
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

            // 2. Телепортируем игрока на физические координаты 3D Континента
            if (point.spawnAnchor != null)
            {
                if (playerTransform != null)
                {
                    playerTransform.position = point.spawnAnchor.position;
                    playerTransform.rotation = point.spawnAnchor.rotation;
                    Debug.Log($"[LANDING SYS] Игрок успешно перенесен в 3D координаты: {point.spawnAnchor.position}");
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
                        EnableStrategicCamera(point.spawnAnchor.position);
                    }
                }
            }
            else
            {
                Debug.LogError($"[LANDING SYS] Критическая ошибка: Spawn Anchor для точки '{point.zoneName}' равен Null! Укажите пустышку Transform в инспекторе.");
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
            EnableStrategicCamera(lookAtPoint);
        }
    }
}
