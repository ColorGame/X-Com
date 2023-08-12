using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuAnimator : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _rifleTransformHand_R; //� ���������� �������� ��������� �������� �� ������ ����
    [SerializeField] private Transform _rifleTransformHand_L; //� ���������� �������� ��������� �������� �� ����� ����
    [SerializeField] private Transform _swordTransform; //� ���������� �������� ��������� ����
    [SerializeField] private Transform _binocularsTransform; //� ���������� �������� ��������� �������
    [SerializeField] private Transform _healFXPrefab; // �������� ��� �������
    [SerializeField] private Transform _bulletProjectilePrefab; // ������ ����
    [SerializeField] private Transform _spotterFireFXPrefab; // ����� ����������
    [SerializeField] private Transform _shootPointTransform; // �������������




    public enum State
    {
        Idle,
        SwordAction,
        SwordAfter,
        ShootAction,
        ShootAfter,
        HealAction,
        HealAfter,
        SpotterFireAction,
        SpotterFireAfter
    }

    private State _state; // ��������� �����
    private float _stateTimer; //������ ���������
    private float _Timer = 1f; //������ ���������


    private bool _canShootBullet; // ����� �������� �����    
    private float _timerShoot; //������ ��������
    private int _counterShoot; // ������� ���������
    private float _delayShoot = 0.2f; //�������� ����� ����������
    private int _numberShoot = 10; // ���������� ���������
    private Transform _spotterFireFX;

    private void Start()
    {
        EquipRifleHand_R(); // ������� ��������
        _state = State.Idle;
        _stateTimer = _Timer;
    }


    private void Update()
    {
        _stateTimer -= Time.deltaTime; // �������� ������ ��� ������������ ���������
        _timerShoot -= Time.deltaTime;// �������� ������ ��� ���������� � � ����������

        switch (_state) // ������������� ���������� ���� � ����������� �� _state
        {

            case State.ShootAction:

            if (_canShootBullet && _timerShoot <= 0) // ���� ���� �������� ����� � ������ ����� ...
            {
                Shoot();
                _timerShoot = _delayShoot; // ��������� ������ = �������� ����� ����������
                _counterShoot += 1; // �������� � �������� ��������� 1 
            }

            if (_counterShoot >= _numberShoot) //����� ������� 3 ���� ��� ����� ���� ����
            {
                _canShootBullet = false;
                _counterShoot = 0; //������� ������� ����
            }

            break;
        }

        if (_stateTimer <= 0) // �� ��������� ������� _musicTimer ������� NextMusic() ������� � ���� ������� ���������� ���������. �������� - � ���� ���� TypeGrenade.Aiming: ����� � case TypeGrenade.Aiming: ��������� �� TypeGrenade.Shooting;
        {
            NextState(); //��������� ���������
        }
    }

    private void NextState() //������� ������������ ���������
    {
        switch (_state)
        {
            case State.Idle:
                _state = State.SwordAction;
                _stateTimer =1f;
                EquipSword(); // ���������� ���
                _animator.SetTrigger("SwordSlash");// ���������� ������
                break;

            case State.SwordAction:
                _state = State.SwordAfter;
                _stateTimer = 1;
                EquipRifleHand_R(); // ������� ��������

                break;

            case State.SwordAfter:
                _state = State.ShootAction;                
                
                float shootingStateTime = _numberShoot * _delayShoot + 0.5f; // ��� ��������� ���������� ������ ������ ����������  ����������������� ��������� ������� = ���������� ��������� * ����� ��������
                _stateTimer = shootingStateTime;
                _canShootBullet = true;

                break;

            case State.ShootAction:
                _state = State.ShootAfter;
                _stateTimer = 1;
                break;

            case State.ShootAfter:
                _state = State.HealAction;
                _stateTimer = 3;
                HideAllEquip(); // �������� ����������
                _animator.SetTrigger("Heal");// 
                Instantiate(_healFXPrefab, transform.position, Quaternion.LookRotation(transform.up));
                break;

            case State.HealAction:
                _state = State.HealAfter;
                _stateTimer = 1;
                EquipRifleHand_R(); // ������� ��������
                break;

            case State.HealAfter:                           
                _state = State.SpotterFireAction;
                _stateTimer = 3;
                EquipBinoculars();// ���������� ��������
                _animator.SetBool("IsLooking", true);
                _spotterFireFX = Instantiate(_spotterFireFXPrefab, transform.position +transform.up*1.7f, Quaternion.identity);
                break;

            case State.SpotterFireAction:
                _state = State.SpotterFireAfter;
                _animator.SetBool("IsLooking", false);
                _stateTimer = 1;
                EquipRifleHand_R(); // ������� ��������

                break;

            case State.SpotterFireAfter:
                _state = State.Idle;
                Destroy(_spotterFireFX.gameObject);
                _stateTimer = 0.5f;
                break;
        }
    }

    private void Shoot()
    {
        _animator.SetTrigger("Shoot");
        Transform bulletProjectilePrefabTransform = Instantiate(_bulletProjectilePrefab, _shootPointTransform.position, Quaternion.identity); // �������� ������ ���� � ����� ��������
        BulletProjectile bulletProjectile = bulletProjectilePrefabTransform.GetComponent<BulletProjectile>(); // ������ ��������� BulletProjectile ��������� ����
        Vector3 targetPosition = transform.forward * 20 + Vector3.up*2f; // ������� ������������ �������� �����. 
        bulletProjectile.Setup(targetPosition, true); // � �������� ������� ������� ������������ �������� �����
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

    private void EquipBinoculars()// ���������� ��������
    {
        HideAllEquip(); // �������� ����������
        _binocularsTransform.gameObject.SetActive(true);
    }

    private void HideAllEquip() // ������ ��� ����������
    {
        _swordTransform.gameObject.SetActive(false);
        _rifleTransformHand_R.gameObject.SetActive(false);
        _rifleTransformHand_L.gameObject.SetActive(false);
        _binocularsTransform.gameObject.SetActive(false);
    }
}
