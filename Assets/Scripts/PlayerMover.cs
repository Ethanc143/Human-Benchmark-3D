using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMover : MonoBehaviour
{
    [Header("Look Settings")]
    [SerializeField] private float lookSpeed = 3f;     

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 10f;   
    [SerializeField] private float boostFactor = 4f;    

    [SerializeField] private Transform cameraTransform; 

    private float yaw = 0f;
    private float pitch = 0f;

    Vector2 v2;
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
        if (cameraTransform == null)
        {
            cameraTransform = transform;
        }
        transform.position += (cameraTransform.forward * v2.y + cameraTransform.right * v2.x).normalized * speed * Time.deltaTime;
    }

    void OnMove(InputValue value)
    {
        v2 = value.Get<Vector2>();
    }

    void OnBoost(InputValue value)
    {
        isBoosting = value.Get<float>() > 0.5f;
    }

    void OnLook(InputValue value)
    {
        Vector2 v2 = value.Get<Vector2>();
        yaw += v2.x * lookSpeed;
        pitch -= v2.y * lookSpeed;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
