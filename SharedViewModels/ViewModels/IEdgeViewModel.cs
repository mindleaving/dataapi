using System.ComponentModel;
using Commons.Mathematics;

namespace SharedViewModels.ViewModels
{
    public interface IEdgeViewModel<out TEdge> : INotifyPropertyChanged
    {
        string GlobalEdgeId { get; }
        TEdge Object { get; }
        
        Point2D Point1 { get; set; }
        Point2D Point2 { get; set; }
    }
}
