﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SoupSoftware.FindSpace.Optimisers;
using System.Drawing;
using SoupSoftware.FindSpace.Interfaces;
using System.Linq;
using System.IO;
using SoupSoftware.FindSpace;

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

        public static IEnumerable<object[]> GetTestData()
        {

            DirectoryInfo dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            FileInfo[] files = dir.GetFiles("*.bmp");
            Type ty = typeof(SoupSoftware.FindSpace.Interfaces.IOptimiser);

            Type[] types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => ty.IsAssignableFrom(p) && !p.IsAbstract && !p.IsInterface && p.GetConstructors().Any(c => c.GetParameters().Length == 0)).ToArray();


            string[] typenames = types.Select(t => t.Name).ToArray();

            string[] paths = files.Where(f => !(typenames.Any(tn => f.Name.Contains(tn)))).Select(res => res.FullName).ToArray();


            return paths.SelectMany(x => types, (x, y) => new object[] { x, y });
            
        }



        [DataTestMethod]
        [DynamicData(nameof(GetTestData), DynamicDataSourceType.Method)]
        public void TestMethod(string testfilepath, Type type)
        {

            Bitmap b = CreateBitmap(testfilepath);
            SoupSoftware.FindSpace.Interfaces.IOptimiser optimiser;

            optimiser = (IOptimiser)Activator.CreateInstance(type);


            SoupSoftware.FindSpace.WhitespacefinderSettings wsf = new SoupSoftware.FindSpace.WhitespacefinderSettings();
            wsf.Optimiser = optimiser;
            wsf.Brightness = 30;


            wsf.backgroundcolor = Color.Empty;
            wsf.Margins = new AutomaticMargin();
            // wsf.Brightness = 1;
            wsf.SearchAlgorithm = new SoupSoftware.FindSpace.ExactSearch();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SoupSoftware.FindSpace.WhiteSpaceFinder w = new SoupSoftware.FindSpace.WhiteSpaceFinder(b, wsf);
            sw.Stop();
            Trace.WriteLine("Init Image " + sw.ElapsedMilliseconds + " ms");
            Rectangle stamp = new Rectangle(0, 0, 60, 60);
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
                b.Dispose();
            }
        }
        public static IEnumerable<object[]> GetTestData2()
        {

            Color[] colors = new Color[]
            {
                Color.White,
                Color.Red,
                Color.Green,
                Color.Blue,
                Color.Purple,
                Color.Yellow,
                Color.Beige,
                Color.Black,
                Color.AliceBlue,
                Color.GhostWhite,
                Color.Goldenrod
            };
            return colors.Select(a => new object[] { a }).AsEnumerable();

        }


        [DataTestMethod]
        [DynamicData(nameof(GetTestData2), DynamicDataSourceType.Method)]
        public void ColorDetectionTests(Color color)
        {

            Bitmap b = new Bitmap(1, 1);

            Rectangle r = new Rectangle(0, 0, 100, 100);

            Graphics g = System.Drawing.Graphics.FromImage(b);
            g.FillRectangle(new SolidBrush(color), r);
            g.Flush();

            SoupSoftware.FindSpace.WhitespacefinderSettings wsf = new SoupSoftware.FindSpace.WhitespacefinderSettings();
            wsf.Margins = new AutomaticMargin();
            wsf.Brightness = 30;
            wsf.backgroundcolor = Color.Empty;
            wsf.Margins = new AutomaticMargin(0);


            SearchMatrix mask = new SearchMatrix(b, wsf);
            PrivateObject obj = new PrivateObject(mask);
            Color Colorres = (Color)obj.Invoke("GetModalColor");

            Assert.AreEqual(color.ToArgb() & 0x00FFFFFF, Colorres.ToArgb());
        }
    }


}

