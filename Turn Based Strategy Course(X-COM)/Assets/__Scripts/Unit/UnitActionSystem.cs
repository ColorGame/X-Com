using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.UI.CanvasScaler;


// ���� ���� ����� � �� ������ ����������� ����� ������. �������� ���, ������� � Project Settings/ Script Execution Order � �������� ���� Deafault Time
public class UnitActionSystem : MonoBehaviour // ������� �������� ����� (��������� ������ �������� �����)
{

    public static UnitActionSystem Instance { get; private set; }   //(������� SINGLETON) ��� �������� ������� ����� ���� ������� (SET-���������) ������ ���� �������, �� ����� ���� �������� GET ����� ������ �������
                                                                    // instance - ���������, � ��� ����� ���� ��������� UnitActionSystem ����� ���� ��� static. Instance ����� ��� ���� ����� ������ ������, ����� ����, ����� ����������� �� Event.

    public event EventHandler OnSelectedUnitChanged; // ��������� ���� ������� (����� ���������� ��������� ���� �� �������� ������� Event)
    public event EventHandler OnSelectedActionChanged; // ��������� �������� �������� (����� �������� �������� �������� � ����� ������ �� �������� ������� Event)
    public event EventHandler OnActionStarted; // �������� ������ ( �� �������� ������� Event ��� ������ ��������)
    
    public event EventHandler<OnUnitSystemEventArgs> OnBusyChanged; // ��������� �������� (����� �������� �������� _isBusy, �� �������� ������� Event, � �������� �� � ���������) � <> -generic ���� ��� ����� ������ ����������

    public class OnUnitSystemEventArgs : EventArgs // �������� ����� �������, ����� � ��������� ������� �������� ������ ������
    {
        public bool isBusy;
        public BaseAction selectedAction; // ��������� ��������
    }


    [SerializeField] private Unit _selectedUnit; // ��������� ���� (�� ���������).���� ������� ������������� ����� ������� ����� ���������� ���������� �����
    [SerializeField] private LayerMask _unitLayerMask; // ����� ���� ������ (�������� � ����������) ���� ������� Units

    private BaseAction _selectedAction; // ��������� ��������// ����� ���������� � Button
    private bool _isBusy; // ����� (������� ���������� ��� ���������� ������������� ��������)


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
        SetSelectedUnit(_selectedUnit, _selectedUnit.GetAction<MoveAction>()); // ���������(����������) ���������� �����, ���������� ��������� ��������, 
                                        // ��� ������ � _selectedUnit ���������� ���� �� ���������
        
        UnitManager.OnAnyUnitDeadAndRemoveList += UnitManager_OnAnyUnitDeadAndRemoveList; //���������� �� ������� ����� ���� ���� � ������ �� ������
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged; // ���������� ��� �������
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if (TurnSystem.Instance.IsPlayerTurn()) // ���� ��� ������ ��
        {
            List<Unit> friendlyUnitList = UnitManager.Instance.GetFriendlyUnitList(); // ������ ������ ������������� ������
            if (friendlyUnitList.Count > 0) // ���� ���� ����� �� �������� ��������� ������� �� ������ �����
            {
                SetSelectedUnit(friendlyUnitList[0], friendlyUnitList[0].GetAction<MoveAction>());
            }
        };
    }

    private void UnitManager_OnAnyUnitDeadAndRemoveList(object sender, EventArgs e)
    {
        if (_selectedUnit.IsDead()) // ���� ���������� ���� ������� �� ...
        {
            List<Unit> friendlyUnitList = UnitManager.Instance.GetFriendlyUnitList(); // ������ ������ ������������� ������
            if (friendlyUnitList.Count > 0) // ���� ���� ����� �� �������� ��������� ������� �� ������ �����
            {
                SetSelectedUnit(friendlyUnitList[0], friendlyUnitList[0].GetAction<MoveAction>());
            }
            else // ���� ��� ������ � ����� �� �����
            {
                Debug.Log("GAME OVER");
            }
        }
    }   
  

    private void Update()
    {   
        if (_isBusy) // ���� ����� ... �� ���������� ����������
        {
            return;
        }

        if (!TurnSystem.Instance.IsPlayerTurn()) // ��������� ��� ������� ������ ���� ��� �� ���������� ����������
        {
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject())  // ��������, ������� �� ��������� ���� �� �������� ����������������� ����������  
                                                            // ���������� � ����� ������� �������. (current - ���������� ������� ������� �������.) (IsPointerOverGameObject() -��������� ��������� (����) �� ������� ������)
        {
            return; // ���� ��������� ���� ��� �������(UI), ����� ������������� ����� , ��� �� �� ����� �������� �� ������, ���� �� ����� � ����� ����� ������� ���������� ��� �������
        }
        if (TryHandleUnitSelection()) // ������� ��������� ������ �����
        {
            return; //���� �� ������� ����� �� TryHandleUnitSelection() ������ true. ����� ������������� �����, ����� �� ����� �������� �� �����, �������� ����� �������, ���������� ��������� ���� �� ��� � ����� ����� �����
        }

        HandleSelectedAction(); // ���������� ��������� ��������        
    }

    public void HandleSelectedAction() // ���������� ��������� ��������
    {
        if (InputManager.Instance.IsMouseButtonDownThisFrame()) // ��� ������� ��� ������ ���� 
        {
            GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPositionOnlyHitVisible()); // ����������� ������� ���� �� �������� � ��������.

            if (!_selectedAction.IsValidActionGridPosition(mouseGridPosition)) // ��������� ��� ������ ���������� ��������, �������� ������� ���� �� ������������ �������� . ���� �� ��������� ��...
            {
                return; // ���������� ����������  //���������� ! � return; �������� �������� ������ if()
            }

            if (!_selectedUnit.TrySpendActionPointsToTakeAction(_selectedAction)) // ��� ���������� ����� ��������� ��������� ���� ��������, ����� ��������� ��������� ��������. ���� �� ����� ��...
            {
                return; // ���������� ����������
            }

            SetBusy(); // ���������� �������
            _selectedAction.TakeAction(mouseGridPosition, ClearBusy); //� ���������� �������� ������� ����� "��������� �������� (�����������)" � ��������� � ������� ������� ClearBusy

            OnActionStarted?.Invoke(this, EventArgs.Empty); // "?"- ��������� ��� !=0. Invoke ������� (this-������ �� ������ ������� ��������� ������� "�����������" � ����� UnitActionSystemUI ����� ��� ������������ "������������"                     
        }
    }

    private void SetBusy() // ���������� �������
    {
        _isBusy = true;
        OnBusyChanged?.Invoke(this, new OnUnitSystemEventArgs // ������� ����� ��������� ������ OnUnitSystemEventArgs
        {
            isBusy =_isBusy,
            selectedAction = _selectedAction,
        });
    }

    private void ClearBusy() // �������� ��������� ��� ����� ���������
    {
        _isBusy = false;
        OnBusyChanged?.Invoke(this, new OnUnitSystemEventArgs // ������� ����� ��������� ������ OnUnitSystemEventArgs
        {
            isBusy = _isBusy,
            selectedAction = _selectedAction,
        });
    }

    private bool TryHandleUnitSelection() // ������� ��������� ������ �����
    {
        if (InputManager.Instance.IsMouseButtonDownThisFrame()) // ��� ������� ��� ������ ���� 
        {
            Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition()); // ��� �� ������ � ����� �� ������ ��� ���������� ������ ����
            if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, _unitLayerMask)) // ������ true ���� �� ���-�� �������. �.�. ������� ����� �������������� �� ����������� ����� ������ �� ������
            {   // �������� ���� �� �� ������� � ������� �� ������ ���������  <Unit>
                if (raycastHit.transform.TryGetComponent<Unit>(out Unit unit)) // ������������ TryGetComponent ����� GetComponent � ��� ��� �� ���� ������ ������� ��������. TryGetComponent - ���������� true, ���� ��������� < > ������. ���������� ��������� ���������� ����, ���� �� ����������.
                {
                    if (unit == _selectedUnit) // ������ �������� ��������� �������� �� ���������� ����� ��� ���������� _selectedAction (�������� ������ ���������� ����� �� �������� ������� ���������� �� ���) ���� ��� ������ ������ �� ������ ���������� _selectedAction �� ������ ����� ������� �����.
                    {
                        // ���� ���� ��� ������
                        return false;
                    }

                    if (unit.IsEnemy()) // ���� ��� ����� � ����� 
                    {
                        // ��� ���� ��� �������� �� ����
                        return false;
                    }
                    SetSelectedUnit(unit, unit.GetAction<MoveAction>()); // ������ (����) � ������� ����� ��� ����������� ���������.
                    return true;
                }
            }
        }
        return false; // ���� ������ �� �������
    }

    public void SetSelectedUnit(Unit unit, BaseAction baseAction) // ���������(����������) ���������� �����,� ���������� ������� ��������, � ��������� �������   
    {
        _selectedUnit = unit; // �������� ���������� � ���� ����� ����������� ��������� ������.

        SetSelectedAction(baseAction); // ������� ��������� "MoveAction"  ������ ���������� ����� (�� ��������� ��� ������ ������� ��������� ����� MoveAction). �������� � ���������� _selectedAction ����� ������� SetSelectedAction()
                
        OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty); // "?"- ��������� ��� !=0. Invoke ������� (this-������ �� ������ ������� ��������� ������� "�����������" � ����� UnitSelectedVisual � UnitActionSystemUI ����� ��� ������������ "������������" ��� ����� ��� ����� ������ �� _selectedUnit)
    }

    public void SetSelectedAction(BaseAction baseAction) //���������� ��������� ��������, � ��������� �������  
    {
        _selectedAction = baseAction;

        OnSelectedActionChanged?.Invoke(this, EventArgs.Empty); // "?"- ��������� ��� !=0. Invoke ������� (this-������ �� ������ ������� ��������� ������� "�����������" � ����� UnitActionSystemUI  GridSystemVisual ����� ��� ������������ "������������")
    }
    public BaseAction GetSelectedAction() // ������� ��������� ��������
    {
        return _selectedAction;
    }

    public Unit GetSelectedUnit() // ������� ������������� ����� ������� ����� ���������� ���������� ����� (��� �� �� ������ ���������� ��������� _selectedUnit)
    {
        return _selectedUnit;
    }



}
