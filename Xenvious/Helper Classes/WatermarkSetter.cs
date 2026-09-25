using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    public class WatermarkSetter
    {
        public static string GetWatermark(DependencyObject obj) => (string)obj.GetValue(WatermarkProperty);

        public static void SetWatermark(DependencyObject obj, string value) => obj.SetValue(WatermarkProperty, value);

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.RegisterAttached("Watermark", typeof(string),
                typeof(WatermarkSetter), new UIPropertyMetadata(default(string)));


        //    if (control == null) return;



        //    if (control == null || control.Template == null) return;

        //    control.ApplyTemplate();

        //    Border border = control.Template.FindName("border", control) as Border;

        //    if (border == null) return;

        //    border.CornerRadius = GetCornerRadius(control);
        //}
    }
}
