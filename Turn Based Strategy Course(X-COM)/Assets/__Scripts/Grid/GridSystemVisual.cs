//#define HEX_GRID_SYSTEM //������������ �������� ������� //  � C# ��������� ��� �������� �������������, ����������� ������� �� ��������public enum����� ��������� ���� ��������� ������������. 
//��� ��������� ���������� ������� ������������� ������ ��������� ����� �� ����������� � ��������� ��� � ��� �������� �����, ��� ��� ����������. 

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using static GridSystemVisual;
using static UnitActionSystem;


// ������� GridSystemVisual ��� ������� ����� ������� �� ���������, ��������� �� �����, ����� ���������� ������� ����������� ����� ����� ����������.
// (Project Settings/ Script Execution Order � �������� ���������� GridSystemVisual ���� Default Time)
public class GridSystemVisual : MonoBehaviour //�������� ������� ������������  ������������ ��������� ����� �� ����� 
{
    public static GridSystemVisual Instance { get; private set; }   //(������� SINGLETON) ��� �������� ������� ����� ���� ������� (SET-���������) ������ ���� �������, �� ����� ���� �������� GET ����� ������ �������
                                                                    // instance - ���������, � ��� ����� ���� ��������� UnitActionSystem ����� ���� ��� static. Instance ����� ��� ���� ����� ������ ������, ����� ����, ����� ����������� �� Event.

    [Serializable] // ����� ��������� ��������� ����� ������������ � ����������
    public struct GridVisualTypeMaterial    //������ ����� ��� ��������� // �������� ��������� ����� � ��������� ������. ������ � �������� ��������� ������������ ��� ���� ������ �������� ����������� ����� ������ � C#
    {                                       //� ������ ��������� ��������� ��������� ����� � ����������
        public GridVisualType gridVisualType;
        public Material materialGrid;
    }

    public enum GridVisualType //���������� ��������� �����
    {
        White,
        Blue,
        BlueSoft,
        Red,
        RedSoft,
        Yellow,
        YellowSoft,
        Green,
        GreenSoft,
    }


    [SerializeField] private Transform _gridSystemVisualSinglePrefab; // ������ ������������ 

    [SerializeField] private List<GridVisualTypeMaterial> _gridVisualTypeMaterialListQuad; // ������ ��� ��������� ����������� ��������� ����� ������� (������ �� ���������� ���� ������) ����������� ��������� ����� // � ���������� ��� ������ ��������� ���������� ��������������� �������� �����
    [SerializeField] private List<GridVisualTypeMaterial> _gridVisualTypeMaterialListHex; // ������ ��� ��������� ����������� ��������� ����� ������������ (������ �� ���������� ���� ������) ����������� ��������� ����� // � ���������� ��� ������ ��������� ���������� ��������������� �������� �����


    private List<GridPosition> _validActionGridPositionForGrenadeActionList; // ����� ���������� -������ ���������� �������� ������� ��� �������� ������� 
    private List<GridPosition> _validActionGridPositionForComboActionList; // ����� ���������� -������ ���������� �������� ������� ��� �������� �����

    private GridSystemVisualSingle[,,] _gridSystemVisualSingleArray; // ���������� ������    

    // ��� ������� ������������ ����� (����������� ������ ��� ������)
    // private GridSystemVisualSingle _lastSelectedGridSystemVisualSingle; // ��������� ��������� �������� ������� ������������ �������

    private void Awake() //��� ��������� ������ Awake ����� ������������ ������ ��� ������������� � ���������� ��������
    {
        // ���� �� �������� � ���������� �� �������� �� �����
        if (Instance != null) // ������� �������� ��� ���� ������ ���������� � ��������� ����������
        {
            Debug.LogError("There's more than one UnitActionSystem!(��� ������, ��� ���� UnitActionSystem!) " + transform + " - " + Instance);
            Destroy(gameObject); // ��������� ���� ��������
            return; // �.�. � ��� ��� ���� ��������� UnitActionSystem ��������� ����������, ��� �� �� ��������� ������ ����
        }
        Instance = this;
    }

    private void Start()
    {
        _gridSystemVisualSingleArray = new GridSystemVisualSingle[ // ������� ������ ������������� �������� width �� height  � loorAmount
            LevelGrid.Instance.GetWidth(),
            LevelGrid.Instance.GetHeight(),
            LevelGrid.Instance.GetFloorAmount()
        ];

        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                for (int floor = 0; floor < LevelGrid.Instance.GetFloorAmount(); floor++)  // ��������� ��� �����
                {
                    GridPosition gridPosition = new GridPosition(x, z, floor);

                    Transform gridSystemVisualSingleTransform = Instantiate(_gridSystemVisualSinglePrefab, LevelGrid.Instance.GetWorldPosition(gridPosition), Quaternion.identity); // �������� ��� ������ � ������ ������� �����

                    _gridSystemVisualSingleArray[x, z, floor] = gridSystemVisualSingleTransform.GetComponent<GridSystemVisualSingle>(); // ��������� ��������� GridSystemVisualSingle � ���������� ������ ��� x,z,floor ��� ����� ������� �������.

                }
            }
        }

        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged; // ���������� �� ������� ��������� �������� �������� (����� �������� �������� �������� � ����� ������ �� �������� ������� Event)
        UnitActionSystem.Instance.OnBusyChanged += Instance_OnBusyChanged; //���������� �� ������� ��������� �������� 

        //  LevelGrid.Instance.OnAnyUnitMovedGridPosition += LevelGrid_OnAnyUnitMovedGridPosition; // ���������� �� ������� ����� ���� ��������� � �������� �������

        MouseWorld.OnMouseGridPositionChanged += MouseWorld_OnMouseGridPositionChanged;// ���������� �� ������� �������� ������� ���� �������� ��� ��������� � ���������� ����� �����. �������� ��������� �������

        UpdateGridVisual();


        // ��������� ��� ����� ��� ������� HEX(�������������� ������ � ������ MouseWorld_OnMouseGridPositionChanged)
        /* for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
         {
             for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
             {
                 _gridSystemVisualSingleArray[x, z].
                     Show(GetGridVisualTypeMaterial(GridVisualType.White));
             }
         }*/

    }


    // ��� ������� ������������ ����� (����������� ������ ��� ������)
    /*private void Update()
    {

        if (_lastSelectedGridSystemVisualSingle != null)
        {
            _lastSelectedGridSystemVisualSingle.HideSelected(); // ������� ��������� ��������� GridSystemVisualSingle
        }

        Vector3 mouseWorldPosition = MouseWorld.GetPosition(); //������� ������� ����
        GridPosition gridPosition = LevelGrid.Instance.GetGridPosition(mouseWorldPosition); // ������� �������� ������� ����
        if (LevelGrid.Instance.IsValidGridPosition(gridPosition)) // ���� ��� ���������� �������� ������� ��
        {
            _lastSelectedGridSystemVisualSingle = _gridSystemVisualSingleArray[gridPosition.x, gridPosition.z]; // �������� ��� ���������� GridSystemVisualSingle
        }

        if (_lastSelectedGridSystemVisualSingle != null)
        {
            _lastSelectedGridSystemVisualSingle.ShowSelected();// ������� ��������� ��������� GridSystemVisualSingle
        }
    }*/


    private void Instance_OnBusyChanged(object sender, OnUnitSystemEventArgs e)
    {
        UpdateGridVisual();
    }

    private void MouseWorld_OnMouseGridPositionChanged(object sender, MouseWorld.OnMouseGridPositionChangedEventArgs e)
    {
        // ��� ��������� ��������� ���� ����� ��������� ����������� ����� ��� ��������, ������� ���������� ������ �������� �������

        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction(); // ������� ��������� ��������

        switch (selectedAction) // ������������� ��������� ������� � ����������� �� ���������� ��������
        {
            case GrenadeFragmentationAction grenadeFragmentationAction:// �� ����� ������� �������

                UpdateVisualDamageCircleGrenade(grenadeFragmentationAction, e);
                break;

            case GrenadeStunAction grenadeStunAction:// �� ����� ������� �������

                UpdateVisualDamageCircleGrenade(grenadeStunAction, e);
                break;

            case GrenadeSmokeAction grenadeSmokeAction:

                UpdateVisualDamageQuadGrenade(grenadeSmokeAction, e);
                break;

            case ComboAction comboAction:
                if (comboAction.GetState() == ComboAction.State.ComboStart)
                {
                    UpdateVisualSelectedQuadComboAction(comboAction, e);
                }
                break;
        }
    }

    private void UpdateVisualSelectedQuadComboAction(ComboAction comboAction, MouseWorld.OnMouseGridPositionChangedEventArgs e)
    {
        _gridSystemVisualSingleArray[e.lastMouseGridPosition.x, e.lastMouseGridPosition.z, e.lastMouseGridPosition.floor].HideQuad(); // ������ ������� �� ���������� ������
        GridPosition mouseGridPosition = e.newMouseGridPosition; // �������� ������� ����

        if (_validActionGridPositionForComboActionList.Contains(mouseGridPosition)) // ���� �������� ������� ���� ������ � ���������� �������� �� ...
        {
            _gridSystemVisualSingleArray[mouseGridPosition.x, mouseGridPosition.z, mouseGridPosition.floor].ShowQuad(GetGridVisualTypeMaterial(GridVisualType.Red)); // ������� ��� ����� �������� ������� ������� �������� �����
        }
    }

    private void UpdateVisualDamageQuadGrenade(GrenadeAction grenadeAction, MouseWorld.OnMouseGridPositionChangedEventArgs e)
    {
        _gridSystemVisualSingleArray[e.lastMouseGridPosition.x, e.lastMouseGridPosition.z, e.lastMouseGridPosition.floor].HideQuad(); // ������ ������� �� ���������� ������
        GridPosition mouseGridPosition = e.newMouseGridPosition; // �������� ������� ����

        if (_validActionGridPositionForGrenadeActionList.Contains(mouseGridPosition)) // ���� �������� ������� ���� ������ � ���������� �������� �� ...
        {
            float damageRadiusInWorldPosition = grenadeAction.GetDamageRadiusInWorldPosition();
            _gridSystemVisualSingleArray[mouseGridPosition.x, mouseGridPosition.z, mouseGridPosition.floor].ShowQuad(GetGridVisualTypeMaterial(GridVisualType.RedSoft), damageRadiusInWorldPosition); // ������� ��� ����� �������� ������� ������� ��������� �� ������� � ��������� ������  � ���� ���������
        }
    }

    private void UpdateVisualDamageCircleGrenade(GrenadeAction grenadeAction, MouseWorld.OnMouseGridPositionChangedEventArgs e)
    {
        _gridSystemVisualSingleArray[e.lastMouseGridPosition.x, e.lastMouseGridPosition.z, e.lastMouseGridPosition.floor].Hide�ircle(); // ������ ���� �� ���������� ������
        GridPosition mouseGridPosition = e.newMouseGridPosition; // �������� ������� ����

        if (_validActionGridPositionForGrenadeActionList.Contains(mouseGridPosition)) // ���� �������� ������� ���� ������ � ���������� �������� �� ...
        {
            float damageRadiusInWorldPosition = grenadeAction.GetDamageRadiusInWorldPosition();
            _gridSystemVisualSingleArray[mouseGridPosition.x, mouseGridPosition.z, mouseGridPosition.floor].Show�ircle(damageRadiusInWorldPosition); // ������� ��� ����� �������� ������� ���� ��������� �� ������� � ��������� ������ �����
        }
    }

    private void HideAllGridPosition() // ������ ��� ������� �����
    {
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                for (int floor = 0; floor < LevelGrid.Instance.GetFloorAmount(); floor++)  // ��������� ��� �����
                {
                    _gridSystemVisualSingleArray[x, z, floor].Hide();
                }
            }
        }
    }

    private void ShowGridPositionRange(GridPosition gridPosition, int range, GridVisualType gridVisualType, bool showFigureRhombus) // �������� ��������� �������� �������� ������� ��� �������� (� ��������� �������� �������� �������, ������ ��������, ��� ��������� ������� �����, ������� ���������� ���� ���� ���������� � ���� ����� �� �������� true, ���� � ���� �������� �� - false )
    {
        // �� �������� ��� � ShootAction � ������ "public override List<GridPosition> GetValidActionGridPositionList()"

        List<GridPosition> gridPositionList = new List<GridPosition>();

        for (int x = -range; x <= range; x++)  // ���� ��� ����� ����� ������� � ������������ unitGridPosition, ������� ��������� ���������� �������� � �������� ������� range
        {
            for (int z = -range; z <= range; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z, 0); // ��������� �������� �������. ��� ������� ���������(0,0) �������� ��� ���� 
                GridPosition testGridPosition = gridPosition + offsetGridPosition; // ����������� �������� �������

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) // �������� �������� �� testGridPosition ���������� �������� �������� ���� ��� �� ��������� � ���� �����
                {
                    continue; // continue ���������� ��������� ���������� � ��������� �������� ����� 'for' ��������� ��� ����
                }

                if (showFigureRhombus)
                {
                    // ��� ������� �������� ������� ���� � �� �������
                    int testDistance = Mathf.Abs(x) + Mathf.Abs(z); // ����� ���� ������������� ��������� �������� �������
                    if (testDistance > range) //������� ������ �� ����� � ���� ����� // ���� ���� � (0,0) �� ������ � ������������ (5,4) ��� �� ������� �������� 5+4>7
                    {
                        continue;
                    }
                }

                //�������� �������� ������� ������� ����� � �������
                if (PathfindingMonkey.Instance.GetGridPositionInAirList().Contains(testGridPosition))
                {
                    continue;
                }

                gridPositionList.Add(testGridPosition);
            }
        }

        ShowGridPositionList(gridPositionList, gridVisualType); // ������� ��������� �������� ��������
    }

    public void ShowGridPositionList(List<GridPosition> gridPositionlist, GridVisualType gridVisualType)  //������� ������ GridPosition (� ��������� ���������� ������ GridPosition � ��������� ������������ ����� gridVisualType)
    {
        foreach (GridPosition gridPosition in gridPositionlist) // � ����� ��������� ������ � �������(�������) ������ �� ������� ������� ��� ��������
        {
            _gridSystemVisualSingleArray[gridPosition.x, gridPosition.z, gridPosition.floor].
                Show(GetGridVisualTypeMaterial(gridVisualType)); // � �������� Show �������� �������� � ����������� �� ����������� ��� �������
        }
    }

    public void UpdateGridVisual() // ���������� ������� �����
    {
        HideAllGridPosition(); // ������ ��� ������� �����

        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit(); //������� ���������� �����

        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction(); // ������� ��������� ��������

        GridVisualType gridVisualType;  // �������� ����� ���� GridVisualType

        switch (selectedAction) // ������������� ��������� ������� ����� � ����������� �� ���������� ��������
        {
            default: // ���� ���� ����� ����������� �� ��������� ���� ��� ��������������� selectedAction
            case MoveAction moveAction: // �� ����� ������ -�����
                gridVisualType = GridVisualType.White;
                break;

            case SpinAction spinAction: // �� ����� �������� -�������
                gridVisualType = GridVisualType.Blue;
                break;

            case HealAction healAction: // �� ����� ������� -�������
                gridVisualType = GridVisualType.Green;
                ShowGridPositionRange(selectedUnit.GetGridPosition(), healAction.GetMaxHealDistance(), GridVisualType.GreenSoft, false); // ������� �������� 
                break;

            case ShootAction shootAction: // �� ����� �������� -�������
                gridVisualType = GridVisualType.Red;
                ShowGridPositionRange(selectedUnit.GetGridPosition(), shootAction.GetMaxShootDistance(), GridVisualType.RedSoft, true); // ������� �������� �������� ����-true
                break;

            case GrenadeAction grenadeAction:// �� ����� ������� ������� -������
                gridVisualType = GridVisualType.Yellow;
                _validActionGridPositionForGrenadeActionList = selectedAction.GetValidActionGridPositionList(); //�������� -������ ���������� �������� ������� ��� �������� �������                                 
                break;

            case SwordAction swordAction: // �� ����� ����� ����� -�������
                gridVisualType = GridVisualType.Red;
                ShowGridPositionRange(selectedUnit.GetGridPosition(), swordAction.GetMaxSwordDistance(), GridVisualType.RedSoft, false); // ������� �������� �����
                break;

            case InteractAction interactAction: // �� ����� �������������� -�������
                gridVisualType = GridVisualType.Blue;
                ShowGridPositionRange(selectedUnit.GetGridPosition(), interactAction.GetMaxInteractDistance(), GridVisualType.BlueSoft, false); // ������� �������� 
                break;

            case ComboAction comboAction: // �� ����� ������ ����� �������� -�������    

                ComboAction.State statecomboAction = comboAction.GetState(); // ������� ��������� ����� 
                switch (statecomboAction)
                {
                    default:
                    case ComboAction.State.ComboSearchPartner: // ���� ���� �������� ��� �����
                        gridVisualType = GridVisualType.Green;
                        ShowGridPositionRange(selectedUnit.GetGridPosition(), comboAction.GetMaxComboDistance(), GridVisualType.GreenSoft, false); // ������� �������� 
                        break;

                    case ComboAction.State.ComboSearchEnemy: // ���� ���� ����� ��
                        gridVisualType = GridVisualType.Red;
                        ShowGridPositionRange(selectedUnit.GetGridPosition(), comboAction.GetMaxComboDistance(), GridVisualType.RedSoft, true); // ������� ��������  ����-true
                        break;

                    case ComboAction.State.ComboStart: // ������ ���� ���� ����������
                        gridVisualType = GridVisualType.RedSoft;
                        _validActionGridPositionForComboActionList = selectedAction.GetValidActionGridPositionList();
                        break;

                }
                break;

            case SpotterFireAction spotterFireAction: // ������������� ���� -���������
                gridVisualType = GridVisualType.Green;
                ShowGridPositionRange(selectedUnit.GetGridPosition(), spotterFireAction.GetSpotterFireDistance(), GridVisualType.GreenSoft, false); // ������� �������� 
                break;
        }

        ShowGridPositionList(selectedAction.GetValidActionGridPositionList(), gridVisualType); // �������(�������) ������ �� ������� ������� ��� �������� (� �������� �������� ������ ���������� ������� ����� ���������� ��������, � ��� ��������� ������������ ������� ��� ����� switch)
    }

    private void UnitActionSystem_OnSelectedActionChanged(object sender, EventArgs e)
    {
        UpdateGridVisual();
    }

    private void LevelGrid_OnAnyUnitMovedGridPosition(object sender, LevelGrid.OnAnyUnitMovedGridPositionEventArgs e)
    {
        UpdateGridVisual();

        //�������� ���������, ������� �� ������ �������, ��� ��������� ��������� ���������� ������ ���, ����� ���� ���������� �������,
        //������ ����� ��������� ���������� ������ �����, ����� ���� ��������� �������� �����.
        //��� �������� �������� ��� �������� ����������, � ��� ����� ��������� ������� �������.
    }

#if HEX_GRID_SYSTEM // ���� ������������ �������� �������

    private Material GetGridVisualTypeMaterial(GridVisualType gridVisualType) //(������� �������� � ����������� �� ���������) �������� ��� ��������� ��� �������� ������������ � ����������� �� ����������� � �������� ��������� �������� ������������
    {
        foreach (GridVisualTypeMaterial gridVisualTypeMaterial in _gridVisualTypeMaterialListHex) // � ����� ��������� ������ ��� ��������� ����������� ��������� ����� 
        {
            if (gridVisualTypeMaterial.gridVisualType == gridVisualType) // ����  ��������� �����(gridVisualType) ��������� � ���������� ��� ��������� �� ..
            {
                return gridVisualTypeMaterial.materialGrid; // ������ �������� ��������������� ������� ��������� �����
            }
        }

        Debug.LogError("�� ���� ����� GridVisualTypeMaterial ��� GridVisualType " + gridVisualType); // ���� �� ������ ����������� ������ ������
        return null;
    }


#else//� ��������� ������ �������������

    private Material GetGridVisualTypeMaterial(GridVisualType gridVisualType) //(������� �������� � ����������� �� ���������) �������� ��� ��������� ��� �������� ������������ � ����������� �� ����������� � �������� ��������� �������� ������������
    {
        foreach (GridVisualTypeMaterial gridVisualTypeMaterial in _gridVisualTypeMaterialListQuad) // � ����� ��������� ������ ��� ��������� ����������� ��������� ����� 
        {
            if (gridVisualTypeMaterial.gridVisualType == gridVisualType) // ����  ��������� �����(gridVisualType) ��������� � ���������� ��� ��������� �� ..
            {
                return gridVisualTypeMaterial.materialGrid; // ������ �������� ��������������� ������� ��������� �����
            }
        }

        Debug.LogError("�� ���� ����� GridVisualTypeMaterial ��� GridVisualType " + gridVisualType); // ���� �� ������ ����������� ������ ������
        return null;
    }
#endif
}
