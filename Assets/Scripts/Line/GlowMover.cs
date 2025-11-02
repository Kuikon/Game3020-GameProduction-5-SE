using UnityEngine;

public class GlowMover : MonoBehaviour
{
    private Transform target;
    public float moveSpeed = 5f;
    public float disappearDistance = 0.2f;

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    void Update()
    {
        if (target == null) return;

        // ターゲット方向に移動
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        // ある程度近づいたら消える
        if (Vector3.Distance(transform.position, target.position) < disappearDistance)
        {
            Destroy(gameObject);
        }
    }
}
