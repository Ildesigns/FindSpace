using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SoupSoftware.FindSpace.Optimisers;
using System.Drawing;
using SoupSoftware.FindSpace.Interfaces;
using System.Linq;

namespace FindSpaceTests
{
    [TestClass]
    public class VisualTests
    {


        public Bitmap CreateBitmap(string filepath)
        {
            Bitmap b = (Bitmap)Bitmap.FromFile(filepath);

            return b;

        }

        [DataTestMethod]
        [DataRow(@"Test1.bmp", typeof(TopLefttOptimiser))]
        [DataRow(@"Test1.bmp", typeof(TopRighttOptimiser))]
        [DataRow(@"Test1.bmp", typeof(TopOptimiser))]
        [DataRow(@"Test1.bmp", typeof(TopCentreOptimiser))]
        [DataRow(@"Test1.bmp", typeof(MiddleLeftOptimiser))]
        [DataRow(@"Test1.bmp", typeof(MiddleCentreOptimiser))]
        [DataRow(@"Test1.bmp", typeof(MiddleRightOptimiser))]
        [DataRow(@"Test1.bmp", typeof(BottomLeftOptimiser))]
        [DataRow(@"Test1.bmp", typeof(BottomCentreOptimiser))]
        [DataRow(@"Test1.bmp", typeof(BottomOptimiser))]
        [DataRow(@"Test1.bmp", typeof(BottomRightOptimiser))]
        [DataRow(@"Test2.bmp", typeof(TopLefttOptimiser))]
        [DataRow(@"Test2.bmp", typeof(TopRighttOptimiser))]
        [DataRow(@"Test2.bmp", typeof(TopOptimiser))]
        [DataRow(@"Test2.bmp", typeof(TopCentreOptimiser))]
        [DataRow(@"Test2.bmp", typeof(MiddleLeftOptimiser))]
        [DataRow(@"Test2.bmp", typeof(MiddleCentreOptimiser))]
        [DataRow(@"Test2.bmp", typeof(MiddleRightOptimiser))]
        [DataRow(@"Test2.bmp", typeof(BottomLeftOptimiser))]
        [DataRow(@"Test2.bmp", typeof(BottomCentreOptimiser))]
        [DataRow(@"Test2.bmp", typeof(BottomOptimiser))]
        [DataRow(@"Test2.bmp", typeof(BottomRightOptimiser))]
        [DataRow(@"Test2.bmp", typeof(TopLefttOptimiser))]
        [DataRow(@"Test2.bmp", typeof(TopRighttOptimiser))]
        [DataRow(@"Test2.bmp", typeof(TopOptimiser))]
        [DataRow(@"Test2.bmp", typeof(TopCentreOptimiser))]
        [DataRow(@"Test2.bmp", typeof(MiddleLeftOptimiser))]
        [DataRow(@"Test2.bmp", typeof(MiddleCentreOptimiser))]
        [DataRow(@"Test2.bmp", typeof(MiddleRightOptimiser))]
        [DataRow(@"Test2.bmp", typeof(BottomLeftOptimiser))]
        [DataRow(@"Test2.bmp", typeof(BottomCentreOptimiser))]
        [DataRow(@"Test2.bmp", typeof(BottomOptimiser))]
        [DataRow(@"Test2.bmp", typeof(BottomRightOptimiser))]
        [DataRow(@"Test2.bmp", typeof(TopLefttOptimiser))]
        [DataRow(@"Test2.bmp", typeof(TopRighttOptimiser))]
        [DataRow(@"Test2.bmp", typeof(TopOptimiser))]
        [DataRow(@"Test2.bmp", typeof(TopCentreOptimiser))]
        [DataRow(@"Test2.bmp", typeof(MiddleLeftOptimiser))]
        [DataRow(@"Test2.bmp", typeof(MiddleCentreOptimiser))]
        [DataRow(@"Test2.bmp", typeof(MiddleRightOptimiser))]
        [DataRow(@"Test2.bmp", typeof(BottomLeftOptimiser))]
        [DataRow(@"Test2.bmp", typeof(BottomCentreOptimiser))]
        [DataRow(@"Test2.bmp", typeof(BottomOptimiser))]
        [DataRow(@"Test2.bmp", typeof(BottomRightOptimiser))]
        [DataRow(@"Test3.bmp", typeof(TopLefttOptimiser))]
        [DataRow(@"Test3.bmp", typeof(TopRighttOptimiser))]
        [DataRow(@"Test3.bmp", typeof(TopOptimiser))]
        [DataRow(@"Test3.bmp", typeof(TopCentreOptimiser))]
        [DataRow(@"Test3.bmp", typeof(MiddleLeftOptimiser))]
        [DataRow(@"Test3.bmp", typeof(MiddleCentreOptimiser))]
        [DataRow(@"Test3.bmp", typeof(MiddleRightOptimiser))]
        [DataRow(@"Test3.bmp", typeof(BottomLeftOptimiser))]
        [DataRow(@"Test3.bmp", typeof(BottomCentreOptimiser))]
        [DataRow(@"Test3.bmp", typeof(BottomOptimiser))]
        [DataRow(@"Test3.bmp", typeof(BottomRightOptimiser))]
        public void TestMethod(string testfilepath, Type type)
        {
            
            Bitmap b = CreateBitmap(testfilepath);
            SoupSoftware.FindSpace.Interfaces.IOptimiser optimiser;
            if (type.GetConstructors().Any(x => x.GetParameters().Count() == 0))
            {
                 optimiser = (IOptimiser)Activator.CreateInstance(type);
            }
            else
            {
                Rectangle re = new Rectangle(0, 0, b.Width, b.Height);
                optimiser = (IOptimiser)Activator.CreateInstance(type,new object[] {re });
            }
                SoupSoftware.FindSpace.WhitespacerfinderSettings wsf = new SoupSoftware.FindSpace.WhitespacerfinderSettings();
            wsf.Optimiser = optimiser;
            wsf.SearchAlgorithm = SoupSoftware.FindSpace.SearchAlgorithm.Optimised;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SoupSoftware.FindSpace.WhiteSpaceFinder w = new SoupSoftware.FindSpace.WhiteSpaceFinder(b, wsf);
            sw.Stop();
            Trace.WriteLine("Init Image " + sw.ElapsedMilliseconds  +" ms");
            Rectangle stamp = new Rectangle(0, 0, 25, 25);
            sw.Reset();
                sw.Start();
            Rectangle? r = w.FindSpaceFor(stamp);
            sw.Stop();
            Trace.WriteLine("Find Image " + sw.ElapsedMilliseconds + " ms");
            Assert.IsNotNull(r);
            if (r != null)
            {
                Graphics g = System.Drawing.Graphics.FromImage(b);
                g.FillRectangle(Brushes.Red, (Rectangle)r);
                g.Flush();

        //        Console.WriteLine(optimiser.GetType().Name + sw.ElapsedMilliseconds / 1000);

                string extension = System.IO.Path.GetExtension(testfilepath);
                string filepath = testfilepath.Replace(extension, optimiser.GetType().Name + extension);
                b.Save(filepath);
            }
        }




       
    }
}
