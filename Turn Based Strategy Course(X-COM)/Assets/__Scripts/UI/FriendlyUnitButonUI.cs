using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class FriendlyUnitButonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameUnitText; // ��� �����
    [SerializeField] private TextMeshProUGUI _actionPointsText; // ���� ��������
    [SerializeField] private Button _button; // ���� ������
    [SerializeField] private GameObject _selectedButtonVisualUI; // ����� �������� � ����. GameObject ��� �� ������ ��� �������� ����� ������ // � ���������� ���� �������� �����
    [SerializeField] private Image _healthBarImage; // � ���������� �������� ����� �������� "Bar"
    [SerializeField] private Image _backgroundImage; // � ���������� �������� ����� �������� "Bar"
    [SerializeField] private Image _actionImage; // � ���������� �������� ����� �������� "Bar"

    private Unit _unit;
    private Color _nameUnitTextColor;
    private Color _actionPointsTextColor;
    private Color _healthBarImageColor;
    private Color _backgroundImageColor;
    private Color _actionImageColor;

   
    public void SetUnit(Unit unit)
    {
        _unit = unit;
        _nameUnitText.text = _unit.gameObject.name.ToUpper(); // ������� ��� � ��������� �����
        _actionPointsText.text = _unit.GetActionPoints().ToString();
        _healthBarImage.fillAmount = _unit.GetHealthNormalized();

        // �������� ����� ��������� ������
        _nameUnitTextColor = _nameUnitText.color;
        _actionPointsTextColor = _actionPointsText.color;
        _healthBarImageColor = _healthBarImage.color;
        _backgroundImageColor = _backgroundImage.color;
        _actionImageColor = _actionImage.color;

        // �.�. ������ ��������� ����������� �� � ������� ����������� � ������� � �� � ����������
        //������� ������� ��� ������� �� ���� ������// AddListener() � �������� ������ �������� �������- ������ �� �������. ������� ����� ��������� �������� ����� ������ () => {...} 
        _button.onClick.AddListener(() =>
        {
            CameraController.Instance.transform.position = _unit.transform.position; //���������� ��������� ��������
        });
    }

    public void UpdateSelectedVisual() // (���������� �������) ��������� � ���������� ������������ ������.(���������� �������� ��� ������ ������ �������� ��������)
    {
        Unit unit = UnitActionSystem.Instance.GetSelectedUnit(); // ���������� ����
        _selectedButtonVisualUI.SetActive(unit == _unit);   // �������� ����� ���� ��� ��� ���� // ���� �� ��������� �� ������� false � ����� �����������       
    }
    public void UpdateActionPoints()
    {
        _actionPointsText.text = _unit.GetActionPoints().ToString();
    }
    public void UpdateHealthBar()
    {
        _healthBarImage.fillAmount = _unit.GetHealthNormalized();
    }

    //3//{ ������ ������ ������ ������ ����� ����� ���������
    private void InteractableEnable() // �������� ��������������
    {
        _button.interactable = true;
        // ����������� ������������ �����
        _nameUnitText.color = _nameUnitTextColor;
        _actionPointsText.color = _actionPointsTextColor;
        _healthBarImage.color = _healthBarImageColor;
        _backgroundImage.color = _backgroundImageColor;
        _actionImage.color = _actionImageColor;

        UpdateSelectedVisual(); // ������� ����������� ����� ������ � ����������� �� ���������� �����
    }

    private void InteractableDesabled() // ��������� �������������� // ������ ����������� �� �������� � ������ ����(������������� � ���������� color  Desabled)
    {
        _button.interactable = false;

        Color nameUnitTextColor = _nameUnitTextColor; // �������� � ��������� ���������� ���� ������
        Color actionPointsTextColor = _actionPointsTextColor;
        Color healthBarImageColor = _healthBarImageColor;
        Color backgroundImageColor = _backgroundImageColor;
        Color actionImageColor = _actionImageColor;


        nameUnitTextColor.a = 0.1f; // ������� �������� ����� ������
        actionPointsTextColor.a = 0.1f;
        healthBarImageColor.a = 0.1f;
        backgroundImageColor.a = 0.1f;
        actionImageColor.a = 0.1f;

        _nameUnitText.color = nameUnitTextColor; // ������� ������� ���� ����� (���� ����������)
        _actionPointsText.color = actionPointsTextColor;
        _healthBarImage.color = healthBarImageColor;
        _backgroundImage.color = backgroundImageColor;
        _actionImage.color = actionImageColor;

        _selectedButtonVisualUI.SetActive(false); //�������� �����
    }

    public void HandleStateButton(bool isBusy) // ���������� ��������� ������
    {
        if (isBusy) // ���� �����
        {
            InteractableDesabled(); // ��������� ��������������
        }
        else
        {
            InteractableEnable(); // �������� ��������������
        }
    }//3//}
}
