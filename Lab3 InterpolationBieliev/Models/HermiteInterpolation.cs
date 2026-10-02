using System.Collections.Generic;

namespace InterpolationLab.Models;

public sealed class HermiteInterpolation : Interpolation
{
    public override double Calculate(IReadOnlyList<PointD> points, double x)
    {
        CheckPoints(points);
        int i = FindSegment(points, x);

        double x0 = points[i].X;
        double x1 = points[i + 1].X;
        double y0 = points[i].Y;
        double y1 = points[i + 1].Y;
        double dx = x1 - x0;
        double t = (x - x0) / dx;

        double m0 = Derivative(points, i);
        double m1 = Derivative(points, i + 1);

        double h00 = 2*t*t*t - 3*t*t + 1;
        double h10 = t*t*t - 2*t*t + t;
        double h01 = -2*t*t*t + 3*t*t;
        double h11 = t*t*t - t*t;

        return h00*y0 + h10*dx*m0 + h01*y1 + h11*dx*m1;
    }

    private static double Derivative(IReadOnlyList<PointD> p, int i)
    {
        if (i == 0)
            return (p[1].Y - p[0].Y) / (p[1].X - p[0].X);

        if (i == p.Count - 1)
            return (p[i].Y - p[i-1].Y) / (p[i].X - p[i-1].X);

        return (p[i+1].Y - p[i-1].Y) / (p[i+1].X - p[i-1].X);
    }
}