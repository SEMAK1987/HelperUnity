using UnityEngine;
using UnityEngine.EventSystems;

/*
 * [FATE CONTINENT - ZENITH DIALOGUE & MAP SYSTEM v18.11.0]
 * Автономный C# компонент для интерактивных маркеров континентов и фракций.
 * Обеспечивает плавное свечение (Bloom/Emission), интерактивный ховер, затухание
 * и интеграцию со звуком через SettingsManager.
 */

namespace FateContinent
{
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

            // Авто-калибровка неоновых цветов, если они оставлены по умолчанию (прозрачные, черные или не настроены)
            if (normalGlowColor.a < 0.05f || normalGlowColor == Color.black || normalGlowColor == Color.clear)
            {
                AutoCalibrateColors();
            }

            // Создаем инстанс материала для индивидуального свечения (чтобы не менять общий ассет)
            if (glowMaterial != null)
            {
                instancedMaterial = Instantiate(glowMaterial);
                spriteRenderer.material = instancedMaterial;
                SetGlowColor(normalGlowColor);
            }
            else
            {
                // Fallback: дублируем стандартный спрайтовый материал
                instancedMaterial = spriteRenderer.material;
            }
        }

        private void AutoCalibrateColors()
        {
            string nameLower = factionName.ToLower();
            
            // 1. Этельгард (Эльфы) — Сапфирово-бирюзовый неон (Диалоги 3/4 или ключевые слова)
            if (nameLower.Contains("аэлисс") || nameLower.Contains("этельгард") || nameLower.Contains("эльф") || nameLower.Contains("порт") || nameLower.Contains("ethel") || associatedDialogueIndex == 3 || associatedDialogueIndex == 4)
            {
                // Hex: #0A6CB2 (Интенсивность +0.5 -> умножение на 1.41)
                // Hover Hex: #00F0FF (Интенсивность +2.0 -> умножение на 4.0)
                normalGlowColor = new Color(10f / 255f, 108f / 255f, 178f / 255f, 1.0f) * 1.41f;
                hoverGlowColor = new Color(0f / 255f, 240f / 255f, 255f / 255f, 1.0f) * 4.0f;
                Debug.Log($"[FactionMapMarker] Авто-калибровка цветов для '{factionName}': Сапфирово-бирюзовый неон (Этельгард)");
            }
            // 2. Арделланд (Люди) — Золотисто-солнечный неон (Диалоги 5/6 или ключевые слова)
            else if (nameLower.Contains("льв") || nameLower.Contains("цитадел") || nameLower.Contains("ардел") || nameLower.Contains("человек") || nameLower.Contains("ardel") || nameLower.Contains("lion") || associatedDialogueIndex == 5 || associatedDialogueIndex == 6)
            {
                // Hex: #B2830A (Интенсивность +0.5)
                // Hover Hex: #FFD000 (Интенсивность +2.0)
                normalGlowColor = new Color(178f / 255f, 131f / 255f, 10f / 255f, 1.0f) * 1.41f;
                hoverGlowColor = new Color(255f / 255f, 208f / 255f, 0f / 255f, 1.0f) * 4.0f;
                Debug.Log($"[FactionMapMarker] Авто-калибровка цветов для '{factionName}': Золотисто-солнечный неон (Арделланд)");
            }
            // 3. Вердантия (Друиды) — Чародейский изумрудный неон (Диалоги 7/8 или ключевые слова)
            else if (nameLower.Contains("друид") || nameLower.Contains("вердант") || nameLower.Contains("святилищ") || nameLower.Contains("лес") || nameLower.Contains("verd") || nameLower.Contains("druid") || associatedDialogueIndex == 7 || associatedDialogueIndex == 8)
            {
                // Hex: #0AB23D (Интенсивность +0.5)
                // Hover Hex: #00FF55 (Интенсивность +2.0)
                normalGlowColor = new Color(10f / 255f, 178f / 255f, 61f / 255f, 1.0f) * 1.41f;
                hoverGlowColor = new Color(0f / 255f, 255f / 255f, 85f / 255f, 1.0f) * 4.0f;
                Debug.Log($"[FactionMapMarker] Авто-калибровка цветов для '{factionName}': Чародейский изумрудный неон (Вердантия)");
            }
            // 4. Ксандрия (Разлом/Арена) — Электрический фиолетовый неон
            else
            {
                // Hex: #6F0AB2 (Интенсивность +0.5)
                // Hover Hex: #CC00FF (Интенсивность +2.0)
                normalGlowColor = new Color(111f / 255f, 10f / 255f, 178f / 255f, 1.0f) * 1.41f;
                hoverGlowColor = new Color(204f / 255f, 0f / 255f, 255f / 255f, 1.0f) * 4.0f;
                Debug.Log($"[FactionMapMarker] Авто-калибровка цветов для '{factionName}': Электрический фиолетовый неон (Ксандрия)");
            }
        }

        private bool isHighlightedChoice = false;

        public void SetHighlightActive(bool active)
        {
            isHighlightedChoice = active;
            isHovered = false;
            
            // Если включен инстанцированный материал, обновляем его цвета мгновенно
            if (active)
            {
                targetScale = baseScale * (hoverScaleMultiplier * 1.15f);
                SetGlowColor(hoverGlowColor);
            }
            else
            {
                targetScale = baseScale;
                SetGlowColor(normalGlowColor * 0.4f); // Приглушаем неактивные точки для фокуса
            }
        }

        void Update()
        {
            if (isHighlightedChoice)
            {
                // Для выбранной точки - усиленная красивая пульсация
                pulseTimer += Time.deltaTime * (pulseSpeed * 1.5f);
                float pulse = Mathf.Sin(pulseTimer) * (pulseRange * 1.2f);
                transform.localScale = baseScale * (hoverScaleMultiplier * 1.12f + pulse);
                
                if (instancedMaterial != null)
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
                if (instancedMaterial != null)
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
            targetScale = baseScale * hoverScaleMultiplier;
            SetGlowColor(hoverGlowColor);

            // Воспроизведение звука наведения через SettingsManager (если он есть) или локальный клип
            PlaySfx(hoverSfxName);
        }

        void OnMouseExit()
        {
            if (isHighlightedChoice) return;
            isHovered = false;
            targetScale = baseScale;
            SetGlowColor(normalGlowColor);
        }

        void OnMouseDown()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            targetScale = baseScale * (hoverScaleMultiplier * 0.9f); // Небольшое сжатие при клике
            PlaySfx(clickSfxName);

            // Событие выбора фракции на карте
            OnMarkerSelected();
        }

        void OnMouseUp()
        {
            if (isHovered)
                targetScale = baseScale * hoverScaleMultiplier;
            else
                targetScale = baseScale;
        }

        private void SetGlowColor(Color color)
        {
            if (instancedMaterial != null)
            {
                // Поддержка стандартного цвета шейдера Sprites-Default и HDR Emission свойств
                if (instancedMaterial.HasProperty("_Color"))
                    instancedMaterial.SetColor("_Color", color);
                
                if (instancedMaterial.HasProperty("_EmissionColor"))
                    instancedMaterial.SetColor("_EmissionColor", color);

                // Для URP / Sprite-Glow шейдеров
                if (instancedMaterial.HasProperty("_GlowColor"))
                    instancedMaterial.SetColor("_GlowColor", color);
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
                // Мгновенный запуск диалога на нужном слайде!
                DialogueSystem_Manager.Instance.StartDialogue(associatedDialogueIndex);
            }
            else
            {
                Debug.LogWarning("[FactionMapMarker] DialogueSystem_Manager.Instance не инициализирован в этой сцене!");
            }
        }
    }
}
