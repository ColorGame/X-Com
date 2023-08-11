using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitWorldUI : MonoBehaviour // ������� ���������������� ��������� ����� //����� � canvas �� �����
{
    [SerializeField] private TextMeshProUGUI _actionPointsText; // �������� ���� UI
    [SerializeField] private TextMeshProUGUI _hitPercentText; // �������� ������� ���������
    [SerializeField] private TextMeshProUGUI _healthPointsText; // �������� ����� ��������
    [SerializeField] private Image _aimImage; // �������� ������ �������
    [SerializeField] private Image _stunnedImage; // �������� ������ ���������
    [SerializeField] private Image _healthBarImage; // � ���������� �������� ����� �������� "Bar"
    [SerializeField] private Unit _unit; // � ���������� �������� �����
    [SerializeField] private HealthSystem _healthSystem; // �������� ������ ����� �� ������ ����� �� ���


    private void Start()
    {
        Unit.OnAnyActionPointsChanged += Unit_OnAnyActionPointsChanged; // ���������� �� ����������� ������� (����� ��������� ����� ��������) // ��������� ���������� - ��� ������� ���������� ����� ���������� ActionPoints � ������ �����, � ��� ������� ������������� �� �������������
        _healthSystem.OnDamageAndHealing += HealthSystem_OnDamageAndHealing; // ���������� �� ������� ������� ����������� ��� ���������.
        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged; // ���������� ��������� �������� ��������
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged; //���������� ��� �������

        UpdateActionPointsText();
        UpdateHealthBar();
        HideHitPercent();
        UpdateStunnedState();
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        UpdateHitPercentText(); // ���� �������� ������ ������� ��������� �.�. � ���� ���� ������� ����� �������� ���� ���������
    }

    private void UnitActionSystem_OnSelectedActionChanged(object sender, EventArgs e) // ���������� ��������� �������� ��������
    {
        // ����������, ����� �� ���������� ������� ��������� � ��������
        HideHitPercent();
        UpdateHitPercentText();
    }

    private void UpdateHitPercentText() // ���������� ������ ������� ���������
    {
        BaseAction baseAction = UnitActionSystem.Instance.GetSelectedAction();

        switch (baseAction)
        {
            case ShootAction shootAction:
                // �������� �������� �������

                Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();

                if (_unit.IsEnemy() != selectedUnit.IsEnemy())
                {
                    // ���� ���� � �������� � ������ ������

                    if (shootAction.IsValidActionGridPosition(_unit.GetGridPosition())) // ���� ���� ���� ������ � ������ ������ �� ������� � ���� ��������
                    {
                        // ��� ���� ������� ��� �� ���������� ������� ��� ��������
                        ShowHitPercent(shootAction.GetHitPercent(_unit));
                    }
                }
                break;
        }
    }
    private void UpdateActionPointsText() // ���������� ������ ����� ��������
    {
        _actionPointsText.text = _unit.GetActionPoints().ToString(); // ������ ���� �������� ����� ����������� � ������ � ��������� � ����� ������� ������������ ��� ������
    }

    private void Unit_OnAnyActionPointsChanged(object sender, EventArgs e)
    {
        UpdateActionPointsText();
        UpdateStunnedState();
    }

    /*//  ���� �� ������ ����� �����, ����� ���� ��������� ��������� � OnAnyActionPointsChanged, ��� ������ ����� ������� ����������� ��� �����.
    private void Unit_OnAnyActionPointsChanged(object sender, EventArgs args)
    {
        Unit unit = sender as Unit;
        Debug.Log($"� {unit} ���� �������� ����������.");
    }*/
    private void UpdateHealthBar() // ���������� ����� ��������
    {
        _healthBarImage.fillAmount = _healthSystem.GetHealthNormalized();
        _healthPointsText.text = _healthSystem.GetHealth().ToString();

    }
    private void HealthSystem_OnDamageAndHealing(object sender, EventArgs e) // ��� ����������� ������� ������� ����� �����
    {
        UpdateHealthBar();
    }

    private void ShowHitPercent(float hitChance)
    {
        _hitPercentText.gameObject.SetActive(true);
        _hitPercentText.text = Mathf.Round(hitChance * 100f) + "%";
        _aimImage.gameObject.SetActive(true);
    }

    private void HideHitPercent()
    {
        _hitPercentText.gameObject.SetActive(false);
        _aimImage.gameObject.SetActive(false);
    }

    private void UpdateStunnedState()
    {
        _stunnedImage.gameObject.SetActive(_unit.GetStunned());
    }
        

    private void OnDestroy()
    {
        UnitActionSystem.Instance.OnSelectedActionChanged -= UnitActionSystem_OnSelectedActionChanged;
        Unit.OnAnyActionPointsChanged -= Unit_OnAnyActionPointsChanged;
    }
}
