using SoupSoftware.FindSpace.Interfaces;
using SoupSoftware.FindSpace.Optimisers;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SoupSoftware.FindSpace
{

   

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

       

        public SearchAlgorithm SearchAlgorithm { get; set; } = SearchAlgorithm.Exact;
        public int Padding { get; set; } = 2;

        public iMargin Margins { get; set; } = new ManualMargin(10, 10, 10, 10);


        public bool AutoRotate { get; set; }

        public int CutOffVal { get; } = 3 * byte.MaxValue;


        public IOptimiser Optimiser { get; set; } = new BottomRightOptimiser();

    }

    public enum SearchAlgorithm { Exact = 1, Optimised = 2 }

}


