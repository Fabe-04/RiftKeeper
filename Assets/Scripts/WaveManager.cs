using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI waveText;

    public static WaveManager Instance;

    bool waveRunning = false;

    // --- CAMBIO 1: Ahora es 'public' para que el GameManager la pueda leer ---
    public int currentWave = 0;
    // ------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        StartCoroutine(StartNextWaveWithDelay(3.0f));
    }

    private void Update()
    {
        if (!waveRunning) return;

        if (EnemyManager.Instance.GetLiveEnemyCount() == 0)
        {
            WaveComplete();
        }
    }

    public bool WaveRuuning() => waveRunning;

    private void StartNewWave()
    {
        currentWave++;
        waveRunning = true;

        // --- CAMBIO 2: Traducción al Español ---
        waveText.text = "Oleada: " + currentWave;
        // ---------------------------------------

        RoomGenerator.Instance.GenerateRoom();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerObject.transform.position = Vector3.zero;
        }

        EnemyManager.Instance.SpawnWave(currentWave);
    }

    private void WaveComplete()
    {
        waveRunning = false;

        // --- CAMBIO 3: Traducción al Español ---
        waveText.text = "¡Oleada " + currentWave + " Completada!";
        // ---------------------------------------

        StartNewWave(); // Nota: Aquí quitaste el retraso en tu código original, si quieres delay usa la corrutina
    }

    IEnumerator StartNextWaveWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNewWave();
    }
}