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
        switch (type)
        {
            case FireballType.Normal:
                rb.gravityScale = 0f; 
                break;

            case FireballType.Meteor:
                rb.gravityScale = 1f;
                break;
        }
    }

    public void SetVelocity(Vector2 velocity)
    {
        if (rb != null) rb.linearVelocity = velocity;
    }

    public void SetSprite(Sprite newSprite)
    {
        if (sr != null) sr.sprite = newSprite;
    }
}
