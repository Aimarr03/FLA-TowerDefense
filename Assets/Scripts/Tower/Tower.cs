using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Tower : MonoBehaviour, I_MouseInteractable
{
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
    public State CurrentState => towerState;
    public enum State
    {
        None,
        Built
    }
    public TowerData TowerData { get; private set; }
    [SerializeReference] private List<TowerAttackBehaviour> AttackBehaviours;
    [SerializeReference] private List<TowerAttackEffect> AttackEffects;

    private void Awake()
    {
        rangeDetection = GetComponentInChildren<AreaDetection>();
        uiAction = GetComponentInChildren<UIAction>();

        AttackBehaviours = new();
        AttackEffects = new();
    }
    private void Start()
    {
        rangeDetection.circleCollider.radius = attackRange;
        rangeDetection.TriggerEnter2D += OnTriggerEnter2D;
        rangeDetection.TriggerExit2D += OnTriggerExit2D;
    }
    void Update()
    {
        if (towerState == State.None) return;
        
        if (target == null || target.isDead)
        {
            SelectPrimalTarget();
            return;
        }

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate)
        {
            Attack();
            attackTimer = 0f;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(CurrentState == State.None) return;
        
        if (other.TryGetComponent(out Enemy enemy))
        {
            TryAddEnemy(enemy);
        }
    }
    private void TryAddEnemy(Enemy enemy)
    {
        if (!IsEnemyAttackable(enemy)) return;

        enemiesInRange.Add(enemy);
        enemy.OnDie += (bool reachDestination) =>
        {
            OnEnemyDie(enemy);
        };
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (CurrentState == State.None) return;
        
        if (other.TryGetComponent(out Enemy enemy))
        {
            enemiesInRange.Remove(enemy);
            if (enemy == target)
                target = null;
        }
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
        enemiesInRange.RemoveAll(e => e == null);

        if (enemiesInRange.Count > 0)
            target = enemiesInRange[0]; // FIFO
    }

    private void Attack()
    {
        var plan = new AttackPlan();
        plan.Targets.Add(target);

        foreach(var attackBehaviour in AttackBehaviours)
            attackBehaviour.PlanAttack(this, plan);
        
        foreach(var target in plan.Targets)
        {
            if(target.isDead) continue;
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
            Damage = attackDamage
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
        return (TowerData.AttackableType & enemy.Type) != 0;
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
    }
    public void Sell()
    {
        Debug.Log("Sell");
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
    }
    private void UpdateData()
    {
        attackDamage = TowerData.Damage(level);
        attackRate = TowerData.AttackRate(level);
        attackRange = TowerData.AttackRange(level);
        rangeDetection.circleCollider.radius = attackRange;

        attackRangeEffect.transform.localScale = Vector3.one * attackRange * 2;
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
