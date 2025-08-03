using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMover : MonoBehaviour
{
    [Header("Look Settings")]
    [SerializeField] private float lookSpeed = 3f;     

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 10f;   
    [SerializeField] private float boostFactor = 4f;    

    private float yaw = 0f;
    private float pitch = 0f;


    Vector3 dir;
    bool isBoosting = false;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void Update()
    {
        float speed = (isBoosting) ? moveSpeed * boostFactor : moveSpeed;
        transform.position += dir * speed * Time.deltaTime;
    }

    void OnMove(InputValue value)
    {
        Vector2 v2 = value.Get<Vector2>();
        dir = (transform.forward * v2.y + transform.right * v2.x).normalized;
    }

    void OnBoost(InputValue value)
    {
        isBoosting = value.Get<float>() > 0.5f;
        Debug.Log("isBoosting");
    }

    void OnLook(InputValue value)
    {
        Vector2 v2 = value.Get<Vector2>();
        yaw += v2.x * lookSpeed;
        pitch -= v2.y * lookSpeed;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
