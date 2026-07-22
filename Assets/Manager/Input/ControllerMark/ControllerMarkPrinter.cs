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
        bool isShow = target != GamepadType.None;

        SetMarksActive(eastMarks, isShow);
        SetMarksActive(southMarks, isShow);
        SetMarksActive(westMarks, isShow);
        SetMarksActive(northMarks, isShow);

        if (!isShow)
            return;

        ControllerMarkData data = MarkData(target);

        SetMarksSprite(eastMarks, data.sp_ButtonEast);
        SetMarksSprite(southMarks, data.sp_ButtonSouth);
        SetMarksSprite(westMarks, data.sp_ButtonWest);
        SetMarksSprite(northMarks, data.sp_ButtonNorth);
    }

    private void SetMarksActive(Image[] marks, bool active)
    {
        foreach (Image mark in marks)
            mark.gameObject.SetActive(active);
    }

    private void SetMarksSprite(Image[] marks, Sprite sprite)
    {
        foreach (Image mark in marks)
            mark.sprite = sprite;
    }
}
