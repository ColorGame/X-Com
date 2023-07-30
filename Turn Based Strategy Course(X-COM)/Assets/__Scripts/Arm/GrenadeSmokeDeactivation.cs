using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeSmokeDeactivation : MonoBehaviour // ����������� ���� �� ������� ����� ���������� �����
{

    private int _startTurnNumber; // ����� ������� (����) ��� ������ 
    private int _currentTurnNumber; // ������� ����� ������� (����) 
    
    private ParticleSystem _particleSystem;
    private CoverObject _coverObject; // ������ �������
    private float _rateOverTime = 50;// ��������, � ������� ���������� ��������� ����� ������� � �������� ������� (�� ��������� 200).

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _coverObject = GetComponent<CoverObject>();
    }

    private void Start()
    {
        _startTurnNumber = TurnSystem.Instance.GetTurnNumber(); // ������� ����� ����
        _coverObject.SetCoverType(CoverType.Full); // ��������� ��������� ������

        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged; // ������. �� ������� ��� �������
    }

    private void TurnSystem_OnTurnChanged(object sender, System.EventArgs e)
    {
        _currentTurnNumber = TurnSystem.Instance.GetTurnNumber(); // ������� ������� ����� ����;

        if (_currentTurnNumber - _startTurnNumber == 4)
        {
            // �� 4 ���� ������������� � ������ ������ �� 50%
            var emission =  _particleSystem.emission;
            emission.rateOverTime = _rateOverTime;
            _coverObject.SetCoverType(CoverType.Half); // ��������� ��������� �� ��������
        }

        if (_currentTurnNumber - _startTurnNumber == 6)
        {
            // �� 6 ���� ��������� ���
            Destroy(gameObject); 
        }
    }

    private void OnDestroy()
    {
        TurnSystem.Instance.OnTurnChanged -= TurnSystem_OnTurnChanged;
    }
}
