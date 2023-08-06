using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MouseWorld : MonoBehaviour // ����� ���������� �� ��������� ������� ����
{

    public static MouseWorld Instance { get; private set; }   //(������� SINGLETON) ��� �������� ������� ����� ���� ������� (SET-���������) ������ ���� �������, �� ����� ���� �������� GET ����� ������ �������
                                                              // instance - ���������, � ��� ����� ���� ��������� MouseWorld ����� ���� ��� static. Instance ����� ��� ���� ����� ������ ������, ����� ����, ����� ����������� �� Event.

    public static event EventHandler<OnMouseGridPositionChangedEventArgs> OnMouseGridPositionChanged; // ������� ������� ���� �� ����� ����������
    public class OnMouseGridPositionChangedEventArgs : EventArgs // �������� ����� �������, ����� � ��������� ������� ��������
    {
        public GridPosition lastMouseGridPosition; // ������� �������� ������� ����
        public GridPosition newMouseGridPosition;  // ����� �������� ������� ����
    }

    [SerializeField] private LayerMask _mousePlaneLayerMask; // ����� ���� ��������� ���� (�������� � ����������)

   private GridPosition _mouseGridPosition;  // �������� ������� ����


    private void Awake()
    {
        // ���� �� �������� � ���������� �� �������� �� �����
        if (Instance != null) // ������� �������� ��� ���� ������ ���������� � ��������� ����������
        {
            Debug.LogError("There's more than one MouseWorld!(��� ������, ��� ���� MouseWorld!) " + transform + " - " + Instance);
            Destroy(gameObject); // ��������� ���� ��������
            return; // �.�. � ��� ��� ���� ��������� MouseWorld ��������� ����������, ��� �� �� ��������� ������ ����
        }
        Instance = this;
    }

    private void Start()
    {
        _mouseGridPosition = LevelGrid.Instance.GetGridPosition(GetPositionOnlyHitVisible());  // ��������� ��� ������ �������� ������� ���� // ������ �������� � Awake() �.�. � ���������� ����� ��������� ������� ������ (��� ��������� ������ InputManager ��� MouseWorld ����������)
    }

    // ��� �����, �������� ��� ������� �� �������� ����.
    /*private void Update()
    {
        transform.position = MouseWorld.GetPosition(); // ��� ����� �������� �� ������ �����
    }*/

    private void Update()
    {
        GridPosition newMouseGridPosition = LevelGrid.Instance.GetGridPosition(GetPositionOnlyHitVisible()); // ������� ����� �������� ������� ����
        if (LevelGrid.Instance.IsValidGridPosition(newMouseGridPosition) && _mouseGridPosition != newMouseGridPosition) // ���� ��� ���������� �������� ������� � ��� �� ����� ���������� �� ...
        {
            OnMouseGridPositionChanged?.Invoke(this, new OnMouseGridPositionChangedEventArgs //�������� - ������� ������� ���� �� ����� ���������� � ��������� ���������� � ����� �������� �������
            {
                lastMouseGridPosition = _mouseGridPosition,
                newMouseGridPosition = newMouseGridPosition,

            }); // �������� ������� � ���������

            _mouseGridPosition = newMouseGridPosition; // ��������� ���������� ������� �� �����
        }
    }

    public static Vector3 GetPosition() // �������� ������� (static ���������� ��� ����� ����������� ������ � �� ������ ������ ����������) // ��� ����������� ����
    {
        Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition()); // ��� �� ������ � ����� �� ������ ��� ���������� ������ ����
        Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, Instance._mousePlaneLayerMask); // Instance._coverLayerMask - ����� ������ ��� �������� ����� ����� 1<<6  �.�. mousePlane ��� 6 �������
        return raycastHit.point; // ���� ��� ������� � �������� �� Physics.Raycast ����� true, � raycastHit.point ������ "����� ����� � ������� ������������, ��� ��� ����� � ���������", � ���� false �� ����� ������� ����������� ������ ������ ��������(� ����� ������ ������ ������� ������).
    }

    public static Vector3 GetPositionOnlyHitVisible() // �������� ������� ��� ���������, ������ ��� ������� �������� (static ���������� ��� ����� ����������� ������ � �� ������ ������ ����������) // � ��������� ��������� ������� ��� ���������� ��������� ����� ��� ����������� �� ������� �� �������� �������� ��������, � ����� �� �� ����� �������� �� ������� ��� ����� �.�. ������ ���� ������ ���������
    {
        Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition()); // ��� �� ������ � ����� �� ������ ��� ���������� ������ ����
        RaycastHit[] raycastHitArray = Physics.RaycastAll(ray, float.MaxValue, Instance._mousePlaneLayerMask); // �������� ������ ���� ��������� ����
        System.Array.Sort(raycastHitArray, (RaycastHit raycastHitA, RaycastHit raycastHitB) => // ����������� �������� � ����� ���������� ������� �� ��������� �� ����� �������� ���� (�.�. ��� ����������� ��������)
        {
            return Mathf.RoundToInt(raycastHitA.distance - raycastHitB.distance); // ����� ���������� // ��������� IComparer ������������� �����, ������� ���������� ��� �������.
        });

        foreach (RaycastHit raycastHit in raycastHitArray) // ��������� ��� ���������� ������
        {
            if (raycastHit.transform.TryGetComponent(out Renderer renderer)) //��������� �������� �� ������� � ������� ����� ��� ��������� Renderer
            {
                if (renderer.enabled) // � ���� �� �����
                {
                    return raycastHit.point;// ������ "����� ����� � ������� ������������, ��� ��� ����� � ���������"
                }
                // ���� �� �� ����� ������ ���������� ��������� � ����
            }
        }

        // ���� �� �� ��� �� ������� ������ ����
        return Vector3.zero;
    }



}
