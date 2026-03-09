using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
using System;

public class Game : MonoBehaviour
{
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private Transform buildingParent;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Text startText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button startButton;
    [SerializeField] private AudioSource placeSound;
    [SerializeField] private AudioSource perfectSound;
    [SerializeField] private AudioSource gameOverSound;
    [SerializeField] private BackgroundManager backgroundManager;
    [SerializeField] private GameObject blockRef; // 外部传入的方块引用

    private GameObject currentBlock;
    private float blockSpeed = 2f;
    private bool isMovingRight = true;
    private int score = 0;
    private int perfectStreak = 0;
    private float buildingHeight = 1f; // 初始值为底座的高度
    private int floorCount = 1; // 楼层计数器，从1开始
    private float maxBuildingWidth = 1f;
    private bool isGameOver = false;
    private bool isGameStarted = false;
    private float shakeMagnitude = 0f;
    private float shakeIncrement = 0.01f;
    private float maxShakeMagnitude = 0.2f;
    private Vector3 initialCameraPos;
    private Vector3 targetCameraPos; // 目标相机位置
    private float blockHeight;


    void Start()
    {
        InitializeGame();
    }

    public void StartGame()
    {
        isGameStarted = true;
        startText.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);
        blockRef.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(-3, 0);
    }

    void InitializeGame()
    {
        blockRef.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        // 重置游戏状态
        score = 0;
        perfectStreak = 0;
        shakeMagnitude = 0f;
        isGameOver = false;
        isGameStarted = false;
        currentBlock = null;
        buildingHeight = blockHeight / 2f;
        floorCount = 1; // 重置楼层计数器

        Debug.Log("Initializing game...");

        if (buildingParent == null)
        {
            Debug.LogError("buildingParent is not assigned!");
            return;
        }

        // 清空现有建筑
        foreach (Transform child in buildingParent)
        {
            Destroy(child.gameObject);
        }

        // 创建初始平台
        CreatePlatform();

        // 重置相机位置
        Camera.main.transform.position = initialCameraPos;
        targetCameraPos = initialCameraPos;

        // 更新UI
        if (scoreText != null)
        {
            UpdateScoreText();
        }
        else
        {
            Debug.LogError("scoreText is not assigned!");
        }

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("gameOverText is not assigned!");
        }

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("restartButton is not assigned!");
        }

        if (startText != null)
        {
            startText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("startText is not assigned!");
        }

        if (startButton != null)
        {
            startButton.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("startButton is not assigned!");
        }

        Debug.Log("Game initialized successfully");
    }

    void CreatePlatform()
    {
        if (blockPrefab == null)
        {
            Debug.LogError("blockPrefab is not assigned!");
            return;
        }

        if (buildingParent == null)
        {
            Debug.LogError("buildingParent is not assigned!");
            return;
        }

        GameObject platform = Instantiate(blockPrefab, buildingParent);
        platform.transform.position = new Vector3(0, 1f, 0);
        Rigidbody2D rb = platform.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("blockPrefab missing Rigidbody2D component!");
            return;
        }

        // 设置为Kinematic类型，使平台固定不动，模拟地基
        rb.bodyType = RigidbodyType2D.Kinematic;
        Debug.Log("Platform created at position: " + platform.transform.position);
    }

    void SpawnNewBlock()
    {
        if (blockPrefab == null)
        {
            Debug.LogError("blockPrefab is not assigned!");
            return;
        }

        if (buildingParent == null)
        {
            Debug.LogError("buildingParent is not assigned!");
            return;
        }

        currentBlock = Instantiate(blockPrefab, buildingParent);
        // 设置方块名称为FloorN格式
        currentBlock.name = "Floor" + floorCount;
        floorCount++;
        // 检查blockRef是否赋值，如果有则在其位置生成，否则在屏幕顶部生成
        Vector3 spawnPosition;
        if (blockRef != null)
        {
            spawnPosition = blockRef.transform.position;
        }
        else
        {
            // 如果blockRef未赋值，在屏幕顶部生成
            float top = getScreenTop();
            spawnPosition = new Vector3(0, top, 0);
            Debug.LogWarning("blockRef is not assigned, spawning block at screen top");
        }
        currentBlock.transform.position = spawnPosition;

        Rigidbody2D rb = currentBlock.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("blockPrefab missing Rigidbody2D component!");
            return;
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        
        // 添加碰撞检测
        BoxCollider2D collider = currentBlock.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            Debug.LogError("blockPrefab missing BoxCollider2D component!");
            return;
        }
        
        // 添加碰撞检测组件
        BlockCollision blockCollision = currentBlock.GetComponent<BlockCollision>();
        if (blockCollision == null)
        {
            blockCollision = currentBlock.AddComponent<BlockCollision>();
        }
        blockCollision.SetGameManager(this);
        
        // 重置碰撞状态
        hasCollided = false;
        Debug.Log("New block spawned at position: " + currentBlock.transform.position);
    }

    private InputAction clickAction;

    void Awake()
    {
        clickAction = new InputAction(binding: "<Mouse>/leftButton");
        clickAction.performed += ctx => ReleaseBlock();
        initialCameraPos = Camera.main.transform.position;
        targetCameraPos = initialCameraPos;
        blockHeight = blockPrefab.GetComponent<BoxCollider2D>().size.y * blockPrefab.transform.localScale.y;

        Debug.Log("blockHeight: " + blockHeight);
        buildingHeight = blockHeight/2;
    }

    void OnEnable()
    {
        clickAction.Enable();
    }

    void OnDisable()
    {
        clickAction.Disable();
    }

    private bool hasCollided = false;

    void Update()
    {
        if (isGameOver || !isGameStarted) return;

        // 控制方块左右移动
        // MoveBlock();

        // 应用建筑摇晃
        // ApplyBuildingShake();
        
        // 平滑移动相机到目标位置
        if (Camera.main != null)
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetCameraPos, 0.1f);
        }

        // 持续检查游戏结束条件
        CheckGameOverInUpdate();

        // 检查当前方块是否放置完成（发生碰撞或掉出y=0之下）
        if (currentBlock != null)
        {
            Rigidbody2D rb = currentBlock.GetComponent<Rigidbody2D>();
            // 检查是否已经启用了物理（处于下落状态）
            if (rb.bodyType == RigidbodyType2D.Dynamic)
            {
                // 检查是否掉出y=0之下
                if (currentBlock.transform.position.y < 0 || hasCollided)
                {
                    // 方块放置完成，清理引用
                    currentBlock = null;
                    hasCollided = false;
                    // 不再自动生成新方块，等待玩家下一次点击
                }
            }
        }
    }

    // 在Update中持续检查游戏结束条件
    void CheckGameOverInUpdate()
    {
        if (isGameOver) return;
        
        if (currentBlock != null)
        {
            Rigidbody2D rb = currentBlock.GetComponent<Rigidbody2D>();
            // 只有当方块已经启用了物理（处于下落状态）时，才检查是否掉落
            if (rb.bodyType == RigidbodyType2D.Dynamic)
            {
                // 检查方块是否掉落
                if (currentBlock.transform.position.y < -6)
                {
                    Debug.Log("Game Over: Block fell below threshold");
                    GameOver();
                }
            }
        }

        // 检查建筑是否倾斜过度（跳过初始平台）
        for (int i = 1; i < buildingParent.childCount; i++)
        {
            Transform child = buildingParent.GetChild(i);
            // 检查建筑块的旋转角度，只有当旋转角度确实超过45度时才触发游戏结束
            float zRotation = child.rotation.eulerAngles.z;
            if (zRotation > 45 && zRotation < 315) // 处理0-360度的角度范围
            {
                Debug.Log("Game Over: Building tilted too much");
                GameOver();
                break;
            }
        }
    }

    void MoveBlock()
    {
        if (currentBlock != null)
        {
            float moveDirection = isMovingRight ? 1 : -1;
            currentBlock.transform.position += new Vector3(moveDirection * blockSpeed * Time.deltaTime, 0, 0);

            // 边界检测
            if (currentBlock.transform.position.x > 3)
            {
                isMovingRight = false;
            }
            else if (currentBlock.transform.position.x < -3)
            {
                isMovingRight = true;
            }
        }
    }

    float getScreenTop()
    {
        Camera mainCamera = Camera.main;
        float cameraHeight = mainCamera.orthographicSize * 2;
        float top = mainCamera.transform.position.y + cameraHeight / 2;
        return top;
    }

    void ReleaseBlock()
    {
        Debug.Log("building height: " + buildingHeight);
        if (!isGameStarted || isGameOver) return;

        // 生成新方块
        SpawnNewBlock();
        
        // 立即启用物理，让方块下坠
        Rigidbody2D rb = currentBlock.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;

        // 计算与下方方块的对齐程度
        float alignment = CalculateAlignment();

        // 播放音效
        if (alignment >= 0.95f)
        {
            perfectSound.Play();
            score += 100;
            perfectStreak++;
            if (perfectStreak > 3)
            {
                score += perfectStreak * 50;
            }
        }
        else
        {
            placeSound.Play();
            score += Mathf.RoundToInt(alignment * 50);
            perfectStreak = 0;
        }

        // 更新分数
        UpdateScoreText();

        // 增加建筑高度
        buildingHeight += blockHeight;


        // 相机跟随建筑高度
        Camera mainCamera = Camera.main;
        Vector3 nowPos = mainCamera.transform.position;
        float top = getScreenTop();
        
        // 当建筑高度接近屏幕顶部时，相机向上移动
        if (top - buildingHeight < 7)
        {
            float delta = 7 - (top - buildingHeight);
            targetCameraPos = new Vector3(targetCameraPos.x, nowPos.y + delta, targetCameraPos.z);
        }
        // 增加摇晃幅度
        // shakeMagnitude = Mathf.Min(shakeMagnitude + shakeIncrement, maxShakeMagnitude);

        // 检查游戏是否结束
        StartCoroutine(CheckGameOver());

        // 不再自动生成新方块，而是在Update中检查方块放置完成后再生成
    }

    // 方块碰撞事件处理
    public void OnBlockCollision()
    {
        hasCollided = true;
    }

    float CalculateAlignment()
    {
        if (buildingParent.childCount < 2) return 1f;

        Transform current = currentBlock.transform;
        Transform previous = buildingParent.GetChild(buildingParent.childCount - 2);

        float currentWidth = current.GetComponent<BoxCollider2D>().size.x * current.localScale.x;
        float previousWidth = previous.GetComponent<BoxCollider2D>().size.x * previous.localScale.x;

        float overlap = Mathf.Max(0, previousWidth - Mathf.Abs(current.position.x - previous.position.x));
        return overlap / Mathf.Min(currentWidth, previousWidth);
    }

    void ApplyBuildingShake()
    {
        if (buildingParent.childCount > 1)
        {
            float shake = Mathf.Sin(Time.time * 5) * shakeMagnitude;
            buildingParent.transform.position = new Vector3(shake, buildingParent.transform.position.y, 0);
        }
    }

    // 保留原有协程，确保在释放方块后也能检查游戏结束
    IEnumerator CheckGameOver()
    {
        yield return new WaitForSeconds(1f);

        // 只有当游戏未结束时才检查，避免重新开始时的误判
        if (!isGameOver)
        {
            CheckGameOverInUpdate();
        }
    }

    IEnumerator SpawnNextBlock()
    {
        yield return new WaitForSeconds(0.5f); // 稍微延迟，让玩家看到上一个方块的放置结果

        if (!isGameOver)
        {
            SpawnNewBlock();
        }
    }

    void GameOver()
    {
        isGameOver = true;
        gameOverSound.Play();
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
        }
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
        }
    }

    public void RestartGame()
    {
        // 停止所有正在运行的协程，避免重新开始时的误判
        StopAllCoroutines();
        InitializeGame();
        StartGame();
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }
}
