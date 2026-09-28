using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xenvious
{
    /// <summary>
    /// A card of the entity pages: header bar with an icon tile, title, a short summary on the
    /// right and a fold arrow; the content below. A hero card (model, placement) gets an accent
    /// frame. The look is the SectionCard style in MainWindow.xaml, so every theme applies.
    /// </summary>
    public class SectionCard : ContentControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(object), typeof(SectionCard));
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(Geometry), typeof(SectionCard));
        public static readonly DependencyProperty SummaryProperty = DependencyProperty.Register(nameof(Summary), typeof(string), typeof(SectionCard));
        public static readonly DependencyProperty HeaderRightProperty = DependencyProperty.Register(nameof(HeaderRight), typeof(object), typeof(SectionCard));
        public static readonly DependencyProperty IsHeroProperty = DependencyProperty.Register(nameof(IsHero), typeof(bool), typeof(SectionCard));
        public static readonly DependencyProperty CanCollapseProperty = DependencyProperty.Register(nameof(CanCollapse), typeof(bool), typeof(SectionCard),
            new PropertyMetadata(true, (d, _) => d.CoerceValue(IsExpandedProperty)));
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(SectionCard),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, (d, v) => ((SectionCard)d).CanCollapse ? v : true));

        public object Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
        public Geometry Icon { get => (Geometry)GetValue(IconProperty); set => SetValue(IconProperty, value); }
        public string Summary { get => (string)GetValue(SummaryProperty); set => SetValue(SummaryProperty, value); }
        /// <summary>Extra content at the right end of the header (a button, a warning chip).</summary>
        public object HeaderRight { get => GetValue(HeaderRightProperty); set => SetValue(HeaderRightProperty, value); }
        public bool IsHero { get => (bool)GetValue(IsHeroProperty); set => SetValue(IsHeroProperty, value); }
        public bool CanCollapse { get => (bool)GetValue(CanCollapseProperty); set => SetValue(CanCollapseProperty, value); }
        public bool IsExpanded { get => (bool)GetValue(IsExpandedProperty); set => SetValue(IsExpandedProperty, value); }
    }
}
