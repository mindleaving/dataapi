using System;
using System.ComponentModel;
using System.Windows.Media;
using Commons.Mathematics;
using Commons.Misc;

namespace SharedWpfControls.ViewModels
{
    public interface IAnnotationRunner : INotifyPropertyChanged
    {
        string Id { get; }
        string Instruction { get; }
        string ButtonText { get; }
        Brush ButtonBackground { get; }
        bool IsActivated { get; }
        void StartAnnotation();
        bool AddPoint(Point2D point);
        void RemovePoint(Point2D point);
        void ButtonPressed();
        event EventHandler<Annotation> AnnotationFinished;
        void Cancel();
    }
}