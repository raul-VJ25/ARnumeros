using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ObjectManipulation : MonoBehaviour
{
    [Header("UI References")]
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
    private MultipleImageTrackerManager trackerManager;

    void Start()
    {
        if (arCamera == null) arCamera = Camera.main;
        if (infoPanel != null) infoPanel.SetActive(false);

        trackerManager = FindObjectOfType<MultipleImageTrackerManager>();
        InitializeTranslations();
    }

    void InitializeTranslations()
    {
        translationData = new Dictionary<string, string>();
        translationData["0"] = "ES: Cero\nEN: Zero\nFR: Zéro\nDE: Null\nIT: Zero\nPT: Zero\nZH: 零\nJP: 零";
        translationData["1"] = "ES: Uno\nEN: One\nFR: Un\nDE: Eins\nIT: Uno\nPT: Um\nZH: 一\nJP: 一";
        translationData["2"] = "ES: Dos\nEN: Two\nFR: Deux\nDE: Zwei\nIT: Due\nPT: Dois\nZH: 二\nJP: 二";
        translationData["3"] = "ES: Tres\nEN: Three\nFR: Trois\nDE: Drei\nIT: Tre\nPT: Três\nZH: 三\nJP: 三";
        translationData["4"] = "ES: Cuatro\nEN: Four\nFR: Quatre\nDE: Vier\nIT: Quattro\nPT: Quatro\nZH: 四\nJP: 四";
        translationData["5"] = "ES: Cinco\nEN: Five\nFR: Cinq\nDE: Fünf\nIT: Cinque\nPT: Cinco\nZH: 五\nJP: 五";
        translationData["6"] = "ES: Seis\nEN: Six\nFR: Six\nDE: Sechs\nIT: Sei\nPT: Seis\nZH: 六\nJP: 六";
        translationData["7"] = "ES: Siete\nEN: Seven\nFR: Sept\nDE: Sieben\nIT: Sette\nPT: Sete\nZH: 七\nJP: 七";
        translationData["8"] = "ES: Ocho\nEN: Eight\nFR: Huit\nDE: Acht\nIT: Otto\nPT: Oito\nZH: 八\nJP: 八";
        translationData["9"] = "ES: Nueve\nEN: Nine\nFR: Neuf\nDE: Neun\nIT: Nove\nPT: Nove\nZH: 九\nJP: 九";
        translationData["mas"] = "ES: Más / Suma\nEN: Plus\nFR: Plus\nDE: Plus\nIT: Più\nPT: Mais\nZH: 加\nJP: プラス";
        translationData["menos"] = "ES: Menos / Resta\nEN: Minus\nFR: Moins\nDE: Minus\nIT: Meno\nPT: Menos\nZH: 减\nJP: マイナス";
        translationData["igual"] = "ES: Igual\nEN: Equals\nFR: Égal\nDE: Gleich\nIT: Uguale\nPT: Igual\nZH: 等于\nJP: イコール";
    }

    void Update()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            Ray ray = arCamera.ScreenPointToRay(touch.position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Se tocó un objeto 3D
                if (selectedObject == hit.transform)
                {
                    ToggleInfoPanel();
                }
                else
                {
                    SelectObject(hit.transform);
                    if (isPanelOpen) ToggleInfoPanel();
                }

                // Notificar al tracker que hay un objeto tocado
                if (trackerManager != null)
                    trackerManager.SetObjectTouched(true);

                lastTouchPos = touch.position;
            }
            else
            {
                // No se tocó ningún objeto
                DeselectObject();
                if (trackerManager != null)
                    trackerManager.SetObjectTouched(false);
            }
        }

        if (selectedObject != null)
        {
            if (Input.touchCount == 1 && touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.position - lastTouchPos;
                selectedObject.Rotate(Vector3.up, delta.x * rotationSpeed, Space.World);
                lastTouchPos = touch.position;
            }

            if (Input.touchCount == 2)
            {
                Touch touch2 = Input.GetTouch(1);
                float currentDist = Vector2.Distance(touch.position, touch2.position);

                if (initialTouchDistance == 0) initialTouchDistance = currentDist;

                float scaleFactor = currentDist / initialTouchDistance;
                float newScale = initialScale * scaleFactor;
                newScale = Mathf.Clamp(newScale, minScale, maxScale);
                selectedObject.localScale = Vector3.one * newScale;
            }
            else
            {
                initialTouchDistance = 0;
            }
        }
    }

    void SelectObject(Transform obj)
    {
        selectedObject = obj;
        initialScale = obj.localScale.x;
        initialTouchDistance = 0;
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
            string key = selectedObject.name;
            infoText.text = translationData.ContainsKey(key) ? translationData[key] : "Unknown";
        }
    }
}