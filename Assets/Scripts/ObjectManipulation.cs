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
        EnhancedTouchSupport.Enable();

        if (arCamera == null) arCamera = Camera.main;
        if (infoPanel != null) infoPanel.SetActive(false);

        InitializeTranslations();
    }

    void InitializeTranslations()
    {
        translationData = new Dictionary<string, string>
        {
            {"0", "ES: Cero\nEN: Zero\nFR: Zéro\nDE: Null\nIT: Zero\nPT: Zero\nZH: 零\nJP: 零"},
            {"1", "ES: Uno\nEN: One\nFR: Un\nDE: Eins\nIT: Uno\nPT: Um\nZH: 一\nJP: 一"},
            {"2", "ES: Dos\nEN: Two\nFR: Deux\nDE: Zwei\nIT: Due\nPT: Dois\nZH: 二\nJP: 二"},
            {"3", "ES: Tres\nEN: Three\nFR: Trois\nDE: Drei\nIT: Tre\nPT: Três\nZH: 三\nJP: 三"},
            {"4", "ES: Cuatro\nEN: Four\nFR: Quatre\nDE: Vier\nIT: Quattro\nPT: Quatro\nZH: 四\nJP: 四"},
            {"5", "ES: Cinco\nEN: Five\nFR: Cinq\nDE: Fünf\nIT: Cinque\nPT: Cinco\nZH: 五\nJP: 五"},
            {"6", "ES: Seis\nEN: Six\nFR: Six\nDE: Sechs\nIT: Sei\nPT: Seis\nZH: 六\nJP: 六"},
            {"7", "ES: Siete\nEN: Seven\nFR: Sept\nDE: Sieben\nIT: Sette\nPT: Sete\nZH: 七\nJP: 七"},
            {"8", "ES: Ocho\nEN: Eight\nFR: Huit\nDE: Acht\nIT: Otto\nPT: Oito\nZH: 八\nJP: 八"},
            {"9", "ES: Nueve\nEN: Nine\nFR: Neuf\nDE: Neun\nIT: Nove\nPT: Nove\nZH: 九\nJP: 九"},
            {"mas", "ES: Más / Suma\nEN: Plus\nFR: Plus\nDE: Plus\nIT: Più\nPT: Mais\nZH: 加\nJP: プラス"},
            {"menos", "ES: Menos / Resta\nEN: Minus\nFR: Moins\nDE: Minus\nIT: Meno\nPT: Menos\nZH: 减\nJP: マイナス"},
            {"igual", "ES: Igual\nEN: Equals\nFR: Égal\nDE: Gleich\nIT: Uguale\nPT: Igual\nZH: 等于\nJP: イコール"}
        };
    }

    void Update()
    {
        var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;

        if (touches.Count == 0) return;

        var touch1 = touches[0];

        if (touch1.phase == TouchPhase.Began)
        {
            Ray ray = arCamera.ScreenPointToRay(touch1.screenPosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (selectedObject == hit.transform) ToggleInfoPanel();
                else SelectObject(hit.transform);
                lastTouchPos = touch1.screenPosition;
            }
            else DeselectObject();
        }

        if (selectedObject != null)
        {
            if (touches.Count == 1 && touch1.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch1.screenPosition - lastTouchPos;
                selectedObject.Rotate(Vector3.up, delta.x * rotationSpeed, Space.World);
                lastTouchPos = touch1.screenPosition;
            }

            if (touches.Count == 2)
            {
                var touch2 = touches[1];
                float currentDist = Vector2.Distance(touch1.screenPosition, touch2.screenPosition);

                if (initialTouchDistance == 0f) initialTouchDistance = currentDist;

                float newScale = initialScale * (currentDist / initialTouchDistance);
                newScale = Mathf.Clamp(newScale, minScale, maxScale);
                selectedObject.localScale = Vector3.one * newScale;
            }
            else initialTouchDistance = 0f;
        }
    }

    void SelectObject(Transform obj)
    {
        selectedObject = obj;
        initialScale = obj.localScale.x;
        initialTouchDistance = 0f;
        if (isPanelOpen) ToggleInfoPanel();
    }

    void DeselectObject()
    {
        selectedObject = null;
        if (isPanelOpen) ToggleInfoPanel();
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