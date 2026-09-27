import sys
sys.path.insert(0, 'docs/handoff')
from vehicle_page import *  # noqa  (entry_bar in the current style, icon_button)
import props_page as pp


def build_zone(src):
    a, b, old = tab_range(src, 'PageZone')
    base = len(src[:a].split('\n')[-1])
    n = base + 20
    m = n + 8
    s = ' ' * m
    g = lambda nm: extract(old, nm)
    opening = old[:old.index('>') + 1]

    bar = entry_bar(g('ddzoneno'), ('zones_placed', 'zones placed'), g('BtnZoneDelete'), g('BtnzoneAdd'), base + 8)

    # Zone type: picker with names, the raw number beside it, what the type does, and the two
    # values under a label that follows the type (filled in MainWindow.Zones.cs).
    zntp = clean(g('tbzonezntp'), add='Grid.Column="2" Height="30"')
    ztype = card('card_zonetype', 'Zone Type', '\n'.join([
        label('zntp', 'Zone Type', m),
        f'{s}<Grid Margin="0,0,0,8">',
        f'{s}    <Grid.ColumnDefinitions>',
        f'{s}        <ColumnDefinition Width="*"/>',
        f'{s}        <ColumnDefinition Width="8"/>',
        f'{s}        <ColumnDefinition Width="64"/>',
        f'{s}    </Grid.ColumnDefinitions>',
        f'{s}    <ComboBox x:Name="ddzonetype" Height="30" IsEnabled="{{Binding ElementName=tbzonezntp, Path=IsEnabled}}" SelectionChanged="ddzonetype_SelectionChanged" DropDownOpened="ddzonetype_DropDownOpened" MaxDropDownHeight="420"/>',
        reindent(zntp, m + 4),
        f'{s}</Grid>',
        f'{s}<Border Background="{{DynamicResource SeactionHeaderBackgroundBrush}}" CornerRadius="4" Padding="10,8" Margin="0,0,0,12">',
        f'{s}    <TextBlock x:Name="lblzonetypeinfo" TextWrapping="Wrap" FontSize="13" Foreground="{{StaticResource NavMutedBrush}}" Text="{T("zt_pick", "Pick a zone type to see what it does.")}"/>',
        f'{s}</Border>',
        two('\n'.join([f'{s}    <TextBlock x:Name="lblzoneznwd" Style="{{StaticResource FieldLabel}}" Text="{T("znwd", "znwd")}"/>',
                       reindent(clean(g('tbzoneznwd'), add='Height="30" Margin="0,0,0,10"'), m + 4)]),
            '\n'.join([f'{s}    <TextBlock x:Name="lblzoneznwvd" Style="{{StaticResource FieldLabel}}" Text="{T("znwvd", "znwvd")}"/>',
                       reindent(clean(g('tbzoneznwvd'), add='Height="30" Margin="0,0,0,10"'), m + 4)]), m),
        f'{s}<TextBlock TextWrapping="Wrap" FontSize="12" Foreground="#FF72767D" Margin="0,0,0,8" Text="{T("zt_hint", "Only some types are described. Any number can be typed in the field beside the list.")}"/>',
    ]), n)

    shape = card('card_shape', 'Shape and Size', '\n'.join([
        field('variation', 'Variation', g('ddzonevariation'), m),
        vector('pastart', 'Startpoint', [g('tbzonestartx'), g('tbzonestarty'), g('tbzonestartz')], icon_button(g('Btnzonegetstart'), 0), m),
        vector('paend', 'Endpoint', [g('tbzoneendx'), g('tbzoneendy'), g('tbzoneendz')], icon_button(g('Btnzonegetend'), 0), m),
        two(field('width', 'Width', g('tbzonewidth'), m + 4), field('height', 'Height', g('tbzoneheight'), m + 4), m),
    ]), n)
    shape = shape.replace(' CornerRadius="4" Background="{DynamicResource SeactionHeaderBackgroundBrush}"', '')

    raw = '\n'.join([
        two(field('actorteam', 'Team', g('ddzoneteam'), m + 4), field('rule', 'Rule', g('tbzonerule'), m + 4), m),
        two(field('priority', 'Priority', g('tbzonepriority'), m + 4), '', m),
        raw_grid([('znbs', 'tbzoneznbs'), ('znbs2', 'tbzoneznbs2'), ('znbs3', 'tbzoneznbs3')], g, m),
    ])
    advanced = expander_card('advanced', 'Advanced', raw, n)

    new = page_shell('PageZone', 'Zone', bar, '', [ztype, shape, advanced], base)
    # No old model controls to keep on this page.
    new = re.sub(r'\n *<!-- The old model controls[^\n]*\n *<StackPanel DockPanel.Dock="Top" Visibility="Collapsed">\n\n *</StackPanel>', '', new)
    new = new.replace('<TabItem x:Name="PageZone" Header="Zone" >', opening.strip(), 1)
    new = new.replace('<DockPanel>', '<DockPanel Margin="10,0">', 1)
    return src[:a - base] + new + src[b:]


if __name__ == '__main__':
    before = names(pp.S)
    out = build_zone(pp.S)
    after = names(out)
    print('missing names:', sorted(before - after))
    print('new names:', sorted(after - before))
    if '--write' in sys.argv:
        if pp.CRLF:
            out = out.replace('\n', '\r\n')
        open(pp.P, 'w', encoding='utf-8-sig').write(out)
