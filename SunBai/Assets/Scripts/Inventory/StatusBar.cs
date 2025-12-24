using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    public Image fillImage; // Should be Image type = Filled
    public Text labelText;

    public void SetValue(float current, float max)
    {
        if (max <= 0f) max = 1f;
        float ratio = Mathf.Clamp01(current / max);
        if (fillImage != null)
            fillImage.fillAmount = ratio;
    }

    public void SetLabel(string text)
    {
        if (labelText != null)
            labelText.text = text;
    }
}


