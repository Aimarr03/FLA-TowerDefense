using UnityEngine;
using UnityEngine.UI;

public class UI_DurationDisplay : MonoBehaviour
{
    [SerializeField] private Image fillDuration;
    float baseDuration;
    float currentDuration;
    void Start()
    {
        GameplayManager.instance.onchangedState += OnChangeState;
        OnChangeState(GameplayManager.State.Building);
    }
    private void OnDestroy()
    {
        GameplayManager.instance.onchangedState -= OnChangeState;
    }
    private void OnChangeState(GameplayManager.State newState)
    {
        switch (newState)
        {
            case GameplayManager.State.Building:
                gameObject.SetActive(true);
                baseDuration = GameplayManager.instance.currentBuildPhaseDuration;
                currentDuration = baseDuration;
                fillDuration.fillAmount = 1;
                break;
            default:
                gameObject.SetActive(false);
                break;
        }
    }
    
    void Update()
    {
        if (GameplayManager.instance.GameState == GameplayManager.State.Building)
        {
            currentDuration -= Time.deltaTime;
            fillDuration.fillAmount = currentDuration / baseDuration;
            if(currentDuration <= 0)
            {
                currentDuration = 0;
            }
        }
    }
}
