﻿
using SoupSoftware.FindSpace.Interfaces;

namespace SoupSoftware.FindSpace.Optimisers
{

    public class BottomRightOptimiser : LinearPointOptimiser
    {
        private readonly ICoordinateSorter Uboundresolver = new UboundLinearSorter();
        private readonly IPointGenerator pointgenerator = new DiagonalPointGenerator();
        public override IPointGenerator PointGenerator { get => pointgenerator; }
        protected override ICoordinateSorter XAxisResolver { get => Uboundresolver; }
        protected override ICoordinateSorter YAxisResolver { get => Uboundresolver; }
    }

    public class MiddleRightOptimiser : LinearPointOptimiser
    {
        private readonly ICoordinateSorter Uboundresolver = new UboundLinearSorter();
        private readonly ICoordinateSorter cntrResolver = new CentreLinearSorter();
        private readonly IPointGenerator pointgenerator = new HorizontalThenVerticalSweepPointGenerator();
        public override IPointGenerator PointGenerator { get => pointgenerator; }
        protected override ICoordinateSorter XAxisResolver { get => Uboundresolver; }
        protected override ICoordinateSorter YAxisResolver { get => cntrResolver; }
    }

    public class TopRightOptimiser : LinearPointOptimiser
    {
        private readonly ICoordinateSorter Lboundresolver = new LboundLinearSorter();
        private readonly ICoordinateSorter Uboundresolver = new UboundLinearSorter();
        private readonly IPointGenerator pointgenerator = new DiagonalPointGenerator();
        public override IPointGenerator PointGenerator { get => pointgenerator; }
        protected override ICoordinateSorter XAxisResolver { get => Uboundresolver; }
        protected override ICoordinateSorter YAxisResolver { get => Lboundresolver; }
    }

}
