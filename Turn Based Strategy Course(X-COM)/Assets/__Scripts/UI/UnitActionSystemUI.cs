using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // ��� ������ � ���������������� �����������
using static UnitActionSystem;

public class UnitActionSystemUI : MonoBehaviour // ������� �������� UI ����� // ����������� ��������� ������ ��� ������ ����� // ����� � Canvas
{

    [SerializeField] private Transform _actionButtonPrefab; // � ���������� ������� ������ ������
    [SerializeField] private Transform _actionButtonContainerTransform; // � ���������� ���������  ��������� ��� ������( ���������� � ����� � Canvas)
    [SerializeField] private Transform _friendlyUnitButonPrefab; // � ���������� ������� ������ ������
    [SerializeField] private Transform _friendlyUnitButonContainerTransform; // � ���������� ���������  ��������� ��� ������( ���������� � ����� � Canvas)
    [SerializeField] private TextMeshProUGUI _actionPointsText; // ������ �� ����� �����
    [SerializeField] private Image _actionPointImage; // �������� ������

    private List<ActionButtonUI> _actionButtonUIList; // ������ ������ ��������
    private List<FriendlyUnitButonUI> _friendlyUnitButonUIList; // ������ ������ ������
    
    private void Awake()
    {
        _actionButtonUIList = new List<ActionButtonUI>(); // �������� ��������� ������
        _friendlyUnitButonUIList = new List<FriendlyUnitButonUI>();
    }

    private void Start()
    {
        UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged; //��������� ���� �������// ������������� �� Event �� UnitActionSystem (���������� �����������). ���������� ��� �� ��������� ������� UnitActionSystem_OnSelectedUnitChanged()
        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged; //��������� �������� ��������// ������������� �� Event ����� ����������� ������ ��� ����� �� ������ ������� �������� // 
        UnitActionSystem.Instance.OnActionStarted += UnitActionSystem_OnActionStarted; // �������� ������// ������������� �� Event// ����� ����������� ������ ��� ��� ������ ��������. //
        UnitManager.OnAnyUnitDeadAndRemoveList += UnitManager_OnAnyUnitDeadAndRemoveList;// ������� ����� ���� ���� � ������ �� ������
        Unit.OnAnyFriendlyUnitDamage += Unit_OnAnyFriendlyUnitDamage; //����� ������������� ���� ������� ����
        Unit.OnAnyFriendlyUnitHealing += Unit_OnAnyFriendlyUnitHealing;//����� ������������� ���� ������� ���������
        //2//3//{ ��� ��������� �������� ������ ������ ����� ����� ���������
        UnitActionSystem.Instance.OnBusyChanged += UnitActionSystem_OnBusyChanged; // ��������� �������� ������������ �� Event � �������� UnitActionSystem_OnBusyChanged, ��� ������� ������� �� ������� ������� �������� //
        //2//3//}
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged; // ������� ����� ���� ������������� �� Event // ����� ����������� (��������� ������ ����� ��������).
        // ������� 2 //{
        Unit.OnAnyActionPointsChanged += Unit_OnAnyActionPointsChanged; //��������� ����� �������� � ������(Any) ������������������ �� ����������� Event // ������ ����������� ������ ��� ��� ��������� ����� �������� � ������(Any) ����� � �� ������ � ����������.
        // ������� 2 //}             

        CreateUnitActionButtons();
        CreateFriendlyUnitButtons();
        UpdateSelectedVisual();
        UpdateActionPoints();
    }


    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        UpdateButtonVisibility();
    }
    private void Unit_OnAnyFriendlyUnitHealing(object sender, EventArgs e)
    {
        foreach (FriendlyUnitButonUI friendlyUnitButonUI in _friendlyUnitButonUIList)
        {
            friendlyUnitButonUI.UpdateHealthBar();
        }
    }

    private void Unit_OnAnyFriendlyUnitDamage(object sender, EventArgs e)
    {
        foreach (FriendlyUnitButonUI friendlyUnitButonUI in _friendlyUnitButonUIList)
        {
            friendlyUnitButonUI.UpdateHealthBar();
        }
    }

    private void UpdateButtonVisibility() // ���������� ������������ ������ � ����������� �� ���� ��� ��� (������� �� ����� �����)
    {
        bool isBusy = !TurnSystem.Instance.IsPlayerTurn(); // ������ ����� ����� ���� (�� �)

        foreach (ActionButtonUI actionButtonUI in _actionButtonUIList) // � ����� ���������� ��������� ������
        {
            actionButtonUI.HandleStateButton(isBusy);
        }
        foreach (FriendlyUnitButonUI friendlyUnitButonUI in _friendlyUnitButonUIList)
        {
            friendlyUnitButonUI.HandleStateButton(isBusy);
        }
        _actionPointsText.gameObject.SetActive(TurnSystem.Instance.IsPlayerTurn()); // ���������� ������ �� ����� ����� ����
        _actionPointImage.gameObject.SetActive(TurnSystem.Instance.IsPlayerTurn());
    }

    private void UnitManager_OnAnyUnitDeadAndRemoveList(object sender, EventArgs e)
    {
        CreateFriendlyUnitButtons();
    }

    /*//2//{ ������ ������ ������ ������ ����� ����� ���������
    private void Show() // ��������
    {
        gameObject.SetActive(true);
    }

    private void Hide() // ������
    {
        gameObject.SetActive(false);
    }

    private void UnitActionSystem_OnBusyChanged(object sender, bool isBusy)
    {
        if (isBusy) // ���� ����� �� ������ ���� ��� �� �������� ������
        {
            Hide();
        }
        else
        {
            Show();
        }
    } //2//}*/

    //3//{ ������ ������ ������ ������ ����� ����� ���������
    private void UnitActionSystem_OnBusyChanged(object sender, OnUnitSystemEventArgs e) 
    {   
        if (e.selectedAction is ComboAction) // ���� ����������� ����� ������� ��������
        {
            ComboAction comboAction = (ComboAction)e.selectedAction;
            ComboAction.State state = comboAction.GetState();

            switch (state)
            {
                case ComboAction.State.ComboSearchEnemy: // ���� ��� ����� �� ������ ������ ���� ��������
                case ComboAction.State.ComboStart:
                    return; // ������� � ���������� ��� ����
            }            
        }

        foreach (ActionButtonUI actionButtonUI in _actionButtonUIList) // � ����� ���������� ��������� ������
        {
            actionButtonUI.HandleStateButton(e.isBusy);
        }
        foreach (FriendlyUnitButonUI friendlyUnitButonUI in _friendlyUnitButonUIList)
        {
            friendlyUnitButonUI.HandleStateButton(e.isBusy);
        }
    } //3//}


    private void CreateUnitActionButtons() // ������� ������ ��� �������� ����� 
    {
        foreach (Transform buttonTransform in _actionButtonContainerTransform) // ������� ��������� � ��������
        {
            Destroy(buttonTransform.gameObject); // ������ ������� ������ ������������� � Transform
        }

        _actionButtonUIList.Clear(); // ������� ����� ������

        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit(); // ������� ���������� �����

        foreach (BaseAction baseAction in selectedUnit.GetBaseActionsArray()) // � ����� ��������� ������ ������� �������� � ���������� �����
        {
            Transform actionButtonTransform = Instantiate(_actionButtonPrefab, _actionButtonContainerTransform); // ��� ������� baseAction �������� ������ ������ � �������� �������� - ��������� ��� ������
            ActionButtonUI actionButtonUI = actionButtonTransform.GetComponent<ActionButtonUI>(); // � ������ ������ ��������� ActionButtonUI
            actionButtonUI.SetBaseAction(baseAction); //������� � ��������� ������� �������� (����� ������)

            MouseEnterExitEvents mouseEnterExitEvents = actionButtonTransform.GetComponent<MouseEnterExitEvents>(); // ������ �� ������ ��������� - ������� ����� � ������ ����� 
            mouseEnterExitEvents.OnMouseEnter += (object sender, EventArgs e) => // ���������� �� ������� - ��� ����� ���� �� ������. ������� ����� ��������� �������� ����� ������ () => {...} 
            {
                TooltipUI.Instance.Show(baseAction.GetToolTip()); // ��� ��������� �� ������ ������� ��������� � ��������� �����
            };
            mouseEnterExitEvents.OnMouseExit += (object sender, EventArgs e) => // ���������� �� ������� - ��� ������ ���� �� ������.
            {
                TooltipUI.Instance.Hide(); // ��� ��������� ���� ������ ���������
            };

            _actionButtonUIList.Add(actionButtonUI); // ������� � ������ ���������� ��������� ActionButtonUI
        }
    }

    private void CreateFriendlyUnitButtons() // ������� ������ ��� �������������� ������
    {
        foreach (Transform buttonTransform in _friendlyUnitButonContainerTransform) // ������� ��������� � ��������
        {
            Destroy(buttonTransform.gameObject); // ������ ������� ������ ������������� � Transform
        }

        _friendlyUnitButonUIList.Clear(); // ������� ����� ������

        foreach (Unit unit in UnitManager.Instance.GetFriendlyUnitList())// ��������� ������������� ������
        {
            Transform actionButtonTransform = Instantiate(_friendlyUnitButonPrefab, _friendlyUnitButonContainerTransform); // ��� ������� ����� �������� ������ ������ � �������� �������� - ��������� ��� ������
            FriendlyUnitButonUI friendlyUnitButonUI = actionButtonTransform.GetComponent<FriendlyUnitButonUI>();// � ������ ������ ��������� FriendlyUnitButonUI
            friendlyUnitButonUI.SetUnit(unit);//������� � ���������

            _friendlyUnitButonUIList.Add(friendlyUnitButonUI);// ������� � ������ ���� ������
        }
    }
    private void UnitActionSystem_OnSelectedUnitChanged(object sender, EventArgs empty) //sender - ����������� // �������� ������ ����� ���� ��������� ��� � ������� ����������� OnSelectedUnitChanged
    {
        CreateUnitActionButtons(); // ������� ������ ��� �������� ����� 
        UpdateSelectedVisual();
        UpdateActionPoints();
    }

    private void UnitActionSystem_OnSelectedActionChanged(object sender, EventArgs empty)
    {
        UpdateSelectedVisual();
    }

    private void UnitActionSystem_OnActionStarted(object sender, EventArgs empty) // �������� �������� - ��� �������� ��� ���� ��� ��������� � ���� �� ��������
    {
        UpdateActionPoints();
    }
    private void UpdateSelectedVisual() //���������� ������������ ������( ��� ������ ������ ������� �����)
    {
        foreach (ActionButtonUI actionButtonUI in _actionButtonUIList)
        {
            actionButtonUI.UpdateSelectedVisual();
        }
        foreach (FriendlyUnitButonUI friendlyUnitButonUI in _friendlyUnitButonUIList)
        {
            friendlyUnitButonUI.UpdateSelectedVisual();
        }
    }

    private void UpdateActionPoints() // ���������� ����� �������� (��� �������� ��������)
    {
        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();// ������� ���������� �����

        _actionPointsText.text =" "+ selectedUnit.GetActionPoints(); //������� ����� ������� � ���� ���������� �����
    }

    private void UpdateActionPointsFriendlyUnitButon() // ���������� ����� �������� 
    {
        foreach (FriendlyUnitButonUI friendlyUnitButonUI in _friendlyUnitButonUIList)
        {
            friendlyUnitButonUI.UpdateActionPoints();
        }
    }
    // �������� // ����� ���������� ������. ����� ����� � ������ Unit � ���������� ������ ����� � ���� ������, ������� ���� � ���� �������. ��� ����������� ����� ��� ������ ����������, ����� ����� ���������� ������ � ���������� ��� �� ���������� ���� �������� "0" � �� ����� �� "2".
    // ������� 1 //- �������� ������� ���������� ������� UnitActionSystemUI , ������� � Project Settings/ Script Execution Order � �������� ���� Deafault Time � �����
    /* private void TurnSystem_OnTurnChanged(object sender, EventArgs empty) // ����� ���� ������� - ��� �������� ��� ���� �������� ��������������, ������� ��.
     {
         UpdateActionPoints();

         // ����� ����� ����������� ���������� ������ �� ����� ���� �����
     }*/

    // ������� 2 //{
    private void Unit_OnAnyActionPointsChanged(object sender, EventArgs empty) //��������� ��������� ����� �������� � ������(Any) ����� � �� ������ � ����������. ������� ��.
    {
        UpdateActionPoints();
        UpdateActionPointsFriendlyUnitButon();
    }// ������� 2 //}
}
