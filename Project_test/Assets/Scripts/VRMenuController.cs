using UnityEngine;

public class VRMenuController : MonoBehaviour
{
    [Header("Références")]
    public Transform xrRig;             // VR Player
    public GameObject locomotionRoot;   // Locomotion
    public Transform gameStartPoint;    // GameStartPoint
    public GameObject menuRoot;         // MenuRoot ou MenuCanvas

    private void Start()
    {
        // Bloque la locomotion dès le lancement
        if (locomotionRoot != null)
            locomotionRoot.SetActive(false);
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
