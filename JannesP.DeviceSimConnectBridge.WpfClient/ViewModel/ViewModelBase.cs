using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.WpfApp.Extensions;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => OnPropertyChanged(false, propertyName);

        protected virtual void OnPropertyChanged(bool skipValidation, [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (!skipValidation) ValidateAsync();
        }
        #endregion INotifyPropertyChanged


        //implementation copied from https://stackoverflow.com/questions/19539492/implement-validation-for-wpf-textboxes
        //but rewritten in large parts
        #region INotifyDataErrorInfo
        private readonly object _lockValidate = new();
        private readonly ConcurrentDictionary<string, List<string>> _errors = new();

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            OnPropertyChanged(true, nameof(HasErrors));
            OnPropertyChanged(true, nameof(HasNoErrors));
        }

        //this is a default interface that doesn't play nice with Nullable Reference Types. We need to answer with 'null' to indicate no errors
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
        public IEnumerable? GetErrors(string? propertyName)
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return null;
            }
            List<string>? errorsForName;
            if (!_errors.TryGetValue(propertyName, out errorsForName))
            {
                return null;
            }
            return errorsForName;
        }

        public bool HasErrors => _errors.Any(kv => kv.Value != null && kv.Value.Count > 0);
        public bool HasNoErrors => !HasErrors;

        public Task ValidateAsync() => Task.Run(() => Validate());

        public void Validate()
        {
            lock (_lockValidate)
            {
                var validationContext = new ValidationContext(this, null, null);
                var validationResults = new List<ValidationResult>();
                Validator.TryValidateObject(this, validationContext, validationResults, true);

                HashSet<string> changedErrors = new();

                //remove all previous errors for properties that don't appear this time
                foreach (KeyValuePair<string, List<string>> kv in _errors)
                {
                    if (validationResults.All(r => r.MemberNames.All(m => m != kv.Key)))
                    {
                        _errors.TryRemove(kv.Key, out _);
                        changedErrors.Add(kv.Key);
                    }
                }

                //group all validationresults by their affected member names
                IEnumerable<IGrouping<string, ValidationResult>>? resultsByMemberName = validationResults
                    .SelectMany(vr => vr.MemberNames, (vr, memberName) => new { memberName, vr })
                    .GroupBy(a => a.memberName, a => a.vr);

                foreach (IGrouping<string, ValidationResult>? prop in resultsByMemberName)
                {
                    var messages = prop.Select(r => r.ErrorMessage).ToList();

                    _errors[prop.Key] = prop.Select(vr => vr.ErrorMessage).NotNullOrEmpty().ToList();
                    changedErrors.Add(prop.Key);
                }
                foreach (string member in changedErrors)
                {
                    OnErrorsChanged(member);
                }
            }
        }

        public static ValidationResult? ValidateProperty(object? value, ValidationContext context)
        {
            if (context.ObjectInstance is not ViewModelBase viewModel)
            {
                throw new InvalidOperationException("This validation methos is only supposed to be used with ViewModels.");
            }
            if (context.MemberName == null) throw new InvalidOperationException("This validation method can only run on Property targets.");
            string? error = viewModel.OnValidateProperty(context.MemberName, value);
            if (error != null)
            {
                return new ValidationResult(error, new[] { nameof(context.MemberName) });
            }
            return null;
        }

        public virtual string? OnValidateProperty(string propertyName, object? value) => null;
        #endregion INotifyDataErrorInfo
    }
}
