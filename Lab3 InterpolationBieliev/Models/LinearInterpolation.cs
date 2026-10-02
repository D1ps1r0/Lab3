using System.Collections.Generic;

namespace InterpolationLab.Models;

public sealed class LinearInterpolation : Interpolation
{
    public override double Calculate(IReadOnlyList<PointD> points, double x)
    {
        CheckPoints(points);
        int i = FindSegment(points, x);
        double dx = points[i + 1].X - points[i].X;
        double t = (x - points[i].X) / dx;
        return points[i].Y + t * (points[i + 1].Y - points[i].Y);
    }
}