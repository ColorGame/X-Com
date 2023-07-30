using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeFragmentationAction : GrenadeAction // ���������� ������� 
{

    public override void _handleAnimationEvents_OnAnimationTossGrenadeEventStarted(object sender, EventArgs e)  // ������� ����������� ������� - "� �������� "������ �������" ���������� �������"
    {
        if (UnitActionSystem.Instance.GetSelectedAction() == this) // �������� ���� �������� �������� (��������) // ��� ���� ������ ��������� �� ������� � ��������, ���� �� ������� �������� �� ���� ������ ��� ������� ������������
        {
            Transform grenadeProjectileTransform = Instantiate(_grenadeProjectilePrefab, _grenadeSpawnTransform.position, Quaternion.identity); // �������� ������ ������� 
            GrenadeProjectile grenadeProjectile = grenadeProjectileTransform.GetComponent<GrenadeProjectile>(); // ������� � ������� ��������� GrenadeProjectile

            grenadeProjectile.SetTypeGrenade(GrenadeProjectile.TypeGrenade.Fragmentation); // ��������� ��� �������
            grenadeProjectile.Setup(_targetGridPositin, OnGrenadeBehaviorComplete); // � ������� ������� Setup() ������� � ��� ������� ������� (��������� ������� ������� ����) � ��������� � ������� ������� OnGrenadeBehaviorComplete ( ��� ������ ������� ����� �������� ��� �������)
        }
    }

    public override string GetActionName() // ��������� ������� �������� //������� ������������� ������� �������
    {
        return "GrenadeFragmentation";
    }   

    
}
