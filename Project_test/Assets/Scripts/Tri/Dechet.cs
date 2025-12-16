using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class Dechet : MonoBehaviour
{
    public enum TypeDechet { Papier, Emballages, Verre, Organique }
    public TypeDechet type;

    [Header("Infos UI")]
    public string dechetName = "Déchet";

    [Tooltip("Affiche un indice de tri (jaune/verte/compost) quand on attrape l'objet.")]
    [SerializeField] private bool showTriHint = true;

    [Header("Save")]
    [SerializeField] private string dechetId;
    public string DechetId => dechetId;

    [Tooltip("Si coché, l'objet se détruit au chargement s'il a déjà été trié (Continue).")]
    [SerializeField] private bool despawnIfAlreadySorted = true;

    private XRGrabInteractable grab;
    private bool sorted = false;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(dechetId))
            dechetId = System.Guid.NewGuid().ToString();
    }

    [ContextMenu("Regenerate ID")]
    private void RegenerateId()
    {
        dechetId = System.Guid.NewGuid().ToString();
    }
#endif

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        if (grab != null)
        {
            grab.selectEntered.AddListener(OnGrab);
            grab.selectExited.AddListener(OnRelease);
        }
    }

    private void OnDisable()
    {
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(OnGrab);
            grab.selectExited.RemoveListener(OnRelease);
        }
    }

    private IEnumerator Start()
    {
        if (!despawnIfAlreadySorted) yield break;

        while (ProgressManager.Instance == null)
            yield return null;

        if (!string.IsNullOrEmpty(dechetId) && ProgressManager.Instance.IsTrashSorted(dechetId))
            Destroy(gameObject);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (sorted) return;

        string msg = $"À trier : {dechetName}";
        if (showTriHint) msg += $"\n→ {GetTriHint()}";

        ToastSystem.Instance?.ShowPersistent(msg);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (sorted) return; // si on est en train d’être trié/détruit, on ne coupe pas le toast “réussite”
        ToastSystem.Instance?.Hide();
    }

    // Appelé par la poubelle quand le tri est validé
    public void NotifySorted()
    {
        sorted = true;
        ToastSystem.Instance?.Hide(); // coupe le “toast de tenue” si besoin
    }

    private string GetTriHint()
    {
        switch (type)
        {
            case TypeDechet.Verre:       return "Poubelle verre (verte)";
            case TypeDechet.Organique:   return "Compost / biodéchets";
            case TypeDechet.Papier:
            case TypeDechet.Emballages:
            default:                     return "Poubelle jaune (emballages/papiers)";
        }
    }
}
