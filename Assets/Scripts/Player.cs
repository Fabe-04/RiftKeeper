using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Interfaz (UI)")]
    public Slider barraDeVida;

    [Header("Visuales")]
    [SerializeField] SpriteRenderer mySprite; // <--- NUEVO: Para girar solo el dibujo

    Animator anim;
    Rigidbody2D rb;

    float moveSpeed = 12;
    public int maxHealth = 100;
    int currentHealth;

    bool dead = false;

    private InputSystem_Actions playerControls;
    private Vector2 movement;

    // --- Variables de Apuntado ---
    private Camera mainCamera;
    private Vector2 aimInput;

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
        playerControls.Player.Attack.performed += HandleShoot;
    }

    private void OnDisable()
    {
        playerControls.Player.Disable();
        playerControls.Player.Attack.performed -= HandleShoot;
    }

    private void Start()
    {
        currentHealth = maxHealth;

        if (barraDeVida != null)
        {
            barraDeVida.maxValue = maxHealth;
            barraDeVida.value = currentHealth;
        }
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

        HandleAiming();
        HandleMovementAnimation();
    }

    private void HandleMovementAnimation()
    {
        anim.SetFloat("velocity", movement.magnitude);
    }

    // --- FUNCIÓN ARREGLADA ---
    private void HandleAiming()
    {
        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(aimInput);
        Vector2 aimDirection = (mouseWorldPosition - (Vector2)transform.position).normalized;

        // --- CAMBIO CLAVE: Usamos flipX en vez de Scale ---
        if (mySprite != null)
        {
            // Si la X es negativa (miramos a la izquierda), activamos el FlipX
            // Si es positiva, lo desactivamos
            mySprite.flipX = (aimDirection.x < 0);
        }
        // -------------------------------------------------

        if (GunManager.Instance != null)
        {
            foreach (Gun gun in GunManager.Instance.activeGuns)
            {
                gun.Aim(aimDirection);
            }
        }
    }
    // -------------------------

    private void HandleShoot(InputAction.CallbackContext context)
    {
        if (dead) return;

        if (GunManager.Instance != null)
        {
            foreach (Gun gun in GunManager.Instance.activeGuns)
            {
                gun.TryToShoot();
            }
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movement * moveSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null)
            Hit(20);
    }

    void Hit(int damage)
    {
        anim.SetTrigger("hit");
        currentHealth -= damage;

        if (barraDeVida != null)
        {
            barraDeVida.value = currentHealth;
        }

        if (currentHealth <= 0)
            Die();
    }

    public void Curar(int cantidad)
    {
        if (dead) return;

        currentHealth += cantidad;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        if (barraDeVida != null)
        {
            barraDeVida.value = currentHealth;
        }

        Debug.Log("❤️ Curado! Vida actual: " + currentHealth);
    }

    void Die()
    {
        dead = true;
        GameManager.instance.GameOver();
    }
}