using UnityEngine;
using UnityEngine.UI;

public class UIBox : MonoBehaviour
{
    public GhostType boxType; // ��F"Red" "Blue" "Green" �Ȃ�
    public Image boxImage;

    private Color defaultColor;

    void Start()
    {
        if (boxImage == null)
            boxImage = GetComponent<Image>();

        defaultColor = boxImage.color;
    }

    public void Highlight(bool state)
    {
        boxImage.color = state ? Color.white : defaultColor;
    }

    public RectTransform RectTransform => transform as RectTransform;
}
