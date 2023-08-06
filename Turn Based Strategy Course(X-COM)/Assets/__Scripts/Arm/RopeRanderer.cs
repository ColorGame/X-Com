using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeRanderer : MonoBehaviour
{
   
    private LineRenderer LineRenderer;

    private void Awake()
    {     
        LineRenderer = GetComponentInChildren<LineRenderer>();
    }
    
    public void RopeDraw(Vector3 targetPosition) // �������� ������� �� ��������� ����� ������ � ���� ���� ���������� �����
    {
        LineRenderer.enabled = true;

        //LineRenderer.SetPosition(0, startPosition);
        LineRenderer.SetPosition(1, targetPosition);
    }

}
