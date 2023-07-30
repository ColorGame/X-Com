using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HealthSystem : MonoBehaviour // ������� �������� // ����� �� �����// ����� �������� �� ����������� ������� � ����� �������� ����, ����� ...
{
    public event EventHandler OnDead; // ���� (�������� ������� ����� ���� ����)
    public event EventHandler OnDamageAndHealing; // ������� ����������� ��� ��������� (������ ������� ��� ��������� ����� ��� ������� )

    [SerializeField] private int _health = 100; // ������� ��������
    private int _healthMax;

    private void Awake()
    {
        _healthMax = _health;
    }

    public void Damage(int damageAmount) // ���� (� �������� �������� �������� �����)
    {
        _health-=damageAmount;

        if (_health < 0)
        {
            _health = 0;
        }

        OnDamageAndHealing?.Invoke(this, EventArgs.Empty); // ������� ������� ��� ��������� �����

        if(_health == 0)
        {
            Die();
        }

        //Debug.Log(_health);
    }

    public void Healing(int healingAmount) // ��������� (� �������� �������� �������� ����������������� ��������)
    {
        _health += healingAmount;

        if (_health > _healthMax)
        {
            _health = _healthMax;
        }

        OnDamageAndHealing?.Invoke(this, EventArgs.Empty); // ������� ������� ��� �������������� ��������               
    }


    private void Die() // �������� ������� ��� ���������� ����� ������ � �� ����� ����� �������� ��������� ����� ��������
    {
        OnDead?.Invoke(this, EventArgs.Empty); // �������� ������� ����� ���� ���� (�� ��� ����� ������������� Unit, UnitRagdollSpawner)        
    }

    public float GetHealthNormalized() // ������ ��������������� �������� �������� (�� 0 �� 1) ��� ��������� �������� ����� ��������
    {
        return (float)_health/ _healthMax; // ����������� �� int � float. ����� ���� 10/100 ������ 0
    }

    public int GetHealth()
    {
        return _health;
    }
    
    public int GetHealthMax()
    {
        return _healthMax;
    }
    
    public bool IsDead()
    {
        return _health == 0;
    }
}
