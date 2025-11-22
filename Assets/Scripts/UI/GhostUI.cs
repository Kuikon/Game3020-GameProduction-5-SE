using UnityEngine;

public class GhostUIOrbit : MonoBehaviour
{
    public RectTransform targetRect;    
    public float speed = 200f;          

    private Vector2[] points;         
    private int currentIndex = 0;

    private RectTransform rect;
    private Animator animator;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        animator = GetComponent<Animator>();

        UpdateCornerPoints();
    }

    void Update()
    {
        if (targetRect == null || rect == null) return;

        Vector2 current = rect.anchoredPosition;
        Vector2 next = points[currentIndex];

 
        Vector2 dir = (next - current).normalized;

        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);

        float movingSpeed = speed * Time.deltaTime;
        animator.SetFloat("Speed", movingSpeed);


        Vector2 newPos = Vector2.MoveTowards(current, next, movingSpeed);
        rect.anchoredPosition = newPos;


        if (Vector2.Distance(newPos, next) < 1f)
        {
            currentIndex = (currentIndex + 1) % points.Length;
        }
    }


    void UpdateCornerPoints()
    {
        Vector2 size = targetRect.sizeDelta;
        float w = size.x / 2f;
        float h = size.y / 2f;

        points = new Vector2[]
        {
            new Vector2(-w,  h),
            new Vector2( w,  h), 
            new Vector2( w, -h), 
            new Vector2(-w, -h),
        };
    }
}
