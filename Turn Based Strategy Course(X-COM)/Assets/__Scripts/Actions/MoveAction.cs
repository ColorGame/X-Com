//#define PATHFINDING

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
// �������� �������� �� ��� ������, ���� ��� �������, ������ �� ����� �����, ��� ����� 'Path' ����������, � �� ������ ������ �����������
// ��� ������ ������ ������ �������������� � ������� ����� ��������, ������������ ����� ����
//using Pathfinding;



public class MoveAction : BaseAction // �������� ����������� ��������� ����� BaseAction // ������� � ��������� ����� // ����� �� ������ �����
{

    public event EventHandler OnStartMoving; // ����� ��������� (����� ���� ������ �������� �� �������� ������� Event)
    public event EventHandler OnStopMoving; // ��������� �������� (����� ���� �������� �������� �� �������� ������� Event)
    public event EventHandler<OnChangeFloorsStartedEventArgs> OnChangedFloorsStarted; // ������ ������ ����� 
    public class OnChangeFloorsStartedEventArgs : EventArgs // �������� ����� �������, ����� � ��������� ������� �������� �������� ������� ����� � ������� �������
    {
        public GridPosition unitGridPosition; // ������ �������
        public GridPosition targetGridPosition; // ���� �������
    }


    [SerializeField] private int maxMoveDistance = 5; // ������������ ��������� �������� � �����

    private List<Vector3> _positionList; // ������� ������� ������ ���������� ���� (� ������������ �������)
    private int _currentPositionIndex; // ������� ������� ������
    private bool _isChangingFloors; // ��� ����� ������
    private float _differentFloorsTeleportTimer; // ������ ������������ �� ������ �����
    private float _differentFloorsTeleportTimerMax = .5f; // ������������ ������ ������������ �� ������ ����� (��� ����� ��������������� �������� ������ ��� �������)


#if PATHFINDING
    public Path _path;
    private Seeker _seeker;




    public void Start()
    {
        _seeker = GetComponent<Seeker>();

    }
#endif

    private void Update()
    {
        if (!_isActive) // ���� �� ������� �� ...
        {
            return; // ������� � ���������� ��� ����
        }


#if PATHFINDING
        if (_path == null || _positionList.Count == 0) //���� ���� �� �������� ��� ���� ����� �� ������� (StartPath - ����� �������� � ��������� �����)
        {
            return; // ������� � ���������� ��� ����
        }
#endif
        // ���� ��������� �� ������ ����� �� _positionList, ������ ��������� ������ ����� targetPosition
        Vector3 targetPosition = _positionList[_currentPositionIndex]; // ������� �������� ����� ������� �� ����� � �������� ��������

        if (_isChangingFloors) // ���� ���� ������� ���� ��
        {
            // ������ ��������� � ������������
            // ��� ������� � ������, � ������� ���� ����� �����������������, ���������� ��� �� �� ������� � ������� ������ �� ������ �� ����������� (�� ������� ����� ��� ����)
            Vector3 targetSameFloorPosition = targetPosition; // ������� ������� ����� �� ����� = ������� ������
            targetSameFloorPosition.y = transform.position.y; // ������� ������� �� ��� � ��� � ������

            Vector3 rotateDirection = (targetSameFloorPosition - transform.position).normalized; // ����������� ��������

            float rotateSpeed = 10f;
            transform.forward = Vector3.Slerp(transform.forward, rotateDirection, Time.deltaTime * rotateSpeed);

            _differentFloorsTeleportTimer -= Time.deltaTime; //�������� ������ ������������ �� ������ �����
            if (_differentFloorsTeleportTimer < 0f) // �� ��������� ������� // ���������� ������������� ������ � ��������������� � ������� ��������� (� ����� � ������ ������� ������� ����� ����������� �������� ������)
            {
                _isChangingFloors = false; 
                transform.position = targetPosition; 
            }

        }
        else
        {
            // ������� ������ �����������

            Vector3 moveDirection = (targetPosition - transform.position).normalized; // ����������� ��������, ��������� ������

            float rotateSpeed = 10f; //��� ������ ��� �������
            transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed); // ������ �����. ������� Lerp �� - Slerp ���������� ������������� ����� ������������� a � b �� ����������� t. �������� t ��������� ���������� [0, 1]. ����������� ��� ��� �������� ��������, ������� ������ ������������� ����� ������ ������������ a � ������ ������������ b �� ������ �������� ��������� t. ���� �������� ��������� ������ � 0, �������� ������ ����� ������ � a, ���� ��� ������ � 1, �������� ������ ����� ������ � b.

            float moveSpead = 4f; //����� ���������//
            transform.position += moveDirection * moveSpead * Time.deltaTime;
        }
           
        float stoppingDistance = 0.2f; // ��������� ��������� //����� ���������//
        if (Vector3.Distance(transform.position, targetPosition) < stoppingDistance)  // ���� ��������� �� ������� ������� ������ ��� ��������� ��������� // �� �������� ����        
        {
            _currentPositionIndex++; // �������� ������ �� �������

            if (_currentPositionIndex >= _positionList.Count) // ���� �� ����� �� ����� ������ �����...
            {
                SoundManager.Instance.SetLoop(false);
                SoundManager.Instance.Stop();

                OnStopMoving?.Invoke(this, EventArgs.Empty); //�������� ������� ��������� ��������

                ActionComplete(); // ������� ������� ������� �������� ���������
            }
            else
            {
                targetPosition = _positionList[_currentPositionIndex]; // ������� �������� ����� ������� �� ����� � �������� ��������
                GridPosition targetGridPosition = LevelGrid.Instance.GetGridPosition(targetPosition); // ������� �������� ������� ������� �������
                GridPosition unitGridPosition = LevelGrid.Instance.GetGridPosition(transform.position); // ������� �������� ������� �����

                if (targetGridPosition.floor != unitGridPosition.floor) // ���� ���� ������� �������� �� ��������� � ������ ����� �� ...
                {
                    // ������ �����
                    _isChangingFloors = true;
                    _differentFloorsTeleportTimer = _differentFloorsTeleportTimerMax;

                    OnChangedFloorsStarted?.Invoke(this, new OnChangeFloorsStartedEventArgs // �������� ������� � ��������� �������� ������� ������ � ���� �������
                    {
                        unitGridPosition = unitGridPosition,
                        targetGridPosition = targetGridPosition,
                    });
                }

            }
        }
    }

    // ������������� TakeAction (��������� �������� (�����������)) // �� ������������� Move � TakeAction
    public override void TakeAction(GridPosition gridPosition, Action onActionComplete) // �������� � ������� �������. � �������� �������� �������� �������  � �������. ������� �� ��� �������� ����� ������� �������
    {
#if PATHFINDING

        _seeker.StartPath(transform.position, LevelGrid.Instance.GetWorldPosition(gridPosition));
        _path = _seeker.GetCurrentPath();
        _positionList = _path.vectorPath;


#else

        List<GridPosition> pathGridPositionList = PathfindingMonkey.Instance.FindPath(_unit.GetGridPosition(), gridPosition, out int pathLength); // ������� ������ ���� ������� ����� �� �������� ��������� ��������� ����� �� �������� (out int pathLength �������� ��� �� ��������������� ���������)

        SoundManager.Instance.SetLoop(true);
        SoundManager.Instance.Play(SoundManager.Sound.Move);

       // ���� ������������� ���������� ������ GridPosition � ������� ���������� Vector3
       _positionList = new List<Vector3>(); // �������������� ������ �������

        foreach (GridPosition pathGridPosition in pathGridPositionList) // ��������� ���������� ������ pathGridPositionList, ����������� �� � ������� ���������� � ������� � _positionList
        {
            _positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPosition)); // ����������� pathGridPosition � ������� � ������� � _positionList
        }

#endif

        _currentPositionIndex = 0; // �� ��������� ���������� � ����
        OnStartMoving?.Invoke(this, EventArgs.Empty); // �������� ������� ����� ��������� 
        ActionStart(onActionComplete); // ������� ������� ������� ����� �������� // �������� ���� ����� � ����� ����� ���� �������� �.�. � ���� ������ ���� EVENT � �� ������ ����������� ����� ���� ��������
    }

    public override List<GridPosition> GetValidActionGridPositionList() //�������� ������ ���������� �������� ������� ��� �������� // ������������� ������� �������
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = _unit.GetGridPosition(); // ������� ������� � �����
        for (int x = -maxMoveDistance; x <= maxMoveDistance; x++) // ���� ��� ����� ����� ������� � ������������ unitGridPosition, ������� ��������� ���������� �������� � �������� ������� maxMoveDistance
        {
            for (int z = -maxMoveDistance; z <= maxMoveDistance; z++)
            {
                for (int floor = -maxMoveDistance; floor <= maxMoveDistance; floor++)
                {

                    GridPosition offsetGridPosition = new GridPosition(x, z, floor); // ��������� �������� �������. ��� ������� ���������(0,0, floor-����) �������� ��� ���� 
                    GridPosition testGridPosition = unitGridPosition + offsetGridPosition; // ����������� �������� �������

                    if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) // �������� �������� �� testGridPosition ���������� �������� �������� ���� ��� �� ��������� � ���� �����
                    {
                        continue; // continue ���������� ��������� ���������� � ��������� �������� ����� 'for' ��������� ��� ����
                    }

                    if (unitGridPosition == testGridPosition) // �������� �������� ������� ��� ���������� ��� ����
                    {
                        // ���� ������ �� ������� ����� ���� :(
                        continue;
                    }

                    if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition)) // �������� �������� ������� ��� ���������� ������ �����
                    {
                        // ������� ������ ������ ������ :(
                        continue;
                    }

                    if (!PathfindingMonkey.Instance.IsWalkableGridPosition(testGridPosition)) //�������� �������� ������� ��� ������ ������ (���� ����������� ����� �������)
                    {
                        continue;
                    }

#if PATHFINDING
                // �� ��������
                var gg = AstarPath.active.data.gridGraph;

                GridNodeBase Unitnode = gg.GetNode(unitGridPosition.x, unitGridPosition.z);
                GridNodeBase Testnode = gg.GetNode(testGridPosition.x, testGridPosition.z);

                if (PathUtilities.IsPathPossible(Unitnode, Testnode))//�������� �������� ������� ���� ������ ������ 
                {
                    continue;
                }

#else

                    if (!PathfindingMonkey.Instance.HasPath(unitGridPosition, testGridPosition)) //�������� �������� ������� ���� ������ ������ 
                    {
                        continue;
                    }
#endif

                    int pathfindingDistanceMultiplier = 10; // ��������� ���������� ����������� ���� (� ������ PathfindingMonkey ������ ��������� �������� �� ������ � ��� ����� ����� 10 �� ��������� 14, ������� ������� ��� ��������� �� ���������� ������)
                    if (PathfindingMonkey.Instance.GetPathLength(unitGridPosition, testGridPosition) > maxMoveDistance * pathfindingDistanceMultiplier) //�������� �������� ������� - ���� ��������� �� ����������� ������ ������ ���������� ������� ���� ����� ���������� �� ���� ���
                    {
                        // ����� ���� ������� ������
                        continue;
                    }

                    validGridPositionList.Add(testGridPosition); // ��������� � ������ �� ������� ������� ������ ��� �����
                                                                 //Debug.Log(testGridPosition);
                }
            }
        }

        return validGridPositionList;
    }

    public override string GetActionName() // ��������� ������� �������� //������� ������������� ������� �������
    {
        return "��������";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition) //�������� �������� ���������� �� // ������������� ����������� ������� �����
    {
        int targetCountAtPosition = _unit.GetAction<ShootAction>().GetTargetCountAtPosition(gridPosition); // � ����� ������ ������ ShootAction � ������� � ���� "�������� ���������� ����� �� �������"
                                                                                                           //� �����, ��� ����� ������� ���� �� ������ ����� �����, ������� ����� ������������ ������ � �������� ������������� ������� ������������ ������� �����. ����� �� ������ ������� ������ � ��������������� ���� � �� ��������� ���� �������� � ������ (Move � Shoot)
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = targetCountAtPosition * 10 +50, //������ � ����� ������� ����������� ���������� ����� ����� � ����������. �������� ���� � ��� ���� ������� �����, � ������� ��� ���������� �����, � ������ ������� �����, � ������� ���� ���� ���������� ����, �� �������� �� ������ ������� �����, ��������� �������� �������� �������� �� ���������� ���������� �����.
        };
        // ��������� �������� ���������� ��� ������ ����� ����� ��������� ������ �������� ��������, ���� �������� ����� ���������� ����� 20%, ���� ����� �������� ����������� ����������� �������� �� ������, �� ������� ��� ���������� �����.
        // �� ����� �� ��������� �������������� ��� ������� �� ����������� ������, � ������� ������ ��������, ��� ������� �� ����������� ������ � ����� ������� ���������
        // ����� ���� ����� ������������, �����, �������, ��� ���������� ����� ������ ����� ��������� �����, ����������� ������ ��� ������� ��������� ��������.
    }

    //����� ������������ ���� ������� ����� ����������.
    //https://community.gamedev.tv/t/more-aggressive-enemy/220615?_gl=1*ueppqc*_ga*NzQ2MDMzMjI4LjE2NzY3MTQ0MDc.*_ga_2C81L26GR9*MTY3OTE1NDA5Ni4zMS4xLjE2NzkxNTQ1MjYuMC4wLjA.



}
