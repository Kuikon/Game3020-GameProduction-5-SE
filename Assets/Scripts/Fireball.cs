using UnityEngine;

public class Fireball : MonoBehaviour
{
    public enum FireballType { Normal, Meteor }
    public FireballType type = FireballType.Normal;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // タイプに応じて挙動を切り替える
        switch (type)
        {
            case FireballType.Normal:
                rb.gravityScale = 0f; // 火の玉は浮遊
                break;

            case FireballType.Meteor:
                rb.gravityScale = 1f; // 隕石は落下
                break;
        }
    }

    /// <summary>
    /// 移動速度を設定
    /// </summary>
    public void SetVelocity(Vector2 velocity)
    {
        if (rb != null) rb.linearVelocity = velocity;
    }

    /// <summary>
    /// 見た目を切り替えたい時に使う
    /// （SpriteをInspectorで差し替えられるように）
    /// </summary>
    public void SetSprite(Sprite newSprite)
    {
        if (sr != null) sr.sprite = newSprite;
    }
}
