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
        // �^�C�v�ɉ����ċ�����؂�ւ���
        switch (type)
        {
            case FireballType.Normal:
                rb.gravityScale = 0f; // �΂̋ʂ͕��V
                break;

            case FireballType.Meteor:
                rb.gravityScale = 1f; // 覐΂͗���
                break;
        }
    }

    /// <summary>
    /// �ړ����x��ݒ�
    /// </summary>
    public void SetVelocity(Vector2 velocity)
    {
        if (rb != null) rb.linearVelocity = velocity;
    }

    /// <summary>
    /// �����ڂ�؂�ւ��������Ɏg��
    /// �iSprite��Inspector�ō����ւ�����悤�Ɂj
    /// </summary>
    public void SetSprite(Sprite newSprite)
    {
        if (sr != null) sr.sprite = newSprite;
    }
}
