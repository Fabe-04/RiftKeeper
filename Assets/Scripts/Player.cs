using TMPro;
using UnityEngine;
using UnityEngine.InputSystem; // Importante

public class Player : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI healthtext;

    Animator anim;
    Rigidbody2D rb;

    float moveSpeed = 12;
    int maxHealth = 100;
    int currentHealth;

    bool dead = false;

    private InputSystem_Actions playerControls;
    private Vector2 movement;

    // --- NUEVO: Variables de Apuntado ---
    private Camera mainCamera;
    private Vector2 aimInput;
    // --- FIN NUEVO ---

    int facingDirection = 1;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // --- NUEVO: Obtener la cámara principal ---
        mainCamera = Camera.main;
        // --- FIN NUEVO ---

        playerControls = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        playerControls.Player.Enable();

        // --- NUEVO: Registrar la acción de "Attack" ---
        // Cuando se presiona "Attack", se llama a la función HandleShoot
        playerControls.Player.Attack.performed += HandleShoot;
        // --- FIN NUEVO ---
    }

    private void OnDisable()
    {
        playerControls.Player.Disable();

        // --- NUEVO: De-registrar la acción ---
        playerControls.Player.Attack.performed -= HandleShoot;
        // --- FIN NUEVO ---
    }


    private void Start()
    {
        currentHealth = maxHealth;
        healthtext.text = maxHealth.ToString();
    }

    private void Update()
    {
        if (dead)
        {
            movement = Vector2.zero;
            anim.SetFloat("velocity", 0);
            return;
        }

        // --- LÓGICA DE INPUT ---
        movement = playerControls.Player.Move.ReadValue<Vector2>();

        // --- NUEVO: Leer la POSICIÓN del mouse (no el delta) ---
        // Usamos la acción "Look" pero leemos el input del mouse directamente
        aimInput = Mouse.current.position.ReadValue();
        // --- FIN NUEVO ---


        // --- NUEVO: Llamar a las funciones de lógica ---
        HandleAiming();
        HandleMovementAnimation();
        // --- FIN NUEVO ---
    }

    private void HandleMovementAnimation()
    {
        anim.SetFloat("velocity", movement.magnitude);

        // Esta lógica de "facing" debe cambiar: ahora la define el mouse, no el movimiento.
        // La eliminaremos por ahora para que el apuntado la controle.
        /*
        if (movement.x != 0)
            facingDirection = movement.x > 0 ? 1 : -1;
        transform.localScale = new Vector2(facingDirection, 1);
        */
    }

    // --- NUEVA FUNCIÓN: HandleAiming() ---
    private void HandleAiming()
    {
        // 1. Convertir la posición del mouse (Pixeles) a la posición del Mundo (Unidades de Unity)
        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(aimInput);

        // 2. Calcular la dirección desde el jugador hacia el mouse
        Vector2 aimDirection = (mouseWorldPosition - (Vector2)transform.position).normalized;

        // 3. Actualizar el "facing" del jugador basado en el mouse
        if (aimDirection.x != 0)
            facingDirection = aimDirection.x > 0 ? 1 : -1;
        transform.localScale = new Vector2(facingDirection, 1);

        // 4. Decirle a todas las armas que apunten en esa dirección
        if (GunManager.Instance != null) // Asegurarse que el Manager existe
        {
            foreach (Gun gun in GunManager.Instance.activeGuns)
            {
                gun.Aim(aimDirection);
            }
        }
    }
    // --- FIN NUEVO ---

    // --- NUEVA FUNCIÓN: HandleShoot() ---
    // Esta función es llamada por el Evento de Input "Attack"
    private void HandleShoot(InputAction.CallbackContext context)
    {
        if (dead) return; // No disparar si está muerto

        // Decirle a todas las armas que intenten disparar
        if (GunManager.Instance != null)
        {
            foreach (Gun gun in GunManager.Instance.activeGuns)
            {
                gun.TryToShoot();
            }
        }
    }
    // --- FIN NUEVO ---

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
        healthtext.text = Mathf.Clamp(currentHealth, 0, maxHealth).ToString();

        if (currentHealth <= 0)
            Die();
    }
    void Die()
    {
        dead = true;
        GameManager.instance.GameOver();
    }
}