using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GrenadeAction : BaseAction // ������� ��������. ��������� BaseAction
                                                 // abstract - �� ��������� ������� Instance (���������) ����� ������.
{
    public event EventHandler OnGrenadeActionStarted;     // �������� ������ ������� ��������    
    public event EventHandler OnGrenadeActionCompleted;   // �������� ������ ������� �����������
    protected enum State
    {
        GrenadeBefore, //�� ������� ������� (�����������)
        GrenadeInstantiate, //�������� �������
        GrenadeAfter,  //����� ������� �������
    }
    
    [SerializeField] protected LayerMask _obstaclesDoorMousePlaneLayerMask; //����� ���� ����������� � ����� (�������� � ����������) ���� ������� Obstacles � DoorInteract MousePlane(��� ��� ���������� ������) � // ����� �� ���� ������ � ���� ���������� ����� ����� -Obstacles, � �� ������ -DoorInteract
    [SerializeField] protected Transform _grenadeProjectilePrefab; // ������ ������ ������� // � ������� ����� �������� ������ �������
    [SerializeField] protected Transform _grenadeSpawnTransform; // ��������� �������� ������� // � ������� ����� �������� ������ �������

    protected State _state; // ��������� �����
    protected float _stateTimer; //������ ���������      


    protected int _maxThrowDistance = 5; //������������ ��������� ������ //����� ���������//
    protected GridPosition _targetGridPositin;
    protected GrenadeProjectile _grenadeProjectile;
    protected HandleAnimationEvents _handleAnimationEvents; // ���������� ������������ �������

    protected override void Awake()
    {
        base.Awake();

        _handleAnimationEvents = GetComponentInChildren<HandleAnimationEvents>();
        _grenadeProjectile = _grenadeProjectilePrefab.GetComponent<GrenadeProjectile>();
    }

    protected void Start()
    {
        _handleAnimationEvents.OnAnimationTossGrenadeEventStarted += _handleAnimationEvents_OnAnimationTossGrenadeEventStarted; // ���������� �� ������� "� �������� "������ �������" ���������� �������"
    }

    public abstract void _handleAnimationEvents_OnAnimationTossGrenadeEventStarted(object sender, EventArgs e); // abstract - ��������� ������������� � ������ ��������� � � ������� ������ ����� ������ ����.
    public abstract int GetGrenadeDamage(); // ����������� �� �������
    protected void Update()
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

        if (_stateTimer <= 0) // �� ��������� ������� ������� NextMusic() ������� � ���� ������� ���������� ���������. �������� - � ���� ���� TypeGrenade.Aiming: ����� � case TypeGrenade.Aiming: ��������� �� TypeGrenade.Shooting;
        {
            NextState(); //��������� ���������
        }
    }

    protected void NextState() //������� ������������ ���������
    {
        switch (_state)
        {
            case State.GrenadeBefore:

                _state = State.GrenadeInstantiate;
                SoundManager.Instance.PlaySoundOneShot(SoundManager.Sound.GrenadeThrow);
                //float grenadeInstantiateStateTime = 0.5f; // ��� ��������� ���������� ������ ������ ����������  ����������������� ��������� �������� ������� //����� ���������// ����� ����� ��������� ����� �������� ������� (������ ��������� AnimationEvent)
                //_musicTimer = grenadeInstantiateStateTime;                               

                break;

            case State.GrenadeInstantiate:
                break;

            case State.GrenadeAfter:
                break;
        }

        //Debug.Log(_state);
    } 

    public override List<GridPosition> GetValidActionGridPositionList()// �������� ������ ���������� �������� ������� ��� �������� // ������������� ������� �������                                                                       
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = _unit.GetGridPosition(); // ������� ������� � ����� �����

        for (int x = -_maxThrowDistance; x <= _maxThrowDistance; x++) // ���� ��� ����� ����� ������� � ������������ unitGridPosition, ������� ��������� ���������� �������� � �������� ������� _maxComboDistance
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

                    //����� ���� ��������
                    /*int pathfindingDistanceMultiplier = 10; // ��������� ���������� ����������� ���� (� ������ PathfindingMonkey ������ ��������� �������� �� ������ � ��� ����� ����� 10 �� ��������� 14, ������� ������� ��� ��������� �� ���������� ������)
                    if (PathfindingMonkey.Instance.GetPathLength(unitGridPosition, testGridPosition) > _maxThrowDistance * pathfindingDistanceMultiplier) //�������� �������� ������� - ���� ��������� �� ����������� ������ ������ ���������� ������� ���� ����� ���������� �� ���� ���
                    {
                        // ����� ���� ������� ������
                        continue;
                    }*/

                    // �������� �� ����������� ������ ����� ����������� 
                    Vector3 worldTestGridPosition = LevelGrid.Instance.GetWorldPosition(testGridPosition);   // ������� ������� ���������� ����������� �������� ������� 
                    Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitGridPosition); // ��������� � ������� ���������� ���������� ��� �������� ������� �����  
                    Vector3 grenadeDirection = (worldTestGridPosition - unitWorldPosition).normalized; //��������������� ������ ����������� ������ �������

                    float heightRaycast = 2.5f; // ������ �������� ���� (����� ������� ��� �� ������������ ������ ����������� �.�. ����� ��� ����� ������� ��������) ����� ����������� ���� 2,5� ������� ������ ������ (������ ������� 3�)
                    if (Physics.Raycast(
                            unitWorldPosition + Vector3.up * heightRaycast,
                            grenadeDirection,
                            Vector3.Distance(unitWorldPosition, worldTestGridPosition),
                            _obstaclesDoorMousePlaneLayerMask)) // ���� ��� ����� � ����������� �� (Raycast -������ bool ����������) Cover ���� ���������� �.�. ����� ������� ����� ������� ��������
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

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)  // ������������� TakeAction ���������� �������� . (������� onActionComplete - �� ���������� ��������). � ����� ������ �������� �������� ������� ClearBusy - �������� ���������
    {
        _state = State.GrenadeBefore; // ���������� ��������� ���������� �������
        float beforeGrenadeStateTime = 0.5f; //�� �������.  ��� ��������� ���������� ������ ������ ����������  ����������������� ��������� ���������� ����� �������� ..//����� ���������//
        _stateTimer = beforeGrenadeStateTime;

        _targetGridPositin = gridPosition; // �������� ���������� ��� �������� �������

        OnGrenadeActionStarted?.Invoke(this, EventArgs.Empty); // �������� ������� �������� ������ ������� �������� ��������� UnitAnimator (� �������� ���� event, �� ���������� �� ���� � � ���� ������� ����� ��������� �������)

        ActionStart(onActionComplete); // ������� ������� ������� ����� �������� // �������� ���� ����� � ����� ����� ���� �������� �.�. � ���� ������ ���� EVENT � �� ������ ����������� ����� ���� ��������
    }
    protected void OnGrenadeBehaviorComplete() // ������������� ����� ������� ���������� ActionComplete() . ���� ����� ������������ ActionComplete() �������� �� ����� ���������� � ���������
    {
        OnGrenadeActionCompleted?.Invoke(this, EventArgs.Empty); // �������� ������� �������� ������ ������� ����������� ��������� UnitAnimator
        ActionComplete(); // ��� ������� ��������� - �������� ��������� ��� ����� ��������� - ������������ ������ UI
    }

    public override int GetMaxActionDistance()//�������� 
    {
        return _maxThrowDistance;
    }       
   



    public float GetDamageRadiusInWorldPosition() => _grenadeProjectile.GetDamageRadiusInWorldPosition(); // �������� �������
    public int GetDamageRadiusInCells() => _grenadeProjectile.GetDamageRadiusInCells(); // �������� �������

    protected void OnDestroy()
    {
        _handleAnimationEvents.OnAnimationTossGrenadeEventStarted -= _handleAnimationEvents_OnAnimationTossGrenadeEventStarted; // �������� �� ������� ����� �� ���������� ������� � ��������� ��������.
    }

    public override int GetActionPointCost() // ������������� ������� ������� // �������� ������ ����� �� �������� (��������� ��������)
    {
        return 2;
    }
}

// https://community.gamedev.tv/t/grenade-can-be-thrown-through-wall/205331 �������� ������� ����� �����