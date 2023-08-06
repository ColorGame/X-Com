using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyNextTurn : MonoBehaviour // ДЕактивация объектов через несколько ходов
{
    private int _startTurnNumber; // Номер очереди (хода) при старте 
    private int _currentTurnNumber; // Текущий номер очереди (хода) 

    private void Start()
    {
        _startTurnNumber = TurnSystem.Instance.GetTurnNumber(); // Получим номер хода

        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged; // Подпиш. на событие Ход Изменен
    }

    private void TurnSystem_OnTurnChanged(object sender, System.EventArgs e)
    {
        _currentTurnNumber = TurnSystem.Instance.GetTurnNumber(); // Получим ТЕКУЩИЙ номер хода;

        if (_currentTurnNumber - _startTurnNumber == 2)
        {
            // через 2 хода уничтожим дым
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        TurnSystem.Instance.OnTurnChanged -= TurnSystem_OnTurnChanged;
    }
}
