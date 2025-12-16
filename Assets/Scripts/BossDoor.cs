using UnityEngine;
using TMPro;

public class BossDoor : MonoBehaviour
{
    [Header("Configuración")]
    public int precioEntrada = 50;

    // Eliminamos la coordenada manual. Ahora es automática.

    [Header("UI Interacción")]
    [SerializeField] GameObject cartelAviso;
    [SerializeField] TextMeshProUGUI textoPrecio;

    private bool jugadorCerca = false;

    private void Start()
    {
        if (cartelAviso != null) cartelAviso.SetActive(false);
        if (textoPrecio != null) textoPrecio.text = $"[E] DESAFIAR JEFE \n ({precioEntrada} MONEDAS)";
    }

    private void Update()
    {
        if (jugadorCerca && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
        {
            IntentarEntrar();
        }
    }

    void IntentarEntrar()
    {
        // Verificaciones de seguridad
        if (GameManager.instance == null || DungeonGenerator.Instance == null) return;

        if (GameManager.instance.SpendCoins(precioEntrada))
        {
            Debug.Log("Acceso concedido. Preparando sala del jefe...");

            // 1. OBTENER POSICIÓN REAL DEL GENERADOR
            Vector3 centroSalaJefe = DungeonGenerator.Instance.ObtenerCentroJefeWorld();

            // 2. SPAWNEAR AL JEFE AHORA (Just-in-Time)
            // Esto evita que el jefe se mueva antes de que lleguemos
            if (EnemyManager.Instance != null)
            {
                EnemyManager.Instance.SpawnBoss(centroSalaJefe);
            }

            // 3. TELETRANSPORTAR JUGADOR
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Teletransportamos al jugador 3 unidades MÁS ABAJO del centro
                // para que no nazca fusionado con el jefe.
                Vector3 posicionEntrada = centroSalaJefe + new Vector3(0, -10f, 0);

                player.transform.position = posicionEntrada;

                // Mover cámara
                if (Camera.main != null)
                {
                    Camera.main.transform.position = new Vector3(posicionEntrada.x, posicionEntrada.y, -10);
                }
            }

            // 4. ACTIVAR UI
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ActivarModoJefe(true);
            }

            // Finalizar puerta
            if (cartelAviso != null) cartelAviso.SetActive(false);
            this.enabled = false;
        }
        else
        {
            Debug.Log("Monedas insuficientes.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorCerca = true;
            if (cartelAviso != null) cartelAviso.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorCerca = false;
            if (cartelAviso != null) cartelAviso.SetActive(false);
        }
    }
}