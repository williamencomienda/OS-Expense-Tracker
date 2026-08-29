using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using GCAT.Models;

namespace GCAT.Views.Controls;

// Lightweight custom-drawn donut chart for "expenses overview" - no charting package
// dependency needed. Bind ItemsSource to an ObservableCollection<ExpenseSlice>.
public class DonutChart : Control
{
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<DonutChart, IEnumerable?>(nameof(ItemsSource));

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            if (change.OldValue is INotifyCollectionChanged oldCollection)
                oldCollection.CollectionChanged -= OnCollectionChanged;
            if (change.NewValue is INotifyCollectionChanged newCollection)
                newCollection.CollectionChanged += OnCollectionChanged;

            InvalidateVisual();
        }

        if (change.Property == BoundsProperty)
            InvalidateVisual();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => InvalidateVisual();

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (ItemsSource == null || Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        var slices = new List<ExpenseSlice>();
        foreach (var item in ItemsSource)
            if (item is ExpenseSlice slice && slice.Percent > 0)
                slices.Add(slice);

        if (slices.Count == 0) return;

        var size = Math.Min(Bounds.Width, Bounds.Height);
        var thickness = size * 0.22;
        var radius = (size - thickness) / 2;
        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);

        const double gapDegrees = 3;
        double startAngle = -90; // start at 12 o'clock

        foreach (var slice in slices)
        {
            var sweep = Math.Max(slice.Percent * 360.0 - gapDegrees, 1.0);
            DrawArcSegment(context, center, radius, thickness, startAngle, sweep, slice.Color);
            startAngle += slice.Percent * 360.0;
        }
    }

    private static void DrawArcSegment(DrawingContext context, Point center, double radius, double thickness,
        double startAngleDeg, double sweepDeg, IBrush brush)
    {
        var startRad = Math.PI * startAngleDeg / 180.0;
        var endRad = Math.PI * (startAngleDeg + sweepDeg) / 180.0;

        var startPoint = new Point(center.X + radius * Math.Cos(startRad), center.Y + radius * Math.Sin(startRad));
        var endPoint = new Point(center.X + radius * Math.Cos(endRad), center.Y + radius * Math.Sin(endRad));

        var figure = new PathFigure { StartPoint = startPoint, IsClosed = false };
        figure.Segments!.Add(new ArcSegment
        {
            Point = endPoint,
            Size = new Size(radius, radius),
            IsLargeArc = sweepDeg > 180,
            SweepDirection = SweepDirection.Clockwise
        });

        var geometry = new PathGeometry();
        geometry.Figures!.Add(figure);

        var pen = new Pen(brush, thickness) { LineCap = PenLineCap.Round };
        context.DrawGeometry(null, pen, geometry);
    }
}
