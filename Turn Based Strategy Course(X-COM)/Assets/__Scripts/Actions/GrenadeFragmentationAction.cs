using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeFragmentationAction : GrenadeAction // Осколочная граната 
{

    public override void _handleAnimationEvents_OnAnimationTossGrenadeEventStarted(object sender, EventArgs e)  // Зададим абстрактную функцию - "В анимации "Бросок гранаты" стартовало событие"
    {
        if (UnitActionSystem.Instance.GetSelectedAction() == this) // Проверим наше действие активное (выбранно) // Все виды гранат подписаны на событие в АНИМАЦИИ, если не сделать проверку то Юнит создат все гранаты одновременно
        {
            Transform grenadeProjectileTransform = Instantiate(_grenadeProjectilePrefab, _grenadeSpawnTransform.position, Quaternion.identity); // Создадим префаб гранаты 
            GrenadeProjectile grenadeProjectile = grenadeProjectileTransform.GetComponent<GrenadeProjectile>(); // Возьмем у гранаты компонент GrenadeProjectile

            grenadeProjectile.SetTypeGrenade(GrenadeProjectile.TypeGrenade.Fragmentation); // Установим Тип ГРАНАТЫ
            grenadeProjectile.Setup(_targetGridPositin, OnGrenadeBehaviorComplete); // И вызовим функцию Setup() передав в нее целевую позицию (сеточныая позиция курсора мыши) и передадим в делегат функцию OnGrenadeBehaviorComplete ( при взрыве гранаты будем вызывать эту функцию)
        }
    }

    public override string GetActionName() // Присвоить базовое действие //целиком переопределим базовую функцию
    {
        return "GrenadeFragmentation";
    }   

    
}
