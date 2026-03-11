using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class ArithmeticInputHandler : MonoBehaviour
{
    ArithmeticInputAction inputActions;
    public Action<int> InputDigit;
    public Action InputEnter;
    public Action InputBackspace;
    private void Awake()
    {
        inputActions = new ArithmeticInputAction();
        inputActions.Enable();
        inputActions.Default.Enable();

        inputActions.Default.Digit.performed += DigitAction;
        inputActions.Default.Enter.performed += EnterAction;
        inputActions.Default.Backspace.performed += BackspaceAction;
    }
    private void DigitAction(InputAction.CallbackContext ctx)
    {
        if (!ctx.ReadValueAsButton())
            return;

        if (ctx.control is not KeyControl key)
            return;

        int digit = key.keyCode switch
        {
            Key.Digit0 or Key.Numpad0 => 0,
            Key.Digit1 or Key.Numpad1 => 1,
            Key.Digit2 or Key.Numpad2 => 2,
            Key.Digit3 or Key.Numpad3 => 3,
            Key.Digit4 or Key.Numpad4 => 4,
            Key.Digit5 or Key.Numpad5 => 5,
            Key.Digit6 or Key.Numpad6 => 6,
            Key.Digit7 or Key.Numpad7 => 7,
            Key.Digit8 or Key.Numpad8 => 8,
            Key.Digit9 or Key.Numpad9 => 9,
            _ => -1
        };

        if (digit == -1)
            return;

        InputDigit?.Invoke(digit);
    }
    private void EnterAction(InputAction.CallbackContext ctx)
    {
        InputEnter?.Invoke();
    }
    private void BackspaceAction(InputAction.CallbackContext ctx)
    {
        InputBackspace?.Invoke();
    }

    

    
}
