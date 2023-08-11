using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class EnemyUnitButonUI : MonoBehaviour // ������ �����
{
    
    [SerializeField] private Button _button; // ���� ������   
   

    private Unit _enemyUnit;   

   
    public void SetUnit(Unit unit)
    {
        _enemyUnit = unit; 

        // �.�. ������ ��������� ����������� �� � ������� ����������� � ������� � �� � ����������
        //������� ������� ��� ������� �� ���� ������// AddListener() � �������� ������ �������� �������- ������ �� �������. ������� ����� ��������� �������� ����� ������ () => {...} 
        _button.onClick.AddListener(() =>
        {
            CameraController.Instance.transform.position = _enemyUnit.transform.position; //���������� ��������� ��������
        });
    }   

   
}
