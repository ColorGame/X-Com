using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    [SerializeField] private Transform _gridDebugObjectPrefab; // ������ ������� ����� //������������ ��� ������ ��������� � ����� ��������� ������ CreateDebugObject
    [SerializeField] private Unit _unit;
    private GridSystemHexAndQuad<GridObject> _gridSystem;

    private void Start()
    {


        //���� CreateDebugObject ������������ ��������� ����� � ������ ������
        /*_gridSystemList = new GridSystemHexAndQuad(10, 10, 2f); // �������� ����� 10 �� 10 � �������� 2 �������
        _gridSystemList.CreateDebugObject(_gridDebugObjectPrefab); // �������� ��� ������ � ������ ������

        Debug.Log(new GridPosition(5, 7)); // ��������� ��� ���������� GridPosition*/
    }

    private void Update()
    {
        //���� 
        //Debug.Log(_gridSystemList.GetGridPosition(MouseWorld.GetPosition())); // ������� ��������� ����� ����� ��� ������

        //���� ���������� �������� ������� ��� ��������




        
        if(Input.GetKeyDown(KeyCode.T))
        {
            //��� ��������� ����� ���������� �� (0,0) � ����� ��������� ����
            /*GridPosition _mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition());

            GridPosition startGridPosition = new GridPosition(0, 0);

            List<GridPosition> gridPositionList = PathfindingMonkey.Instance.FindPath(startGridPosition, _mouseGridPosition);

            for (int i = 0; i < gridPositionList.Count -1 ; i++)
            {
                Debug.DrawLine(
                    LevelGrid.Instance.GetWorldPosition(gridPositionList[i]),
                    LevelGrid.Instance.GetWorldPosition(gridPositionList[i + 1]),
                    Color.white,
                    10f
                    );
            }*/

            
        }      
    }
}

