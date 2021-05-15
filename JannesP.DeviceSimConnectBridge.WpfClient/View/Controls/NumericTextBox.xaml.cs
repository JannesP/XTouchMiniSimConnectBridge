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
        public NumericTextBox()
        {
            InitializeComponent();
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out _))
            {
                e.Handled = true;
            }
            base.OnPreviewTextInput(e);
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