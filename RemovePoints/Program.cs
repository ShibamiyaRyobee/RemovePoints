using BenchmarkDotNet.Running;
using RemovePoints;

internal class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<RemoveAllBench>();

        // 【2】原作者改善案
        // 読む気にならないので詳しく読んでないが、検索が速くなるように自前で並べ替え済みリストを作った？
        // それならそれでSortedList<T>などを使いそうなものだが。
        // いずれにしてもわざわざ長い記述をしたうえで遅く、使ってはいけない。
        // X 方向の最大幅を求める
        //int min1 = canvasPoints.Min(i => i.X);
        //int min2 = removePoints.Min(i => i.X);
        //int max1 = canvasPoints.Max(i => i.X);
        //int max2 = removePoints.Max(i => i.X);
        //int stride = (max1 > max2 ? max1 : max2) - (min1 < min2 ? min1 : min2) + 1;

        //// ソートする
        //var before = new List<Point>(canvasPoints);
        //var removes = removePoints.Distinct().ToList();
        //before.Sort((a, b) => a.Y == b.Y ? a.X - b.X : a.Y - b.Y);
        //removes.Sort((a, b) => a.Y == b.Y ? a.X - b.X : a.Y - b.Y);

        //// 閾値との違いを削除対象として判定する
        //canvasPoints.Clear();
        //int idxRemove = removes.Count - 1;
        //int numCheck = removes[idxRemove].X + removes[idxRemove].Y * stride;
        //for (int idx = before.Count - 1; idx >= 0; --idx)
        //{
        //    var num = before[idx].X + before[idx].Y * stride;
        //    while (num < numCheck && idxRemove > 0)
        //    {
        //        numCheck = removes[--idxRemove].X + removes[idxRemove].Y * stride;
        //    }
        //    if (num != numCheck) canvasPoints.Add(before[idx]);
        //}


        // 3.お仕置き案
        //canvasPoints.RemoveAll(removePoints.ToHashSet().Contains);

        // 4.おまけ　単純に差集合でもいいのでは？案
        // リストから複数の要素を削除する手段として、最初にこれに思い至っても自然である。実際これで十分なことも多そう。
        // 今回はcanvasPointsの要素数が多いので、結果を別インスタンスで作るのは流石に不利だ。
        // 原作者の「改善」よりは数倍速い。
        //　canvasPoints = canvasPoints.Except(removePoints).ToList();



        //sw.Stop();

        //// お仕置き版は100万要素から100要素消すのに20msほどと出るので、これならBenchmarkDotNetに持っていく気にもなる
        //Console.WriteLine($"{sw.ElapsedMilliseconds}ms");

        //// 最適化で「結果使っていないな」と思われても嫌なので使うふりくらいはしておく
        //Console.WriteLine(canvasPoints[canvasPoints.Count / 2]);
    }
}