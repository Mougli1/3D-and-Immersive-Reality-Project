using System.Collections;
using TMPro;
using UnityEngine;

public class ToastSystem : MonoBehaviour
{
    public static ToastSystem Instance { get; private set; }

    [SerializeField] private GameObject toastPanel;
    [SerializeField] private TextMeshProUGUI toastText;

    Coroutine routine;

    private void Awake()
    {
        Instance = this;
        if (toastPanel) toastPanel.SetActive(false);
    }

    public void Show(string msg, float seconds)
    {
        if (!toastPanel || !toastText) return;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(CoShow(msg, seconds));
    }

    public void ShowPersistent(string msg)
    {
        if (!toastPanel || !toastText) return;

        if (routine != null) StopCoroutine(routine);
        routine = null;

        toastText.SetText(msg);
        toastPanel.SetActive(true);
    }

    public void Hide()
    {
        if (!toastPanel) return;

        if (routine != null) StopCoroutine(routine);
        routine = null;

        toastPanel.SetActive(false);
    }

    IEnumerator CoShow(string msg, float seconds)
    {
        toastText.SetText(msg);
        toastPanel.SetActive(true);
        yield return new WaitForSeconds(seconds);
        toastPanel.SetActive(false);
    }
}
