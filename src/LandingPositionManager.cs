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
        public LandingPoint[] landingPoints;

        [Header("🎥 Ссылки на объекты сцены")]
        [Tooltip("Объект игрока, который будет десантирован")]
        public Transform playerTransform;

        [Tooltip("Главная камера для плавной фокусировки")]
        public Transform mainCameraTransform;

        [Header("✨ Настройки плавного перемещения камеры")]
        public bool smoothCameraMove = true;
        public float cameraMoveSpeed = 3.0f;
        public Vector3 cameraOffset = new Vector3(0, 15f, -10f); // Ракурс сверху на точку

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Вызывается автоматически, когда игрок выбирает точку высадки и завершает диалог!
        /// </summary>
        /// <param name="zoneIndex">Индекс выбранной зоны (0: Кровавые Пустоши, 1: Ледяной Пик, 2: Древние Руины)</param>
        public void DispatchLanding(int zoneIndex)
        {
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
