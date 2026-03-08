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
        // 获取碰撞到的物体
        GameObject collidedObject = collision.gameObject;
        
        // 获取自身刚体
        Rigidbody2D selfRigidbody = GetComponent<Rigidbody2D>();
        
        // 获取碰撞物体的刚体
        Rigidbody2D targetRigidbody = collidedObject.GetComponent<Rigidbody2D>();

        // 调试信息
        Debug.Log("碰撞检测: 自身物体: " + gameObject.name + ", 碰撞物体: " + collidedObject.name);
        Debug.Log("碰撞检测: targetRigidbody: " + (targetRigidbody != null ? targetRigidbody.gameObject.name : "null"));
        Debug.Log("碰撞检测: selfRigidbody: " + (selfRigidbody != null ? selfRigidbody.gameObject.name : "null"));
        Debug.Log("碰撞检测: 是否自身碰撞: " + (gameObject == collidedObject));
        Debug.Log("碰撞检测: 是否刚体相同: " + (targetRigidbody == selfRigidbody));

        // 确保碰撞到的物体有刚体，且不是自身，否则关节无法连接
        if (targetRigidbody != null && gameObject != collidedObject)
        {
            // 2. 动态添加一个固定关节
            FixedJoint2D joint = gameObject.AddComponent<FixedJoint2D>();

            // 3. 将关节连接到碰撞到的物体上
            joint.connectedBody = targetRigidbody;

            Debug.Log("方块已连接到: " + collidedObject.name);

            // 禁用当前方块的刚体运动，使其保持稳定
            selfRigidbody.bodyType = RigidbodyType2D.Kinematic;
        }
        else
        {
            Debug.Log("碰撞检测: 跳过自身碰撞或无刚体的物体");
        }
        // 当方块与其他物体碰撞时，通知游戏管理器
        if (gameManager != null)
        {
            gameManager.OnBlockCollision();
        }
    }
}