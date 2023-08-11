using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenagerUI : MonoBehaviour
{
   [SerializeField] private OptionsUI _optionsUI;

    

    private void Update()
    {
        if (InputManager.Instance.IsEscButtonDownThisFrame()) // Если нажата ESC
        {
            _optionsUI.ToggleVisible();
        }
    }
}
