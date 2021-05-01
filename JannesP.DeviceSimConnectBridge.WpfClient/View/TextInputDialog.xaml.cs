using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JannesP.DeviceSimConnectBridge.WpfApp.View
{
    /// <summary>
    /// Interaction logic for TextInputDialog.xaml
    /// </summary>
    public partial class TextInputDialog : Window, INotifyPropertyChanged
    {
        private readonly Func<string, string> _validator;
        private string _inputText = "";
        private string? _validationError;

        public TextInputDialog(string title, string text, Func<string, string> validator)
        {
            DataContext = this;
            InitializeComponent();
            Title = title;
            DialogMessage = text;
            _validator = validator;
        }

        public string DialogMessage { get; } = "Design time message.";
        public string InputText
        {
            get => _inputText;
            set
            {
                if (_inputText != value)
                {
                    _inputText = value;
                    ValidationError = _validator.Invoke(value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InputText)));
                }
            }
        }

        public string? ValidationError
        {
            get => _validationError;
            set
            {
                if (_validationError != value)
                {
                    _validationError = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValidationError)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInputValid)));
                }
            }
        }

        public bool IsInputValid => ValidationError == null;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OkButton_Click(object sender, RoutedEventArgs e) => DialogResult = true;

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            base.Close();
        }
    }
}
