using System.Windows.Media;
using Commons.Mathematics;
using Commons.Misc;

namespace SharedWpfControls.Objects
{
    public class AnnotationRunnerSettings
    {
        public string AnnotationRunnerId { get; set; }
        public AnnotationShapeType ShapeType { get; set; }
        public string Instruction { get; set; }
        public string InactiveButtonText { get; set; }
        public string ActiveButtonText { get; set; }
        public Brush ActiveButtonBrush { get; set; }
        public bool AutoFinishAnnotation { get; set; } = false;
        public Range<int> AnnotationPointCountRange { get; set; } = new Range<int>(0, int.MaxValue);
    }
}