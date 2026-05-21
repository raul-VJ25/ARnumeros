using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class ObjectManipulation : MonoBehaviour
{
    [Header("UI Configuration")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Camera arCamera;

    [Header("Interaction Settings")]
    [SerializeField] private float rotationSpeed = 0.5f;
    [SerializeField] private float minScale = 0.3f;
    [SerializeField] private float maxScale = 1.5f;
    [SerializeField] private float tapThreshold = 15f; // Píxeles de movimiento para considerar "Arrastre"
    [SerializeField] private int panelFontSize = 28;

    private Transform selectedObject;
    private Vector2 touchStartPos;
    private Vector2 lastTouchPos;
    private float initialTouchDistance;
    private float initialScale;
    private bool isDragging;
    private bool wasPinching;
    private Dictionary<string, string> translationData;

    void Start()
    {
        EnhancedTouchSupport.Enable();
        if (arCamera == null) arCamera = Camera.main;
        if (infoPanel != null) infoPanel.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseInfoPanel);
        }

        if (infoText != null)
        {
            infoText.fontSize = panelFontSize;
        }

        InitializeTranslations();
    }

    void InitializeTranslations()
    {
        translationData = new Dictionary<string, string>
        {
            {"0", "0\n\nEspañol: Cero\nInglés: Zero\nFrancés: Zéro\nPortugués: Zero\nAlemán: Null\nItaliano: Zero\nNeerlandés: Nul\nTurco: Sıfır\nPolaco: Zero\nSueco: Noll"},
            {"1", "1\n\nEspañol: Uno\nInglés: One\nFrancés: Un\nPortugués: Um\nAlemán: Eins\nItaliano: Uno\nNeerlandés: Een\nTurco: Bir\nPolaco: Jeden\nSueco: En"},
            {"2", "2\n\nEspañol: Dos\nInglés: Two\nFrancés: Deux\nPortugués: Dois\nAlemán: Zwei\nItaliano: Due\nNeerlandés: Twee\nTurco: İki\nPolaco: Dwa\nSueco: Två"},
            {"3", "3\n\nEspañol: Tres\nInglés: Three\nFrancés: Trois\nPortugués: Três\nAlemán: Drei\nItaliano: Tre\nNeerlandés: Drie\nTurco: Üç\nPolaco: Trzy\nSueco: Tre"},
            {"4", "4\n\nEspañol: Cuatro\nInglés: Four\nFrancés: Quatre\nPortugués: Quatro\nAlemán: Vier\nItaliano: Quattro\nNeerlandés: Vier\nTurco: Dört\nPolaco: Cztery\nSueco: Fyra"},
            {"5", "5\n\nEspañol: Cinco\nInglés: Five\nFrancés: Cinq\nPortugués: Cinco\nAlemán: Fünf\nItaliano: Cinque\nNeerlandés: Vijf\nTurco: Beş\nPolaco: Pięć\nSueco: Fem"},
            {"6", "6\n\nEspañol: Seis\nInglés: Six\nFrancés: Six\nPortugués: Seis\nAlemán: Sechs\nItaliano: Sei\nNeerlandés: Zes\nTurco: Altı\nPolaco: Sześć\nSueco: Sex"},
            {"7", "7\n\nEspañol: Siete\nInglés: Seven\nFrancés: Sept\nPortugués: Sete\nAlemán: Sieben\nItaliano: Sette\nNeerlandés: Zeven\nTurco: Yedi\nPolaco: Siedem\nSueco: Sju"},
            {"8", "8\n\nEspañol: Ocho\nInglés: Eight\nFrancés: Huit\nPortugués: Oito\nAlemán: Acht\nItaliano: Otto\nNeerlandés: Acht\nTurco: Sekiz\nPolaco: Osiem\nSueco: Åtta"},
            {"9", "9\n\nEspañol: Nueve\nInglés: Nine\nFrancés: Neuf\nPortugués: Nove\nAlemán: Neun\nItaliano: Nove\nNeerlandés: Negen\nTurco: Dokuz\nPolaco: Dziewięć\nSueco: Nio"}
        };
    }

    void Update()
    {
        var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;

        if (touches.Count == 0)
        {
            isDragging = false;
            wasPinching = false;
            initialTouchDistance = 0f;
            return;
        }

        var touch1 = touches[0];

        // INICIO DEL TOQUE
        if (touch1.phase == TouchPhase.Began)
        {
            touchStartPos = touch1.screenPosition;
            lastTouchPos = touch1.screenPosition;
            isDragging = false;
            wasPinching = false;
            initialTouchDistance = 0f;

            Ray ray = arCamera.ScreenPointToRay(touch1.screenPosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                selectedObject = hit.transform;
                initialScale = selectedObject.localScale.x;
                // AQUÍ NO ABRIMOS EL PANEL AÚN, ESPERAMOS A VER SI ES TAP O DRAG
            }
            else
            {
                selectedObject = null;
            }
        }

        if (selectedObject != null)
        {
            // PELLIZCO (2 dedos)
            if (touches.Count == 2)
            {
                wasPinching = true;
                isDragging = true; // El pellizco cuenta como interacción, no tap
                var touch2 = touches[1];
                float currentDist = Vector2.Distance(touch1.screenPosition, touch2.screenPosition);
                if (initialTouchDistance == 0f) initialTouchDistance = currentDist;

                float newScale = initialScale * (currentDist / initialTouchDistance);
                newScale = Mathf.Clamp(newScale, minScale, maxScale);
                selectedObject.localScale = Vector3.one * newScale;
            }
            // ROTACIÓN / ARRASTRE (1 dedo)
            else if (touches.Count == 1 && touch1.phase == TouchPhase.Moved)
            {
                float distanceMoved = Vector2.Distance(touchStartPos, touch1.screenPosition);

                // Si se ha movido más del umbral (15px), se considera arrastre
                if (distanceMoved > tapThreshold)
                {
                    isDragging = true;
                }

                if (isDragging)
                {
                    Vector2 delta = touch1.screenPosition - lastTouchPos;
                    selectedObject.Rotate(Vector3.up, delta.x * rotationSpeed, Space.World);
                }

                lastTouchPos = touch1.screenPosition;
            }

            // FINAL DEL TOQUE
            if (touch1.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Canceled)
            {
                // Solo abrir/cerrar si fue un TAP (sin arrastre) Y tocamos un objeto
                // Esto evita que se abra al rotar y que se cierre al tocar fuera
                if (!isDragging && !wasPinching && selectedObject != null)
                {
                    ToggleInfoPanel();
                }

                // Resetear estados
                isDragging = false;
                wasPinching = false;
                initialTouchDistance = 0f;
            }
        }
    }

    void ToggleInfoPanel()
    {
        if (infoPanel == null || infoText == null) return;

        bool newState = !infoPanel.activeSelf;
        infoPanel.SetActive(newState);

        if (newState && selectedObject != null)
        {
            translationData.TryGetValue(selectedObject.name, out string text);
            infoText.text = string.IsNullOrEmpty(text) ? "Unknown" : text;
        }
    }

    void CloseInfoPanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }
}