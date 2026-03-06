using UnityEngine;

public class BlockCollision : MonoBehaviour
{
    private Game gameManager;

    public void SetGameManager(Game manager)
    {
        gameManager = manager;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 当方块与其他物体碰撞时，通知游戏管理器
        if (gameManager != null)
        {
            gameManager.OnBlockCollision();
        }
    }
}