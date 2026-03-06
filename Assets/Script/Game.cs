using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

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

    private GameObject currentBlock;
    private float blockSpeed = 2f;
    private bool isMovingRight = true;
    private int score = 0;
    private int perfectStreak = 0;
    private float buildingHeight = 0f;
    private float maxBuildingWidth = 1f;
    private bool isGameOver = false;
    private bool isGameStarted = false;
    private float shakeMagnitude = 0f;
    private float shakeIncrement = 0.01f;
    private float maxShakeMagnitude = 0.2f;
    private Vector3 initialCameraPos;
    private Vector3 targetCameraPos; // 目标相机位置

    void Start()
    {
        InitializeGame();
    }

    public void StartGame()
    {
        isGameStarted = true;
        startText.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);
        // 不立即生成新方块，等待玩家第一次点击
    }

    void InitializeGame()
    {
        // 重置游戏状态
        score = 0;
        perfectStreak = 0;
        buildingHeight = 0f;
        shakeMagnitude = 0f;
        isGameOver = false;
        isGameStarted = false;
        currentBlock = null;

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
        // 在屏幕顶部生成
        float top = getScreenTop();
        currentBlock.transform.position = new Vector3(0, top, 0);

        Rigidbody2D rb = currentBlock.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("blockPrefab missing Rigidbody2D component!");
            return;
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        
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
                    // 方块放置完成，生成新方块
                    currentBlock = null;
                    hasCollided = false;
                    StartCoroutine(SpawnNextBlock());
                }
            }
        }
    }

    // 在Update中持续检查游戏结束条件
    void CheckGameOverInUpdate()
    {
        if (currentBlock != null)
        {
            // 检查方块是否掉落
            if (currentBlock.transform.position.y < -6)
            {
                Debug.Log("Game Over: Block fell below threshold");
                GameOver();
            }
        }

        // 检查建筑是否倾斜过度
        foreach (Transform child in buildingParent)
        {
            if (Mathf.Abs(child.rotation.eulerAngles.z) > 45)
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
        Vector3 nowPos = mainCamera.transform.position;
        float cameraHeight = mainCamera.orthographicSize * 2;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        float top = mainCamera.transform.position.y + cameraHeight / 2;
        return top;
    }

    void ReleaseBlock()
    {
        if (!isGameStarted || isGameOver) return;

        // 如果是第一次点击，生成第一个方块
        if (currentBlock == null)
        {
            SpawnNewBlock();
            return;
        }

        // 启用物理
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
        buildingHeight += 1f;

        // 相机跟随建筑高度
        Camera mainCamera = Camera.main;
        
        // 计算目标相机位置，确保建筑顶部与相机保持6f的间距
        float targetY = buildingHeight - 4f; // 让建筑顶部位于相机下方6f处
        
        // 平滑设置目标位置
        targetCameraPos = new Vector3(targetCameraPos.x, targetY, targetCameraPos.z);
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

        CheckGameOverInUpdate();
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
        InitializeGame();
        StartGame();
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }
}
