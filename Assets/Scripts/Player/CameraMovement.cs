using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float _speed = default;
    [SerializeField] private float _sensitivity = default;

    private Transform _transform;
    private Vector3 _movementInput ;

    private void Start()
    {
        _transform = transform;
    }

    public void Move(float x, float z)
    {
        _movementInput.x = x;
        _movementInput.z = z;

        _transform.position += _speed * Time.deltaTime * _movementInput;
    }
}
