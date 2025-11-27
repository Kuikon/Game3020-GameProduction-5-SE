using UnityEngine;

public class ExpDropManager : MonoBehaviour
{
    public static ExpDropManager Instance;

    [Header("Drop Prefab")]
    public GameObject expOrbPrefab;

    private void Awake()
    {
        Instance = this;
        if (expOrbPrefab == null)
        {
            expOrbPrefab = Resources.Load<GameObject>("Prefabs/EXPOrb");

            if (expOrbPrefab == null)
                Debug.LogError("❌ EXPOrb not found in Resources!");
        }
    }

    public void SpawnExpOrbs(Vector3 position, int count, int expPerDrop, Transform player)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(-0.1f, 0.3f),
                0f
            );

            GameObject orb = Instantiate(
                expOrbPrefab,
                position + offset,
                Quaternion.identity
            );

            DroppedBall db = orb.GetComponent<DroppedBall>();
            if (db != null)
            {
                db.expAmount = expPerDrop;
            }
        }
    }
}
