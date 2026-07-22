using UnityEngine;

[CreateAssetMenu(fileName = "ControllerMarkData", menuName = "Input/ControllerMarkData")]
public class ControllerMarkData : ScriptableObject
{
    [Header("Button")]
    public Sprite sp_ButtonEast;
    public Sprite sp_ButtonSouth;
    public Sprite sp_ButtonWest;
    public Sprite sp_ButtonNorth;

}
