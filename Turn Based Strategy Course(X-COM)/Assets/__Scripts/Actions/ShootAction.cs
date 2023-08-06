using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShootAction : BaseAction
{

    public static event EventHandler<OnShootEventArgs> OnAnyShoot;  // ������� - ����� ����� �������� (����� ����� ���� ������ �������� �� �������� ������� Event) // <Unit> ������� �������� �������� ����� ��� ����
                                                                    // static - ���������� ��� event ����� ������������ ��� ����� ������ �� �������� �� ���� ������� � ��� �������� ������.
                                                                    // ������� ��� ������������� ����� ������� ��������� �� ����� ������ �� �����-���� ���������� �������, ��� ����� �������� ������ � ������� ����� �����,
                                                                    // ������� ����� ��������� ���� � �� �� ������� ��� ������ �������. 

    public event EventHandler<OnShootEventArgs> OnShoot; // ����� �������� (����� ���� ������ �������� �� �������� ������� Event) // <OnShootEventArgs> ������� �������� �������� ����� ��� ����

    public class OnShootEventArgs : EventArgs // �������� ����� �������, ����� � ��������� ������� �������� ������ ������
    {
        public Unit targetUnit; // ������� ���� � ���� ��������
        public Unit shootingUnit; // ����������� ���� ��� ��� ��������
    }

    private enum State
    {
        Aiming,     // ������������
        Shooting,   // ��������
        Cooloff,    // ��������� (��������� �������� ������ ��� �� �������� ��������)
    }

    [SerializeField] private LayerMask _obstaclesDoorMousePlaneCoverLayerMask; //����� ���� ����������� ���� ������� Obstacles � DoorInteract � MousePlane(���) Cover// ����� �� ���� ������ � ���� ���������� ����� ����� -Obstacles, � �� ������ -DoorInteract //��� ����� ������� ������ �������� �������� �� Box collider ����� ����� ����� ����� ������������� ������� ���� // Cover ������ ������������� ���� ����� �������� ���� ������� (���� ��������� ��� ������� Cover)
    [SerializeField] private LayerMask _smokeCoverLayerMask; //����� ���� ���� � ��������� (�������� � ����������) ���� ������� Smoke � Cover // ����� �� ���� ��������� � ���� �� �������,  ���������� ��������������� ����� �����t 
    [SerializeField] private LayerMask _coverLayerMask; // ����� ���� �������
    [SerializeField] private LayerMask _smokeLayerMask; // ����� ���� ���   
    [SerializeField] private int _numberShoot = 3; // ���������� ���������
    [SerializeField] private float _delayShoot = 0.2f; //�������� ����� ����������
    [SerializeField] private Transform _bulletProjectilePrefab; // � ���������� �������� ������ ����
    [SerializeField] private Transform _shootPointTransform; // � ���������� �������� ����� �������� ����� �� ��������
    [SerializeField] private Transform _aimPointTransform; // � ���������� �������� ����� ������������ ����� �� ������ (����� ���� ���� ������ ��� ��� ��������� ��������)

    private State _state; // ��������� �����
    private int _maxShootDistance = 7;
    private float _stateTimer; //������ ���������
    private Unit _targetUnit; // ���� � �������� �������� �������
    private bool _canShootBullet; // ����� �������� �����    
    private float _timerShoot; //������ ��������
    private int _counterShoot; // ������� ���������
    private bool _hit; // ����� ��� ��������
    private float _hitPercent; // ������� ���������
    private float _cellSize;// ������ ������

    private void Start()
    {
        _hitPercent = 1f; //��������� ������� ��������� ������������ 100%
        _cellSize = LevelGrid.Instance.GetCellSize();
    }
    private void Update()
    {
        if (!_isActive) // ���� �� ������� �� ...
        {
            return; // ������� � ���������� ��� ����
        }

        _stateTimer -= Time.deltaTime; // �������� ������ ��� ������������ ���������
        _timerShoot -= Time.deltaTime;// �������� ������ ��� ���������� � � ����������

        switch (_state) // ������������� ���������� ���� � ����������� �� _state
        {
            case State.Aiming:

                Vector3 aimDirection = (_targetUnit.GetWorldPosition() - transform.position).normalized; // ����������� ������������, ��������� ������
                aimDirection.y = 0; // ����� ���� �� ���������� ��� �������� (�.�. ������ ����� �������������� ������ �� ��������� x,z)

                float rotateSpeed = 10f; //����� ���������// ��� ������ ��� �������
                transform.forward = Vector3.Slerp(transform.forward, aimDirection, Time.deltaTime * rotateSpeed); // ������ �����.                               

                break;

            case State.Shooting:

                if (_canShootBullet && _timerShoot <= 0) // ���� ���� �������� ����� � ������ ����� ...
                {
                    Shoot();
                    _timerShoot = _delayShoot; // ��������� ������ = �������� ����� ����������
                    _counterShoot += 1; // �������� � �������� ��������� 1 
                }

                if (_counterShoot >= _numberShoot || _targetUnit.IsDead()) //����� ������� 3 ���� ��� ����� ���� ����
                {
                    _canShootBullet = false;
                    _counterShoot = 0; //������� ������� ����
                }

                break;

            case State.Cooloff: // ���� ������ �� ����� �������� �������� ��������� ��� �������
                break;
        }

        if (_stateTimer <= 0) // �� ��������� ������� _stateTimer ������� NextState() ������� � ���� ������� ���������� ���������. �������� - � ���� ���� TypeGrenade.Aiming: ����� � case TypeGrenade.Aiming: ��������� �� TypeGrenade.Shooting;
        {
            NextState(); //��������� ���������
        }
    }

    private void NextState() //������� ������������ ���������
    {
        switch (_state)
        {
            case State.Aiming:
                _state = State.Shooting;
                float shootingStateTime = _numberShoot * _delayShoot + 0.5f; // ��� ��������� ���������� ������ ������ ����������  ����������������� ��������� ������� = ���������� ��������� * ����� ��������
                _stateTimer = shootingStateTime;
                break;
            case State.Shooting:
                _state = State.Cooloff;
                float cooloffStateTime = 0.5f; // ��� ��������� ���������� ������ ������ ����������  ����������������� ��������� ���������� //���� ���������// ����������������� �������� ��������(��������� ������))
                _stateTimer = cooloffStateTime;
                break;
            case State.Cooloff:
                ActionComplete(); // ������� ������� ������� �������� ���������
                break;
        }

        //Debug.Log(_state);
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        _targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition); // ������� ����� � �������� ������� � �������� ���

        _state = State.Aiming; // ���������� ��������� ������������ 
        float aimingStateTime = 0.5f; // ��� ��������� ���������� ������ ������ ����������  ����������������� ��������� ������������ //����� ���������//
        _stateTimer = aimingStateTime;

        _canShootBullet = true;

        ActionStart(onActionComplete); // ������� ������� ������� ����� �������� � ��������� ������� // �������� ���� ����� � ����� ����� ���� �������� �.�. � ���� ������ ���� EVENT � �� ������ ����������� ����� ���� ��������
    }

    private void Shoot() // �������
    {
        //������� ������������� ���� ��� ������ �����. �� ��� ������ ��� ��������� �� ������� ScreenShake. ������� ��������� �� �������
        //ScreenShake.Instance.Shake(5);
        OnAnyShoot?.Invoke(this, new OnShootEventArgs // ������� ����� ��������� ������ OnShootEventArgs
        {
            targetUnit = _targetUnit,
            shootingUnit = _unit
        }); // �������� ������� ����� ����� �������� � � �������� ��������� � ���� �������� � ��� �������� (���������� ScreenShakeActions ��� ���������� ������ ������ � UnitRagdollSpawner- ��� ����������� ����������� ���������)

        OnShoot?.Invoke(this, new OnShootEventArgs // ������� ����� ��������� ������ OnShootEventArgs
        {
            targetUnit = _targetUnit,
            shootingUnit = _unit
        }); // �������� ������� ����� �������� � � �������� ��������� � ���� �������� � ��� �������� (UnitAnimator-���������)

        // ��������� ��������� ��� ������
        _hit = UnityEngine.Random.Range(0, 1f) < GetHitPercent(_targetUnit);

        Transform bulletProjectilePrefabTransform = Instantiate(_bulletProjectilePrefab, _shootPointTransform.position, Quaternion.identity); // �������� ������ ���� � ����� ��������
        BulletProjectile bulletProjectile = bulletProjectilePrefabTransform.GetComponent<BulletProjectile>(); // ������ ��������� BulletProjectile ��������� ����
        Vector3 targetUnitAimPointPosition = _targetUnit.GetAction<ShootAction>().GetAimPoinTransform().position; // ������� ������������ �������� �����. 

        if (_hit) // ���� ������ ��
        {
            bulletProjectile.Setup(targetUnitAimPointPosition, _hit); // � �������� ������� ������� ������������ �������� �����
            _targetUnit.Damage(5); // ��� ����� ����� ����� 10. � ���������� ����� ����� ���� ���������� �� ������ //���� ���������//
        }
        else // ���� ������
        {
            //�������� ������� �� X Y Z
            targetUnitAimPointPosition += Vector3.one * UnityEngine.Random.Range(-0.8f, 0.8f);
            bulletProjectile.Setup(targetUnitAimPointPosition, _hit); // � �������� ������� ������� ������� �������� �����. � �������������� ����������� �� �
        }
    }

    public float GetHitPercent(Unit enemyUnit) // �������� ������� ��������� �� ����� (� �������� �������� �����)
    {
        _hitPercent = 1f; //��������� ������� ��������� ������������ 100%

        // �������� ��� CoverSmokeObject �� ���� ��������

        Vector3 unitWorldPosition = _unit.GetWorldPosition(); // ������� ������� ���������� �����
        Vector3 enemyUnitWorldPosition = enemyUnit.GetWorldPosition(); //������� ������� ���������� ����������
        Vector3 shototDirection = (enemyUnitWorldPosition - unitWorldPosition).normalized; //��������������� ������ ����������� ��������
        float heightRaycast = 0.5f; // ������ �������� ���� (������ ������ ��� �� ������� � Half ������������ ���������)              
        float maxPenaltyAccuracy = 0; // ������������ ����� ������������
        Collider ignoreCoverSmokeCollider = null; // ������������ CoverSmokeObject

        // ��������� ��� � ������ ����� �� ��������� 1.5 ������ � ������ ��� ������� ������. ���� ������ �� ���������� ����� �� ����� �������(��� ����� ������������ �.�. �� �� �������� �� ���� � ������ ������ �����������). ���� ������ �� ���������� ����� �� ����� �������
        if (Physics.Raycast(
                 unitWorldPosition + Vector3.up * heightRaycast,
                 shototDirection,
                 out RaycastHit hitCoverInfo,
                 _cellSize *1.5f,
                 _coverLayerMask))
        {
            ignoreCoverSmokeCollider = hitCoverInfo.collider;            
        }
        /*Debug.DrawRay(unitWorldPosition + Vector3.up * heightRaycast,
                 shototDirection * (_cellSize *1.5f),
                 Color.white,
                 100);*/

        // ��������� ��� CoverSmokeObject � ������� maxPenaltyAccuracy
        RaycastHit[] raycastHitArray = Physics.RaycastAll(
                unitWorldPosition + Vector3.up * heightRaycast,
                shototDirection,
                Vector3.Distance(unitWorldPosition, enemyUnitWorldPosition),
                _smokeCoverLayerMask); // �������� ������ ���� ��������� ����. Obstacles ���� ��������� �.�. ����� ���� ������ ��������

        /*Debug.DrawRay(unitWorldPosition + Vector3.up * heightRaycast,
                shototDirection * Vector3.Distance(unitWorldPosition, enemyUnitWorldPosition),
                 Color.green,
                 999);*/

        foreach (RaycastHit raycastHit in raycastHitArray) // ��������� ��� ���������� ������ � ������� ������������ ����� ������������ maxPenaltyAccuracy
        {
            Collider coverSmokeCollider = raycastHit.collider; // ������� ��������, � ������� ������.

            if (coverSmokeCollider == ignoreCoverSmokeCollider)
            {
                //��������� ��������� ������� ���� ������������
                continue;
            }
            CoverSmokeObject coverSmokeObject = coverSmokeCollider.GetComponent<CoverSmokeObject>(); // ������� �� ���������, � ������� ������, ��������� CoverSmokeObject - ������ ������� ��� ���
            float penaltyAccuracy = coverSmokeObject.GetPenaltyAccuracy(); // ������� ����� ������������
            if (penaltyAccuracy > maxPenaltyAccuracy)
            {
                maxPenaltyAccuracy = penaltyAccuracy;
            }
        }

        // ��������� ��� �� ������� ����� ��� �� ��������� ���� � ���� (��� �� ����� �������� ������ ���������(������ �����))
        if (Physics.Raycast(
                 enemyUnitWorldPosition + Vector3.up * heightRaycast,
                 shototDirection * (-1),
                 out RaycastHit hitSmokeInfo,
                 Vector3.Distance(unitWorldPosition, enemyUnitWorldPosition),
                 _smokeLayerMask)) // ��������� ������ ���
        {
            CoverSmokeObject coverSmokeObject = hitSmokeInfo.collider.GetComponent<CoverSmokeObject>(); // ������� �� ���������, � ������� ������, ��������� CoverSmokeObject - ������ �������
            float penaltyAccuracy = coverSmokeObject.GetPenaltyAccuracy(); // ������� ����� ������������
            if (penaltyAccuracy > maxPenaltyAccuracy)
            {
                maxPenaltyAccuracy = penaltyAccuracy;
            }
        }
        return _hitPercent -= maxPenaltyAccuracy;
    }

    public override string GetActionName() // ������� ��� ��� ������
    {
        return "Shoot";
    }

    public override List<GridPosition> GetValidActionGridPositionList() //�������� ������ ���������� �������� ������� ��� �������� // ������������� ������� �������
    {
        GridPosition unitGridPosition = _unit.GetGridPosition(); // ������� ������� � ����� �����
        return GetValidActionGridPositionList(unitGridPosition);
    }

    //���������� �� ������ ���� ����������.
    public List<GridPosition> GetValidActionGridPositionList(GridPosition unitGridPosition) //�������� ������ ���������� �������� ������� ��� ��������.
                                                                                            //�������� ������ ���������� ����� ������ ������� �����
                                                                                            //� �������� �������� �������� ������� �����                                                                                            
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        for (int x = -_maxShootDistance; x <= _maxShootDistance; x++) // ���� ��� ����� ����� ������� � ������������ unitGridPosition, ������� ��������� ���������� �������� � �������� ������� _maxShootDistance
        {
            for (int z = -_maxShootDistance; z <= _maxShootDistance; z++)
            {
                for (int floor = -_maxShootDistance; floor <= _maxShootDistance; floor++)
                {

                    GridPosition offsetGridPosition = new GridPosition(x, z, floor); // ��������� �������� �������. ��� ������� ���������(0,0, 0-����) �������� ��� ���� 
                    GridPosition testGridPosition = unitGridPosition + offsetGridPosition; // ����������� �������� �������

                    if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) // �������� �������� �� testGridPosition ���������� �������� �������� ���� ��� �� ��������� � ���� �����
                    {
                        continue; // continue ���������� ��������� ���������� � ��������� �������� ����� 'for' ��������� ��� ����
                    }
                    // ��� ������� �������� ������� ���� � �� �������
                    int testDistance = Mathf.Abs(x) + Mathf.Abs(z); // ����� ���� ������������� ��������� �������� �������
                    if (testDistance > _maxShootDistance) //������� ������ �� ����� � ���� ����� // ���� ���� � (0,0) �� ������ � ������������ (5,4) ��� �� ������� �������� 5+4>7
                    {
                        continue;
                    }

                    if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition)) // �������� �������� ������� ��� ��� ������ (��� ����� ������ � ������� �� ����� �� ��� �������)
                    {
                        // ������� ����� �����, ��� ������
                        continue;
                    }

                    Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);   // ������� ����� �� ����� ����������� �������� ������� 
                                                                                                    // GetUnitAtGridPosition ����� ������� null �� � ���� ���� �� ��������� ������� �������, ��� ��� �������� �� �����
                    if (targetUnit.IsEnemy() == _unit.IsEnemy()) // ���� ����������� ���� ���� � ��� ���� ���� ���� �� (���� ��� ��� � ����� ������� �� ����� ������������ ���� ������)
                    {
                        // ��� ������������� � ����� "�������"
                        continue;
                    }

                    // �������� �� ����������������� �� ���� , Cover ������ ������������� ���� ����� �������� ���� ������� (���� ��������� ��� ������� Cover)
                    Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitGridPosition); // ��������� � ������� ���������� ���������� ��� �������� ������� �����  
                    Vector3 shototDirection = (targetUnit.GetWorldPosition() - unitWorldPosition).normalized; //��������������� ������ ����������� ��������
                    Vector3 shootPoint = _shootPointTransform.position - unitWorldPosition; // ������� ���������� �� ����� �������� �� ��������� ����� (��������� ��� 2 �����)

                    float reserveHeight = 0.15f; // ������ �� ������ ���� ���� ����� �������� ()
                    if (Physics.Raycast(
                            unitWorldPosition + Vector3.up * (shootPoint.y + reserveHeight),
                            shototDirection,
                            Vector3.Distance(unitWorldPosition, targetUnit.GetWorldPosition()),
                            _obstaclesDoorMousePlaneCoverLayerMask)) // ���� ��� ����� � ����������� �� (Raycast -������ bool ����������)
                    {
                        // �� �������������� ������������
                        continue;
                    }

                    /*Debug.DrawRay(unitWorldPosition + Vector3.up * (shootPoint.y + reserveHeight),
                        shototDirection * Vector3.Distance(unitWorldPosition, startUnit.GetWorldPosition()),
                        Color.red,
                        999);*/

                    validGridPositionList.Add(testGridPosition); // ��������� � ������ �� ������� ������� ������ ��� �����
                                                                 //Debug.Log(testGridPosition);
                }
            }
        }

        return validGridPositionList;
    }



    public Unit GetTargetUnit() // �������� _unitPartner
    {
        return _targetUnit;
    }

    public int GetMaxShootDistance() // �������� _maxShootDistance
    {
        return _maxShootDistance;
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition) //�������� �������� ���������� ��  ��� ���������� ��� �������� �������// ������������� ����������� ������� ����� //EnemyAIAction ������ � ������ ���������� ������� �������, ���� ������ - ��������� ������ ������ � ����������� �� ��������� ����� ������� ��� �����
    {
        Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition); // ������� ����� ��� ���� ������� ��� ���� ����

        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            //actionValue = 100 +Mathf.RoundToInt(1- startUnit.GetHealthNormalized()) *100,  // ��������� ������ ��� �������� �� ������ ������������� ������ .
            // �������� ���� ���� ��������� ������ �� GetHealthNormalized() ������ 1  ����� (1-1)*100 = 0 � ����� actionValue ���������� ������� 100
            // �� ���� �������� �������� ����� �� GetHealthNormalized() ������ 0,5 ����� (1-0,5)*100 = 50 � actionValue ������ ������ 150 ����� ������� ���������� �������� 
            // ������ �� �������� ����� � ������ ������ ������������ ��������
            // �������� ������ ������
            actionValue = 100 + Mathf.RoundToInt(AttackScore(targetUnit))
        };
    }

    public float AttackScore(Unit unit) // ������ ����� // � ������ ������� ���� ������, �� ����� ��� ������ ��������� ��������, �������� ��� ��� ��� ����� �������
    {
        int health = unit.GetHealth();
        int healthMax = unit.GetHealthMax();

        float unitPerHealthPoint = 100 / healthMax;  //��� ���� ��������, ��� ���� ����� ���� ����
        return (healthMax - health) * unitPerHealthPoint + unitPerHealthPoint; // ���� ���� � ���� ����� ������������ �������� (health=healthMax), �� ������ ����� ������� ���� � ������� ������������ ��������� (������ ������ �������� � ������ �������).
                                                                               // ���� ����� ����������� � ���� ������ ���������� �������� �������� 20 �� � ������� healthMax=100 � � ������� 120 �� �������� ������� �.�. � ���� ������ ������������ �������� � �� ������� ������ ����� ��������
    }

    public int GetTargetCountAtPosition(GridPosition gridPosition) // �������� ���������� ����� �� �������
    {
        return GetValidActionGridPositionList(gridPosition).Count; // ������� ���������� ����� �� ������ ���������� �����
    }

    public Transform GetAimPoinTransform() // �������� ����� ������������
    {
        return _aimPointTransform;
    }

    public Transform GetShootPoinTransform() // �������� ����� ��������
    {
        return _shootPointTransform;
    }
}
