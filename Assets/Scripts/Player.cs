using TMPro;
using UnityEngine;
using UnityEngine.InputSystem; 

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

    // --- Variables de Apuntado ---
    private Camera mainCamera;
    private Vector2 aimInput;

    int facingDirection = 1;

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

        // Leer la POSICIÓN del mouse
        aimInput = Mouse.current.position.ReadValue();

        HandleAiming();
        HandleMovementAnimation();
    }

    private void HandleMovementAnimation()
    {
        anim.SetFloat("velocity", movement.magnitude);
    }

    private void HandleAiming()
    {
        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(aimInput);
        Vector2 aimDirection = (mouseWorldPosition - (Vector2)transform.position).normalized;

        if (aimDirection.x != 0)
            facingDirection = aimDirection.x > 0 ? 1 : -1;
        transform.localScale = new Vector2(facingDirection, 1);

        if (GunManager.Instance != null) 
        {
            foreach (Gun gun in GunManager.Instance.activeGuns)
            {
                gun.Aim(aimDirection);
            }
        }
    }

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
        healthtext.text = Mathf.Clamp(currentHealth, 0, maxHealth).ToString();

        if (currentHealth <= 0)
            Die();
    }

    // --- NUEVA FUNCIÓN: CURAR ---
    // Esta función la llamará la poción (ItemLoot.cs)
    public void Curar(int cantidad)
    {
        if (dead) return; // No curar a los muertos

        currentHealth += cantidad;

        // Evitar que la vida suba más de 100
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        // Actualizar el texto de la pantalla
        healthtext.text = currentHealth.ToString();

        Debug.Log("❤️ Curado! Vida actual: " + currentHealth);
    }
    // --- FIN NUEVA FUNCIÓN ---

    void Die()
    {
        dead = true;
        GameManager.instance.GameOver();
    }
}