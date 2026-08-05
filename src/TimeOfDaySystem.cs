using UnityEngine;
using UnityEngine.UI;

public class TimeOfDaySystem : MonoBehaviour
{
    [Header("UI Components")]
    public Image backgroundImage;
    [Tooltip("UI картинка Солнца (будет перемещаться по дуге)")]
    public RectTransform sunObject;
    [Tooltip("UI картинка Луны/Полумесяца (будет перемещаться по дуге)")]
    public RectTransform moonObject;

    [Header("Time Settings")]
    [Tooltip("Длительность полных игровых суток в реальных секундах")]
    public float dayCycleLengthSeconds = 120f; 

    [Header("Day Phase Colors")]
    public Color morningColor = new Color(1f, 0.73f, 0.62f);  // Теплый розово-оранжевый рассвет
    public Color dayColor = new Color(0.53f, 0.81f, 0.98f);    // Яркий чистый полдень
    public Color eveningColor = new Color(0.42f, 0.28f, 0.67f); // Мистический фиолетовый закат
    public Color nightColor = new Color(0.06f, 0.06f, 0.16f);  // Глубокая бархатная ночь

    [Header("Celestial Orbit Path")]
    [Tooltip("Максимальная высота подъема светил (в пикселях по оси Y)")]
    public float orbitHeight = 350f;
    [Tooltip("Ширина траектории полета (в пикселях по оси X, обычно во весь экран)")]
    public float orbitWidth = 800f;
    [Tooltip("Смещение по высоте от центра (Y)")]
    public float verticalOffset = -100f;

    [Header("Atmosphere Control")]
    [Tooltip("Плавность свечения (если есть CanvasGroup для плавного проявления/затухания)")]
    public bool useFading = true;

    private float currentTime = 0f;

    private void Start()
    {
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
        ValidateReferences();
    }

    private void Update()
    {
        // Продвигаем время суток вперед
        currentTime += Time.deltaTime;
        if (currentTime >= dayCycleLengthSeconds)
        {
            currentTime = 0f;
        }

        float normalizedTime = currentTime / dayCycleLengthSeconds; // Спектр от 0.0f до 1.0f

        UpdateBackgroundSky(normalizedTime);
        UpdateCelestialPositions(normalizedTime);
    }

    private void ValidateReferences()
    {
        if (backgroundImage == null)
            Debug.LogWarning("[TimeOfDaySystem] Внимание: Компонент Background Image не найден! Фон не будет менять окрас.");
        if (sunObject == null)
            Debug.LogWarning("[TimeOfDaySystem] Предупреждение: Солнце (Sun Object) не назначено в инспекторе.");
        if (moonObject == null)
            Debug.LogWarning("[TimeOfDaySystem] Предупреждение: Луна (Moon Object) не назначена в инспекторе.");
    }

    private void UpdateBackgroundSky(float normalizedTime)
    {
        if (backgroundImage == null) return;

        Color targetColor;

        // Плавное переливание по фазам суток
        if (normalizedTime < 0.25f) // Утро (0.00 - 0.25)
        {
            float t = normalizedTime / 0.25f;
            targetColor = Color.Lerp(nightColor, morningColor, t);
        }
        else if (normalizedTime < 0.5f) // День (0.25 - 0.50)
        {
            float t = (normalizedTime - 0.25f) / 0.25f;
            targetColor = Color.Lerp(morningColor, dayColor, t);
        }
        else if (normalizedTime < 0.75f) // Вечер (0.50 - 0.75)
        {
            float t = (normalizedTime - 0.5f) / 0.25f;
            targetColor = Color.Lerp(dayColor, eveningColor, t);
        }
        else // Ночь (0.75 - 1.00)
        {
            float t = (normalizedTime - 0.75f) / 0.25f;
            targetColor = Color.Lerp(eveningColor, nightColor, t);
        }

        backgroundImage.color = targetColor;
    }

    private void UpdateCelestialPositions(float normalizedTime)
    {
        // Солнце активно во время дневной половины цикла (0.0 до 0.5)
        if (sunObject != null)
        {
            float sunActiveTime = normalizedTime; // Отрезок с рассвета до заката
            
            // Если сейчас ночь, прячем солнце под горизонт или выключаем
            if (sunActiveTime > 0.5f)
            {
                sunObject.gameObject.SetActive(false);
            }
            else
            {
                sunObject.gameObject.SetActive(true);
                
                // Нормализуем путь солнца: 0.0 - 0.5 превращаем в 0.0 - 1.0
                float t = sunActiveTime / 0.5f; 
                
                // Угол полета по дуге от 180 до 0 градусов (в радианах: от PI до 0)
                float angle = Mathf.PI * (1f - t); 
                
                float x = Mathf.Cos(angle) * (orbitWidth * 0.5f);
                float y = Mathf.Sin(angle) * orbitHeight + verticalOffset;
                
                sunObject.anchoredPosition = new Vector2(x, y);

                // Плавное растворение на восходе/заходе
                if (useFading)
                {
                    CanvasGroup cg = sunObject.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        cg.alpha = Mathf.Sin(angle); // Максимум в зените
                    }
                }
            }
        }

        // Луна активна во время ночной половины цикла (0.5 до 1.0)
        if (moonObject != null)
        {
            float moonActiveTime = normalizedTime;
            
            if (moonActiveTime < 0.5f)
            {
                moonObject.gameObject.SetActive(false);
            }
            else
            {
                moonObject.gameObject.SetActive(true);
                
                // Нормализуем путь луны: 0.5 - 1.0 превращаем в 0.0 - 1.0
                float t = (moonActiveTime - 0.5f) / 0.5f;
                
                // Угол полета луны: от PI до 0
                float angle = Mathf.PI * (1f - t);
                
                float x = Mathf.Cos(angle) * (orbitWidth * 0.5f);
                float y = Mathf.Sin(angle) * orbitHeight + verticalOffset;
                
                moonObject.anchoredPosition = new Vector2(x, y);

                // Плавное растворение на восходе/заходе
                if (useFading)
                {
                    CanvasGroup cg = moonObject.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        cg.alpha = Mathf.Sin(angle);
                    }
                }
            }
        }
    }
}
