using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class ComboAction : BaseAction // ����� // �������� ����� ��������� ������ ��� �������� ����� ������������
{
    public static event EventHandler<OnComboEventArgs> OnAnyUnitComboStateChanged; // � ������ ����� ���������� ��������� ����� 
                                                                                   // static - ���������� ��� event ����� ������������ ��� ����� ������ �� �������� �� ���� ������� � ��� �������� ������.                                                                                
    public class OnComboEventArgs : EventArgs // �������� ����� �������, ����� � ��������� ������� �������� ������ ������
    {
        public Unit partnerUnit; // ���� ������� �� ������� ���� �������� ���������
        public State state; // ���������
    }

    public event EventHandler<Unit> OnComboActionStarted;     // �������� ����� �������� 
    public event EventHandler<Unit> OnComboActionCompleted;   // �������� ����� ����������� 

    public enum State
    {
        ComboSearchPartner, //����� �������� ��� �����
        ComboSearchEnemy,   //����� �����
        ComboStart,         //����� �����
        ComboAfter,         //����� �����
    }

    [SerializeField] private LayerMask _obstaclesDoorMousePlaneCoverLayerMask; //����� ���� ����������� ���� ������� Obstacles � DoorInteract � MousePlane(���) Cover// ����� �� ���� ������ � ���� ���������� ����� ����� -Obstacles, � �� ������ -DoorInteract //��� ����� ������� ������ �������� �������� �� Box collider ����� ����� ����� ����� ������������� ������� ���� 
    [SerializeField] private Transform _comboPartnerFXPrefab; // ���������� ����� ���������� ��������

    private State _state; // ��������� �����
    private float _stateTimer; //������ ���������
    private Unit _unitPartner; // ���� ������� � ������� ����� ������ �����
    private Unit _unitEnemy;  // ���� ����    
    private GridPosition _targetPointEnemyGridPosition; // ����� ����������� �����
    private Transform _comboPartnerFXPrefabInstantiateTransform; // ��������� ������
    private RopeRanderer _ropeRandererUnit;
    private RopeRanderer _ropeRandererParten;

    private int _searchEnemyPointCost = 2; // ��������� ������ �����
    private int _maxComboPartnerDistance = 1; //������������ ��������� ����� ��� ������ �������� � �������������� �����//����� ���������//
    private int _maxComboEnemyDistance = 5; //������������ ��������� ����� ��� ������ �����//����� ���������//
    private float zOffset = 0; // 

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        _state = State.ComboSearchPartner; // ��������� ��������� �� ��������� �.�. ���������� � ������ GetValidActionGridPositionList

        _ropeRandererUnit = _unit.GetUnitRope().GetRopeRanderer();

        ComboAction.OnAnyUnitComboStateChanged += ComboAction_OnAnyUnitComboStateChanged;
        UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged; //��������� ���� �������//
    }

    private void Update()
    {
        if (!_isActive) // ���� �� ������� �� ...
        {
            return; // ������� � ���������� ��� ����
        }

        _stateTimer -= Time.deltaTime; // �������� ������ ��� ������������ ���������

        switch (_state) // ������������� ���������� ���� � ����������� �� _state
        {
            case State.ComboSearchPartner:

                // ����������� � ������� ����� � ��� ����� �����
                float rotateSpeed = 10f;
                Vector3 unitPartnerDirection = (_unitPartner.transform.position - transform.position).normalized; // ����������� � �������� �����, ��������� ������
                transform.forward = Vector3.Slerp(transform.forward, unitPartnerDirection, Time.deltaTime * rotateSpeed); // ������ �����.
                break;

            case State.ComboSearchEnemy:

                HookShootin();

                break;

            case State.ComboStart:

                PullEnemy();

                break;

            case State.ComboAfter:
                break;
        }

        if (_stateTimer <= 0) // �� ��������� ������� ������� NextState() ������� � ���� ������� ���������� ���������. 
        {
            NextState(); //��������� ���������
        }

       // Debug.Log(_state);
    }

    private void NextState() //������� ������������ ��������� (����� ������������ ��� ���������)
    {
        switch (_state)
        {
            case State.ComboSearchPartner:

                _comboPartnerFXPrefabInstantiateTransform = Instantiate(_comboPartnerFXPrefab, transform.position + Vector3.up * 1.7f, Quaternion.identity); // �������� �������� ��������������
                _comboPartnerFXPrefabInstantiateTransform.LookAt(_unitPartner.transform.position + Vector3.up * 1.7f); // � �������� � ������� ��������

                _state = State.ComboSearchEnemy;                
                OnAnyUnitComboStateChanged?.Invoke(this, new OnComboEventArgs // � ����� �������� ���� ������� ���������, ��� �� �� ���� ��������� ��������� ���� �������� (��� GetActionPointCost() ������� �� ���������)
                {
                    partnerUnit = _unitPartner,
                    state = _state
                });
                ActionComplete(); // �������� �������� ������ �������� � ������� ����  (�������� ������� ClearBusy ���������� �� ������ UnitActionSystem, � � UnitActionSystem_OnBusyChanged �� ������ UnitActionSystemUI ������� ��� ��������, ���-�� ������ ���������� ������������ �� ������ ����� ��������)
                break;

            case State.ComboSearchEnemy:
                _state = State.ComboStart;
                ActionComplete();
                break;
            case State.ComboStart:
                _state = State.ComboAfter;                
                OnAnyUnitComboStateChanged?.Invoke(this, new OnComboEventArgs // � ����� �������� ���� ������� ���������, ��� �� �� ���� ����� �� ����� �����
                {
                    partnerUnit = _unitPartner,
                    state = _state
                });
                float ComboAfterStateTime = 0.5f;
                _stateTimer = ComboAfterStateTime;
                break;

            case State.ComboAfter: // � ���� ��������� ������ UI ����������
                Destroy(_comboPartnerFXPrefabInstantiateTransform.GameObject());
                _unitPartner.GetUnitRope().HideRope();
                _unit.GetUnitRope().HideRope();
                ActionComplete(); // ������� ������� ������� �������� ���������
                break;
        }
    }    

    private void HookShootin() // �������� ������
    {
        float rotateSpeed = 10f;
        Vector3 partnerEnemyDirection = (_unitEnemy.transform.position - _unitPartner.transform.position).normalized; // ����������� � �������� �����, ��������� ������
        Vector3 unitEnemyDirection = (_unitEnemy.transform.position - transform.position).normalized; // ����������� � �������� �����, ��������� ������

        // ��������� �������� � ������ �����
        _unitPartner.transform.forward = Vector3.Slerp(_unitPartner.transform.forward, partnerEnemyDirection, Time.deltaTime * rotateSpeed); // ������ �����.
        transform.forward = Vector3.Slerp(transform.forward, unitEnemyDirection, Time.deltaTime * rotateSpeed); // ������ �����.

        // ����� ���������� 
        if (Vector3.Dot(unitEnemyDirection, transform.forward) >= 0.95f) // ����� ���������� 1, ���� ��� ��������� � ����� � ��� �� �����������, -1, ���� ��� ��������� � ���������� ��������������� ������������, � ����, ���� ������� ���������������.
        {
            //�������� ��������
            Vector3 enemuAimPoint = _unitEnemy.GetAction<ShootAction>().GetAimPoinTransform().position; // ����� ������������ ������
            // ��������� ������� � ������� ����� (����� �������� � ��������� Z)
            _ropeRandererParten.transform.LookAt(enemuAimPoint);
            _ropeRandererUnit.transform.LookAt(enemuAimPoint);          

            float speedShootRope = 12;
            zOffset += Time.deltaTime * speedShootRope;

            if (zOffset <= Vector3.Distance(_unitPartner.transform.position, _unitEnemy.transform.position))  // ���� ��������� �� ������� ������� ������ ��� ��������� ��������� // �� �� �������� ����        
            {
                _ropeRandererParten.RopeDraw(Vector3.forward * zOffset);// ������ �������       
            }
            if (zOffset <= Vector3.Distance(transform.position, _unitEnemy.transform.position))  // ���� ��������� �� ������� ������� ������ ��� ��������� ��������� // �� �� �������� ����        
            {
                _ropeRandererUnit.RopeDraw(Vector3.forward * zOffset); // ������ �������               
            }

            if (zOffset >= Vector3.Distance(_unitPartner.transform.position, _unitEnemy.transform.position) &&
                zOffset >= Vector3.Distance(transform.position, _unitEnemy.transform.position))
            {
                SoundManager.Instance.PlaySoundOneShot(SoundManager.Sound.HookShoot);
                NextState(); //��������� ���������
            }
        }
    }

    private void PullEnemy() // ����� �����
    {
        Vector3 targetPointEnemyWorldPosition = LevelGrid.Instance.GetWorldPosition(_targetPointEnemyGridPosition); // ������� ������� ���� ���� ����������� �����                

        Vector3 moveEnemyDirection = (targetPointEnemyWorldPosition - _unitEnemy.transform.position).normalized; // ����������� ��������, ��������� ������

        float moveEnemySpead = 6f; //����� ���������//
        _unitEnemy.transform.position += moveEnemyDirection * moveEnemySpead * Time.deltaTime;              

        // ��������� �������� � ����� � ������� �����
        _unitPartner.transform.LookAt(_unitEnemy.transform);
        transform.LookAt(_unitEnemy.transform);

        // ��������� ��������� �� �������� �� ����� � �� ����� �� �����
        float zDistancePartner = Vector3.Distance(_unitPartner.transform.position, _unitEnemy.transform.position);
        _ropeRandererParten.RopeDraw(Vector3.forward * zDistancePartner);// ������ ������� 
        float zDistanceUnit = Vector3.Distance(transform.position, _unitEnemy.transform.position);
        _ropeRandererUnit.RopeDraw(Vector3.forward * zDistanceUnit); // ������ ������� 

        float stoppingDistance = 0.2f; // ��������� ��������� //����� ���������//
        if (Vector3.Distance(_unitEnemy.transform.position, targetPointEnemyWorldPosition) < stoppingDistance)  // ���� ��������� �� ������� ������� ������ ��� ��������� ��������� // �� �������� ����        
        {
            float stunPercent = 0.3f; // ������� ���������
            _unitEnemy.Stun(stunPercent); //����� ���������// �������
            SoundManager.Instance.PlaySoundOneShot(SoundManager.Sound.HookPull);
            NextState(); //��������� ���������
        }
    }

    private void UnitActionSystem_OnSelectedUnitChanged(object sender, EventArgs e) // ���� �� ����� ���������� ����� � ������� ������� �����, �� ����� ���� ����������
    {
        if (_state == State.ComboSearchEnemy) // ���� ���� ���� � ��������� ������ �����
        {
            _state = State.ComboSearchPartner;
            if (_comboPartnerFXPrefabInstantiateTransform != null) // ���� ������� �������� ��������������
            {
                Destroy(_comboPartnerFXPrefabInstantiateTransform.GameObject()); // ���������
            }
        };
    }

    private void ComboAction_OnAnyUnitComboStateChanged(object sender, OnComboEventArgs e)
    {
        if (e.partnerUnit == _unit) // ���� ������� ��� ����� - ��� � ��
        {
            SetState(e.state); // �������� ��� ���������
        };
    }

    public override string GetActionName() // ��������� ������� �������� //������� ������������� ������� �������
    {
        return "����";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition) //�������� �������� ���������� �� // ������������� ����������� ������� �����
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 40, //�������� ������� �������� ��������. ����� ��������� ����� ���� ������ ������� ������� �� �����, 
        };
    }

    public override List<GridPosition> GetValidActionGridPositionList()// �������� ������ ���������� �������� ������� ��� �������� // ������������� ������� �������                                                                     
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = _unit.GetGridPosition(); // ������� ������� � ����� �����

        int maxComboDistance = GetMaxComboDistance();      
        for (int x = -maxComboDistance; x <= maxComboDistance; x++) // ���� ��� ����� ����� ������� � ������������ unitGridPosition, ������� ��������� ���������� �������� � �������� ������� maxComboDistance
        {
            for (int z = -maxComboDistance; z <= maxComboDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z, 0); // ��������� �������� �������. ��� ������� ���������(0,0, 0-����) �������� ��� ���� 
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition; // ����������� �������� �������
                Unit targetUnit = null;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) // �������� �������� �� testGridPosition ���������� �������� �������� ���� ��� �� ��������� � ���� �����
                {
                    continue; // continue ���������� ��������� ���������� � ��������� �������� ����� 'for' ��������� ��� ����
                }

                //�������� �������� ������� ������� ����� � �������
                if (PathfindingMonkey.Instance.GetGridPositionInAirList().Contains(testGridPosition))
                {
                    continue;
                }

                switch (_state)
                {
                    default:
                    case State.ComboSearchPartner:

                        if (_unit.GetActionPoints() < _searchEnemyPointCost) // ���� � ����� �� ������� ����� ��� ����������� �������� �� (�.�. ����� ��������� ������ �� �����, ���� ��� ����� �� � ����� ������)
                        {
                            return validGridPositionList; // ������ ������ ������
                        }

                        if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition)) // �������� �������� ������� ��� ��� ������ 
                        {
                            // ������� ����� �����, ��� ������
                            continue;
                        }

                        // ���� ���� �������� �� �������� ������
                        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);   // ������� ����� �� ����� ����������� �������� �������  // GetUnitAtGridPosition ����� ������� null �� � ���� ���� �� ��������� ������� �������, ��� ��� �������� �� �����                        
                        if (targetUnit.IsEnemy() != _unit.IsEnemy()) // ���� ����������� �� � ���� ������� (���������� ���)
                        {
                            continue;
                        }

                        // �������� �������� �� ������������� �����
                        int actionPoint = targetUnit.GetActionPoints(); // �������� ���� �������� � ������������ �����                
                        if (actionPoint < _searchEnemyPointCost)
                        {
                            // ���� � ���� ��������� ����� �������� �� �� ��� �� �������
                            continue;
                        }

                        if (targetUnit == _unit)
                        {
                            // � ����� ����� ����� ������ ������
                            continue;
                        }
                        break;

                    case State.ComboSearchEnemy:

                        // ��� ������� �������� ������ ������� ���� � �� �������
                        int testDistance = Mathf.Abs(x) + Mathf.Abs(z); // ����� ���� ������������� ��������� �������� �������
                        if (testDistance > maxComboDistance) //������� ������ �� ����� � ���� ����� // ���� ���� � (0,0) �� ������ � ������������ (5,4) ��� �� ������� �������� 5+4>7
                        {
                            continue;
                        }

                        if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition)) // �������� �������� ������� ��� ��� ������ 
                        {
                            // ������� ����� �����, ��� ������
                            continue;
                        }

                        // ���� ���� ����� �� �������� ������������� ������
                        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);   // ������� ����� �� ����� ����������� �������� ������� // GetUnitAtGridPosition ����� ������� null �� � ���� ���� �� ��������� ������� �������, ��� ��� �������� �� �����
                        if (targetUnit.IsEnemy() == _unit.IsEnemy()) // ���� ����������� � ����� ������� (���������� ���)
                        {
                            continue;
                        }

                        // �������� �� ����������������� ������ �� ���� 
                        Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitGridPosition); // ��������� � ������� ���������� ���������� ��� �������� ������� �����  
                        Vector3 HookDirection = (targetUnit.GetWorldPosition() - unitWorldPosition).normalized; //��������������� ������ ����������� �������� �����
                        float heightRaycast = 1.7f; // ������ �������� ���� �� ������ ������ (���� �� ����� ������ ���������)
                        if (Physics.Raycast(
                                unitWorldPosition + Vector3.up * heightRaycast,
                                HookDirection,
                                Vector3.Distance(unitWorldPosition, targetUnit.GetWorldPosition()),
                                _obstaclesDoorMousePlaneCoverLayerMask)) // ���� ��� ����� � ����������� �� (Raycast -������ bool ����������)
                        {
                            // �� �������������� ������������
                            continue;
                        }
                        break;

                    case State.ComboStart:                      

                        if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition)) // �������� �������� ������� � �������. ����� ���������� ������������ ����� �� ������
                        {
                            // ��� ���� - ���� ����������
                            continue;
                        }

                        // �������� �� ����������������� ������ �� ����
                        unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitGridPosition); // ��������� � ������� ���������� ���������� ��� �������� ������� �����
                        Vector3 testWorldPosition = LevelGrid.Instance.GetWorldPosition(testGridPosition);// 
                        HookDirection = (testWorldPosition - unitWorldPosition).normalized; //��������������� ������ ����������� �������� �����
                        heightRaycast = 1.5f; // ������ �������� ����
                        if (Physics.Raycast(
                                unitWorldPosition + Vector3.up * heightRaycast,
                                HookDirection,
                                Vector3.Distance(unitWorldPosition, testWorldPosition),
                                _obstaclesDoorMousePlaneCoverLayerMask)) // ���� ��� ����� � ����������� �� (Raycast -������ bool ����������)
                        {
                            // �� �������������� ������������
                            continue;
                        }

                        if (!PathfindingMonkey.Instance.IsWalkableGridPosition(testGridPosition)) //�������� �������� ������� ��� ������ ������ (���� ����������� ����� �������)
                        {
                            continue;
                        }
                        break;
                }
                validGridPositionList.Add(testGridPosition); // ��������� � ������ �� ������� ������� ������ ��� �����
                                                             
                //Debug.Log(testGridPosition);
            }
        }
        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete) // ���������� ��������  (onActionComplete - �� ���������� ��������). � �������� ����� ���������� ������� Action 
    {
        if (_state == State.ComboAfter)// ���� �� ����� � ����� ���� ��������� - ����� ����� �� 
        {
            _state = State.ComboSearchPartner;
        }

        SetupTakeActionFromState(gridPosition);

        ActionStart(onActionComplete); // ������� ������� ������� ����� �������� ��������� ������ � UPDATE// �������� ���� ����� � ����� ����� ���� �������� �.�. � ���� ������ ���� EVENT � �� ������ ����������� ����� ���� ��������
    }

    private void SetupTakeActionFromState(GridPosition gridPosition) //��������� ���������� �������� � ����������� �� ���������
    {
        switch (_state)
        {
            default:
            case State.ComboSearchPartner: // ������ ��������
                _unitPartner = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition); // ������� ����� ��� �����
                _ropeRandererParten = _unitPartner.GetUnitRope().GetRopeRanderer(); // ������� � �������� ��������� �������
                float ComboSearchPartnerStateTime = 0.5f; //����� ��������.  ��� ��������� ���������� ������ ������ ����������  ����������������� ��������� ����� �������� ..//����� ���������//
                _stateTimer = ComboSearchPartnerStateTime;
                break;

            case State.ComboSearchEnemy:  // ���� ���� ����� ��                 
                _unitPartner.SpendActionPoints(GetActionPointCost()); // ������ � �������� ���� �������� (� ���� ��� ������� � HandleSelectedAction() � ������ UnitActionSystem)
                _unitEnemy = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition); // ��������� �����                
                _unitPartner.GetUnitRope().ShowRope();
                _unit.GetUnitRope().ShowRope();
                // ����� ����� ������� �.�. ��������� ������, �� ����� ���������� ������� ����� � ������ NextStste()
                float ComboSearchEnemyStateTime = 5f;
                _stateTimer = ComboSearchEnemyStateTime;

                // ��������� ������ � ��� // ���������� ������� �� ����� �� ����� ����������� � ��������� ������
                break;

            case State.ComboStart:
                _targetPointEnemyGridPosition = gridPosition; // ������� ����� ���� ���� ����������� �����
                // ����� ����� ������� �.�. ��������� ������,  �� ����� ���������� ������� ����� � ������ NextStste()
                float ComboStartStateTime = 5f;
                _stateTimer = ComboStartStateTime;
                break;
        }
    }

    public override int GetActionPointCost() // ������������� ������� ������� // �������� ������ ����� �� �������� (��������� ��������)
    {
        switch (_state)
        {
            default:
            case State.ComboSearchPartner:
            case State.ComboStart: // ���� ��������� ��� ��������� �� �����, �� ����� ���������� ������� �� ����
                return 0; // ����� �������� ��� ����� ������ �� �����              
            case State.ComboSearchEnemy:
                return _searchEnemyPointCost;
        }
    }

    public int GetMaxComboDistance()
    {
        int maxComboDistance;
        switch (_state)
        {
            default:
            case State.ComboSearchPartner:

                maxComboDistance = _maxComboPartnerDistance;
                break;

            case State.ComboSearchEnemy:
                maxComboDistance = _maxComboEnemyDistance;
                break;

            case State.ComboStart:
                maxComboDistance = _maxComboPartnerDistance;
                break;
        }
        return maxComboDistance;
    }    
    public State GetState()
    {
        return _state;
    }
    private State SetState(State state)
    {
        return _state = state;
    }
}