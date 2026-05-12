using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BotController : MonoBehaviour
{
    private List<Tower> allTower;
    private List<BuyAction> buyActions;
    private bool isActive = false;
    float intervalDecision = 0.5f;
    float currentInterval = 0f;
    public void Init()
    {
        allTower = FindObjectsByType<Tower>(FindObjectsSortMode.None).ToList();
        isActive = true;
        
        buyActions = new();
        foreach(var buyAct in TD_API.BuildActions)
        {
            buyActions.Add(buyAct as BuyAction);
        }

        currentInterval = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive) return;

        currentInterval += Time.deltaTime;
        if(currentInterval > intervalDecision)
        {
            currentInterval = 0;
            Build();
        }
    }
    private void Build()
    {
        Tower tower = GetRandomUnBuiltTower();
        TowerActionSO buildAction = GetRandomSufficientTowerBuild();
        if(tower == null || buildAction == null) return;

        buildAction.Executes(tower);
    }
    private Tower GetRandomUnBuiltTower()
    {
        List<Tower> emptyTower = allTower.Where(tower => tower.CurrentState == Tower.State.None).ToList();
        int maxIndex = emptyTower.Count() - 1;
        if(maxIndex < 0) 
            return null;
        
        int randomIndex = Random.Range(0, maxIndex + 1);
        Tower randomNotBuiltTower = emptyTower[maxIndex];
        return randomNotBuiltTower;
    }
    private TowerActionSO GetRandomSufficientTowerBuild()
    {
        var actions = buyActions.Where(tower => TD_API.Economy.IsEnough(tower.GetBuildCost())).ToList();
        return GetRandomBuildTowerAction(actions);
    }
    private TowerActionSO GetRandomBuildTowerAction(List<BuyAction> actions)
    {
        if(actions.Count == 0)
        {
            return null;
        }
        int maxIndex = actions.Count - 1;
        int randomIndex = Random.Range(0, maxIndex + 1);

        TowerActionSO randomBuildAction = actions[randomIndex];
        return randomBuildAction;
    }
}
