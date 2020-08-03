using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Commons.Mathematics;
using Commons.Misc;
using SharedViewModels.Helpers;
using SharedViewModels.ViewModels;
using SharedWpfControls.Helpers;
using SharedWpfControls.Objects;

namespace SharedWpfControls.ViewModels
{
    public abstract class AnnotationRunnerBase : NotifyPropertyChangedBase, IAnnotationRunner
    {
        public const string AnnotationRunnerIdKey = "AnnotationRunnerId";

        private static readonly Color DefaultButtonBackground = Color.FromRgb(0xdd, 0xdd, 0xdd);

        protected readonly AnnotationRunnerSettings settings;
        protected readonly List<Point2D> annotationPoints = new List<Point2D>();

        protected AnnotationRunnerBase(AnnotationRunnerSettings settings)
        {
            this.settings = settings;
            AnnotationRunnerSettingsValidator.ValidateSettings(settings);
            Id = settings.AnnotationRunnerId;
            ButtonText = settings.InactiveButtonText;
            Instruction = settings.Instruction;
        }

        public string Id { get; }
        public string Instruction { get; }
        private string buttonText;
        public string ButtonText
        {
            get => buttonText;
            protected set
            {
                buttonText = value;
                OnPropertyChanged();
            }
        }

        private Brush buttonBackground = new SolidColorBrush(DefaultButtonBackground);
        public Brush ButtonBackground
        {
            get => buttonBackground;
            protected set
            {
                buttonBackground = value;
                OnPropertyChanged();
            }
        }

        public bool IsActivated { get; protected set; }
        public void StartAnnotation()
        {
            IsActivated = true;
            annotationPoints.Clear();
            if (settings.ActiveButtonText != null)
                ButtonText = settings.ActiveButtonText;
            if(settings.ActiveButtonBrush != null)
                ButtonBackground = settings.ActiveButtonBrush;
        }

        public bool AddPoint(Point2D point)
        {
            if (!IsActivated)
                return false;
            if (annotationPoints.Count >= settings.AnnotationPointCountRange.To)
            {
                StaticMessageBoxSpawner.Show("Cannot add more points to annotation. Maximum reached");
                return false;
            }
            annotationPoints.Add(point);
            if(settings.AutoFinishAnnotation 
               && annotationPoints.Count >= settings.AnnotationPointCountRange.From)
            {
                FinishAnnotation();
            }
            return true;
        }

        public void RemovePoint(Point2D point)
        {
            if(!IsActivated)
                return;
            annotationPoints.Remove(point);
        }

        public void ButtonPressed()
        {
            if(!IsActivated)
                return;
            if(settings.AutoFinishAnnotation)
                return;
            if (annotationPoints.Count < settings.AnnotationPointCountRange.From)
            {
                StaticMessageBoxSpawner.Show("Not enough annotation points yet");
                return;
            }
            FinishAnnotation();
        }

        private void FinishAnnotation()
        {
            var annotation = new Annotation(annotationPoints.ToList(), settings.ShapeType);
            annotation.AdditionalInformation.Add(AnnotationRunnerIdKey, Id);
            OnAnnotationFinished(annotation);
            Reset();
        }

        public void Cancel()
        {
            Reset();
        }

        private void Reset()
        {
            IsActivated = false;
            ButtonText = settings.InactiveButtonText;
            ButtonBackground = new SolidColorBrush(DefaultButtonBackground);
        }

        public event EventHandler<Annotation> AnnotationFinished;
        private void OnAnnotationFinished(Annotation annotation)
        {
            AnnotationFinished?.Invoke(this, annotation);
        }
    }
}