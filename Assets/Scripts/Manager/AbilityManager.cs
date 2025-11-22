using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;

    public bool canDrawLine;
    public bool hasSpeedUp;
    public bool hasLongerLine;
    public bool hasDarknessVision;
    public bool hasCaptureBoost;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
