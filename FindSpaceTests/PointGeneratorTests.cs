using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoupSoftware.FindSpace.Optimisers;
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

        [DataTestMethod]
        [DataRow(100,new int[]{10,10})]
        [DataRow(160, new int[] { 16, 10 })]
        [DataRow(160, new int[] { 10, 16 })]
        public void DiagonalPointGenTouchesAllPoints(int val, int[] dims)
        {
            
            DiagonalPointGenerator gen = new DiagonalPointGenerator();
            int[] r = Enumerable.Range(0, dims[0]).ToArray();
            int[] r2 = Enumerable.Range(0, dims[1]).ToArray();
            p.AddRange(gen.GetOptimisedPoints(r, r2));
           
            Assert.AreEqual(val, p.Distinct().Count());
        }

        [DataTestMethod]
        [DataRow(100, new int[] { 10, 10 })]
        [DataRow(160, new int[] { 16, 10 })]
        [DataRow(160, new int[] { 10, 16 })]
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
        [DataRow(100, new int[] { 10, 10 })]
        [DataRow(160, new int[] { 16, 10 })]
        [DataRow(160, new int[] { 10, 16 })]
        public void VerticalThenHorizontalSweepPointGenerator(int val, int[] dims)
        {
            List<Point> p = new List<Point>();
            VerticalThenHorizontalSweepPointGenerator gen = new VerticalThenHorizontalSweepPointGenerator();
            int[] r = Enumerable.Range(0, dims[0]).ToArray();
            int[] r2 = Enumerable.Range(0, dims[1]).ToArray();
            p.AddRange(gen.GetOptimisedPoints(r, r2));

            Assert.AreEqual(val, p.Distinct().Count());
        }





    }
}
