using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Loot Configuration")]
    // 1. AQUI ELEGIRAS SI ES BASICO O CHARGER EN EL INSPECTOR
    public LootManager.EnemyType enemyType;

    [Header("Stats")]
    [SerializeField] int maxHealth = 100;
    [SerializeField] float baseSpeed = 2f; // Velocidad base para enemigos normales

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
    private Transform target; // El jugador

    private Rigidbody2D rb;
    private Animator anim;
    private Coroutine chargeCoroutine;

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
            currentMoveSpeed = baseSpeed * chargerSlowModifier;
        }
        else
        {
            currentMoveSpeed = baseSpeed;
        }

        target = GameObject.FindWithTag("Player")?.transform;

        // Verificamos si EnemyManager existe antes de registrar (por seguridad)
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.RegisterEnemy(this);
    }

    private void Update()
    {
        // Condiciones para detener la lógica
        if (target == null || (WaveManager.Instance != null && !WaveManager.Instance.WaveRuuning()) || isPreparingCharge || isCharging || currentHealth <= 0)
        {
            return;
        }

        // Lógica de voltear (facing)
        var playerToTheRight = target.position.x > transform.position.x;
        transform.localScale = new Vector2(playerToTheRight ? -1 : 1, 1);

        // Comprobar si un Charger debe iniciar la preparación de carga
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

    // --- CORUTINA DE EMBESTIDA ---
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

        // --- 2. AQUI ESTA LA MAGIA DEL LOOT ---
        // Justo antes de destruir, le decimos al manager que suelte el premio
        if (LootManager.Instance != null)
        {
            LootManager.Instance.SpawnLoot(transform.position, enemyType);
        }
        // --------------------------------------

        Destroy(gameObject);
    }
}