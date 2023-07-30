using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SwordAction;

public class HealAction : BaseAction // �������� ������� ��������� ����� BaseAction // ������� � ��������� ����� // ����� �� ������ �����
{


    public event EventHandler<Unit> OnHealActionStarted;     // �������� ������� �������� (����� ������� ������ � ��������� ����) // � ������� ����� ���������� ����� �������� ����� (� HandleAnimationEvents ����� ��������� ��������)
    public event EventHandler<Unit> OnHealActionCompleted;   // �������� ������� ����������� (��������� ���� � ������� ������)



    private enum State
    {
        HealBefore, //�� ������� (�����������)
        HealAfter,  //����� �������
    }


    private State _state; // ��������� �����
    private float _stateTimer; //������ ���������
    private Unit _targetUnit;// ���� �������� �����

    private int _maxHealDistance = 1; //������������ ��������� �������//����� ���������//
          


    private void Update()
    {
        if (!_isActive) // ���� �� ������� �� ...
        {
            return; // ������� � ���������� ��� ����
        }

        _stateTimer -= Time.deltaTime; // �������� ������ ��� ������������ ���������

        switch (_state) // ������������� ���������� ���� � ����������� �� _state
        {
            case State.HealBefore:

                if (_targetUnit != _unit) // ���� ����� ������� ����� �� ����������� � ��� �������
                {
                    Vector3 targetDirection = (_targetUnit.GetWorldPosition() - transform.position).normalized; // ����������� � �������� �����, ��������� ������
                    float rotateSpeed = 10f; //����� ���������//

                    transform.forward = Vector3.Slerp(transform.forward, targetDirection, Time.deltaTime * rotateSpeed); // ������ �����.
                }

                break;

            case State.HealAfter:
                break;
        }

        if (_stateTimer <= 0) // �� ��������� ������� ������� NextState() ������� � ���� ������� ���������� ���������. �������� - � ���� ���� TypeGrenade.Aiming: ����� � case TypeGrenade.Aiming: ��������� �� TypeGrenade.Shooting;
        {
            NextState(); //��������� ���������
        }
    }

    private void NextState() //������� ������������ ���������
    {
        switch (_state)
        {
            case State.HealBefore:
                _state = State.HealAfter;
                float afterHealStateTime = 3f; // ��� ��������� ���������� ������ ������ ����������  ����������������� ��������� ����� ������� //����� ���������// (������ ��������� � ������������� ��������, ����� ������ ������������ � ������������ ������)
                _stateTimer = afterHealStateTime;
                
                Heal(); // �����
                
                break;

            case State.HealAfter:

                OnHealActionCompleted?.Invoke(this, _targetUnit);  // �������� ������� �������� ������� ����������� (��������� UnitAnimator, ��� ����� �������� ������)

                ActionComplete(); // ������� ������� ������� �������� ���������
                break;
        }

        //Debug.Log(_state);
    }

    private void Heal() // �������
    {
       _targetUnit.Healing(50);
    }




    public override string GetActionName() // ��������� ������� �������� //������� ������������� ������� �������
    {
        return "Heal";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition) //�������� �������� ���������� �� // ������������� ����������� ������� �����
    {
        float HealthNormalized = _unit.GetHealthNormalized(); // ������� ��������������� �������� �����

        if (HealthNormalized <= 0.3) //���� �������� ������ ��� ����� 30% ��
        {
            return new EnemyAIAction
            {
                gridPosition = gridPosition,
                actionValue = 150, //�������� ������� �������� �������� ����� �������. 
            };
        }
        else
        {
            return new EnemyAIAction
            {
                gridPosition = gridPosition,
                actionValue = 50, //�������� ������� �������� ��������. ����� ��������� ������� ���� ������ ������� ������� �� �����, 
            };
        }

    }

    public override List<GridPosition> GetValidActionGridPositionList()// �������� ������ ���������� �������� ������� ��� �������� // ������������� ������� �������
                                                                       // ���������� �������� ������� ��� �������� ������� ����� ������ ��� ����� ���� 
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = _unit.GetGridPosition(); // ������� ������� � ����� �����

        for (int x = -_maxHealDistance; x <= _maxHealDistance; x++) // ���� ��� ����� ����� ������� � ������������ unitGridPosition, ������� ��������� ���������� �������� � �������� ������� _maxHealDistance
        {
            for (int z = -_maxHealDistance; z <= _maxHealDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z, 0); // ��������� �������� �������. ��� ������� ���������(0,0, 0-����) �������� ��� ���� 
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition; // ����������� �������� �������

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) // �������� �������� �� testGridPosition ���������� �������� �������� ���� ��� �� ��������� � ���� �����
                {
                    continue; // continue ���������� ��������� ���������� � ��������� �������� ����� 'for' ��������� ��� ����
                }

                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition)) // �������� �������� ������� ��� ��� ������ (��� ����� ������ � ������� �� ����� �� ��������)
                {
                    // ������� ����� �����, ��� ������
                    continue;
                }

                Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);   // ������� ����� �� ����� ����������� �������� ������� 
                                                                                                // GetUnitAtGridPosition ����� ������� null �� � ���� ���� �� ��������� ������� �������, ��� ��� �������� �� �����
                if (targetUnit.IsEnemy() != _unit.IsEnemy()) // ���� ����������� ���� ���� � ��� ���� ��� (���������� �������)
                {
                    // ��� ������������� � ������ "��������"
                    continue;
                }



                validGridPositionList.Add(testGridPosition); // ��������� � ������ �� ������� ������� ������ ��� �����
                //Debug.Log(testGridPosition);
            }
        }
        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete) // (onActionComplete - �� ���������� ��������). � �������� ����� ���������� ������� Action 
                                                                                        // � ������ ������ �������� �������� ������� �� �� ���������� - GridPosition _gridPosition - �� �������� ���� ��� ���� ����� ��������������� ��������� ������� ������� TakeAction.
                                                                                        // ���� ������ ������, ������� ���������� - public class BaseParameters{} 
                                                                                        // � ����������� � ������� ����� �������������� ��� ������� �������� -
                                                                                        // public SpinBaseParameters : BaseParameters{}
                                                                                        // ����� ������� - public override void TakeAction(BaseParameters baseParameters ,Action onActionComplete){
                                                                                        // SpinBaseParameters spinBaseParameters = (SpinBaseParameters)baseParameters;}
    {
        _targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition); // ������� ����� �������� ����� �������� (��� ����� ���� � ��� ����)

        _state = State.HealBefore; // ���������� ��������� ���������� �� �������
        float beforeHealStateTime = 0.5f; //�� �������.  ��� ��������� ���������� ������ ������ ����������  ����������������� ��������� ���������� ����� �������� ..//����� ���������//
        _stateTimer = beforeHealStateTime;

        OnHealActionStarted?.Invoke(this, _targetUnit); // �������� ������� �������� ������� �������� � ��������� ���� ����� ��������� UnitAnimator (��������� ������)
        ActionStart(onActionComplete); // ������� ������� ������� ����� �������� // �������� ���� ����� � ����� ����� ���� �������� �.�. � ���� ������ ���� EVENT � �� ������ ����������� ����� ���� ��������
    }

    public override int GetActionPointCost() // ������������� ������� ������� // �������� ������ ����� �� �������� (��������� ��������)
    {
        return 2;
    }


}
