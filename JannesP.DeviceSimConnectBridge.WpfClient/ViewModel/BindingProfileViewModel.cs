using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.WpfApp.Options;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel
{
    public interface IBindingProfileViewModel
    {
        string? Name { get; set; }
        Guid? UniqueId { get; }
        bool IsCurrent { get; }
    }

    public class DesignTimeBindingProfileViewModel : IBindingProfileViewModel
    {
        private static int _mockNum = 0;

        public string? Name { get; set; } = $"DesignTime Profile {++_mockNum}";
        public Guid? UniqueId { get; } = Guid.NewGuid();
        public bool IsCurrent { get; set; } = false;
    }

    public class BindingProfileViewModel : ViewModelBase, IBindingProfileViewModel
    {
        private readonly BindingProfile _model;

        public BindingProfileViewModel(BindingProfile model)
        {
            _model = model;
            _name = model.Name;
        }

        public Guid? UniqueId => _model.UniqueId;

        private string? _name;
        private bool _isCurrent = false;

        public string? Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }

            }
        }

        public bool IsCurrent
        {
            get => _isCurrent; 
            set
            {
                if (_isCurrent != value)
                {
                    _isCurrent = value;
                    OnPropertyChanged();
                }
                
            }
        }
    }
}
