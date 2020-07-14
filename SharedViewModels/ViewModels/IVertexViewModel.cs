using System;
using System.ComponentModel;
using System.Windows.Input;
using Commons.Mathematics;

namespace SharedViewModels.ViewModels
{
    public interface IVertexViewModel<out TVertex> : INotifyPropertyChanged
    {
        uint VertexId { get; }
        string GlobalVertexId { get; }
        TVertex Object { get; }

        Point2D Center { get; }
        void SetPosition(double x, double y);
        double Width { get; }
        double Height { get; }

        string DisplayName { get; }
        bool IsSelected { get; set; }
        ICommand DeleteCommand { get; }
        ICommand ToggleSelectedCommand { get; }
        event EventHandler RequestDelete;
    }
}
