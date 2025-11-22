using UnityEngine;

public class DrawLineItem : MonoBehaviour
{
    public float floatSpeed = 2f;        
    public float floatAmplitude = 0.1f; 

    private float floatTimer = 0f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
        floatTimer = Random.Range(0f, Mathf.PI * 2f); 
    }

    void Update()
    {
        floatTimer += Time.deltaTime * floatSpeed;
        float offset = Mathf.Sin(floatTimer) * floatAmplitude;

        transform.position = startPos + new Vector3(0, offset, 0);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            AbilityManager.Instance.canDrawLine = true;
            Destroy(gameObject);
        }
    }
}
