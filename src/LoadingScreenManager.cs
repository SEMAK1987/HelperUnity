using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance;

    [Header("UI элементы")]
    public GameObject loadingContainer; // Родитель всего экрана загрузки (в Overlay_Layer)
    public Slider progressBar;          // Полоска загрузки
    public TextMeshProUGUI progressText; // Текст процента
    public TextMeshProUGUI statusText;   // Текст статуса (например, "Загрузка ресурсов...")

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        if (loadingContainer != null)
            loadingContainer.SetActive(false);
    }

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadAsync(sceneIndex));
    }

    private IEnumerator LoadAsync(int sceneIndex)
    {
        if (loadingContainer != null)
            loadingContainer.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false; // Ждем заполнения полоски

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            if (progressBar != null)
                progressBar.value = progress;
            
            if (progressText != null)
                progressText.text = (progress * 100f).ToString("F0") + "%";

            // Если загрузка завершена (в Unity 0.9 означает 100% готовность к активации)
            if (operation.progress >= 0.9f)
            {
                if (statusText != null) 
                {
                    statusText.text = "— ЗАГРУЗКА ЗАВЕРШЕНА —\nНАЖМИТЕ ЛЮБУЮ КЛАВИШУ";
                    
                    // Эффект пульсации для привлечения внимания
                    float pulse = 0.95f + Mathf.PingPong(Time.time * 2f, 0.1f);
                    statusText.transform.localScale = new Vector3(pulse, pulse, 1f);
                }
                
                if (Input.anyKey)
                {
                    operation.allowSceneActivation = true;
                }
            }

            yield return null;
        }

        if (loadingContainer != null)
            loadingContainer.SetActive(false);
    }
}
