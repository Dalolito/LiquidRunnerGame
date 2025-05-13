using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlsHelpPanel : MonoBehaviour
{
    [Header("Panel Settings")]
    public bool showOnStart = true;
    public KeyCode toggleKey = KeyCode.H;  // Tecla para mostrar/ocultar el panel
    
    [Header("UI References")]
    public GameObject panelContainer;      // Panel principal que contiene todos los elementos
    public Image[] shapeImages;            // Imágenes para cada forma disponible
    public TextMeshProUGUI[] keyTexts;     // Textos para mostrar las teclas (1, 2, 3, 4)
    public TextMeshProUGUI movementText;   // Texto para controles de movimiento
    public TextMeshProUGUI jumpText;       // Texto para controles de salto
    
    [Header("Appearance")]
    public Color backgroundColor = new Color(0, 0, 0, 0.7f);  // Color de fondo del panel
    public Color textColor = Color.white;                     // Color del texto
    public Color highlightColor = new Color(0, 0.8f, 1f);     // Color de resaltado para la forma actual
    
    private LiquidCharacterController playerController;       // Referencia al controlador del jugador
    private CanvasGroup canvasGroup;                          // Para controlar la transparencia del panel
    
    void Start()
    {
        // Encontrar el controlador del jugador
        playerController = FindObjectOfType<LiquidCharacterController>();
        
        // Obtener el CanvasGroup (lo añadiremos si no existe)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Configurar el panel inicial
        SetupPanel();
        
        // Mostrar/ocultar el panel según la configuración
        if (showOnStart)
        {
            ShowPanel();
        }
        else
        {
            HidePanel();
        }
    }
    
    void Update()
    {
        // Alternar la visibilidad del panel con la tecla configurada
        if (Input.GetKeyDown(toggleKey))
        {
            TogglePanel();
        }
        
        // Si el controlador del jugador está disponible, actualizar el resaltado
        if (playerController != null)
        {
            UpdateHighlightedShape();
        }
    }
    
    void SetupPanel()
    {
        // Verificar que tenemos todos los elementos necesarios
        if (panelContainer == null)
        {
            Debug.LogError("Panel container is missing!");
            return;
        }
        
        // Configurar el panel con los colores definidos
        Image panelImage = panelContainer.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.color = backgroundColor;
        }
        
        // Configurar los textos de los controles
        if (movementText != null)
        {
            movementText.text = "Mover: ← →";
            movementText.color = textColor;
        }
        
        if (jumpText != null)
        {
            jumpText.text = "Saltar: Espacio";
            jumpText.color = textColor;
        }
        
        // Configurar los textos de las teclas
        for (int i = 0; i < keyTexts.Length; i++)
        {
            if (keyTexts[i] != null)
            {
                keyTexts[i].text = (i + 1).ToString();
                keyTexts[i].color = textColor;
            }
        }
    }
    
    void UpdateHighlightedShape()
    {
        // Resetear todos los colores de las imágenes de forma
        for (int i = 0; i < shapeImages.Length; i++)
        {
            if (shapeImages[i] != null)
            {
                // Color normal para todas las formas
                shapeImages[i].color = Color.white;
            }
        }
        
        // Resaltar la forma actual
        int currentShapeIndex = (int)playerController.currentShape;
        if (currentShapeIndex >= 0 && currentShapeIndex < shapeImages.Length)
        {
            if (shapeImages[currentShapeIndex] != null)
            {
                // Color de resaltado para la forma actual
                shapeImages[currentShapeIndex].color = highlightColor;
            }
        }
    }
    
    public void ShowPanel()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        if (panelContainer != null)
        {
            panelContainer.SetActive(true);
        }
    }
    
    public void HidePanel()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        if (panelContainer != null)
        {
            panelContainer.SetActive(false);
        }
    }
    
    public void TogglePanel()
    {
        if (canvasGroup != null && canvasGroup.alpha > 0)
        {
            HidePanel();
        }
        else
        {
            ShowPanel();
        }
    }
}