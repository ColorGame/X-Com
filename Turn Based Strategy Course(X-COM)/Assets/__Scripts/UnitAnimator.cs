using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimator : MonoBehaviour // Анимация юнита и создание пули (ПЛАНЫ: в дальнейшем можно создать класс оружие Arms и расширять его, тогда в этих классах и будем создавать пули)
{
    [SerializeField] private Animator _animator;

    [SerializeField] private Transform _bulletProjectilePrefab; // в инспекторе закинуть префаб пули
    [SerializeField] private Transform _shootPointTransform; // в инспекторе закинуть точку выстрела лежит на автомате

    // Если сложная система оружий то лучше создать отдельный скрипт который отвечает за смену оружия
    // Менеджер по оружию описание логики https://community.gamedev.tv/t/weapon-manager/213840
    [SerializeField] private Transform _rifleTransformHand_R; //в инспекторе закинуть Трансформ Винтовки на ПРАВОЙ РУКЕ
    [SerializeField] private Transform _rifleTransformHand_L; //в инспекторе закинуть Трансформ Винтовки на ЛЕВОЙ РУКЕ
    [SerializeField] private Transform _swordTransform; //в инспекторе закинуть Трансформ Мечя

    [SerializeField] private Transform _healVFXPrefab; // Свечение при лечении

    // В дальнейшем сохраним активное оружие и будем его активировать после лечения
    /*private bool _swordActive;
    private bool _rifleActive;*/

    private void Awake()
    {
        if (TryGetComponent<MoveAction>(out MoveAction moveAction)) // Попробуем получить компонент MoveAction и если получиться сохраним в moveAction
        {
            moveAction.OnStartMoving += MoveAction_OnStartMoving; // Подпишемся на событие
            moveAction.OnStopMoving += MoveAction_OnStopMoving; // Подпишемся на событие
            moveAction.OnChangedFloorsStarted += MoveAction_OnChangedFloorsStarted; // Подпишемся на событие
        }

        if (TryGetComponent<ShootAction>(out ShootAction shootAction)) // Попробуем получить компонент ShootAction и если получиться сохраним в shootAction
        {
            shootAction.OnShoot += ShootAction_OnShoot; // Подпишемся на событие
        }

        if (TryGetComponent<SwordAction>(out SwordAction swordAction)) // Попробуем получить компонент SwordAction и если получиться сохраним в swordAction
        {
            swordAction.OnSwordActionStarted += SwordAction_OnSwordActionStarted; // Подпишемся на событие
            swordAction.OnSwordActionCompleted += SwordAction_OnSwordActionCompleted;
        }
        if (TryGetComponent<HealAction>(out HealAction healAction)) // Попробуем получить компонент HealAction и если получиться сохраним в healAction
        {
            healAction.OnHealActionStarted += HealAction_OnHealActionStarted;// Подпишемся на событие
            healAction.OnHealActionCompleted += HealAction_OnHealActionCompleted;
        }

        if (TryGetComponent <GrenadeAction>(out GrenadeAction grenadeAction))// Попробуем получить компонент GrenadeAction и если получиться сохраним в grenadeAction
        {
            grenadeAction.OnGrenadeActionStarted += GrenadeAction_OnGrenadeActionStarted;// Подпишемся на событие
            grenadeAction.OnGrenadeActionCompleted += GrenadeAction_OnGrenadeActionCompleted;         
        }
    }

    private void MoveAction_OnChangedFloorsStarted(object sender, MoveAction.OnChangeFloorsStartedEventArgs e)
    {
        if (e.targetGridPosition.floor > e.unitGridPosition.floor) // Если целевая позиция находиться этажом выше то
        {
            // Прыгать
            _animator.SetTrigger("JumpUp");
        }
        else
        {
            // Падать
            _animator.SetTrigger("JumpDown");
        }

    }

    private void Start()
    {
        EquipRifleHand_R(); // Включим винтовку
    }


    private void GrenadeAction_OnGrenadeActionCompleted(object sender, EventArgs e)
    {        
        EquipRifleHand_R(); // Включим винтовку
    }

    private void GrenadeAction_OnGrenadeActionStarted(object sender, EventArgs e)
    {        
        EquipRifleHand_L(); //Установим винтовку на ЛЕВУЮ руку
        _animator.SetTrigger("Grenady");// Установить тригер( нажать спусковой крючок)
    }

    private void HealAction_OnHealActionCompleted(object sender, Unit unit)
    {
        EquipRifleHand_R(); // Включим винтовку

        //Буду создавть в Анимации через AnimationEvent (HandleAnimationEvents)
        //Instantiate(_healVFXPrefab, unit.GetWorldPosition(), Quaternion.LookRotation(Vector3.up)); // Создадим префаб частиц для юнита которого исцеляем (Не забудь в инспекторе включить у частиц Stop Action - Destroy)
    }

    private void HealAction_OnHealActionStarted(object sender, Unit unit) 
    {
        HideAllEquip(); // выключим экипировку
        _animator.SetTrigger("Heal");// Установить тригер( нажать спусковой крючок)
    }


    private void SwordAction_OnSwordActionCompleted(object sender, EventArgs e) // Когда действие завершено поменяем меч на винтовку
    {
        EquipRifleHand_R(); // Включим винтовку
    }

    private void SwordAction_OnSwordActionStarted(object sender, EventArgs e)
    {
        
        EquipSword();   // Включим МЕЧ
        _animator.SetTrigger("SwordSlash");// Установить тригер( нажать спусковой крючок)
    }

    private void MoveAction_OnStartMoving(object sender, EventArgs empty)
    {
        _animator.SetBool("IsWalking", true); // Включить анимацию Хотьбыw
    }
    private void MoveAction_OnStopMoving(object sender, EventArgs empty)
    {
        _animator.SetBool("IsWalking", false); // Выключить анимацию Хотьбы
    }

    private void ShootAction_OnShoot(object sender, ShootAction.OnShootEventArgs e)
    {
        _animator.SetTrigger("Shoot"); // Установить тригер( нажать спусковой крючок)

        Transform bulletProjectilePrefabTransform = Instantiate(_bulletProjectilePrefab, _shootPointTransform.position, Quaternion.identity); // Создадим префаб пули в точке выстрела

        BulletProjectile bulletProjectile = bulletProjectilePrefabTransform.GetComponent<BulletProjectile>(); // Вернем компонент BulletProjectile созданной пули

        Vector3 targetUnitShootAtPosition = e.targetUnit.GetWorldPosition(); // Мировая позиция целевого юнита. (Целевой Юнит Стрельба По Позиции)

        float unitShoulderHeight = 1.7f; // Высота плеча юнита,
        targetUnitShootAtPosition.y += unitShoulderHeight; // В результате ПУЛЯ будет выпущенна в голову врага
        bulletProjectile.Setup(targetUnitShootAtPosition); // В аргумент предали Мировую позицию целевого юнита. с преобразовоной координатой по У
    }

    private void EquipSword() // Экипировка меч
    {
        HideAllEquip(); // выключим экипировку
        _swordTransform.gameObject.SetActive(true);        
    }

    private void EquipRifleHand_R() // Экипировка винтовка Правой Руки
    {
        HideAllEquip(); // выключим экипировку
        _rifleTransformHand_R.gameObject.SetActive(true);
    }

    private void EquipRifleHand_L() // Экипировка винтовка Левой Руки
    {
        HideAllEquip(); // выключим экипировку
        _rifleTransformHand_L.gameObject.SetActive(true);
    }

    private void HideAllEquip() // Скрыть всю экипировку
    {
        _swordTransform.gameObject.SetActive(false);
        _rifleTransformHand_R.gameObject.SetActive(false);
        _rifleTransformHand_L.gameObject.SetActive(false);
    }

   /* private void SaveStateEquip() // Сохраним активное оружие
    {
        _swordActive = _swordTransform.gameObject.activeSelf;
        _rifleActive = _rifleTransformHand_R.gameObject.activeSelf;
    }*/

}
