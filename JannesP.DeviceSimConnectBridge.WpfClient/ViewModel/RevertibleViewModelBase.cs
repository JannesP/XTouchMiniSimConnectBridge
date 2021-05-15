using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.WpfApp.Extensions;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel
{
    public abstract class RevertibleViewModelBase : ViewModelBase
    {
        private bool _isRevertingChanges = false;
        private bool _isTouched;

        protected RevertibleViewModelBase()
        {
            Children = new ObservableCollection<RevertibleViewModelBase>();
            Children.CollectionChanged += Children_CollectionChanged;
        }

        public bool IsTouched
        {
            get => _isTouched;
            private set
            {
                if (_isTouched != value)
                {
                    _isTouched = value;
                    OnPropertyChanged(true);
                }
            }
        }

        protected ObservableCollection<RevertibleViewModelBase> Children { get; }
        protected void AddChildren(params RevertibleViewModelBase[] revertibleViewModels) => AddChildren(revertibleViewModels.AsEnumerable());
        protected void AddChildren(IEnumerable<RevertibleViewModelBase> revertibleViewModels)
        {
            foreach (RevertibleViewModelBase vm in revertibleViewModels)
            {
                Children.Add(vm);
            }
        }

        protected void RemoveChildren(params RevertibleViewModelBase[] revertibleViewModels) => RemoveChildren(revertibleViewModels.AsEnumerable());
        protected void RemoveChildren(IEnumerable<RevertibleViewModelBase> revertibleViewModels)
        {
            foreach (RevertibleViewModelBase vm in revertibleViewModels)
            {
                Children.Remove(vm);
            }
        }

        public void ApplyChanges()
        {
            OnChildrenApplyChanges();
            OnApplyChanges();
            OnEndApplyChanged();
        }

        protected abstract void OnApplyChanges();

        protected virtual void OnChildrenApplyChanges()
        {
            foreach (RevertibleViewModelBase child in Children.Cast<RevertibleViewModelBase>())
            {
                child.ApplyChanges();
            }
        }

        protected virtual void OnEndApplyChanged()
        {
            IsTouched = false;
        }

        public void RevertChanges()
        {
            OnBeginRevertChanges();
            OnChildrenRevertChanges();
            OnRevertChanges();
            OnEndRevertChanges();
        }

        protected virtual void OnBeginRevertChanges()
        {
            _isRevertingChanges = true;
        }
        protected abstract void OnRevertChanges();

        protected virtual void OnChildrenRevertChanges()
        {
            foreach (RevertibleViewModelBase child in Children.Cast<RevertibleViewModelBase>())
            {
                child.RevertChanges();
            }
        }
        protected virtual void OnEndRevertChanges()
        {
            IsTouched = false;
            _isRevertingChanges = false;
        }

        protected override void OnPropertyChanged(bool skipValidation, [CallerMemberName] string? propertyName = null)
        {
            base.OnPropertyChanged(skipValidation, propertyName);
            if (!_isRevertingChanges)
            {
                IsTouched = true;
            }
        }
        private void Child_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsTouched) && (sender?.GetPropertyValueByName<bool>(e.PropertyName)) == true)
            {
                IsTouched = true;
            }
        }

        private void Children_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (object childObj in e.OldItems)
                {
                    if (childObj is RevertibleViewModelBase child)
                    {
                        child.PropertyChanged -= Child_PropertyChanged;
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (object childObj in e.NewItems)
                {
                    if (childObj is RevertibleViewModelBase child)
                    {
                        child.PropertyChanged += Child_PropertyChanged;
                    }
                }
            }
        }
    }
}
