using UnityEngine;
using UnityEngine.UI;

// Este script requiere que el objeto tenga un CanvasGroup
[RequireComponent(typeof(CanvasGroup))]
public class UIBlinker : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Velocidad del parpadeo")]
    [SerializeField] float speed = 2f;
    [Tooltip("Transparencia mínima (0 es invisible, 1 es visible)")]
    [Range(0f, 1f)]
    [SerializeField] float minAlpha = 0.4f;
    [Tooltip("Transparencia máxima")]
    [Range(0f, 1f)]
    [SerializeField] float maxAlpha = 1f;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        // Usamos una onda seno (Mathf.Sin) para crear un ciclo suave entre -1 y 1.
        // Lo ajustamos para que oscile entre 0 y 1 usando (sin + 1) / 2.
        float alphaOscillation = (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f;

        // Interpolamos (Lerp) entre el alpha mínimo y máximo usando esa oscilación.
        canvasGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, alphaOscillation);
    }
}