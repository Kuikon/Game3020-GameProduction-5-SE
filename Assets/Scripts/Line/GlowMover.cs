using UnityEngine;

public class GlowMover : MonoBehaviour
{
    [SerializeField] private float destroyedTime = 1f;
    [SerializeField] private float speed = 3f;

    [Header("Color Settings")]
    [SerializeField] private Color normalColor = Color.cyan;
    [SerializeField] private Color reverseColor = Color.red;  

    private Transform target;
    private bool moveUpward = false;
    private bool reverse = false;
    private bool isDestroyScheduled = false;
    private SpriteRenderer sprite; 
    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.color = normalColor; 
        }
    }

    public void SetTarget(Transform t)
    {
        target = t;
        reverse = false;
        UpdateColor();
    }

    public void SetReverseTarget(Transform t)
    {
        target = t;
        reverse = true;
        UpdateColor();
    }

    public void ReleaseUpward()
    {
        moveUpward = true;
        target = null;
    }

    void Update()
    {
        if (moveUpward)
        {
            transform.position += Vector3.up * speed * Time.deltaTime;
            if (!isDestroyScheduled)
            {
                isDestroyScheduled = true;
                Destroy(gameObject, destroyedTime);
            }
            return;
        }

        if (target != null)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            if (reverse)
                dir *= -1f;
            transform.position += dir * speed * Time.deltaTime;
            Destroy(gameObject, destroyedTime);
        }
    }
    private void UpdateColor()
    {
        if (sprite == null) return;
        sprite.color = reverse ? reverseColor : normalColor;
    }
}
