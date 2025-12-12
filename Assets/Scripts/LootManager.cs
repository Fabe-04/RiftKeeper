using UnityEngine;
using TMPro;

public class LootManager : MonoBehaviour
{
    public static LootManager Instance;

    [Header("Objetos que van a caer")]
    public GameObject monedaPrefab;
    public GameObject pocionPrefab;
    public GameObject flechaPickupPrefab; // <--- NUEVO: Arrastra tu prefab aquí

    [Header("Interfaz (UI)")]
    public TextMeshProUGUI textoMonedas;
    public int monedasTotales = 0;

    public enum EnemyType { Basico, Charger }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ActualizarTexto();
    }

    public void SpawnLoot(Vector3 posicionMuerte, EnemyType tipoEnemigo)
    {
        Vector3 posicionVisible = new Vector3(posicionMuerte.x, posicionMuerte.y, 0f);

        // --- LÓGICA DE FLECHAS (Para todos los enemigos) ---
        // 50% de probabilidad de recuperar una flecha al matar
        if (flechaPickupPrefab != null)
        {
            if (Random.Range(0f, 100f) <= 100f)
            {
                Instantiate(flechaPickupPrefab, posicionVisible, Quaternion.identity);
            }
        }
        // ---------------------------------------------------

        if (tipoEnemigo == EnemyType.Basico)
        {
            if (Random.Range(0f, 100f) <= 70f)
            {
                // Un pequeño desplazamiento para que no caiga encima de la flecha
                Instantiate(monedaPrefab, posicionVisible + new Vector3(1.5f, 0, 0), Quaternion.identity);
            }
        }
        else if (tipoEnemigo == EnemyType.Charger)
        {
            Instantiate(monedaPrefab, posicionVisible + new Vector3(0.5f, 0, 0), Quaternion.identity);

            if (Random.Range(0f, 100f) <= 100f) // Probabilidad Poción
            {
                Vector3 posPocion = posicionVisible + new Vector3(-0.5f, 0, 0);
                Instantiate(pocionPrefab, posPocion, Quaternion.identity);
            }
        }
    }

    public void SumarMoneda(int cantidad)
    {
        monedasTotales += cantidad;
        ActualizarTexto();
    }

    void ActualizarTexto()
    {
        if (textoMonedas != null) textoMonedas.text = "Monedas: " + monedasTotales.ToString();
    }
}