using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BallController : MonoBehaviour
{
    public static bool IsAnyBallBeingDragged = false;
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
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }
    void Update()
    {
        ClampPosition();
    }
    void OnMouseDown()
    {
        isDragging = true;
        IsAnyBallBeingDragged = true;
        rb.bodyType = RigidbodyType2D.Kinematic;
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mousePos.x, mousePos.y, 0);
    }

    void OnMouseDrag()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        transform.position = mousePos + offset;
    }

    void OnMouseUp()
    {
        isDragging = false;
        IsAnyBallBeingDragged = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(initialDirection * returnForce, ForceMode2D.Impulse);
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isDragging) return;
        UIBox box = other.GetComponent<UIBox>();
        if (box != null)
        {
            Debug.Log($"🎯 Ball {name} entered {box.name}");
            BoxManager.Instance.ProcessDrop(this, box);
        }
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
    void OnDestroy()
    {
        if (IsAnyBallBeingDragged)
            IsAnyBallBeingDragged = false;
    }
}
