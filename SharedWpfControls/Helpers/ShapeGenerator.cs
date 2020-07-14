using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Commons.CoordinateTransform;
using Commons.Extensions;
using Commons.Mathematics;
using Commons.Misc;
using Commons.Physics;
using SharedViewModels.Helpers;

namespace SharedWpfControls.Helpers
{
    public static class ShapeGenerator
    {
        private static double GetLineThickness(Size imageSize) => 0.005 * Math.Min(imageSize.Height, imageSize.Width);

        public static Shape Generate(Annotation annotation, Size imageSize, Calibration calibration = null)
        {
            var lineThickness = GetLineThickness(imageSize);
            switch (annotation.ShapeType)
            {
                case AnnotationShapeType.Point:
                    return GeneratePointMarker(annotation, lineThickness);
                case AnnotationShapeType.Line:
                    return GenerateLine(annotation, lineThickness);
                case AnnotationShapeType.PolyLine:
                    return GeneratePolyLine(annotation, lineThickness);
                case AnnotationShapeType.Ellipse:
                    if (calibration != null)
                        return GenerateCircleWithCalibration(annotation, calibration, lineThickness);
                    return GenerateCircleWithoutCalibration(annotation, lineThickness);
                case AnnotationShapeType.Rectangle:
                case AnnotationShapeType.Polygon:
                    return GeneratePolygon(annotation, lineThickness);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Shape GeneratePointMarker(Annotation annotation, double lineThickness)
        {
            var point = annotation.Points.Single();
            var rectangle = new Rectangle
            {
                Width = 10,
                Height = 10,
                Stroke = Brushes.LawnGreen,
                StrokeThickness = lineThickness
            };
            Canvas.SetLeft(rectangle, point.X - rectangle.Width / 2);
            Canvas.SetTop(rectangle, point.Y - rectangle.Height / 2);
            return rectangle;
        }

        private static Shape GenerateLine(Annotation annotation, double lineThickness)
        {
            var line = new Line
            {
                X1 = annotation.Points[0].X,
                Y1 = annotation.Points[0].Y,
                X2 = annotation.Points[1].X,
                Y2 = annotation.Points[1].Y,
                Stroke = Brushes.Red,
                StrokeThickness = lineThickness
            };
            return line;
        }

        private static Shape GeneratePolyLine(Annotation annotation, double lineThickness)
        {
            var line = new Polyline
            {
                Points = new PointCollection(annotation.Points.Select(p => new Point(p.X, p.Y))),
                Stroke = Brushes.Red,
                StrokeThickness = lineThickness
            };
            return line;
        }

        private static Shape GenerateCircleWithCalibration(Annotation annotation, Calibration calibration, double lineThickness)
        {
            var pinholeProjection = new PinholeToPlaneProjection(calibration);
            var realWorldCircle = RealWorldAnnotationHelpers.ConstructRealWorldCircles(new []{ annotation }, pinholeProjection).Single();
            var imageCirclePoints = new PointCollection(100);
            foreach (var angle in SequenceGeneration.Linspace(-Math.PI, Math.PI, 100))
            {
                var realWorldCirclePoint = realWorldCircle.Center + new Vector2D(
                                               realWorldCircle.Radius * Math.Cos(angle),
                                               realWorldCircle.Radius * Math.Sin(angle));
                var imageCirclePoint = pinholeProjection.InverseTransform(realWorldCirclePoint);
                imageCirclePoints.Add(new Point(imageCirclePoint.X, imageCirclePoint.Y));
            }
            var polygon = new Polygon
            {
                Points = imageCirclePoints,
                Stroke = Brushes.Red,
                StrokeThickness = lineThickness
            };
            return polygon;
        }

        private static Shape GenerateCircleWithoutCalibration(Annotation annotation, double lineThickness)
        {
            var xOrderedAnnotationPoints = annotation.Points
                .OrderBy(p => p.X)
                .ToList();
            var firstAxisVector = xOrderedAnnotationPoints.First().VectorTo(xOrderedAnnotationPoints.Last());
            var secondAxisVector = xOrderedAnnotationPoints[1].VectorTo(xOrderedAnnotationPoints[2]);
            var firstAxisRotation = Math.Atan2(firstAxisVector.Y, firstAxisVector.X).To(Unit.Radians);
            var circleCenter = new Point2D(
                xOrderedAnnotationPoints.Average(p => p.X),
                xOrderedAnnotationPoints.Average(p => p.Y));
            var circle = new Ellipse
            {
                Width = firstAxisVector.Magnitude(),
                Height = secondAxisVector.Magnitude(),
                RenderTransform =
                    new RotateTransform(firstAxisRotation.In(Unit.Degree), firstAxisVector.X / 2, firstAxisVector.Y / 2),
                Stroke = Brushes.Red,
                StrokeThickness = lineThickness
            };
            Canvas.SetLeft(circle, circleCenter.X - circle.Width / 2);
            Canvas.SetTop(circle, circleCenter.Y - circle.Height / 2);
            return circle;
        }

        private static Shape GeneratePolygon(Annotation annotation, double lineThickness)
        {
            var centerPoint = new Point2D(
                annotation.Points.Average(p => p.X),
                annotation.Points.Average(p => p.Y));
            var orderedPoints = annotation.Points
                .OrderBy(p => Math.Atan2(p.Y - centerPoint.Y, p.X - centerPoint.X))
                .ToList();
            var polygon = new Polygon
            {
                Points = new PointCollection(orderedPoints.Select(p => new Point(p.X, p.Y))),
                Stroke = Brushes.DodgerBlue,
                StrokeThickness = lineThickness
            };
            return polygon;
        }
    }
}