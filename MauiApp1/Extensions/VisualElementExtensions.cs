using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace MauiApp1.Extensions
{
    public static class VisualElementExtensions
    {
        public static Point GetAbsolutePosition(this VisualElement element)
        {
            try
            {
                var x = element.X;
                var y = element.Y;

                var parent = element.Parent;
                while (parent != null)
                {
                    if (parent is VisualElement visualParent)
                    {
                        x += visualParent.X;
                        y += visualParent.Y;
                    }
                    parent = parent.Parent;
                }

                return new Point(x, y);
            }
            catch
            {
                return Point.Zero;
            }
        }

        public static Rect GetAbsoluteBounds(this VisualElement element)
        {
            try
            {
                var position = element.GetAbsolutePosition();
                return new Rect(position.X, position.Y, element.Width, element.Height);
            }
            catch
            {
                return Rect.Zero;
            }
        }

        public static bool ContainsPoint(this VisualElement element, Point point)
        {
            try
            {
                var bounds = element.GetAbsoluteBounds();
                return bounds.Contains(point);
            }
            catch
            {
                return false;
            }
        }
    }
}
