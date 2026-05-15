using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class MultipleImageTrackerManager : MonoBehaviour
{
    [Header("🎛️ Configuración de Seguimiento")]
    [SerializeField] private List<GameObject> objectsToSpawn = new List<GameObject>();

    [Tooltip("Velocidad de suavizado (5-15 recomendado). Mayor = más rígido, Menor = más fluido")]
    [Range(1f, 20f)]
    [SerializeField] private float smoothingSpeed = 8f;

    [Tooltip("Desplazamiento respecto a la tarjeta: X=Lateral, Y=Arriba, Z=Hacia ti")]
    [SerializeField] private Vector3 markerOffset = new Vector3(0f, 0.08f, 0.18f);

    [Header("🖥️ Interfaz de Usuario")]
    [SerializeField] private GameObject detectionStatusPanel;
    [SerializeField] private TextMeshProUGUI statusText;

    private ARTrackedImageManager _trackedImageManager;
    private Dictionary<string, GameObject> _arObjects;
    private Dictionary<string, bool> _activeStates;

    void Start()
    {
        _trackedImageManager = GetComponent<ARTrackedImageManager>();
        if (_trackedImageManager == null)
        {
            Debug.LogError("❌ Falta ARTrackedImageManager en el XR Origin.");
            return;
        }

        _arObjects = new Dictionary<string, GameObject>();
        _activeStates = new Dictionary<string, bool>();

        SetUpSceneElements();
        _trackedImageManager.trackablesChanged.AddListener(OnImageTrackedChanged);
        UpdateDetectionUI();
    }

    private void OnDestroy()
    {
        if (_trackedImageManager != null)
            _trackedImageManager.trackablesChanged.RemoveListener(OnImageTrackedChanged);
    }

    private void OnImageTrackedChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
            UpdateTrackedImages(trackedImage);

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
            UpdateTrackedImages(trackedImage);

        foreach (var trackedImage in eventArgs.removed)
        {
            if (trackedImage.Value != null)
            {
                string imageName = trackedImage.Value.referenceImage.name;
                if (_arObjects.ContainsKey(imageName))
                {
                    _arObjects[imageName].SetActive(false);
                    _activeStates[imageName] = false;
                }
            }
        }
        UpdateDetectionUI();
    }

    private void UpdateTrackedImages(ARTrackedImage trackedImage)
    {
        if (trackedImage == null) return;

        string imageName = trackedImage.referenceImage.name;
        if (!_arObjects.ContainsKey(imageName)) return;

        // Si el tracking se degrada, ocultar
        if (trackedImage.trackingState is TrackingState.Limited or TrackingState.None)
        {
            _arObjects[imageName].SetActive(false);
            _activeStates[imageName] = false;
            return;
        }

        // Activar objeto
        _arObjects[imageName].SetActive(true);
        _activeStates[imageName] = true;

        // Calcular posición y rotación objetivo con offset relativo a la tarjeta
        Vector3 targetPos = trackedImage.transform.position
                          + (trackedImage.transform.right * markerOffset.x)
                          + (trackedImage.transform.up * markerOffset.y)
                          + (trackedImage.transform.forward * markerOffset.z);

        Quaternion targetRot = trackedImage.transform.rotation;

        // Aplicar suavizado (Lerp para posición, Slerp para rotación)
        Transform objTransform = _arObjects[imageName].transform;
        float smoothFactor = smoothingSpeed * Time.deltaTime;

        objTransform.position = Vector3.Lerp(objTransform.position, targetPos, smoothFactor);
        objTransform.rotation = Quaternion.Slerp(objTransform.rotation, targetRot, smoothFactor);
    }

    private void UpdateDetectionUI()
    {
        if (statusText == null || detectionStatusPanel == null) return;

        List<string> activeNames = new List<string>();
        foreach (var kvp in _activeStates)
        {
            if (kvp.Value) activeNames.Add(kvp.Key);
        }

        if (activeNames.Count > 0)
        {
            detectionStatusPanel.GetComponent<UnityEngine.UI.Image>().color = new Color32(30, 100, 30, 220);
            statusText.text = $"✅ Detectando:\n{string.Join(", ", activeNames)}";
            statusText.color = Color.white;
        }
        else
        {
            detectionStatusPanel.GetComponent<UnityEngine.UI.Image>().color = new Color32(100, 80, 20, 220);
            statusText.text = " Buscando marcadores...";
            statusText.color = new Color(1, 0.9f, 0.3f);
        }
    }

    private void SetUpSceneElements()
    {
        foreach (GameObject prefab in objectsToSpawn)
        {
            if (prefab == null) continue;

            GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            instance.name = prefab.name;
            instance.SetActive(false);

            if (!_arObjects.ContainsKey(instance.name))
            {
                _arObjects.Add(instance.name, instance);
                _activeStates.Add(instance.name, false);
            }
            else
            {
                Destroy(instance);
            }
        }
    }
}