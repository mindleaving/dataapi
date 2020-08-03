using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Commons.CoordinateTransform;
using Commons.Extensions;
using Commons.Mathematics;
using Commons.Misc;
using SharedViewModels.ViewModels;
using SharedWpfControls.Helpers;
using SharedWpfControls.Objects;

namespace SharedWpfControls.ViewModels
{
    public class ImageAnnotationViewModel : NotifyPropertyChangedBase, IDisposable
    {
        private DistanceMeasurementAnnotationRunner distanceMeasurementAnnotationRunner;
        private Size imageSize = new Size(1000, 800);

        public ImageAnnotationViewModel(params IAnnotationRunner[] annotationRunners)
        {
            CanvasViewModel = new ZoomPanCanvasViewModel();
            CanvasViewModel.AnnotationPointAdded += AddAnnotationPoint;
            CanvasViewModel.AnnotationPointRemoved += RemoveAnnotationPoint;
            annotationRunners.ForEach(AnnotationRunners.Add);
            AnnotationRunners.ForEach(annotationRunner => annotationRunner.AnnotationFinished += AnnotationHasFinished);
            StartAnnotationCommand = new RelayCommand<IAnnotationRunner>(InteractWithAnnotationRunner, CanInteractWithAnnotationRunner);
            CancelAnnotationCommand = new RelayCommand(CancelAnnotation, CanCancelAnnotation);
            DeleteSelectedAnnotationCommand = new RelayCommand(DeleteSelectedAnnotation);
        }

        public ZoomPanCanvasViewModel CanvasViewModel { get; }

        private Calibration calibration;
        public Calibration Calibration
        {
            get => calibration;
            set
            {
                calibration = value;
                if(calibration != null)
                    CreateDistanceMeasurementAnnotationRunner(calibration);
                OnPropertyChanged();
            }
        }

        public void SetImage(string imagePath, bool clearAnnotations = true)
        {
            var bitmapImage = new BitmapImage(new Uri(imagePath));
            SetImage(bitmapImage, clearAnnotations);
        }

        public void SetImage(byte[] imageData, bool clearAnnotations = true)
        {
            var bitmapSource = BitmapLoader.FromByteArray(imageData);
            SetImage(bitmapSource, clearAnnotations);
        }

        public void SetImage(BitmapSource bitmapSource, bool clearAnnotations = true)
        {
            var dpiNormalizedBitmapImage = DpiNormalizer.Normalize(bitmapSource);
            var image = new Image {Source = dpiNormalizedBitmapImage};
            imageSize = new Size(bitmapSource.PixelWidth, bitmapSource.PixelHeight);
            CanvasViewModel.SetImage(image, clearAnnotations);
            if(clearAnnotations)
                Annotations.Clear();
        }

        /// <summary>
        /// Adds annotations. NOTE: Does not trigger <see cref="AnnotationAdded"/> nor <see cref="AnnotationRemoved"/> events.
        /// </summary>
        public void AddAnnotations(IEnumerable<Annotation> annotations)
        {
            var annotationsWithShape = annotations
                .Select(annotation => new AnnotationWithShape(
                    annotation, 
                    ShapeGenerator.Generate(annotation, imageSize, Calibration),
                    null))
                .ToList();
            annotationsWithShape.ForEach(Annotations.Add);
            annotationsWithShape.ForEach(x => CanvasViewModel.AnnotationShapes.Add(x.Shape));
        }

        public bool IsAnnotationInProgress { get; private set; }
        public ObservableCollection<IAnnotationRunner> AnnotationRunners { get; } = new ObservableCollection<IAnnotationRunner>();
        public ObservableCollection<AnnotationWithShape> Annotations { get; } = new ObservableCollection<AnnotationWithShape>();
        private Brush previousSelectedShapeBrush;
        private AnnotationWithShape selectedAnnotation;
        public AnnotationWithShape SelectedAnnotation
        {
            get => selectedAnnotation;
            set
            {
                if(selectedAnnotation != null)
                {
                    selectedAnnotation.Shape.Stroke = previousSelectedShapeBrush;
                }
                selectedAnnotation = value;
                OnPropertyChanged();
                if(selectedAnnotation == null)
                    return;
                previousSelectedShapeBrush = selectedAnnotation.Shape.Stroke;
                selectedAnnotation.Shape.Stroke = Brushes.Turquoise;
            }
        }

        public event EventHandler<Annotation> AnnotationAdded;
        public event EventHandler<Annotation> AnnotationRemoved;

        private string instruction;
        public string Instruction
        {
            get => instruction;
            private set
            {
                instruction = value;
                OnPropertyChanged();
            }
        }
        private Brush instructionBrush = Brushes.Black;
        public Brush InstructionBrush
        {
            get => instructionBrush;
            private set
            {
                instructionBrush = value;
                OnPropertyChanged();
            }
        }

        public ICommand StartAnnotationCommand { get; }
        public ICommand CancelAnnotationCommand { get; }
        public ICommand DeleteSelectedAnnotationCommand { get; }
        private IAnnotationRunner ActiveAnnotationRunner { get; set; }

        private void AddAnnotationPoint(object sender, Point2D point)
        {
            if (!IsAnnotationInProgress)
                return;
            if (!ActiveAnnotationRunner.AddPoint(point))
                CanvasViewModel.RemoveAnnotationPoint(point);
        }

        private void RemoveAnnotationPoint(object sender, Point2D point)
        {
            if(!IsAnnotationInProgress)
                return;
            ActiveAnnotationRunner.RemovePoint(point);
        }

        private bool CanInteractWithAnnotationRunner(IAnnotationRunner runner)
        {
            if (!CanvasViewModel.IsImageSet)
                return false;
            return !IsAnnotationInProgress || runner.Equals(ActiveAnnotationRunner);
        }

        private void InteractWithAnnotationRunner(IAnnotationRunner runner)
        {
            if (IsAnnotationInProgress)
            {
                runner.ButtonPressed();
            }
            else
            {
                ActiveAnnotationRunner = runner;
                Instruction = ActiveAnnotationRunner.Instruction;
                IsAnnotationInProgress = true;
                CanvasViewModel.StartAnnotation();
                ActiveAnnotationRunner.StartAnnotation();
            }
        }

        private bool CanCancelAnnotation()
        {
            return IsAnnotationInProgress;
        }

        private void CancelAnnotation()
        {
            GetReadyForNewAnnotation();
        }

        private void AnnotationHasFinished(object sender, Annotation annotation)
        {
            FinishAnnotation(annotation);
        }

        private void FinishAnnotation(Annotation annotation)
        {
            var annotationShape = ShapeGenerator.Generate(annotation, imageSize, Calibration);
            CanvasViewModel.AnnotationShapes.Add(annotationShape);
            Annotations.Add(new AnnotationWithShape(annotation, annotationShape, ActiveAnnotationRunner?.Id));
            AnnotationAdded?.Invoke(this, annotation);

            GetReadyForNewAnnotation();
        }

        private void GetReadyForNewAnnotation()
        {
            IsAnnotationInProgress = false;
            Instruction = string.Empty;
            ActiveAnnotationRunner.Cancel();
            ActiveAnnotationRunner = null;
            CanvasViewModel.StopAnnotation();
        }

        private void DeleteSelectedAnnotation()
        {
            if(SelectedAnnotation == null)
                return;
            var annotation = SelectedAnnotation.Annotation;
            CanvasViewModel.AnnotationShapes.Remove(SelectedAnnotation.Shape);
            Annotations.Remove(SelectedAnnotation);
            AnnotationRemoved?.Invoke(this, annotation);
        }

        private void CreateDistanceMeasurementAnnotationRunner(Calibration calibration)
        {
            if (distanceMeasurementAnnotationRunner != null)
            {
                AnnotationRunners.Remove(distanceMeasurementAnnotationRunner);
                distanceMeasurementAnnotationRunner.AnnotationFinished -= AnnotationHasFinished;
            }

            distanceMeasurementAnnotationRunner = new DistanceMeasurementAnnotationRunner(
                new AnnotationRunnerSettings
                {
                    ShapeType = AnnotationShapeType.Line,
                    AutoFinishAnnotation = true,
                    AnnotationPointCountRange = new Range<int>(2, 2),
                    InactiveButtonText = "Measure distance",
                    Instruction = "Click on two image points get their distance"
                },
                calibration);
            distanceMeasurementAnnotationRunner.AnnotationFinished += AnnotationHasFinished;
            AnnotationRunners.Add(distanceMeasurementAnnotationRunner);
        }

        public void Dispose()
        {
            AnnotationRunners.ForEach(annotationRunner => annotationRunner.AnnotationFinished -= AnnotationHasFinished);
        }
    }
}