using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitComboSystem : MonoBehaviour // �������� � ��������� ������� �������� �� ������ ������������ � �����
{
  //  public static UnitComboSystem Instance { get; private set; }   //(������� SINGLETON) ��� �������� ������� ����� ���� ������� (SET-���������) ������ ���� �������, �� ����� ���� �������� GET ����� ������ �������
                                                                   // instance - ���������, � ��� ����� ���� ��������� UnitComboSystem ����� ���� ��� static. Instance ����� ��� ���� ����� ������ ������, ����� ����, ����� ����������� �� Event.


    public event EventHandler OnStartComboAction; // �������� ����� ��������.


    private Unit _startComboUnit; // ���� ����� ��������������� ����� ��������
    private Unit _targetComboUnit; // ���� � ������� ����� ������ �����    

    private void Awake()
    {
       /* // ���� �� �������� � ���������� �� �������� �� �����
        if (Instance != null) // ������� �������� ��� ���� ������ ���������� � ��������� ����������
        {
            Debug.LogError("There's more than one UnitComboSystem!(��� ������, ��� ���� UnitComboSystem!) " + transform + " - " + Instance);
            Destroy(gameObject); // ��������� ���� ��������
            return; // �.�. � ��� ��� ���� ��������� UnitComboSystem ��������� ����������, ��� �� �� ��������� ������ ����
        }
        Instance = this; */      
    }

    private void Start()
    {
       // ComboAction.OnAnyComboActionStarted += ComboAction_OnAnyComboActionStarted; // � ������ �������� ����� �������� 
    }

    /*private void ComboAction_OnAnyComboActionStarted(object sender, ComboAction.OnComboEventArgs e)
    {
        _startComboUnit = e.startUnit;
        _targetComboUnit = e.partnerUnit;
                
        OnStartComboAction?.Invoke(this, e);
    }*/
}
