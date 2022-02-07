using SoupSoftware.FindSpace.Interfaces;
using SoupSoftware.FindSpace.Optimisers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SoupSoftware.FindSpace
{
    public class AutomaticMargin : iMargin
    {
        public AutomaticMargin(int top, int bottom, int left, int right)
        {
            Top = top;
            Bottom = bottom;
            Left = left;
            Right = right;
        }

        public bool AutoExpand { get; set; } = true;

        public int Left { get; private set; }

        public int Right { get; private set; }

        public int Top { get; private set; }

        public int Bottom { get; private set; }

        private Rectangle WorkArea;

        public Rectangle GetworkArea(Bitmap image)
        {
            if (image.Width - (Left + Right) < 0 ||
                image.Height - (Top + Bottom) < 0)
            {
                throw new IndexOutOfRangeException("The margins are larger than the image");
            }

            RefineArea(image);

            return WorkArea;
        }

        private int InnerTop = 5;           // desired padding between Margin and document contents
        private int InnerBottom = 5;        // desired padding between Margin and document contents
        private int InnerLeft = 5;          // desired padding between Margin and document contents
        private int InnerRight = 5;         // desired padding between Margin and document contents

        private void RefineArea(Bitmap image)
        {
            int w = image.Width;
            int h = image.Height;

            int depth;
            byte[] data;
            WhiteSpace.searchMatrix.GetBitmapData(image, out depth, out data);

            // use sobel edge detection (expensive, parallelization attempts cause noisy/incorrect image)
            Bitmap sobelFiltered = image.Sobel();

            //sobelFiltered.Save("SobelFiltered.bmp");

            WhiteSpace.searchMatrix.GetBitmapData(sobelFiltered, out depth, out data);

            //Point mid = new Point(image.Width % 2 == 1 ? (image.Width - 1)/2 : image.Width / 2, image.Width % 2 == 1 ? (image.Height - 1) / 2 : image.Height / 2);

            Tuple<int, int>[] ylimits = new Tuple<int, int>[w];
            Tuple<int, int>[] xlimits = new Tuple<int, int>[h];

            int[] GetColumnData(int x, byte[] input, int width, int Bpp)
            {
                if (input.Length == 0)
                    throw new InvalidOperationException("Cannot get column data for input of length 0");
                int stride = width * Bpp;
                int ny = input.Length / stride;
                int[] outData = new int[ny];
                for (int y = 0; y < ny; y++)
                {
                    outData[y] = (input[y * stride + x * Bpp] << 16) + (input[y * stride + x * Bpp + 1] << 8) + input[y * stride + x * Bpp + 2];
                }
                return outData;
            }

            int[] GetRowData(int y, byte[] input, int width, int Bpp)
            {
                if (input.Length == 0)
                    throw new InvalidOperationException("Cannot get row data for input of length 0");
                int stride = width * Bpp;
                int ny = input.Length / stride;
                int[] outData = new int[width];
                for (int x = 0; x < width; x++)
                {
                    outData[x] = (input[y * stride + x * Bpp] << 16) + (input[y * stride + x * Bpp + 1] << 8) + input[y * stride + x * Bpp + 2];
                }
                return outData;
            }


            lock (data)
            {

                // 1. Get each row/column
                // 2. Find 1st and last non-black pixel in each
                // 3. Store those in Tuples to be filtered

                // Scan from Left
                Parallel.For(0, w, (i) => {
                    int[] pixels = GetColumnData(i, data, w, depth);

                    var nonBlackFilter = Enumerable.Range(0, pixels.Length).Where(p => pixels[p] != 0);
                    bool filterHasValues = nonBlackFilter.ToArray().Length > 0;
                    int upper = filterHasValues ? nonBlackFilter.Max() : -1;
                    int lower = filterHasValues ? nonBlackFilter.Min() : -1;

                    ylimits[i] = new Tuple<int, int>(upper, lower);
                });

                // Scan from Top
                Parallel.For(0, h, (j) => {
                    int[] pixels = GetRowData(j, data, w, depth);

                    var nonBlackFilter = Enumerable.Range(0, pixels.Length).Where(p => pixels[p] != 0);
                    bool filterHasValues = nonBlackFilter.ToArray().Length > 0;
                    int upper = filterHasValues ? nonBlackFilter.Max() : -1;
                    int lower = filterHasValues ? nonBlackFilter.Min() : -1;

                    xlimits[j] = new Tuple<int, int>(upper, lower);
                });
            }

            // Filter tuples to find initial values
            Top = ylimits.Select(ylim => ylim.Item2)
                            .Where(item => item != -1)
                            .Min();
            Left = xlimits.Select(xlim => xlim.Item2)
                           .Where(item => item != -1)
                           .Min();
            Bottom = h - ylimits.Select(ylim => ylim.Item1)
                         .Where(item => item != -1)
                         .Max();
            Right = w - xlimits.Select(xlim => xlim.Item1)
                          .Where(item => item != -1)
                          .Max();

            //  Should the workarea expand to image borders if inner padding cant be kept?
            void Expand()
            {
                Left = Math.Max(Left - InnerLeft, 0);

                Right = Math.Max(Right - InnerRight, 0);

                Top = Math.Max(Top - InnerTop, 0);

                Bottom = Math.Max(Bottom - InnerBottom, 0);
            }

            if (AutoExpand)
                Expand();

            /*
            Bitmap test = new Bitmap(image);

            using (Graphics g = Graphics.FromImage(test))
            {
                WorkArea = new Rectangle(Top, Left, w - (Right + Left), h - (Bottom + Top));

                // draw initial result
                g.DrawRectangle(new Pen(Color.FromArgb(188, 255, 0, 0)), WorkArea);

                // expand if possible
                Expand();

                WorkArea = new Rectangle(Top, Left, w - (Right + Left), h - (Bottom + Top));

                // draw expanded result
                g.DrawRectangle(new Pen(Color.FromArgb(188, 0, 0, 255)), WorkArea);
            }

            test.Save("AutomaticMargins.bmp");
            */

            WorkArea = new Rectangle(Left, Top, w - 1 - (Left + Right), h - 1 - (Top + Bottom)); ;
        }

    }


    public class ManualMargin : iMargin
    {
        public ManualMargin(int left, int right, int top, int bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public Rectangle GetworkArea(Bitmap image)
        {
            if (image.Width - (Left + Right) < 0 ||
                image.Height - (Top + Bottom) < 0)
            {
                throw new IndexOutOfRangeException("The margins are larger than the image");
            }

            return new Rectangle(Left, Top, image.Width - 1 - (Left + Right), image.Height - 1 - (Top + Bottom));
        }


        public bool AutoExpand { get; set; } = true;


        public int Left { get; set; }

        public int Right { get; set; }

        public int Top { get; set; }

        public int Bottom { get; set; }
    }


    public class WhitespacerfinderSettings
    {

        //this value is used to determine if a pixel is empty or not. Future tweak to find average non black pixel and use the color of this
        public int Brightness { get; set; } = 10;

        public bool AutoDetectBackGroundColor = false;
        public Color backGroundColor { get; set; } = Color.White;

        public Color Color { get; set; } = Color.White;



        public IDeepSearch SearchAlgorithm { get; set; } = new ExactSearch();



        public int Padding { get; set; } = 2;

        public iMargin Margins { get; set; } = new ManualMargin(10, 10, 10, 10);


        public bool AutoRotate { get; set; }

        public int CutOffVal { get; } = 3 * byte.MaxValue;


        public IOptimiser Optimiser { get; set; } = new BottomRightOptimiser();

    }


    public interface IDeepSearch
    {
        int Search(ISearchMatrix masks, int Left, int Top, int Width, int Height);
    }

    public static class Extensions
    {
        // Sobel directional matrices
        private static readonly int[,] Gx = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        private static readonly int[,] Gy = { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

        // ITU-R BT.2100 standard
        private static readonly float[] GrayscaleVector = { 0.2627f, 0.6780f, 0.0593f };

        // adapted from https://epochabuse.com/csharp-sobel/
        public static Bitmap Sobel(this Bitmap bmp, bool outAsGrayscale = false)
        {
            int w = bmp.Width;
            int h = bmp.Height;
            int depth;
            byte[] inputData;
            WhiteSpace.searchMatrix.GetBitmapData(bmp, out depth, out inputData);

            byte[] bits = new byte[w * h * depth];
            GCHandle handle = GCHandle.Alloc(bits, GCHandleType.Pinned);

            Bitmap output = new Bitmap(w, h, w * depth, bmp.PixelFormat, handle.AddrOfPinnedObject());

            float xr = 0.0f, xg = 0.0f, xb = 0.0f;  // initialise x sums
            float yr = 0.0f, yg = 0.0f, yb = 0.0f;  // initialise y sums
            float rt = 0.0f, gt = 0.0f, bt = 0.0f;  // initialise totals

            int byteOffset = 0;                     // position/index of kernal in image
            int calcOffset = 0;                     // position/index of cursor for kernal

            for (int i = 1; i < w - 1; i++)
            {
                for (int j = 1; j < h - 1; j++)
                {
                    xr = 0; xg = 0; xb = 0; // reset x sums
                    yr = 0; yg = 0; yb = 0; // reset y sums
                    rt = 0; gt = 0; bt = 0; // reset totals

                    byteOffset = j * (w * depth) + (i * depth);

                    for (int k = -1; k <= 1; k++)
                    {
                        for (int k2 = -1; k2 <= 1; k2++)
                        {
                            calcOffset = byteOffset + (k * depth) + (k2 * depth * w);

                            xb += (inputData[calcOffset]) * Gx[k2 + 1, k + 1];
                            xg += (inputData[calcOffset + 1]) * Gx[k2 + 1, k + 1];
                            xr += (inputData[calcOffset + 2]) * Gx[k2 + 1, k + 1];
                            yb += (inputData[calcOffset]) * Gy[k2 + 1, k + 1];
                            yg += (inputData[calcOffset + 1]) * Gy[k2 + 1, k + 1];
                            yr += (inputData[calcOffset + 2]) * Gy[k2 + 1, k + 1];
                        }
                    }

                    //total rgb values for this pixel
                    bt = (float)Math.Sqrt((xb * xb) + (yb * yb));
                    gt = (float)Math.Sqrt((xg * xg) + (yg * yg));
                    rt = (float)Math.Sqrt((xr * xr) + (yr * yr));

                    //set limits, bytes can hold values from 0 up to 255;
                    if (bt > 255) bt = 255;
                    else if (bt < 0) bt = 0;
                    if (gt > 255) gt = 255;
                    else if (gt < 0) gt = 0;
                    if (rt > 255) rt = 255;
                    else if (rt < 0) rt = 0;

                    if (!outAsGrayscale)
                    {
                        //set new data in the other byte array for your image data
                        bits[byteOffset] = (byte)(rt);
                        bits[byteOffset + 1] = (byte)(gt);
                        bits[byteOffset + 2] = (byte)(bt);
                        //bits[byteOffset + 3] = 255;   // ignore alpha bit
                    }
                    else
                    {
                        byte val = (byte)(GrayscaleVector[0] * rt + GrayscaleVector[1] * gt + GrayscaleVector[2] * bt);
                        bits[byteOffset] = val;
                        bits[byteOffset + 1] = val;
                        bits[byteOffset + 2] = val;
                    }
                }
            }


            return output;
        }

    }

}

