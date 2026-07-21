using System;
using System.Collections.Generic;

/// <summary>
/// 関連する2つの値を1組として保持する、シリアライズ可能なジェネリック構造体です。
/// 1つ目と2つ目の値をそれぞれ異なる型で保持でき、両方の値に基づく
/// 等価比較、非等価比較、ハッシュコード生成を提供します。
/// </summary>
/// <typeparam name="F">1つ目の値の型です。</typeparam>
/// <typeparam name="S">2つ目の値の型です。</typeparam>
[System.Serializable]
public struct Pair<F, S>
{
    /// <summary>指定された2つの値を使用してペアを初期化します。</summary>
    /// <param name="f">1つ目に設定する値です。</param>
    /// <param name="s">2つ目に設定する値です。</param>
    public Pair(F f, S s)
    {
        this.first = f;
        this.second = s;
    }
    /// <summary>ペアの1つ目の値です。</summary>
    public F first;
    /// <summary>ペアの2つ目の値です。</summary>
    public S second;
    /// <summary>指定オブジェクトが同じ型のペアで、両方の値が等しいか判定します。</summary>
    /// <param name="obj">比較するオブジェクトです。</param>
    /// <returns>1つ目と2つ目の値がともに等しい場合は <see langword="true" /> です。</returns>
    public override bool Equals(object obj)
    {
        if (obj is Pair<F, S> other)
        {
            // 各型の既定EqualityComparerを使用し、nullを含む値も比較します。
            return EqualityComparer<F>.Default.Equals(first, other.first)
                && EqualityComparer<S>.Default.Equals(second, other.second);
        }
        return false;
    }

    /// <summary>1つ目と2つ目の値を組み合わせたハッシュコードを取得します。</summary>
    /// <returns>このペアを表すハッシュコードです。</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(first, second);
    }
    /// <summary>2つのペアに含まれる両方の値が等しいか判定します。</summary>
    /// <param name="a">比較する左辺のペアです。</param>
    /// <param name="b">比較する右辺のペアです。</param>
    /// <returns>両方の値が等しい場合は <see langword="true" /> です。</returns>
    public static bool operator ==(Pair<F, S> a, Pair<F, S> b)
    {
        return a.first.Equals(b.first) && a.second.Equals(b.second);
    }
    /// <summary>2つのペアに含まれるいずれかの値が異なるか判定します。</summary>
    /// <param name="a">比較する左辺のペアです。</param>
    /// <param name="b">比較する右辺のペアです。</param>
    /// <returns>いずれかの値が異なる場合は <see langword="true" /> です。</returns>
    public static bool operator !=(Pair<F, S> a, Pair<F, S> b)
    {
        return !a.first.Equals(b.first) || !a.second.Equals(b.second);
    }

}