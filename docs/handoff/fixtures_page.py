import sys
sys.path.insert(0, 'docs/handoff')
from vehicle_page import *  # noqa  (entry_bar in the current style, icon_button, toggles)
import props_page as pp


def entry_bar_add_only(combo, count_label, add_btn, n):
    """Entry bar for a list without a delete action (fixtures)."""
    bar = entry_bar(combo, count_label, '<Button x:Name="__DEL__" Click="__X__"/>', add_btn, n)
    # drop the delete button the template adds
    return re.sub(r'\n *<Button x:Name="__DEL__"[\s\S]*?</Button>', '', bar)


def build_fixtures(src):
    a, b, old = tab_range(src, 'Pagecentity')
    base = len(src[:a].split('\n')[-1])
    n = base + 20
    m = n + 8
    s = ' ' * m
    g = lambda nm: extract(old, nm)
    opening = old[:old.index('>') + 1]

    bar = entry_bar_add_only(g('ddcentityno'), ('fixtures_placed', 'fixtures placed'), g('BtncdefAdd'), base + 8)

    # "Advanced" switch and its label stay, hidden: the position is always shown now.
    hidden = '\n'.join([reindent(g('Lblcdefadvanced'), base + 12), reindent(g('cbcdefadvanced'), base + 12)])

    # Prop under the cursor, as on Advanced Placement: live name and a take button.
    take = g('Btncdefgetmodel')
    take_click = re.search(r'Click="([^"]+)"', take).group(1)
    hover = '\n'.join([
        f'{s}<Border CornerRadius="5" Background="{{DynamicResource SeactionHeaderBackgroundBrush}}" Padding="12,8" Margin="0,0,0,12">',
        f'{s}    <Grid>',
        f'{s}        <Grid.ColumnDefinitions>',
        f'{s}            <ColumnDefinition Width="Auto"/>',
        f'{s}            <ColumnDefinition Width="*"/>',
        f'{s}            <ColumnDefinition Width="Auto"/>',
        f'{s}        </Grid.ColumnDefinitions>',
        f'{s}        <TextBlock Text="⌖" FontSize="22" Foreground="#FFFAC828" VerticalAlignment="Center" Margin="0,0,10,0"/>',
        f'{s}        <StackPanel Grid.Column="1" VerticalAlignment="Center" Margin="0,0,10,0">',
        f'{s}            <TextBlock FontSize="11" Foreground="{{StaticResource NavMutedBrush}}" Text="{T("adv_hovered", "Prop under the cursor")}"/>',
        f'{s}            <TextBlock x:Name="lblcdefhovered" FontSize="14" FontWeight="SemiBold" TextTrimming="CharacterEllipsis" Text="{T("adv_hover_none", "aim at a prop in the creator")}"/>',
        f'{s}        </StackPanel>',
        f'{s}        <Button x:Name="Btncdefgetmodel" Grid.Column="2" Style="{{StaticResource FormButtonPrimary}}" Height="32" Padding="14,0" Click="{take_click}" IsEnabled="False" ToolTip="{T("cdefgetmodel", "Get Info from hovered Prop")}">',
        f'{s}            <TextBlock Foreground="#FF202225" Text="{T("adv_take", "Take")}"/>',
        f'{s}        </Button>',
        f'{s}    </Grid>',
        f'{s}</Border>',
    ])

    # The position grid keeps its name; the old code folds it with the hidden switch.
    loc_vec = vector('loc', 'Location', [g('tbcdeflocx'), g('tbcdeflocy'), g('tbcdeflocz')], icon_button(g('Btncdefgetloc'), 0), m + 4)
    loc_vec = loc_vec.replace(' CornerRadius="4" Background="{DynamicResource SeactionHeaderBackgroundBrush}"', '')
    fixture = card('card_fixture', 'Object in the world', '\n'.join([
        f'{s}<TextBlock TextWrapping="Wrap" FontSize="13" Foreground="{{StaticResource NavMutedBrush}}" Margin="0,0,0,12" Text="{T("cdef_intro", "Fixtures are objects that already stand in the game world. The job can hide them, freeze them or swap their model.")}"/>',
        hover,
        label('model', 'Model', m),
        reindent(clean(g('tbcdefmodel'), add='Height="30"'), m),
        f'{s}<TextBlock x:Name="lblcdefmodelname" FontSize="12" Foreground="#FF72767D" Margin="2,3,0,10" TextTrimming="CharacterEllipsis"/>',
        f'{s}<StackPanel x:Name="cdefadvanced">',
        loc_vec,
        f'{s}</StackPanel>',
        label('cdef_bulk', 'CodeWalker line', m),
        reindent(clean(g('tbcdefbulk'), add='Height="30" Margin="0,0,0,4"'), m),
        f'{s}<TextBlock FontSize="12" Foreground="#FF72767D" Margin="2,0,0,10" TextWrapping="Wrap" Text="{T("cdef_bulk_tip", "Paste name: hash: X: Y: Z: from CodeWalker to set model and position at once.")}"/>',
    ]), n)

    effect = card('card_fixture_do', 'What happens', '\n'.join([
        toggle('cdefhide', 'Hide', g('cbcdefhide'), m, tip=('cdef_hide_tip', 'Hides the world object at this spot.')),
        toggle('cdeffreeze', 'Freeze', g('cbcdeffreeze'), m, tip=('cdef_freeze_tip', 'Freezes the world object so it cannot be moved.')),
        toggle('cdefmissionent', 'Mission Entity', g('cbcdefmissionent'), m, tip=('cdef_missionent_tip', 'Only used while a cutscene plays.')),
    ]), n)

    raw = '\n'.join([
        two(field('cdef_mnswap', 'Model swap', g('tbcdefmnswap'), m + 4), field('cdef_wprad', 'Search radius', g('tbcdefwprad'), m + 4), m),
        f'{s}<TextBlock FontSize="12" Foreground="#FF72767D" Margin="2,0,0,10" TextWrapping="Wrap" Text="{T("cdef_raw_tip", "Model swap: slot in the job model list (0-39), -1 is off. Search radius in metres, only used with bit 5 or 6 in bits; otherwise 0.1 m.")}"/>',
        raw_grid([('bits', 'tbcdefbits')], g, m),
    ])
    advanced = expander_card('advanced', 'Advanced', raw, n)

    new = page_shell('Pagecentity', 'centitydef', bar, hidden, [fixture, '\n'.join([effect, advanced])], base)
    new = new.replace('<TabItem x:Name="Pagecentity" Header="centitydef" >', opening.strip(), 1)
    new = new.replace('<DockPanel>', '<DockPanel Margin="10,0">', 1)
    return src[:a - base] + new + src[b:]


if __name__ == '__main__':
    before = names(pp.S)
    out = build_fixtures(pp.S)
    after = names(out)
    print('missing names:', sorted(before - after))
    print('new names:', sorted(after - before))
    if '--write' in sys.argv:
        if pp.CRLF:
            out = out.replace('\n', '\r\n')
        open(pp.P, 'w', encoding='utf-8-sig').write(out)
