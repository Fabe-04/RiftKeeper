using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float speed = 12f;
    [SerializeField] float lifetime = 3f;
    [SerializeField] int damage = 25;

    [Header("Recuperación")]
    public GameObject flechaPickupPrefab; // <--- AQUÍ VA EL PREFAB DEL PASO 1
    [Range(0, 100)] public float probabilidad = 100f; // 100% de que caiga

    void Start()
    {
        Destroy(gameObject, lifetime); // Si no choca con nada, desaparece
    }

    private void FixedUpdate()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. SI CHOCA CON ENEMIGO
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Hit(damage);
            // --- CAMBIO: BORRAMOS 'SoltarFlecha()' DE AQUÍ ---
            // Solo hacemos daño y destruimos el proyectil.
            // El premio lo soltará el LootManager cuando el enemigo muera.
            Destroy(gameObject);
        }
        // 2. SI CHOCA CON PARED (Recuperar flechas fallidas)
        else if (collision.CompareTag("Wall"))
        {
            SoltarFlecha(); // Aquí SI dejamos que caiga, porque fallaste el tiro
            Destroy(gameObject);
        }
    }

    void SoltarFlecha()
    {
        // Verificamos si asignaste el prefab (para evitar errores)
        if (flechaPickupPrefab != null)
        {
            if (Random.Range(0f, 100f) <= probabilidad)
            {
                Instantiate(flechaPickupPrefab, transform.position, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogError("⚠️ ¡OLVIDASTE ASIGNAR EL PREFAB DE LA FLECHA EN EL INSPECTOR!");
        }
    }
}