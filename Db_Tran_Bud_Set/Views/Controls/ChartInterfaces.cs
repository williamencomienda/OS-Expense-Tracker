using System.Collections.Generic;
using Avalonia.Media;

namespace GCAT.Views.Controls;

// Implement this on any model to make it plottable by DonutChart - decouples the chart
// from any specific model type (e.g. ExpenseSlice), so it's reusable for other data too.
public interface IChartSlice
{
    double Value { get; }
    IBrush Brush { get; }
}

// Implement this on any model to make it plottable by BarChart. A group can carry any
// number of series (not hardcoded to "income vs expenses"), so the same control works
// for single-series, two-series, or multi-series bar charts elsewhere in the app.
public interface IBarGroup
{
    string Label { get; }
    IReadOnlyList<IBarSeriesValue> Series { get; }
}

public interface IBarSeriesValue
{
    double Value { get; }
    IBrush Brush { get; }
}

// Simple concrete implementation for models that want to build their Series list inline.
public readonly struct BarSeriesValue : IBarSeriesValue
{
    public BarSeriesValue(double value, IBrush brush)
    {
        Value = value;
        Brush = brush;
    }

    public double Value { get; }
    public IBrush Brush { get; }
}
