using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BallController : MonoBehaviour
{
    public Vector2 initialDirection;
    public GhostType type;
    private Rigidbody2D rb;
    private bool isDragging;
    private Vector3 offset;
    private Camera cam;

    public Boundry xBoundary;   
    public Boundry yBoundary;   

    [Header("Force Settings")]
    public float returnForce = 1f;
    private UIBox hoveredBox;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }
    void Update()
    {
        ClampPosition();
        if (isDragging)
            DetectUIBoxUnderCursor();
    }
    void OnMouseDown()
    {
        isDragging = true;
        rb.bodyType = RigidbodyType2D.Kinematic;
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mousePos.x, mousePos.y, 0);
    }

    void OnMouseDrag()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);
        Vector3 targetPos = worldPos + offset;

        targetPos.x = Mathf.Clamp(targetPos.x, xBoundary.min, xBoundary.max);
        targetPos.y = Mathf.Clamp(targetPos.y, yBoundary.min, yBoundary.max);

        rb.MovePosition(targetPos);
    }

    void OnMouseUp()
    {
        isDragging = false;
        rb.bodyType = RigidbodyType2D.Dynamic;

        rb.angularVelocity = 0f;

        if (hoveredBox != null)
        {
 
            BoxManager.Instance.ProcessDrop(this, hoveredBox);
        }
        else
        {
            rb.AddForce(initialDirection * returnForce, ForceMode2D.Impulse);
        }

        hoveredBox = null;
    }
    private void DetectUIBoxUnderCursor()
    {
        hoveredBox = null;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var r in results)
        {
            var box = r.gameObject.GetComponent<UIBox>();
            if (box != null)
            {
                hoveredBox = box;
                break;
            }
        }
    }
    public IEnumerator MoveToBoxCenter(Vector3 target)
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        float t = 0f;
        Vector3 start = transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        //Destroy(gameObject); 
    }
    private void ClampPosition()
    {
        Vector3 pos = transform.position;

        if (pos.x < xBoundary.min || pos.x > xBoundary.max)
        {
            rb.linearVelocity = new Vector2(-rb.linearVelocity.x, rb.linearVelocity.y);
        }

        if (pos.y < yBoundary.min || pos.y > yBoundary.max)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -rb.linearVelocity.y);
        }

        pos.x = Mathf.Clamp(pos.x, xBoundary.min, xBoundary.max);
        pos.y = Mathf.Clamp(pos.y, yBoundary.min, yBoundary.max);
        transform.position = pos;
    }
}
