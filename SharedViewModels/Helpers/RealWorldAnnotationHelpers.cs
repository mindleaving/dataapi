using System;
using System.Collections.Generic;
using System.Linq;
using Commons.CoordinateTransform;
using Commons.Extensions;
using Commons.Mathematics;
using Commons.Misc;

namespace SharedViewModels.Helpers
{
    public static class RealWorldAnnotationHelpers
    {
        public static IEnumerable<Circle> ConstructRealWorldCircles(
            IEnumerable<Annotation> circleAnnotations, 
            PinholeToPlaneProjection pinholeProjection)
        {
            foreach (var circleAnnotation in circleAnnotations)
            {
                yield return ConstructRealWorldCircle(circleAnnotation, pinholeProjection);
            }
        }

        public static Circle ConstructRealWorldCircle(
            Annotation circleAnnotation,
            PinholeToPlaneProjection pinholeProjection)
        {
            var realWorldPoints = TransformToRealWorld(circleAnnotation.Points, pinholeProjection).ToList();
            var centerPoint = CalculateCenterPoint(realWorldPoints);
            var radius = realWorldPoints.Average(p => p.DistanceTo(centerPoint));
            return new Circle(centerPoint, radius);
        }

        public static Annotation GetMatchingPolygon(
            List<Annotation> annotations,
            Circle realWorldCircle,
            PinholeToPlaneProjection pinholeProjection)
        {
            var cheesePolygons = annotations.Where(annotation => annotation.ShapeType == AnnotationShapeType.Polygon);
            foreach (var cheesePolygon in cheesePolygons)
            {
                var realWorldPoints = TransformToRealWorld(cheesePolygon.Points, pinholeProjection).ToList();
                var centerPoint = CalculateCenterPoint(realWorldPoints);
                if (centerPoint.DistanceTo(realWorldCircle.Center) < realWorldCircle.Radius)
                    return cheesePolygon;
            }
            throw new Exception("No matching cheese polygon found");
        }

        public static List<Point2D> GetMatchingRealWorldPolygon(
            List<Annotation> annotations, 
            Circle realWorldCircle, 
            PinholeToPlaneProjection pinholeProjection)
        {
            var matchingPolygon = GetMatchingPolygon(annotations, realWorldCircle, pinholeProjection);
            return TransformToRealWorld(matchingPolygon.Points, pinholeProjection).ToList();
        }

        public static Point2D CalculateCenterPoint(IReadOnlyCollection<Point2D> points)
        {
            if (points.Count == 4) // Assume that these are the 4 axis points
            {
                var firstPoint = points.First();
                var pointFurthestAway = points.Skip(1).MaximumItem(p => p.DistanceTo(firstPoint));
                var line1 = new LineSegment2D(new Point2D(firstPoint.X, firstPoint.Y),  new Point2D(pointFurthestAway.X, pointFurthestAway.Y));

                var otherPoints = points.Except(new[] {firstPoint, pointFurthestAway}).ToList();
                var line2 = new LineSegment2D(new Point2D(otherPoints[0].X, otherPoints[0].Y), new Point2D(otherPoints[1].X, otherPoints[1].Y));
                if(!line1.Intersects(line2, out var intersectionPoint))
                    throw new InvalidOperationException("Annotation points cannot be assembled to circle axes");
                return intersectionPoint;
            }
            return new Point2D(
                points.Average(p => p.X),
                points.Average(p => p.Y));
        }

        public static IEnumerable<Point2D> TransformToRealWorld(IEnumerable<Point2D> points, PinholeToPlaneProjection pinholeProjection)
        {
            return points.Select(pinholeProjection.Transform);
        }
    }
}