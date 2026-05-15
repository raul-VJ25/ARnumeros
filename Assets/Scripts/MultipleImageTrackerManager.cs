using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class MultipleImageTrackerManager : MonoBehaviour
{
    [Header("Tracking Configuration")]
    [SerializeField] private List<GameObject> objectsToSpawn = new List<GameObject>();
    [Range(5f, 25f)]
    [SerializeField] private float smoothingSpeed = 12f;
    [SerializeField] private Vector3 markerOffset = new Vector3(0f, 0.08f, 0.15f);

    [Header("Status UI")]
    [SerializeField] private GameObject statusPanel;
    [SerializeField] private TextMeshProUGUI statusText;

    private ARTrackedImageManager trackedImageManager;
    private Dictionary<string, GameObject> arObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, bool> activeStates = new Dictionary<string, bool>();
    private bool isObjectTouched = false;

    void Start()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
        if (trackedImageManager == null) return;

        SetUpSceneElements();
        trackedImageManager.trackablesChanged.AddListener(OnImageTrackedChanged);
        UpdateStatusUI();
    }

    void OnDestroy()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackablesChanged.RemoveListener(OnImageTrackedChanged);
    }

    void Update()
    {
        // Solo mostrar azul si ObjectManipulation detectó un toque en objeto
        if (isObjectTouched)
        {
            statusPanel.GetComponent<UnityEngine.UI.Image>().color = new Color32(20, 20, 150, 220);
            statusText.text = "Object Selected";
            statusText.color = Color.white;
        }
        else
        {
            UpdateStatusUI();
        }
    }

    public void SetObjectTouched(bool touched)
    {
        isObjectTouched = touched;
    }

    void OnImageTrackedChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var img in eventArgs.added) UpdateTrackedImages(img);
        foreach (var img in eventArgs.updated) UpdateTrackedImages(img);

        foreach (var img in eventArgs.removed)
        {
            if (img.Value != null)
            {
                string name = img.Value.referenceImage.name;
                if (arObjects.ContainsKey(name))
                {
                    arObjects[name].SetActive(false);
                    activeStates[name] = false;
                }
            }
        }
        UpdateStatusUI();
    }

    void UpdateTrackedImages(ARTrackedImage trackedImage)
    {
        if (trackedImage == null) return;
        string name = trackedImage.referenceImage.name;
        if (!arObjects.ContainsKey(name)) return;

        if (trackedImage.trackingState is TrackingState.Limited or TrackingState.None)
        {
            arObjects[name].SetActive(false);
            activeStates[name] = false;
            return;
        }

        arObjects[name].SetActive(true);
        activeStates[name] = true;

        Vector3 targetPos = trackedImage.transform.position
                          + trackedImage.transform.right * markerOffset.x
                          + trackedImage.transform.up * markerOffset.y
                          + trackedImage.transform.forward * markerOffset.z;

        Transform t = arObjects[name].transform;
        float factor = 1f - Mathf.Exp(-smoothingSpeed * Time.deltaTime);
        t.position = Vector3.Lerp(t.position, targetPos, factor);
        t.rotation = Quaternion.Slerp(t.rotation, trackedImage.transform.rotation, factor);
    }

    void UpdateStatusUI()
    {
        if (statusPanel == null || statusText == null) return;

        List<string> active = new List<string>();
        foreach (var kvp in activeStates) if (kvp.Value) active.Add(kvp.Key);

        if (active.Count > 0)
        {
            statusPanel.GetComponent<UnityEngine.UI.Image>().color = new Color32(30, 100, 30, 220);
            statusText.text = "Detecting: " + string.Join(", ", active);
            statusText.color = Color.white;
        }
        else
        {
            statusPanel.GetComponent<UnityEngine.UI.Image>().color = new Color32(100, 80, 20, 220);
            statusText.text = "Searching for markers...";
            statusText.color = new Color(1f, 0.9f, 0.3f);
        }
    }

    void SetUpSceneElements()
    {
        foreach (GameObject prefab in objectsToSpawn)
        {
            if (prefab == null) continue;
            GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            instance.name = prefab.name;
            instance.SetActive(false);
            if (!arObjects.ContainsKey(instance.name))
            {
                arObjects.Add(instance.name, instance);
                activeStates.Add(instance.name, false);
            }
            else Destroy(instance);
        }
    }
}