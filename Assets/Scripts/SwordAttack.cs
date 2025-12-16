using System.Collections;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    [Header("Configuración del Golpe")]
    [SerializeField] int damage = 40;
    [SerializeField] float attackDuration = 0.2f;
    [SerializeField] float swingAngle = 90f;

    [Header("Referencias")]
    [SerializeField] Collider2D hitbox;

    private bool isAttacking = false;

    private void Awake()
    {
        if (hitbox == null) hitbox = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (hitbox != null) hitbox.enabled = false;
    }

    // --- NUEVO: Función para mejorar el daño desde la tienda ---
    public void UpgradeDamage(int amount)
    {
        damage += amount;
        Debug.Log($"¡Espada mejorada! Nuevo daño: {damage}");
    }
    // -----------------------------------------------------------

    public void Attack(Vector2 aimDirection)
    {
        if (isAttacking) return;
        StartCoroutine(PerformSwing(aimDirection));
    }

    private IEnumerator PerformSwing(Vector2 direction)
    {
        isAttacking = true;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float startAngle, endAngle;

        if (direction.x < 0)
        {
            startAngle = targetAngle - (swingAngle / 2);
            endAngle = targetAngle + (swingAngle / 2);
            transform.localScale = new Vector3(1, -1, 1);
        }
        else
        {
            startAngle = targetAngle + (swingAngle / 2);
            endAngle = targetAngle - (swingAngle / 2);
            transform.localScale = new Vector3(1, 1, 1);
        }

        if (hitbox != null) hitbox.enabled = true;

        float timer = 0f;
        while (timer < attackDuration)
        {
            float t = timer / attackDuration;
            float currentAngle = Mathf.Lerp(startAngle, endAngle, t);
            transform.rotation = Quaternion.Euler(0, 0, currentAngle);
            timer += Time.deltaTime;
            yield return null;
        }

        if (hitbox != null) hitbox.enabled = false;
        isAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) return;

        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy == null) enemy = collision.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            enemy.Hit(damage);
        }
    }
}