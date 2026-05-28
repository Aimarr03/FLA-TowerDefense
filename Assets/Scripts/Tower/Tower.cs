using System;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour, I_MouseInteractable
{
    [Serializable]
    public class Performance
    {
        public float damageDealt;
        public int enemySlain;
        public float currentScore = 0;
    }
    [Header("General Info")]
    [SerializeField] private string TowerName = string.Empty;
    [SerializeField] private Bullet bullet;
    [SerializeField] private float attackDamage = 3f;
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private float attackRange = 3;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private Sprite unBuildSprite;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer attackRangeEffect;


    [Header("Bot Property")]
    [SerializeField, Range(1,10)] private int preferenceScore;
    private AreaDetection rangeDetection;
    private UIAction uiAction;

    public readonly List<Enemy> enemiesInRange = new();
    private Enemy target;
    private float attackTimer;

    private int level = -1;
    private int maxLevel = 3;
    private State towerState = State.None;

    public int Level => level;
    public bool IsMax => level >= maxLevel;
    public int PreferenceScore => preferenceScore;
    public State CurrentState => towerState;
    public enum State
    {
        None,
        Built
    }
    public TowerData TowerData { get; private set; }
    [Header("Behaviour Property")]
    [SerializeReference] private List<TowerAttackBehaviour> AttackBehaviours;
    [SerializeReference] private List<TowerAttackEffect> AttackEffects;
    public static Action<Tower,TowerActionType> ActionInvoke;
    public Performance performance;
    private void Awake()
    {
        rangeDetection = GetComponentInChildren<AreaDetection>();
        uiAction = GetComponentInChildren<UIAction>();

        AttackBehaviours = new();
        AttackEffects = new();
        performance = new();
    }
    private void Start()
    {
        rangeDetection.circleCollider.radius = attackRange;
        rangeDetection.TriggerEnter2D += TriggerEnter;
        rangeDetection.TriggerExit2D += TriggerExit;
    }
    void Update()
    {
        if (towerState == State.None) return;
        
        float decayedPerformance = performance.currentScore - (Time.deltaTime * 3);
        performance.currentScore = Mathf.Max(decayedPerformance, 0);
        
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate)
        {
            SelectPrimalTarget();
            Attack();
            attackTimer = 0f;
        }
    }
    private void TriggerEnter(Collider2D other)
    {
        if(CurrentState == State.None) return;
        
        if (other.TryGetComponent(out Enemy enemy))
        {
            TryAddEnemy(enemy);
        }
    }
    private void TriggerExit(Collider2D other)
    {
        if (CurrentState == State.None) return;
        
        if (other.TryGetComponent(out Enemy enemy))
        {
            enemiesInRange.Remove(enemy);
            if (enemy == target)
                target = null;
        }
    }
    private void TryAddEnemy(Enemy enemy)
    {
        if (!IsEnemyAttackable(enemy)) return;

        Debug.Log($"Added Enemy, Health: {enemy.MaxHealth} || {enemy != null && !enemy.isDead}");
        enemiesInRange.Add(enemy);
        enemy.OnDie += (Enemy enemy, bool reachDestination) =>
        {
            OnEnemyDie(enemy);
        };
    }

    
    private void DetectEnemiesInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Enemy enemy))
            {
                TryAddEnemy(enemy);
            }
        }
    }
    private void SelectPrimalTarget()
    {
        enemiesInRange.RemoveAll(e => e == null || e.isDead);

        if (enemiesInRange.Count > 0)
            target = enemiesInRange[0]; // FIFO
    }

    private void Attack()
    {
        Debug.Log("[Debug Tower] Attack");
        var plan = new AttackPlan();
        plan.Targets.Add(target);

        foreach(var attackBehaviour in AttackBehaviours)
            attackBehaviour.PlanAttack(this, plan);
        
        foreach(var target in plan.Targets)
        {
            if(target == null || target.isDead) continue;
            for(int i = 0; i < plan.Targets.Count; i++)
            {
                Fire(target);
            }
        }
    }
    private void Fire(Enemy enemy)
    {
        AttackContext attackContext = new AttackContext()
        {
            Target = enemy,
            Source = this,
            Damage = attackDamage,
            towerType = TowerData.Type,
            AttackableType = TowerData.AttackableType
        };
        
        foreach(var attackEffect in AttackEffects)
            attackContext.Effects.Add(attackEffect);

        Bullet newBullet = Instantiate(bullet, transform.position, Quaternion.identity);
        newBullet.gameObject.SetActive(true);
        newBullet.SetTarget(attackContext);
    }
    private void OnEnemyDie(Enemy enemy)
    {
        if(enemy == target)
        {
            SelectPrimalTarget();
        }
    }
    public bool IsEnemyAttackable(Enemy enemy)
    {
        bool isAttackable = (TowerData.AttackableType & enemy.Type) != 0;
        Debug.Log($"[Debug Tower] {isAttackable}");
        return isAttackable;
    }

    public void Build(TowerData towerData)
    {
        Debug.Log("Build");
        towerState = State.Built;
        level = 1;
        
        TowerData = towerData;
        TowerName = TowerData.TowerName;

        float buyCost = TowerData.CostBuild();
        TD_API.Economy.UseMoney((int)buyCost);
        visual.sprite = towerData.GetSprite(level - 1);

        foreach(var attackEffect in TowerData.TowerAttackEffects)
        {
            if (!AttackEffects.Contains(attackEffect))
            {
                AttackEffects.Add(attackEffect);
            }
        }
        foreach (var attackBehaviour in TowerData.TowerAttackBehaviours)
        {
            if (!AttackBehaviours.Contains(attackBehaviour))
            {
                AttackBehaviours.Add(attackBehaviour);
            }
        }
        animator.runtimeAnimatorController = towerData.animatorController;
        animator.Play($"Upgrade_{level}");
        UpdateData();
        
        DetectEnemiesInRange();
        performance = new()
        {
            currentScore = 160
        };
        ActionInvoke?.Invoke(this,TowerActionType.Buy);
    }
    public void Sell()
    {
        Debug.Log("Sell");
        ActionInvoke?.Invoke(this,TowerActionType.Presell);
        float sellCost = TowerData.SellCost(level);

        TD_API.Economy.GainMoney((int)sellCost);
        towerState = State.None;
        level = -1;

        animator.runtimeAnimatorController = null;
        visual.sprite = unBuildSprite;

        foreach (var attackEffect in TowerData.TowerAttackEffects)
        {
            if (AttackEffects.Contains(attackEffect))
            {
                AttackEffects.Remove(attackEffect);
            }
        }
        foreach (var attackBehaviour in TowerData.TowerAttackBehaviours)
        {
            if (AttackBehaviours.Contains(attackBehaviour))
            {
                AttackBehaviours.Remove(attackBehaviour);
            }
        }
        
        TowerName = "";
        attackRange = 0;
        attackDamage = 0;
        attackRate = 0;

        TowerData = null;
        performance = new();
        ActionInvoke?.Invoke(this,TowerActionType.Sell);
        OnMouseDeselect();
    }
    public void Upgrade()
    {
        Debug.Log("Upgrade");
        level++;
        level = Mathf.Min(level, maxLevel);
        visual.sprite = TowerData.GetSprite(level - 1);

        float upgradeCost = TowerData.UpgradeCost(level);
        TD_API.Economy.UseMoney((int)upgradeCost);
        animator.Play($"Upgrade_{level}");
        UpdateData();

        DetectEnemiesInRange();
        performance.currentScore += 75;
        ActionInvoke?.Invoke(this, TowerActionType.Upgrade);
    }
    private void UpdateData()
    {
        attackDamage = TowerData.Damage(level);
        attackRate = TowerData.AttackRate(level);
        attackRange = TowerData.AttackRange(level);
        rangeDetection.circleCollider.radius = attackRange;

        attackRangeEffect.transform.localScale = Vector3.one * attackRange * 2;
        attackTimer = 0f;
    }


    public void OnHighlighted()
    {
           
    }

    public void OnDehighlighted()
    {
        
    }

    public void OnMouseLeftDown()
    {
        
    }

    public void OnMouseSelect()
    {
        if(CurrentState == State.Built)
        {
            attackRangeEffect.gameObject.SetActive(true);
        }
        List<ActionContext> listActions = new();
        var actions = TD_API.TowerActions;
        switch (CurrentState)
        {
            case State.None:
                actions = TD_API.BuildActions;
                break;
            
            case State.Built:
                actions = TD_API.TowerActions;
                break;
        }
        foreach (var action in actions)
        {
            listActions.Add(action.GetActionContext(this));
        }
        bool condition = GameplayManager.instance.GameState switch 
        { 
            GameplayManager.State.Building => true, 
            GameplayManager.State.Defending => true,
            _ => false 
        };
        if (!condition) return;
        uiAction.OnDisplayAction(listActions);
    }

    public void OnMouseDeselect()
    {
        uiAction.CloseAction();
        attackRangeEffect.gameObject.SetActive(false);
    }
}
