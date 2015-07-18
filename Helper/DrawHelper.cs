﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace CrazyStorm
{
    class DrawHelper
    {
        public static void DrawArrow(Canvas canvas, int x, int y, int length, int thick, float angle, Color color, float opacity)
        {
            var rad = angle / 180 * Math.PI;
            int arrowHeadX = length, arrowHeadY = 0;
            int arrowLeftX = arrowHeadX - 10, arrowLeftY = arrowHeadY - 5;
            int arrowRightX = arrowHeadX - 10, arrowRightY = arrowHeadY + 5;
            var arrowLine = new Line();
            arrowLine.Opacity = opacity;
            arrowLine.X1 = x;
            arrowLine.Y1 = y;
            arrowLine.X2 = x + (arrowHeadX * Math.Cos(rad) + arrowHeadY * (-Math.Sin(rad)));
            arrowLine.Y2 = y + (arrowHeadX * Math.Sin(rad) + arrowHeadY * Math.Cos(rad));
            arrowLine.StrokeThickness = thick;
            arrowLine.Stroke = new SolidColorBrush(color);
            var arrowLeft = new Line();
            arrowLeft.Opacity = opacity;
            arrowLeft.X1 = arrowLine.X2;
            arrowLeft.Y1 = arrowLine.Y2;
            arrowLeft.X2 = x + (arrowLeftX * Math.Cos(rad) + arrowLeftY * (-Math.Sin(rad)));
            arrowLeft.Y2 = y + (arrowLeftX * Math.Sin(rad) + arrowLeftY * Math.Cos(rad));
            arrowLeft.StrokeThickness = thick;
            arrowLeft.Stroke = new SolidColorBrush(color);
            var arrowRight = new Line();
            arrowRight.Opacity = opacity;
            arrowRight.X1 = arrowLine.X2;
            arrowRight.Y1 = arrowLine.Y2;
            arrowRight.X2 = x + (arrowRightX * Math.Cos(rad) + arrowRightY * (-Math.Sin(rad)));
            arrowRight.Y2 = y + (arrowRightX * Math.Sin(rad) + arrowRightY * Math.Cos(rad));
            arrowRight.StrokeThickness = thick;
            arrowRight.Stroke = new SolidColorBrush(color);
            canvas.Children.Add(arrowLine);
            canvas.Children.Add(arrowLeft);
            canvas.Children.Add(arrowRight);
        }
        public static void DrawFan(Canvas canvas, int x ,int y, float radius, float startAngle, float endAngle, Color color, float opacity)
        {
            if (endAngle - startAngle < 360)
            {
                var startRad = startAngle / 180 * Math.PI;
                var endRad = endAngle / 180 * Math.PI;
                var path = new Path();
                path.Fill = new SolidColorBrush(color);
                path.Opacity = opacity;
                var fan = new PathGeometry();
                var figure = new PathFigure();
                figure.StartPoint = new Point(x, y);
                var start = new LineSegment();
                start.Point = new Point(x + radius * Math.Cos(startRad), y + radius * Math.Sin(startRad));
                var arc = new ArcSegment();
                arc.IsLargeArc = Math.Abs(endAngle - startAngle) > 180 ? true : false;
                arc.Size = new Size(radius, radius);
                arc.Point = new Point(x + radius * Math.Cos(endRad), y + radius * Math.Sin(endRad));
                var end = new LineSegment();
                end.Point = new Point(x, y);
                figure.Segments.Add(start);
                figure.Segments.Add(arc);
                figure.Segments.Add(end);
                fan.Figures.Add(figure);
                path.Data = fan;
                canvas.Children.Add(path);
            }
            else
            {
                var elipse = new Ellipse();
                elipse.Width = radius * 2;
                elipse.Height = radius * 2;
                elipse.Fill = new SolidColorBrush(color);
                elipse.Opacity = opacity;
                elipse.SetValue(Canvas.LeftProperty, (double)x - radius);
                elipse.SetValue(Canvas.TopProperty, (double)y - radius);
                canvas.Children.Add(elipse);
            }
        }
    }
}
