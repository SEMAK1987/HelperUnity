using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FateContinent
{
    /// <summary>
    /// Контроллер мистического фона диалога в стиле Zenith (8K Ultra-High Density).
    /// Автоматически управляет отображением сгенерированных артов позади диалогов,
    /// устанавливает корректный порядок слоев (Sorting Order = -100) и подстраивается 
    /// под любые форматы разрешения экрана (Universal Sync), защищая от сброса.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class FateDialogueBackgroundController : MonoBehaviour
    {
        public static FateDialogueBackgroundController Instance { get; private set; }

        [Header("Настройки фона Zenith")]
        [Tooltip("Ссылка на дочернее изображение, содержащее текстуру фона")]
        [SerializeField] private Image backgroundImage;
        
        [Tooltip("Длительность плавного проявления фона (в секундах)")]
        [SerializeField] private float defaultFadeDuration = 1.2f;

        private Canvas targetCanvas;
        private CanvasScaler canvasScaler;
        private CanvasGroup canvasGroup;
        private Coroutine activeFadeRoutine;

        private void Awake()
        {
            // Одиночка (Singleton) с защитой от дублирования
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                ConfigureBackgroundProperties();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Программная инициализация и калибровка параметров Canvas для полной синхронизации разрешений
        /// </summary>
        public void ConfigureBackgroundProperties()
        {
            targetCanvas = GetComponent<Canvas>();
            if (targetCanvas != null)
            {
                // Принудительно устанавливаем режим отрисовки в Screen Space - Overlay (КОММЕНТИРУЕМ ДЛЯ ПРЕДОТВРАЩЕНИЯ ПЕРЕКРЫТИЯ КАМЕРЫ!)
                // targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                // [CRITICAL EXPLICIT RULE] Устанавливаем самый нижний порядок отрисовки, гарантирующий подложку
                targetCanvas.sortingOrder = -100;
                
                Debug.Log("<color=#00FF99>[FATE BACKGROUND]</color> Canvas настроен со значением Sorting Order = -100. Метод отрисовки сохранен из инспектора.");
            }

            // Получаем или добавляем CanvasScaler для Universal Resolution Sync
            canvasScaler = GetComponent<CanvasScaler>();
            if (canvasScaler == null)
            {
                canvasScaler = gameObject.AddComponent<CanvasScaler>();
            }

            if (canvasScaler != null)
            {
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0.5f; // Баланс для 16:9, 16:10 и ультрашироких экранов
            }

            // Получаем или добавляем CanvasGroup для управления альфа-каналом
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // Настройка backgroundImage для автоматического заполнения (Stretch-Stretch)
            if (backgroundImage == null)
            {
                // Ищем во вложенных объектах
                backgroundImage = GetComponentInChildren<Image>();
                if (backgroundImage == null)
                {
                    // Создаем динамический дочерний объект для спрайта фона
                    GameObject bgInner = new GameObject("Zenith_Background_Image_Dynamic");
                    bgInner.transform.SetParent(this.transform, false);
                    backgroundImage = bgInner.AddComponent<Image>();
                    
                    // Отключаем перехват кликов мыши, чтобы кнопки под фоном оставались кликабельными
                    backgroundImage.raycastTarget = false;
                }
            }

            if (backgroundImage != null)
            {
                RectTransform rt = backgroundImage.GetComponent<RectTransform>();
                if (rt != null)
                {
                    // Программное натяжение по всем четырем осям (Stretch-Stretch)
                    rt.anchorMin = new Vector2(0f, 0f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    rt.pivot = new Vector2(0.5f, 0.5f);
                }
                
                // Убеждаемся, что изображение имеет правильный рендеринг цвета
                backgroundImage.color = Color.white;
            }
        }

        private void Start()
        {
            // По умолчанию, если при старте открыт диалог 1-й фазы, показываем фон!
            if (DialogueSystem_Manager.Instance != null && DialogueSystem_Manager.Instance.IsDialogueActive && DialogueSystem_Manager.Instance.SelectedZoneIndex == 0)
            {
                ShowBackground(0.0f); // Показать мгновенно
            }
            else
            {
                // Иначе держим скрытым до вызова
                if (canvasGroup != null) canvasGroup.alpha = 0f;
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Мягко показать мистический фон
        /// </summary>
        public void ShowBackground(float duration = -1f)
        {
            gameObject.SetActive(true);
            float targetDuration = duration >= 0f ? duration : defaultFadeDuration;

            if (activeFadeRoutine != null) StopCoroutine(activeFadeRoutine);

            if (targetDuration <= 0f)
            {
                if (canvasGroup != null) canvasGroup.alpha = 1f;
            }
            else
            {
                activeFadeRoutine = StartCoroutine(FadeRoutine(1f, targetDuration));
            }
            
            Debug.Log("<color=#00FFFF>[FATE BACKGROUND]</color> Показ фона запущен (плавность: " + targetDuration + " сек).");
        }

        /// <summary>
        /// Плавное увядание фона при десантировании на 3D карту
        /// </summary>
        public void HideBackground(float duration = -1f)
        {
            float targetDuration = duration >= 0f ? duration : defaultFadeDuration;

            if (activeFadeRoutine != null) 
            {
                StopCoroutine(activeFadeRoutine);
                activeFadeRoutine = null;
            }

            // Если фоновый GameObject уже деактивирован, просто сбрасываем параметры и выходим
            if (!gameObject.activeInHierarchy)
            {
                if (canvasGroup != null) canvasGroup.alpha = 0f;
                return;
            }

            if (targetDuration <= 0f)
            {
                if (canvasGroup != null) canvasGroup.alpha = 0f;
                gameObject.SetActive(false);
            }
            else
            {
                activeFadeRoutine = StartCoroutine(FadeRoutine(0f, targetDuration, true));
            }

            Debug.Log("<color=#FF6600>[FATE BACKGROUND]</color> Увядание фона выполнено/запущено перед 3D десантированием на " + targetDuration + " сек.");
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration, bool disableOnEnd = false)
        {
            if (canvasGroup == null) yield break;

            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / duration);
                
                // Плавное сглаживание перехода (SmoothStep)
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime * normalizedTime * (3f - 2f * normalizedTime));
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;

            if (disableOnEnd && targetAlpha <= 0.05f)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
