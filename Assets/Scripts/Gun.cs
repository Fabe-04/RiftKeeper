using Unity.Mathematics;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] GameObject muzzle;
    [SerializeField] Transform muzzlePosition;
    [SerializeField] GameObject projectile;

    [Header("Config")]
    // [SerializeField] float fireDistance = 10; // Ya no es necesario
    [SerializeField] float fireRate = 0.5f;

    Transform player;
    Vector2 offset;

    private float timeSinceLastShot = 0f;
    // Transform closestEnemy; // ELIMINADO
    Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
        timeSinceLastShot = fireRate; // Inicia listo para disparar
        player = GameObject.Find("Player").transform;
    }

    private void Update()
    {
        // El arma solo sigue al jugador y actualiza su cooldown
        timeSinceLastShot += Time.deltaTime;
    }

    // --- SE ELIMINÓ FindClosestEnemy() ---
    
    // --- NUEVA FUNCIÓN PÚBLICA: Aim() ---
    // Esta función la llamará el Player.cs cada frame
    public void Aim(Vector2 aimDirection)
    {
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    // --- FIN NUEVO ---


    // --- SE ELIMINÓ Shooting() (lógica de auto-disparo) ---

    // --- NUEVA FUNCIÓN PÚBLICA: TryToShoot() ---
    // Esta función la llamará el Player.cs al hacer clic
    public void TryToShoot()
    {
        if (timeSinceLastShot >= fireRate)
        {
            Shoot();
            timeSinceLastShot = 0;
        }
    }
    // --- FIN NUEVO ---

    void Shoot()
    {
        var muzzleGo = Instantiate(muzzle, muzzlePosition.position, transform.rotation);
        muzzleGo.transform.SetParent(transform);
        Destroy(muzzleGo, 0.05f);

        var projectileGo = Instantiate(projectile, muzzlePosition.position, transform.rotation);
        
        // --- ¡¡¡BUG CORREGIDO!!! ---
        // Se destruye la instancia (projectileGo), no el prefab (projectile)
        Destroy(projectileGo, 3);
        // --- FIN CORRECCIÓN ---
    }
    
    public void SetOffset(Vector2 o)
    {
        offset = o;
    }
}