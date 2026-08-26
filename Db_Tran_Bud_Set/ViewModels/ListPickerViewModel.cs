using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GCAT.ViewModels;

// A single row in a ListPickerViewModel or a chip in FiltersViewModel.
public class ListOption : ViewModelBase
{
    public string Label { get; set; } = string.Empty;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }
}

// Drives the simple vertical "Filter by type" / "Filter by account" popups (images 3 & 4) -
// a title, a close (X), and a single-select list of options. Reused for both since they're
// visually and structurally identical, just with different titles/options.
public class ListPickerViewModel : ViewModelBase
{
    public string Title { get; }
    public ObservableCollection<ListOption> Options { get; }

    public RelayCommand SelectCommand { get; }
    public RelayCommand CloseCommand { get; }

    public event Action<string>? OptionChosen;
    public event Action? CloseRequested;

    public ListPickerViewModel(string title, IEnumerable<string> options, string selectedLabel)
    {
        Title = title;
        Options = new ObservableCollection<ListOption>(
            options.Select(o => new ListOption { Label = o, IsSelected = o == selectedLabel }));

        SelectCommand = new RelayCommand(param =>
        {
            if (param is ListOption option)
            {
                foreach (var o in Options) o.IsSelected = ReferenceEquals(o, option);
                OptionChosen?.Invoke(option.Label);
            }
        });

        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke());
    }
}
