using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Commons;
using Commons.Mathematics;
using DataAPI.Service.AccessManagement;
using NUnit.Framework;

namespace DataAPI.Service.Test.AccessManagement
{
    [TestFixture]
    public class HashComparerTest
    {
        [Test]
        public void ArraysOfDifferentLengthReturnFalse()
        {
            var a = new byte[3];
            var b = new byte[4];
            Assert.That(HashComparer.Compare(a, b), Is.False);
        }

        [Test]
        public void DifferentArraysReturnFalse()
        {
            var a = new byte[] { 0x41, 0x42, 0x43 };
            var b = new byte[] { 0x01, 0x02, 0x03 };
            Assert.That(HashComparer.Compare(a, b), Is.False);
        }

        [Test]
        public void EqualArraysReturnTrue()
        {
            var a = new byte[] { 0x41, 0x42, 0x43 };
            var b = new byte[] { 0x41, 0x42, 0x43 };
            Assert.That(HashComparer.Compare(a, b), Is.True);
        }

        [Test]
        public void ComparisonIsConstantTime()
        {
            var runTimes = new List<RunTime>();
            var a = new byte[128];
            var b = new byte[128];
            for (var i = 0; i < 100000; i++)
            {
                var differencePosition = i % a.Length;
                b[differencePosition] = 0x01;
                var stopwatch = Stopwatch.StartNew();
                HashComparer.Compare(a, b);
                stopwatch.Stop();
                if(i > 10) // Exclude first runs to exclude startup effects
                    runTimes.Add(new RunTime(differencePosition, stopwatch.ElapsedTicks));
                b[differencePosition] = 0x00;
            }
            var correlation = StatisticalOperations.Correlation(
                runTimes.Select(x => (double)x.DifferencePosition).ToArray(),
                runTimes.Select(x => (double)x.ElapsedTicks).ToArray());
            Console.WriteLine($"Correlation: {correlation}");
            Assert.That(correlation, Is.EqualTo(0).Within(0.01), "Correlation");
        }

        private class RunTime
        {
            public RunTime(
                int differencePosition,
                long elapsedTicks)
            {
                DifferencePosition = differencePosition;
                ElapsedTicks = elapsedTicks;
            }

            public int DifferencePosition { get; }
            public long ElapsedTicks { get; }
        }
    }
}
