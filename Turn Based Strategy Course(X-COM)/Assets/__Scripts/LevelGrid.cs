using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �������� ������� ���������� ������� LevelGrid, ������� � Project Settings/ Script Execution Order � �������� ���������� LevelGrid ���� Default Time, ����� LevelGrid ���������� ������ �� ���� ��� ��������� �������� ����� ���� ( � Start() �� ��������� ����� PathfindingMonkey - ��������� ������ ����)

public class LevelGrid : MonoBehaviour // �������� ������ ������� ��������� ������ ������� ������ . �������� ������ ��������� ��� �������� ������������� ����� � �������� ������� �����
{

    public static LevelGrid Instance { get; private set; }   //(������� SINGLETON) ��� �������� ������� ����� ���� ������� (SET-���������) ������ ���� �������, �� ����� ���� �������� GET ����� ������ �������
                                                             // instance - ���������, � ��� ����� ���� ��������� LevelGrid ����� ���� ��� static. Instance ����� ��� ���� ����� ������ ������, ����� ����, ����� ����������� �� Event.

    public const float FLOOR_HEIGHT = 3f; // ������ ����� � ������ - ��� ������ ������

    public event EventHandler<OnAnyUnitMovedGridPositionEventArgs> OnAnyUnitMovedGridPosition; //������� ������� ����� - ����� ���� ��������� � �������� �������  // <OnAnyUnitMovedGridPositionEventArgs>- ������� �������� ����� ������� ������ ���������

    public class OnAnyUnitMovedGridPositionEventArgs : EventArgs // �������� ����� �������, ����� � ��������� ������� �������� ����� � �������� �������
    {
        public Unit unit;
        public GridPosition fromGridPosition;
        public GridPosition toGridPosition;
    }


    [SerializeField] private Transform _gridDebugObjectPrefab; // ������ ������� ����� //������������ ��� ������ ��������� � ����� ��������� ������ CreateDebugObject

    [SerializeField] private int _width = 10;     // ������
    [SerializeField] private int _height = 10;    // ������
    [SerializeField] private float _cellSize = 2f;// ������ ������
    [SerializeField] private int _floorAmount = 2;// ���������� ������

    private List<GridSystemHexAndQuad<GridObject>> _gridSystemList; //������ �������� ������ .� �������� ������� ��� GridObject

    private void Awake()
    {
        // ���� �� �������� � ���������� �� �������� �� �����
        if (Instance != null) // ������� �������� ��� ���� ������ ���������� � ��������� ����������
        {
            Debug.LogError("There's more than one LevelGrid!(��� ������, ��� ���� LevelGrid!) " + transform + " - " + Instance);
            Destroy(gameObject); // ��������� ���� ��������
            return; // �.�. � ��� ��� ���� ��������� LevelGrid ��������� ����������, ��� �� �� ��������� ������ ����
        }
        Instance = this;

        _gridSystemList = new List<GridSystemHexAndQuad<GridObject>>(); // �������������� ������

        for (int floor = 0; floor < _floorAmount; floor++) // ��������� ����� � �� ������ �������� �������� �������
        {
            GridSystemHexAndQuad<GridObject> gridSystem = new GridSystemHexAndQuad<GridObject>(_width, _height, _cellSize, floor, FLOOR_HEIGHT, // �������� ����� 10 �� 10 � �������� 2 ������� �� ����� floor ������� 3  � � ������ ������ �������� ������ ���� GridObject
                 (GridSystemHexAndQuad<GridObject> g, GridPosition gridPosition) => new GridObject(g, gridPosition)); //� ��������� ��������� ��������� ������� ������� �������� ����� ������ => new GridObject(g, _gridPosition) � ��������� �� ��������. (������ ��������� ����� ������� � ��������� �����)
                                                                                                                      // _gridSystemList.CreateDebugObject(_gridDebugObjectPrefab); // �������� ��� ������ � ������ ������ // �������������� �.�. PathfindingGridDebugObject ����� ��������� ��������������� ������ _gridDebugObjectPrefab

            _gridSystemList.Add(gridSystem); // ������� � ������ ��������� �����
        }
    }

    private void Start()
    {
        PathfindingMonkey.Instance.Setup(_width, _height, _cellSize, _floorAmount); // �������� ����� ����� ������ ���� // �������� ��� ���� ����� �������� ������ �� ���� ��� ��������� �������� ����� ����
    }

    private GridSystemHexAndQuad<GridObject> GetGridSystem(int floor) // �������� �������� ������� ��� ������� �����
    {
        return _gridSystemList[floor];
    }


    public void AddUnitAtGridPosition(GridPosition gridPosition, Unit unit) // �������� ������������� ����� � �������� ������� �����
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition); // ������� GridObject ������� ��������� � gridPosition
        gridObject.AddUnit(unit); // �������� ����� 
    }

    public List<Unit> GetUnitListAtGridPosition(GridPosition gridPosition) // �������� ������ ������ � �������� ������� �����
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition); // ������� GridObject ������� ��������� � gridPosition
        return gridObject.GetUnitList();// ������� �����
    }

    public void RemoveUnitAtGridPosition(GridPosition gridPosition, Unit unit) // �������� ����� �� �������� ������� �����
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition); // ������� GridObject ������� ��������� � gridPosition
        gridObject.RemoveUnit(unit); // ������ �����
    }

    public void UnitMovedGridPosition(Unit unit, GridPosition fromGridPosition, GridPosition toGridPosition) // ���� ��������� � �������� ������� �� ������� fromGridPosition � ������� toGridPosition
    {
        RemoveUnitAtGridPosition(fromGridPosition, unit); // ������ ����� �� ������� ������� �����

        AddUnitAtGridPosition(toGridPosition, unit);  // ������� ����� � ��������� ������� �����

        OnAnyUnitMovedGridPosition?.Invoke(this, new OnAnyUnitMovedGridPositionEventArgs // ������� ����� ��������� ������ OnAnyUnitMovedGridPositionEventArgs
        {
            unit = unit,
            fromGridPosition = fromGridPosition,
            toGridPosition = toGridPosition,

        }); // �������� ������� ����� ���� ��������� � �������� ������� ( � ��������� ��������� ����� ���� ������ � ����)
    }

    public int GetFloor(Vector3 worldPosition) // �������� ����
    {
        return Mathf.RoundToInt(worldPosition.y / FLOOR_HEIGHT); // ������� ������� �� � �� ������ ����� � �������� �� ������ ��� ����� ������� ����
    }

    // ��� �� �� ���������� ��������� ���������� LevelGrid (� �� ������ ��������� ����_gridSystem) �� ������������ ������ � GridPosition ������� �������� ������� ��� ������� � GridPosition
    public GridPosition GetGridPosition(Vector3 worldPosition) // ������� �������� ������� ��� ������� ���������
    {
        int floor = GetFloor(worldPosition); // ������ ����
        return GetGridSystem(floor).GetGridPosition(worldPosition); // ��� ����� ����� ������ �������� �������
    }

    public Vector3 GetWorldPosition(GridPosition gridPosition) => GetGridSystem(gridPosition.floor).GetWorldPosition(gridPosition); // �������� �������
    
    public bool IsValidGridPosition(GridPosition gridPosition) // �������� �� ���������� �������� ��������
    {
        if (gridPosition.floor < 0 || gridPosition.floor >= _floorAmount) // ������� �� ������� ����� ������
        {
            return false;
        }
        else
        {
            return GetGridSystem(gridPosition.floor).IsValidGridPosition(gridPosition); // �������� ������� ��� ��������� ������� � IsValidGridPosition �� _gridSystemList
        }

    }
    public int GetWidth() => GetGridSystem(0).GetWidth(); // ��� ����� ����� ���������� ����� ����� �� ����� 0 ����
    public int GetHeight() => GetGridSystem(0).GetHeight();
    public float GetCellSize() => GetGridSystem(0).GetCellSize();
    public int GetFloorAmount() => _floorAmount;

    public bool HasAnyUnitOnGridPosition(GridPosition gridPosition) // ���� �� ����� ������ ���� �� ���� �������� �������
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition); // ������� GridObject ������� ��������� � _gridPosition
        return gridObject.HasAnyUnit();
    }
    public Unit GetUnitAtGridPosition(GridPosition gridPosition) // �������� ����� � ���� �������� �������
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition); // ������� GridObject ������� ��������� � _gridPosition
        return gridObject.GetUnit();
    }


    // IInteractable ��������� �������������� - ��������� � ������ InteractAction ����������������� � ����� �������� (�����, �����, ������...) - ������� ��������� ���� ���������
    public IInteractable GetInteractableAtGridPosition(GridPosition gridPosition) // �������� ��������� �������������� � ���� �������� �������
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition); // ������� GridObject ������� ��������� � _gridPosition
        return gridObject.GetInteractable();
    }
    public void SetInteractableAtGridPosition(GridPosition gridPosition, IInteractable interactable) // ���������� ���������� ��������� �������������� � ���� �������� �������
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition); // ������� GridObject ������� ��������� � _gridPosition
        gridObject.SetInteractable(interactable);
    }
    public void ClearInteractableAtGridPosition(GridPosition gridPosition) // �������� ��������� �������������� � ���� �������� �������
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition); // ������� GridObject ������� ��������� � _gridPosition
        gridObject.ClearInteractable(); // �������� ��������� �������������� � ���� �������� �������
    }

}
