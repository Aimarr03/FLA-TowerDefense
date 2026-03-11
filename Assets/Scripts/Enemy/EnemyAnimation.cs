using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    Animator animator;
    private readonly int XDir = Animator.StringToHash("XDir");
    private readonly int YDir = Animator.StringToHash("YDir");
    private readonly int DamagedConst = Animator.StringToHash("Damaged");
    private readonly int DeathConst = Animator.StringToHash("Death");
    private readonly int DieConst = Animator.StringToHash("Die");
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetDirection(Vector2 direction)
    {
        animator.SetFloat(XDir, direction.x);
        animator.SetFloat(YDir, direction.y);
    }
    public void OnTakeDamage()
    {
        animator.SetTrigger(DamagedConst);
    }
    public void OnDie()
    {
        animator.SetBool(DeathConst, true);
        animator.SetTrigger(DieConst);
    }
}
