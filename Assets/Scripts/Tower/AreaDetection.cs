using System;
using UnityEngine;

public class AreaDetection : MonoBehaviour
{
    public CircleCollider2D circleCollider;

    public Action<Collider2D> TriggerEnter2D;
    public Action<Collider2D> TriggerExit2D;
    private void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[Debug Tower]Trigger: {collision.gameObject}");
        TriggerEnter2D?.Invoke(collision);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        TriggerExit2D?.Invoke(collision);
    }
}
