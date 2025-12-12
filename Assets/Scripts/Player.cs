using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Interfaz (UI)")]
    public Slider barraDeVida;
    public Slider barraStamina;          // <--- Barra de cansancio
    public TextMeshProUGUI textoFlechas; // <--- Contador de flechas

    [Header("Recursos")]
    public int maxFlechas = 10;
    public int flechasActuales;
    public float maxStamina = 100;
    public float currentStamina;

    [Header("Configuración Movimiento")]
    [SerializeField] float walkSpeed = 8f;
    [SerializeField] float runSpeed = 14f;
    float moveSpeed;

    // Variables internas
    Animator anim;
    Rigidbody2D rb;
    int currentHealth;
    public int maxHealth = 100;
    bool dead = false;

    private InputSystem_Actions playerControls;
    private Vector2 movement;
    private Camera mainCamera;
    private Vector2 aimInput;

    [SerializeField] SpriteRenderer mySprite;

    // Variable para saber si corremos
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

        // Disparar con clic izquierdo
        playerControls.Player.Attack.performed += HandleAttack;

        // Detectar tecla Shift para correr
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
        flechasActuales = 20;

        // --- CORRECCIÓN VITAL ---
        // Configuramos los límites de las barras al iniciar
        if (barraDeVida != null)
        {
            barraDeVida.maxValue = maxHealth; // La barra vale 100
            barraDeVida.value = currentHealth;
        }

        if (barraStamina != null)
        {
            barraStamina.maxValue = maxStamina; // La barra vale 100
            barraStamina.value = currentStamina;
        }
        // ------------------------

        UpdateUI();
    }

    private void Update()
    {
        if (dead)
        {
            movement = Vector2.zero;
            anim.SetFloat("velocity", 0);
            return;
        }

        movement = playerControls.Player.Move.ReadValue<Vector2>();
        aimInput = Mouse.current.position.ReadValue();

        HandleStamina(); // Gestionar el correr
        HandleAiming();
        HandleMovementAnimation();
    }

    // --- 1. SISTEMA DE CORRER (STAMINA) ---
    void HandleStamina()
    {
        // Si nos movemos Y pulsamos Shift Y tenemos energía
        if (movement.magnitude > 0 && runHeld && currentStamina > 0)
        {
            moveSpeed = runSpeed;
            currentStamina -= 30f * Time.deltaTime; // Gastamos energía
        }
        else
        {
            moveSpeed = walkSpeed;
            // Recuperamos energía si no corremos
            if (currentStamina < maxStamina)
                currentStamina += 15f * Time.deltaTime;
        }

        // Actualizamos la barra azul/amarilla
        if (barraStamina != null)
            barraStamina.value = currentStamina;
    }

    // --- 2. SISTEMA DE DISPARO (FLECHAS) ---
    private void HandleAttack(InputAction.CallbackContext context)
    {
        if (dead) return;

        if (GunManager.Instance != null)
        {
            if (flechasActuales > 0)
            {
                // Disparamos
                foreach (Gun gun in GunManager.Instance.activeGuns)
                {
                    gun.TryToShoot();
                }

                flechasActuales--; // Restamos una flecha
                UpdateUI();        // Actualizamos el texto
            }
            else
            {
                Debug.Log("🚫 ¡Clic! Sin flechas.");
            }
        }
    }

    // --- 3. RECOGER FLECHAS ---
    public void RecogerFlecha(int cantidad)
    {
        flechasActuales += cantidad;
        // No pasarnos del máximo
        if (flechasActuales > maxFlechas) flechasActuales = maxFlechas;

        UpdateUI();
        Debug.Log("Flecha recuperada!");
    }

    // --- FUNCIONES VISUALES ---
    void UpdateUI()
    {
        if (barraDeVida != null)
            barraDeVida.value = currentHealth;

        if (textoFlechas != null)
            textoFlechas.text = "Flechas: " + flechasActuales + "/" + maxFlechas;
    }

    private void HandleMovementAnimation()
    {
        anim.SetFloat("velocity", movement.magnitude);
    }

    private void HandleAiming()
    {
        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(aimInput);
        Vector2 aimDirection = (mouseWorldPosition - (Vector2)transform.position).normalized;

        if (mySprite != null)
            mySprite.flipX = (aimDirection.x < 0);

        if (GunManager.Instance != null)
        {
            foreach (Gun gun in GunManager.Instance.activeGuns)
                gun.Aim(aimDirection);
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movement * moveSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null) Hit(20);
    }

    void Hit(int damage)
    {
        anim.SetTrigger("hit");
        currentHealth -= damage;
        UpdateUI();

        if (currentHealth <= 0)
            Die();
    }

    public void Curar(int cantidad)
    {
        currentHealth += cantidad;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateUI();
    }

    void Die()
    {
        dead = true;
        GameManager.instance.GameOver();
    }
}