using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI Global")]
    [SerializeField] TextMeshProUGUI waveText; // Texto de carga "Generando..."

    [Header("--- PANTALLA DE VICTORIA ---")]
    [SerializeField] GameObject victoryPanel;
    [SerializeField] TextMeshProUGUI vicTxtTiempo;    // Arrastra Txt_Tiempo aquí
    [SerializeField] TextMeshProUGUI vicTxtEnemigos;  // Arrastra Txt_Enemigos aquí
    [SerializeField] Button vicBtnRestart;            // Arrastra RestartButton aquí
    [SerializeField] Button vicBtnMenu;               // Arrastra MenuButton aquí

    [Header("--- PANTALLA DE DERROTA ---")]
    [SerializeField] GameObject gameOverPanel;
    // Si tu panel de derrota tiene textos de stats, agrégalos aquí también.
    // Por ahora asumo que es simple:
    [SerializeField] Button defBtnRestart;            // Botón reiniciar del Game Over
    [SerializeField] Button defBtnMenu;               // Botón menú del Game Over (si tienes)

    [Header("Economía")]
    public int currentCoins = 0;

    // --- ESTADÍSTICAS INTERNAS ---
    private float tiempoInicio;
    private int enemigosAbatidos = 0;
    private bool gameRunning = true;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
    }

    void Start()
    {
        // 1. Inicializar UI (Ocultar paneles)
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // 2. Conectar Botones de Victoria
        if (vicBtnRestart != null) vicBtnRestart.onClick.AddListener(RestartGame);
        if (vicBtnMenu != null) vicBtnMenu.onClick.AddListener(GoToMainMenu);

        // 3. Conectar Botones de Derrota
        if (defBtnRestart != null) defBtnRestart.onClick.AddListener(RestartGame);
        if (defBtnMenu != null) defBtnMenu.onClick.AddListener(GoToMainMenu);

        // 4. Inicializar Stats
        tiempoInicio = Time.time;
        enemigosAbatidos = 0;

        // 5. Iniciar Monedas
        if (UIManager.Instance != null)
            UIManager.Instance.ActualizarMonedas(currentCoins);

        StartCoroutine(IniciarJuego());
    }

    // --- LÓGICA DE JUEGO (Tu código original) ---
    IEnumerator IniciarJuego()
    {
        if (waveText != null) waveText.text = "Generando Mazmorra...";
        yield return null;

        if (DungeonGenerator.Instance != null)
            DungeonGenerator.Instance.GenerarMazmorra();

        yield return new WaitForFixedUpdate();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && DungeonGenerator.Instance.CentrosDeSalas.Count > 0)
        {
            Vector2 posInicio = DungeonGenerator.Instance.CentrosDeSalas[0];
            TeletransportarSeguro(player, posInicio);
            if (Camera.main != null)
                Camera.main.transform.position = new Vector3(posInicio.x, posInicio.y, -10f);
        }

        if (waveText != null) waveText.text = "";
        gameRunning = true;
    }

    void TeletransportarSeguro(GameObject player, Vector2 destino)
    {
        player.transform.position = new Vector3(destino.x, destino.y, 0f);
    }

    // --- ECONOMÍA Y KILLS ---
    public void AddCoins(int amount)
    {
        currentCoins += amount;
        if (UIManager.Instance != null) UIManager.Instance.ActualizarMonedas(currentCoins);
    }

    public bool SpendCoins(int amount)
    {
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            if (UIManager.Instance != null) UIManager.Instance.ActualizarMonedas(currentCoins);
            return true;
        }
        return false;
    }

    // ¡LLAMAR A ESTO DESDE ENEMY.CS CUANDO MUERA UN ENEMIGO!
    public void RegistrarKill()
    {
        enemigosAbatidos++;
    }

    // --- VICTORIA Y DERROTA ---
    public void Victory()
    {
        FinalizarPartida(true);
    }

    public void GameOver()
    {
        FinalizarPartida(false);
    }

    void FinalizarPartida(bool victoria)
    {
        gameRunning = false;
        Time.timeScale = 0f; // Pausar juego

        // Calcular Tiempo
        float duracion = Time.time - tiempoInicio;
        string textoTiempo = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(duracion / 60), Mathf.FloorToInt(duracion % 60));

        if (victoria)
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
                // Actualizar textos de la imagen
                if (vicTxtTiempo != null) vicTxtTiempo.text = "TIEMPO EN PARTIDA: " + textoTiempo;
                if (vicTxtEnemigos != null) vicTxtEnemigos.text = "ENEMIGOS DERROTADOS: " + enemigosAbatidos;
                // Nota: "Oleadas" lo ignoramos o ponemos "Salas: 8" si quieres.
            }
            Debug.Log("¡VICTORIA!");
        }
        else
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            // Si tu panel de derrota tuviera textos de stats, actualízalos aquí igual que arriba.
            Debug.Log("GAME OVER");
        }
    }

    // --- NAVEGACIÓN ---
    void RestartGame()
    {
        Time.timeScale = 1f; // Importante reactivar tiempo
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void GoToMainMenu()
    {
        Time.timeScale = 1f;
        // Asegúrate que tu escena del menú se llame "MainMenu"
        SceneManager.LoadScene("MainMenu");
    }

    public bool IsGameRunning() => gameRunning;
}