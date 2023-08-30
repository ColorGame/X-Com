using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class CameraController : MonoBehaviour // � ���������� ����� ������� ����������� � �������� InputSystem
{

    public static CameraController Instance { get; private set; }   //(������� SINGLETON) ��� �������� ������� ����� ���� ������� (SET-���������) ������ ���� �������, �� ����� ���� �������� GET ����� ������ �������
                                                                    // instance - ���������, � ��� ����� ���� ��������� LevelGrid ����� ���� ��� static. Instance ����� ��� ���� ����� ������ ������, ����� ����, ����� ����������� �� Event.


    private const float MIN_FOLLOW_Y_OFFSET = 2f;
    private const float MAX_FOLLOW_Y_OFFSET = 15f;

    [SerializeField] private CinemachineVirtualCamera _cinemachineVirtualCamera;
    [SerializeField] private Collider cameraBoundsCollider; //������ �� ��������� ������� ������������ ����������� ������

    private CinemachineTransposer _cinemachineTransposer;
    private Vector3 _targetFollowOffset; // ������� �������� ����������       
    private bool _edgeScrolling; // ��������� �� �����    

    private void Awake()
    {
        // ���� �� �������� � ���������� �� �������� �� �����
        if (Instance != null) // ������� �������� ��� ���� ������ ���������� � ��������� ����������
        {
            Debug.LogError("There's more than one CameraController!(��� ������, ��� ���� CameraController!) " + transform + " - " + Instance);
            Destroy(gameObject); // ��������� ���� ��������
            return; // �.�. � ��� ��� ���� ��������� CameraController ��������� ����������, ��� �� �� ��������� ������ ����
        }
        Instance = this;

        _edgeScrolling = PlayerPrefs.GetInt("edgeScrolling", 1) == 1; // �������� ���������� �������� _edgeScrolling � ���� ��� 1 �� ����� ������ ���� ��=1 �� ����� ���� (�� PlayerPrefs.GetInt ������ ������ ������� ��������� ������� ���������� ������)
    }

    private void Start()
    {
        _cinemachineTransposer = _cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>(); // ������� � �������� ��������� CinemachineTransposer �� ����������� ������, ����� � ���������� �������� �� ��������� ��� ZOOM ������

        _targetFollowOffset = _cinemachineTransposer.m_FollowOffset; // �������� ����������
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleZoom();
    }

    private void HandleMovement() // ������ ��������
    {
        Vector2 inputMoveDirection = InputManager.Instance.GetCameraMoveVector(); // ����������� ��������� ��������� (�������� ����� ������ ��������������)

        if (_edgeScrolling) // ���� ��������� �� ����� ������������� �� ������� ������� ����
        {
            Vector2 mousePosition = InputManager.Instance.GetMouseScreenPosition(); 
            float edgeScrollingSize = 20; // (���������� ��������) ������ �� ���� ������ ��� ���������� �������� ������
            if (mousePosition.x > Screen.width - edgeScrollingSize) // ���� ��� ������ ������ ����� - ������ �� ����
            {
                inputMoveDirection.x = +1f;
            }
            if (mousePosition.x < edgeScrollingSize)
            {
                inputMoveDirection.x = -1f;
            }
            if (mousePosition.y > Screen.height - edgeScrollingSize)
            {
                inputMoveDirection.y = +1f;
            }
            if (mousePosition.y < edgeScrollingSize)
            {
                inputMoveDirection.y = -1f;
            }
        }

        float moveSpeed = 10f; // �������� ������

        //����� �������� ��������� �������� ����������� ������ inputMoveDirection � moveVector
        Vector3 moveVector = transform.forward * inputMoveDirection.y + transform.right * inputMoveDirection.x; // �������� ��������� ��������. ��������� ������ forward(z) ������� �� inputMoveDirection.y, � ��������� ������ right(x) ������� �� inputMoveDirection.x
        Vector3 targetPosition = transform.position + moveVector * moveSpeed * Time.deltaTime; //��������� ������� ������� � ������� �����  ����������� ��� ������

        //��������� ��������
        targetPosition.x = Mathf.Clamp(targetPosition.x,
            cameraBoundsCollider.bounds.min.x ,
            cameraBoundsCollider.bounds.max.x);
        targetPosition.z = Mathf.Clamp(targetPosition.z,
            cameraBoundsCollider.bounds.min.z ,
            cameraBoundsCollider.bounds.max.z );

       // Debug.Log( cameraBoundsCollider.bounds.min);
       transform.position = targetPosition; // ���������� � ���������� �������
    }

    private void HandleRotation() // ������ �������
    {
        Vector3 rotationVector = new Vector3(0, 0, 0); // ������ �������� // ����� ������� ������ ������ ��� Y (�������� ����� ������ ��������������)

        rotationVector.y = InputManager.Instance.GetCameraRotateAmount(); //�������� �������� �������� ������ �� ��� �

        float rotationSpeed = 100f;
        transform.eulerAngles += rotationVector * rotationSpeed * Time.deltaTime;
        //��� ���� ������
        //transform .Rotate(rotationVector, rotationSpeed * Time.deltaTime);
    }

    private void HandleZoom() // ������ ���������������
    {
        //Debug.Log(InputManager.Instance.GetCameraZoomAmount()); // ������� ����� ������� �������� ������

        float zoomIncreaseAmount = 1f; //������� �������� ���������� (�������� ����������)

        _targetFollowOffset.y += InputManager.Instance.GetCameraZoomAmount() * zoomIncreaseAmount; // �������� �������� ���������� ������

        // �� �� ���������� Time.deltaTime �.�. ��������� ��� ��������� �������� ��������� �������� � �� ��������� �� �������� (���� ����� ��� � ����������� ��� ������� ������� �������� Input.GetKeyDown)

        _targetFollowOffset.y = Mathf.Clamp(_targetFollowOffset.y, MIN_FOLLOW_Y_OFFSET, MAX_FOLLOW_Y_OFFSET);// ��������� �������� ���������������
        float zoomSpeed = 5f;
        _cinemachineTransposer.m_FollowOffset = Vector3.Lerp(_cinemachineTransposer.m_FollowOffset, _targetFollowOffset, Time.deltaTime * zoomSpeed); // ��������� ���� ��������� ��������, ��� ��������� ���������� Lerp
    }

    public float GetCameraHeight() // �������� ������ ������ ��������
    {
        return _targetFollowOffset.y;
    }

    public void SetEdgeScrolling(bool edgeScrolling) // ���������� ������� �������� ��� - ��������� �� �����
    {
        this._edgeScrolling = edgeScrolling;
        PlayerPrefs.SetInt("edgeScrolling", edgeScrolling ? 1 : 0); // �������� ���������� �������� � ������ (���� _edgeScrolling ������ �� ��������� 1 ���� ���� ��������� 0 )
    }

    public bool GetEdgeScrolling() // ������� ������� �������� ��� - ��������� �� �����
    {
        return _edgeScrolling;
    }

}
