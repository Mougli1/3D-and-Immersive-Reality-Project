using TMPro;
using UnityEngine;

public class TrashBinInfoZone : MonoBehaviour
{
    [Header("UI à afficher")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TMP_Text infoText;

    [Header("Texte")]
    [TextArea(2, 6)]
    [SerializeField] private string message;

    [Header("Options")]
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private Transform panelToRotate;
    [SerializeField] private bool debugLogs = false;

    private void Awake()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);

        if (infoText != null && !string.IsNullOrEmpty(message))
            infoText.text = message;
    }

    private void LateUpdate()
    {
        if (!faceCamera || infoPanel == null || !infoPanel.activeSelf) return;

        var cam = Camera.main;
        if (cam == null) return;

        Transform t = panelToRotate != null ? panelToRotate : infoPanel.transform;

        Vector3 lookPos = cam.transform.position;
        lookPos.y = t.position.y;
        t.LookAt(lookPos);
        t.Rotate(0f, 180f, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (debugLogs) Debug.Log("[BinInfo] Enter -> show");
        if (infoPanel != null) infoPanel.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (debugLogs) Debug.Log("[BinInfo] Exit -> hide");
        if (infoPanel != null) infoPanel.SetActive(false);
    }
}
