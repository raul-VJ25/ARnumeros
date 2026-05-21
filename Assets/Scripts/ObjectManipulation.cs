using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem.EnhancedTouch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class ObjectManipulation : MonoBehaviour
{
    [Header("UI Configuration")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private Camera arCamera;

    [Header("Interaction Settings")]
    [SerializeField] private float rotationSpeed = 0.5f;
    [SerializeField] private float minScale = 0.3f;
    [SerializeField] private float maxScale = 1.5f;
    [SerializeField] private float tapThreshold = 12f;

    private Transform selectedObject;
    private Vector2 touchStartPos;
    private Vector2 lastTouchPos;
    private float initialTouchDistance;
    private float initialScale;
    private bool isPanelOpen;
    private bool isDragging;
    private bool wasPinching;
    private Dictionary<string, string> translationData;

    void Start()
    {
        EnhancedTouchSupport.Enable();
        if (arCamera == null) arCamera = Camera.main;
        if (infoPanel != null) infoPanel.SetActive(false);
        InitializeTranslations();
    }

    void InitializeTranslations()
    {
        translationData = new Dictionary<string, string>
        {
            {"0", "ES: Cero\nEN: Zero\nFR: Zéro\nDE: Null\nIT: Zero\nPT: Zero"},
            {"1", "ES: Uno\nEN: One\nFR: Un\nDE: Eins\nIT: Uno\nPT: Um"},
            {"2", "ES: Dos\nEN: Two\nFR: Deux\nDE: Zwei\nIT: Due\nPT: Dois"},
            {"3", "ES: Tres\nEN: Three\nFR: Trois\nDE: Drei\nIT: Tre\nPT: Três"},
            {"4", "ES: Cuatro\nEN: Four\nFR: Quatre\nDE: Vier\nIT: Quattro\nPT: Quatro"},
            {"5", "ES: Cinco\nEN: Five\nFR: Cinq\nDE: Fünf\nIT: Cinque\nPT: Cinco"},
            {"6", "ES: Seis\nEN: Six\nFR: Six\nDE: Sechs\nIT: Sei\nPT: Seis"},
            {"7", "ES: Siete\nEN: Seven\nFR: Sept\nDE: Sieben\nIT: Sette\nPT: Sete"},
            {"8", "ES: Ocho\nEN: Eight\nFR: Huit\nDE: Acht\nIT: Otto\nPT: Oito"},
            {"9", "ES: Nueve\nEN: Nine\nFR: Neuf\nDE: Neun\nIT: Nove\nPT: Nove"},
            {"mas", "ES: Más / Suma\nEN: Plus\nFR: Plus\nDE: Plus\nIT: Più\nPT: Mais"},
            {"menos", "ES: Menos / Resta\nEN: Minus\nFR: Moins\nDE: Minus\nIT: Meno\nPT: Menos"},
            {"igual", "ES: Igual\nEN: Equals\nFR: Égal\nDE: Gleich\nIT: Uguale\nPT: Igual"}
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
                if (isPanelOpen) ToggleInfoPanel();
            }
            else
            {
                selectedObject = null;
                if (isPanelOpen) ToggleInfoPanel();
            }
        }

        if (selectedObject != null)
        {
            if (touches.Count == 2)
            {
                wasPinching = true;
                isDragging = true;
                var touch2 = touches[1];
                float currentDist = Vector2.Distance(touch1.screenPosition, touch2.screenPosition);
                if (initialTouchDistance == 0f) initialTouchDistance = currentDist;

                float newScale = initialScale * (currentDist / initialTouchDistance);
                newScale = Mathf.Clamp(newScale, minScale, maxScale);
                selectedObject.localScale = Vector3.one * newScale;
            }
            else if (touches.Count == 1 && touch1.phase == TouchPhase.Moved)
            {
                if (wasPinching)
                {
                    wasPinching = false;
                    lastTouchPos = touch1.screenPosition;
                    return;
                }

                float distanceMoved = Vector2.Distance(touchStartPos, touch1.screenPosition);
                if (distanceMoved > tapThreshold) isDragging = true;

                if (isDragging)
                {
                    Vector2 delta = touch1.screenPosition - lastTouchPos;
                    selectedObject.Rotate(Vector3.up, delta.x * rotationSpeed, Space.World);
                }
                lastTouchPos = touch1.screenPosition;
            }

            if (touch1.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Canceled)
            {
                if (!isDragging && !wasPinching) ToggleInfoPanel();
                selectedObject = null;
                isDragging = false;
                wasPinching = false;
                initialTouchDistance = 0f;
            }
        }
    }

    void ToggleInfoPanel()
    {
        if (infoPanel == null || infoText == null) return;
        isPanelOpen = !isPanelOpen;
        infoPanel.SetActive(isPanelOpen);

        if (isPanelOpen && selectedObject != null)
        {
            translationData.TryGetValue(selectedObject.name, out string text);
            infoText.text = string.IsNullOrEmpty(text) ? "Unknown" : text;
        }
    }
}