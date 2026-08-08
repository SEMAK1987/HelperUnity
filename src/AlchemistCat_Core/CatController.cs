using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core)
/// Контроллер милого Кота-Алхимика. Управляет анимацией,
/// реакцией на поглаживание (клики), мурлыканьем и варкой зелий.
/// </summary>
public class CatController : MonoBehaviour
{
    public static CatController Instance { get; private set; }

    [Header("Спрайты Кота")]
    public Sprite idleSprite;
    public Sprite happySprite;
    public Sprite sleepingSprite;
    public Sprite brewingSprite;

    [Header("UI Ссылки")]
    public Image catImage;
    public TextMeshProUGUI meowBubbleText;
    public GameObject bubbleObject;

    [Header("Звуки Кота")]
    public AudioClip meowSound;
    public AudioClip purrSound;

    private Coroutine bubbleCoroutine;
    private Vector3 originalScale;

    public enum CatState { Idle, Happy, Sleeping, Brewing }
    private CatState currentState = CatState.Idle;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (catImage == null) catImage = GetComponent<Image>();
        originalScale = transform.localScale;

        if (bubbleObject != null) bubbleObject.SetActive(false);
        SetState(CatState.Idle);
    }

    /// <summary>
    /// Меняет состояние кота и обновляет его визуальный спрайт.
    /// </summary>
    public void SetState(CatState newState)
    {
        currentState = newState;
        if (catImage == null) return;

        switch (currentState)
        {
            case CatState.Idle:
                catImage.sprite = idleSprite;
                break;
            case CatState.Happy:
                catImage.sprite = happySprite;
                break;
            case CatState.Sleeping:
                catImage.sprite = sleepingSprite;
                break;
            case CatState.Brewing:
                catImage.sprite = brewingSprite;
                break;
        }
    }

    /// <summary>
    /// Вызывается при клике/поглаживании кота в интерфейсе.
    /// </summary>
    public void OnCatClicked()
    {
        if (currentState == CatState.Sleeping)
        {
            WakeUp();
            return;
        }

        // Запускаем анимацию покачивания
        StopAllCoroutines();
        StartCoroutine(BounceCatCoroutine());

        // Добавляем немного опыта Коту
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddXP(5);
        }

        // Проигрываем звук и показываем "Мяу!"
        if (meowSound != null && SettingsManager.Instance != null)
        {
            SettingsManager.Instance.PlaySoundEffect(meowSound);
        }

        string[] catPhrases = {
            "Муррр... Погладь еще!", "Мяу! Котел готов к варке!", "Дай мышку, хозяин!",
            "Ура, алхимия!", "Мяу! Наставник спит!", "Фррр... Зелье пахнет вкусно!"
        };
        string phrase = catPhrases[Random.Range(0, catPhrases.Length)];
        ShowMeowBubble(phrase);
    }

    public void ShowMeowBubble(string text)
    {
        if (bubbleObject == null || meowBubbleText == null) return;

        if (bubbleCoroutine != null) StopCoroutine(bubbleCoroutine);
        bubbleCoroutine = StartCoroutine(ShowBubbleCoroutine(text));
    }

    private IEnumerator ShowBubbleCoroutine(string text)
    {
        bubbleObject.SetActive(true);
        meowBubbleText.text = text;
        yield return new WaitForSeconds(3f);
        bubbleObject.SetActive(false);
    }

    private IEnumerator BounceCatCoroutine()
    {
        float duration = 0.15f;
        float elapsed = 0f;

        // Быстрое сжатие
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(originalScale, new Vector3(originalScale.x * 1.15f, originalScale.y * 0.85f, originalScale.z), t);
            yield return null;
        }

        elapsed = 0f;
        // Возврат
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(new Vector3(originalScale.x * 1.15f, originalScale.y * 0.85f, originalScale.z), originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    public void WakeUp()
    {
        SetState(CatState.Idle);
        ShowMeowBubble("Мяу! Я проснулся!");
        if (meowSound != null && SettingsManager.Instance != null)
        {
            SettingsManager.Instance.PlaySoundEffect(meowSound);
        }
    }
}
