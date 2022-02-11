﻿using System.Collections.Generic;
using System.Drawing;

namespace SoupSoftware.FindSpace.Interfaces
{

    public interface IOptimiser
    {

        IEnumerable<Point> GetOptimisedPoints(Rectangle rect);


    }

    public interface ICoordinateSorter
    {
        IEnumerable<int> GetOptimisedPositions(int lower, int upper);

    }

    public interface IPointGenerator
    {
        IEnumerable<Point> GetOptimisedPoints(IEnumerable<int> xcoords, IEnumerable<int> ycoords);

    }

    public interface iMargin
    {

        bool AutoExpand { get;  }
        int Left { get; }
        int Right { get; }
        int Top { get; }
        int Bottom { get; }
        Rectangle GetworkArea(searchMatrix masks);
    }

    public interface ISearchMatrix {

         byte[,] mask { get;  }
         int[,] maskvalsx { get;  }
         int[,] maskvalsy { get;  }
         int[,] deepCheck { get; }
         int[] colSums { get; }
         int[] rowSums { get;  }
         bool maskCalculated { get;}
        void CalculateMask(int stampwidth, int stampheight, Rectangle WorkArea);

            }
}

