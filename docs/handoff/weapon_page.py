import sys
sys.path.insert(0, 'docs/handoff')
from vehicle_page import *  # noqa  (entry_bar in the current style, icon_button, toggles)
import props_page as pp


def build_weapon(src):
    a, b, old = tab_range(src, 'PageWeapon')
    base = len(src[:a].split('\n')[-1])
    n = base + 20
    m = n + 8
    s = ' ' * m
    g = lambda nm: extract(old, nm)
    opening = old[:old.index('>') + 1]

    hidden_box = extract_container(old, '<Border Grid.Row="1" CornerRadius="5" Background="{DynamicResource SeactionHeaderBackgroundBrush}" Margin="8">')
    hidden = '\n'.join([reindent(g('Lblweapmodel'), base + 12), reindent(hidden_box, base + 12)])
    bar = entry_bar(g('ddweapno'), ('weapons_placed', 'weapons placed'), g('BtnWeapDelete'), g('BtnWeapAdd'), base + 8)

    model = reindent('<local:ModelCard x:Name="WeapModelCard" Margin="0,0,0,12"/>', n)

    placement = card('card_position', 'Placement', '\n'.join([
        vector('loc', 'Location', [g('tbweaplocx'), g('tbweaplocy'), g('tbweaplocz')], icon_button(g('Btnweapgetloc'), 0), m),
        two(field('heading', 'Heading', g('tbweapheading'), m + 4), '', m),
        toggle('weapenablerot', 'Enable Rotation', g('cbweapenablerot'), m),
        two(field('rotx', 'Rotation X', g('tbweaprotx'), m + 4), field('roty', 'Rotation Y', g('tbweaproty'), m + 4), m),
    ]), n).replace(' CornerRadius="4" Background="{DynamicResource SeactionHeaderBackgroundBrush}"', '')

    # The invisible switch keeps its named label.
    inv_label = clean(g('Lbl_weap_invisible'), drop=DROP + ['FontSize', 'VerticalAlignment', 'TextTrimming', 'Grid.Column'],
                      add='Style="{StaticResource FormLabel}"')
    invisible = '\n'.join([
        f'{s}<Grid Style="{{StaticResource FormRow}}">',
        f'{s}    <Grid.ColumnDefinitions>\n{s}        <ColumnDefinition Width="*"/>\n{s}        <ColumnDefinition Width="46"/>\n{s}    </Grid.ColumnDefinitions>',
        reindent(inv_label, m + 4),
        reindent(clean(g('cb_weap_invisible'), add='Grid.Column="1" Style="{StaticResource FormToggle}"'), m + 4),
        f'{s}</Grid>',
    ])
    options = card('options', 'Options', '\n'.join([
        two(field('dmgmult', 'Damage Multiplier', g('tbweapdmgmlt'), m + 4), '', m),
        invisible,
        toggles([('weapciv', 'Collect in Vehicle', 'cb_weap_civ', None)], g, m),
        sep(m),
        toggles([(f'weapbrest{t}', f'Hide for Team {t}', f'cb_weap_brest{t}', None) for t in range(1, 5)], g, m),
    ]), n)

    rules = card('aslrheader', 'Associated Rule', '\n'.join([
        two(field('asrlspawnon', 'Spawn On', g('ddweapspawnon'), m + 4), field('asrlspawnteam', 'Spawn by Team', g('ddweapspawnteam'), m + 4), m),
        two(field('asrlspawn', 'Spawn at Rule', g('tbweapspawnrule'), m + 4), '', m),
        sep(m),
        two(field('asrlclteam', 'Clear up by Team', g('ddweapteamclear'), m + 4), field('asrlclear', 'Clear up at Rule', g('tbweapclearrule'), m + 4), m),
    ]), n)

    raw = '\n'.join([
        two(field('actorteam', 'Team', g('ddweaponteam'), m + 4), '', m),
        raw_grid([('vput', 'tbweapvput'), ('rput', 'tbweaprput'), ('bits', 'tbweapbits'), ('clip', 'tbweapclip'),
                  ('sub', 'tbweapsub'), ('iptnp', 'tbweapiptnp'), ('wcpm', 'tbweapwcpm'), ('vclnrl', 'tbweapvclnrl'),
                  ('vclnr', 'tbweapvclnr'), ('vclnt', 'tbweapvclnt'), ('vasso', 'tbweapvasso'), ('vasss', 'tbweapvasss'),
                  ('vasst', 'tbweapvasst')], g, m),
    ])
    advanced = expander_card('advanced', 'Advanced', raw, n)

    new = page_shell('PageWeapon', 'Weapon', bar, hidden, [model, placement, options, '\n'.join([rules, advanced])], base)
    new = new.replace('<TabItem x:Name="PageWeapon" Header="Weapon" >', opening.strip(), 1)
    new = new.replace('<DockPanel>', '<DockPanel Margin="10,0">', 1)
    return src[:a - base] + new + src[b:]


if __name__ == '__main__':
    before = names(pp.S)
    out = build_weapon(pp.S)
    after = names(out)
    print('missing names:', sorted(before - after))
    print('new names:', sorted(after - before))
    if '--write' in sys.argv:
        if pp.CRLF:
            out = out.replace('\n', '\r\n')
        open(pp.P, 'w', encoding='utf-8-sig').write(out)
