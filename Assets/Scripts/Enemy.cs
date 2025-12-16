using UnityEngine;
using System.Collections;
using System;

public class Enemy : MonoBehaviour
{
    [Header("Loot Configuration")]
    public LootManager.EnemyType enemyType;

    [Header("Stats")]
    [SerializeField] int maxHealth = 100;
    [SerializeField] float baseSpeed = 3.5f;

    // --- CAMBIO IMPORTANTE: PUBLIC para que el Hitbox hijo pueda leerlo ---
    public int damageToPlayer = 10;
    // ---------------------------------------------------------------------

    [Header("Charger (Elite)")]
    [SerializeField] bool isCharger;
    [SerializeField] float chargerSlowModifier = 0.6f;
    [SerializeField] float distanceToEngageCharge = 6f;
    [SerializeField] float chargeSpeed = 10f;
    [SerializeField] float chargePrepareTime = 1.0f;
    [SerializeField] float chargeDistance = 8f;

    public Action<int, int> OnHealthChanged;
    public Action OnDeath;

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
    private float enemyScale = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        currentHealth = maxHealth;

        if (enemyType == LootManager.EnemyType.Charger)
        {
            enemyScale = 1.5f;
            rb.mass = 50f;
            currentMoveSpeed = isCharger ? baseSpeed * chargerSlowModifier : baseSpeed;
        }
        else
        {
            enemyScale = 1f;
            rb.mass = 10f;
            currentMoveSpeed = baseSpeed;
        }

        target = GameObject.FindWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (GameManager.instance != null && !GameManager.instance.IsGameRunning()) return;

        if (target == null || isPreparingCharge || isCharging || currentHealth <= 0)
            return;

        var playerToTheRight = target.position.x > transform.position.x;
        transform.localScale = new Vector3(playerToTheRight ? -enemyScale : enemyScale, enemyScale, 1);

        if (isCharger && Vector2.Distance(transform.position, target.position) < distanceToEngageCharge)
        {
            if (chargeCoroutine == null) chargeCoroutine = StartCoroutine(ChargeAttack());
        }
    }

    private void FixedUpdate()
    {
        bool gameActive = (GameManager.instance != null && GameManager.instance.IsGameRunning());
        bool canMove = target != null && gameActive && !isPreparingCharge && !isCharging && currentHealth > 0;

        if (canMove)
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

        if (target != null)
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

        if (isCharging) StopCharge();
    }

    // --- LÓGICA DE COLISIÓN LIMPIA (Solo Paredes) ---
    // Hemos eliminado la lógica de daño al Player aquí porque
    // ahora lo maneja el script 'EnemyHitbox' en el objeto hijo.

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCharging && collision.gameObject.CompareTag("Wall")) StopCharge();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isCharging && collision.gameObject.CompareTag("Wall")) StopCharge();
    }
    // ------------------------------------------------

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

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (isCharging || isPreparingCharge)
        {
            if (chargeCoroutine != null) StopCoroutine(chargeCoroutine);
            StopCharge();
        }

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;

        OnDeath?.Invoke();

        if (GameManager.instance != null)
            GameManager.instance.RegistrarKill();

        if (LootManager.Instance != null)
            LootManager.Instance.SpawnLoot(transform.position, enemyType);

        Destroy(gameObject);
    }
}