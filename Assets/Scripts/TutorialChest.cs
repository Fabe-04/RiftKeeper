using UnityEngine;
using UnityEngine.InputSystem; // Asegúrate de tener el Input System
using UnityEngine.UI;

public class TutorialChest : MonoBehaviour
{
    [Header("Referencias Internas")]
    [Tooltip("El Canvas/Panel que contiene el pergamino y el botón cerrar")]
    [SerializeField] GameObject uiPergamino;

    [Tooltip("El Canvas WorldSpace con el texto '[E] Abrir'")]
    [SerializeField] GameObject textoInteractuar;

    [Tooltip("Sprite del cofre (para cambiarlo a abierto)")]
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite spriteCofreAbierto;

    private bool jugadorCerca = false;
    private bool yaAbierto = false;

    private void Start()
    {
        // Al nacer, nos aseguramos que la UI esté oculta
        if (uiPergamino != null) uiPergamino.SetActive(false);
        if (textoInteractuar != null) textoInteractuar.SetActive(false);
    }

    private void Update()
    {
        // Detectar tecla E (Nuevo Input System)
        if (jugadorCerca && !yaAbierto)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                AbrirCofre();
            }
        }
    }

    void AbrirCofre()
    {
        yaAbierto = true;

        // 1. Cambiar sprite visual
        if (spriteRenderer != null && spriteCofreAbierto != null)
            spriteRenderer.sprite = spriteCofreAbierto;

        // 2. Ocultar el aviso "[E]"
        if (textoInteractuar != null) textoInteractuar.SetActive(false);

        // 3. Abrir la UI del Pergamino
        if (uiPergamino != null)
        {
            uiPergamino.SetActive(true);
            Time.timeScale = 0f; // PAUSAR EL JUEGO
        }
    }

    // --- ESTA FUNCIÓN SE LLAMA DESDE EL BOTÓN 'CERRAR' (ON CLICK) ---
    public void CerrarPergamino()
    {
        if (uiPergamino != null) uiPergamino.SetActive(false);
        Time.timeScale = 1f; // REANUDAR EL JUEGO

        // Opcional: Destruir el script para que no se pueda interactuar más
        // Destroy(this); 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !yaAbierto)
        {
            jugadorCerca = true;
            if (textoInteractuar != null) textoInteractuar.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorCerca = false;
            if (textoInteractuar != null) textoInteractuar.SetActive(false);
        }
    }
}