using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoupSoftware.FindSpace.Interfaces;
using SoupSoftware.FindSpace.Optimisers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FindSpaceTests
{
    [TestClass]
    public class FindSapceTests
    {

        List<Point> p;

        [TestInitialize]
    public void init() 
    { 
            p = new List<Point>( );

    }

         public void HorizontalThenVerticalSweepPointGenerator(int val, int[] dims)
        {
            List<Point> p = new List<Point>();
            HorizontalThenVerticalSweepPointGenerator gen = new HorizontalThenVerticalSweepPointGenerator();
            int[] r = Enumerable.Range(0, dims[0]).ToArray();
            int[] r2 = Enumerable.Range(0, dims[1]).ToArray();
            p.AddRange(gen.GetOptimisedPoints(r, r2));

            Assert.AreEqual(val, p.Distinct().Count());
        }
        [DataTestMethod]
        [DataRow(typeof(VerticalThenHorizontalSweepPointGenerator), new int[] { 10, 10 })]
        [DataRow(typeof(VerticalThenHorizontalSweepPointGenerator),  new int[] { 16, 10 })]
        [DataRow(typeof(VerticalThenHorizontalSweepPointGenerator), new int[] { 10, 16 })]

        [DataRow(typeof(HorizontalThenVerticalSweepPointGenerator), new int[] { 10, 10 })]
        [DataRow(typeof(HorizontalThenVerticalSweepPointGenerator), new int[] { 16, 10 })]
        [DataRow(typeof(HorizontalThenVerticalSweepPointGenerator), new int[] { 10, 16 })]

        [DataRow(typeof(DiagonalPointGenerator), new int[] { 10, 10 })]
        [DataRow(typeof(DiagonalPointGenerator), new int[] { 16, 10 })]
        [DataRow(typeof(DiagonalPointGenerator), new int[] { 10, 16 })]

        public void PointGeneratorTests(Type t, int[] dims)
        {
            List<Point> p = new List<Point>();
            IPointGenerator gen = (IPointGenerator)Activator.CreateInstance(t);
            int[] r = Enumerable.Range(0, dims[0]).ToArray();
            int[] r2 = Enumerable.Range(0, dims[1]).ToArray();
            p.AddRange(gen.GetOptimisedPoints(r, r2));

            Assert.AreEqual(dims[0]*dims[1], p.Distinct().Count());
        }





    }
}
