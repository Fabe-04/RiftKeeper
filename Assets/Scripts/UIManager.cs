using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panel Estado (Arriba Izq)")]
    [SerializeField] Slider sliderVida;
    [SerializeField] TextMeshProUGUI textoVidaNumerico;
    [SerializeField] Slider sliderStamina;
    [SerializeField] TextMeshProUGUI textoMonedas;
    [SerializeField] Transform contenedorBuffs;
    [SerializeField] GameObject prefabIconoBuff;

    [Header("Panel Mapa (Arriba Der)")]
    [SerializeField] TextMeshProUGUI textoEnemigos;

    [Header("Panel Armas (Abajo Der)")]
    [SerializeField] Image iconoEspada;
    [SerializeField] Image iconoArco;
    [SerializeField] TextMeshProUGUI textoFlechas;

    // --- NUEVAS SECCIONES PARA EL JEFE ---
    [Header("Panel Jefe Final (Oculto por defecto)")]
    [SerializeField] GameObject panelJefe; // El objeto padre que contiene la barra
    [SerializeField] Slider sliderVidaJefe;
    [SerializeField] TextMeshProUGUI textoNombreJefe;

    [Header("Pantallas Finales")]
    [SerializeField] GameObject panelVictoria;
    // -------------------------------------

    private Color colorActivo = Color.white;
    private Color colorInactivo = new Color(1, 1, 1, 0.4f);

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (textoEnemigos != null) textoEnemigos.gameObject.SetActive(false);

        // Aseguramos que la UI del jefe y victoria empiecen ocultas
        if (panelJefe != null) panelJefe.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
    }

    // --- ESTADO ---
    public void ActualizarVida(int actual, int max)
    {
        if (sliderVida != null) { sliderVida.maxValue = max; sliderVida.value = actual; }
        if (textoVidaNumerico != null) textoVidaNumerico.text = $"{actual}/{max}";
    }

    public void ActualizarStamina(float actual, float max)
    {
        if (sliderStamina != null) { sliderStamina.maxValue = max; sliderStamina.value = actual; }
    }

    public void ActualizarMonedas(int cantidad)
    {
        if (textoMonedas != null) textoMonedas.text = cantidad.ToString();
    }

    // --- ARMAS ---
    public void MostrarArcoEnHUD(bool mostrar)
    {
        if (iconoArco != null) iconoArco.gameObject.SetActive(mostrar);
        if (textoFlechas != null) textoFlechas.gameObject.SetActive(mostrar);
    }

    public void ActualizarFlechas(int cantidad)
    {
        if (textoFlechas != null) textoFlechas.text = "x" + cantidad.ToString();
    }

    public void CambiarArmaVisual(bool usandoArco)
    {
        if (iconoEspada != null) iconoEspada.color = usandoArco ? colorInactivo : colorActivo;
        if (iconoArco != null) iconoArco.color = usandoArco ? colorActivo : colorInactivo;
    }

    // --- COMBATE ---
    public void ActivarCombateUI(int cantidad)
    {
        if (textoEnemigos != null)
        {
            textoEnemigos.gameObject.SetActive(true);
            textoEnemigos.text = $"Enemigos: {cantidad}";
        }
    }

    public void DesactivarCombateUI()
    {
        if (textoEnemigos != null) textoEnemigos.text = "Sala Despejada";
    }

    public void ActualizarEnemigosRestantes(int cantidad)
    {
        if (cantidad <= 0) DesactivarCombateUI();
        else if (textoEnemigos != null) textoEnemigos.text = $"Enemigos: {cantidad}";
    }

    // --- BUFFS ---
    public void AgregarBuff(Sprite icono)
    {
        if (prefabIconoBuff != null && contenedorBuffs != null && icono != null)
        {
            GameObject nuevoIcono = Instantiate(prefabIconoBuff, contenedorBuffs);
            nuevoIcono.GetComponent<Image>().sprite = icono;
        }
    }

    // --- FUNCIONES DEL JEFE (NUEVAS) ---
    public void ActivarModoJefe(bool activar)
    {
        if (panelJefe != null) panelJefe.SetActive(activar);
    }

    public void ActualizarVidaJefe(int actual, int max)
    {
        if (sliderVidaJefe != null)
        {
            sliderVidaJefe.maxValue = max;
            sliderVidaJefe.value = actual;
        }
    }

    public void MostrarVictoria()
    {
        if (panelVictoria != null) panelVictoria.SetActive(true);
    }
}