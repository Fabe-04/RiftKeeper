using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShopNPC : MonoBehaviour
{
    [SerializeField] GameObject cartelInteraccion; // El canvas flotante "Presiona E"
    private bool playerInRange;
    private Player playerRef;

    private void Start()
    {
        if (cartelInteraccion != null) cartelInteraccion.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (ShopUIManager.Instance != null)
            {
                ShopUIManager.Instance.OpenShop(playerRef);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            playerRef = collision.GetComponent<Player>();
            if (cartelInteraccion != null) cartelInteraccion.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (cartelInteraccion != null) cartelInteraccion.SetActive(false);

            // Cerrar tienda si te alejas
            if (ShopUIManager.Instance != null) ShopUIManager.Instance.CloseShop();
        }
    }
}