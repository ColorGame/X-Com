using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GridSystemVisual;
// ����� // ������ � ����� ���� Cover ���������� ������ �� ������� ���� 1,4�. (��� ������� ������������������ �� Smoke)
// ���� ������ ���� 1,4� (��� ������ ����� �������� � �������� �����), �� �� ����������� OBSTACLES-������������, �.�. ������ �� ��������������� (�������� ������ �� 0%). 
// CoverFull -1,4� 
// CoverHalf -0,7� 
public class CoverSmokeObject : MonoBehaviour//  ������ ������� ��� ��� 
{
    [SerializeField] private LayerMask _coverLayerMask; // ����� ���� ������� (�������� � ����������)
    [SerializeField] private LayerMask _smokeLayerMask; // ����� ���� ���� (�������� � ����������)
    [SerializeField] private CoverSmokeType _coverSmokeType;
    [SerializeField] private float _penaltyAccuracy;  // ����� ������������ ������� ��������������� ��� �� �� ����� ���� �������� ������� ������
           
    private void Start()
    {
        //������� ��������������� ���������.
        //��������� ���. ��� �� ����� �������� ������ ���������(������ �����), ������� ������� ��� ������

        float raycastOffsetDistance = 1.5f; // ��������� �������� ����
        float raycastDistance = 0.5f; // ��������� �������� ����

        if(Physics.Raycast(transform.position + Vector3.up * raycastOffsetDistance, Vector3.down, raycastDistance, _coverLayerMask)) // ��������� ����� � ������ 1,5 ���� �� 0,5� ������ �� ����� �over 
        {
            // ���� �� ������ �� ���  CoverFull-������� ������
            _coverSmokeType = CoverSmokeType.CoverFull;
        }
        else
        {
            // � ��������� ������ ��� CoverHalf-������� �� ��������
            _coverSmokeType = CoverSmokeType.CoverHalf;
        }

        if (Physics.Raycast(transform.position + Vector3.down * raycastOffsetDistance, Vector3.up, raycastOffsetDistance * 2, _smokeLayerMask)) // ��������� ����� �� ��� ���� �� ����� Smoke 
        {
            // ���� �� ������ �� ���  SmokeFull-��� ������, ��� ������ �� ������ ������ ����� ������ ����� ������ GrenadeSmokeDeactivation
            _coverSmokeType = CoverSmokeType.SmokeFull;
        }

        _penaltyAccuracy = GetPenaltyFromEnumAccuracy(_coverSmokeType); // ��������� ������� ������ � ����������� �� �������������� ����
    }

    public float GetPenaltyFromEnumAccuracy(CoverSmokeType coverSmokeType) // ������� ����� ������������ � ����������� �� ��������� ������� //����� ���������//
    {
        switch (coverSmokeType)
        {
            case CoverSmokeType.None: // 
                _penaltyAccuracy = 0;
                break;

            case CoverSmokeType.CoverHalf: //������� �� ��������
                _penaltyAccuracy = 0.2f;
                break;

            case CoverSmokeType.CoverFull: //������� ������
                _penaltyAccuracy = 0.6f;
                break;

            case CoverSmokeType.SmokeHalf: //��� �� ��������
                _penaltyAccuracy = 0.25f;
                break;

            case CoverSmokeType.SmokeFull: //��� ������
                _penaltyAccuracy = 0.5f;
                break;
        }
        return _penaltyAccuracy;
    }

    public float GetPenaltyAccuracy() // ������� ����� ������������
    {
        return _penaltyAccuracy;
    }    
    public CoverSmokeType GetCoverSmokeType()
    {
        return _coverSmokeType;
    }

    public void SetCoverSmokeType(CoverSmokeType coverType)
    {
        _coverSmokeType = coverType;
    }
}

public enum CoverSmokeType
{
    None,       //����
    CoverHalf,  //������� �� ��������
    CoverFull,  //������� ������
    SmokeHalf,  //��� �� ��������
    SmokeFull   //��� ������
}
