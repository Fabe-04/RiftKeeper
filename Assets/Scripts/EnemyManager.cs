using System.Collections.Generic; // ¡Importante! Para usar Listas
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject chargerPrefab;

    // --- LÓGICA DE SPAWN ELIMINADA DE AQUÍ ---
    // [SerializeField] float timeBetweenSpawns = 0.5f;
    // public float currentTimeBetweenSpawns;

    Transform enemiesParent;
    public static EnemyManager Instance;

    // --- NUEVO: El "Contador" de Enemigos ---
    private List<Enemy> liveEnemies = new List<Enemy>();
    // --- FIN NUEVO ---


    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        enemiesParent = GameObject.Find("Enemies").transform;
    }

    // --- SE ELIMINÓ EL "Update()" ---
    // Ya no generamos enemigos constantemente, sino en ráfagas.

    Vector2 RandomPosition()
    {
        // 1. Pregunta al RoomGenerator por los límites de la sala actual
        BoundsInt salaBounds = RoomGenerator.Instance.SalaGeneradaBounds;

        // 2. Calcula una posición X e Y aleatoria DENTRO de esos límites
        // Sumamos/Restamos 1 para dejar un pequeño margen y no aparecer pegado al muro
        float randomX = Random.Range(salaBounds.xMin + 1.5f, salaBounds.xMax - 1.5f);
        float randomY = Random.Range(salaBounds.yMin + 1.5f, salaBounds.yMax - 1.5f);

        return new Vector2(randomX, randomY);
    }

    // --- NUEVA FUNCIÓN: Llamada por WaveManager ---
    public void SpawnWave(int waveNumber)
    {
        // Una fórmula simple para hacer que el juego sea más difícil
        int enemiesToSpawn = 5 + (waveNumber * 2); // Oleada 1 = 7, Oleada 2 = 9, etc.

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            // Tu lógica de spawn de 90/10 estaba perfecta
            var roll = Random.Range(0, 100);
            var enemyType = roll < 90 ? enemyPrefab : chargerPrefab;

            var e = Instantiate(enemyType, RandomPosition(), Quaternion.identity);
            e.transform.SetParent(enemiesParent);

            // ¡No te olvides de añadirlo a la lista!
            // (El script 'Enemy' se registrará a sí mismo en Start())
        }
    }
    // --- FIN NUEVO ---


    // --- NUEVAS FUNCIONES DE REGISTRO ---
    // El 'Enemy.cs' llamará a estas funciones

    public void RegisterEnemy(Enemy enemy)
    {
        liveEnemies.Add(enemy);
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        liveEnemies.Remove(enemy);
    }

    public int GetLiveEnemyCount()
    {
        return liveEnemies.Count;
    }
    // --- FIN NUEVO ---

    public void DestroyAllEnemies()
    {
        // Esta función sigue siendo útil si el jugador sale
        foreach (Transform e in enemiesParent)
            Destroy(e.gameObject);

        liveEnemies.Clear(); // Limpiamos la lista
    }
}