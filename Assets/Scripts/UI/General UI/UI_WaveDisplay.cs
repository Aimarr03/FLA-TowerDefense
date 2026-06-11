using System.Text;
using TMPro;
using UnityEngine;

public class UI_WaveDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI tmp_enemyWaves;
    [SerializeField] TextMeshProUGUI tmp_detailWave;

    private void Start()
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
        if (!GameplayManager.instance.IsActive) return;
        if (newState == GameplayManager.State.Defending)
        {
            tmp_enemyWaves.gameObject.SetActive(false);
            tmp_detailWave.gameObject.SetActive(false);
        }
        else if(newState == GameplayManager.State.Building)
        {
            tmp_enemyWaves.gameObject.SetActive(true);
            tmp_detailWave.gameObject.SetActive(true);

            int enemyWave = GameplayManager.instance.CurrentWave;
            int maxWaves = GameplayManager.instance.MaxWave;
            tmp_enemyWaves.text = $"Enemy Waves: {enemyWave}/{maxWaves}";

            var enemyWaveData = GameplayManager.instance.enemySpawnInfos;
            StringBuilder sb = new StringBuilder();
            foreach (var wave in enemyWaveData)
            {
                sb.AppendLine($"{wave.type.ToString()}: {wave.amount}");
            }
            string detail = sb.ToString();
            tmp_detailWave.text = detail;
        }
    }
}
