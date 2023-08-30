using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class SpotterFireAction : BaseAction // �������� �������������� ���� ��������� ����� BaseAction // ������� � ��������� ����� // ����� �� ������ �����
{

    public event EventHandler OnSpotterFireActionStarted;     // �������� �������������� ����  �������� // � ������� ����� ���������� ����� �������� ����� (� HandleAnimationEvents ����� ��������� ��������)
    public event EventHandler OnSpotterFireActionCompleted;   // �������� �������������� ����  ����������� (��������� ���� � ������� ������)



    private enum State
    {
        SpotterFireBefore, //�� 
        SpotterFireAfter,  //����� 
    }


    [SerializeField] private Transform _spotterFireFXPrefab; // ����� ����������
    private List<Transform> _spotterFireFXList; // ������ ��������� ������ ����������
    private State _state; // ��������� �����
    private float _stateTimer; //������ ���������
    private Unit _partnerUnit;// ���� � ������� ������������ �����    

    private int _maxSpotterFireDistance = 1; //������������ ��������� ������ ����� ��� ������������� ����//����� ���������//

    private void Start()
    {
        UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged; // ��������� ���� �������
    }

    private void UnitActionSystem_OnSelectedUnitChanged(object sender, EventArgs e)
    {
        if (_partnerUnit != null) // ���� ���� �������
        {
            Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
            if (selectedUnit != _partnerUnit) // ���� ���������� ���� �� ������� ��
            {
                _partnerUnit.GetAction<ShootAction>().�learSpotterFireUnit(); // �������� � �������� ���� ��������������� ����
                foreach (Transform spotterFireFX in _spotterFireFXList) // ������ �����
                {
                    Destroy(spotterFireFX.GameObject()); // ��������� �����
                }
                _partnerUnit = null; // ������� ��������

                OnSpotterFireActionCompleted?.Invoke(this, EventArgs.Empty); // �������� ������� �������� ������������� ����������� ��������� UnitAnimator (�������� ������)
            }
        }
    }



    private void Update()
    {
        if (!_isActive) // ���� �� ������� �� ...
        {
            return; // ������� � ���������� ��� ����
        }

        switch (_state) // ������������� ���������� ���� � ����������� �� _state
        {
            case State.SpotterFireBefore:

                NextState();
                break;

            case State.SpotterFireAfter:
                NextState();
                break;
        }
    }

    private void NextState() //������� ������������ ���������
    {
        switch (_state)
        {
            case State.SpotterFireBefore:

                _state = State.SpotterFireAfter;

                break;

            case State.SpotterFireAfter:

                ActionComplete(); // ������� ������� ������� �������� ���������

                break;
        }

        //Debug.Log(_state);
    }

    public override string GetActionName() // ��������� ������� �������� //������� ������������� ������� �������
    {
        return "��������";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition) //�������� �������� ���������� �� // ������������� ����������� ������� �����
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 40, //�������� ������� �������� ��������. ����� ��������� ������� ���� ������ ������� ������� �� �����, 
        };
    }

    public override List<GridPosition> GetValidActionGridPositionList()// �������� ������ ���������� �������� ������� ��� �������� // ������������� ������� �������
                                                                       // ���������� �������� ������� ��� �������� ������� ����� ������ ��� ����� ���� 
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = _unit.GetGridPosition(); // ������� ������� � ����� �����

        for (int x = -_maxSpotterFireDistance; x <= _maxSpotterFireDistance; x++) // ���� ��� ����� ����� ������� � ������������ unitGridPosition, ������� ��������� ���������� �������� � �������� ������� _maxComboDistance
        {
            for (int z = -_maxSpotterFireDistance; z <= _maxSpotterFireDistance; z++)
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

                // �������� �������� �� ������������� �����
                int actionPoint = targetUnit.GetActionPoints(); // �������� ���� �������� � ������������ �����                
                if (actionPoint == 0)
                {
                    // �� ������ ��� �� �������
                    continue;
                }
                if (targetUnit == _unit)
                {
                    // � ����� ����� ������ ������
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
        _partnerUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition); // ������� ����� � �������� ����� �������������� �����
        _state = State.SpotterFireBefore; // ���������� ��������� ���������� 

        // ������ ����� ������������ � ������
        Transform unitAimPoinTransform = _unit.GetAction<ShootAction>().GetAimPoinTransform(); 
        Transform partnerAimPoinTransform = _partnerUnit.GetAction<ShootAction>().GetAimPoinTransform();
        _spotterFireFXList = new List<Transform> // �������� ����� � ������� � ������
        {
            Instantiate(_spotterFireFXPrefab, unitAimPoinTransform.position, Quaternion.identity, unitAimPoinTransform),
            Instantiate(_spotterFireFXPrefab, partnerAimPoinTransform.position, Quaternion.identity ,partnerAimPoinTransform)
        };

        SoundManager.Instance.PlaySoundOneShot(SoundManager.Sound.Spotter);

        _partnerUnit.GetAction<ShootAction>().SetSpotterFireUnit(_unit); // ��������� ��������, ����, ��� �������. ����
        UnitActionSystem.Instance.SetSelectedUnit(_partnerUnit, _partnerUnit.GetAction<ShootAction>()); //������� �������� ���������� � ������� �������� ��������


        OnSpotterFireActionStarted?.Invoke(this, EventArgs.Empty); // �������� ������� �������� ������������� �������� ��������� UnitAnimator (��������� ������)
        ActionStart(onActionComplete); // ������� ������� ������� ����� �������� // �������� ���� ����� � ����� ����� ���� �������� �.�. � ���� ������ ���� EVENT � �� ������ ����������� ����� ���� ��������
    }

    public override int GetActionPointCost() // ������������� ������� ������� // �������� ������ ����� �� �������� (��������� ��������)
    {
        return 2;
    }
    public override int GetMaxActionDistance()
    {
        return _maxSpotterFireDistance;
    }
   
    public Transform GetSpotterFireFXPrefab()
    {
        return _spotterFireFXPrefab;
    }

    private void OnDestroy()
    {
        UnitActionSystem.Instance.OnSelectedUnitChanged -= UnitActionSystem_OnSelectedUnitChanged; // ��������� ���� �������
    }

    public override string GetToolTip()
    {
        return "����� �������� ����� 2  �����" + "\n" +
            "���� - " + GetActionPointCost()+"  ����������� � ������� �����" + "\n" +
            "��������� - " + GetMaxActionDistance() + "\n" +
        "� ��������� ������������� ���� � ������ �������� �� 50%," + "\n" +
        " � �������� ����������� 100%)\r\n";
    }
}
