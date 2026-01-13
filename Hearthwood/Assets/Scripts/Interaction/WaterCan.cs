using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class WaterCan : MonoBehaviour
{
    [Header("Arrosage (durée & portée)")]
    public float pourDuration = 3f;
    public float pourRange = 0.7f;

    [Header("Animation (pencher / retour)")]
    public Transform visualToRotate;
    public float tiltAngle = 60f;
    public float tiltInTime = 0.25f;
    public float tiltOutTime = 0.25f;

    [Header("Point de sortie + FX")]
    public Transform pourPoint;
    public ParticleSystem waterParticles;

    private XRGrabInteractable grab;
    private Coroutine pourRoutine;
    private Quaternion visualInitialLocalRot;

    private bool didWaterThisPour = false;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();

        if (grab != null)
        {
            grab.activated.AddListener(OnActivate);
            grab.deactivated.AddListener(OnDeactivate);
        }
    }

    private void Start()
    {
        if (visualToRotate != null)
            visualInitialLocalRot = visualToRotate.localRotation;
        else
            visualInitialLocalRot = transform.localRotation;
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
        if (pourRoutine != null) return;

        didWaterThisPour = false;

        pourRoutine = StartCoroutine(PourSequence());
    }

    private void OnDeactivate(DeactivateEventArgs args)
    {
        if (pourRoutine != null)
        {
            StopCoroutine(pourRoutine);
            pourRoutine = null;
        }

        if (waterParticles != null && waterParticles.isPlaying)
            waterParticles.Stop();

        StartCoroutine(RestoreRotation());
    }

    private IEnumerator PourSequence()
    {
        Transform v = (visualToRotate != null) ? visualToRotate : transform;

        // Pencher
        Quaternion from = visualInitialLocalRot;
        Quaternion to = from * Quaternion.Euler(tiltAngle, 0f, 0f); // penche autour de l'axe local X

        yield return RotateLocalOverTime(v, from, to, tiltInTime);

        // Démarrer les particules
        if (waterParticles != null)
            waterParticles.Play();

        // Arroser pendant X secondes
        float t = 0f;
        while (t < pourDuration)
        {
            t += Time.deltaTime;

            if (!didWaterThisPour)
                DoWaterRaycast();

            yield return null;
        }

        // Stop particules
        if (waterParticles != null && waterParticles.isPlaying)
            waterParticles.Stop();

        // Revenir
        yield return RotateLocalOverTime(v, to, from, tiltOutTime);

        pourRoutine = null;
    }

    private void DoWaterRaycast()
    {
        Vector3 origin = (pourPoint != null) ? pourPoint.position : transform.position;

        // Raycast vertical vers le bas (pluie qui tombe)
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, pourRange))
        {
            SoilPlot plot = hit.collider.GetComponentInParent<SoilPlot>();
            if (plot != null)
            {
                plot.WaterPlant();

                // un seul arrosage pris en compte par appui
                didWaterThisPour = true;
            }
        }
    }

    private IEnumerator RestoreRotation()
    {
        Transform v = (visualToRotate != null) ? visualToRotate : transform;

        // Revenir doucement à la rotation initiale
        yield return RotateLocalOverTime(v, v.localRotation, visualInitialLocalRot, tiltOutTime);
    }

    private IEnumerator RotateLocalOverTime(Transform target, Quaternion a, Quaternion b, float duration)
    {
        if (duration <= 0f)
        {
            target.localRotation = b;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            target.localRotation = Quaternion.Slerp(a, b, k);
            yield return null;
        }

        target.localRotation = b;
    }
}
