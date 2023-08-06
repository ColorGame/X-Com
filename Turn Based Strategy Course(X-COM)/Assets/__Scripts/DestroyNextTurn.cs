using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyNextTurn : MonoBehaviour // ����������� �������� ����� ��������� �����
{
    private int _startTurnNumber; // ����� ������� (����) ��� ������ 
    private int _currentTurnNumber; // ������� ����� ������� (����) 

    private void Start()
    {
        _startTurnNumber = TurnSystem.Instance.GetTurnNumber(); // ������� ����� ����

        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged; // ������. �� ������� ��� �������
    }

    private void TurnSystem_OnTurnChanged(object sender, System.EventArgs e)
    {
        _currentTurnNumber = TurnSystem.Instance.GetTurnNumber(); // ������� ������� ����� ����;

        if (_currentTurnNumber - _startTurnNumber == 2)
        {
            // ����� 2 ���� ��������� ���
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        TurnSystem.Instance.OnTurnChanged -= TurnSystem_OnTurnChanged;
    }
}
