using System;
using Commons.Misc;
using SharedWpfControls.Objects;

namespace SharedWpfControls.Helpers
{
    public static class AnnotationRunnerSettingsValidator
    {
        public static void ValidateSettings(AnnotationRunnerSettings settings)
        {
            if (settings.ShapeType == AnnotationShapeType.Point)
            {
                if(!settings.AutoFinishAnnotation)
                    throw new ArgumentException("Point annotations must always be auto-finished");
                if (settings.AnnotationPointCountRange.From != 1
                    || settings.AnnotationPointCountRange.To != 1)
                {
                    throw new ArgumentException($"Point count must be 1 for shape '{nameof(AnnotationShapeType.Point)}'");
                }
            }
            if (settings.ShapeType == AnnotationShapeType.Line)
            {
                if (!settings.AutoFinishAnnotation)
                    throw new ArgumentException("Line annotations must always be auto-finished");
                if (settings.AnnotationPointCountRange.From != 2
                    || settings.AnnotationPointCountRange.To != 2)
                {
                    throw new ArgumentException($"Point count must be 2 for shape '{nameof(AnnotationShapeType.Line)}'");
                }
            }
        }
    }
}