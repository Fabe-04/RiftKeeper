using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Estadísticas")]
    [SerializeField] float speed = 12f;
    [SerializeField] float lifetime = 3f;
    [SerializeField] int damage = 25;

    [Header("Recuperación de Flechas")]
    [Tooltip("Prefab del item 'ArrowPickup' para recoger del suelo")]
    public GameObject flechaPickupPrefab;
    [Range(0, 100)] public float probabilidadRecuperar = 50f;

    void Start()
    {
        Destroy(gameObject, lifetime); // Autodestrucción si no choca
    }

    private void FixedUpdate()
    {
        // Mover la flecha hacia adelante (su derecha local)
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. IGNORAR AL JUGADOR (Para no dispararte a ti mismo)
        if (collision.CompareTag("Player")) return;

        // Ignorar otros proyectiles o triggers de zona
        if (collision.isTrigger && !collision.CompareTag("Enemy")) return;

        // 2. CHOQUE CON ENEMIGO
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Hit(damage);
            // Al golpear carne, la flecha se rompe (se destruye sin soltar pickup)
            Destroy(gameObject);
        }
        // 3. CHOQUE CON PARED (Oportunidad de recuperar)
        else if (collision.CompareTag("Wall") || collision.gameObject.layer == LayerMask.NameToLayer("Walls")) // Ajusta según tu proyecto
        {
            IntentarDejarFlecha();
            Destroy(gameObject);
        }
        // 4. CUALQUIER OTRA COSA SÓLIDA
        else if (!collision.isTrigger)
        {
            Destroy(gameObject);
        }
    }

    void IntentarDejarFlecha()
    {
        if (flechaPickupPrefab != null)
        {
            if (Random.Range(0f, 100f) <= probabilidadRecuperar)
            {
                // Instanciamos la flecha recogida en el lugar del choque
                Instantiate(flechaPickupPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}