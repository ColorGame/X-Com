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

    [SerializeField] private LayerMask _obstaclesAndDoorLayerMask; //����� ���� ����������� � ����� (�������� � ����������) ���� ������� Obstacles � DoorInteract � MousePlane(���) // ����� �� ���� ������ � ���� ���������� ����� ����� -Obstacles, � �� ������ -DoorInteract //��� ����� ������� ������ �������� �������� �� Box collider ����� ����� ����� ����� ������������� ������� ����
    [SerializeField] private LayerMask _smokeAndCoverLayerMask; //����� ���� ���� � ��������� (�������� � ����������) ���� ������� Smoke � Cover // ����� �� ���� ��������� � ���� �� �������,  ���������� ��������������� ����� �����t 
    [SerializeField] private int _numberShoot = 3; // ���������� ���������
    [SerializeField] private float _delayShoot = 0.2f; //�������� ����� ����������
    [SerializeField] private Transform _bulletProjectilePrefab; // � ���������� �������� ������ ����
    [SerializeField] private Transform _shootPointTransform; // � ���������� �������� ����� �������� ����� �� ��������

    private State _state; // ��������� �����
    private int _maxShootDistance = 7;
    private float _stateTimer; //������ ���������
    private Unit _targetUnit; // ���� � �������� �������� �������
    private bool _canShootBullet; // ����� �������� �����    
    private float _timerShoot; //������ ��������
    private int _counterShoot; // ������� ���������
    private bool _hit; // ����� ��� ��������
    private float _hitPercent; // ������� ���������

    private void Start()
    {
        _hitPercent = 1f; //��������� ������� ��������� ������������ 100%
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
        float aimingStateTime = 1f; // ��� ��������� ���������� ������ ������ ����������  ����������������� ��������� ������������ //����� ���������//
        _stateTimer = aimingStateTime;

        // _timerShoot = 0; // �������������

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
        Vector3 targetUnitWorldPosition = _targetUnit.GetWorldPosition(); // ������� ������� �������� �����. 
        float unitShoulderHeight = 1.7f; // ������ ����� �����,
        targetUnitWorldPosition.y += unitShoulderHeight; // � ���������� ���� ����� ��������� � ������ �����

        if (_hit) // ���� ������ ��
        {
            bulletProjectile.Setup(targetUnitWorldPosition); // � �������� ������� ������� ������� �������� �����. � �������������� ����������� �� �
            _targetUnit.Damage(10); // ��� ����� ����� ����� 10. � ���������� ����� ����� ���� ���������� �� ������ //���� ���������//
        }
        else // ���� ������
        {          
            //�������� ������� �� X Z
            targetUnitWorldPosition.x += UnityEngine.Random.Range(0.5f, 0.8f);
            targetUnitWorldPosition.z += UnityEngine.Random.Range(0.5f, 0.8f);
            bulletProjectile.Setup(targetUnitWorldPosition); // � �������� ������� ������� ������� �������� �����. � �������������� ����������� �� �
        }
    }

    public float GetHitPercent(Unit enemyUnit) // �������� ������� ��������� �� ����� (� �������� �������� �����)
    {
        _hitPercent = 1f; //��������� ������� ��������� ������������ 100%

        // �������� �� ����������������� �� ����
        Vector3 unitWorldPosition = _unit.GetWorldPosition(); // ������� ������� ���������� �����
        Vector3 enemyUnitWorldPosition = enemyUnit.GetWorldPosition(); //������� ������� ���������� ����������
        Vector3 shototDirection = (enemyUnitWorldPosition - unitWorldPosition).normalized; //��������������� ������ ����������� ��������
        RaycastHit hitInfo; // ���������, ������������ ��� ��������� ���������� ������� �� raycast.

        float unitShoulderHeight = 1.7f; // ������ ����� �����, � ���������� ����� ������������� ���������� � ������������ �������
        if (Physics.Raycast(
                unitWorldPosition + Vector3.up * unitShoulderHeight,
                shototDirection,
                out hitInfo,
                Vector3.Distance(unitWorldPosition, enemyUnitWorldPosition),
                _smokeAndCoverLayerMask)) // ���� ��� ����� � Smoke ��� CoverObject (Raycast -������ bool ����������, � ��� � ������� �����������)
        {
            CoverObject coverObject = hitInfo.collider.GetComponent<CoverObject>(); // ������� �� ���������, � ������� ������, ��������� CoverObject - ������ �������
            switch (coverObject.GetCoverType())
            {
                case CoverType.Full: // ���� ������� ������ �� �������� �������� �� 50%
                    _hitPercent -= .5f;
                    break;
                case CoverType.Half: // ���� ������� �� ������ �� �������� �������� �� 25%
                    _hitPercent -= .25f;
                    break;
            }
        }

        return _hitPercent;
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

                    // �������� �� ����������������� �� ����
                    Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitGridPosition); // ��������� � ������� ���������� ���������� ��� �������� ������� �����  
                    Vector3 shototDirection = (targetUnit.GetWorldPosition() - unitWorldPosition).normalized; //��������������� ������ ����������� ��������

                    float unitShoulderHeight = 1.7f; // ������ ����� �����, � ���������� ����� ������������� ���������� � ������������ �������
                    if (Physics.Raycast(
                            unitWorldPosition + Vector3.up * unitShoulderHeight,
                            shototDirection,
                            Vector3.Distance(unitWorldPosition, targetUnit.GetWorldPosition()),
                            _obstaclesAndDoorLayerMask)) // ���� ��� ����� � ����������� �� (Raycast -������ bool ����������)
                    {
                        // �� �������������� ������������
                        continue;
                    }

                    validGridPositionList.Add(testGridPosition); // ��������� � ������ �� ������� ������� ������ ��� �����
                                                                 //Debug.Log(testGridPosition);
                }
            }
        }

        return validGridPositionList;
    }



    public Unit GetTargetUnit() // �������� _targetUnit
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
            //actionValue = 100 +Mathf.RoundToInt(1- targetUnit.GetHealthNormalized()) *100,  // ��������� ������ ��� �������� �� ������ ������������� ������ .
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
}
