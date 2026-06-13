using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private RectTransform MainOptions;
    [SerializeField] private RectTransform PlayOptions;
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickClip;
    void Awake()
    {
        MainMenuOption();
    }
    public void PlayOption()
    {
        PlayOptions.gameObject.SetActive(true);
        MainOptions.gameObject.SetActive(false);
    }
    public void MainMenuOption()
    {
        PlayOptions.gameObject.SetActive(false);
        MainOptions.gameObject.SetActive(true);
    }
    public void PlayStatic()
    {
        var builder = new GameConfigBuilder();
        builder.SetScenarioMode(GameplayManager.ScenarioMode.Static);
        GameManager.instance.LoadPlayScene(builder.Build());
    }
    public void PlayDynamic()
    {
        var builder = new GameConfigBuilder();
        builder.SetScenarioMode(GameplayManager.ScenarioMode.DDA);
        GameManager.instance.LoadPlayScene(builder.Build());
    }
    public void PlayClickClip()
    {
        audioSource.PlayOneShot(clickClip);
    }
}
