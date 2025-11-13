using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float speed = 12f;
    [SerializeField] float lifetime = 3f; // Tiempo de vida automático
    [SerializeField] int damage = 25;

    void Start()
    {
        // Auto-destrucción después de tiempo por si no choca con nada
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        // ✅ Velocidad corregida
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificar si es enemigo
        var enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Hit(damage);
            Destroy(gameObject);
        }
        // También destruirse al chocar con paredes/obstáculos
        else if (collision.CompareTag("Wall") || collision.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
