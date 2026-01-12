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

    [Header("Menu")]
    public Transform menuSpawnPoint;       // optionnel : si vous voulez un point dédié
    public GameObject firstMenuPanel;      // optionnel : le panneau "tout premier" du menu

    private Vector3 menuSpawnPos;
    private Quaternion menuSpawnRot;
    private bool menuSpawnCaptured = false;

    [Header("XR Camera")]
    public Transform xrCamera; // la Main Camera dans votre XR Origin

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

    public void ExitToMainMenu()
    {
        // 1) Bloquer locomotion (comme au départ)
        if (locomotionRoot != null)
            locomotionRoot.SetActive(false);

        // 2) Revenir au spawn du menu (position + direction)
        if (xrRig != null)
        {
            var cc = xrRig.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;

            MoveRigSoCameraIsAt(menuCamPos, menuCamYaw);

            if (cc) cc.enabled = true;
        }

        // 3) Réafficher le menu
        if (menuRoot != null)
            menuRoot.SetActive(true);

        // 4) Forcer l’affichage du "premier panneau" (si vous en avez plusieurs)
        if (firstMenuPanel != null)
        {
            // Désactive tous les panels frères, puis active le bon
            var parent = firstMenuPanel.transform.parent;
            if (parent != null)
            {
                for (int i = 0; i < parent.childCount; i++)
                    parent.GetChild(i).gameObject.SetActive(false);
            }
            firstMenuPanel.SetActive(true);
        }

        // 5) Rafraîchir le texte Start/Continue
        bool hasProgress = (ProgressManager.Instance != null && ProgressManager.Instance.TrashCollected > 0);
        if (continueButton != null) continueButton.interactable = true;
        if (continueLabel != null) continueLabel.text = hasProgress ? "Continue" : "Start";
    }

    public void ExitGame()
    {
        // Ne supprime rien. Ferme juste l'app.
        PlayerPrefs.Save(); // optionnel mais sûr si vous stockez des réglages (ex: volume)

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // pour que ça "marche" quand vous testez dans l'Editor
    #else
        Application.Quit(); // en build (Quest/PC), ça ferme l'app
    #endif
    }


    private void MoveRigSoCameraIsAt(Vector3 targetCamPos, float targetYaw)
    {
        if (xrRig == null) return;

        if (xrCamera == null && Camera.main != null)
            xrCamera = Camera.main.transform;

        // Fallback (si vraiment pas de camera)
        if (xrCamera == null)
        {
            xrRig.SetPositionAndRotation(targetCamPos, Quaternion.Euler(0f, targetYaw, 0f));
            return;
        }

        var cc = xrRig.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        // offset caméra en "rig space"
        Vector3 camOffsetLocal = Quaternion.Inverse(xrRig.rotation) * (xrCamera.position - xrRig.position);

        // on n’applique que le yaw
        Quaternion yawRot = Quaternion.Euler(0f, targetYaw, 0f);
        xrRig.rotation = yawRot;

        // offset caméra en world après rotation
        Vector3 camOffsetWorld = xrRig.rotation * camOffsetLocal;

        // positionner le rig pour que la caméra tombe pile sur targetCamPos
        xrRig.position = targetCamPos - camOffsetWorld;

        if (cc) cc.enabled = true;
    }



}
