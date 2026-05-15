using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ObjectManipulation : MonoBehaviour
{
    private Transform selectedObject;
    private Vector2 lastTouchPosition;
    private float initialTouchDistance;
    private float initialScale;
    private Camera arCamera;

    // Velocidades ajustables
    [SerializeField] private float rotationSpeed = 0.5f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 3.0f;

    void Start()
    {
        // Buscar la cámara AR automáticamente
        arCamera = Camera.main;
        if (arCamera == null)
        {
            Debug.LogError("No se encontró la cámara AR!");
        }
    }

    void Update()
    {
        HandleTouchInput();
    }

    void HandleTouchInput()
    {
        // Si hay toques en pantalla
        if (Input.touchCount > 0)
        {
            Touch touch1 = Input.GetTouch(0);

            // Detectar qué objeto fue tocado
            if (touch1.phase == TouchPhase.Began)
            {
                Ray ray = arCamera.ScreenPointToRay(touch1.position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    selectedObject = hit.transform;
                    lastTouchPosition = touch1.position;
                    initialTouchDistance = 0;
                    initialScale = selectedObject.localScale.x;

                    Debug.Log($"Objeto seleccionado: {selectedObject.name}");
                }
            }

            // Gestos de pellizco (dos dedos) para escalar
            if (Input.touchCount == 2 && selectedObject != null)
            {
                Touch touch2 = Input.GetTouch(1);

                float currentTouchDistance = Vector2.Distance(touch1.position, touch2.position);

                if (initialTouchDistance == 0)
                {
                    initialTouchDistance = currentTouchDistance;
                }

                float scaleFactor = currentTouchDistance / initialTouchDistance;
                float newScale = Mathf.Clamp(initialScale * scaleFactor, minScale, maxScale);

                selectedObject.localScale = Vector3.one * newScale;
            }

            // Rotación con un dedo
            if (selectedObject != null && touch1.phase == TouchPhase.Moved)
            {
                Vector2 touchDelta = touch1.position - lastTouchPosition;

                // Rotar sobre el eje Y
                selectedObject.Rotate(Vector3.up, touchDelta.x * rotationSpeed, Space.World);

                lastTouchPosition = touch1.position;
            }
        }
    }
}