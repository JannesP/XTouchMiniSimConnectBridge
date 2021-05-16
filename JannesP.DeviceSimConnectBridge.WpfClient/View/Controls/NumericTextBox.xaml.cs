using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JannesP.DeviceSimConnectBridge.WpfApp.View.Controls
{
    /// <summary>
    /// Interaction logic for NumericTextBox.xaml
    /// </summary>
    public partial class NumericTextBox : TextBox
    {
        public static readonly DependencyProperty NumericValueProperty = DependencyProperty.Register(nameof(NumericValue), typeof(int?), typeof(NumericTextBox), new PropertyMetadata(0, new PropertyChangedCallback(OnNumericValueChanged)));

        public NumericTextBox()
        {
            InitializeComponent();
        }

        public int? NumericValue { get => GetValue(NumericValueProperty) as int?; set => SetValue(NumericValueProperty, value); }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out _))
            {
                e.Handled = true;
            }
            base.OnPreviewTextInput(e);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            if (NumericValue?.ToString() != base.Text)
            {
                NumericValue = GetValueFromString(base.Text);
            }
        }

        private static int GetValueFromString(string text)
        {
            return int.TryParse(text, out int result) ? result : 0;
        }

        private static void OnNumericValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tb = (NumericTextBox)d;
            string? s = e.NewValue?.ToString();
            if (s != tb.Text)
            {
                tb.Text = s;
            }
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                if (!int.TryParse((string)e.DataObject.GetData(typeof(string)), out _))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}