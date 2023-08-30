using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GrenadeProjectile;

public class GrenadeStunAction : GrenadeAction
{
    [SerializeField, Min(0)] private int _grenadeDamage = 0;
    public override void _handleAnimationEvents_OnAnimationTossGrenadeEventStarted(object sender, EventArgs e)  // ������� ����������� ������� - "� �������� "������ �������" ���������� �������"
    {
        if (UnitActionSystem.Instance.GetSelectedAction() == this) // �������� ���� �������� �������� (��������) // ��� ���� ������ ��������� �� ������� � ��������, ���� �� ������� �������� �� ���� ������ ��� ������� ������������
        {
            Transform grenadeProjectileTransform = Instantiate(_grenadeProjectilePrefab, _grenadeSpawnTransform.position, Quaternion.identity); // �������� ������ ������� 
            GrenadeProjectile grenadeProjectile = grenadeProjectileTransform.GetComponent<GrenadeProjectile>(); // ������� � ������� ��������� GrenadeProjectile

            grenadeProjectile.Setup(_targetGridPositin, TypeGrenade.Stun, OnGrenadeBehaviorComplete, _grenadeDamage); // � ������� ������� Setup() ������� � ��� ������� ������� (��������� ������� ������� ����) ��� �������  � ��������� � ������� ������� OnGrenadeBehaviorComplete ( ��� ������ ������� ����� �������� ��� �������)
        }
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition) //�������� �������� ���������� �� // ������������� ����������� ������� �����
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 50, //�������� �������� ��������. ����� ������� ������� ���� ������ ������� ������� �� �����, 
        };
    }
    public override string GetActionName() // ��������� ������� �������� //������� ������������� ������� �������
    {
        return "�������";
    }

    public override string GetToolTip()
    {
        return "���� - " + GetActionPointCost() + "\n" +
            "��������� - " + GetMaxActionDistance() + "\n" +
             "���� - " + GetGrenadeDamage() + "\n" +
            "�������-��� ��������� ���� �������� � ���� ����, ������ �� ������� ��������";
    }

    public override int GetGrenadeDamage()
    {
        return _grenadeDamage;
    }
}
