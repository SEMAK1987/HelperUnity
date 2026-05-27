using UnityEngine;
using UnityEngine.EventSystems;

/*
 * [FATE CONTINENT - ZENITH DIALOGUE & MAP SYSTEM v18.9.0]
 * Автономный C# компонент для интерактивных маркеров континентов и фракций.
 * Обеспечивает плавное свечение (Bloom/Emission), интерактивный ховер, затухание
 * и интеграцию со звуком через SettingsManager.
 */

namespace FateContinent
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))] // Необходим для кликов в 2D/2.5D мировом пространстве
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

        void Update()
        {
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

            isHovered = true;
            targetScale = baseScale * hoverScaleMultiplier;
            SetGlowColor(hoverGlowColor);

            // Воспроизведение звука наведения через SettingsManager (если он есть) или локальный клип
            PlaySfx(hoverSfxName);
        }

        void OnMouseExit()
        {
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
                // Если у SettingsManager.Instance есть метод воспроизведения звука по имени (в виде строки):
                try
                {
                    // Имитируем вызов через Broadcast/SendMessage или если есть нужный метод в API
                    SettingsManager.Instance.SendMessage("PlaySFX", sfxName, SendMessageOptions.DontRequireReceiver);
                }
                catch
                {
                    Debug.LogWarning($"[FactionMapMarker] Не удалось проиграть клип '{sfxName}' через SettingsManager.Instance.");
                }
            }
            else
            {
                // Безопасный резервный вызов
                var settingsManagerObj = GameObject.Find("SettingsManager");
                if (settingsManagerObj != null)
                {
                    settingsManagerObj.SendMessage("PlaySFX", sfxName, SendMessageOptions.DontRequireReceiver);
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
