using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections.Generic;

// Alias para evitar conflictos de nombres con el sistema antiguo
using ETouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using ETouchPhase = UnityEngine.InputSystem.TouchPhase;

public class ObjectManipulation : MonoBehaviour
{
    [Header("UI Configuration")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private Camera arCamera;

    [Header("Interaction Settings")]
    [SerializeField] private float rotationSpeed = 0.5f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 2.5f;

    private Transform selectedObject;
    private Vector2 lastTouchPos;
    private float initialTouchDistance;
    private float initialScale;
    private bool isPanelOpen;
    private Dictionary<string, string> translationData;

    void Start()
    {
        // CRÍTICO: Habilitar el soporte táctil mejorado para New Input System
        EnhancedTouchSupport.Enable();

        if (arCamera == null) arCamera = Camera.main;
        if (infoPanel != null) infoPanel.SetActive(false);

        InitializeTranslations();
    }

    void OnDestroy()
    {
        EnhancedTouchSupport.Disable();
    }

    void InitializeTranslations()
    {
        translationData = new Dictionary<string, string>();
        translationData["Number-0"] = "ES: Cero\nEN: Zero\nFR: Zéro\nDE: Null\nIT: Zero\nPT: Zero\nZH: 零\nJP: 零";
        translationData["Number-1"] = "ES: Uno\nEN: One\nFR: Un\nDE: Eins\nIT: Uno\nPT: Um\nZH: 一\nJP: 一";
        translationData["Number-2"] = "ES: Dos\nEN: Two\nFR: Deux\nDE: Zwei\nIT: Due\nPT: Dois\nZH: 二\nJP: 二";
        translationData["Number-3"] = "ES: Tres\nEN: Three\nFR: Trois\nDE: Drei\nIT: Tre\nPT: Três\nZH: 三\nJP: 三";
        translationData["Number-4"] = "ES: Cuatro\nEN: Four\nFR: Quatre\nDE: Vier\nIT: Quattro\nPT: Quatro\nZH: 四\nJP: 四";
        translationData["Number-5"] = "ES: Cinco\nEN: Five\nFR: Cinq\nDE: Fünf\nIT: Cinque\nPT: Cinco\nZH: 五\nJP: 五";
        translationData["Number-6"] = "ES: Seis\nEN: Six\nFR: Six\nDE: Sechs\nIT: Sei\nPT: Seis\nZH: 六\nJP: 六";
        translationData["Number-7"] = "ES: Siete\nEN: Seven\nFR: Sept\nDE: Sieben\nIT: Sette\nPT: Sete\nZH: 七\nJP: 七";
        translationData["Number-8"] = "ES: Ocho\nEN: Eight\nFR: Huit\nDE: Acht\nIT: Otto\nPT: Oito\nZH: 八\nJP: 八";
        translationData["Number-9"] = "ES: Nueve\nEN: Nine\nFR: Neuf\nDE: Neun\nIT: Nove\nPT: Nove\nZH: 九\nJP: 九";
        translationData["Symbol-Plus"] = "ES: Más / Suma\nEN: Plus\nFR: Plus\nDE: Plus\nIT: Più\nPT: Mais\nZH: 加\nJP: プラス";
        translationData["Symbol-Minus"] = "ES: Menos / Resta\nEN: Minus\nFR: Moins\nDE: Minus\nIT: Meno\nPT: Menos\nZH: 减\nJP: マイナス";
        translationData["Symbol-Equals"] = "ES: Igual\nEN: Equals\nFR: Égal\nDE: Gleich\nIT: Uguale\nPT: Igual\nZH: 等于\nJP: イコール";
    }

    void Update()
    {
        // Obtener toques activos del nuevo sistema
        var activeTouches = ETouch.activeTouches;
        if (activeTouches.Count == 0) return;

        // CASO 1: Un solo dedo (Selección y Rotación)
        if (activeTouches.Count == 1)
        {
            var touch = activeTouches[0];

            // TAP: Selección
            if (touch.phase == ETouchPhase.Began)
            {
                Ray ray = arCamera.ScreenPointToRay(touch.screenPosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    // Si tocas el mismo objeto, abre/cierra info. Si tocas otro, selecciónalo.
                    if (selectedObject == hit.transform)
                    {
                        ToggleInfoPanel();
                    }
                    else
                    {
                        SelectObject(hit.transform);
                    }
                    lastTouchPos = touch.screenPosition;
                }
                else
                {
                    // Si tocas el fondo, deseleccionar
                    DeselectObject();
                }
            }
            // DRAG: Rotación
            else if (touch.phase == ETouchPhase.Moved && selectedObject != null)
            {
                Vector2 delta = touch.screenPosition - lastTouchPos;
                selectedObject.Rotate(Vector3.up, delta.x * rotationSpeed, Space.World);
                lastTouchPos = touch.screenPosition;
            }
        }
        // CASO 2: Dos dedos (Pellizco / Pinch para Escalar)
        else if (activeTouches.Count == 2)
        {
            var touch1 = activeTouches[0];
            var touch2 = activeTouches[1];

            if (selectedObject != null)
            {
                float currentDistance = Vector2.Distance(touch1.screenPosition, touch2.screenPosition);

                // Guardar distancia inicial al empezar el pellizco
                if (initialTouchDistance == 0)
                {
                    initialTouchDistance = currentDistance;
                }
                else
                {
                    float scaleFactor = currentDistance / initialTouchDistance;
                    float newScale = initialScale * scaleFactor;
                    newScale = Mathf.Clamp(newScale, minScale, maxScale);
                    selectedObject.localScale = Vector3.one * newScale;
                }
            }
        }
        else
        {
            // Resetear distancia si hay más o menos de 2 dedos
            initialTouchDistance = 0;
        }
    }

    void SelectObject(Transform obj)
    {
        selectedObject = obj;
        initialScale = obj.localScale.x;
        initialTouchDistance = 0; // Resetear pinch
        if (isPanelOpen) ToggleInfoPanel(); // Cerrar panel si estaba abierto
    }

    void DeselectObject()
    {
        selectedObject = null;
        if (isPanelOpen) ToggleInfoPanel(); // Cerrar panel al deseleccionar
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