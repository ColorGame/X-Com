using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenagerUI : MonoBehaviour
{
   [SerializeField] private OptionsUI _optionsUI;

    

    private void Update()
    {
        if (InputManager.Instance.IsEscButtonDownThisFrame()) // ���� ������ ESC
        {
            _optionsUI.ToggleVisible();
        }
    }
}
