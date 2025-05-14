using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResizeWindow : MonoBehaviour
{
    public float targetAspect = 16f / 9f;
    public float resizeDelay = 0.01f; // Сколько ждать после изменения размера

    private int lastWidth;
    private int lastHeight;
    private float resizeTimer = -1f;

    void Start()
    {
        DontDestroyOnLoad(this);
        lastWidth = Screen.width;
        lastHeight = Screen.height;
    }

    void Update()
    {
        int currentWidth = Screen.width;
        int currentHeight = Screen.height;

        if (currentWidth != lastWidth || currentHeight != lastHeight)
        {
            // Окно изменяется — сбрасываем таймер
            resizeTimer = resizeDelay;

            lastWidth = currentWidth;
            lastHeight = currentHeight;
        }

        if (resizeTimer >= 0f)
        {
            resizeTimer -= Time.deltaTime;

            if (resizeTimer <= 0f)
            {
                // Размер стабилизировался — корректируем
                FixAspectRatio();
            }
        }
    }

    void FixAspectRatio()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        int width = Screen.width;
        int height = Screen.height;

        if (Mathf.Abs(currentAspect - targetAspect) > 0.01f)
        {
            if (currentAspect > targetAspect)
            {
                width = Mathf.RoundToInt(height * targetAspect);
            }
            else
            {
                height = Mathf.RoundToInt(width / targetAspect);
            }

            Screen.SetResolution(width, height, false);
        }

        // Сохраняем новые значения
        lastWidth = width;
        lastHeight = height;
    }
}
