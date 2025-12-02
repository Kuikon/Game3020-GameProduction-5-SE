using UnityEngine;
using System;
using System.Collections;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target to follow")]
    [SerializeField] Transform target;

    [Header("Camera settings")]
    [SerializeField] float smoothSpeed = 5f;
    [SerializeField] Vector3 offset;

    [Header("Patrol Settings")]
    public Transform[] ghostPoints;      
    public float patrolMoveSpeed = 3f;  
    public float lookTime = 1.5f;       

    [Header("Player control")]
    [SerializeField] PlayerController playerController;
    public Action OnPatrolStarted;                 
    public Action<Transform> OnReachedPoint;
    public Action OnPatrolEnd;

    private bool isPatrolling = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (playerController == null && target != null)
        {
            playerController = target.GetComponentInParent<PlayerController>();
        }
    }
    private void Start()
    {
        if (ghostPoints == null || ghostPoints.Length == 0)
        {
            GameObject pointsRoot = GameObject.Find("GhostPoints");
            if (pointsRoot != null)
            {
                ghostPoints = pointsRoot.GetComponentsInChildren<Transform>();
            }
            else
            {
                Debug.LogWarning("GhostPoints object not found in scene");
            }
        }
    }
    void LateUpdate()
    {
        if (isPatrolling) return;
        if (target == null) return;

        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
    }
    public void StartPatrol()
    {
        if (!gameObject.activeInHierarchy)
        {
           
            gameObject.SetActive(true);
        }
        if (!isPatrolling)
        {
            OnPatrolStarted?.Invoke(); 
            StartCoroutine(PatrolRoutine());
            GameManager.Instance.DisablePlayerControl();
        }
    }
    IEnumerator PatrolRoutine()
    {
        isPatrolling = true;
        if (playerController != null)
        {
            playerController.enabled = false;
            var rb = playerController.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }
        foreach (Transform point in ghostPoints)
        {
            yield return StartCoroutine(MoveToPoint(point)); 
            yield return new WaitForSeconds(lookTime);     
        }
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        isPatrolling = false;
        OnPatrolEnd?.Invoke();
        GameManager.Instance.EnablePlayerControl();
    }
    public void SetGhostPoints(Transform[] points)
    {
        ghostPoints = points;
    }
    IEnumerator MoveToPoint(Transform point)
    {
        Vector3 targetPos = point.position;
        targetPos.z = transform.position.z;
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPos,
                patrolMoveSpeed * Time.deltaTime
            );
            yield return null;
        }
        OnReachedPoint?.Invoke(point);
    }
}
