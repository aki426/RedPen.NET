using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Lucene.Net.Util.Packed.AbstractAppendingInt64Buffer;

namespace VerifyBasicFunction.Tests
{
    public class ListVsHashSetTests
    {
        private ITestOutputHelper output;

        public ListVsHashSetTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        // List<int>とHashSet<int>のContainsとIterationの速度比較
        // List: Contains takes 26ms, counter: 21614000
        // HashSet: Contains takes 0ms, counter: 21614000
        // List: Iteration takes 1ms, counter: 705532704
        // HashSet: Iteration takes 3ms, counter: 705532704

        [Fact]
        public void EvaluateTimeTest()
        {
            List<int> list = Enumerable.Range(1, 10000).ToList();
            HashSet<int> set = Enumerable.Range(1, 10000).ToHashSet();
            var q = new List<int> { 1, 200, 310, 4900, 5600, 600, 74, 830, 9000, 99 };

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // 1,000 loop
            int counter = 0;
            foreach (var times in Enumerable.Range(1, 1000))
            {
                foreach (var i in q)
                {
                    if (list.Contains(i))
                    {
                        counter += i;
                    }
                }
            }

            sw.Stop();

            output.WriteLine($"List: Contains takes {sw.ElapsedMilliseconds}ms, counter: {counter}");

            sw.Restart();

            // 1,000 loop
            counter = 0;
            foreach (var times in Enumerable.Range(1, 1000))
            {
                foreach (var i in q)
                {
                    if (set.Contains(i))
                    {
                        counter += i;
                    }
                }
            }
            sw.Stop();

            output.WriteLine($"HashSet: Contains takes {sw.ElapsedMilliseconds}ms, counter: {counter}");

            sw.Restart();

            counter = 0;
            foreach (var times in Enumerable.Range(1, 100))
            {
                foreach (var i in list)
                {
                    counter += i;
                }
            }

            sw.Stop();

            output.WriteLine($"List: Iteration takes {sw.ElapsedMilliseconds}ms, counter: {counter}");

            sw.Restart();

            counter = 0;
            foreach (var times in Enumerable.Range(1, 100))
            {
                foreach (var i in set)
                {
                    counter += i;
                }
            }

            sw.Stop();

            output.WriteLine($"HashSet: Iteration takes {sw.ElapsedMilliseconds}ms, counter: {counter}");
        }

        // List<string>とHashSet<string>のContainsとIterationの速度比較
        // List: Contains takes 76ms, counter: 29000
        // HashSet: Contains takes 0ms, counter: 29000
        // List: Iteration takes 4ms, counter: 3889400
        // HashSet: Iteration takes 5ms, counter: 3889400

        [Fact]
        public void EvaluateAsStringTimeTest()
        {
            List<string> list = Enumerable.Range(1, 10000).Select(i => i.ToString()).ToList();
            HashSet<string> set = Enumerable.Range(1, 10000).Select(i => i.ToString()).ToHashSet();
            var q = (new List<int> { 1, 200, 310, 4900, 5600, 600, 74, 830, 9000, 99 }).Select(i => i.ToString()).ToList();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // 1,000 loop
            int counter = 0;
            foreach (var times in Enumerable.Range(1, 1000))
            {
                foreach (var i in q)
                {
                    if (list.Contains(i))
                    {
                        counter += i.Length;
                    }
                }
            }

            sw.Stop();

            output.WriteLine($"List: Contains takes {sw.ElapsedMilliseconds}ms, counter: {counter}");

            sw.Restart();

            // 1,000 loop
            counter = 0;
            foreach (var times in Enumerable.Range(1, 1000))
            {
                foreach (var i in q)
                {
                    if (set.Contains(i))
                    {
                        counter += i.Length;
                    }
                }
            }
            sw.Stop();

            output.WriteLine($"HashSet: Contains takes {sw.ElapsedMilliseconds}ms, counter: {counter}");

            sw.Restart();

            counter = 0;
            foreach (var times in Enumerable.Range(1, 100))
            {
                foreach (var i in list)
                {
                    counter += i.Length;
                }
            }

            sw.Stop();

            output.WriteLine($"List: Iteration takes {sw.ElapsedMilliseconds}ms, counter: {counter}");

            sw.Restart();

            counter = 0;
            foreach (var times in Enumerable.Range(1, 100))
            {
                foreach (var i in set)
                {
                    counter += i.Length;
                }
            }

            sw.Stop();

            output.WriteLine($"HashSet: Iteration takes {sw.ElapsedMilliseconds}ms, counter: {counter}");
        }
    }
}
