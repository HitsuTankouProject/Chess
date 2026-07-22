using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class ControllerMarkPrinter
{
    private ResourcesData _resourcesData => GameManager.Instance.resourcesData;
    private ControllerMarkData MarkData(GamepadType target)
    {
        switch(target)
        {
            case GamepadType.PlayStation:   return _resourcesData.ps_ControllerMark;
            case GamepadType.Xbox:          return _resourcesData.xbox_ControllerMark;
            case GamepadType.Switch:        return _resourcesData.switch_ControllerMark;
            default:                        return null;
        }
    }

    [Header("Button East Mark")]
    public Image[] eastMarks;
    [Header("Button South Mark")]
    public Image[] southMarks;
    [Header("Button West Mark")]
    public Image[] westMarks;
    [Header("Button North Mark")]
    public Image[] northMarks;

    public void ChangeMark(GamepadType target)
    {
        ControllerMarkData targetData = MarkData(target);

        if (targetData == null)
        {

            foreach (var mark in eastMarks)
                mark.enabled = false;
            foreach (var mark in southMarks)
                mark.enabled = false;
            foreach (var mark in westMarks)
                mark.enabled = false;
            foreach (var mark in northMarks)
                mark.enabled = false;

            return;
        }

        foreach (var mark in eastMarks)
            mark.sprite = targetData.sp_ButtonEast;
        foreach (var mark in southMarks)
            mark.sprite = targetData.sp_ButtonSouth;
        foreach (var mark in westMarks)
            mark.sprite = targetData.sp_ButtonWest;
        foreach (var mark in northMarks)
            mark.sprite = targetData.sp_ButtonNorth;
    }


}
