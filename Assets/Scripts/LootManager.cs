using UnityEngine;
using TMPro;

public class LootManager : MonoBehaviour
{
    public static LootManager Instance;

    [Header("Objetos que van a caer")]
    public GameObject monedaPrefab;
    public GameObject pocionPrefab;

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
        // Posición base (donde muere el enemigo, pero en Z=0)
        Vector3 posicionVisible = new Vector3(posicionMuerte.x, posicionMuerte.y, 0f);

        if (tipoEnemigo == EnemyType.Basico)
        {
            float probabilidad = Random.Range(0f, 100f);
            if (probabilidad <= 70f)
            {
                Instantiate(monedaPrefab, posicionVisible, Quaternion.identity);
            }
        }
        else if (tipoEnemigo == EnemyType.Charger)
        {
            // 1. Soltamos la Moneda en el sitio exacto
            Instantiate(monedaPrefab, posicionVisible, Quaternion.identity);

            float probabilidadPocion = Random.Range(0f, 100f);

            // (Dejado en 100f para tus pruebas, luego bájalo a 25f)
            if (probabilidadPocion <= 100f)
            {
                // --- CAMBIO AQUÍ: EL EMPUJÓN ---
                // Creamos una nueva posición sumándole 0.8 en X (a la derecha)
                Vector3 posicionPocion = posicionVisible + new Vector3(2.5f, 0f, 0f);
                // -------------------------------

                // Usamos 'posicionPocion' en vez de 'posicionVisible'
                Instantiate(pocionPrefab, posicionPocion, Quaternion.identity);
            }
        }
    }

    public void SumarMoneda(int cantidad)
    {
        monedasTotales += cantidad;
        ActualizarTexto();
        Debug.Log("💰 MONEDAS: " + monedasTotales);
    }

    void ActualizarTexto()
    {
        if (textoMonedas != null)
        {
            textoMonedas.text = "Monedas: " + monedasTotales.ToString();
        }
    }
}