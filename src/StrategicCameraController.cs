using UnityEngine;

namespace FateContinent
{
    /// <summary>
    /// Разработчик: Fate Continent (Континент Судьбы) • Версия v18.11.7
    /// Скрипт свободного управления стратегической камерой после высадки на континент.
    /// Поддерживает:
    /// - Классическое движение WASD / стрелочки
    /// - Перетаскивание карты зажатием ПРАВОЙ кнопки мыши (RMB) или СРЕДНЕЙ кнопки мыши
    /// - Плавный зум колесиком мыши (Scroll Wheel) с регулировкой угла наклона (Pitch)
    /// - Ускорение движения при зажатом Shift
    /// - Строгие физические границы полета (Bounds), чтобы не улететь в пустоту
    /// - Плавную иннерцию (Lerp) для кинематографического скольжения
    /// </summary>
    public class StrategicCameraController : MonoBehaviour
    {
        public static StrategicCameraController Instance { get; private set; }

        [Header("🎛️ Общие настройки")]
        [Tooltip("Разрешено ли игроку управлять камерой в данный момент")]
        public bool isControlEnabled = false;

        [Header("🚀 Свойства перемещения")]
        [Tooltip("Базовая скорость перемещения камеры")]
        public float baseMoveSpeed = 15.0f;
        [Tooltip("Множитель скорости при зажатом Shift")]
        public float shiftSpeedMultiplier = 2.5f;
        [Tooltip("Плавность остановки (чем ниже, тем более плавной будет инерция)")]
        public float movementSmoothing = 10.0f;

        [Header("🖱️ Перетаскивание мышью")]
        [Tooltip("Чувствительность перетаскивания карты правой кнопкой мыши")]
        public float dragSensitivity = 1.0f;

        [Header("🔍 Настройки масштабирования (Zoom)")]
        [Tooltip("Текущий уровень приближения")]
        public float currentZoom = 0.5f; // От 0.0 (максимально близко) до 1.0 (максимально высоко)
        [Tooltip("Минимальная высота камеры (максимальный зум)")]
        public float minHeight = 5.0f;
        [Tooltip("Максимальная высота камеры (минимальный зум)")]
        public float maxHeight = 40.0f;
        [Tooltip("Чувствительность колесика мыши")]
        public float zoomSensitivity = 5.0f;
        [Tooltip("Плавность приближения")]
        public float zoomSmoothing = 8.0f;

        [Header("📐 Динамический наклон (Tilt)")]
        [Tooltip("Угол наклона камеры при максимальном приближении (низко)")]
        public float tiltAtMinHeight = 35.0f;
        [Tooltip("Угол наклона камеры при максимальном отдалении (высоко)")]
        public float tiltAtMaxHeight = 65.0f;

        [Header("🚧 Границы игровой карты (Borders)")]
        [Tooltip("Ограничение по оси X (влево/вправо)")]
        public Vector2 xBounds = new Vector2(-150f, 150f);
        [Tooltip("Ограничение по оси Z (вперед/назад)")]
        public Vector2 zBounds = new Vector2(-150f, 150f);

        // Внутренние переменные для плавного демпфирования
        private Vector3 targetPosition;
        private float targetZoom;
        private Vector3 lastDragMousePosition;

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

        private void Start()
        {
            // Считываем стартовые координаты
            targetPosition = transform.position;
            // Рассчитываем начальный зум на основе текущей высоты камеры
            currentZoom = Mathf.InverseLerp(minHeight, maxHeight, transform.position.y);
            targetZoom = currentZoom;
        }

        private void Update()
        {
            if (!isControlEnabled) return;

            HandleKeyboardMovement();
            HandleMouseDrag();
            HandleZoom();
            ApplyCameraTransforms();
        }

        /// <summary>
        /// Мгновенная фокусировка камеры на конкретную точку 3D мира
        /// </summary>
        public void FocusOnPoint(Vector3 worldPoint, Vector3 offset)
        {
            targetPosition = worldPoint + offset;
            // Автоматически ограничиваем в рамках дозволенных границ
            targetPosition.x = Mathf.Clamp(targetPosition.x, xBounds.x, xBounds.y);
            targetPosition.z = Mathf.Clamp(targetPosition.z, zBounds.x, zBounds.y);
            
            // Задаем средний зум по умолчанию при фокусировке
            targetZoom = 0.4f; 
            
            // Если игрок еще не управлял камерой, переносим физически сразу, чтобы не было "дергания"
            if (!isControlEnabled)
            {
                transform.position = targetPosition;
                transform.rotation = Quaternion.Euler(Mathf.Lerp(tiltAtMinHeight, tiltAtMaxHeight, targetZoom), transform.rotation.eulerAngles.y, 0f);
            }
        }

        private void HandleKeyboardMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal"); // A, D, Left, Right
            float vertical = Input.GetAxisRaw("Vertical");     // W, S, Up, Down

            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

            if (inputDirection.magnitude > 0.1f)
            {
                // Рассчитываем скорость с учетом Shift
                float speed = baseMoveSpeed;
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    speed *= shiftSpeedMultiplier;
                }

                // Вращаем направление движения относительно поворота камеры по горизонтали (Y),
                // чтобы клавиша W всегда двигала камеру вперед по экрану, а не по глобальным осям.
                Vector3 cameraForward = transform.forward;
                cameraForward.y = 0f;
                cameraForward.Normalize();

                Vector3 cameraRight = transform.right;
                cameraRight.y = 0f;
                cameraRight.Normalize();

                Vector3 movement = (cameraForward * inputDirection.z + cameraRight * inputDirection.x) * speed * Time.deltaTime;
                targetPosition += movement;

                // Зажимаем целевую позицию в границах карты
                targetPosition.x = Mathf.Clamp(targetPosition.x, xBounds.x, xBounds.y);
                targetPosition.z = Mathf.Clamp(targetPosition.z, zBounds.x, zBounds.y);
            }
        }

        private void HandleMouseDrag()
        {
            // Зажатие правой (1) или средней (2) клавиши мыши для перетаскивания карты
            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                lastDragMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
            {
                Vector3 currentMousePos = Input.mousePosition;
                Vector3 delta = currentMousePos - lastDragMousePosition;

                if (delta.magnitude > 0.1f)
                {
                    // Рассчитываем коэффициенты перетаскивания относительно текущей высоты камеры (чем выше, тем быстрее тянем)
                    float heightFactor = transform.position.y / maxHeight;
                    float finalSensitivity = dragSensitivity * 0.1f * (heightFactor + 0.2f);

                    Vector3 cameraRight = transform.right;
                    cameraRight.y = 0f;
                    cameraRight.Normalize();

                    Vector3 cameraForward = transform.forward;
                    cameraForward.y = 0f;
                    cameraForward.Normalize();

                    // Перемещаем цель камеры в противоположную сторону сдвига мыши
                    Vector3 dragMovement = (-cameraRight * delta.x - cameraForward * delta.y) * finalSensitivity;
                    targetPosition += dragMovement;

                    targetPosition.x = Mathf.Clamp(targetPosition.x, xBounds.x, xBounds.y);
                    targetPosition.z = Mathf.Clamp(targetPosition.z, zBounds.x, zBounds.y);

                    lastDragMousePosition = currentMousePos;
                }
            }
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                targetZoom -= scroll * zoomSensitivity;
                targetZoom = Mathf.Clamp01(targetZoom);
            }
        }

        private void ApplyCameraTransforms()
        {
            // 1. Плавно интерполируем зум
            currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomSmoothing);

            // 2. Рассчитываем целевую высоту по оси Y на основе интерполированного зума
            float targetHeight = Mathf.Lerp(minHeight, maxHeight, currentZoom);
            targetPosition.y = targetHeight;

            // 3. Плавно интерполируем позицию всей камеры
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * movementSmoothing);

            // 4. Плавно интерполируем угол наклона (Tilt) — при приближении камера наклоняется к горизонту, при отдалении смотрит строго вниз
            float targetTilt = Mathf.Lerp(tiltAtMinHeight, tiltAtMaxHeight, currentZoom);
            Quaternion targetRotation = Quaternion.Euler(targetTilt, transform.rotation.eulerAngles.y, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * zoomSmoothing);
        }
    }
}
