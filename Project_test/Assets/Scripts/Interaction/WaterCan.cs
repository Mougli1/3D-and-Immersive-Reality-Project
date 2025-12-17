using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class WaterCan : MonoBehaviour
{
    [Header("Raycast vers le sol")]
    public float pourRange = 0.7f;   // distance sous l’arrosoir

    private XRGrabInteractable grab;
    private bool isPouring = false;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();

        // On écoute l'événement "activate" de l'interactable (gâchette)
        grab.activated.AddListener(OnActivate);
        grab.deactivated.AddListener(OnDeactivate);
    }

    private void OnDestroy()
    {
        if (grab != null)
        {
            grab.activated.RemoveListener(OnActivate);
            grab.deactivated.RemoveListener(OnDeactivate);
        }
    }

    private void OnActivate(ActivateEventArgs args)
    {
        isPouring = true;
    }

    private void OnDeactivate(DeactivateEventArgs args)
    {
        isPouring = false;
    }

    private void Update()
    {
        if (!isPouring) return;

        // On regarde droit sous l'arrosoir
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, pourRange))
        {
            SoilPlot plot = hit.collider.GetComponent<SoilPlot>();
            if (plot != null)
            {
                plot.WaterPlant();   // 💧 arrosage de cette parcelle
            }
        }
    }
}