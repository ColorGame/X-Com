using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class InteractAction : BaseAction // �������� ��������������
{
    public static event EventHandler OnAnyInteractActionComplete; // ����� �������������� ���������

    private int _maxInteractDistance = 1; // ��������� ��������������
    private GridPosition _targetGridPosition;

    private void Update()
    {
        if (!_isActive) // ���� �� ������� �� ...
        {
            return; // ������� � ���������� ��� ����
        }

        Vector3 targetDirection = (LevelGrid.Instance.GetWorldPosition(_targetGridPosition) - transform.position).normalized; // ����������� � ������� �������, ��������� ������
        float rotateSpeed = 10f; //����� ���������//

        transform.forward = Vector3.Slerp(transform.forward, targetDirection, Time.deltaTime * rotateSpeed); // ������ �����.

        //��� �� �� �� ������ ��������� � ����� ��������� ����� �� ���������� ��������� �������� - ������� �������� ��� ��������
        //����� �������� ������ ������� ����� ������� �� ����� ����� ����� �� ��� ����������� ������
        //ActionComplete(); //�������� ���������

    }

    public override string GetActionName() // ������� ��� ��� ������
    {
        return "��������������";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition) //�������� �������� ���������� ��  ��� ���������� ��� �������� �������// ������������� ����������� ������� ����� //EnemyAIAction ������ � ������ ���������� ������� �������, ���� ������ - ��������� ������ ������ � ����������� �� ��������� ����� ������� ��� �����
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 0
        };
    }

    public override List<GridPosition> GetValidActionGridPositionList() // �������� ������ ���������� �������� ������� ��� �������� // ������������� ������� �������  
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>(); 

        GridPosition unitGridPosition = _unit.GetGridPosition(); // ������� ������� � ����� �����

        for (int x = -_maxInteractDistance; x <= _maxInteractDistance; x++) // ���� ��� ����� ����� ������� � ������������ unitGridPosition, ������� ��������� ���������� �������� � �������� ������� _maxInteractDistance
        {
            for (int z = -_maxInteractDistance; z <= _maxInteractDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z, 0);  // ��������� �������� �������. ��� ������� ���������(0,0, 0-����) �������� ��� ���� 
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;  // ����������� �������� �������

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) // �������� �������� �� testGridPosition ���������� �������� �������� ���� ��� �� ��������� � ���� �����
                {
                    continue;
                }

                /*//�������� ����������� �������� ������� �� ������� �����
                DoorInteract door = LevelGrid.Instance.GetDoorAtGridPosition(testGridPosition);

                if (door == null)
                {
                    // � ���� ������� ����� ��� �����
                    continue;
                }*/
                // �������� ��������� �������������� ��� �� �� ����� ���������������� �� ������ � ������
                IInteractable interactable  = LevelGrid.Instance.GetInteractableAtGridPosition(testGridPosition);

                if (interactable == null)
                {
                    // � ���� ������� ����� ��� ������� ��������������
                    continue;
                }
                

                validGridPositionList.Add(testGridPosition);
            }
        }

        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete) // ������������� TakeAction (��������� �������� (�����������). (������� onActionComplete - �� ���������� ��������). � ����� ������ �������� �������� ������� ClearBusy - �������� ���������
    {
        IInteractable interactable = LevelGrid.Instance.GetInteractableAtGridPosition(gridPosition); // ������� IInteractable(��������� ��������������) �� ���������� �������� ������� // ��� ��� ������� ����� ������ �� ������� (�����, �����, ������...) - ��� �� �� ���������� ���� ���������
        SoundManager.Instance.PlaySoundOneShot(SoundManager.Sound.Interact);
        interactable.Interact(OnInteractComplete); //���������� �������������� � ���������� IInteractable(��������� ��������������) � ��������� ������ - ��� ���������� �������������� (���� ������� ����� �������� ���� �����)
        _targetGridPosition = gridPosition;
        ActionStart(onActionComplete); // ������� ������� ������� ����� �������� // �������� ���� ����� � ����� ����� ���� �������� �.�. � ���� ������ ���� EVENT � �� ������ ����������� ����� ���� ��������
    }

    private void OnInteractComplete() //��� ���������� ��������������
    {

        OnAnyInteractActionComplete?.Invoke(this, EventArgs.Empty);
        ActionComplete(); //�������� ���������
        //��� �� �� �� ������ ��������� � ����� ��������� ����� �� ���������� ��������� �������� - ������� �������� ��� ��������
        //����� �������� ������ ������� ����� ������� �� ����� ����� ����� �� ��� ����������� ������
        
    }

    public override int GetMaxActionDistance()
    {
        return _maxInteractDistance;
    }

    public override string GetToolTip()
    {
        return "���� - " + GetActionPointCost() + "\n" +
                "��������� - " + GetMaxActionDistance() + "\n" +
                "����� ��������� �����, � <color=#00ff00> ������� �����</color>  ";
    }
}

