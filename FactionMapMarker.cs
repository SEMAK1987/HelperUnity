using UnityEngine;
using UnityEngine.EventSystems;

/*
 * [FATE CONTINENT - ZENITH DIALOGUE & MAP SYSTEM v18.11.1]
 * Автономный C# компонент для интерактивных маркеров континентов и фракций.
 * Обеспечивает плавное свечение (Bloom/Emission), интерактивный ховер, затухание
 * и интеграцию со звуком через SettingsManager.
 */

namespace FateContinent
{
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    // [RequireComponent(typeof(Collider2D))] - УБРАНО для предотвращения ошибки "Can't add script" в редакторе Unity! Скрипт теперь добавляется безупречно.
    public class FactionMapMarker : MonoBehaviour
    {
        [Header("Настройки фракции")]
        public string factionName = "Синяя Империя";
        [TextArea(2, 4)]
        public string factionDescription = "Имперские земли под предводительством Аэлиссы Эльфийской.";
        
        [Header("Интеграция с диалогами")]
        [Tooltip("Индекс реплики в DialogueSystem_Manager, который запустится при клике (например, 3 для выбора локации)")]
        public int associatedDialogueIndex = 3;
        [Tooltip("Запускать ли диалоговое окно при клике на маркер")]
        public bool triggerDialogueOnClassClick = true;

        [Header("Визуальные эффекты свечения")]
        [Tooltip("Материал с поддержкой Bloom / HDR Emission")]
        public Material glowMaterial;
        [ColorUsage(true, true)]
        public Color normalGlowColor = new Color(0.1f, 0.4f, 1.0f, 1.0f);
        [ColorUsage(true, true)]
        public Color hoverGlowColor = new Color(0.3f, 0.7f, 1.0f, 2.0f);
        
        [Header("Плавная анимация")]
        public bool enablePulse = true;
        public float pulseSpeed = 2.0f;
        public float pulseRange = 0.15f;
        public float hoverScaleMultiplier = 1.12f;
        public float scaleSpeed = 8.0f;

        [Header("Интеграция звука")]
        [Tooltip("Будет воспроизведен звук клика при активации")]
        public string clickSfxName = "UI_Click_Metallic";
        public string hoverSfxName = "UI_Hover_Soft";

        [Header("Компенсация масштабирования")]
        [Tooltip("Переопределение локального масштаба из FateMapManager")]
        public Vector3 localScaleOverride = Vector3.one;

        // Внутренние переменные
        private SpriteRenderer spriteRenderer;
        private Vector3 baseScale;
        private Vector3 targetScale;
        private Material instancedMaterial;
        private bool isHovered = false;
        private float pulseTimer = 0f;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white; // Держим идеально белый цвет оригинальной картинки!
            }
            baseScale = transform.localScale;
            targetScale = baseScale;

            // Динамическая проверка колайдера
            Collider2D col = GetComponent<Collider2D>();
            if (col == null)
            {
                // Авто-добавление CircleCollider2D делает настройку невероятно удобной и беспроблемной!
                col = gameObject.AddComponent<CircleCollider2D>();
                Debug.Log($"[FactionMapMarker] Колайдер 2D отсутствовал на '{gameObject.name}'. Автоматически добавлен CircleCollider2D для корректного перехвата кликов.");
            }

            // Убрали принудительную перекалибровку неоновых цветов, если пользователь установил белый цвет на максимум!
            normalGlowColor = Color.white;
            hoverGlowColor = Color.white;

            // Создаем инстанс материала для индивидуального свечения (чтобы не менять общий ассет)
            if (glowMaterial != null)
            {
                if (Application.isPlaying)
                {
                    instancedMaterial = Instantiate(glowMaterial);
                    
                    // Гарантированно отключаем ZWrite на инстанцируемом материале во избежание черного/белого круга
                    // позади спрайта на точках с фоном карты!
                    if (instancedMaterial.HasProperty("_ZWrite"))
                    {
                        instancedMaterial.SetFloat("_ZWrite", 0.0f);
                    }
                    if (instancedMaterial.HasProperty("_ZWriteControl"))
                    {
                        instancedMaterial.SetFloat("_ZWriteControl", 0.0f);
                    }
                    instancedMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                    spriteRenderer.material = instancedMaterial;
                    SetGlowColor(normalGlowColor);
                }
                else
                {
                    spriteRenderer.sharedMaterial = glowMaterial;
                    instancedMaterial = null; // Не используем инстанс и не мутируем в редакторе
                }
            }
            else
            {
                // Fallback: дублируем стандартный спрайтовый материал
                if (Application.isPlaying)
                {
                    instancedMaterial = spriteRenderer.material;
                }
                else
                {
                    instancedMaterial = null; // Не используем инстанс в редакторе
                }
            }
        }

        private void AutoCalibrateColors()
        {
            // Метод оставлен пустым, так как мы хотим сохранить оригинальные цвета картинок без принудительного неонового тинтирования!
        }

        private bool isHighlightedChoice = false;

        public void SetHighlightActive(bool active)
        {
            isHighlightedChoice = active;
            isHovered = false;
            
            // Если включен инстанцированный материал, обновляем его цвета мгновенно
            if (active)
            {
                SetGlowColor(hoverGlowColor);
            }
            else
            {
                // Никогда не затемняем и не приглушаем неактивные точки! Альфа канал всегда на максимум!
                SetGlowColor(normalGlowColor);
            }
        }

        void Update()
        {
            // --- ZENITH QUANTUM INDEPENDENT SCALE CALCULATION ---
            float masterRingScale = 1.0f;
            float parentMapScale = 1.0f;

            if (FateMapManager.Instance != null)
            {
                masterRingScale = FateMapManager.Instance.ringScale;
                parentMapScale = FateMapManager.Instance.mapScale;
            }

            // Абсолютная независимость: делим желаемый ringScale на масштаб карты (parentMapScale).
            // Это гарантирует, что при увеличении карты кольцо остается точно заданного размера!
            float compensatedComp = masterRingScale / (parentMapScale > 0.001f ? parentMapScale : 1.0f);
            baseScale = new Vector3(compensatedComp, compensatedComp, 1.0f);

            // Если задано внешнее переопределение размера (например, из FateMapManager), используем его
            if (localScaleOverride != Vector3.one && localScaleOverride != Vector3.zero)
            {
                baseScale = localScaleOverride;
            }

            // Рассчитываем целевой масштаб на основе интерактивных состояний в реальном времени
            if (isHighlightedChoice)
            {
                targetScale = baseScale * (hoverScaleMultiplier * 1.15f);
            }
            else if (isHovered)
            {
                targetScale = baseScale * hoverScaleMultiplier;
            }
            else
            {
                targetScale = baseScale;
            }

            if (isHighlightedChoice)
            {
                // Для выбранной точки - усиленная красивая пульсация
                pulseTimer += Time.deltaTime * (pulseSpeed * 1.5f);
                float pulse = Mathf.Sin(pulseTimer) * (pulseRange * 1.2f);
                transform.localScale = baseScale * (hoverScaleMultiplier * 1.12f + pulse);
                
                if (instancedMaterial != null && Application.isPlaying)
                {
                    Color pulsedColor = hoverGlowColor * (1.1f + pulse * 0.3f);
                    SetGlowColor(pulsedColor);
                }
                return;
            }

            // Плавная интерполяция размера (Ховер эффект)
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);

            // Эффект пульсации светимости/размера для неактивного ожидания
            if (enablePulse && !isHovered)
            {
                pulseTimer += Time.deltaTime * pulseSpeed;
                float pulse = Mathf.Sin(pulseTimer) * pulseRange;
                transform.localScale = baseScale * (1.0f + pulse);
                
                // Если включен HDR материал, пульсируем его интенсивность свечения
                if (instancedMaterial != null && Application.isPlaying)
                {
                    Color pulsedColor = normalGlowColor * (1.0f + pulse * 0.5f);
                    SetGlowColor(pulsedColor);
                }
            }
        }

        void OnMouseEnter()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return; // Проверка на перекрытие интерфейсом
            if (isHighlightedChoice) return;

            isHovered = true;
            SetGlowColor(hoverGlowColor);

            // Воспроизведение звука наведения через SettingsManager (если он есть) или локальный клип
            PlaySfx(hoverSfxName);
        }

        void OnMouseExit()
        {
            if (isHighlightedChoice) return;
            isHovered = false;
            SetGlowColor(normalGlowColor);
        }

        void OnMouseDown()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            // Мягко сжимаем размер при нажатии
            transform.localScale = baseScale * (hoverScaleMultiplier * 0.9f);
            PlaySfx(clickSfxName);

            // Событие выбора фракции на карте
            OnMarkerSelected();
        }

        void OnMouseUp()
        {
            // Состояние сжатия сбрасывается автоматически в следующем кадре Update()
        }

        private void SetGlowColor(Color color)
        {
            Color colorWithMaxAlpha = color;
            colorWithMaxAlpha.a = 1.0f; // Всегда держим альфа-канал на абсолютном максимуме!

            if (instancedMaterial != null)
            {
                // Поддержка стандартного цвета шейдера Sprites-Default и HDR Emission свойств
                if (instancedMaterial.HasProperty("_Color"))
                    instancedMaterial.SetColor("_Color", colorWithMaxAlpha);
                
                if (instancedMaterial.HasProperty("_EmissionColor"))
                    instancedMaterial.SetColor("_EmissionColor", colorWithMaxAlpha);

                // Для URP / Sprite-Glow шейдеров
                if (instancedMaterial.HasProperty("_GlowColor"))
                    instancedMaterial.SetColor("_GlowColor", colorWithMaxAlpha);
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white; // Постоянно держим исходный белый цвет без тинта
            }
        }

        private void PlaySfx(string sfxName)
        {
            if (SettingsManager.Instance != null)
            {
                // Идеальный прямой вызов через синглтон, ставший возможным в v18.9.0
                SettingsManager.Instance.PlaySFX(sfxName);
            }
            else
            {
                // Безопасный резервный вызов через SendMessage
                var settingsManagerObj = GameObject.Find("SettingsManager");
                if (settingsManagerObj != null)
                {
                    settingsManagerObj.SendMessage("PlaySFX", sfxName, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    Debug.LogWarning($"[FactionMapMarker] Не удалось найти SettingsManager на сцене для воспроизведения '{sfxName}'.");
                }
            }
        }

        private void OnMarkerSelected()
        {
            Debug.Log($"[Fate Map] Выбран маркер фракции: {factionName}. Саб-описание: {factionDescription}");
            
            if (triggerDialogueOnClassClick && DialogueSystem_Manager.Instance != null)
            {
                // Мгновенный запуск диалога на нужном слайде с передачей имени и подписи!
                DialogueSystem_Manager.Instance.OnMapMarkerClicked(associatedDialogueIndex, factionName, factionDescription);
            }
            else
            {
                Debug.LogWarning("[FactionMapMarker] DialogueSystem_Manager.Instance не инициализирован в этой сцене!");
            }
        }

        public void ApplyGlowColorInEditor()
        {
            if (Application.isPlaying) return;
            
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalGlowColor;
            }
        }
    }
}
