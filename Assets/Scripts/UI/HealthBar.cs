using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBarImage;
    private void Awake()
    {
        Hide();
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void UpdateHealth(float normalizedValue)
    {
        if(normalizedValue > 0)
        {
            Show();
        }
        else
        {
            Hide();
        }
        healthBarImage.fillAmount = normalizedValue;
    }
}
