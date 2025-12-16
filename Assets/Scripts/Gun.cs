using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] GameObject muzzle;
    [SerializeField] Transform muzzlePosition;
    [SerializeField] GameObject projectile;

    [Header("Configuración Visual")]
    [SerializeField] float distanceFromPlayer = 0.8f; // Qué tan lejos del cuerpo flota el arma
    [SerializeField] float fireRate = 0.5f;

    private float timeSinceLastShot = 0f;
    private Transform playerTransform; // Referencia al Player principal
    private SpriteRenderer gunSprite;

    private void Awake()
    {
        gunSprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Buscamos al padre (que debería ser el Player o WeaponHolder)
        // Nota: Asumimos que GunManager hace al arma hija del Player
        playerTransform = transform.parent;
        if (playerTransform == null)
        {
            // Fallback por si no tiene padre
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        timeSinceLastShot += Time.deltaTime;
        // La posición y rotación se manejan en Aim() llamado por Player.cs
    }

    // Esta función es llamada por Player.cs en cada frame
    public void Aim(Vector2 aimDirection)
    {
        if (playerTransform == null) return;

        // 1. Calcular el ángulo de rotación
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // 2. Colocar el arma en un círculo alrededor del jugador
        // Usamos la dirección normalizada multiplicada por la distancia deseada
        Vector3 orbitPosition = playerTransform.position + (Vector3)(aimDirection.normalized * distanceFromPlayer);
        transform.position = orbitPosition;

        // 3. Rotar el arma para que apunte en la dirección
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 4. Voltear el sprite si apuntamos a la izquierda (para que no quede boca abajo)
        if (gunSprite != null)
        {
            // Si el ángulo es > 90 o < -90 (lado izquierdo), volteamos Y
            bool aimLeft = Mathf.Abs(angle) > 90f;
            gunSprite.flipY = aimLeft;

            // IMPORTANTE: Si el arma tiene muzzle/hijos, flipY del sprite no los mueve.
            // Una alternativa mejor para armas complejas es escalar en Y negativo:
            // transform.localScale = new Vector3(1, aimLeft ? -1 : 1, 1);
        }
    }

    public void TryToShoot()
    {
        if (timeSinceLastShot >= fireRate)
        {
            Shoot();
            timeSinceLastShot = 0;
        }
    }

    void Shoot()
    {
        if (muzzle != null && muzzlePosition != null)
        {
            var muzzleGo = Instantiate(muzzle, muzzlePosition.position, transform.rotation);
            muzzleGo.transform.SetParent(transform);
            Destroy(muzzleGo, 0.05f);
        }

        if (projectile != null && muzzlePosition != null)
        {
            var projectileGo = Instantiate(projectile, muzzlePosition.position, transform.rotation);
            Destroy(projectileGo, 3);
        }
    }

    // Esta función ya no es estrictamente necesaria con la nueva lógica de órbita,
    // pero la dejamos por compatibilidad si GunManager la llama.
    public void SetOffset(Vector2 o) { }
}