using System;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Input;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel;

namespace JannesP.DeviceSimConnectBridge.WpfApp.View.Windows
{
    /// <summary>
    /// Interaction logic for TextInputDialog.xaml
    /// </summary>
    public partial class TextInputDialog : Window
    {
        private readonly TextInputDialogViewModel _dataContext;

        public TextInputDialog(string title, string text, Func<object, ValidationResult?> validator)
        {
            _dataContext = new TextInputDialogViewModel(text, validator);
            DataContext = _dataContext;
            Title = title;
            InitializeComponent();
        }

        public string? Result { get; private set; }

        private void Cancel()
        {
            DialogResult = false;
            base.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => Cancel();

        private void Ok()
        {
            _dataContext.Validate();
            if (!_dataContext.HasErrors)
            {
                DialogResult = true;
                Result = _dataContext.InputText;
                base.Close();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) => Ok();

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    e.Handled = true;
                    Ok();
                    break;

                case Key.Escape:
                    e.Handled = true;
                    Cancel();
                    break;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    e.Handled = true;
                    Cancel();
                    break;
            }
        }
    }

    public class TextInputDialogViewModel : ViewModelBase
    {
        private readonly Func<object, ValidationResult?> _validationFunction;
        private string _inputText = "";

        public TextInputDialogViewModel(string text, Func<object, ValidationResult?> validator)
        {
            DialogMessage = text;
            _validationFunction = validator;
            ValidateAsync();
        }

        public string DialogMessage { get; } = "Design time message.";

        [CustomValidation(typeof(TextInputDialogViewModel), nameof(ExecuteCustomValidation))]
        public string InputText
        {
            get => _inputText;
            set
            {
                if (_inputText != value)
                {
                    _inputText = value;
                    OnPropertyChanged();
                }
            }
        }

        public static ValidationResult? ExecuteCustomValidation(string value, ValidationContext context)
        {
            var instance = (TextInputDialogViewModel)context.ObjectInstance;
            ValidationResult? result = instance._validationFunction?.Invoke(value);
            if (result != null && context.MemberName != null)
            {
                result = new ValidationResult(result.ErrorMessage, new[] { context.MemberName });
            }
            return result;
        }
    }
}