using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Configuración UI")]
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] Button restartButton; // Botón Reiniciar
    [SerializeField] Button menuButton;    // <--- NUEVO: Botón Menú

    [Header("Estadísticas Finales")]
    [SerializeField] TextMeshProUGUI textoTiempo;
    [SerializeField] TextMeshProUGUI textoEnemigos;
    [SerializeField] TextMeshProUGUI textoOleadas;

    private float tiempoDeJuego;
    private int enemigosEliminados;
    private bool gameRunning = true;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Configurar Botón Reiniciar
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        // --- NUEVO: Configurar Botón Menú ---
        if (menuButton != null)
            menuButton.onClick.AddListener(BackToMenu);
        // ------------------------------------

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        tiempoDeJuego = 0f;
        enemigosEliminados = 0;
        gameRunning = true;
    }

    private void Update()
    {
        if (gameRunning)
        {
            tiempoDeJuego += Time.deltaTime;
        }
    }

    public void RegistrarMuerteEnemigo()
    {
        if (gameRunning) enemigosEliminados++;
    }

    public void GameOver()
    {
        gameRunning = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // 1. Tiempo
            float minutos = Mathf.FloorToInt(tiempoDeJuego / 60);
            float segundos = Mathf.FloorToInt(tiempoDeJuego % 60);
            if (textoTiempo != null)
                textoTiempo.text = string.Format("Tiempo: {0:00}:{1:00}", minutos, segundos);

            // 2. Enemigos
            if (textoEnemigos != null)
                textoEnemigos.text = "Enemigos Eliminados: " + enemigosEliminados;

            // 3. Oleadas
            if (textoOleadas != null)
            {
                if (WaveManager.Instance != null)
                {
                    int oleadaFinal = WaveManager.Instance.currentWave;
                    textoOleadas.text = "Oleadas: " + oleadaFinal;
                }
                else textoOleadas.text = "Oleadas: -";
            }
        }
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- NUEVO: Función para ir al Menú ---
    void BackToMenu()
    {
        // Asegúrate que tu escena se llame EXACTAMENTE "MenuPrincipal"
        SceneManager.LoadScene("MenuPrincipal");
    }
}