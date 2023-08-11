using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class EnemyUnitButonUI : MonoBehaviour // Кнопка врага
{
    
    [SerializeField] private Button _button; // Сама кнопка   
   

    private Unit _enemyUnit;   

   
    public void SetUnit(Unit unit)
    {
        _enemyUnit = unit; 

        // т.к. кнопки создаются динамически то и события настраиваем в скрипте а не в инспекторе
        //Добавим событие при нажатии на нашу кнопку// AddListener() в аргумент должен получить делегат- ссылку на функцию. Функцию будем объявлять АНАНИМНО через лямбду () => {...} 
        _button.onClick.AddListener(() =>
        {
            CameraController.Instance.transform.position = _enemyUnit.transform.position; //Установить Выбранное Действие
        });
    }   

   
}
