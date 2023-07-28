//#define HEX_GRID_SYSTEM //������������ �������� ������� //  � C# ��������� ��� �������� �������������, ����������� ������� �� ������������� ��������� ���� ��������� ������������. 
//��� ��������� ���������� ������� ������������� ������ ��������� ����� �� ����������� � ��������� ��� � ��� �������� �����, ��� ��� ����������. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystemVisualSingle : MonoBehaviour //�������� ������� ������������ ������� ����� (������) // ����� �� ����� ������� (����� �����)
{
    [SerializeField] private MeshRenderer _meshRendererQuad; // ����� �������� � ����. MeshRenderer ��� �� ������ ��� �������� ��� ���������� ������

    [SerializeField] private MeshRenderer _meshRendererHex; // ����� �������� � ����. MeshRenderer ��� �� ������ ��� �������� ��� ���������� ������

    [SerializeField] private MeshRenderer _meshRenderer�ircle; // ����� �������� � ����. MeshRenderer ��� �� ������ ��� �������� ��� ���������� ������

    // ��� ������� ������������ ����� (����������� ������ ��� ������)
    //[SerializeField] private GameObject _SelectedGameObject; // ��� ������� ������������ �����

#if HEX_GRID_SYSTEM // ���� ������������ �������� �������

    private void Start()
    {
       _meshRendererQuad.enabled = false; //������ ���������� ������
    }

    public void Show(Material material) // ��������
    {
        _meshRendererHex.enabled = true;
        _meshRendererHex.material = material; // ��������� ���������� ��� ��������
    }

    public void Hide() // ������
    {
        _meshRendererHex.enabled = false;
    }

    // ��� ������� ������������ ����� (����������� ������ ��� ������)
    /*public void ShowSelected() // �������� ���������� ������
    {
        _SelectedGameObject.SetActive(true);
    }
    
    public void HideSelected() // ������ ���������� ������
    {
        _SelectedGameObject.SetActive(false);
    }*/


#else//� ��������� ������ �������������

    private void Start()
    {
        _meshRendererHex.enabled = false; //������ ������������ ������
        _meshRenderer�ircle.enabled = false; //������ ����
    }

    public void Show�ircle(float radius) // �������� ���� ������� ��������
    {
        _meshRenderer�ircle.enabled = true;
        _meshRenderer�ircle.transform.localScale = Vector3.one* 2*radius;
    }

    public void Hide�ircle() // ������
    {
        _meshRenderer�ircle.enabled = false;
    }

    public void Show(Material material) // ��������
    {
        _meshRendererQuad.enabled = true;
        _meshRendererQuad.material = material; // ��������� ���������� ��� ��������
    }

    public void Hide() // ������
    {
        _meshRendererQuad.enabled = false;
        _meshRenderer�ircle.enabled = false;
    }
#endif

}
