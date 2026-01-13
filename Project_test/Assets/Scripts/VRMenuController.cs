using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class VRMenuController : MonoBehaviour
{
    [Header("Références")]
    public Transform xrRig; // VR Player
    public GameObject locomotionRoot;
    public Transform gameStartPoint;
    public GameObject menuRoot; // MenuRoot ou MenuCanvas

    [Header("UI (Start / Continue / Restart)")]
    public Button continueButton;
    public Button restartButton;
    public TextMeshProUGUI continueLabel;

    [Header("Menu")]
    public Transform menuSpawnPoint;
    public GameObject firstMenuPanel;
    private Vector3 menuSpawnPos;
    private Quaternion menuSpawnRot;
    private bool menuSpawnCaptured = false;

    [Header("XR Camera")]
    public Transform xrCamera; // Main Camera

    private Vector3 menuCamPos;
    private float menuCamYaw;

    // Permet de relancer la scène et auto-start après Restart
    private static bool autoStartAfterRestart = false;

    private void Start()
    {

        if (!menuSpawnCaptured)
        {
            if (menuSpawnPoint != null)
            {
                menuCamPos = menuSpawnPoint.position;
                menuCamYaw = menuSpawnPoint.eulerAngles.y;
            }
            else if (xrCamera != null)
            {
                menuCamPos = xrCamera.position;
                menuCamYaw = xrCamera.eulerAngles.y;
            }
            else if (xrRig != null)
            {
                menuCamPos = xrRig.position;
                menuCamYaw = xrRig.eulerAngles.y;
            }

            menuSpawnCaptured = true;
        }

        // Bloque la locomotion dès le lancement
        if (locomotionRoot != null)
            locomotionRoot.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        // Si Restart, on démarre direct
        if (autoStartAfterRestart)
        {
            autoStartAfterRestart = false;
            StartGame();
            return;
        }

        // Activer/désactiver Continue selon progression
        bool hasProgress = false;
        if (ProgressManager.Instance != null)
            hasProgress = ProgressManager.Instance.TrashCollected > 0;

        if (continueButton != null)
            continueButton.interactable = true;

        if (continueLabel != null)
            continueLabel.text = hasProgress ? "Continue" : "Start";
    }

    public void ContinueGame()
    {
        StartGame();
    }

    public void RestartGame()
    {
        // Reset progression (réécrit une save propre à 0)
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.ResetAllProgress();

        // Recharge la scène pour respawn tous les déchets placés dans la scène
        autoStartAfterRestart = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void StartGame()
    {
        //TP le joueur au point de départ
        if (xrRig != null && gameStartPoint != null)
        {
            xrRig.position = gameStartPoint.position;
            xrRig.rotation = gameStartPoint.rotation;
        }

        // Réactive la locomotion
        if (locomotionRoot != null)
            locomotionRoot.SetActive(true);

        // Masque le menu
        if (menuRoot != null)
            menuRoot.SetActive(false);
    }

    public void ExitToMainMenu()
    {
        // Bloque locomotion 
        if (locomotionRoot != null)
            locomotionRoot.SetActive(false);

        // Revenir au spawn du menu
        if (xrRig != null)
        {
            var cc = xrRig.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;

            MoveRigSoCameraIsAt(menuCamPos, menuCamYaw);

            if (cc) cc.enabled = true;
        }

        // Réaffiche le menu
        if (menuRoot != null)
            menuRoot.SetActive(true);

        if (firstMenuPanel != null)
        {
            var parent = firstMenuPanel.transform.parent;
            if (parent != null)
            {
                for (int i = 0; i < parent.childCount; i++)
                    parent.GetChild(i).gameObject.SetActive(false);
            }
            firstMenuPanel.SetActive(true);
        }

        bool hasProgress = (ProgressManager.Instance != null && ProgressManager.Instance.TrashCollected > 0);
        if (continueButton != null) continueButton.interactable = true;
        if (continueLabel != null) continueLabel.text = hasProgress ? "Continue" : "Start";
    }

    public void ExitGame()
    {
        // Ferme l'app
        PlayerPrefs.Save();

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }


    private void MoveRigSoCameraIsAt(Vector3 targetCamPos, float targetYaw)
    {
        if (xrRig == null) return;

        if (xrCamera == null && Camera.main != null)
            xrCamera = Camera.main.transform;

        if (xrCamera == null)
        {
            xrRig.SetPositionAndRotation(targetCamPos, Quaternion.Euler(0f, targetYaw, 0f));
            return;
        }

        var cc = xrRig.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        Vector3 camOffsetLocal = Quaternion.Inverse(xrRig.rotation) * (xrCamera.position - xrRig.position);

        Quaternion yawRot = Quaternion.Euler(0f, targetYaw, 0f);
        xrRig.rotation = yawRot;

        Vector3 camOffsetWorld = xrRig.rotation * camOffsetLocal;

        xrRig.position = targetCamPos - camOffsetWorld;

        if (cc) cc.enabled = true;
    }



}
