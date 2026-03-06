using UnityEngine;

public class Block : MonoBehaviour
{
    // 方块的基本属性
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    
    void Start()
    {
        // 获取组件
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        // 设置初始属性
        if (rb != null)
        {
            rb.mass = 1f;
            rb.linearDamping = 0.1f;
            rb.angularDamping = 0.5f;
        }
        
        if (boxCollider != null)
        {
            boxCollider.size = new Vector2(1, 1);
        }
    }
}