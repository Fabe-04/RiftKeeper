using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Prefabs")]
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject chargerPrefab;
    [SerializeField] GameObject bossPrefab; // <--- NUEVO: Para el Jefe Final

    [Header("Configuración de Spawning")]
    [SerializeField] int minEnemies = 2;
    [SerializeField] int maxEnemies = 4;

    [Header("Ajustes de Posicionamiento")]
    [Tooltip("Distancia desde la pared hacia adentro")]
    [SerializeField] float margenParedes = 2.0f;
    // Eliminada variable 'grosorBorde' para limpiar el Warning

    Transform enemiesParent;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public List<Enemy> SpawnEnemiesInRoom(Vector2 roomCenter, int anchoSala, int altoSala)
    {
        List<Enemy> spawnedList = new List<Enemy>();

        if (enemiesParent == null)
        {
            GameObject p = new GameObject("Enemies_Container");
            enemiesParent = p.transform;
        }

        int count = Random.Range(minEnemies, maxEnemies + 1);

        float limiteX = (anchoSala / 2f) - margenParedes;
        float limiteY = (altoSala / 2f) - margenParedes;

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPos = ObtenerPosicionPerimetral(roomCenter, limiteX, limiteY);

            GameObject prefab = (Random.value > 0.8f) ? chargerPrefab : enemyPrefab;

            if (prefab != null)
            {
                GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity, enemiesParent);
                Enemy script = obj.GetComponent<Enemy>();
                if (script != null) spawnedList.Add(script);
            }
        }
        return spawnedList;
    }

    // --- NUEVO MÉTODO PARA EL JEFE (Soluciona el Error CS1061) ---
    public void SpawnBoss(Vector2 position)
    {
        if (bossPrefab != null)
        {
            if (enemiesParent == null)
            {
                GameObject p = new GameObject("Enemies_Container");
                enemiesParent = p.transform;
            }
            // Instanciamos el jefe en el centro de su sala
            Instantiate(bossPrefab, position, Quaternion.identity, enemiesParent);
        }
        else
        {
            Debug.LogWarning("EnemyManager: ¡No has asignado el Boss Prefab en el Inspector!");
        }
    }
    // ------------------------------------------------------------

    Vector2 ObtenerPosicionPerimetral(Vector2 centro, float xMax, float yMax)
    {
        int lado = Random.Range(0, 4);
        float x = 0, y = 0;

        switch (lado)
        {
            case 0: // ARRIBA
                x = Random.Range(-xMax, xMax);
                y = yMax;
                break;
            case 1: // ABAJO
                x = Random.Range(-xMax, xMax);
                y = -yMax;
                break;
            case 2: // IZQUIERDA
                x = -xMax;
                y = Random.Range(-yMax, yMax);
                break;
            case 3: // DERECHA
                x = xMax;
                y = Random.Range(-yMax, yMax);
                break;
        }
        return centro + new Vector2(x, y);
    }

    public void DestroyAllEnemies()
    {
        if (enemiesParent != null)
        {
            foreach (Transform child in enemiesParent) Destroy(child.gameObject);
        }
    }
}