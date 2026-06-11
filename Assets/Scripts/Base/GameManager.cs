using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private const string MainMenuScene = "Main Menu";
    private const string PlayScene = "Prototipe_Main";

    public static GameManager instance;
    public GameConfig currentGameConfig;
    void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
        instance = this;
    }
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public void LoadPlayScene(GameConfig config)
    {
        currentGameConfig = config;
        SceneManager.LoadScene(PlayScene);
    }

    private void OnSceneLoaded(Scene loadedScene, LoadSceneMode loadSceneMode)
    {
        string loadedSceneName = loadedScene.name;
        
        var playScene = PlayScene.ToLower();
        var mainMenuScene = MainMenuScene.ToLower();
        loadedSceneName = loadedSceneName.ToLower();
        
        if(loadedSceneName == mainMenuScene)
        {
                   
        }
        else if(loadedSceneName == playScene)
        {
            var experimentManager = FindFirstObjectByType<ExperimentManager>();
            if (currentGameConfig.useExperiment)
            {
                experimentManager.Initialized();
            }
            else
            {
                Debug.Log("Debug Scene gameplay");       
                var gameplayManager = FindFirstObjectByType<GameplayManager>();
                gameplayManager.InitializeGame(currentGameConfig);
                gameplayManager.StartGame();
            }

        }
    }
}
