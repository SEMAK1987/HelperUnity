using UnityEngine;

public class SimpleFloatAnimation : MonoBehaviour
{
    [Header("Настройки анимации")]
    public float amplitude = 10f; // Размах движения
    public float speed = 2f;      // Скорость движения
    public bool useWorldSpace = false;

    private Vector3 startPos;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
            startPos = rectTransform.anchoredPosition;
        else
            startPos = transform.localPosition;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * speed) * amplitude;
        
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(startPos.x, newY);
        }
        else
        {
            if (useWorldSpace)
                transform.position = new Vector3(startPos.x, newY, startPos.z);
            else
                transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
        }
    }
}
