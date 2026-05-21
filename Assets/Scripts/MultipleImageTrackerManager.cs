using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class MultipleImageTrackerManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> objectsToSpawn = new List<GameObject>();
    [SerializeField] private float smoothingSpeed = 18f;
    [SerializeField] private Vector3 markerOffset = new Vector3(0f, 0.08f, 0.12f);
    [SerializeField] private float positionThreshold = 0.003f;
    [SerializeField] private GameObject statusPanel;
    [SerializeField] private TextMeshProUGUI statusText;

    private static readonly Quaternion initialRotation = Quaternion.Euler(-90f, 180f, 0f);

    private ARTrackedImageManager trackedImageManager;
    private Dictionary<string, GameObject> arObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, bool> activeStates = new Dictionary<string, bool>();
    private Dictionary<string, float> initialScales = new Dictionary<string, float>();
    private Dictionary<string, float> lastActiveTime = new Dictionary<string, float>();

    private const float RESET_DELAY = 5f;

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
    }

    void UpdateTrackedImages(ARTrackedImage trackedImage)
    {
        if (trackedImage == null) return;
        string name = trackedImage.referenceImage.name;
        if (!arObjects.ContainsKey(name)) return;

        bool isValid = trackedImage.trackingState == TrackingState.Tracking;

        if (isValid)
        {
            bool wasInactive = !activeStates[name];
            arObjects[name].SetActive(true);
            activeStates[name] = true;
            lastActiveTime[name] = Time.time;

            Transform t = arObjects[name].transform;

            if (wasInactive)
            {
                t.rotation = trackedImage.transform.rotation * initialRotation;
                t.localScale = Vector3.one * initialScales[name];
            }

            Vector3 targetPos = trackedImage.transform.position
                              + trackedImage.transform.right * markerOffset.x
                              + trackedImage.transform.up * markerOffset.y
                              + trackedImage.transform.forward * markerOffset.z;

            if (Vector3.Distance(t.position, targetPos) > positionThreshold)
            {
                float factor = 1f - Mathf.Exp(-smoothingSpeed * Time.deltaTime);
                t.position = Vector3.Lerp(t.position, targetPos, factor);
            }
            else
            {
                t.position = targetPos;
            }
        }
        else
        {
            arObjects[name].SetActive(false);
            activeStates[name] = false;
        }
        UpdateStatusUI();
    }

    void CheckResetTimers()
    {
        foreach (var kvp in activeStates)
        {
            string name = kvp.Key;
            if (!kvp.Value && lastActiveTime.ContainsKey(name))
            {
                if (Time.time - lastActiveTime[name] > RESET_DELAY)
                {
                    Transform t = arObjects[name].transform;
                    t.localScale = Vector3.one * initialScales[name];
                    arObjects[name].SetActive(false);
                    lastActiveTime[name] = Time.time;
                }
            }
        }
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

            GameObject instance = Instantiate(prefab, Vector3.zero, initialRotation);
            instance.name = prefab.name;
            instance.SetActive(false);

            if (!arObjects.ContainsKey(instance.name))
            {
                arObjects.Add(instance.name, instance);
                activeStates.Add(instance.name, false);
                initialScales.Add(instance.name, instance.transform.localScale.x);
                lastActiveTime.Add(instance.name, Time.time);
            }
            else
            {
                Destroy(instance);
            }
        }
    }

    void Update()
    {
        CheckResetTimers();
    }
}