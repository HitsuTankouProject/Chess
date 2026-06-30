using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

public class aaa : MonoBehaviour
{
    private void Start()
    {
        foreach(Gamepad gamepad in Gamepad.all)
        {
            Debug.Log($"{gamepad.name.ToLower()} + {gamepad.displayName} + {gamepad.description.product}");
            var desc = gamepad.description;

            Debug.Log(desc.interfaceName);
            Debug.Log(desc.manufacturer);
            Debug.Log(desc.product);
        }
    }
}
