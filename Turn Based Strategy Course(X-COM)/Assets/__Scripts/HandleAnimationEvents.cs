using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleAnimationEvents : MonoBehaviour // ���������� ������������ �������
{
    public event EventHandler OnAnimationTossGrenadeEventStarted;     // �������� � �������� "������ �������" ���������� �������  (� ���� ������ ����� ���������� �������) // ��� ������������� ������� ����� AnimationEvent � GrenadyAction � ��� ����� ��������� �������

    [SerializeField] private Transform _healVFXPrefab; // �������� ��� �������
    [SerializeField] private Unit _unit;

    private Unit _targetUnit;
    private HealAction _healAction;

    
    private void Start()
    {
        //Unit unit = GetComponentInParent<Unit>(); // ������� ��������� Unit �� �������� 
        if (_unit != null) // ���� ���� ����������
        {
            _unit.TryGetComponent<HealAction>(out HealAction healAction);// ��������� �� ����� �������� ��������� HealAction � ���� ���������� �������� � healAction
            _healAction = healAction;

            _healAction.OnHealActionStarted += HealAction_OnHealActionStarted; // ���������� �� �������
            //_healAction.OnHealActionCompleted += HealAction_OnHealActionCompleted;

        }
    }



    /* private void HealAction_OnHealActionCompleted(object sender, Unit unit)
     {
         throw new NotImplementedException();
     }*/

    private void HealAction_OnHealActionStarted(object sender, Unit unit)
    {
        _targetUnit = unit;
    }

    private void InstantiateHealVFXPrefab() // ������� � AnimationEvent �� �������� ������� StendUp
    {
        Instantiate(_healVFXPrefab, _targetUnit.GetWorldPosition(), Quaternion.LookRotation(Vector3.up)); // �������� ������ ������ ��� ����� �������� �������� (�� ������ � ���������� �������� � ������ Stop Action - Destroy)
    }

    private void StartIntermediateEvent() // ����� �������������� �������
    {
        OnAnimationTossGrenadeEventStarted?.Invoke(this, EventArgs.Empty); // �������� ������� � �������� "������ �������" ���������� ������� (��������� GrenadyAction)           
    }

    private void OnDestroy()
    {
        if (_healAction != null)
        {
            _healAction.OnHealActionStarted -= HealAction_OnHealActionStarted;// �������� �� ������� ����� �� ���������� ������� � ��������� ��������.
        }
    }


}
