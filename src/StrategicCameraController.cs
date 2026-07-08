using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
        public float currentZoom = 0.3f; // От 0.0 (максимально близко) до 1.0 (максимально высоко)
        [Tooltip("Минимальная высота камеры (максимальный зум)")]
        public float minHeight = 0.6f;   // Настроено под уровень красивого приближения карты
        [Tooltip("Максимальная высота камеры (минимальный зум)")]
        public float maxHeight = 8.0f;  // Настроено, чтобы карта не отдалялась слишком далеко
        [Tooltip("Чувствительность колесика мыши")]
        public float zoomSensitivity = 3.0f;
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

        [Header("📐 Автоматический расчет границ")]
        [Tooltip("Автоматически ограничивать по размерам New_Kontinent, если он найден")]
        public bool autoFitToContinent = true;
        [Tooltip("Запас (padding) при автоматическом расчете границ")]
        public float autoFitPadding = 5.0f;

        [Header("🖥️ Прокрутка по краям экрана (Edge Scrolling)")]
        [Tooltip("Включить прокрутку камеры при подведении мыши к краю экрана")]
        public bool useEdgeScrolling = true;
        [Tooltip("Ширина активной зоны у края экрана (в пикселях)")]
        public float edgeScrollBorder = 20f;
        [Tooltip("Скорость прокрутки по краям")]
        public float edgeScrollSpeed = 12.0f;

        // Внутренние переменные для плавного демпфирования
        private Vector3 targetPosition;
        private float targetZoom;
        private Vector3 lastDragMousePosition;

        private float originalEdgeScrollSpeed;
        private float originalDragSensitivity;
        private bool hasInitializedOriginals = false;

        private void InitializeOriginals()
        {
            if (hasInitializedOriginals) return;
            originalEdgeScrollSpeed = edgeScrollSpeed;
            originalDragSensitivity = dragSensitivity;
            hasInitializedOriginals = true;
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
            }
        }

        private void Start()
        {
            InitializeOriginals();

            // Принудительно отключаем свободное перемещение камеры во время стартовых диалогов Аэлиссы,
            // чтобы пользователь не улетел в пустоту, пока идет вступительный разговор и выбор зон.
            isControlEnabled = false;

            // Автоматическое исправление завышенных высот камеры из-за старой сериализации в инспекторе
            if (minHeight >= 3.0f)
            {
                minHeight = 0.6f;
                Debug.Log($"<color=#00FFCC>[CAMERA CALIBRATION]</color> Скорректировали минимальную высоту камеры с {minHeight}f до идеальных 0.6f.");
            }
            if (maxHeight >= 20.0f)
            {
                maxHeight = 8.0f;
                Debug.Log($"<color=#00FFCC>[CAMERA CALIBRATION]</color> Скорректировали максимальную высоту камеры с {maxHeight}f до идеальных 8.0f.");
            }

            // Пытаемся автоматически подстроить границы под размеры New_Kontinent
            AutoFitBounds();

            // Считываем стартовые координаты
            targetPosition = transform.position;
            // Рассчитываем начальный зум на основе текущей высоты камеры
            currentZoom = Mathf.InverseLerp(minHeight, maxHeight, transform.position.y);
            targetZoom = currentZoom;
        }

        private void Update()
        {
            float mouseSensitivity = PlayerPrefs.GetFloat("FATE_MOUSE_SENSITIVITY", 1.0f);
            if (!hasInitializedOriginals)
            {
                InitializeOriginals();
            }
            edgeScrollSpeed = originalEdgeScrollSpeed * mouseSensitivity;
            dragSensitivity = originalDragSensitivity * mouseSensitivity;

            if (!isControlEnabled) return;

            // [BLOCK CAMERA WHEN IN TOWN VIEW OR CASTLE DETAILS OPEN]
            // Предотвращаем любое скольжение или перетаскивание карты, если игрок взаимодействует с замком (своим или вражеским)
            if (FateCastleManager.Instance != null && (FateCastleManager.Instance.isTownViewActive || FateCastleManager.Instance.isDetailsOpen))
            {
                // Сбрасываем таргет-координаты на текущие, чтобы после выхода из панели камера не прыгала
                targetPosition = transform.position;
                return;
            }

            // Раз в секунду (приблизительно 60 кадров) пересчитываем границы, если континент масштабируется
            if (autoFitToContinent && Time.frameCount % 60 == 0)
            {
                AutoFitBounds();
            }

            HandleKeyboardMovement();
            HandleMouseDrag();
            HandleMouseEdgeScrolling();
            HandleZoom();
            ApplyCameraTransforms();
        }

        /// <summary>
        /// Автоматический расчет границ камеры по размеру New_Kontinent
        /// </summary>
        public void AutoFitBounds()
        {
            if (!autoFitToContinent) return;

            GameObject continent = GameObject.Find("New_Kontinent");
            if (continent != null)
            {
                Renderer[] renderers = continent.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds combinedBounds = renderers[0].bounds;
                    bool hasValidBound = false;

                    if (combinedBounds.size.magnitude > 0.1f) hasValidBound = true;

                    for (int i = 1; i < renderers.Length; i++)
                    {
                        if (renderers[i].bounds.size.magnitude > 0.1f)
                        {
                            if (!hasValidBound)
                            {
                                combinedBounds = renderers[i].bounds;
                                hasValidBound = true;
                            }
                            else
                            {
                                combinedBounds.Encapsulate(renderers[i].bounds);
                            }
                        }
                    }

                    if (hasValidBound)
                    {
                        xBounds = new Vector2(combinedBounds.min.x - autoFitPadding, combinedBounds.max.x + autoFitPadding);
                        zBounds = new Vector2(combinedBounds.min.z - autoFitPadding, combinedBounds.max.z + autoFitPadding);
                    }
                }
            }
        }

        /// <summary>
        /// Ограничивает позицию камеры так, чтобы точка фокуса (куда смотрит центр экрана на плоскости Y = 0)
        /// не выходила за пределы игровых границ xBounds и zBounds.
        /// </summary>
        public Vector3 ClampCameraPositionByGroundFocus(Vector3 camPos, float zoomVal)
        {
            float height = Mathf.Lerp(minHeight, maxHeight, zoomVal);
            float tilt = Mathf.Lerp(tiltAtMinHeight, tiltAtMaxHeight, zoomVal);
            
            // Направление взгляда камеры с учетом текущего наклона (Pitch)
            Quaternion rot = Quaternion.Euler(tilt, transform.rotation.eulerAngles.y, 0f);
            Vector3 forwardDir = rot * Vector3.forward;
            
            // Предотвращаем деление на ноль или взгляд горизонтально/вверх
            if (forwardDir.y >= -0.01f)
            {
                camPos.x = Mathf.Clamp(camPos.x, xBounds.x, xBounds.y);
                camPos.z = Mathf.Clamp(camPos.z, zBounds.x, zBounds.y);
                return camPos;
            }
            
            // Ищем расстояние вдоль вектора взгляда до плоскости Y = 0 (земля)
            float distanceToGround = -height / forwardDir.y;
            Vector3 groundPoint = camPos + forwardDir * distanceToGround;
            
            // Зажимаем фокус на земле в пределах дозволенных границ
            float clampedGroundX = Mathf.Clamp(groundPoint.x, xBounds.x, xBounds.y);
            float clampedGroundZ = Mathf.Clamp(groundPoint.z, zBounds.x, zBounds.y);
            
            // Из зажатой точки на земле вычисляем обратную позицию камеры
            Vector3 clampedCamPos = new Vector3(clampedGroundX, 0f, clampedGroundZ) - forwardDir * distanceToGround;
            clampedCamPos.y = height; // Удерживаем верную высоту
            
            return clampedCamPos;
        }

        /// <summary>
        /// Прокрутка камеры движением мыши к краям экрана (Pan / Edge Scroll)
        /// </summary>
        private void HandleMouseEdgeScrolling()
        {
            if (!useEdgeScrolling) return;

            float horizontal = 0f;
            float vertical = 0f;

            Vector2 mousePos = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null) mousePos = mouse.position.ReadValue();
#else
            mousePos = Input.mousePosition;
#endif

            if (mousePos.x >= 0 && mousePos.x <= Screen.width && mousePos.y >= 0 && mousePos.y <= Screen.height)
            {
                if (mousePos.x < edgeScrollBorder) horizontal = -1f;
                else if (mousePos.x > Screen.width - edgeScrollBorder) horizontal = 1f;

                if (mousePos.y < edgeScrollBorder) vertical = -1f;
                else if (mousePos.y > Screen.height - edgeScrollBorder) vertical = 1f;
            }

            if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
            {
                Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
                
                Vector3 cameraForward = transform.forward;
                cameraForward.y = 0f;
                cameraForward.Normalize();

                Vector3 cameraRight = transform.right;
                cameraRight.y = 0f;
                cameraRight.Normalize();

                Vector3 movement = (cameraForward * inputDirection.z + cameraRight * inputDirection.x) * edgeScrollSpeed * Time.deltaTime;
                targetPosition += movement;

                targetPosition = ClampCameraPositionByGroundFocus(targetPosition, targetZoom);
            }
        }

        /// <summary>
        /// Мгновенная фокусировка камеры на конкретную точку 3D мира
        /// </summary>
        public void FocusOnPoint(Vector3 worldPoint, Vector3 offset)
        {
            targetPosition = worldPoint + offset;
            
            // Задаем средний зум по умолчанию при фокусировке и мгновенно сбрасываем текущий интерполированный зум,
            // чтобы избежать моментального взлета и улета камеры вверх!
            targetZoom = 0.4f; 
            currentZoom = 0.4f;

            // Ограничиваем в рамках дозволенных границ focus-точку
            targetPosition = ClampCameraPositionByGroundFocus(targetPosition, targetZoom);
            
            // Если игрок еще не управлял камерой, переносим физически сразу, чтобы не было "дергания"
            if (!isControlEnabled)
            {
                transform.position = targetPosition;
                transform.rotation = Quaternion.Euler(Mathf.Lerp(tiltAtMinHeight, tiltAtMaxHeight, targetZoom), transform.rotation.eulerAngles.y, 0f);
            }
        }

        private void HandleKeyboardMovement()
        {
            float horizontal = 0f;
            float vertical = 0f;

#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) horizontal = -1f;
                else if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal = 1f;

                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) vertical = 1f;
                else if (kb.sKey.isPressed || kb.downArrowKey.isPressed) vertical = -1f;
            }
#else
            horizontal = Input.GetAxisRaw("Horizontal"); // A, D, Left, Right
            vertical = Input.GetAxisRaw("Vertical");     // W, S, Up, Down
#endif

            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

            if (inputDirection.magnitude > 0.1f)
            {
                // Рассчитываем скорость с учетом Shift
                float speed = baseMoveSpeed;
                
#if ENABLE_INPUT_SYSTEM
                bool shiftPressed = kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed);
#else
                bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif

                if (shiftPressed)
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

                // Зажимаем целевую позицию в границах карты (по точке взгляда)
                targetPosition = ClampCameraPositionByGroundFocus(targetPosition, targetZoom);
            }
        }

        private void HandleMouseDrag()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse == null) return;

            bool rightPressed = mouse.rightButton.isPressed;
            bool middlePressed = mouse.middleButton.isPressed;
            Vector3 currentMousePos = mouse.position.ReadValue();

            if (mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame)
            {
                lastDragMousePosition = currentMousePos;
            }
#else
            bool rightPressed = Input.GetMouseButton(1);
            bool middlePressed = Input.GetMouseButton(2);
            Vector3 currentMousePos = Input.mousePosition;

            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                lastDragMousePosition = currentMousePos;
            }
#endif

            if (rightPressed || middlePressed)
            {
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

                    targetPosition = ClampCameraPositionByGroundFocus(targetPosition, targetZoom);

                    lastDragMousePosition = currentMousePos;
                }
            }
        }

        private void HandleZoom()
        {
            float scroll = 0f;
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null)
            {
                scroll = mouse.scroll.ReadValue().y * 0.005f; // ...
            }
#else
            scroll = Input.GetAxis("Mouse ScrollWheel");
#endif
            if (Mathf.Abs(scroll) > 0.001f)
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

            // 2.1. Дополнительно зажимаем саму целевую позицию к границам карты (по точке взгляда)
            targetPosition = ClampCameraPositionByGroundFocus(targetPosition, currentZoom);

            // 3. Плавно интерполируем позицию всей камеры
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * movementSmoothing);

            // 3.1. Обеспечиваем гарантированное строгое удержание камеры в рамках границ (по точке взгляда)
            transform.position = ClampCameraPositionByGroundFocus(transform.position, currentZoom);

            // 4. Плавно интерполируем угол наклона (Tilt) — при приближении камера наклоняется к горизонту, при отдалении смотрит строго вниз
            float targetTilt = Mathf.Lerp(tiltAtMinHeight, tiltAtMaxHeight, currentZoom);
            Quaternion targetRotation = Quaternion.Euler(targetTilt, transform.rotation.eulerAngles.y, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * zoomSmoothing);
        }
    }
}
