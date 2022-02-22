using SoupSoftware.FindSpace;
using SoupSoftware.FindSpace.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using WhitSpace.Results;

namespace SoupSoftware.FindSpace
{
    public class WhiteSpaceFinder
    {

        private readonly Bitmap image;
        private SearchMatrix masks;

        private Rectangle WorkArea;


        private void init(Bitmap image)
        {
            masks = new SearchMatrix(image, this.Settings);
            WorkArea = Settings.Margins.GetWorkArea(masks);

        }

        public WhiteSpaceFinder(Bitmap orig)
        {


            using (Bitmap newBmp = new Bitmap(orig))
            {
                image = newBmp.Clone(new Rectangle(0, 0, newBmp.Width, newBmp.Height), PixelFormat.Format24bppRgb);
            }

            Settings = new WhitespacefinderSettings();
            init(image);
        }
        public WhiteSpaceFinder(Bitmap Image, WhitespacefinderSettings settings)
        {
            image = Image;
            Settings = settings;
            init(image);
        }
        protected WhitespacefinderSettings Settings;

        public Rectangle FindSpaceAt(Rectangle stamp, Point pt)
        {
            this.Settings.Optimiser = new Optimisers.TargetOptimiser(pt);
            return FindSpaceFor(stamp);
        }

        public Rectangle FindSpaceFor(Rectangle stamp)
        {
            if ((WorkArea.Height - (2 * Settings.Padding + stamp.Height) < 0) ||
              (WorkArea.Width - (2 * Settings.Padding + stamp.Width) < 0)
              )
            {
                throw new Exception("The image is smaller than the stamp + padding + margin");
            }

            int stampwidth = stamp.Width + 2 * Settings.Padding;
            int stampheight = stamp.Height + 2 * Settings.Padding;

            // subtract stamp width and height to keep the search restricted to the top left pxel of the stamp (avoids fits past the bounds of the image)
            Rectangle TopLeftBiasedScanArea = new Rectangle(WorkArea.Left, WorkArea.Top, WorkArea.Width - stampwidth, WorkArea.Height - stampheight);
            masks.UpdateMask(stampwidth, stampheight, WorkArea);

            if (Settings.Margins is IAutoMargin)
            {
                WorkArea = Settings.Margins.GetWorkArea(masks);
                masks.UpdateMask(stampwidth, stampheight, WorkArea);
                TopLeftBiasedScanArea = new Rectangle(WorkArea.Left, WorkArea.Top, WorkArea.Width - stampwidth, WorkArea.Height - stampheight);
                //Trace.WriteLine($"Margins: L={Settings.Margins.Left}, T={Settings.Margins.Top}, R={Settings.Margins.Right}, B={Settings.Margins.Bottom}");
            }

            FindResults findReturn = FindLocations(stampwidth, stampheight, masks, TopLeftBiasedScanArea);
            FindResults findReturn90 = new FindResults(image.Width, image.Height, TopLeftBiasedScanArea);

            if (Settings.AutoRotate && !findReturn.hasExactMatches() && stampheight != stampwidth)
            {
                findReturn90 = FindLocations(stampheight, stampwidth, masks, TopLeftBiasedScanArea);
            }
            return SelectBestArea(stampwidth, stampheight, TopLeftBiasedScanArea, findReturn, findReturn90);
        }
        
        private Rectangle SelectBestArea(int stampwidth, int stampheight, Rectangle ScanArea, FindResults findReturn, FindResults findReturn90)
        {
            Rectangle place2 = new Rectangle(0, 0, stampwidth, stampheight);
            if (findReturn.hasExactMatches())
            {
                place2 = findReturn.exactMatches.First();
            }
            else if (findReturn90.hasExactMatches())
            {
                place2 = findReturn90.exactMatches.First();
            }
            else
            {
                FindResults target = findReturn.minValue <= findReturn90.minValue ? findReturn : findReturn90;
                foreach (Point p in this.Settings.Optimiser.GetOptimisedPoints(ScanArea))
                {
                    if (target.possibleMatches[p.X, p.Y] == target.minValue)
                    {
                        place2 = new Rectangle(p.X, p.Y, stampwidth, stampheight);
                    }
                }
            }
            place2 = new Rectangle(place2.X + Settings.Padding, place2.Y + Settings.Padding, stampwidth - 2 * Settings.Padding, stampheight - 2 * Settings.Padding);
            return place2;
        }

        

        

        

        private FindResults FindLocations(int stampwidth, int stampheight, SearchMatrix masks, Rectangle ScanArea)
        {

            int deepCheckFail = (stampheight * stampwidth) + 1;
            FindResults findReturn = new FindResults(masks.Mask.GetLength(0), masks.Mask.GetLength(1), ScanArea);
            findReturn.containsResults = true;
            //iterate the 2 matrices, if the top left corners X & Y sums is greater than the sticker dimensions its a potential location, 
            // aswe add the loctions transposing the loction to the top left.
            foreach (Point p in this.Settings.Optimiser.GetOptimisedPoints(ScanArea))


            {
                if (masks.MaskValsX[p.X, p.Y] > stampwidth && masks.MaskValsY[p.X, p.Y] > stampheight)
                {

                    findReturn.possibleMatches[p.X, p.Y] = Settings.SearchAlgorithm.Search(masks,
                        p.X, p.Y, stampwidth, stampheight);



                    if (findReturn.possibleMatches[p.X, p.Y] == 0)
                    {
                        //if there are no zeros we can use this space, currently the first found place is used. (The algo is pre-optimised for desired location).

                        findReturn.exactMatches.Add(new Rectangle(
                                                     p.X, p.Y, stampwidth, stampheight
                                                    ));
                        //bail on first find exact macth.
                        return findReturn;
                    }
                }
                else
                {
                    // if the top left corner is not greater than sticker size just skip it..
                    //when it comes to secondary searches we set the number of conflicting spaces to the max value possible.
                    findReturn.possibleMatches[p.X, p.Y] = deepCheckFail;
                }
            }

            return findReturn;
        }


        public void MaskToBitmap(string filepath, bool runs = false)
        {
            int LinearInterp(int start, int end, double percentage) => start + (int)Math.Round(percentage * (end - start));
            Color ColorInterp(float percentage, Color start, Color end) =>
                Color.FromArgb(LinearInterp(start.A, end.A, percentage),
                               LinearInterp(start.R, end.R, percentage),
                               LinearInterp(start.G, end.G, percentage),
                               LinearInterp(start.B, end.B, percentage));
            Color GradientPick(float percentage, Color Start, Color End)
            {
                if (percentage < 0.5)
                    return ColorInterp(percentage / 0.5f, Start, End);
                else
                    return ColorInterp((percentage - 0.5f) / 0.5f, Start, End);
            }

            int w = image.Width;
            int h = image.Height;
            int bpp = Image.GetPixelFormatSize(image.PixelFormat) / 8;

            void WriteInts(string path, int[,] arr)
            {
                Bitmap bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);

                BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                IntPtr ptr = bitmapData.Scan0;

                lock (arr)
                {
                    IEnumerable<int> ints = arr.Cast<int>();
                    float max = ints.Max();
                    float min = ints.Min();
                    Color red = Color.FromArgb(255, 255, 0, 0);
                    Color green = Color.FromArgb(255, 0, 255, 0);
                    for (int j = 0; j < h; j++)
                    {
                        for (int i = 0; i < w; i++)
                        {
                            Color col = GradientPick((arr[i, j] - min) / max, red, green);
                            System.Runtime.InteropServices.Marshal.WriteByte(ptr, (j * bitmapData.Stride) + (i * bpp) + 0, col.B);
                            System.Runtime.InteropServices.Marshal.WriteByte(ptr, (j * bitmapData.Stride) + (i * bpp) + 1, col.G);
                            System.Runtime.InteropServices.Marshal.WriteByte(ptr, (j * bitmapData.Stride) + (i * bpp) + 2, col.R);
                        }
                    }
                }

                bmp.UnlockBits(bitmapData);

                bmp.Save(path);
            }

            Bitmap maskBitmap = new Bitmap(w, h, PixelFormat.Format24bppRgb);

            BitmapData data = maskBitmap.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            IntPtr intPtr = data.Scan0;


            lock (masks.Mask)
            {
                for (int i = 0; i < w; i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        bool maskFilter = masks.Mask[i, j] == 0;
                        System.Runtime.InteropServices.Marshal.WriteByte(intPtr, (j * data.Stride) + (i * bpp) + 0, 0);
                        System.Runtime.InteropServices.Marshal.WriteByte(intPtr, (j * data.Stride) + (i * bpp) + 1, maskFilter ? (byte)0 : (byte)255);
                        System.Runtime.InteropServices.Marshal.WriteByte(intPtr, (j * data.Stride) + (i * bpp) + 2, maskFilter ? (byte)255 : (byte)0);
                    }
                }
            }

            //RGB[] f = sRGB.Deserialize<RGB[]>(buffer)
            maskBitmap.UnlockBits(data);

            maskBitmap.Save(filepath);

            if (runs)
            {
                WriteInts(filepath.Replace(".bmp", "-ColRuns.bmp"), masks.MaskValsY);

                WriteInts(filepath.Replace(".bmp", "-RowRuns.bmp"), masks.MaskValsX);
            }
        }



    }


}


