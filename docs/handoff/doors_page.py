import sys
sys.path.insert(0, 'docs/handoff')
from vehicle_page import *  # noqa  (entry_bar in the current style, icon_button, toggles)
import props_page as pp

NO_BOX = ' CornerRadius="4" Background="{DynamicResource SeactionHeaderBackgroundBrush}"'


def build_doors(src):
    a, b, old = tab_range(src, 'PageDoors')
    base = len(src[:a].split('\n')[-1])
    n = base + 20
    m = n + 8
    g = lambda nm: extract(old, nm)
    opening = old[:old.index('>') + 1]

    bar = entry_bar(g('dddoorsno'), ('doors_placed', 'doors placed'), g('BtndoorsDelete'), g('BtndoorsAdd'), base + 8)

    placement = card('card_position', 'Placement', '\n'.join([
        vector('loc', 'Location', [g('tbdoorslocx'), g('tbdoorslocy'), g('tbdoorslocz')], icon_button(g('Btndoorsgetloc'), 0), m),
        two(field('model', 'Model', g('tbdoorsmodel'), m + 4), '', m),
    ]), n).replace(NO_BOX, '')

    door = card('card_door', 'Door', '\n'.join([
        two(field('fopen', 'Open Ratio', g('tbdoorsfopen'), m + 4), '', m),
        toggles([('doorslock', 'Lock', 'cbdoorslock', None),
                 ('doorsswing', 'Swing Free', 'cbdoorsswing', None)], g, m),
    ]), n)

    update = card('doorsupdate', 'Update on', '\n'.join([
        two(field('doorsudrle', 'Update on Priority', g('tbdoorsudrle'), m + 4), field('doorsudtem', 'Update by Team', g('tbdoorsudtem'), m + 4), m),
        toggles([('doorsmid', 'Update on Mid Point', 'cbdoorsmid', None),
                 ('doorsswingu', 'Swing Free on Update', 'cbdoorsswingu', None)], g, m),
        two(field('doorsudrat', 'Open Ratio on Update', g('tbdoorsudrat'), m + 4), '', m),
        sep(m),
        raw_grid([('uaurt', 'tbdoorsuaurt'), ('uaudst', 'tbdoorsuaudst'), ('dirud', 'tbdoorsdirud')], g, m),
        vector('dirduv', 'dirduv', [g('tbdoorsdirduvx'), g('tbdoorsdirduvy'), g('tbdoorsdirduvz')], icon_button(g('Btndoorsdirduvgetloc'), 0), m),
    ]), n).replace(NO_BOX, '')

    raw = '\n'.join([
        raw_grid([('bits', 'tbdoorsbits'), ('fcz', 'tbdoorsfcz'), ('foz', 'tbdoorsfoz'), ('aurt', 'tbdoorsaurt'),
                  ('audst', 'tbdoorsaudst'), ('dird', 'tbdoorsdird')], g, m),
        vector('dirdv', 'dirdv', [g('tbdoorsdirdvx'), g('tbdoorsdirdvy'), g('tbdoorsdirdvz')], icon_button(g('Btndoorsdirdvgetloc'), 0), m),
        raw_grid([('org', 'tbdoorsorg'), ('dcoid', 'tbdoorsdcoid'), ('dtime', 'tbdoorsdtime'), ('ornlo', 'tbdoorsornlo'),
                  ('dle', 'tbdoorsdle')], g, m),
        toggle('lfp', 'lfp', g('cbdoorslfp'), m),
    ])
    advanced = expander_card('advanced', 'Advanced', raw, n).replace(NO_BOX, '')

    new = page_shell('PageDoors', 'Doors', bar, '', ['\n'.join([placement, door]), update, advanced], base)
    new = re.sub(r'\n *<!-- The old model controls[^\n]*\n *<StackPanel DockPanel.Dock="Top" Visibility="Collapsed">\n\n *</StackPanel>', '', new)
    new = new.replace('<TabItem x:Name="PageDoors" Header="Doors" >', opening.strip(), 1)
    new = new.replace('<DockPanel>', '<DockPanel Margin="10,0">', 1)
    return src[:a - base] + new + src[b:]


if __name__ == '__main__':
    before = names(pp.S)
    out = build_doors(pp.S)
    after = names(out)
    print('missing names:', sorted(before - after))
    print('new names:', sorted(after - before))
    if '--write' in sys.argv:
        if pp.CRLF:
            out = out.replace('\n', '\r\n')
        open(pp.P, 'w', encoding='utf-8-sig').write(out)
