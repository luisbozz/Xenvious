import sys
sys.path.insert(0, 'docs/handoff')
from props_page import *  # noqa
import props_page as pp


def toggles(items, g, n):
    return '\n'.join(toggle(k, fb, g(cb), n, tip=tip) for k, fb, cb, tip in items)


def toggle_bound(text, cb, n):
    """Switch row whose label is a ready binding (the team locks add the team number)."""
    s = ' ' * n
    return (f'{s}<Grid Style="{{StaticResource FormRow}}">\n'
            f'{s}    <Grid.ColumnDefinitions>\n{s}        <ColumnDefinition Width="*"/>\n{s}        <ColumnDefinition Width="46"/>\n{s}    </Grid.ColumnDefinitions>\n'
            f'{s}    <TextBlock Style="{{StaticResource FormLabel}}" Text="{text}"/>\n'
            + reindent(clean(cb, add='Grid.Column="1" Style="{StaticResource FormToggle}"'), n + 4) + '\n'
            f'{s}</Grid>')


def entry_bar(combo, count_label, delete_btn, add_btn, n):
    """Entry bar as the props page has it now (after 061c3bf): no box, ‹ number › as one field."""
    s = ' ' * n
    combo_name = re.search(r'x:Name="([^"]+)"', combo).group(1)
    combo_el = clean(combo, add='Width="84" Height="30" BorderThickness="0"')
    del_name = re.search(r'x:Name="([^"]+)"', delete_btn).group(1)
    del_click = re.search(r'Click="([^"]+)"', delete_btn).group(1)
    add_name = re.search(r'x:Name="([^"]+)"', add_btn).group(1)
    add_click = re.search(r'Click="([^"]+)"', add_btn).group(1)
    return f'''{s}<Border DockPanel.Dock="Top" Padding="2,0" Margin="0,12,0,12">
{s}    <DockPanel>
{s}        <StackPanel DockPanel.Dock="Right" Orientation="Horizontal">
{s}            <!-- ‹ number › : steps through the entries without opening the list. -->
{s}            <Border Background="{{DynamicResource ComboBoxBackground}}" BorderBrush="{{DynamicResource ComboBoxBorder}}" BorderThickness="1" CornerRadius="4" Height="32" Margin="0,0,10,0">
{s}                <StackPanel Orientation="Horizontal">
{s}                    <Button Style="{{StaticResource EntryStepButton}}" Content="‹" Tag="{{Binding ElementName={combo_name}}}" CommandParameter="-1" Click="EntryStep_Click" ToolTip="{T('entry_prev', 'Previous')}"/>
{reindent(combo_el, n + 20)}
{s}                    <Button Style="{{StaticResource EntryStepButton}}" Content="›" Tag="{{Binding ElementName={combo_name}}}" CommandParameter="1" Click="EntryStep_Click" ToolTip="{T('entry_next', 'Next')}"/>
{s}                </StackPanel>
{s}            </Border>
{s}            <Button x:Name="{del_name}" Style="{{StaticResource FormButton}}" Width="34" Height="32" Padding="0" Margin="0,0,6,0" Click="{del_click}" ToolTip="{T('delete', 'Delete')}">
{s}                <Path Data="{ICON_TRASH}" Stroke="{{DynamicResource TextColor}}" StrokeThickness="1.5" StrokeLineJoin="Round" Width="16" Height="16"/>
{s}            </Button>
{s}            <Button x:Name="{add_name}" Style="{{StaticResource FormButtonPrimary}}" Height="32" Padding="10,0,12,0" Click="{add_click}" ToolTip="{T('add', 'Add')}">
{s}                <StackPanel Orientation="Horizontal">
{s}                    <Path Data="{ICON_PLUS}" Stroke="#FF202225" StrokeThickness="2" Width="16" Height="16" Margin="0,0,6,0" VerticalAlignment="Center"/>
{s}                    <TextBlock VerticalAlignment="Center" Foreground="#FF202225" Text="{T('add', 'Add')}"/>
{s}                </StackPanel>
{s}            </Button>
{s}        </StackPanel>
{s}        <TextBlock VerticalAlignment="Center" FontSize="14" Foreground="{{StaticResource NavMutedBrush}}">
{s}            <Run FontSize="17" FontWeight="Bold" Foreground="{{DynamicResource TextColor}}" Text="{{Binding Items.Count, ElementName={combo_name}, Mode=OneWay}}"/>
{s}            <Run Text="{T(count_label[0], count_label[1])}"/>
{s}        </TextBlock>
{s}    </DockPanel>
{s}</Border>'''


def icon_button(btn, n):
    """Cursor button beside X/Y/Z: crosshair icon in the square field button, as on props."""
    btn = re.sub(r'Style="\{DynamicResource TitleBarButton\}"', 'Style="{StaticResource FieldIconButton}"', btn)
    btn = re.sub(r'<Path( x:Name="[^"]+")?[^/]*/>',
                 r'<Path\1 Data="M8,1.5 L8,4.5 M8,11.5 L8,14.5 M1.5,8 L4.5,8 M11.5,8 L14.5,8 M8,5 A3,3 0 1 1 7.99,5" '
                 r'Stroke="{Binding Foreground, RelativeSource={RelativeSource AncestorType=Button}}" StrokeThickness="1.6" Width="16" Height="16" />', btn)
    return btn


def doors(g, n):
    """Six doors, each with an open and a closed switch."""
    s = ' ' * n
    rows = [('vehdoorsfl', 'Front Left', 'fl'), ('vehdoorsfr', 'Front Right', 'fr'), ('vehdoorsrl', 'Rear Left', 'rl'),
            ('vehdoorsrr', 'Rear Right', 'rr'), ('vehdoorshood', 'Hood', 'hood'), ('vehdoorstrunk', 'Trunk', 'trunk')]
    out = [f'{s}<Grid Margin="0,0,0,8">',
           f'{s}    <Grid.ColumnDefinitions>',
           f'{s}        <ColumnDefinition Width="*"/>',
           f'{s}        <ColumnDefinition Width="56"/>',
           f'{s}        <ColumnDefinition Width="56"/>',
           f'{s}    </Grid.ColumnDefinitions>',
           f'{s}    <Grid.RowDefinitions>',
           f'{s}        <RowDefinition Height="Auto"/>']
    out += [f'{s}        <RowDefinition Height="34"/>'] * len(rows)
    out += [f'{s}    </Grid.RowDefinitions>',
            f'{s}    <TextBlock Grid.Column="1" Style="{{StaticResource FieldAxis}}" HorizontalAlignment="Right" Text="{T("door_open", "Open")}"/>',
            f'{s}    <TextBlock Grid.Column="2" Style="{{StaticResource FieldAxis}}" HorizontalAlignment="Right" Text="{T("door_closed", "Closed")}"/>']
    for i, (key, fb, d) in enumerate(rows, start=1):
        out.append(f'{s}    <TextBlock Grid.Row="{i}" Style="{{StaticResource FormLabel}}" Text="{T(key, fb)}"/>')
        for col, kind in ((1, 'open'), (2, 'close')):
            cb = clean(g(f'cb_veh_door_{kind}_{d}'), add=f'Grid.Row="{i}" Grid.Column="{col}" Style="{{StaticResource FormToggle}}"')
            out.append(reindent(cb, n + 4))
    out.append(f'{s}</Grid>')
    return '\n'.join(out)


def build_vehicle(src):
    a, b, old = tab_range(src, 'PageVehicle')
    base = len(src[:a].split('\n')[-1])
    n = base + 20
    m = n + 8
    g = lambda nm: extract(old, nm)
    opening = old[:old.index('>') + 1]

    hidden_box = extract_container(old, '<Border Grid.Row="1" CornerRadius="5" Background="{DynamicResource SeactionHeaderBackgroundBrush}" Margin="8">')
    hidden = '\n'.join([reindent(g('Lblvehmodel'), base + 12), reindent(hidden_box, base + 12)])
    bar = entry_bar(g('ddvehno'), ('vehicles_placed', 'vehicles placed'), g('BtnVehDelete'), g('BtnVehAdd'), base + 8)

    model = reindent('<local:ModelCard x:Name="VehModelCard" Margin="0,0,0,12"/>', n)

    placement = card('card_position', 'Placement', '\n'.join([
        vector('loc', 'Location', [g('tbvehlocx'), g('tbvehlocy'), g('tbvehlocz')], icon_button(g('Btnvehgetloc'), 0), m),
        vector('vrot', 'Rotation', [g('tbvehrotx'), g('tbvehroty'), g('tbvehrotz')], None, m),
        two(field('heading', 'Heading', g('tbvehhead'), m + 4), '', m),
    ]), n)

    look = card('card_look', 'Look', '\n'.join([
        two(field('pcol', 'Primary Color', g('ddvehcol1'), m + 4), field('scol', 'Secondary Color', g('ddvehcol2'), m + 4), m),
        two(field('windowtint', 'Window Tint', g('ddvehwindowtint'), m + 4), field('livery', 'Livery', g('tbvehlivery'), m + 4), m),
        two(field('icon', 'Icon', g('ddvehicon'), m + 4), field('iconsize', 'Icon Size', g('tbvehiconsize'), m + 4), m),
        toggles([
            ('vehneonlights', 'Neon Lights', 'cb_veh_neon', None),
            ('remwindows', 'Remove Windows', 'cb_veh_remove_windows', None),
            ('vehlights', 'Vehicle Lights', 'cb_veh_lights', None),
            ('vehsirens', 'Vehicle Sirens', 'cb_veh_sirens', None),
            ('vehsirensaudio', 'Vehicle Sirens Audio', 'cb_veh_sirens_audio', None),
            ('vehbox', 'Spawn Boxes', 'cb_veh_box', None),
        ], g, m),
    ]), n)

    state = card('card_state', 'State', '\n'.join([
        two(field('health', 'Health', g('tbvehhlth'), m + 4), field('veh_hp_engine', 'Engine', g('tbvehenghp'), m + 4), m),
        two(field('veh_hp_tank', 'Petrol Tank', g('tbvehptrhp'), m + 4), field('veh_hp_body', 'Body', g('tbvehbdyhp'), m + 4), m),
        toggles([
            ('godmode', 'God Mode', 'cb_veh_godmode', None),
            ('bptires', 'Bulletproof Tires', 'cb_veh_bptires', None),
            ('vehengineon', 'Engine on', 'cb_veh_engine', ('vehengineoninfo', 'Engine on')),
            ('veheiw', 'Explode in Water', 'cb_veh_explodeinwater', None),
            ('nottargetable', 'Not Targetable', 'cb_veh_nottargetable', None),
            ('markveh', 'Mark Vehicle', 'cb_veh_mark', None),
        ], g, m),
    ]), n)

    locks = card('card_locks', 'Locks', '\n'.join(
        [toggle_bound(f"{{Binding Translation[vehlockteam], StringFormat='{{}}{{0}} {t}', FallbackValue='Locked for Team {t}'}}", g(f'cb_veh_lockteam{t}'), m)
         for t in range(1, 5)]
        + [toggles([
            ('lockveh', 'Lock Vehicle', 'cb_veh_locveh', None),
            ('lockveh2', 'Lock Vehicle for Players', 'cb_veh_locvehforp', None),
            ('freezecar', 'Freeze Car', 'cb_veh_freeze', ('freezecarinfo', 'Freeze Car')),
        ], g, m)]), n)

    rules = card('aslrheader', 'Associated Rule', '\n'.join([
        two(field('vehrsp', 'Vehicle Respawn', g('ddvehrsp'), m + 4), field('vehvrr', 'Respawn Range', g('tbvehvrr'), m + 4), m),
        sep(m),
        two(field('asrlspawnon', 'Spawn On', g('ddvehspawnon'), m + 4), field('asrlspawnteam', 'Spawn by Team', g('ddvehspawnteam'), m + 4), m),
        two(field('asrlspawn', 'Spawn at Rule', g('tbvehspawnrule'), m + 4), '', m),
        toggle('spwnrlivc', 'Ignore Visibility Check on Spawn', g('cb_veh_spwnrlivc'), m, tip=('spwnrlivc', 'Ignore Visibility Check on Spawn')),
        sep(m),
        two(field('asrlclteam', 'Clear up by Team', g('ddvehteamclear'), m + 4), field('asrlclear', 'Clear up at Rule', g('tbvehclearrule'), m + 4), m),
        toggle('clrlivc', 'Ignore Visibility Check on Clean up', g('cb_veh_clrlivc'), m, tip=('clrlivc', 'Ignore Visibility Check on Clean up')),
    ]), n)

    door_card = card('card_doors', 'Doors', doors(g, m), n)

    # Bitset picker and its value side by side, as on the actor page.
    s = ' ' * m
    vbs = '\n'.join([
        f'{s}<TextBlock Style="{{StaticResource FieldLabelRaw}}" Text="vbs2 … vbs9"/>',
        f'{s}<Grid Margin="0,0,0,10">',
        f'{s}    <Grid.ColumnDefinitions>',
        f'{s}        <ColumnDefinition Width="110"/>',
        f'{s}        <ColumnDefinition Width="*"/>',
        f'{s}    </Grid.ColumnDefinitions>',
        reindent(clean(g('ddvehvbs'), add='Height="30"'), m + 4),
        reindent(clean(g('tbvehvbs'), add='Grid.Column="1" Height="30"'), m + 4),
        f'{s}</Grid>',
    ])
    raw = '\n'.join([
        vbs,
        two(field('actorteam', 'Team', g('ddvehteamrlprio'), m + 4), field('rule', 'Rule', g('tbvehrule'), m + 4), m),
        two(field('priority', 'Priority', g('tbvehpriority'), m + 4), '', m),
        two(field('jtop', 'Jump To Objective(Pass)', g('tbvehjtop'), m + 4), field('jtof', 'Jump To Objective(Fail)', g('tbvehjtof'), m + 4), m),
        raw_grid([('objt', 'tbvehobjt'), ('spwn', 'tbvehspwn'), ('team', 'tbvehteam'), ('spsrc', 'tbvehspsrc'),
                  ('spasr', 'tbvehspasr'), ('vebs', 'tbvehvebs'), ('drbs', 'tbvehdrbs'), ('vehcr', 'tbvehvehcr'),
                  ('vehct', 'tbvehvehct')], g, m),
    ])
    advanced = expander_card('advanced', 'Advanced', raw, n)

    # Field buttons sit on the card without a box of their own (as on props since 5bc4df8).
    placement = placement.replace(' CornerRadius="4" Background="{DynamicResource SeactionHeaderBackgroundBrush}"', '')
    new = page_shell('PageVehicle', 'Vehicles', bar, hidden, [
        model,
        placement,
        '\n'.join([look, locks]),
        '\n'.join([state, door_card]),
        '\n'.join([rules, advanced]),
    ], base)
    new = new.replace('<TabItem x:Name="PageVehicle" Header="Vehicles" >', opening.strip(), 1)
    new = new.replace('<DockPanel>', '<DockPanel Margin="10,0">', 1)
    return src[:a - base] + new + src[b:]


if __name__ == '__main__':
    before = names(pp.S)
    out = build_vehicle(pp.S)
    after = names(out)
    print('missing names:', sorted(before - after))
    print('new names:', sorted(after - before))
    if '--write' in sys.argv:
        if pp.CRLF:
            out = out.replace('\n', '\r\n')
        open(pp.P, 'w', encoding='utf-8-sig').write(out)
