using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class WaterCan : MonoBehaviour
{
    [Header("Arrosage (durée & portée)")]
    public float pourDuration = 3f;     // durée d'arrosage automatique
    public float pourRange = 0.7f;      // distance max sous le PourPoint

    [Header("Animation (pencher / retour)")]
    public Transform visualToRotate;    // le child "Model" (IMPORTANT)
    public float tiltAngle = 60f;       // angle de penchement
    public float tiltInTime = 0.25f;    // temps pour se pencher
    public float tiltOutTime = 0.25f;   // temps pour revenir

    [Header("Point de sortie + FX")]
    public Transform pourPoint;         // le child "PourPoint" au bout du bec
    public ParticleSystem waterParticles;

    private XRGrabInteractable grab;
    private Coroutine pourRoutine;
    private Quaternion visualInitialLocalRot;

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
        // On mémorise la rotation "normale" du visuel
        if (visualToRotate != null)
            visualInitialLocalRot = visualToRotate.localRotation;
        else
            visualInitialLocalRot = transform.localRotation; // fallback si vous ne settez pas visualToRotate
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
        // Si déjà en train d'arroser, on ignore
        if (pourRoutine != null) return;

        pourRoutine = StartCoroutine(PourSequence());
    }

    private void OnDeactivate(DeactivateEventArgs args)
    {
        // Option : si vous relâchez avant la fin des 3 sec, on stop et on revient
        if (pourRoutine != null)
        {
            StopCoroutine(pourRoutine);
            pourRoutine = null;
        }

        // Stop FX + retour à la rotation normale
        if (waterParticles != null && waterParticles.isPlaying)
            waterParticles.Stop();

        StartCoroutine(RestoreRotation());
    }

    private IEnumerator PourSequence()
    {
        Transform v = (visualToRotate != null) ? visualToRotate : transform;

        // 1) Pencher
        Quaternion from = visualInitialLocalRot;
        Quaternion to = from * Quaternion.Euler(tiltAngle, 0f, 0f); // penche autour de l'axe local X

        yield return RotateLocalOverTime(v, from, to, tiltInTime);

        // 2) Démarrer les particules
        if (waterParticles != null)
            waterParticles.Play();

        // 3) Arroser pendant X secondes
        float t = 0f;
        while (t < pourDuration)
        {
            t += Time.deltaTime;

            DoWaterRaycast();

            yield return null;
        }

        // 4) Stop particules
        if (waterParticles != null && waterParticles.isPlaying)
            waterParticles.Stop();

        // 5) Revenir
        yield return RotateLocalOverTime(v, to, from, tiltOutTime);

        pourRoutine = null;
    }

    private void DoWaterRaycast()
    {
        Vector3 origin = (pourPoint != null) ? pourPoint.position : transform.position;

        // Raycast vertical vers le bas (pluie qui tombe)
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, pourRange))
        {
            // IMPORTANT : si votre collider touché est sur un enfant,
            // SoilPlot peut être sur le parent → GetComponentInParent est plus robuste
            SoilPlot plot = hit.collider.GetComponentInParent<SoilPlot>();
            if (plot != null)
            {
                plot.WaterPlant(); // votre SoilPlot gère déjà "1 seule fois"
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
