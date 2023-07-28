using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimator : MonoBehaviour // �������� ����� � �������� ���� (�����: � ���������� ����� ������� ����� ������ Arms � ��������� ���, ����� � ���� ������� � ����� ��������� ����)
{
    [SerializeField] private Animator _animator;

    [SerializeField] private Transform _bulletProjectilePrefab; // � ���������� �������� ������ ����
    [SerializeField] private Transform _shootPointTransform; // � ���������� �������� ����� �������� ����� �� ��������

    // ���� ������� ������� ������ �� ����� ������� ��������� ������ ������� �������� �� ����� ������
    // �������� �� ������ �������� ������ https://community.gamedev.tv/t/weapon-manager/213840
    [SerializeField] private Transform _rifleTransformHand_R; //� ���������� �������� ��������� �������� �� ������ ����
    [SerializeField] private Transform _rifleTransformHand_L; //� ���������� �������� ��������� �������� �� ����� ����
    [SerializeField] private Transform _swordTransform; //� ���������� �������� ��������� ����

    [SerializeField] private Transform _healVFXPrefab; // �������� ��� �������

    // � ���������� �������� �������� ������ � ����� ��� ������������ ����� �������
    /*private bool _swordActive;
    private bool _rifleActive;*/

    private void Awake()
    {
        if (TryGetComponent<MoveAction>(out MoveAction moveAction)) // ��������� �������� ��������� MoveAction � ���� ���������� �������� � moveAction
        {
            moveAction.OnStartMoving += MoveAction_OnStartMoving; // ���������� �� �������
            moveAction.OnStopMoving += MoveAction_OnStopMoving; // ���������� �� �������
            moveAction.OnChangedFloorsStarted += MoveAction_OnChangedFloorsStarted; // ���������� �� �������
        }

        if (TryGetComponent<ShootAction>(out ShootAction shootAction)) // ��������� �������� ��������� ShootAction � ���� ���������� �������� � shootAction
        {
            shootAction.OnShoot += ShootAction_OnShoot; // ���������� �� �������
        }

        if (TryGetComponent<SwordAction>(out SwordAction swordAction)) // ��������� �������� ��������� SwordAction � ���� ���������� �������� � swordAction
        {
            swordAction.OnSwordActionStarted += SwordAction_OnSwordActionStarted; // ���������� �� �������
            swordAction.OnSwordActionCompleted += SwordAction_OnSwordActionCompleted;
        }
        if (TryGetComponent<HealAction>(out HealAction healAction)) // ��������� �������� ��������� HealAction � ���� ���������� �������� � healAction
        {
            healAction.OnHealActionStarted += HealAction_OnHealActionStarted;// ���������� �� �������
            healAction.OnHealActionCompleted += HealAction_OnHealActionCompleted;
        }

        if (TryGetComponent <GrenadeAction>(out GrenadeAction grenadeAction))// ��������� �������� ��������� GrenadeAction � ���� ���������� �������� � grenadeAction
        {
            grenadeAction.OnGrenadeActionStarted += GrenadeAction_OnGrenadeActionStarted;// ���������� �� �������
            grenadeAction.OnGrenadeActionCompleted += GrenadeAction_OnGrenadeActionCompleted;         
        }
    }

    private void MoveAction_OnChangedFloorsStarted(object sender, MoveAction.OnChangeFloorsStartedEventArgs e)
    {
        if (e.targetGridPosition.floor > e.unitGridPosition.floor) // ���� ������� ������� ���������� ������ ���� ��
        {
            // �������
            _animator.SetTrigger("JumpUp");
        }
        else
        {
            // ������
            _animator.SetTrigger("JumpDown");
        }

    }

    private void Start()
    {
        EquipRifleHand_R(); // ������� ��������
    }


    private void GrenadeAction_OnGrenadeActionCompleted(object sender, EventArgs e)
    {        
        EquipRifleHand_R(); // ������� ��������
    }

    private void GrenadeAction_OnGrenadeActionStarted(object sender, EventArgs e)
    {        
        EquipRifleHand_L(); //��������� �������� �� ����� ����
        _animator.SetTrigger("Grenady");// ���������� ������( ������ ��������� ������)
    }

    private void HealAction_OnHealActionCompleted(object sender, Unit unit)
    {
        EquipRifleHand_R(); // ������� ��������

        //���� �������� � �������� ����� AnimationEvent (HandleAnimationEvents)
        //Instantiate(_healVFXPrefab, unit.GetWorldPosition(), Quaternion.LookRotation(Vector3.up)); // �������� ������ ������ ��� ����� �������� �������� (�� ������ � ���������� �������� � ������ Stop Action - Destroy)
    }

    private void HealAction_OnHealActionStarted(object sender, Unit unit) 
    {
        HideAllEquip(); // �������� ����������
        _animator.SetTrigger("Heal");// ���������� ������( ������ ��������� ������)
    }


    private void SwordAction_OnSwordActionCompleted(object sender, EventArgs e) // ����� �������� ��������� �������� ��� �� ��������
    {
        EquipRifleHand_R(); // ������� ��������
    }

    private void SwordAction_OnSwordActionStarted(object sender, EventArgs e)
    {
        
        EquipSword();   // ������� ���
        _animator.SetTrigger("SwordSlash");// ���������� ������( ������ ��������� ������)
    }

    private void MoveAction_OnStartMoving(object sender, EventArgs empty)
    {
        _animator.SetBool("IsWalking", true); // �������� �������� ������w
    }
    private void MoveAction_OnStopMoving(object sender, EventArgs empty)
    {
        _animator.SetBool("IsWalking", false); // ��������� �������� ������
    }

    private void ShootAction_OnShoot(object sender, ShootAction.OnShootEventArgs e)
    {
        _animator.SetTrigger("Shoot"); // ���������� ������( ������ ��������� ������)

        Transform bulletProjectilePrefabTransform = Instantiate(_bulletProjectilePrefab, _shootPointTransform.position, Quaternion.identity); // �������� ������ ���� � ����� ��������

        BulletProjectile bulletProjectile = bulletProjectilePrefabTransform.GetComponent<BulletProjectile>(); // ������ ��������� BulletProjectile ��������� ����

        Vector3 targetUnitShootAtPosition = e.targetUnit.GetWorldPosition(); // ������� ������� �������� �����. (������� ���� �������� �� �������)

        float unitShoulderHeight = 1.7f; // ������ ����� �����,
        targetUnitShootAtPosition.y += unitShoulderHeight; // � ���������� ���� ����� ��������� � ������ �����
        bulletProjectile.Setup(targetUnitShootAtPosition); // � �������� ������� ������� ������� �������� �����. � �������������� ����������� �� �
    }

    private void EquipSword() // ���������� ���
    {
        HideAllEquip(); // �������� ����������
        _swordTransform.gameObject.SetActive(true);        
    }

    private void EquipRifleHand_R() // ���������� �������� ������ ����
    {
        HideAllEquip(); // �������� ����������
        _rifleTransformHand_R.gameObject.SetActive(true);
    }

    private void EquipRifleHand_L() // ���������� �������� ����� ����
    {
        HideAllEquip(); // �������� ����������
        _rifleTransformHand_L.gameObject.SetActive(true);
    }

    private void HideAllEquip() // ������ ��� ����������
    {
        _swordTransform.gameObject.SetActive(false);
        _rifleTransformHand_R.gameObject.SetActive(false);
        _rifleTransformHand_L.gameObject.SetActive(false);
    }

   /* private void SaveStateEquip() // �������� �������� ������
    {
        _swordActive = _swordTransform.gameObject.activeSelf;
        _rifleActive = _rifleTransformHand_R.gameObject.activeSelf;
    }*/

}
