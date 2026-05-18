using System;
using System.Collections;
using System.Diagnostics;


public abstract class BuffBasic
{
    public abstract ChessType buffChess { get; }
    public ChessBasic _buffChess { get; private set; }

    public abstract string buffName { get; }

    public uint nowBuffLevel { get; private set; } = 0;
    private const uint maxBuffLevel = 3;

    public abstract void ResetBuff();
    public abstract void FirstLevel();
    public abstract void SecondLevel();
    public abstract void ThirdLevel();

    public virtual void LevelUp(out bool success)
    {
        if (nowBuffLevel >= maxBuffLevel || nowBuffLevel < 0) 
        {
            success = false;
            return;
        }
        nowBuffLevel++;
        switch (nowBuffLevel)
        {
            case 1:FirstLevel();break;
            case 2:SecondLevel();break;
            case 3:ThirdLevel();break;
            default: success = false; return;
        }
        success = true;
    }
    public virtual void LevelUpToTargetLevel(uint targetLevel, out bool success)
    {
        if(targetLevel > maxBuffLevel)
        {
            success = false;
            return;
        }

        nowBuffLevel = targetLevel;
        switch (nowBuffLevel)
        {
            case 0:
                ResetBuff();
                break;
            case 1: 
                FirstLevel(); 
                break;
            case 2:
                FirstLevel();
                SecondLevel();
                break;
            case 3:
                FirstLevel();
                SecondLevel();
                ThirdLevel(); 
                break;

            default: success = false; return;


        }
        success = true;

    }

    public virtual void BuffInit(ChessBasic target)
    {
        nowBuffLevel = 0;
        ResetBuff();
        _buffChess = target;
    }

}
