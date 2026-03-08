using System;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField] private GameObject initialBackground; // 初始背景
    [SerializeField] private GameObject repeatingBackground; // 可循环拼接的背景块
    [SerializeField] private Camera mainCamera; // 主相机

    private float backgroundHeight; // 背景块的高度
    private float lastBackgroundTop; // 最后一个背景块的顶部位置

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // 自动计算背景高度
        CalculateBackgroundHeight();

        // 初始化最后一个背景块的顶部位置为初始背景的顶部
        if (initialBackground != null)
        {
            SpriteRenderer spriteRenderer = initialBackground.GetComponent<SpriteRenderer>();
            lastBackgroundTop = spriteRenderer.bounds.size.y;
        }
    }

    void CalculateBackgroundHeight()
    {
        // 如果初始背景不可用，使用重复背景计算高度
        if (repeatingBackground != null)
        {
            SpriteRenderer spriteRenderer = repeatingBackground.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                backgroundHeight = spriteRenderer.sprite.bounds.size.y * repeatingBackground.transform.localScale.y;
                return;
            }
            Debug.Log("重复背景高度计算成功: " + backgroundHeight);
        } else
        {
            // 如果都不可用，使用默认值
            backgroundHeight = 20f;
            Debug.LogWarning("无法自动计算背景高度，使用默认值: " + backgroundHeight);
        }
    }

    void Update()
    {
        // 计算相机的顶部位置
        float cameraTop = mainCamera.transform.position.y + mainCamera.orthographicSize;

        // 当相机顶部接近最后一个背景块的顶部时，生成新的背景块
        if (cameraTop > lastBackgroundTop - backgroundHeight)
        {
            SpawnNewBackground();
        }
    }

    void SpawnNewBackground()
    {
        if (repeatingBackground == null)
        {
            Debug.LogError("repeatingBackground is not assigned!");
            return;
        }

        // 计算新背景块的位置（在最后一个背景块的上方）
        Vector3 newPosition = new Vector3(0, lastBackgroundTop + backgroundHeight / 2 - 0.01f, 0);

        // 实例化新的背景块
        GameObject newBackground = Instantiate(repeatingBackground, transform);
        newBackground.transform.position = newPosition;

        // 更新最后一个背景块的顶部位置
        lastBackgroundTop = newPosition.y + backgroundHeight / 2;
    }
}