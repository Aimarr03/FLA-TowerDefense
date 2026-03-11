using UnityEngine;

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
        if(target == null) return;
        transform.position = Vector2.MoveTowards(transform.position, target.transform.position, projectileSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == target.gameObject)
        {
            target.TakeDamage(damage);
            foreach(var attackEffect in attackContext.Effects)
            {
                attackEffect.Apply(attackContext);
            }

            gameObject.SetActive(false);
            Destroy(gameObject, 1f);
        }
    }
}
