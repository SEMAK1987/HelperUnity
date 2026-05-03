using UnityEngine;

/*
 * [FATE CONTINENT EXPANSION v17.18.30] 
 * Улучшенный скрипт камеры для главного меню. 
 * Включает плавную интерполяцию (SmoothStep) и защиту от "стоячей" камеры.
 */

public class MenuBackgroundCamera_Fate : MonoBehaviour
{
    [Header("Настройки фокусировки")]
    [Tooltip("Список Empty-объектов в сцене. ВАЖНО: они должны быть в мировом пространстве, а не внутри Canvas!")]
    public Transform[] castlePoints; 
    
    [Header("Параметры движения")]
    [Range(0.01f, 1.0f)]
    public float transitionSpeed = 0.15f;
    public bool useSmoothStep = true;
    
    [Header("Отладка")]
    [SerializeField] private int currentIndex = 0;
    [SerializeField] private float timer = 0f;

    void Start()
    {
        if (castlePoints != null && castlePoints.Length > 0 && castlePoints[0] != null)
        {
            transform.position = castlePoints[0].position;
            transform.rotation = castlePoints[0].rotation;
        }
        else
        {
            Debug.LogWarning("[MenuBackgroundCamera] Точки фокусировки не назначены!");
        }
    }

    void Update()
    {
        if (castlePoints == null || castlePoints.Length < 2) return;

        // Плавное приращение времени
        timer += Time.deltaTime * transitionSpeed;
        
        if (timer >= 1f)
        {
            timer = 0f;
            currentIndex = (currentIndex + 1) % castlePoints.Length;
        }

        int nextIndex = (currentIndex + 1) % castlePoints.Length;
        
        if (castlePoints[currentIndex] == null || castlePoints[nextIndex] == null) return;

        // Расчет коэффициента плавности
        float t = timer;
        if (useSmoothStep)
        {
            t = Mathf.SmoothStep(0f, 1f, timer);
        }

        // Интерполяция позиции (сохраняем Z от точек, но даем возможность ручной правки)
        Vector3 targetPos = Vector3.Lerp(castlePoints[currentIndex].position, castlePoints[nextIndex].position, t);
        transform.position = targetPos;
        
        transform.rotation = Quaternion.Slerp(castlePoints[currentIndex].rotation, castlePoints[nextIndex].rotation, t);
    }

    // Метод для ручного переключения (например, при клике на фракцию)
    public void GoToPoint(int index)
    {
        if (index >= 0 && index < castlePoints.Length)
        {
            currentIndex = index;
            timer = 0f;
        }
    }
}
