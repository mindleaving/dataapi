namespace SharedViewModels.ViewModels
{
    public class TextParameterViewModel : NotifyPropertyChangedBase, IParameterValueViewModel
    {
        public TextParameterViewModel(string text = null)
        {
            Text = text;
        }

        private string text;
        public string Text
        {
            get => text;
            set
            {
                text = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Value));
            }
        }

        public string Value
        {
            get => Text;
            set => Text = value;
        }
    }
}
