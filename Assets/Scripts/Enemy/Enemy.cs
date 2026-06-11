using System;
using UnityEngine;
[Flags]
public enum EnemyTraverseType
{
    Ground = 0x01,
    Fly = 0x02
}
public enum EnemyType
{
    Goblin,
    Slime,
    Wolf,
    Bee
}
public class Enemy : MonoBehaviour
{
    [Header("General Data")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float currentHealh;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private float damage = 5;
    [SerializeField] private float damageRate = 1f;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private EnemyType enemyType = EnemyType.Bee;
    [SerializeField] private EnemyTraverseType enemyGroundType = EnemyTraverseType.Ground;

    [Header("Behaviour")]
    [SerializeField] private MainBase target;
    private HealthBar healthBar;
    private CharacterMovement characterMovement;
    private EnemyAnimation enemyAnimation;

    private float currentAttackTick = 0f;
    private Vector2 direction;
    private int bounty = 10;
    public Action<Enemy, bool> OnDie;
    public EnemyTraverseType Type => enemyGroundType;
    Action currentAction;

    public bool isDead => currentHealh <= 0;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealh;
    public EnemyType EnemyType => enemyType;
    private void Awake()
    {
        characterMovement = GetComponent<CharacterMovement>();
        healthBar = GetComponentInChildren<HealthBar>(true);
        enemyAnimation = GetComponentInChildren<EnemyAnimation>();

        characterMovement.MovementSpeed = movementSpeed;
        currentHealh = maxHealth;
    }
    void Start()
    {
        GameplayManager.instance.onchangedState += OnGameStateChange; 
    }
    void OnDestroy()
    {
        GameplayManager.instance.onchangedState -= OnGameStateChange; 
    }

    private void Update()
    {
        currentAction?.Invoke();
    }
    public void Init(EnemyData enemyData)
    {
        maxHealth = enemyData.MaxHealth;
        maxHealth = maxHealth * GameplayManager.instance.MultiplierHP;
        damage = enemyData.Damage; 
        damageRate = enemyData.AttackRate;
        movementSpeed = enemyData.MovementSpeed;
        enemyGroundType = enemyData.enemyGroundType;
        enemyType = enemyData.enemyType;
        
        currentHealh = maxHealth;

        float finalMovementSpeed = movementSpeed;
        characterMovement.MovementSpeed = finalMovementSpeed;
        bounty = enemyData.bounty;

        target = GameplayManager.instance.MainBase;
        target.OnDeath += StopAction;
        PreMove();
    }
    private void OnGameStateChange(GameplayManager.State state)
    {
        bool stopCondition = state switch
        {
             GameplayManager.State.GameOver => true,
             GameplayManager.State.Win => true,
             _ => false
        };

        if (stopCondition)
        {
            StopAction(); 
        }
    }
    private void StopAction()
    {
        characterMovement.CancelWalk();
        currentAction = null;
    }
    private void PreMove()
    {
        characterMovement.OnArrived += OnReachTarget;
        characterMovement.ChangeMoveDirection += OnChangeMoveDirection;
        
        characterMovement.SetPath(target.transform.position);
        characterMovement.MaxDistance = attackRange;
    }

    private void OnChangeMoveDirection(Vector2 direction)
    {
        enemyAnimation.SetDirection(direction);
        this.direction = direction;
    }

    private void OnReachTarget()
    {
        if(target == null)
            return;
        
        target.TakeDamage(1);
        characterMovement.OnArrived -= OnReachTarget;
        characterMovement.ChangeMoveDirection -= OnChangeMoveDirection;
        
        OnDie?.Invoke(this, true);
        Destroy(gameObject, 1);
    }
    private void Attacking()
    {
        currentAttackTick += Time.deltaTime;
        if(currentAttackTick > damageRate)
        {
            currentAttackTick = 0;
            target.TakeDamage(damage);
        }
    }
    
    public void TakeDamage(float damage)
    {
        if(isDead) return;
        
        currentHealh -= damage;
        currentHealh = Mathf.Max(currentHealh, 0);
        healthBar.UpdateHealth(currentHealh / maxHealth);
        if(currentHealh <= 0)
        {
            healthBar.Hide();
            GetComponent<Collider2D>().enabled = false;
            StopAction();
            OnDie?.Invoke(this, false);
            enemyAnimation.OnDie();

            int finalBounty = (int) (bounty * GameplayManager.instance.MultiplierGold);
            GameplayManager.Economy.GainMoney(finalBounty);
            //gameObject.SetActive(false);
        }
        else
        {
            enemyAnimation.OnTakeDamage();
        }
    }
}
