using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.ConsoleArguments.ListBenchmarks;
using BenchmarkDotNet.Engines;
using System;
using System.Drawing;

namespace RemovePoints;

[MemoryDiagnoser]
[InvocationCount(InvocationsPerIteration)]
public class RemoveAllBench
{
    /// <summary>
    /// Iterationごとのメソッド呼び出し回数
    /// </summary>
    private const int InvocationsPerIteration = 8;

    /// <summary>
    /// 点全体の数
    /// </summary>
    private static readonly int TestDataLength = 1000000;

    /// <summary>
    /// 削除する点の数
    /// </summary>
    private static readonly int RemoveLength = 100;

    /// <summary>
    /// テスト用リスト群
    /// 今回は破壊的変更を含むテストなので、繰り返す前に操作対象を複製して使う必要がある
    /// </summary>
    private List<Point>[]? _canvasPointsFamily;

    /// <summary>
    /// 点全体のリスト（削除操作はこれを複製したものに対して行う）
    /// </summary>
    private List<Point>? _canvasPointsSource;

    /// <summary>
    /// 削除する点
    /// </summary>
    private List<Point>? _removePoints;

    private Random? _random;

    private int _iterationIndex = 0;

    [GlobalSetup]
    public void Setup()
    {
        _random = new(42);

        _canvasPointsSource = Enumerable.Repeat(0, TestDataLength).Select(_ => new Point(_random.Next(), _random.Next())).ToList();

        // 点全体のリストからランダムに抜粋したものを削除対象として定義する
        _removePoints = Enumerable.Repeat(0, RemoveLength).Select(_ => _canvasPointsSource[_random.Next(TestDataLength)]).ToList();
    }

    [IterationCleanup]
    public void CleanupIteration() => _iterationIndex = 0;

    [IterationSetup]
    public void SetupListIteration()
    {
        if (_canvasPointsFamily != null)
        {
            foreach (var canvasPoints in _canvasPointsFamily)
            {
                canvasPoints.Clear();
            }
        }
        _canvasPointsFamily = Enumerable.Range(0, InvocationsPerIteration).Select(_ => new List<Point>(_canvasPointsSource!)).ToArray();
    }

    /// <summary>
    /// 1.原作（？）
    /// List<T>.Contains()はO(n)操作なので、長いループから呼び出すべきではない。
    /// 普通にクソ。
    /// </summary>
    [Benchmark]
    public void Kuso()
    {
        _canvasPointsFamily![_iterationIndex++].RemoveAll(p => _removePoints!.Contains(p));
    }

    /// <summary>
    /// 2.生産者の「改善」案
    /// 読む気にならないので詳しく読んでないが、検索が速くなるように自前で並べ替え済みリストを作ったうえで結果は詰めなおし？
    /// それならそれでSortedList<T>などを使いそうなものだが。
    /// いずれにしてもわざわざ長い記述をしたうえで遅く、決して使ってはいけない。
    /// </summary>
    [Benchmark]
    public void BetterKuso()
    {
        var canvasPoints = _canvasPointsFamily![_iterationIndex++];

        // X 方向の最大幅を求める
        int min1 = canvasPoints!.Min(i => i.X);
        int min2 = _removePoints!.Min(i => i.X);
        int max1 = canvasPoints!.Max(i => i.X);
        int max2 = _removePoints!.Max(i => i.X);
        int stride = (max1 > max2 ? max1 : max2) - (min1 < min2 ? min1 : min2) + 1;

        // ソートする
        var before = new List<Point>(canvasPoints!);
        var removes = _removePoints!.Distinct().ToList();
        before.Sort((a, b) => a.Y == b.Y ? a.X - b.X : a.Y - b.Y);
        removes.Sort((a, b) => a.Y == b.Y ? a.X - b.X : a.Y - b.Y);

        // 閾値との違いを削除対象として判定する
        canvasPoints!.Clear();
        int idxRemove = removes.Count - 1;
        int numCheck = removes[idxRemove].X + removes[idxRemove].Y * stride;
        for (int idx = before.Count - 1; idx >= 0; --idx)
        {
            var num = before[idx].X + before[idx].Y * stride;
            while (num < numCheck && idxRemove > 0)
            {
                numCheck = removes[--idxRemove].X + removes[idxRemove].Y * stride;
            }
            if (num != numCheck) canvasPoints.Add(before[idx]);
        }
    }

    /// <summary>
    /// 3.お仕置き案
    /// 基本に忠実
    /// </summary>
    [Benchmark]
    public void Normal()
    {
        _canvasPointsFamily![_iterationIndex++].RemoveAll(_removePoints!.ToHashSet().Contains);
    }

    /// <summary>
    /// 4.おまけ　単純に差集合でもいいのでは？案
    /// リストから複数の要素を削除する手段として、最初にこれに思い至っても自然である。実際これで十分なことも多そう。
    /// 今回はcanvasPointsの要素数が多いので、結果を別インスタンスで作るのは流石に不利だ。
    /// </summary>
    [Benchmark]
    public void Except()
    {
        _canvasPointsFamily![_iterationIndex]  = _canvasPointsFamily[_iterationIndex].Except(_removePoints!).ToList();
        _iterationIndex++;
    }
}
