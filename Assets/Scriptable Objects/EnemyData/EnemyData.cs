using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/Create Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string EnemyName = "";
    public string EnemyDesc = "";
    
    public float MaxHealth = 20;
    public float MovementSpeed = 5f;

    public float Damage = 10;
    [Range(0.33f, 2f)] public float AttackRate = 1.1f;

    public EnemyTraverseType enemyGroundType;
    public EnemyType enemyType;

    public Enemy enemyPrefab;
}
