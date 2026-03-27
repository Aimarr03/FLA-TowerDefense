using TMPro;
using UnityEngine;

public class UI_MoneyDisplay : MonoBehaviour
{
    [SerializeField] private string preHeader = "Money";
    
    private TextMeshProUGUI moneyText;
    private int bufferMoney = 0;
    private void Awake()
    {
        moneyText = GetComponentInChildren<TextMeshProUGUI>();
    }
    private void Start()
    {
        TD_API.Economy.OnMoneyChange += UpdateDisplayMoney;
    }
    private void OnDestroy()
    {
        TD_API.Economy.OnMoneyChange -= UpdateDisplayMoney;
    }
    private void UpdateDisplayMoney(int newMoney)
    {
        moneyText.text = $"{preHeader}: {newMoney}";
        bufferMoney = newMoney;
    }

}
