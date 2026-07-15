using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

[SelectionBase]
/// <summary>
/// InputSystem の入力デバイス管?ク?ス
/// Mouse / Keyboard / Gamepad を取得・管?する
/// </summary>
class InPutDevice
{
    /// <summary> 現在接続されている Mouse </summary>
    public Mouse mouse => Mouse.current;

    /// <summary> 現在接続されている Keyboard </summary>
    public Keyboard keyboard => Keyboard.current;

    /// <summary> 接続?の Gamepad一? </summary>
    public ReadOnlyArray<Gamepad> connectingGamepad => Gamepad.all;

}

