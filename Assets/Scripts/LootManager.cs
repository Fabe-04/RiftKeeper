using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LootManager : MonoBehaviour
{
    public static LootManager Instance;

    [System.Serializable]
    public struct LootItem
    {
        public GameObject prefab;
        [Range(0, 100)] public float dropChance;
    }

    [Header("Tablas de Loot")]
    public List<LootItem> basicEnemyLoot;
    public List<LootItem> chargerEnemyLoot;
    public List<LootItem> bossLoot;

    // Enum para tipos de enemigos
    public enum EnemyType { Basico, Charger, Boss }

    [Header("Loot Global")]
    public GameObject flechaPickupPrefab;
    [Range(0, 100)] public float flechaDropChance = 50f;

    // NOTA: Eliminamos 'textoMonedas' y 'monedasTotales' de aquí.
    // La UI y el dato real viven en GameManager.

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SpawnLoot(Vector3 posicionMuerte, EnemyType tipoEnemigo)
    {
        // 1. Loot Global (Flechas)
        if (flechaPickupPrefab != null)
        {
            if (Random.Range(0f, 100f) <= flechaDropChance)
            {
                Instantiate(flechaPickupPrefab, posicionMuerte, Quaternion.identity);
            }
        }

        // 2. Selección de Tabla
        List<LootItem> tableToUse = null;

        switch (tipoEnemigo)
        {
            case EnemyType.Basico: tableToUse = basicEnemyLoot; break;
            case EnemyType.Charger: tableToUse = chargerEnemyLoot; break;
            case EnemyType.Boss: tableToUse = bossLoot; break;
        }

        if (tableToUse == null) return;

        // 3. Generar Loot
        foreach (var item in tableToUse)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= item.dropChance)
            {
                Vector3 spawnPos = posicionMuerte + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                Instantiate(item.prefab, spawnPos, Quaternion.identity);
            }
        }
    }

    // REDIRECCIÓN: Ahora sumar moneda llama al GameManager
    public void SumarMoneda(int cantidad)
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.AddCoins(cantidad);
        }
    }
}