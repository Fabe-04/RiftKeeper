using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] int maxHealth = 100;
    [SerializeField] float baseSpeed = 2f; // Velocidad base para enemigos normales

    [Header("Charger (Elite)")]
    [SerializeField] bool isCharger;
    [SerializeField] float chargerSlowModifier = 0.75f; // Qué tan más lento es el charger (ej: 0.75 = 75% de la velocidad base)
    [SerializeField] float distanceToEngageCharge = 5f; // Distancia a la que empieza a preparar la carga
    [SerializeField] float chargeSpeed = 12f;          // Velocidad durante la embestida
    [SerializeField] float chargePrepareTime = 0.8f;     // Tiempo de la animación de "aviso"
    [SerializeField] float chargeDistance = 7f;          // Distancia MÁXIMA que recorrerá la embestida

    // --- Variables de estado ---
    private bool isPreparingCharge = false;
    private bool isCharging = false;
    private float currentMoveSpeed;         // Velocidad actual (puede cambiar si es charger)
    private Vector2 chargeTargetDirection;  // Hacia dónde embestirá
    private Vector2 chargeStartPosition;    // Dónde empezó la embestida

    private int currentHealth;
    private Transform target; // El jugador

    private Rigidbody2D rb;
    private Animator anim;
    private Coroutine chargeCoroutine; // Para poder detener la embestida si choca

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        currentHealth = maxHealth;

        // --- AJUSTE DE VELOCIDAD INICIAL ---
        if (isCharger)
        {
            currentMoveSpeed = baseSpeed * chargerSlowModifier; // Charger es más lento normalmente
        }
        else
        {
            currentMoveSpeed = baseSpeed;
        }
        // --- FIN AJUSTE ---

        target = GameObject.FindWithTag("Player")?.transform; // Busca al jugador por Tag (más seguro)

        EnemyManager.Instance.RegisterEnemy(this);
    }

    private void Update()
    {
        // Condiciones para detener la lógica
        if (target == null || !WaveManager.Instance.WaveRuuning() || isPreparingCharge || isCharging || currentHealth <= 0)
        {
            return; // No hacer nada si está cargando, preparando, muerto o no hay objetivo/oleada
        }

        // Lógica de voltear (facing)
        var playerToTheRight = target.position.x > transform.position.x;
        transform.localScale = new Vector2(playerToTheRight ? -1 : 1, 1);

        // Comprobar si un Charger debe iniciar la preparación de carga
        if (isCharger && Vector2.Distance(transform.position, target.position) < distanceToEngageCharge)
        {
            // Solo iniciar si no está ya en proceso
            if (chargeCoroutine == null)
            {
                chargeCoroutine = StartCoroutine(ChargeAttack());
            }
        }
    }

    private void FixedUpdate()
    {
        // El movimiento normal (perseguir) solo ocurre si NO es charger o si es charger pero NO está preparando/cargando
        bool canMoveNormally = target != null && WaveManager.Instance.WaveRuuning() && !isPreparingCharge && !isCharging && currentHealth > 0;

        if (canMoveNormally)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            rb.linearVelocity = direction * currentMoveSpeed; // Usa la velocidad ajustada
        }
        else if (!isCharging) // Si no se puede mover normalmente Y NO está cargando, detenerse
        {
            rb.linearVelocity = Vector2.zero;
        }
        // Si está cargando (isCharging == true), la corrutina controla la velocidad.
    }

    // --- CORUTINA DE EMBESTIDA MEJORADA ---
    private IEnumerator ChargeAttack()
    {
        isPreparingCharge = true;
        rb.linearVelocity = Vector2.zero; // Detenerse para preparar

        // --- PASO 1: El "Aviso" (Tell) ---
        anim?.SetTrigger("PrepareCharge"); // Disparador para la animación de aviso
        Debug.Log("Charger preparando embestida!");

        // Guardamos la DIRECCIÓN hacia el jugador en este instante
        chargeTargetDirection = (target.position - transform.position).normalized;
        // La posición la guardaremos al empezar a embestir

        yield return new WaitForSeconds(chargePrepareTime);

        // --- PASO 2: La Embestida ---
        isPreparingCharge = false;
        isCharging = true;
        chargeStartPosition = rb.position; // Guardamos dónde empezamos a embestir

        // Aplicamos la velocidad de embestida en la dirección guardada
        rb.linearVelocity = chargeTargetDirection * chargeSpeed;

        anim?.SetTrigger("Charge"); // Disparador para la animación de embestida
        Debug.Log("¡¡CHARGER EMBISTIENDO!!");

        // --- NUEVO: Bucle de Distancia ---
        // Espera hasta recorrer la distancia O chocar (ver OnCollisionEnter2D)
        float distanceTraveled = 0f;
        while (isCharging && distanceTraveled < chargeDistance)
        {
            distanceTraveled = Vector2.Distance(chargeStartPosition, rb.position);
            yield return null; // Espera al siguiente frame
        }
        // --- FIN NUEVO ---

        // Si el bucle terminó por distancia (y no por colisión), detenemos manualmente
        if (isCharging)
        {
            StopCharge();
        }
    }
    // --- FIN CORUTINA MEJORADA ---

    // --- NUEVO: Detectar Colisión DURANTE la embestida ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si estamos embistiendo Y chocamos con un muro...
        if (isCharging && collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Charger chocó con muro!");
            StopCharge(); // Detenemos la embestida
        }
        // Podrías añadir lógica si choca con el Player durante la carga aquí
    }
    // --- FIN NUEVO ---


    // --- NUEVA FUNCIÓN: Para detener limpiamente la embestida ---
    private void StopCharge()
    {
        if (!isCharging) return; // Evitar llamadas múltiples

        isCharging = false;
        rb.linearVelocity = Vector2.zero; // Detenerse
        currentMoveSpeed = baseSpeed * chargerSlowModifier; // Volver a velocidad lenta
        anim?.SetTrigger("Idle"); // Volver a animación normal
        Debug.Log("Embestida detenida.");

        // Limpiar la referencia a la corutina para poder cargar de nuevo
        chargeCoroutine = null;

        // Podrías añadir un cooldown aquí antes de que pueda volver a cargar
        // StartCoroutine(ChargeCooldown(2.0f));
    }
    // --- FIN NUEVO ---

    public void Hit(int damage)
    {
        if (currentHealth <= 0) return; // Evitar daño múltiple si ya está muerto

        currentHealth -= damage;
        anim?.SetTrigger("hit");

        // Detener la carga si le disparan mientras carga (opcional pero bueno)
        if (isCharging || isPreparingCharge)
        {
            StopCoroutine(chargeCoroutine); // Detiene la corutina ChargeAttack
            StopCharge();                   // Llama a la limpieza
        }


        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Detener cualquier movimiento o carga pendiente
        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;
        isCharging = false;
        isPreparingCharge = false;

        EnemyManager.Instance.UnregisterEnemy(this);

        // Podrías activar una animación de muerte aquí antes de destruir
        // anim?.SetTrigger("Die");
        // yield return new WaitForSeconds(deathAnimationTime);

        Destroy(gameObject);
    }
}