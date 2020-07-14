using System.Windows.Controls;
using SharedViewModels.Objects;

namespace SharedWpfControls.Objects
{
    public class WpfPasswordBox : IPasswordBox
    {
        public WpfPasswordBox(int tabIndex = 1)
        {
            TabIndex = tabIndex;
        }

        public PasswordBox PasswordBox { get; } = new PasswordBox();

        public string Password
        {
            get => PasswordBox.Password;
            set => PasswordBox.Password = value;
        }
        public int TabIndex { get; set; }
    }
}