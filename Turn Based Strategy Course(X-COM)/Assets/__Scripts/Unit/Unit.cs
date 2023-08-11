using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Unit : MonoBehaviour // ���� ���� ����� �������� �� ������� �� ����� � ���� ��������, ��������� �����
{

    private const int ACTION_POINTS_MAX = 3; //���� ���������//

    // ��� // ������� 2 //� UnitActionSystemUI
    public static event EventHandler OnAnyActionPointsChanged;  // static - ���������� ��� event ����� ������������ ��� ����� ������ �� �������� �� ���� ������� � ��� �������� ������. ������� ��� ������������� ����� ������� ��������� �� ����� ������ �� �����-���� ���������� �������, ��� ����� �������� ������ � ������� ����� �����, ������� ����� ��������� ���� � �� �� ������� ��� ������ �������. 
                                                                // ��������� ����� �������� � ������(Any) ����� � �� ������ � ����������.
    public static event EventHandler OnAnyFriendlyUnitDamage; //����� ������������� ���� ������� ����
    public static event EventHandler OnAnyUnitSpawned; // ������� ����� ���������(���������) ����
    public static event EventHandler OnAnyUnitDead; // ������� ����� ������� ����


    [SerializeField] private bool _isEnemy; //� ���������� � ������� ����� ��������� �������

    // ������� ������
    private GridPosition _gridPosition;
    private HealthSystem _healthSystem;
    private BaseAction[] _baseActionsArray; // ������ ������� �������� // ����� ������������ ��� �������� ������   
    private UnitRope _unitRope;
    private int _actionPoints = ACTION_POINTS_MAX; // ���� ��������
    private float _penaltyStunPercent;  // �������� ������� ��������� (����� ��������� � ���� ���)
    private bool _stunned = false; // ����������(�� ��������� ����)
    /*private int _startStunTurnNumber; //����� ������� (����) ��� ������ ������� ���������
    private int _durationStunEffectTurnNumber; // ����������������� ����������� ������� ���������� �����*/

    private void Awake()
    {
        _healthSystem = GetComponent<HealthSystem>();

        if (TryGetComponent<UnitRope>(out UnitRope unitRope))
        {
            _unitRope = unitRope;
        }
        _baseActionsArray = GetComponents<BaseAction>(); // _moveAction � _spinAction ����� ����� ��������� ������ ����� �������
    }

    private void Start()
    {
        // ����� Unit ����������� �� ��������� ���� ��������� � ����� � ��������� ���� � GridObject(�������� �����) � ������ ������
        _gridPosition = LevelGrid.Instance.GetGridPosition(transform.position); //������� ������� ����� �� �����. ��� ����� ����������� ������� ������� ����� � ������� �� �����
        LevelGrid.Instance.AddUnitAtGridPosition(_gridPosition, this); // ������ � LevelGrid ������� ������ � ������������ ���������� � ������� AddUnitAtGridPosition

        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged; // ������. �� ������� ��� �������        
        _healthSystem.OnDead += HealthSystem_OnDead; // ������������� �� Event. ����� ����������� ��� ������ �����

        OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty); // �������� ������� ����� ���������(���������) ����. ������� ��������� ������� ����� ����������� ��� ���� ��������� ������
    }


    private void Update()
    {
        GridPosition newGridPosition = LevelGrid.Instance.GetGridPosition(transform.position); //������� ����� ������� ����� �� �����.
        if (newGridPosition != _gridPosition) // ���� ����� ������� �� ����� ���������� �� ��������� �� ...
        {
            // ������� ��������� ����� �� �����
            GridPosition oldGridPosition = _gridPosition; // �������� ������ ������� ��� �� �������� � event
            _gridPosition = newGridPosition; //������� ������� - ����� ������� ����������� �������

            LevelGrid.Instance.UnitMovedGridPosition(this, oldGridPosition, newGridPosition); //� UnitMovedGridPosition ��������� �������. ������� ��� ������ �������� � ������ . ����� �� ��������� ������� ����� ����������� � ���� ��� �� ���������
        }
    }

    public T GetAction<T>() where T : BaseAction //�������� ��� ��������� ������ ���� �������� �������� // �������� ����� � GENERICS � ��������� ������ �  BaseAction
    {
        foreach (BaseAction baseAction in _baseActionsArray) // ��������� ������ ������� ��������
        {
            if (baseAction is T) // ���� T ��������� � ����� ������ baseAction �� ...
            {
                return baseAction as T; // ������ ��� ������� �������� ��� � // (T)baseAction; - ��� ���� ����� ������
            }
        }
        return null; // ���� ��� ���������� �� ������ ����
    }

    public GridPosition GetGridPosition() // �������� �������� �������
    {
        return _gridPosition;
    }


    public Vector3 GetWorldPosition() // �������� ������� �������
    {
        return transform.position;
    }

    public BaseAction[] GetBaseActionsArray() // �������� ������ ������� ��������
    {
        return _baseActionsArray;
    }

    public bool TrySpendActionPointsToTakeAction(BaseAction baseAction) // ��������� ��������� ���� ��������, ����� ��������� �������� // ���� ����� ��������� ������ ��� ������ ������
    {
        if (CanSpendActionPointsToTakeAction(baseAction))
        {
            SpendActionPoints(baseAction.GetActionPointCost());
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CanSpendActionPointsToTakeAction(BaseAction baseAction) //�� ����� ��������� ���� ��������, ����� ��������� �������� ? 
    {
        if (_actionPoints >= baseAction.GetActionPointCost()) // ���� ����� �������� ������� ��...
        {
            return true; // ����� ��������� ��������
        }
        else
        {
            return false; // ��� ����� �� �������
        }

        /*// �������������� ������ ���� ����
        return _actionPoints >= baseAction.GetActionPointCost();*/
    }

    public void SpendActionPoints(int amount) //��������� ���� �������� (amount- ���������� ������� ���� ���������)
    {
        _actionPoints -= amount;

        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty); // ��������� ������� ����� ���������� ����� ��������.(��� // ������� // 2 //� UnitActionSystemUI)
    }

    public int GetActionPoints() // �������� ���� ��������
    {
        return _actionPoints;
    }


    public void TurnSystem_OnTurnChanged(object sender, EventArgs empty) //��� ������� ������� ���� �������� �� ������������
    {
        if ((IsEnemy() && !TurnSystem.Instance.IsPlayerTurn()) || // ���� ��� ���� � ��� ������� (�� ������� ������) ��� ��� �� ����(�����) � ������� ������ ��...
            (!IsEnemy() && TurnSystem.Instance.IsPlayerTurn()))
        {
            _actionPoints = ACTION_POINTS_MAX;
            
            if (_penaltyStunPercent!=0)
            {
                _actionPoints -= Mathf.RoundToInt(_actionPoints * _penaltyStunPercent); // �������� �����
                _penaltyStunPercent = 0;
                SetStunned(false); // �������� ���������
            }

            //  ����� ������� ���������������� �� ���� ���
            /*int passedTurnNumber = TurnSystem.Instance.GetTurnNumber() - _startStunTurnNumber;// ������ ����� �� ������ ���������
            if (passedTurnNumber <= _durationStunEffectTurnNumber) // ���� ����� ������ ������ ��� ����� ������������ ��������� (������ ��������� ��� ���������)
            {
                _actionPoints -= Mathf.RoundToInt(_actionPoints * _penaltyStunPercent); // �������� �����
                _penaltyStunPercent = _penaltyStunPercent *0.3f; // �������� ����� ������� 30% �� ������������ (��� ���� ���� ��������� ������� ��������� �����)
            }
            if (passedTurnNumber > _durationStunEffectTurnNumber) //���� ����� ������ ������ ����������������� ���������
            {
                SetStunned(false); // �������� ���������
                _penaltyStunPercent = 0; // � ������� �����
            }  */

            OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty); // ��������� ������� ����� ���������� ����� ��������.(��� // ������� // 2 //� UnitActionSystemUI)
        }
    }

    public bool IsEnemy() // �������� ����
    {
        return _isEnemy;
    }

    public void Healing(int healingAmount) // ��������� (� �������� �������� �������� ����������������� ��������)
    {
        _healthSystem.Healing(healingAmount);
    }

    public void Damage(int damageAmount) // ���� (� �������� �������� �������� �����������)
    {
        _healthSystem.Damage(damageAmount);
        if(!_isEnemy)// ���� �� ���� ��
        {
            OnAnyFriendlyUnitDamage?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Stun(float stunPercent) // �������� �� stunPercent(������� ���������)
    {
        SetStunned(true);
        _penaltyStunPercent = stunPercent; // ��������� �������� ���������

        /*// ����� ������� ���������������� �� ���� ���
        _startStunTurnNumber = TurnSystem.Instance.GetTurnNumber(); // ������� ��������� ����� ����              
        if (_actionPoints > 0) // ���� ����� ���� ������ ����
        {
            _durationStunEffectTurnNumber = 1; //����� ���������// ��������� ����� ������� ���� ��������� ���
        }
        if(_actionPoints<=0) // ���� ����� ���� ����
        {
            _durationStunEffectTurnNumber = 3; //����� ���������// ��������� ����� ������� ��������� 3 ���� (����� ��� �����)
        }*/
        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty); // ��������� ������� ����� ���������� ����� ��������.
    }


    private void HealthSystem_OnDead(object sender, EventArgs e) // ����� ����������� ��� ������ �����

    {
        LevelGrid.Instance.RemoveUnitAtGridPosition(_gridPosition, this); // ������ �� �������� ������� �������� �����

        Destroy(gameObject); // ��������� ������� ������ � �������� ���������� ������ ������

        // ������� ������ ��������� ����� ���� ������� ��� ���������� �����        

        OnAnyUnitDead?.Invoke(this, EventArgs.Empty); // �������� ������� ����� ������� ����. ������� ��������� ������� ����� ����������� ��� ������ �������� �����      
    }

    public float GetHealthNormalized() // �������� ��� ������
    {
        return _healthSystem.GetHealthNormalized();
    }
    public int GetHealth() // �������� ��� ������
    {
        return _healthSystem.GetHealth();
    }
    public int GetHealthMax() // �������� ��� ������
    {
        return _healthSystem.GetHealthMax();
    }
    public bool IsDead()
    {
        return _healthSystem.IsDead();
    }
    public HealthSystem GetHealthSystem()
    {
        return _healthSystem;
    }

    public UnitRope GetUnitRope()
    {
        return _unitRope;
    }

    public bool GetStunned()
    {
        return _stunned;
    }
    private void SetStunned(bool stunned)
    {
        _stunned = stunned;
    }

}
