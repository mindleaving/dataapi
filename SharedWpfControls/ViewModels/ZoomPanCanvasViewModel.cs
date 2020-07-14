using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Commons.Extensions;
using Commons.Mathematics;
using SharedViewModels.ViewModels;

namespace SharedWpfControls.ViewModels
{
    public class ZoomPanCanvasViewModel : NotifyPropertyChangedBase
    {
        public event EventHandler<Point2D> AnnotationPointAdded;
        public event EventHandler<Point2D> AnnotationPointRemoved;

        public bool IsImageSet { get; private set; }
        public void SetImage(Image image, bool clearAnnotations = true)
        {
            if(clearAnnotations)
                ClearCanvas();
            Canvas.SetLeft(image, 0);
            Canvas.SetTop(image, 0);
            var existingImage = AnnotationShapes.Where(x => x is Image).ToList();
            existingImage.ForEach(x => AnnotationShapes.Remove(x));
            AnnotationShapes.Insert(0, image);

            CanvasScale = 800.0 / Math.Max(image.Source.Width, image.Source.Height);
            CanvasPan = new Point2D(0, 0);
            IsImageSet = true;
        }

        public void ClearCanvas()
        {
            AnnotationShapes.Clear();
            IsImageSet = false;
        }

        public void DrawPoint(Point2D point, out Shape shape)
        {
            var annotationRectangleSize = CalculateAnnotationRectangleSize();
            shape = new Rectangle
            {
                Width = annotationRectangleSize,
                Height = annotationRectangleSize,
                StrokeThickness = 0.2 * annotationRectangleSize,
                Stroke = Brushes.LawnGreen
            };
            Canvas.SetLeft(shape, point.X - annotationRectangleSize / 2);
            Canvas.SetTop(shape, point.Y - annotationRectangleSize / 2);
            AnnotationShapes.Add(shape);
        }

        private double canvasScale = 1;
        public double CanvasScale
        {
            get => canvasScale;
            set
            {
                canvasScale = value;
                BuildCombinedCanvasTransform();
                UpdateAnnotationRectangles();
            }
        }

        private Point2D canvasPan = new Point2D(0, 0);
        public Point2D CanvasPan
        {
            get => canvasPan;
            set
            {
                canvasPan = value;
                BuildCombinedCanvasTransform();
            }
        }

        private Transform canvasTransform = new TransformGroup();
        public Transform CanvasTransform
        {
            get { return canvasTransform; }
            set
            {
                canvasTransform = value;
                OnPropertyChanged();
            }
        }

        private void BuildCombinedCanvasTransform()
        {
            CanvasTransform = new TransformGroup
            {
                Children = new TransformCollection(new Transform[]
                {
                    new TranslateTransform(CanvasPan.X, CanvasPan.Y),
                    new ScaleTransform(CanvasScale, CanvasScale)
                })
            };
        }

        private readonly List<ShapePoint> inProgressAnnotationShapePoints = new List<ShapePoint>();
        public ObservableCollection<object> AnnotationShapes { get; } = new ObservableCollection<object>();

        public bool IsAnnotationInProgress { get; private set; }
        public void StartAnnotation()
        {
            IsAnnotationInProgress = true;
        }
        public void StopAnnotation()
        {
            inProgressAnnotationShapePoints.ForEach(x => AnnotationShapes.Remove(x.Shape));
            inProgressAnnotationShapePoints.Clear();
            IsAnnotationInProgress = false;
        }

        public void ReportAnnotationPoint(Point2D point)
        {
            if(!IsAnnotationInProgress)
                return;
            if (!inProgressAnnotationShapePoints.Any())
            {
                AddAnnotationShape(point);
                return;
            }

            // Check if another annotation point is nearby and remove it
            var closestAnnotationPoint = EnumerableExtensions.MinimumItem(inProgressAnnotationShapePoints, p => Point2DExtensions.DistanceTo(p.Point, point));
            var distance = Point2DExtensions.DistanceTo(closestAnnotationPoint.Point, point);
            var distanceThreshold = 50 / CanvasScale;
            if (distance < distanceThreshold)
            {
                RemoveAnnotationShape(closestAnnotationPoint);
            }
            else
            {
                AddAnnotationShape(point);
            }            
        }

        private void AddAnnotationShape(Point2D point)
        {
            DrawPoint(point, out var rectangle);
            var shapePoint = new ShapePoint
            {
                Point = point,
                Shape = rectangle
            };
            inProgressAnnotationShapePoints.Add(shapePoint);
            AnnotationPointAdded?.Invoke(this, point);
        }

        public void RemoveAnnotationPoint(Point2D point)
        {
            var shapePoint = inProgressAnnotationShapePoints.SingleOrDefault(x => x.Point == point);
            if(shapePoint == null)
                return;
            RemoveAnnotationShape(shapePoint);
        }

        private void RemoveAnnotationShape(ShapePoint shapePoint)
        {
            inProgressAnnotationShapePoints.Remove(shapePoint);
            AnnotationShapes.Remove(shapePoint.Shape);
            AnnotationPointRemoved?.Invoke(this, shapePoint.Point);
        }

        private double CalculateAnnotationRectangleSize()
        {
            return 10 / CanvasScale;
        }

        private void UpdateAnnotationRectangles()
        {
            var annotationRectangleSize = CalculateAnnotationRectangleSize();
            foreach (var annotationPoint in inProgressAnnotationShapePoints)
            {
                var shape = annotationPoint.Shape;
                shape.Width = annotationRectangleSize;
                shape.Height = annotationRectangleSize;
                shape.StrokeThickness = 0.2 * annotationRectangleSize;
                Canvas.SetLeft(shape, annotationPoint.Point.X - annotationRectangleSize / 2);
                Canvas.SetTop(shape, annotationPoint.Point.Y - annotationRectangleSize / 2);
            }
        }

        private class ShapePoint
        {
            public Shape Shape { get; set; }
            public Point2D Point { get; set; }
        }
    }
}