using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Loot Configuration")]
    public LootManager.EnemyType enemyType;

    [Header("Stats")]
    [SerializeField] int maxHealth = 100;
    [SerializeField] float baseSpeed = 2f;

    [Header("Charger (Elite)")]
    [SerializeField] bool isCharger;
    [SerializeField] float chargerSlowModifier = 0.75f;
    [SerializeField] float distanceToEngageCharge = 5f;
    [SerializeField] float chargeSpeed = 12f;
    [SerializeField] float chargePrepareTime = 0.8f;
    [SerializeField] float chargeDistance = 7f;

    // --- Variables de estado ---
    private bool isPreparingCharge = false;
    private bool isCharging = false;
    private float currentMoveSpeed;
    private Vector2 chargeTargetDirection;
    private Vector2 chargeStartPosition;

    private int currentHealth;
    private Transform target;

    private Rigidbody2D rb;
    private Animator anim;
    private Coroutine chargeCoroutine;

    // --- NUEVO: Variable para guardar el tamaño ---
    private float enemyScale = 1f;
    // ---------------------------------------------

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        currentHealth = maxHealth;

        // --- NUEVO: Configurar Tamaño y Masa según el tipo ---
        if (enemyType == LootManager.EnemyType.Charger)
        {
            // EL CHARGER SERÁ MÁS GRANDE Y PESADO
            enemyScale = 1.5f; // 30% más grande (puedes poner 1.5f si quieres más)
            rb.mass = 50f;     // Más pesado para empujar a los pequeños

            // Ajustamos velocidad si es charger
            if (isCharger) currentMoveSpeed = baseSpeed * chargerSlowModifier;
            else currentMoveSpeed = baseSpeed;
        }
        else
        {
            // ENEMIGO NORMAL
            enemyScale = 1f;
            rb.mass = 10f; // Peso normal
            currentMoveSpeed = baseSpeed;
        }
        // -----------------------------------------------------

        target = GameObject.FindWithTag("Player")?.transform;

        if (EnemyManager.Instance != null)
            EnemyManager.Instance.RegisterEnemy(this);
    }

    private void Update()
    {
        if (target == null || (WaveManager.Instance != null && !WaveManager.Instance.WaveRuuning()) || isPreparingCharge || isCharging || currentHealth <= 0)
        {
            return;
        }

        // --- Lógica de voltear (facing) ARREGLADA ---
        var playerToTheRight = target.position.x > transform.position.x;

        // EN LUGAR DE USAR 1, USAMOS LA VARIABLE 'enemyScale'
        // Si mira a la derecha es -scale, si mira a la izquierda es scale (o viceversa según tu sprite)
        transform.localScale = new Vector3(playerToTheRight ? -enemyScale : enemyScale, enemyScale, 1);
        // --------------------------------------------

        if (isCharger && Vector2.Distance(transform.position, target.position) < distanceToEngageCharge)
        {
            if (chargeCoroutine == null)
            {
                chargeCoroutine = StartCoroutine(ChargeAttack());
            }
        }
    }

    private void FixedUpdate()
    {
        bool canMoveNormally = target != null && (WaveManager.Instance != null && WaveManager.Instance.WaveRuuning()) && !isPreparingCharge && !isCharging && currentHealth > 0;

        if (canMoveNormally)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            rb.linearVelocity = direction * currentMoveSpeed;
        }
        else if (!isCharging)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private IEnumerator ChargeAttack()
    {
        isPreparingCharge = true;
        rb.linearVelocity = Vector2.zero;

        anim?.SetTrigger("PrepareCharge");

        chargeTargetDirection = (target.position - transform.position).normalized;

        yield return new WaitForSeconds(chargePrepareTime);

        isPreparingCharge = false;
        isCharging = true;
        chargeStartPosition = rb.position;

        rb.linearVelocity = chargeTargetDirection * chargeSpeed;

        anim?.SetTrigger("Charge");

        float distanceTraveled = 0f;
        while (isCharging && distanceTraveled < chargeDistance)
        {
            distanceTraveled = Vector2.Distance(chargeStartPosition, rb.position);
            yield return null;
        }

        if (isCharging)
        {
            StopCharge();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCharging && collision.gameObject.CompareTag("Wall"))
        {
            StopCharge();
        }
    }

    private void StopCharge()
    {
        if (!isCharging) return;

        isCharging = false;
        rb.linearVelocity = Vector2.zero;
        currentMoveSpeed = baseSpeed * chargerSlowModifier;
        anim?.SetTrigger("Idle");

        chargeCoroutine = null;
    }

    public void Hit(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        anim?.SetTrigger("hit");

        if (isCharging || isPreparingCharge)
        {
            if (chargeCoroutine != null) StopCoroutine(chargeCoroutine);
            StopCharge();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;
        isCharging = false;
        isPreparingCharge = false;

        if (EnemyManager.Instance != null)
            EnemyManager.Instance.UnregisterEnemy(this);

        if (LootManager.Instance != null)
        {
            LootManager.Instance.SpawnLoot(transform.position, enemyType);
        }

        Destroy(gameObject);
    }
}