using System;
using System.Collections.Generic;

[System.Serializable]
public struct Pair<F, S>
{
    //public Pair()
    //{ }
    public Pair(F f, S s)
    {
        this.first = f;
        this.second = s;
    }
    public F first;
    public S second;

    public override bool Equals(object obj)
    {
        if (obj is Pair<F, S> other)
        {
            return EqualityComparer<F>.Default.Equals(first, other.first)
                && EqualityComparer<S>.Default.Equals(second, other.second);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(first, second);
    }

    public static bool operator ==(Pair<F, S> a, Pair<F, S> b)
    {
        return a.first.Equals(b.first) && a.second.Equals(b.second);
    }
    public static bool operator !=(Pair<F, S> a, Pair<F, S> b)
    {
        return !a.first.Equals(b.first) || !a.second.Equals(b.second);
    }

}