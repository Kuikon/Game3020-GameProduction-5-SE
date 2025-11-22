using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target to follow")]
    [SerializeField] Transform target;       
    [Header("Camera settings")]
    [SerializeField] float smoothSpeed = 5f;
    [SerializeField] Vector3 offset;
    void Awake() => DontDestroyOnLoad(gameObject);

    void LateUpdate()
    {
        if (target == null) return;


        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
