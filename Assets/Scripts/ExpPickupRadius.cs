using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class ExpPickupRadius : MonoBehaviour
{
    private CircleCollider2D col;
    private Transform player;

    private void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        player = transform.parent;
    }

    private void Start()
    {
        CircleCollider2D myCol = GetComponent<CircleCollider2D>();
        Collider2D playerCol = transform.parent.GetComponent<Collider2D>();

        if (playerCol != null)
            Physics2D.IgnoreCollision(myCol, playerCol);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Dog"))
        {
            Debug.Log("Ghost を無視しました");
            return;
        }

       
    }


    public void AddRadius(float extra)
    {
        float before = col.radius;
        col.radius += extra;
    }
}
