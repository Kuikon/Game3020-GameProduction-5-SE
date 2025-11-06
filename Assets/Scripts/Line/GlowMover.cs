using UnityEngine;

public class GlowMover : MonoBehaviour
{
    [SerializeField] private float destroyedTime = 1f;
    private Transform target;
    private bool moveUpward = false;
    private bool reverse = false;
    private bool isDestroyScheduled = false;
    private float speed = 3f;

    public void SetTarget(Transform t)
    {
        target = t;
        reverse = false;
    }

    public void SetReverseTarget(Transform t)
    {
        target = t;
        reverse = true;
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
            // 🔁 通常 or 逆方向の制御
            Vector3 dir = (target.position - transform.position).normalized;
            if (reverse)
                dir *= -1f; // ← 逆向きにする！

            transform.position += dir * speed * Time.deltaTime;
            Destroy(gameObject, destroyedTime);
        }
    }
}
