using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GrenadeProjectile;

public class GrenadeFragmentationAction : GrenadeAction // ���������� ������� 
{

    public override void _handleAnimationEvents_OnAnimationTossGrenadeEventStarted(object sender, EventArgs e)  // ������� ����������� ������� - "� �������� "������ �������" ���������� �������"
    {
        if (UnitActionSystem.Instance.GetSelectedAction() == this) // �������� ���� �������� �������� (��������) // ��� ���� ������ ��������� �� ������� � ��������, ���� �� ������� �������� �� ���� ������ ��� ������� ������������
        {
            Transform grenadeProjectileTransform = Instantiate(_grenadeProjectilePrefab, _grenadeSpawnTransform.position, Quaternion.identity); // �������� ������ ������� 
            GrenadeProjectile grenadeProjectile = grenadeProjectileTransform.GetComponent<GrenadeProjectile>(); // ������� � ������� ��������� GrenadeProjectile
                        
            grenadeProjectile.Setup(_targetGridPositin, TypeGrenade.Fragmentation, OnGrenadeBehaviorComplete); // � ������� ������� Setup() ������� � ��� ������� ������� (��������� ������� ������� ����) ��� ������� � ��������� � ������� ������� OnGrenadeBehaviorComplete ( ��� ������ ������� ����� �������� ��� �������)
        }
    }
    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition) //�������� �������� ���������� �� // ������������� ����������� ������� �����
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 60, //�������� �������� ��������. ����� ������� ������� ���� ������ ������� ������� �� �����, 
        };
    }
    public override string GetActionName() // ��������� ������� �������� //������� ������������� ������� �������
    {
        return "����������";
    }   

    
}
