using UnityEngine;
using UnityEngine.AdaptivePerformance;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float projectileSpeed = 3f;
    [SerializeField] private float damage = 1f;

    Enemy target;
    AttackContext attackContext;
    public void SetTarget(AttackContext attackContext)
    {
        this.attackContext = attackContext;
        target = attackContext.Target;
        damage = attackContext.Damage;
    }
    private void Update()
    {
        if (target == null) return;
        if (target.isDead)
        {
            gameObject.SetActive(false);
            Destroy(gameObject, 1f);
            target = null;
            return;
        }
        transform.position = Vector2.MoveTowards(transform.position, target.transform.position, projectileSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(target == null)
        {
            Destroy(gameObject, 1f);
            gameObject.SetActive(false);
            return;    
        }
        
        if(collision.gameObject == target.gameObject)
        {
            target.OnDie += OnEnemyDie;
            
            target.TakeDamage(damage);
            var performace = attackContext.Source.performance;
            performace.damageDealt += damage;

            foreach(var attackEffect in attackContext.Effects)
            {
                attackEffect.Apply(attackContext);
            }

            gameObject.SetActive(false);
            target.OnDie -= OnEnemyDie;
            Destroy(gameObject, 1f);
        }
    }
    private void OnEnemyDie(Enemy enemym, bool dieByTower)
    {
        var tower = attackContext.Source;
        if(tower.CurrentState == Tower.State.None)
            return;
        var towerPerformance = attackContext.Source.performance;
        towerPerformance.enemySlain++;
    }
}
