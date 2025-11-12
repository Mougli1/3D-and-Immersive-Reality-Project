using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerCC : MonoBehaviour {
    public float moveSpeed = 3.5f;
    public float mouseSensitivity = 120f;
    public float gravity = -9.81f;
    public Transform cam;

    private bool inputEnabled = true;
    private float yVelocity, pitch;
    private CharacterController cc;

    private void Awake() {
        cc = GetComponent<CharacterController>();
        SetInputEnabled(true);
    }

    public void SetInputEnabled(bool enabled) {
        inputEnabled = enabled;
        Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !enabled;
    }

    private void Update() {
        if (!inputEnabled) return;

        float mx = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(0f, mx, 0f);
        pitch = Mathf.Clamp(pitch - my, -75f, 75f);
        if (cam) cam.localEulerAngles = new Vector3(pitch, 0f, 0f);

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = (transform.right * h + transform.forward * v) * moveSpeed;

        if (cc.isGrounded && yVelocity < 0f) yVelocity = -2f;
        yVelocity += gravity * Time.deltaTime;

        cc.Move((move + Vector3.up * yVelocity) * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Escape)) SetInputEnabled(false);
    }
}
