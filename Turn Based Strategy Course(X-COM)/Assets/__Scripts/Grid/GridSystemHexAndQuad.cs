//#define HEX_GRID_SYSTEM //������������ �������� ������� //  � C# ��������� ��� �������� �������������, ����������� ������� �� ������������� ��������� ���� ��������� ������������. 
//��� ��������� ���������� ������� ������������� ������ ��������� ����� �� ����������� � ��������� ��� � ��� �������� �����, ��� ��� ����������. 
// ��� 3 ������� GridSystemVisual  GridSystemVisualSingle  PathfindingMonkey
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GridSystemHexAndQuad<TGridObject>  // �������� ������� ������������ � ���������� // ����������� ����� C#// ����� ������������ ����������� ��� �������� ����� ����� ������� �� �� ��������� MonoBehaviour/
                                                //<TGridObject> - Generic, ��� ���� ����� GridSystemHexAndQuad ����� �������� �� ������ � GridObject �� � � ��. ������������� �� ������ �������� �����
                                                // Generic - �������� ����������� ����� ���� GridSystemHexAndQuad ��� ������ ���� (��� ���� ��� �� �������� ����������� ��� � ������ ������� �����)

{
    private const float HEX_VERTICAL_OFFSET_MULTIPLIER = 0.75f; //������������ ��������� ������������� ��������
    
    private Vector3 _globalOffset = new Vector3(0, 0, 0); // �������� ����� � ������� ����������� //����� ���������//

    private int _width;     // ������
    private int _height;    // ������
    private float _cellSize;// ������ ������
    private int _floor;// ���� �� ������� ������������� ���� �������� �������
    private float _floorHeight;// ������ �����
    private TGridObject[,] _gridObjectArray; // ��������� ������ �������� �����


    public GridSystemHexAndQuad(int width, int height, float cellSize, int floor, float floorHeight, Func<GridSystemHexAndQuad<TGridObject>, GridPosition, TGridObject> createGridObject)  // �����������
                                                                                                                                                             // Func - ��� ���������� ������� (������ �������� � ��������� ��� ���<TGridObject> ������� ���������� ��� ������� � ������� ��� createGridObject)
    {
        _width = width; // ���� �� �� ������� �� _width � width �� ������ ��� ��� // this.width = width;
        _height = height;
        _cellSize = cellSize;
        _floor = floor;
        _floorHeight = floorHeight;

        _gridObjectArray = new TGridObject[width, height]; // ������� ������ ����� ������������� �������� width �� height
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z, floor);
                _gridObjectArray[x, z] = createGridObject(this, gridPosition); // ������� ������� createGridObject � � �������� ��������� ���� GridSystemHexAndQuad � ������� �����. ��������� ��� � ������ ������� ����� � ��������� ������ ��� x,z ��� ����� ������� �������.

                // ��� �����                
                //Debug.DrawLine(GetWorldPosition(_gridPosition), GetWorldPosition(_gridPosition) + Vector3.right* .2f, Color.white, 1000); // ��� ����� �������� ��������� ����� � ������ ������ ������ �����
            }
        }
    }

    public Vector3 GetWorldPosition(GridPosition gridPosition) // �������� ������� ���������
    {
#if HEX_GRID_SYSTEM // ���� ������������ �������� �������
        return
             new Vector3(gridPosition.x, 0, 0) * _cellSize +
             new Vector3(0, 0, gridPosition.z) * _cellSize * HEX_VERTICAL_OFFSET_MULTIPLIER + // �� ��� Z ���� �������� �� 75% �� ������� ������ � ������� �� ���������� ����� ��� ������� �� ���� ������
             (((gridPosition.z % 2) == 1) ? new Vector3(1, 0, 0) * _cellSize * .5f : Vector3.zero) + // ���� ������ �������� �.�. -������� �� ������� �� ������ �� 2 ����� 1- �� ������� �� �� ��� � ������ �� �������� ������� ������ (1%2==1  3%2==1 5%2==1 ...���������� ��� ����� ���������� �� 1,1 � 0,0  gridPosition.z % 2 - ��� ������� ���������, ����� ������� ��� � ���� ���� �������))
             _globalOffset + //���� ����� �������� ����� � ���������� ����������� �� ������� ��� ����������
             new Vector3(0,gridPosition.floor,0) * _floorHeight; // ����� ���� � ������ �����

#else //� ��������� ������ ������������� 

        return new Vector3(gridPosition.x, 0, gridPosition.z) * _cellSize + 
            new Vector3(0, gridPosition.floor, 0) * _floorHeight; // ����� ���� � ������ �����
#endif
    }

    public GridPosition GetGridPosition(Vector3 worldPosition) // �������� �������� ��������� (��������� ������������ ����� ��������� �����)
    {
#if HEX_GRID_SYSTEM // ���� ������������ �������� �������

        GridPosition roughXZ = new GridPosition( // ��������������� XZ
                Mathf.RoundToInt(worldPosition.x / _cellSize),
                Mathf.RoundToInt(worldPosition.z / _cellSize / HEX_VERTICAL_OFFSET_MULTIPLIER),
                _floor
        );

        bool oddRow = roughXZ.z % 2 == 1; // oddRow - �������� ��� . ���� ������ �� �� ��������� � �������� ����

        List<GridPosition> neighbourGridPositionList = new List<GridPosition> // ������ �������� �������� �������
        {
            roughXZ + new GridPosition(-1, 0, _floor), //��������� �����
            roughXZ + new GridPosition(+1, 0, _floor), //��������� ������

            roughXZ + new GridPosition(0, +1, _floor), //��������� �����
            roughXZ + new GridPosition(0, -1, _floor), //��������� ����

            roughXZ + new GridPosition(oddRow ? +1 : -1, +1, _floor), // ���� � �������� ���� �� �� � +1(������) ���� ��� �� � - 1(�����),  �� Z �����
            roughXZ + new GridPosition(oddRow ? +1 : -1, -1, _floor), // ���� � �������� ���� �� �� � +1(������) ���� ��� �� � - 1(�����),  �� Z ����
        };

        GridPosition closestGridPosition = roughXZ; // ��������� ������� ����� ����� ����� = ��������������� XZ

        foreach (GridPosition neighbourGridPosition in neighbourGridPositionList) // ��������� ������ �������� ����� . ������� ��������� �� ����� ������� �����(�������� ������� ����) �� ������� ���������� �������� ������ � ������� � ��������� 
        {
            if (Vector3.Distance(worldPosition, GetWorldPosition(neighbourGridPosition)) <
                Vector3.Distance(worldPosition, GetWorldPosition(closestGridPosition)))
            {
                //�������� �����, ��� ����� �������
                closestGridPosition = neighbourGridPosition; // ��������� ���� �������� ��� ����� �������
            };
        }
        return closestGridPosition;



#else //� ��������� ������ ������������� 
        return new GridPosition
            (
            Mathf.RoundToInt(worldPosition.x / _cellSize),  // ��������� Mathf.RoundToInt ��� �������������� float � int
            Mathf.RoundToInt(worldPosition.z / _cellSize),
            _floor
            );
#endif
    }

    public void CreateDebugObject(Transform debugPrefab) // ������� ������ ������� ( public ��� �� ������� �� ������ Testing � ������� ������� �����)   // ��� Transform � GameObject ��������������� �.�. � ������ GameObject ���� Transform � � ������� Transform ���� ������������� GameObject
                                                         // � �������� ��� ������ ��� ����� Transform �������� �������. ���� � ��������� ������� ��� GameObject, ����� � ������, ���� �� �� ������ ����� ������� GameObject �������� ��� �������, ��� �������� ������ �������������� ��� "debugGameObject.Transform.LocalScale..."
                                                         // ������� ��� ��������� ���� � ��������� ��������� ��� Transform.
    {
        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z, _floor); // ������� �����

                Transform debugTransform = GameObject.Instantiate(debugPrefab, GetWorldPosition(gridPosition), Quaternion.identity);  // �������� ��������� ����������� �������(debugPrefab) � ������ ������ ����� // �.�. ��� ���������� MonoBehaviour �� �� ����� �������� ������������ Instantiate ������ ����� GameObject.Instantiate
                GridDebugObject gridDebugObject = debugTransform.GetComponent<GridDebugObject>(); // � ���������� ������ ������� ��������� GridDebugObject
                gridDebugObject.SetGridObject(GetGridObject(gridPosition)); // �������� ����� SetGridObject() � �������� ���� ������� ����� ����������� � ������� _gridPosition // GetGridObject(_gridPosition) as GridObject - �������� ��������� <TGridObject> ��� GridObject

                // debugTransform.GetComponentInChildren<TextMeshPro>().text = _gridPosition.ToString(); // ��� �������� ������� ��� ����������� ��������� ������ �����( �� ����� ������ �� ���������� debugPrefab) � ������ ����� GridDebugObject
            }
        }
    }

    public TGridObject GetGridObject(GridPosition gridPosition) // ������ ������� ������� ��������� � ������ ������� ����� .������� ��������� �.�. ����� ����������� �������� �� ���.
    {
        return _gridObjectArray[gridPosition.x, gridPosition.z]; // x,z ��� ������� ������� �� ������� ����� ������� ������ �������
    }

    public bool IsValidGridPosition(GridPosition gridPosition) // �������� �� ���������� �������� ��������
    {
        return  gridPosition.x >= 0 &&
                gridPosition.z >= 0 &&
                gridPosition.x < _width &&
                gridPosition.z < _height &&
                gridPosition.floor == _floor;
        // ��������� ��� ���������� ��� �������� ������ 0 � ������ ������ � ������ ����� �����
    }

    public int GetWidth()
    {
        return _width;
    }

    public int GetHeight()
    {
        return _height;
    }

    public float GetCellSize()
    {
        return _cellSize;
    }

}
