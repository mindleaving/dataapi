using System.Windows.Shapes;
using Commons.Misc;

namespace SharedWpfControls.ViewModels
{
    public class AnnotationWithShape
    {
        public AnnotationWithShape(Annotation annotation, Shape shape, string annotationRunnerId)
        {
            Annotation = annotation;
            Shape = shape;
            AnnotationRunnerId = annotationRunnerId;
        }

        public Annotation Annotation { get; }
        public Shape Shape { get; }
        public string AnnotationRunnerId { get; }
    }
}