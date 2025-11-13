using System.Collections;
using TMPro; // Asegúrate de que esto esté
using UnityEngine;
using UnityEngine.UI; // Necesario para la UI

public class WaveManager : MonoBehaviour // ¡Asegúrate de que el nombre de la clase coincida con el archivo!
{
    // --- SE ELIMINÓ "timeText" ---
    [SerializeField] TextMeshProUGUI waveText;

    public static WaveManager Instance;

    bool waveRunning = false; // Inicia en 'false' hasta que estemos listos
    int currentWave = 0;

    // --- SE ELIMINÓ "currentWaveTime" ---

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // Esperamos 3 segundos antes de empezar la primera oleada
        StartCoroutine(StartNextWaveWithDelay(3.0f));
    }

    // --- "Update()" AHORA ES NUESTRO TEMPORIZADOR DE VERIFICACIÓN ---
    private void Update()
    {
        // Si la oleada no está corriendo, no hagas nada
        if (!waveRunning) return;

        // Si la oleada está corriendo Y el contador de enemigos llega a CERO...
        if (EnemyManager.Instance.GetLiveEnemyCount() == 0)
        {
            // ¡Gana la oleada!
            WaveComplete();
        }
    }
    // --- FIN NUEVO ---

    public bool WaveRuuning() => waveRunning;

    private void StartNewWave()
    {
        currentWave++;
        waveRunning = true;
        waveText.text = "Wave: " + currentWave;

        // --- ¡LA NUEVA LÍNEA CLAVE! ---
        // 1. Generamos la sala ANTES de generar enemigos
        RoomGenerator.Instance.GenerateRoom();
        // Mover al jugador al centro de la nueva sala (posición 0,0 relativa a la sala)
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player"); // Busca al jugador por Tag
        if (playerObject != null)
        {
            // Lo teletransportamos al centro (0,0,0) del mundo, que coincide con el centro de la sala
            playerObject.transform.position = Vector3.zero;
        }
        // --- FIN NUEVO ---

        // 2. Ahora sí, generamos los enemigos dentro de la sala recién creada
        EnemyManager.Instance.SpawnWave(currentWave);
    }

    // --- SE ELIMINÓ LA CORUTINA "WaveTimer()" ---

    private void WaveComplete()
    {
        waveRunning = false;
        waveText.text = "Wave " + currentWave + " Complete!";

        // Ya no necesitamos destruir a los enemigos, ¡ya están muertos!
        // EnemyManager.Instance.DestroyAllEnemies(); 

        // Empezamos la siguiente oleada después de 5 segundos de descanso
        StartNewWave();
    }

    // --- NUEVA CORUTINA DE RETRASO ---
    IEnumerator StartNextWaveWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNewWave();
    }
    // --- FIN NUEVO ---
}