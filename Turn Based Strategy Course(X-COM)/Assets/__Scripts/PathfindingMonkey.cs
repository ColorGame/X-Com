//#define HEX_GRID_SYSTEM //������������ �������� ������� //  � C# ��������� ��� �������� �������������, ����������� ������� �� ������������� ��������� ���� ��������� ������������. 
//��� ��������� ���������� ������� ������������� ������ ��������� ����� �� ����������� � ��������� ��� � ��� �������� �����, ��� ��� ����������. 

using System.Collections.Generic;
using UnityEngine;

// �������� ������� ���������� ������� PathfindingMonkey, ������� � Project Settings/ Script Execution Order � �������� ���������� PathfindingMonkey ���� Default Time, ����� PathfindingMonkey ���������� ������ �� ���� ��� ��������� �������� ����� ���� 

public class PathfindingMonkey : MonoBehaviour // ����� ���� // ������ ������� ����� ������������ ������ ��� ������ ���� (����� �� ����� LevelGrid)
{

    public static PathfindingMonkey Instance { get; private set; }   //(������� SINGLETON) ��� �������� ������� ����� ���� ������� (SET-���������) ������ ���� �������, �� ����� ���� �������� GET ����� ������ �������
                                                                     // instance - ���������, � ��� ����� ���� ��������� PathfindingMonkey ����� ���� ��� static. Instance ����� ��� ���� ����� ������ ������, ����� ����, ����� ����������� �� Event.



    private const int MOVE_STRAIGHT_COST = 10; // ��������� �������� ����� (��� �������� ����� 10 � �� 1 ��� �� �� ������������ float)
    private const int MOVE_DIAGONAL_COST = 14; // ��������� �������� �� ��������� ( ������������� �� ������� �������� ������ ���������� �� ����� ��������� ������� �������������� ������������. � ����� ��� �������� ����� 14 � �� 1,4 ��� �� �� ������������ float)

    //���������� ��� ������ ���������� ������ GetNeighbourList() ����� ��������
    /*// ���������� �������� ������� ��� ������ �������� �����
    private GridPosition UP = new(0, 1);
    private GridPosition DOWN = new(0, -1);
    private GridPosition RIGHT = new(1, 0);
    private GridPosition LEFT = new(-1, 0);*/

    [SerializeField] private Transform _pathfindingGridDebugObject; // ������ ������� ����� //������������ ��� ������ ��������� � ����� ��������� ������ CreateDebugObject
    [SerializeField] private LayerMask _obstaclesCoverLayerMask; // ����� ���� ����������� (�������� � ����������) ���� ������� Obstacles � Cover// ����� �� ���� ������ � ���� ���������� ����� ����� -Obstacles ����� ������ � �� �������� Cover
    [SerializeField] private LayerMask _floorLayerMask; // ����� ���� ���� (�������� � ����������) ���� ������� MousePlane -��� ��� � ����
    [SerializeField] private Transform _pathfindingLinkContainer; // ��������� ������ ��� ������ ���� // � ���������� �������� �� ����� PathfindingLinkContainer

    private int _width;     // ������
    private int _height;    // ������
    private float _cellSize;// ������ ������
    private int _floorAmount; // ���������� ������
    private List<GridSystemHexAndQuad<PathNode>> _gridSystemList; //������ ������� �������� ������ � ����� PathNode
    private List<PathfindingLink> _pathfindingLinkList; //������ ������ ��� ������ ���� (�.�. ������ �� ����� ������� ����� ���� ��������� - �������� ��� ������ � �������� ����� ���������� � 2 �����������)
    private List<GridPosition> _gridPositionInAirList; // ������ ������� � �������

    private void Awake()
    {
        // ���� �� �������� � ���������� �� �������� �� �����
        if (Instance != null) // ������� �������� ��� ���� ������ ���������� � ��������� ����������
        {
            Debug.LogError("There's more than one Pathfinding!(��� ������, ��� ���� Pathfinding!) " + transform + " - " + Instance);
            Destroy(gameObject); // ��������� ���� ��������
            return; // �.�. � ��� ��� ���� ��������� PathfindingMonkey ��������� ����������, ��� �� �� ��������� ������ ����
        }
        Instance = this;
    }

    public void Setup(int width, int height, float cellSize, int floorAmount) // �������� ���� ������ �����
    {
        _width = width;
        _height = height;
        _cellSize = cellSize;
        _floorAmount = floorAmount;

        _gridSystemList = new List<GridSystemHexAndQuad<PathNode>>();
        _gridPositionInAirList = new List<GridPosition>();

        for (int floor = 0; floor < floorAmount; floor++)
        {
            GridSystemHexAndQuad<PathNode> gridSystem = new GridSystemHexAndQuad<PathNode>(_width, _height, _cellSize, floor, LevelGrid.FLOOR_HEIGHT, // �������� ����� 10 �� 10 � �������� 2 ������� �� ����� floor c �������� ������� ����� � � ������ ������ �������� ������ ���� PathNode
                    (GridSystemHexAndQuad<PathNode> g, GridPosition gridPosition) => new PathNode(gridPosition));   //� ��������� ��������� ��������� ������� ������� �������� ����� ������ => new PathNode(_gridPosition) � ��������� �� ��������. (������ ��������� ����� ������� � ��������� �����)

            // _gridSystemList.CreateDebugObject(_pathfindingGridDebugObject); // �������� ��� ������ � ������ ������// ���������� ������ ����� ������� �.�. ��������� ���������

            _gridSystemList.Add(gridSystem);
        }

        // � ����� �������� ��� ������ �� ����������� ������������, ����� �������� ����� �� ������ ������� � ��� ������ ������� ��� ���� ������������� � ���� 1�� ��� ����� � ��� �� ������� ���������� � 2��� ��� �������� �� ����������� _obstaclesDoorMousePlaneCoverLayerMask, ���� ��� ���� ��������� ��� ������ ����� �� ����������
        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                for (int flooor = 0; flooor < floorAmount; flooor++)
                {
                    GridPosition gridPosition = new GridPosition(x, z, flooor); // ������� �����
                    Vector3 worldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition); // ������� ������� ����������
                    float raycastOffsetDistance = 1f; // ��������� �������� ����

                    GetNode(x, z, flooor).SetIsWalkable(false); //��� ������ ������� ��� ���� �������������

                    //��������� ���. // ��� ������� ���������� ��� �� ��� ��������� �������� ����� �������� ������ ����,  ������� ������� ��� �����, � ����� �������� ����, ����� ������ �������� � ��� ���� ������ ��� �������� ����, ��� ����� ����������������� � ��������� ������ ����
                    if (Physics.Raycast(
                         worldPosition + Vector3.up * raycastOffsetDistance,
                         Vector3.down,
                         raycastOffsetDistance * 2,
                         _floorLayerMask)) // ���� ��� ����� � ��� �� ������� ��� ���������� (Raycast -������ bool ����������)
                    {
                        GetNode(x, z, flooor).SetIsWalkable(true);
                    }
                    else // ���� ��� ������� ��� ���� �� ������� �� � ��������� ������
                    {
                        _gridPositionInAirList.Add(gridPosition);
                    }

                    //��������� ���. ��� �� ����� �������� ������ ���������(������ �����), ������� ������� ��� ����, � ����� �������� �����, ����� ������ �������� � ��� ���� ������ ��� �������� ����, ��� ����� ����������������� � ��������� ������ ����
                    // ����� ������� ��������� � UNITY � �� ������� ������� ����. Project Settings/Physics/Queries Hit Backfaces - ��������� �������, � ����� ����� �������� �� ����� ���������
                    if (Physics.Raycast(
                         worldPosition + Vector3.down * raycastOffsetDistance,
                         Vector3.up,
                         raycastOffsetDistance * 2,
                         _obstaclesCoverLayerMask)) // ���� ��� ����� � ����������� ��� � ������� �� ��������� ������ �� ���������� (Raycast -������ bool ����������)
                    {
                        GetNode(x, z, flooor).SetIsWalkable(false);
                    }
                }
            }
        }

        _pathfindingLinkList = new List<PathfindingLink>();

      //  if (_pathfindingLinkContainer.childCount !=0) // ���� ��������� �������� �������� ������� ��
       // {
            foreach (Transform pathfindingLinkTransform in _pathfindingLinkContainer) // ��������� �������� �������� ���������� ������ ��� ������ ����
            {
                if (pathfindingLinkTransform.TryGetComponent(out PathfindingLinkMonoBehaviour pathfindingLinkMonoBehaviour)) // � ��������� ������� ��������� ������� PathfindingLinkMonoBehaviour
                {
                    _pathfindingLinkList.Add(pathfindingLinkMonoBehaviour.GetPathfindingLink()); // // ������� �������� ������� � ������� � ������ ������ ��� ������ ����
                }
            }
       // }

    }

    public List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition endGridPosition, out int pathLength) // ����� ���� // ���� �� ����������� �������� ����� "out", �� ������� ������ ���������� �������� ��� ���� ���������� pathLength-����� ����, �� ���� ��� ������ ������� ��������.
    {
        List<PathNode> openList = new List<PathNode>();     // �������� ������ "�������� ������" - ��� ���� ������� ��������� ����� 
        List<PathNode> closedList = new List<PathNode>();   // �������� ������ "�������� ������" - ��� ����, � ������� ��� ��� ���������� �����

        PathNode startNode = GetGridSystem(startGridPosition.floor).GetGridObject(startGridPosition); // ������� ��������� ����, ������ GridObject ���� PathNode � ���������� ��� startGridPosition
        PathNode endNode = GetGridSystem(endGridPosition.floor).GetGridObject(endGridPosition); // ������� �������� ����, ������ GridObject ���� PathNode � ���������� ��� endGridPosition

        openList.Add(startNode); //������� � ������ ��������� ����

        for (int x = 0; x < _width; x++) // � ����� ������� ��� ���� �������� ������� � ������� ���������
        {
            for (int z = 0; z < _height; z++)
            {
                for (int flooor = 0; flooor < _floorAmount; flooor++)
                {
                    GridPosition gridPosition = new GridPosition(x, z, flooor); // ������� �����

                    PathNode pathNode = GetGridSystem(flooor).GetGridObject(gridPosition); // ������� ������ ����� ���� PathNode

                    pathNode.SetGCost(int.MaxValue); // ��������� �������� G ������������ ������
                    pathNode.SetHCost(0);            // ��������� �������� H =0
                    pathNode.CalculateFCost();       // ��������� �������� F
                    pathNode.ResetCameFromPathNode(); // ������� ������ �� ���������� ���� ���� 
                }
            }
        }

        startNode.SetGCost(0); // G -��� ��������� �������� �� ����������� ���� � ���������� ������� ���������. �� ��� �� ������ �� ������������ ������� G=0
        startNode.SetHCost(CalculateHeuristicDistance(startGridPosition, endGridPosition)); // ��������� ����������� H ���������
        startNode.CalculateFCost();

        while (openList.Count > 0) // ���� � �������� ������ ���� �������� �� ��� �������� ��� ���� ���� ��� ������. ���� ����� �������� ���� �� ��������� ��� ������
        {
            PathNode currentNode = GetLowestFCostPathNode(openList); // ������� ���� ���� � ���������� ���������� F �� openList � ������� ��� ������� �����

            if (currentNode == endNode) // ��������� ����� �� ��� ������� ���� ��������� ����
            {
                // �������� ��������� ����
                pathLength = endNode.GetFCost(); // ������ ��������� ����� ����
                return CalculatePath(endNode); // ������ ���������� ����
            }

            openList.Remove(currentNode); // ������ ������� ���� �� ��������� ������
            closedList.Add(currentNode);  // � ������� � �������� ������ // ��� �������� ��� �� ������ �� ����� ����

            foreach (PathNode neighbourNode in GetNeighbourList(currentNode)) // ��������� ���� �������� ����
            {
                if (closedList.Contains(neighbourNode)) // ��������� ��� �� �������� ���� ����� � "�������� ������"
                {
                    //�� ��� ������ �� ����� ����
                    continue;
                }

                //  ��������� ���� �� ��������� ��� ������
                if (!neighbourNode.GetIsWalkable()) // ��������, �������� ���� - �������� ��� ������ // ���� �� �������� �� ������� � "�������� ������" � �������� � ���������� ����
                {
                    closedList.Add(neighbourNode);
                    continue;
                }
#if HEX_GRID_SYSTEM // ���� ������������ �������� �������

                int tentativeGCost =currentNode.GetGCost() + MOVE_STRAIGHT_COST;// ��������������� ��������� G = ������� G + ��������� �������� �����

#else//� ��������� ������ ������������� 
                int tentativeGCost = currentNode.GetGCost() + CalculateHeuristicDistance(currentNode.GetGridPosition(), neighbourNode.GetGridPosition()); // ��������������� ��������� G = ������� G + ��������� ����������� �� �������� � ��������� ����
#endif

                if (tentativeGCost < neighbourNode.GetGCost()) // ���� ��������������� ��������� G ������ ��������� G ��������� ����(�� ��������� ��� ����� �������� MaxValue ) (�� ����� ����� ���� ��� �� ������� � ���� �������� ����)
                {
                    neighbourNode.SetCameFromPathNode(currentNode); // ���������� - �� �������� ���� ������ - � �������� ���� ����
                    neighbourNode.SetGCost(tentativeGCost); // ��������� �� �������� ���� ����������� �������� G
                    neighbourNode.SetHCost(CalculateHeuristicDistance(neighbourNode.GetGridPosition(), endGridPosition)); // ��������� �������� H �� �������� �� ���������
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode)) // ���� �������� ������ �� �������� ����� ��������� ���� �� ������� ���
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }

        // ���� �� ������
        pathLength = 0;
        return null;
    }

    public int CalculateHeuristicDistance(GridPosition gridPositionA, GridPosition gridPositionB) // ��������� �������������(���������������) ���������� ����� ������ 2�� ��������� ��������� ��� ����� �������� "H"
    {
#if HEX_GRID_SYSTEM // ���� ������������ �������� �������

        return Mathf.RoundToInt(MOVE_STRAIGHT_COST *Vector3.Distance(GetGridSystem(gridPositionA.floor).GetWorldPosition(gridPositionA), GetGridSystem(gridPositionB.floor).GetWorldPosition(gridPositionB)));


#else//� ��������� ������ ������������� 
        GridPosition gridPositionDistance = gridPositionA - gridPositionB; //����� ���� ������������� (��������� ���������� ��������� ����� ������ � ������ ������ - �� ������� ���� �������� ������������ ������ �����)

        // ��� �������� ������ �� ������ (����� ������������ ��� ������ ����� ������)
        //int totalDistance = Mathf.Abs(gridPositionDistance.x) + Mathf.Abs(gridPositionDistance.z); // ������� ����� ��������� �� ������ (�� ��������� �������� �� ��������� ������ �� ������. �������� �� ����� (0,0) � ����� (3,2) �������� ��� ���� � ����� � ��� ���� ����� ����� 5)

        int xDistsnce = Mathf.Abs(gridPositionDistance.x); //����������� � (0,0) �� (1,1) ����� �������� 1 ����������, ������ ���� ����������� �� ������ // ���� � (0,0) �� (2,1) �� ��������� ����� ��� ����� ����. ������� ��� ������� ���������� ���������� ���� ����� ����������� ����� �� ���������� (x,z)
        int zDistsnce = Mathf.Abs(gridPositionDistance.z);
        int remaining = Mathf.Abs(xDistsnce - zDistsnce); // ���������� ��������� ����� ������������ �� ������, ����� �� ������
        int CalculateDistance = MOVE_DIAGONAL_COST * Mathf.Min(xDistsnce, zDistsnce) + MOVE_STRAIGHT_COST * remaining; // ������ ����������� ���������� � ������� GridPosition ��� ����� �������� �� ��������� � �������� �� ������
        return CalculateDistance;
#endif
    }

    private PathNode GetLowestFCostPathNode(List<PathNode> pathNodeList) // �������� ���� ���� � ���������� ���������� F  � �������� ��������� ������ ����� ����// ��� ����������� ������ ����� �� ������ �� ���� ������
                                                                         // ���� ��������� ����� ����� ���������� ���������� F �� �������� ������ ���������� � ������
                                                                         // � ����� ������ �� ���������� ������ ������ ������� (������� �� ������������ ������ � ������ GetNeighbourList())
    {
        PathNode lowestFCostPathNode = pathNodeList[0]; // ������� ������ ������� � ������
        for (int i = 0; i < pathNodeList.Count; i++) // ��������� � ����� ������ ��������
        {
            if (pathNodeList[i].GetFCost() < lowestFCostPathNode.GetFCost()) // ���� �������� F ������ �������� ������ �������� �� ������� ��� lowestFCostPathNode
            {
                lowestFCostPathNode = pathNodeList[i];
            }
        }
        return lowestFCostPathNode;
    }

    private GridSystemHexAndQuad<PathNode> GetGridSystem(int floor) // �������� �������� ������� ��� ������� �����
    {
        return _gridSystemList[floor];
    }

    private PathNode GetNode(int x, int z, int floor) // �������� ���� � ������������ �� GridPosition(x,z)
    {
        return GetGridSystem(floor).GetGridObject(new GridPosition(x, z, floor));
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode) // �������� ������ ������� ��� currentNode (��� ������������ 6 �������� , ��� ����������  8 �������
    {
        List<PathNode> neighbourList = new List<PathNode>(); // �������������� ����� ������ �������

        GridPosition gridPosition = currentNode.GetGridPosition();

        // ������� �������� ����� �� ���� �� ������� ����� ����� ������� ������ ������ �� ������� ������

        if (gridPosition.x - 1 >= 0)
        {
            //Left
            neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z + 0, gridPosition.floor)); // ������� ���� �����

#if HEX_GRID_SYSTEM // ���� ������������ �������� �������
// ��� ����������� ��� ������
#else//� ��������� ������ ������������� 
            if (gridPosition.z - 1 >= 0)
            {
                //Left Down
                neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z - 1, gridPosition.floor)); // ������� ���� ����� � ����
            }

            if (gridPosition.z + 1 < _height)
            {
                //Left Up
                neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z + 1, gridPosition.floor)); // ������� ���� ����� � �����
            }
#endif
        }

        if (gridPosition.x + 1 < _width)
        {
            //Right
            neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z + 0, gridPosition.floor)); // ������� ���� ������

#if HEX_GRID_SYSTEM // ���� ������������ �������� �������
// ��� ����������� ��� ������
#else//� ��������� ������ ������������� 
            if (gridPosition.z - 1 >= 0)
            {
                //Right Down
                neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z - 1, gridPosition.floor)); // ������� ���� ������ � ����
            }

            if (gridPosition.z + 1 < _height)
            {
                //Right Up
                neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z + 1, gridPosition.floor)); // ������� ���� ������ � �����
            }
#endif
        }

        // ��� ������������� ���� �������� - �� ����� ����� � ����� � �����. ������ - �� ������ � ����� � ������ �����. (������� � ������ ������� ��������� ����� �������� oddRow)
        if (gridPosition.z - 1 >= 0)
        {
            //Down
            neighbourList.Add(GetNode(gridPosition.x + 0, gridPosition.z - 1, gridPosition.floor)); // ������� ���� �����
        }
        if (gridPosition.z + 1 < _height)
        {
            //Up
            neighbourList.Add(GetNode(gridPosition.x + 0, gridPosition.z + 1, gridPosition.floor)); // ������� ���� ������
        }


#if HEX_GRID_SYSTEM // ���� ������������ �������� �������

        bool oddRow = gridPosition.z % 2 == 1; // oddRow - �������� ���.���� ������ �� �� ��������� � �������� ����

        if (oddRow) // ���� ��������
        {
            if (gridPosition.x + 1 < _width)
            {
                if (gridPosition.z - 1 >= 0)
                {
                    neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z - 1, gridPosition.floor));// ������� ���� ������ �����
                }
                if (gridPosition.z + 1 < _height)
                {
                    neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z + 1, gridPosition.floor)); // ������� ���� ������ � �����
                }
            }
        }
        else // ���� ������
        {
            if (gridPosition.x - 1 >= 0)
            {
                if (gridPosition.z - 1 >= 0)
                {
                    neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z - 1, gridPosition.floor)); // ������� ���� ����� �����
                }
                if (gridPosition.z + 1 < _height)
                {
                    neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z + 1, gridPosition.floor)); // ������� ���� ����� � �����
                }
            }
        }
#endif

        List<PathNode> totalNeighbourList = new List<PathNode>(); // ����� ������ �������
        totalNeighbourList.AddRange(neighbourList); //AddRange- ��������� �������� ��������� ��������� � ����� ������

        List<GridPosition> pathfindingLinkGridPositionList = GetPathfindingLinkConnectedGridPositionList(gridPosition); // �������� ������ �������� ������� ������� ������� � ���� �������� ��������

        foreach (GridPosition pathfindingLinkGridPosition in pathfindingLinkGridPositionList) // ��������� ������ � ����������� GridPosition(�������� �������) � PathNode(���� ����)
        {
            totalNeighbourList.Add(
                GetNode( // ������� ���� � ������������
                    pathfindingLinkGridPosition.x,
                    pathfindingLinkGridPosition.z,
                    pathfindingLinkGridPosition.floor
                )
            );
        }


        return totalNeighbourList;
    }

    private List<GridPosition> GetPathfindingLinkConnectedGridPositionList(GridPosition gridPosition) // �������� ������ �������� ������� ������� ������� � ���� �������� ��������
    {
        List<GridPosition> gridPositionList = new List<GridPosition>(); // �������������� ������

        foreach (PathfindingLink pathfindingLink in _pathfindingLinkList) // ��������� ����� ������ ��� ������ ����
        {
            // ���� ���� �������� ������� ���������  � ����� �� ������� ������ �� ������� � ������ ������ ������� ������
            if (pathfindingLink.gridPositionA == gridPosition)
            {
                gridPositionList.Add(pathfindingLink.gridPositionB);
            }
            if (pathfindingLink.gridPositionB == gridPosition)
            {
                gridPositionList.Add(pathfindingLink.gridPositionA);
            }
        }

        return gridPositionList;
    }

    //2 ������ ���������� ������ GetNeighbourList() ����� ��������
    /* private List<PathNode> GetNeighbourList(PathNode currentNode) // �������� ������ ������� ��� currentNode
     {
         List<PathNode> neighbourList = new List<PathNode>(); // �������������� ����� ������ �������

         GridPosition _gridPosition = currentNode.GetGridPosition(); // ������� �������� ������� ������ ������

         GridPosition[] neigboursPositions = //�������� ������ �������� ������� �������� �����
         {
         _gridPosition + UP,
         _gridPosition + UP + RIGHT,
         _gridPosition + RIGHT,
         _gridPosition + RIGHT + DOWN,
         _gridPosition + DOWN,
         _gridPosition + DOWN + LEFT,
         _gridPosition + LEFT,
         _gridPosition + LEFT + UP
         };

         foreach (GridPosition p in neigboursPositions) // � ����� �������� �� ������������ ���� �������� �������
         {
             if (_gridSystemList.IsValidGridPosition(p))
             {
                 neighbourList.Add(GetNode(p.x, p.z));
             }                
         }

         return neighbourList;
     }*/

    private List<GridPosition> CalculatePath(PathNode endNode) //���������� ���� (����� ����������� ������ � �������� �����������)
    {
        List<PathNode> pathNodeList = new List<PathNode>(); // �������������� ������ ����� ����
        pathNodeList.Add(endNode);
        PathNode currentNod = endNode;

        while (currentNod.GetCameFromPathNode() != null) //� ����� ������� ������������ ���� � ������. ������������ ���� - ��� �� � �������� �� ���� ������ (� ���� ����� 1 ������ �� �������� � �������� ������). �������� ���� ����� ��������� � � ���� GetCameFromPathNode() = null - ���� ���������
        {
            pathNodeList.Add(currentNod.GetCameFromPathNode());//������� � ������ ��� ������������ ����
            currentNod = currentNod.GetCameFromPathNode(); // ������������ ���� ����������� �������
        }

        pathNodeList.Reverse(); // �.�. �� ������ � ����� ���� ����������� ��� ������ ��� �� �������� �������� �� ������ � ������
        // ��������� ��� ������ "PathNode-�����" � ������ "GridPosition - ������� �����"
        List<GridPosition> gridPositionList = new List<GridPosition>();

        foreach (PathNode pathNode in pathNodeList)
        {
            gridPositionList.Add(pathNode.GetGridPosition());
        }

        return gridPositionList;
    }

    public void SetIsWalkableGridPosition(GridPosition gridPosition, bool isWalkable) // ���������� ��� ����� ��� ������ (� ����������� �� isWalkable)  ������ �� ���������� � �������� �������� �������
    {
        GetGridSystem(gridPosition.floor).GetGridObject(gridPosition).SetIsWalkable(isWalkable);
    }

    public bool IsWalkableGridPosition(GridPosition gridPosition) // ����� ������ �� ���������� � �������� �������� �������
    {
        return GetGridSystem(gridPosition.floor).GetGridObject(gridPosition).GetIsWalkable();
    }

    public bool HasPath(GridPosition startGridPosition, GridPosition endGridPosition) // ����� ���� ?
    {
        return FindPath(startGridPosition, endGridPosition, out int pathLength) != null; // ���� ���� ���� �� startGridPosition � endGridPosition �� �������� true ���� ���� ��� �������� false (out int pathLength �������� ��� �� ��������������� ���������)
    }

    public int GetPathLength(GridPosition startGridPosition, GridPosition endGridPosition) // �������� ����� ���� (�������� F -endGridPosition)  
    {
        FindPath(startGridPosition, endGridPosition, out int pathLength);
        return pathLength;
    }

    public List<GridPosition> GetGridPositionInAirList() //�������� ������ ������� � �������
    {
        return _gridPositionInAirList;
    }

}
