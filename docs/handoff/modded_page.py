import sys
sys.path.insert(0, 'docs/handoff')
from auto_page import *  # noqa


def find(n, name):
    if n.name() == name:
        return n
    for c in n.children:
        r = find(c, name)
        if r:
            return r


LAYOUT = ['Grid.Row', 'Grid.Column', 'Grid.RowSpan', 'Grid.ColumnSpan', 'Margin', 'Height', 'Width', 'VerticalAlignment', 'HorizontalAlignment']


def build_modded(src):
    a, b, old = tab_range(src, 'PageInnerModdedProps')
    base = len(src[:a].split('\n')[-1])
    opening = old[:old.index('>') + 1]
    roots = parse(src, a + len(opening), b - len('</TabItem>'))
    root = roots[0]
    g = lambda nm: find(root, nm).text(src)
    t = ' ' * base
    n = base + 24
    m = n + 8
    s = ' ' * m

    def btn(name):
        return reindent(clean(g(name), drop=DROP + ['Style', 'local:CornerRadiusSetter.CornerRadius'],
                              add='Style="{StaticResource FormButton}" Height="32" Margin="0,0,0,8"', drop_tag=False), m)

    settings = card('settings', 'Settings', '\n'.join([
        field('set_creator', 'Creator', g('ddMPROPSCreator'), m),
        field('category', 'Category', g('ddMPropsReplaceCategory'), m),
        sep(m),
        btn('Btnmpropsrestore'), btn('Btnmpropsimport'), btn('Btnmpropsexport'),
    ]), n)

    # Model list with the bulk text box over it (the switch shows one or the other),
    # then the new model and the two replace buttons.
    bulk_grid = '\n'.join([
        f'{s}<Grid x:Name="mpropsmaingrid">',
        f'{s}    <Grid.RowDefinitions>',
        f'{s}        <RowDefinition Height="360"/>',
        f'{s}        <RowDefinition Height="Auto"/>',
        f'{s}        <RowDefinition Height="Auto"/>',
        f'{s}    </Grid.RowDefinitions>',
        reindent(clean(g('tbmpropsbulk'), drop=LAYOUT, add='Margin="0,0,0,8"', drop_tag=False), m + 4),
        reindent(clean(g('mpropModelList'), drop=LAYOUT, add='Margin="0,0,0,8"'), m + 4),
        f'{s}    <StackPanel Grid.Row="1">',
        label('mp_newmodel', 'New model (hash)', m + 8),
        reindent(clean(g('tbmpropsmodelchange'), drop=LAYOUT, add='Height="30" Margin="0,0,0,10"'), m + 8),
        f'{s}    </StackPanel>',
        f'{s}    <Grid Grid.Row="2">',
        f'{s}        <Grid.ColumnDefinitions>\n{s}            <ColumnDefinition Width="*"/>\n{s}            <ColumnDefinition Width="8"/>\n{s}            <ColumnDefinition Width="*"/>\n{s}        </Grid.ColumnDefinitions>',
        reindent(clean(g('Btnmpropsreplace'), drop=DROP + ['Style', 'local:CornerRadiusSetter.CornerRadius'],
                       add='Style="{StaticResource FormButtonPrimary}" Height="32" Margin="0,0,0,8"', drop_tag=False), m + 8),
        reindent(clean(g('Btnmpropsreplaceall'), drop=DROP + ['Style', 'local:CornerRadiusSetter.CornerRadius'],
                       add='Grid.Column="2" Style="{StaticResource FormButton}" Height="32" Margin="0,0,0,8"', drop_tag=False), m + 8),
        f'{s}    </Grid>',
        f'{s}</Grid>',
    ])
    bulk = card('bulkedit', 'Bulk edit', '\n'.join([
        toggle('bulkedit', 'Bulk edit', g('cbmpbulk'), m),
        bulk_grid,
    ]), n)

    rows = []
    for key, text in [('race', 'Race Creator'), ('lts', 'LTS Creator'), ('capture', 'Capture Creator'),
                      ('dm', 'Deathmatch Creator'), ('survival', 'Survival Creator')]:
        lbl = clean(g(f'lblmpropsinitialised{key}'), drop=LAYOUT + ['FontSize', 'TextTrimming'],
                    add='Grid.Column="1" FontSize="13" VerticalAlignment="Center"')
        rows.append('\n'.join([
            f'{s}<Grid Style="{{StaticResource FormRow}}">',
            f'{s}    <Grid.ColumnDefinitions>\n{s}        <ColumnDefinition Width="*"/>\n{s}        <ColumnDefinition Width="Auto"/>\n{s}    </Grid.ColumnDefinitions>',
            f'{s}    <TextBlock Style="{{StaticResource FormLabel}}" Text="{text}"/>',
            reindent(lbl, m + 4),
            f'{s}</Grid>']))
    advanced = card('advanced', 'Advanced', '\n'.join(
        [sub('mp_loaded', 'Loaded in', m)] + rows + [
            sep(m),
            field('mpcategory', 'Override Prop Category', g('ddMPropsCategory'), m),
            toggle('mpforcemurica', 'Enable Murica Color', g('cbMPropsForceMurica'), m),
        ]), n)

    drop = clean(g('mpropsdrop'), drop=['Grid.Row', 'Grid.Column', 'Grid.RowSpan', 'Grid.ColumnSpan'], drop_tag=False)
    anim_grid = find(root, 'mpropsloadanimation').parent
    anim = clean(anim_grid.text(src), drop=['Grid.Row', 'Grid.Column', 'Grid.RowSpan', 'Grid.ColumnSpan'], drop_tag=False)

    cols = '\n'.join(f'{t}                        <StackPanel Width="340" Margin="0,0,12,0">\n{c}\n{t}                        </StackPanel>'
                     for c in [settings, bulk, advanced])
    new = f'''{t}{opening.strip()}

{t}    <!-- Modded props: settings, the model list to swap (bulk edit) and the advanced options as cards.
{t}         mpropspanelmainmain is hidden while loading, mpropsdrop is the import overlay. -->
{t}    <Border x:Name="mpropspanel" Background="Transparent">
{t}        <Grid>
{t}            <Grid x:Name="mpropspanelmainmain">
{t}                <ScrollViewer VerticalScrollBarVisibility="Auto" HorizontalScrollBarVisibility="Disabled">
{t}                    <WrapPanel x:Name="mpropspanelmain" Orientation="Horizontal" Margin="10,12,10,12">
{cols}
{t}                    </WrapPanel>
{t}                </ScrollViewer>
{reindent(drop, base + 16)}
{t}            </Grid>
{reindent(anim, base + 12)}
{t}        </Grid>
{t}    </Border>

{t}</TabItem>'''
    return src[:a - base] + new + src[b:]


if __name__ == '__main__':
    pp.S = open(pp.P, encoding='utf-8-sig').read().replace('\r\n', '\n')
    before = live_names(pp.S)
    out = build_modded(pp.S)
    after = live_names(out)
    print('missing', sorted(before - after), 'new', sorted(after - before))
    if '--write' in sys.argv and not (before - after):
        open(pp.P, 'w', encoding='utf-8-sig').write(out.replace('\n', '\r\n') if pp.CRLF else out)
        print('written')
