using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace FateContinent
{
    /// <summary>
    /// Разработчик: Fate Continent (Континент Судьбы) • Версия v18.10.1
    /// Координатор входа в геймплей с эффектом плавного проявления (Fade In) из темноты 
    /// и автоматическим запуском диалога с Аэлиссой в зависимости от выбранного героя.
    /// </summary>
    public class FateGameplayIntro_Manager : MonoBehaviour
    {
        public static FateGameplayIntro_Manager Instance { get; private set; }

        [Header("🖤 Настройки Плавного Проявления (Fade)")]
        [Tooltip("Продолжительность затухания темного экрана в секундах")]
        public float fadeDuration = 2.0f;
        
        [Tooltip("Задержка перед началом проявления темного экрана")]
        public float delayBeforeFade = 0.5f;

        [Tooltip("Цвет заставки (по умолчанию черный)")]
        public Color fadeColor = Color.black;

        [Header("⛓️ Ссылка на затемняющий UI (Опционально)")]
        [Tooltip("Если не назначен, скрипт автоматически создаст временный холст для плавного проявления")]
        public Image manualFadeImage;

        private Image runtimeFadeImage;
        private CanvasGroup runtimeCanvasGroup;

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

            // Самовосстановление: если ручной оверлей не задан, создаем его динамически во избежание NullReferenceException!
            if (manualFadeImage == null)
            {
                SetupDynamicFadeOverlay();
            }
        }

        private void Start()
        {
            // Начинаем плавный переход и последующий диалог
            StartCoroutine(PerformGameplayIntroSequence());
        }

        /// <summary>
        /// Динамическое и безопасное создание полноэкранного черного оверлея на старте.
        /// Элемент создается поверх всего, плавно рассеивается и удаляется для оптимизации.
        /// </summary>
        private void SetupDynamicFadeOverlay()
        {
            Debug.Log("[FATE INTRO] Ручной оверлей затемнения не назначен. Инициализируем динамический Zenith Fade Screen...");

            GameObject fadeCanvasGov = new GameObject("Zenith_DynamicFadeCanvas");
            Canvas canvas = fadeCanvasGov.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1400; // Поверх диалогов (999), но под меню паузы (1500)

            fadeCanvasGov.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            fadeCanvasGov.AddComponent<GraphicRaycaster>();

            GameObject imageGov = new GameObject("BlackOverlayImage");
            imageGov.transform.SetParent(fadeCanvasGov.transform, false);
            
            RectTransform rect = imageGov.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            runtimeFadeImage = imageGov.AddComponent<Image>();
            runtimeFadeImage.color = fadeColor;
            
            runtimeCanvasGroup = imageGov.AddComponent<CanvasGroup>();
            runtimeCanvasGroup.alpha = 1.0f; // Полная темнота при старте

            // Предотвращаем уничтожение временного холста при переходах в начале кадра
            DontDestroyOnLoad(fadeCanvasGov);
        }

        private IEnumerator PerformGameplayIntroSequence()
        {
            // Считываем оверлей (ручной или программный)
            Image activeImage = manualFadeImage != null ? manualFadeImage : runtimeFadeImage;
            CanvasGroup activeGroup = manualFadeImage != null ? manualFadeImage.GetComponent<CanvasGroup>() : runtimeCanvasGroup;

            if (activeImage != null)
            {
                // Задаем начальный цвет и блокируем клики, пока идет проявление экрана
                activeImage.color = fadeColor;
                activeImage.raycastTarget = true;
            }

            if (activeGroup != null)
            {
                activeGroup.alpha = 1.0f;
            }

            Debug.Log("[FATE INTRO] Начало заставки. Экран затемнен. Ожидаем delayBeforeFade...");
            yield return new WaitForSeconds(delayBeforeFade);

            // Инициируем диалог СРАЗУ на старте рассеивания темноты, чтобы избежать простоя ("пустого экрана")!
            Debug.Log("[FATE INTRO] Начинаем рассеивание темноты и сразу инициализируем диалог с Аэлиссой...");
            if (DialogueSystem_Manager.Instance != null)
            {
                DialogueSystem_Manager.Instance.StartDialogue(0);
                Debug.Log("[FATE INTRO] Скрипт вовремя вызвал DialogueSystem_Manager.Instance.StartDialogue(0)!");
            }
            else
            {
                Debug.LogWarning("[FATE INTRO] Ожидаемый DialogueSystem_Manager.Instance не обнаружен на сцене! Попробуем запустить поиск.");
                DialogueSystem_Manager dm = FindFirstObjectByType<DialogueSystem_Manager>();
                if (dm != null)
                {
                    dm.StartDialogue(0);
                }
                else
                {
                    Debug.LogError("[FATE INTRO] КРИТИЧЕСКАЯ ОШИБКА: DialogueSystem_Manager отсутствует на сцене! Путнику некому дать задание.");
                }
            }

            // Плавно рассеиваем темноту
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / fadeDuration;
                float alpha = Mathf.Lerp(1.0f, 0.0f, progress);

                if (activeGroup != null)
                {
                    activeGroup.alpha = alpha;
                }
                else if (activeImage != null)
                {
                    Color col = activeImage.color;
                    col.a = alpha;
                    activeImage.color = col;
                }

                yield return null;
            }

            // Дозачистка: убираем видимость полностью
            if (activeGroup != null) activeGroup.alpha = 0f;
            if (activeImage != null)
            {
                Color col = activeImage.color;
                col.a = 0f;
                activeImage.color = col;
                activeImage.raycastTarget = false; // Возвращаем клики в игру
            }

            Debug.Log("[FATE INTRO] Темнота благополучно рассеялась. Диалог и сцена полностью проявлены.");

            // Если оверлей был создан динамически — удаляем его из памяти для повышения производительности
            if (manualFadeImage == null && activeImage != null)
            {
                Transform canvasParent = activeImage.transform.parent;
                if (canvasParent != null)
                {
                    Destroy(canvasParent.gameObject, 1.0f);
                }
            }
        }
    }
}
