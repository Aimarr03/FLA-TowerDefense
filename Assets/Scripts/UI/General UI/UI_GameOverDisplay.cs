using UnityEngine;

public class UI_GameOverDisplay : MonoBehaviour
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
            gameObject.SetActive(newState == GameplayManager.State.GameOver);
            GameplayManager.instance.onchangedState -= OnGameStateChanged;
        }
    }
}
