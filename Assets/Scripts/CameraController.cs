using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 8f;

    private void Update()
    {
        Vector3 input = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) input.y += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) input.y -= 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) input.x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input.x += 1f;

        transform.position += input.normalized * moveSpeed * Time.deltaTime;
    }
}
