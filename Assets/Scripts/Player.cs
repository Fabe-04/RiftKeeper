using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Combate & Inventario")]
    public bool hasBow = false;
    public bool isUsingBow = false;

    [Header("Defensa")]
    [SerializeField] float tiempoInvulnerabilidad = 1.0f;
    private float ultimoGolpeTime;

    [Header("Referencias de Combate")]
    public SwordAttack swordScript;
    // --- NUEVO: Referencia al objeto visual para poder ocultarlo ---
    public GameObject objetoEspadaVisual;
    // --------------------------------------------------------------

    [Header("Recursos")]
    public int maxFlechas = 10;
    public int flechasActuales;
    public float maxStamina = 100;
    public float currentStamina;

    [Header("Movimiento")]
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float runSpeed = 8.5f;
    float moveSpeed;

    Animator anim;
    Rigidbody2D rb;

    public int currentHealth;
    public int maxHealth = 100;
    bool dead = false;

    private InputSystem_Actions playerControls;
    private Vector2 movement;
    private Camera mainCamera;
    private Vector2 aimInput;
    [SerializeField] SpriteRenderer mySprite;
    private bool runHeld = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        playerControls = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        playerControls.Player.Enable();
        playerControls.Player.Attack.performed += HandleAttack;
        // Input 'Next' (Tecla Q) configurado en el Input System
        playerControls.Player.Next.performed += ctx => SwapWeapon();
        playerControls.Player.Sprint.performed += ctx => runHeld = true;
        playerControls.Player.Sprint.canceled += ctx => runHeld = false;
    }

    private void OnDisable()
    {
        playerControls.Player.Disable();
        playerControls.Player.Attack.performed -= HandleAttack;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        flechasActuales = 5;
        ultimoGolpeTime = -tiempoInvulnerabilidad;

        // 1. Sincronizar UI Inicial
        UpdateUI();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.MostrarArcoEnHUD(hasBow);
            if (GameManager.instance != null)
                UIManager.Instance.ActualizarMonedas(GameManager.instance.currentCoins);
        }

        // 2. Estado visual inicial de las armas
        ToggleGunVisuals(false); // Apagar arco al inicio

        if (objetoEspadaVisual != null)
            objetoEspadaVisual.SetActive(true); // Asegurar que la espada se ve al iniciar
    }

    private void Update()
    {
        if (dead) { StopMotion(); return; }

        movement = playerControls.Player.Move.ReadValue<Vector2>();
        aimInput = Mouse.current.position.ReadValue();

        HandleStamina();
        HandleAiming();
        HandleMovementAnimation();
    }

    // --- LÓGICA DE ARMAS (AUDITADA) ---
    private void SwapWeapon()
    {
        if (!hasBow) return; // Seguridad: si no tengo arco, no hago nada

        isUsingBow = !isUsingBow;

        // 1. Gestionar Arco (GunManager)
        ToggleGunVisuals(isUsingBow);

        // 2. Gestionar Espada (Visual)
        // Si uso arco (true) -> Espada apagada (false)
        // Si no uso arco (false) -> Espada encendida (true)
        if (objetoEspadaVisual != null)
            objetoEspadaVisual.SetActive(!isUsingBow);

        // 3. Gestionar UI
        if (UIManager.Instance != null)
            UIManager.Instance.CambiarArmaVisual(isUsingBow);

        UpdateUI();
    }

    void ToggleGunVisuals(bool active)
    {
        if (GunManager.Instance != null)
        {
            foreach (Gun gun in GunManager.Instance.activeGuns)
                gun.gameObject.SetActive(active);
        }
    }

    // --- COMBATE ---
    private void HandleAttack(InputAction.CallbackContext context)
    {
        if (dead) return;

        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(aimInput);
        Vector2 aimDirection = (mouseWorldPosition - (Vector2)transform.position).normalized;

        if (isUsingBow && hasBow)
        {
            if (GunManager.Instance != null)
            {
                if (flechasActuales > 0)
                {
                    foreach (Gun gun in GunManager.Instance.activeGuns) gun.TryToShoot();
                    flechasActuales--;
                    UpdateUI();
                }
            }
        }
        else
        {
            if (swordScript != null)
            {
                swordScript.Attack(aimDirection);
                anim.SetTrigger("attackMelee");
            }
        }
    }

    // --- RESTO DE SISTEMAS (Sin Cambios) ---
    public void UpgradeMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        UpdateUI();
    }

    public void UpgradeMaxStamina(float amount)
    {
        maxStamina += amount;
        currentStamina = maxStamina;
        UpdateUI();
    }

    public void UpgradeSwordDamage(int amount)
    {
        if (swordScript != null) swordScript.UpgradeDamage(amount);
    }

    void HandleStamina()
    {
        if (movement.magnitude > 0 && runHeld && currentStamina > 0)
        {
            moveSpeed = runSpeed;
            currentStamina -= 25f * Time.deltaTime;
        }
        else
        {
            moveSpeed = walkSpeed;
            if (currentStamina < maxStamina) currentStamina += 15f * Time.deltaTime;
        }

        if (UIManager.Instance != null)
            UIManager.Instance.ActualizarStamina(currentStamina, maxStamina);
    }

    private void HandleAiming()
    {
        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(aimInput);
        Vector2 aimDirection = (mouseWorldPosition - (Vector2)transform.position).normalized;

        if (mySprite != null) mySprite.flipX = (aimDirection.x < 0);

        if (isUsingBow && GunManager.Instance != null)
        {
            foreach (Gun gun in GunManager.Instance.activeGuns)
                gun.Aim(aimDirection);
        }
    }

    private void HandleMovementAnimation() => anim.SetFloat("velocity", movement.magnitude);
    private void FixedUpdate() => rb.linearVelocity = movement * moveSpeed;

    public void TakeDamage(int damage)
    {
        if (Time.time - ultimoGolpeTime < tiempoInvulnerabilidad) return;

        ultimoGolpeTime = Time.time;
        currentHealth -= damage;
        anim.SetTrigger("hit");
        UpdateUI();

        if (currentHealth <= 0) Die();
    }

    void Hit(int damage) => TakeDamage(damage);

    public void Curar(int cantidad)
    {
        currentHealth += cantidad;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateUI();
    }

    public void RecogerFlecha(int cantidad)
    {
        flechasActuales += cantidad;
        if (flechasActuales > maxFlechas) flechasActuales = maxFlechas;
        UpdateUI();
    }

    void Die()
    {
        dead = true;
        GameManager.instance.GameOver();
    }

    void StopMotion()
    {
        movement = Vector2.zero;
        anim.SetFloat("velocity", 0);
        rb.linearVelocity = Vector2.zero;
    }

    void UpdateUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ActualizarVida(currentHealth, maxHealth);
            UIManager.Instance.ActualizarStamina(currentStamina, maxStamina);
            UIManager.Instance.ActualizarFlechas(flechasActuales);
            UIManager.Instance.CambiarArmaVisual(isUsingBow);
        }
    }
}