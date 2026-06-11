using UnityEngine;

public class UI_GameWinDisplay : MonoBehaviour
{
    private void Start()
    {
        GameplayManager.instance.onchangedState += OnGameStateChanged;
        gameObject.SetActive(false);
    }

    private void OnGameStateChanged(GameplayManager.State newState)
    {
        bool removalCallbackCondition = newState switch
        {
            GameplayManager.State.Win => true,
            GameplayManager.State.GameOver => true,
            _ => false
        };
        if (removalCallbackCondition)
        {
            gameObject.SetActive(newState == GameplayManager.State.Win);
            GameplayManager.instance.onchangedState -= OnGameStateChanged;
        }
    }
}
