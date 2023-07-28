using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable // ��������� �������������� (����� ��� ���� ����������������� ��������)
                               // ��������� ������������ ��������� ���, ������� ����� ���������� ��������� ���������� - ����� ������� � ������� ��� ���������� (��� ����������� ������ � BaseAction).
                               // ����� ���� ���������� ��������� ������ � ���������, ������� ��������� ������ ����������.
                               // ��������� ��� �������� � ������������ ������������. ����� class ��� struct ����������� ���� ����������� , ������ ����������� ���� ��������. 
{
    void Interact(Action onInteractionComplete); // ��������������. � �������� ������� ������� �������������� ��������� 

}
//�������, ��� ����� ���� �������������, �������� ����� InteractableBase � ��������� ���� ����� ��� ��������� ��������. ��� �� �������, ��� ������� �� ����,
//����� ����� ��� � ��� ���� ����� ����. ���� � ��� ���� 10 ����������������� ��������, � ��� ��� ���������� ����� ������ ����, ��, ��������, ���� �� ������� ������� ���,
//�� ���� ������ � 2 ���� ����������� ���, ��, ��������, ��� �� ������ ������.��� �� �������, ��� ������� �� ����, ����� ����� ��� � ��� ���� ����� ����.

// ����� � �������� �������� ������������� ������� �������, ��������� ��� ��������, ������ ��� � ������, ��� ��� ����� ������ ������� ����� ��������, ������� � ��������� �� ����� �����.
// ��������� ����� ��� ���� ������ ������, ��������� � ���� ������� � �� ����������� �����, ����� ������ ����������������� ������ ������� ���������.
// � ���� ��� ��� ��� ����� �����-�� ����� ��� ����� ����, �� ��� ����� ������ ������������ ���������, � ������� � C # 8 ������ �� ������ ���������� ��� �� ��������� ������ ����������. 
