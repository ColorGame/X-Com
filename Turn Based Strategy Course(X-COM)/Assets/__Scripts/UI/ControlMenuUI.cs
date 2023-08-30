using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class ControlMenuUI : MonoBehaviour
{
    private Animator _animator; //
    private bool _isOpen = false; //


    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        UpdateStateControlMenu(_isOpen);
    }

    public void UpdateStateControlMenu(bool isOpen)
    {
        if (isOpen)
        {
            OpenControlMenu();
        }
        else
        {
            CloseControlMenu();
        }
    }

    public void OpenControlMenu()
    {
        _isOpen= true;
        _animator.SetBool("IsOpen", _isOpen); // Настроим булевую переменную "GetIsOpen". Передадим ей значение _isOpen
    }

    public void CloseControlMenu()
    {
        _isOpen= false;
        _animator.SetBool("IsOpen", _isOpen); // Настроим булевую переменную "GetIsOpen". Передадим ей значение _isOpen
    }

    public bool GetIsOpen()
    {
        return _isOpen;
    }

    public void SetIsOpen(bool isOpen)
    {
        _isOpen = isOpen;
    }
}
