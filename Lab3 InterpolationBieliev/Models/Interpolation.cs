using System;
using System.Collections.Generic;

namespace InterpolationLab.Models;

public abstract class Interpolation
{
    public abstract double Calculate(IReadOnlyList<PointD> points, double x);

    protected static void CheckPoints(IReadOnlyList<PointD> points)
    {
        if (points.Count < 2)
            throw new ArgumentException("Потрібно щонайменше 2 опорні точки.");

        for (int i = 1; i < points.Count; i++)
            if (points[i].X <= points[i - 1].X)
                throw new ArgumentException("Однакові або неправильні X заборонені.");
    }

    protected static int FindSegment(IReadOnlyList<PointD> points, double x)
    {
        if (x < points[0].X || x > points[^1].X)
            throw new ArgumentOutOfRangeException(nameof(x));

        for (int i = 0; i < points.Count - 1; i++)
            if (x <= points[i + 1].X) return i;

        return points.Count - 2;
    }
}