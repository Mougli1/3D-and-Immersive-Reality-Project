using UnityEngine;
using System.Collections;

public class TrashBinLidScript : MonoBehaviour
{
    public Transform lid;
    public float openAngle = -70f;
    public float speed = 4f;

    private Quaternion closedRot;
    private Quaternion openRot;

    void Start()
    {
        //Rotation de base = fermé
        closedRot = lid.localRotation;

        //Rotation quand c’est ouvert
        openRot = Quaternion.Euler(openAngle, 0, 0);
    }

    public void OpenLid()
    {
        Debug.Log("OUVERTURE DU COUVERCLE !");
        StopAllCoroutines();
        StartCoroutine(OpenClose());
    }

    IEnumerator OpenClose()
    {
        float t = 0f;

        //Ouvre
        while (t < 1)
        {
            t += Time.deltaTime * speed;
            lid.localRotation = Quaternion.Slerp(closedRot, openRot, t);
            yield return null;
        }

        // Petite pause
        yield return new WaitForSeconds(0.5f);

        // Referme
        t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime * speed;
            lid.localRotation = Quaternion.Slerp(openRot, closedRot, t);
            yield return null;
        }
    }
}
