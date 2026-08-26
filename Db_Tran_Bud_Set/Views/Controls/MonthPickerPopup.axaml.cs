using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using GCAT.ViewModels;

namespace GCAT.Views.Controls;

public partial class MonthPickerPopup : UserControl
{
    public static readonly StyledProperty<MonthPickerViewModel?> MonthPickerProperty =
        AvaloniaProperty.Register<MonthPickerPopup, MonthPickerViewModel?>(nameof(MonthPicker));

    public MonthPickerViewModel? MonthPicker
    {
        get => GetValue(MonthPickerProperty);
        set => SetValue(MonthPickerProperty, value);
    }

    public static readonly StyledProperty<ICommand?> CloseCommandProperty =
        AvaloniaProperty.Register<MonthPickerPopup, ICommand?>(nameof(CloseCommand));

    public ICommand? CloseCommand
    {
        get => GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public MonthPickerPopup()
    {
        InitializeComponent();
    }
}
