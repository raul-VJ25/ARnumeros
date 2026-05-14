using UnityEngine;
using TMPro;

public class ObjectSelector : MonoBehaviour
{
    public Camera AR_Camera;

    private GameObject selectedObject;

    public TextMeshProUGUI cajaTexto;

    private void Update()
    {
        if (selectedObject != null)
        {
            cajaTexto.text = selectedObject.name;
        }

        if (Input.touchCount > 0)
        {

            Touch toque = Input.GetTouch(0);

            switch (toque.phase)
            {
                case TouchPhase.Began:
                    Ray rayo = AR_Camera.ScreenPointToRay(toque.position);

                    RaycastHit hit;

                    if (Physics.Raycast(rayo, out hit))
                    {
                        if (hit.collider != null)
                        {
                            selectedObject = hit.collider.gameObject;
                        }
                    }

                    break;

                case TouchPhase.Ended:

                    break;

                case TouchPhase.Moved:

                    break;
            }


        }
    }
}
