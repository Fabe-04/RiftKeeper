using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour
{
    public static ShopUIManager Instance;

    [Header("UI Referencias")]
    [SerializeField] GameObject shopPanel;
    [SerializeField] Transform itemsContainer;
    [SerializeField] GameObject itemButtonPrefab;
    [SerializeField] TextMeshProUGUI coinsText;

    [Header("Configuración")]
    public List<ShopItemData> itemsDisponibles;

    private Player playerRef;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        // Nos aseguramos al inicio de que esté cerrado y el tiempo corra
        shopPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OpenShop(Player player)
    {
        Debug.Log("Abriendo Tienda...");
        playerRef = player;
        shopPanel.SetActive(true);
        UpdateCoinsText();
        GenerateButtons();

        // Pausar juego
        Time.timeScale = 0f;
    }

    // --- FUNCIÓN CRÍTICA: CERRAR ---
    public void CloseShop()
    {
        Debug.Log("Cerrando Tienda..."); // Si no ves esto en consola, el botón no está conectado
        shopPanel.SetActive(false);
        Time.timeScale = 1f; // Reactivar tiempo es vital
    }

    void UpdateCoinsText()
    {
        if (GameManager.instance != null)
        {
            coinsText.text = $"Monedas: {GameManager.instance.currentCoins}";
            // Sincronizar también el HUD principal
            if (UIManager.Instance != null)
                UIManager.Instance.ActualizarMonedas(GameManager.instance.currentCoins);
        }
    }

    void GenerateButtons()
    {
        // Limpiar botones viejos
        foreach (Transform child in itemsContainer) Destroy(child.gameObject);

        Debug.Log($"Generando botones. Items en lista: {itemsDisponibles.Count}");

        foreach (var item in itemsDisponibles)
        {
            // --- AUDITORÍA DEL ARCO ---
            if (item.tipo == ShopItemData.TipoEfecto.ComprarArco)
            {
                Debug.Log($"Detectado item ARCO. ¿El jugador ya lo tiene?: {playerRef.hasBow}");

                // Si el jugador YA tiene el arco, saltamos este ciclo (no creamos botón)
                if (playerRef.hasBow)
                {
                    Debug.Log("Omitiendo botón de arco (Ya comprado).");
                    continue;
                }
            }
            // --------------------------

            GameObject btnObj = Instantiate(itemButtonPrefab, itemsContainer);

            // Configurar Imagen
            Image[] images = btnObj.GetComponentsInChildren<Image>();
            // images[0] es el fondo del botón, images[1] es el icono
            if (images.Length > 1)
            {
                images[1].sprite = item.icono;
                images[1].rectTransform.localScale = item.escalaIcono;
                images[1].rectTransform.anchoredPosition = item.posicionIcono;
                images[1].preserveAspect = true;
            }

            // Configurar Textos
            TextMeshProUGUI[] texts = btnObj.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0) texts[0].text = item.nombreItem;
            if (texts.Length > 1) texts[1].text = $"${item.precio}";

            // Configurar Botón de Compra
            Button btn = btnObj.GetComponent<Button>();
            // Limpiamos listeners previos por seguridad
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => TryBuy(item, btnObj));
        }
    }

    void TryBuy(ShopItemData item, GameObject btnObj)
    {
        if (GameManager.instance.currentCoins >= item.precio)
        {
            bool exito = AplicarEfecto(item);

            if (exito)
            {
                GameManager.instance.SpendCoins(item.precio);
                UpdateCoinsText();

                // Agregar icono de Buff al HUD (si no es el arco)
                if (UIManager.Instance != null && item.icono != null && item.tipo != ShopItemData.TipoEfecto.ComprarArco)
                {
                    UIManager.Instance.AgregarBuff(item.icono);
                }

                // Si compramos el Arco
                if (item.tipo == ShopItemData.TipoEfecto.ComprarArco)
                {
                    Debug.Log("Arco comprado. Ocultando botón y actualizando HUD.");
                    if (UIManager.Instance != null) UIManager.Instance.MostrarArcoEnHUD(true);
                    Destroy(btnObj); // Borramos el botón de la tienda inmediatamente
                }
            }
        }
        else
        {
            Debug.Log("Dinero insuficiente");
        }
    }

    bool AplicarEfecto(ShopItemData item)
    {
        switch (item.tipo)
        {
            case ShopItemData.TipoEfecto.Curacion:
                // Solo compra si no está lleno de vida
                if (playerRef.currentHealth >= playerRef.maxHealth) return false;
                playerRef.Curar(item.valorEfecto);
                return true;

            case ShopItemData.TipoEfecto.Flechas:
                playerRef.RecogerFlecha(item.valorEfecto);
                return true;

            case ShopItemData.TipoEfecto.UpgradeDaño:
                playerRef.UpgradeSwordDamage(item.valorEfecto);
                return true;

            case ShopItemData.TipoEfecto.UpgradeVida:
                playerRef.UpgradeMaxHealth(item.valorEfecto);
                return true;

            case ShopItemData.TipoEfecto.ComprarArco:
                playerRef.hasBow = true; // <--- AQUÍ ACTIVAMOS EL ARCO EN EL PLAYER
                return true;
        }
        return false;
    }
}