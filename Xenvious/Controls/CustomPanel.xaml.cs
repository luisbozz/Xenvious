using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Controls
{
    /// <summary>
    /// Interaction logic for CustomPanel.xaml
    /// </summary>
    [ContentProperty("Children")]
    public partial class CustomPanel : UserControl
    {

        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register("HeaderText", typeof(string), typeof(CustomPanel), new PropertyMetadata(string.Empty));

        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set 
            { 
                SetValue(HeaderTextProperty, value);
                HeaderLabel.Content = value;
            }
        }

        public CustomPanel()
        {
            InitializeComponent();
        }
    }
}
