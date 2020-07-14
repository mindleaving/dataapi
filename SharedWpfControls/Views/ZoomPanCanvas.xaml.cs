using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Commons.Mathematics;
using SharedWpfControls.ViewModels;

namespace SharedWpfControls.Views
{
    /// <summary>
    /// Interaction logic for ZoomPanCanvas.xaml
    /// </summary>
    public partial class ZoomPanCanvas : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(ZoomPanCanvasViewModel), typeof(ZoomPanCanvas), new PropertyMetadata(default(ZoomPanCanvasViewModel)));

        public ZoomPanCanvas()
        {
            InitializeComponent();
        }

        public ZoomPanCanvasViewModel ViewModel
        {
            get { return (ZoomPanCanvasViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        private void Canvas_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var originalScale = ViewModel.CanvasScale;
            var newScale = originalScale * (1 + 0.1 * e.Delta / 120);

            var mousePosition = ToPoint2D(e.GetPosition(ItemsControl));
            var originalCanvasPosition = (1 / originalScale) * mousePosition;
            var newCanvasPosition = (1 / newScale) * mousePosition;
            var positionChange = newCanvasPosition - originalCanvasPosition;
            ViewModel.CanvasPan += positionChange;
            ViewModel.CanvasScale = newScale;
        }

        private bool isMoving;
        private Point moveStartPoint;
        private Point2D originalPan;
        private bool hasMovedSignificantly;
        private void ImageCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!isMoving)
                return;
            if (e.LeftButton != MouseButtonState.Pressed)
                StopPanning();
            var mousePosition = e.GetPosition(ItemsControl);
            var mouseDisplacement = mousePosition - moveStartPoint;
            if (mouseDisplacement.Length > 10)
                hasMovedSignificantly = true;
            var panChange = mouseDisplacement / ViewModel.CanvasScale;
            ViewModel.CanvasPan = originalPan + new Point2D(panChange.X, panChange.Y);
        }

        private void ImageCanvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartPanning(e);
        }

        private void StartPanning(MouseButtonEventArgs e)
        {
            isMoving = true;
            hasMovedSignificantly = false;
            var mousePosition = e.GetPosition(ItemsControl);
            moveStartPoint = mousePosition;
            originalPan = ViewModel.CanvasPan;
        }

        private void ImageCanvas_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StopPanning();
            if (!hasMovedSignificantly && ViewModel.IsAnnotationInProgress)
            {
                var firstClickPositionInCanvas = -1 * originalPan + (1 / ViewModel.CanvasScale) * ToPoint2D(moveStartPoint);
                ViewModel.ReportAnnotationPoint(firstClickPositionInCanvas);
            }
        }

        private void StopPanning()
        {
            isMoving = false;
        }

        private static Point2D ToPoint2D(Point point)
        {
            return new Point2D(point.X, point.Y);
        }
    }
}
