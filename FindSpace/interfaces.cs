using System.Collections.Generic;
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

        bool AutoExpand { get; set; }
        int Left { get; }
        int Right { get; }
        int Top { get; }
        int Bottom { get; }
        Rectangle GetworkArea(Bitmap image);
    }

}

