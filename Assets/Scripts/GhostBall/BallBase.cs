using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class BallBase : MonoBehaviour
{
    [Header("Common Ball Data")]
    public GhostType type;
    protected SpriteRenderer sr;
    protected bool isActive = true;

    protected virtual void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = GhostBase.GetColorByType(type); // ゴーストタイプの色反映
    }

    // 共通：色更新など
    public virtual void SetType(GhostType newType)
    {
        type = newType;
        if (sr != null)
            sr.color = GhostBase.GetColorByType(newType);
    }

    // 共通：消滅処理
    protected virtual void DestroyBall(float delay = 0f)
    {
        if (!isActive) return;
        isActive = false;
        Destroy(gameObject, delay);
    }
}
