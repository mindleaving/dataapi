using System;
using Commons.CoordinateTransform;
using Commons.Extensions;
using Commons.Misc;
using SharedViewModels.Helpers;
using SharedWpfControls.Objects;

namespace SharedWpfControls.ViewModels
{
    public class DistanceMeasurementAnnotationRunner : AnnotationRunnerBase
    {
        private readonly PinholeToPlaneProjection pinholeToPlaneProjection;

        public DistanceMeasurementAnnotationRunner(AnnotationRunnerSettings settings, Calibration calibration)
            : base(settings)
        {
            if(settings.ShapeType != AnnotationShapeType.Line)
            {
                throw new ArgumentException(
                    $"Annotation shape type must be '{nameof(AnnotationShapeType.Line)}' " +
                    $"for {nameof(DistanceMeasurementAnnotationRunner)}");
            }
            pinholeToPlaneProjection = new PinholeToPlaneProjection(calibration);
            AnnotationFinished += PerformMeasurement;
        }

        private void PerformMeasurement(object sender, Annotation annotation)
        {
            if(annotation.Points.Count != 2)
                return;
            var imagePoint1 = annotation.Points[0];
            var imagePoint2 = annotation.Points[1];
            var realWorldPoint1 = pinholeToPlaneProjection.Transform(imagePoint1);
            var realWorldPoint2 = pinholeToPlaneProjection.Transform(imagePoint2);
            var distance = realWorldPoint1.DistanceTo(realWorldPoint2);
            StaticMessageBoxSpawner.Show(
                $"Distance: {distance:F2} mm{Environment.NewLine}"
                + $"{Environment.NewLine}"
                + $"Point 1:  ({imagePoint1.X:F1} px, {imagePoint1.Y:F1} px)  =>  ({realWorldPoint1.X:F2} mm, {realWorldPoint1.Y:F2} mm){Environment.NewLine}"
                + $"Point 2:  ({imagePoint2.X:F1} px, {imagePoint2.Y:F1} px)  =>  ({realWorldPoint2.X:F2} mm, {realWorldPoint2.Y:F2} mm)");
        }
    }
}
