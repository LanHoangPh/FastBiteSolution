using CommunityToolkit.Mvvm.ComponentModel;
using FluentValidation;
using System.Collections;
using System.ComponentModel;

namespace FastBiteGroup.Desktop.UI.Validators;

public abstract class ValidationViewModelBase<T> : ObservableObject, INotifyDataErrorInfo 
    where T : ValidationViewModelBase<T>
{
    private readonly AbstractValidator<T> _validator;
    private readonly Dictionary<string, List<string>> _errors = new();

    protected ValidationViewModelBase(AbstractValidator<T> validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public bool HasErrors => _errors.Any();

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return _errors.Values.SelectMany(x => x);

        return _errors.TryGetValue(propertyName, out var errors) ? errors : Enumerable.Empty<string>();
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        // Run validation when a bound property changes (except for helper states)
        if (e.PropertyName != null && e.PropertyName != nameof(HasErrors))
        {
            ValidateProperty(e.PropertyName);
        }
    }

    public bool ValidateProperty(string propertyName)
    {
        var context = ValidationContext<T>.CreateWithOptions((T)this, x => x.IncludeProperties(propertyName));
        var result = _validator.Validate(context);

        // Clear existing errors for this property
        _errors.Remove(propertyName);

        // Add new errors if any
        var propertyErrors = result.Errors
            .Where(x => x.PropertyName == propertyName)
            .Select(x => x.ErrorMessage)
            .ToList();

        if (propertyErrors.Any())
        {
            _errors[propertyName] = propertyErrors;
        }

        // Raise notification events for WPF binding engine
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));

        return !propertyErrors.Any();
    }

    public void ClearErrors()
    {
        var properties = _errors.Keys.ToList();
        _errors.Clear();
        foreach (var propertyName in properties)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
        OnPropertyChanged(nameof(HasErrors));
    }

    public bool ValidateAll()
    {
        var result = _validator.Validate((T)this);
        _errors.Clear();

        foreach (var group in result.Errors.GroupBy(x => x.PropertyName))
        {
            _errors[group.Key] = group.Select(x => x.ErrorMessage).ToList();
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(group.Key));
        }

        // Notify for properties that no longer have errors
        var allValidatedProperties = result.Errors.Select(x => x.PropertyName).ToHashSet();
        foreach (var key in _errors.Keys.ToList())
        {
            if (!allValidatedProperties.Contains(key))
            {
                _errors.Remove(key);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(key));
            }
        }

        OnPropertyChanged(nameof(HasErrors));
        return !HasErrors;
    }
}
