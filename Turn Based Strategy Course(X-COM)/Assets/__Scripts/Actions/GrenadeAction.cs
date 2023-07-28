using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeAction : BaseAction // ������� ��������. ��������� BaseAction
{

    public event EventHandler OnGrenadeActionStarted;     // �������� ������ ������� ��������    
    public event EventHandler OnGrenadeActionCompleted;   // �������� ������ ������� �����������

    private enum State
    {
        GrenadeBefore, //�� ������� ������� (�����������)
        GrenadeInstantiate, //�������� �������
        GrenadeAfter,  //����� ������� �������
    }


    private State _state; // ��������� �����
    private float _stateTimer; //������ ���������


    [SerializeField] private Transform _grenadeProjectilePrefab; // ������ ������ ������� // � ������� ����� �������� ������ �������
    [SerializeField] private Transform _grenadeSpawnTransform; // ��������� �������� ������� // � ������� ����� �������� ������ �������
    [SerializeField] private LayerMask _obstaclesAndDoorLayerMask; //����� ���� ����������� � ����� (�������� � ����������) ���� ������� Obstacles � DoorInteract MousePlane(��� ��� ���������� ������) // ����� �� ���� ������ � ���� ���������� ����� ����� -Obstacles, � �� ������ -DoorInteract


    private int _maxThrowDistance = 7; //������������ ��������� ������ //����� ���������//

    private GrenadeProjectile _grenadeProjectile;
    private HandleAnimationEvents _handleAnimationEvents; // ���������� ������������ �������
    private GridPosition _targetGridPositin;

    protected override void Awake()
    {
        base.Awake();

        _grenadeProjectile = _grenadeProjectilePrefab.GetComponent<GrenadeProjectile>();

        _handleAnimationEvents = GetComponentInChildren<HandleAnimationEvents>();
    }

    private void Start()
    {
        _handleAnimationEvents.OnAnimationTossGrenadeEventStarted += _handleAnimationEvents_OnAnimationTossGrenadeEventStarted; // ���������� �� ������� "� �������� "������ �������" ���������� �������"
    }


    private void Update()
    {
        if (!_isActive) // ���� �� ������� �� ...
        {
            return; // ������� � ���������� ��� ����
        }

        //���� �������� ��� ������ ����� �� �� ������ ������ ��������� ������� �� ���������� ���� ������ ������� ������� �� ���� (��� � �������� ���� �� ��������� ���� ��������)
        //������� ������ ������� ����� �������� ����� ������� �� ����� ������� ����� ��� ���������
        //ActionComplete(); // 

        _stateTimer -= Time.deltaTime; // �������� ������ ��� ������������ ���������

        switch (_state) // ������������� ���������� ���� � ����������� �� _state
        {
            case State.GrenadeBefore:

                Vector3 targetPositin = LevelGrid.Instance.GetWorldPosition(_targetGridPositin);
                Vector3 targetDirection = (targetPositin - transform.position).normalized; // ����������� � ������� ������, ��������� ������
                targetDirection.y = 0; // ����� ���� �� ���������� ��� ������ (�.�. ������ ����� �������������� ������ �� ��������� x,z)

                float rotateSpeed = 10f; //����� ���������//
                transform.forward = Vector3.Slerp(transform.forward, targetDirection, Time.deltaTime * rotateSpeed); // ������ �����.                

                break;

            case State.GrenadeInstantiate:// ����� ����� ��������� �������� ������� (������ ��������� AnimationEvent)
                break;

            case State.GrenadeAfter: // ���� ������ �� ����� �������� �������� ��������� ��� �������             
                break;
        }

        if (_stateTimer <= 0) // �� ��������� ������� ������� NextState() ������� � ���� ������� ���������� ���������. �������� - � ���� ���� State.Aiming: ����� � case State.Aiming: ��������� �� State.Shooting;
        {
            NextState(); //��������� ���������
        }
    }

    private void NextState() //������� ������������ ���������
    {
        switch (_state)
        {
            case State.GrenadeBefore:

                _state = State.GrenadeInstantiate;
                //float grenadeInstantiateStateTime = 0.5f; // ��� ��������� ���������� ������ ������ ����������  ����������������� ��������� �������� ������� //����� ���������// ����� ����� ��������� ����� �������� ������� (������ ��������� AnimationEvent)
                //_stateTimer = grenadeInstantiateStateTime;                               

                break;

            case State.GrenadeInstantiate:
                break;

            case State.GrenadeAfter:
                break;
        }

        //Debug.Log(_state);
    }


    public override string GetActionName() // ��������� ������� �������� //������� ������������� ������� �������
    {
        return "Grenade";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition) //�������� �������� ���������� �� // ������������� ����������� ������� �����
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 0, //�������� ������ �������� ��������. ����� ������� ������� ���� ������ ������� ������� �� �����, 
        };
    }

    public override List<GridPosition> GetValidActionGridPositionList()// �������� ������ ���������� �������� ������� ��� �������� // ������������� ������� �������                                                                       
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = _unit.GetGridPosition(); // ������� ������� � ����� �����

        for (int x = -_maxThrowDistance; x <= _maxThrowDistance; x++) // ���� ��� ����� ����� ������� � ������������ unitGridPosition, ������� ��������� ���������� �������� � �������� ������� _maxHealDistance
        {
            for (int z = -_maxThrowDistance; z <= _maxThrowDistance; z++)
            {
                for (int floor = -_maxThrowDistance; floor <= _maxThrowDistance; floor++)
                {

                    GridPosition offsetGridPosition = new GridPosition(x, z, floor); // ��������� �������� �������. ��� ������� ���������(0,0, 0-����) �������� ��� ���� 
                    GridPosition testGridPosition = unitGridPosition + offsetGridPosition; // ����������� �������� �������

                    if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) // �������� �������� �� testGridPosition ���������� �������� �������� ���� ��� �� ��������� � ���� �����
                    {
                        continue; // continue ���������� ��������� ���������� � ��������� �������� ����� 'for' ��������� ��� ����
                    }

                    // ��� ������� ������� ������� ������� ���� � �� �������
                    int testDistance = Mathf.Abs(x) + Mathf.Abs(z); // ����� ���� ������������� ��������� �������� �������
                    if (testDistance > _maxThrowDistance) //������� ������ �� ����� � ���� ����� // ���� ���� � (0,0) �� ������ � ������������ (5,4) ��� �� ������� �������� 5+4>7
                    {
                        continue;
                    }

                    /*if (!PathfindingMonkey.Instance.HasPath(unitGridPosition, testGridPosition)) //�������� �������� ������� ���� ������ ������ ��� �� ��� ���� ������ � ����� (Obstacles -�����������)  (������� ����� ������ � ����������� ��������)
                    {
                        continue;
                    }*/

                    int pathfindingDistanceMultiplier = 10; // ��������� ���������� ����������� ���� (� ������ PathfindingMonkey ������ ��������� �������� �� ������ � ��� ����� ����� 10 �� ��������� 14, ������� ������� ��� ��������� �� ���������� ������)
                    if (PathfindingMonkey.Instance.GetPathLength(unitGridPosition, testGridPosition) > _maxThrowDistance * pathfindingDistanceMultiplier) //�������� �������� ������� - ���� ��������� �� ����������� ������ ������ ���������� ������� ���� ����� ���������� �� ���� ���
                    {
                        // ����� ���� ������� ������
                        continue;
                    }

                    // �������� �� ����������� ������ ����� �����������
                    Vector3 worldTestGridPosition = LevelGrid.Instance.GetWorldPosition(testGridPosition);   // ������� ������� ���������� ����������� �������� ������� 
                    Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitGridPosition); // ��������� � ������� ���������� ���������� ��� �������� ������� �����  
                    Vector3 grenadeDirection = (worldTestGridPosition - unitWorldPosition).normalized; //��������������� ������ ����������� ������ �������

                    float unitShoulderHeight = 1.7f; // ������ ����� �����, � ���������� ����� ������������� ���������� � ������������ �������
                    if (Physics.Raycast(
                            unitWorldPosition + Vector3.up * unitShoulderHeight,
                            grenadeDirection,
                            Vector3.Distance(unitWorldPosition, worldTestGridPosition),
                            _obstaclesAndDoorLayerMask)) // ���� ��� ����� � ����������� �� (Raycast -������ bool ����������)
                    {
                        // �� �������������� ������������
                        continue;
                    }

                    //�������� �������� ������� ������� ����� � �������
                    if (PathfindingMonkey.Instance.GetGridPositionInAirList().Contains(testGridPosition)) 
                    {
                        continue;
                    }


                    validGridPositionList.Add(testGridPosition); // ��������� � ������ �� ������� ������� ������ ��� �����
                                                                 
                    //Debug.Log(testGridPosition);
                }
            }
        }
        return validGridPositionList;
    }



    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)  // ������������� TakeAction (��������� �������� (�����������). (������� onActionComplete - �� ���������� ��������). � ����� ������ �������� �������� ������� ClearBusy - �������� ���������
    {
        _state = State.GrenadeBefore; // ���������� ��������� ���������� �� �������
        float beforeGrenadeStateTime = 0.5f; //�� �������.  ��� ��������� ���������� ������ ������ ����������  ����������������� ��������� ���������� ����� �������� ..//����� ���������//
        _stateTimer = beforeGrenadeStateTime;

        _targetGridPositin = gridPosition; // �������� ���������� ��� �������� �������

        OnGrenadeActionStarted?.Invoke(this, EventArgs.Empty); // �������� ������� �������� ������ ������� �������� ��������� UnitAnimator (� �������� ���� event, �� ���������� �� ���� � � ���� ������� ����� ��������� �������)

        ActionStart(onActionComplete); // ������� ������� ������� ����� �������� // �������� ���� ����� � ����� ����� ���� �������� �.�. � ���� ������ ���� EVENT � �� ������ ����������� ����� ���� ��������
    }

    private void _handleAnimationEvents_OnAnimationTossGrenadeEventStarted(object sender, EventArgs e)
    {
        Transform grenadeProjectileTransform = Instantiate(_grenadeProjectilePrefab, _grenadeSpawnTransform.position, Quaternion.identity); // �������� ������ ������� 
        GrenadeProjectile grenadeProjectile = grenadeProjectileTransform.GetComponent<GrenadeProjectile>(); // ������� � ������� ��������� GrenadeProjectile
        grenadeProjectile.Setup(_targetGridPositin, OnGrenadeBehaviorComplete); // � ������� ������� Setup() ������� � ��� ������� ������� (��������� ������� ������� ����) � ��������� � ������� ������� OnGrenadeBehaviorComplete ( ��� ������ ������� ����� �������� ��� �������)
    }

    private void OnGrenadeBehaviorComplete() // ������������� ����� ������� ���������� ActionComplete() . ���� ����� ������������ ActionComplete() �������� �� ����� ���������� � ���������
    {
        OnGrenadeActionCompleted?.Invoke(this, EventArgs.Empty); // �������� ������� �������� ������ ������� ����������� ��������� UnitAnimator
        ActionComplete(); // ��� ������� ��������� - �������� ��������� ��� ����� ��������� - ������������ ������ UI
    }

    public int GetMaxThrowDistance()//�������� _maxHealDistance
    {
        return _maxThrowDistance;
    }

    public float GetDamageRadiusInWorldPosition() => _grenadeProjectile.GetDamageRadiusInWorldPosition(); // �������� �������
    public int GetDamageRadiusInCells() => _grenadeProjectile.GetDamageRadiusInCells(); // �������� �������


    private void OnDestroy()
    {
        _handleAnimationEvents.OnAnimationTossGrenadeEventStarted -= _handleAnimationEvents_OnAnimationTossGrenadeEventStarted; // �������� �� ������� ����� �� ���������� ������� � ��������� ��������.
    }

}

// https://community.gamedev.tv/t/grenade-can-be-thrown-through-wall/205331 �������� ������� ����� �����