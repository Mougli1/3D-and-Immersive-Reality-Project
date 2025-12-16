using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class VRMenuController : MonoBehaviour
{
    [Header("Références")]
    public Transform xrRig;             // VR Player
    public GameObject locomotionRoot;   // Locomotion
    public Transform gameStartPoint;    // GameStartPoint
    public GameObject menuRoot;         // MenuRoot ou MenuCanvas

    [Header("UI (Start / Continue / Restart)")]
    public Button continueButton;
    public Button restartButton;
    public TextMeshProUGUI continueLabel; // optionnel (texte du bouton)

    // Permet de relancer la scène et auto-start après Restart
    private static bool autoStartAfterRestart = false;

    private void Start()
    {
        // Bloque la locomotion dès le lancement
        if (locomotionRoot != null)
            locomotionRoot.SetActive(false);

        // Brancher les boutons (si pas déjà fait via l’Inspector)
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        // Si on vient d'un Restart => on démarre direct
        if (autoStartAfterRestart)
        {
            autoStartAfterRestart = false;
            StartGame();
            return;
        }

        // Activer/désactiver Continue selon progression
        bool hasProgress = false;
        if (ProgressManager.Instance != null)
            hasProgress = ProgressManager.Instance.TrashCollected > 0; // simple (mission déchets)

        if (continueButton != null)
            continueButton.interactable = true; // même sans save, ça sert de "Start"

        if (continueLabel != null)
            continueLabel.text = hasProgress ? "Continue" : "Start";
    }

    public void ContinueGame()
    {
        StartGame();
    }

    public void RestartGame()
    {
        // Reset progression (et réécrit une save propre à 0)
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.ResetAllProgress();

        // Recharger la scène pour respawn tous les déchets placés dans la scène
        autoStartAfterRestart = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void StartGame()
    {
        // 1) TP le joueur au point de départ
        if (xrRig != null && gameStartPoint != null)
        {
            xrRig.position = gameStartPoint.position;
            xrRig.rotation = gameStartPoint.rotation;
        }

        // 2) 🔓 Réactiver la locomotion
        if (locomotionRoot != null)
            locomotionRoot.SetActive(true);

        // 3) Masquer le menu
        if (menuRoot != null)
            menuRoot.SetActive(false);
    }
}
