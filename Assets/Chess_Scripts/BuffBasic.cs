using System;
using System.Collections;


public abstract class BuffBasic
{
    public abstract ChessType buffChess { get; }
    public abstract string buffName { get; }

    public uint nowBuffLevel { get; private set; } = 0;
    private const uint maxBuffLevel = 3;

    public abstract void FirstLevel();
    public abstract void SecondLevel();
    public abstract void ThirdLevel();

    public virtual void LevelUp(out bool success)
    {
        if (nowBuffLevel >= maxBuffLevel) 
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
    
    public virtual void BuffInit()
    {
        nowBuffLevel = 0;
    }

}
