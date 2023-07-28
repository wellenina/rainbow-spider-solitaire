using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InputManager
{
    public static bool inputDisabled;
    public static bool areMultipleCardsMoving;

    public static void DisableInput(bool disabled)
    {
        inputDisabled = disabled;
    }

    public static void MultipleCardsMoving(bool areMoving)
    {
        areMultipleCardsMoving = areMoving;
        DisableInput(areMoving);
    }
}
