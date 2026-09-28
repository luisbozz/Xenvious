import re, sys

P = 'Xenvious/MainWindow.xaml'
raw = open(P, encoding='utf-8-sig').read()
CRLF = '\r\n' in raw
S = raw.replace('\r\n', '\n')


def extract(src, name):
    """Returns the full element (with children) that carries x:Name=name."""
    i = src.index(f'x:Name="{name}"')
    start = src.rindex('<', 0, i)
    tag = re.match(r'<([\w:.]+)', src[start:]).group(1)
    # self-closing?
    depth = 0
    pos = start
    pat = re.compile(r'<(/?)' + re.escape(tag) + r'(?=[\s/>])')
    while True:
        m = pat.search(src, pos)
        if not m:
            raise ValueError(name)
        end_of_tag = src.index('>', m.start())
        selfclose = src[end_of_tag - 1] == '/'
        if m.group(1) == '/':
            depth -= 1
            if depth == 0:
                return src[start:end_of_tag + 1]
        elif not selfclose:
            depth += 1
        elif depth == 0:
            return src[start:end_of_tag + 1]
        pos = end_of_tag + 1


def extract_container(src, marker):
    """The element whose opening tag contains marker (unique)."""
    assert src.count(marker) == 1, marker
    i = src.index(marker)
    start = src.rindex('<', 0, i + 1) if src[i] != '<' else i
    tag = re.match(r'<([\w:.]+)', src[start:]).group(1)
    depth = 0
    pos = start
    pat = re.compile(r'<(/?)' + re.escape(tag) + r'(?=[\s/>])')
    while True:
        m = pat.search(src, pos)
        end_of_tag = src.index('>', m.start())
        selfclose = src[end_of_tag - 1] == '/'
        if m.group(1) == '/':
            depth -= 1
            if depth == 0:
                return src[start:end_of_tag + 1]
        elif not selfclose:
            depth += 1
        pos = end_of_tag + 1


DROP = ['Grid.Row', 'Grid.Column', 'Grid.RowSpan', 'Grid.ColumnSpan', 'Margin', 'Height', 'MaxHeight',
        'VerticalAlignment', 'HorizontalAlignment', 'Width']


def clean(el, drop=DROP, add='', drop_tag=True):
    """Strips layout attributes from the opening tag of el and adds new ones."""
    m = re.match(r'<[\w:.]+', el)
    end = el.index('>')
    head = el[:end]
    for a in drop + (['Tag'] if drop_tag else []):
        head = re.sub(r'\s' + re.escape(a) + r'="(?:[^"{]|\{[^}]*\})*"', '', head)
    selfclose = head.endswith('/')
    if selfclose:
        head = head[:-1].rstrip()
    head = head + (' ' + add if add else '') + (' /' if selfclose else '')
    head = head.replace(' /', '/') if selfclose else head
    return head + el[end:]


def indent(block, n):
    lines = block.split('\n')
    # normalise: first line has no indent, the rest keep relative indent
    base = min((len(l) - len(l.lstrip()) for l in lines[1:] if l.strip()), default=0)
    first = lines[0].strip()
    out = [' ' * n + first]
    for l in lines[1:]:
        out.append(' ' * n + l[base:] if l.strip() else '')
    # inner lines were deeper than the opening tag; keep them one level in
    return '\n'.join(out)


def reindent(block, n):
    """Moves a whole element so its opening tag sits at column n."""
    lines = block.split('\n')
    if len(lines) == 1:
        return ' ' * n + block.strip()
    closing = lines[-1]
    base = len(closing) - len(closing.lstrip())
    out = [' ' * n + lines[0].strip()]
    for l in lines[1:]:
        cut = min(base, len(l) - len(l.lstrip()))
        out.append((' ' * n + l[cut:]) if l.strip() else '')
    return '\n'.join(out)


def T(key, fb):
    fb = fb if re.fullmatch(r'[\w .+/#-]*', fb) else f"'{fb}'"
    return f'{{Binding Translation[{key}], FallbackValue={fb}}}'


I = 0  # base indent set per page


def label(key, fb, n):
    return f'{" " * n}<TextBlock Style="{{StaticResource FieldLabel}}" Text="{T(key, fb)}"/>'


def field(key, fb, el, n, margin='0,0,0,10'):
    return label(key, fb, n) + '\n' + reindent(clean(el, add=f'Height="30" Margin="{margin}"'), n)


def two(a, b, n):
    return (f'{" " * n}<Grid>\n'
            f'{" " * n}    <Grid.ColumnDefinitions>\n'
            f'{" " * n}        <ColumnDefinition Width="*"/>\n'
            f'{" " * n}        <ColumnDefinition Width="10"/>\n'
            f'{" " * n}        <ColumnDefinition Width="*"/>\n'
            f'{" " * n}    </Grid.ColumnDefinitions>\n'
            f'{" " * n}    <StackPanel Grid.Column="0">\n{a}\n{" " * n}    </StackPanel>\n'
            f'{" " * n}    <StackPanel Grid.Column="2">\n{b}\n{" " * n}    </StackPanel>\n'
            f'{" " * n}</Grid>')


def vector(key, fb, xs, button, n):
    """X/Y/Z in one row, plus the icon button at the end."""
    s = ' ' * n
    out = [label(key, fb, n), f'{s}<Grid Margin="0,0,0,10">', f'{s}    <Grid.ColumnDefinitions>']
    out += [f'{s}        <ColumnDefinition Width="*"/>', f'{s}        <ColumnDefinition Width="6"/>'] * 3
    out += [f'{s}        <ColumnDefinition Width="30"/>', f'{s}    </Grid.ColumnDefinitions>']
    for i, (axis, el) in enumerate(zip('XYZ', xs)):
        out.append(f'{s}    <StackPanel Grid.Column="{i * 2}">')
        out.append(f'{s}        <TextBlock Style="{{StaticResource FieldAxis}}" Text="{axis}"/>')
        out.append(reindent(clean(el, add='Height="30"'), n + 8))
        out.append(f'{s}    </StackPanel>')
    if button:
        out.append(f'{s}    <Border Grid.Column="6" Width="30" Height="30" VerticalAlignment="Bottom" CornerRadius="4" Background="{{DynamicResource SeactionHeaderBackgroundBrush}}">')
        out.append(reindent(button, n + 8))
        out.append(f'{s}    </Border>')
    out.append(f'{s}</Grid>')
    return '\n'.join(out)


def toggle(key, fb, cb, n, tip=None):
    s = ' ' * n
    tt = f' ToolTip="{T(*tip)}"' if tip else ''
    cbname = re.search(r'x:Name="([^"]+)"', cb).group(1)
    return (f'{s}<Grid Style="{{StaticResource FormRow}}">\n'
            f'{s}    <Grid.ColumnDefinitions>\n{s}        <ColumnDefinition Width="*"/>\n{s}        <ColumnDefinition Width="46"/>\n{s}    </Grid.ColumnDefinitions>\n'
            f'{s}    <TextBlock Style="{{StaticResource FormLabel}}" Text="{T(key, fb)}"{tt} IsEnabled="{{Binding ElementName={cbname}, Path=IsEnabled}}"/>\n'
            + reindent(clean(cb, add='Grid.Column="1" Style="{StaticResource FormToggle}"'), n + 4) + '\n'
            f'{s}</Grid>')


def card(key, fb, body, n, extra_header=''):
    s = ' ' * n
    return (f'{s}<Border Style="{{StaticResource DashCard}}">\n'
            f'{s}    <DockPanel>\n'
            f'{s}        <Border DockPanel.Dock="Top" Style="{{StaticResource DashCardHeader}}">\n'
            f'{s}            <TextBlock Style="{{StaticResource DashCardTitle}}" Text="{T(key, fb)}"/>\n'
            f'{s}        </Border>\n'
            f'{s}        <StackPanel Margin="14,12,14,4">\n{body}\n{s}        </StackPanel>\n'
            f'{s}    </DockPanel>\n'
            f'{s}</Border>')


def expander_card(key, fb, body, n):
    """Card that starts folded; for raw values nobody needs every day."""
    s = ' ' * n
    return (f'{s}<Border Style="{{StaticResource DashCard}}">\n'
            f'{s}    <Expander Style="{{StaticResource CardExpander}}" Header="{T(key, fb)}">\n'
            f'{s}        <StackPanel Margin="14,12,14,4">\n{body}\n{s}        </StackPanel>\n'
            f'{s}    </Expander>\n'
            f'{s}</Border>')


def sep(n):
    return f'{" " * n}<Rectangle Height="1" Fill="{{DynamicResource SeactionHeaderBackgroundBrush}}" Margin="0,4,0,12"/>'


def sub(key, fb, n):
    return f'{" " * n}<TextBlock Style="{{StaticResource CardSub}}" Text="{T(key, fb)}"/>'


ICON_TRASH = 'M3,4 L13,4 M6,4 L6,2.5 L10,2.5 L10,4 M4.5,4 L5.2,14 L10.8,14 L11.5,4'
ICON_PLUS = 'M8,3 L8,13 M3,8 L13,8'


def entry_bar(combo, count_label, move_btn, delete_btn, add_btn, n):
    """Top bar of an entry page: how many there are, ‹ number ›, move, delete, add."""
    s = ' ' * n
    combo_name = re.search(r'x:Name="([^"]+)"', combo).group(1)
    combo_el = clean(combo, add='Width="96" Height="28" Margin="2,0"')
    move_el = clean(move_btn, drop=DROP + ['Style', 'local:CornerRadiusSetter.CornerRadius', 'Foreground', 'Padding'],
                    add='Style="{StaticResource FormButton}" Height="32" Padding="12,0" Margin="0,0,10,0"', drop_tag=False)
    # delete / add: icon buttons instead of text boxes
    del_name = re.search(r'x:Name="([^"]+)"', delete_btn).group(1)
    del_click = re.search(r'Click="([^"]+)"', delete_btn).group(1)
    add_name = re.search(r'x:Name="([^"]+)"', add_btn).group(1)
    add_click = re.search(r'Click="([^"]+)"', add_btn).group(1)
    return f'''{s}<Border DockPanel.Dock="Top" Background="{{DynamicResource SectionBackgroundBrush}}" CornerRadius="5" Padding="12,8" Margin="0,10,0,12">
{s}    <DockPanel>
{s}        <StackPanel DockPanel.Dock="Right" Orientation="Horizontal">
{reindent(move_el, n + 12)}
{s}            <!-- ‹ number › : steps through the entries without opening the list. -->
{s}            <Border Background="#FF232529" CornerRadius="5" Padding="3" Margin="0,0,10,0">
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
{s}                    <TextBlock VerticalAlignment="Center" Text="{T('add', 'Add')}"/>
{s}                </StackPanel>
{s}            </Button>
{s}        </StackPanel>
{s}        <TextBlock VerticalAlignment="Center" FontSize="14" Foreground="{{StaticResource NavMutedBrush}}">
{s}            <Run FontSize="17" FontWeight="Bold" Foreground="{{DynamicResource TextColor}}" Text="{{Binding Items.Count, ElementName={combo_name}, Mode=OneWay}}"/>
{s}            <Run Text="{T(count_label[0], count_label[1])}"/>
{s}        </TextBlock>
{s}    </DockPanel>
{s}</Border>'''


def page_shell(tab_name, header, bar, hidden, columns, n):
    s = ' ' * n
    cols = '\n'.join(f'{s}                <StackPanel Width="340" Margin="0,0,12,0">\n{c}\n{s}                </StackPanel>' for c in columns)
    return f'''{s}<TabItem x:Name="{tab_name}" Header="{header}" >

{s}    <!-- Entry bar on top, then cards by topic: model and position, look, rules, raw values. -->
{s}    <DockPanel>
{bar}
{s}        <!-- The old model controls: hidden, but their code still reads and fills them. -->
{s}        <StackPanel DockPanel.Dock="Top" Visibility="Collapsed">
{hidden}
{s}        </StackPanel>
{s}        <ScrollViewer VerticalScrollBarVisibility="Auto" HorizontalScrollBarVisibility="Disabled">
{s}            <WrapPanel Orientation="Horizontal" Margin="0,0,0,12">
{cols}
{s}            </WrapPanel>
{s}        </ScrollViewer>
{s}    </DockPanel>

{s}</TabItem>'''


def tab_range(src, name):
    el = extract(src, name)
    i = src.index(el)
    return i, i + len(el), el


def build_normal(src):
    a, b, old = tab_range(src, 'PageInnerNormalProps')
    base = len(src[:a].split('\n')[-1])  # indent of <TabItem
    n = base + 20  # inside the column StackPanels
    g = lambda nm: extract(old, nm)
    hidden_box = extract_container(old, '<Border Grid.Row="1" Visibility="Collapsed"')
    hidden = '\n'.join([reindent(g('Lblpropmodel'), base + 12), reindent(hidden_box, base + 12)])
    bar = entry_bar(g('ddpropno'), ('props_placed', 'props placed'), g('BtnPropsToDynamic'), g('BtnPropsDelete'), g('BtnPropsAdd'), base + 8)

    model = reindent('<local:ModelCard x:Name="PropModelCard" Margin="0,0,0,12"/>', n)
    getloc = g('Btnpropsgetloc')
    rotchain = g('Btnpropsrotchain')
    pos_body = '\n'.join([
        vector('loc', 'Location', [g('tbpropslocx'), g('tbpropslocy'), g('tbpropslocz')], getloc, n + 8),
        vector('vrot', 'Rotation', [g('tbpropsrotx'), g('tbpropsroty'), g('tbpropsrotz')], rotchain, n + 8),
        two(field('heading', 'Heading', g('tbpropshead'), n + 8), '', n + 8),
        sep(n + 8),
        sub('lock', 'Lock', n + 8),
        toggle('loc', 'Location', g('cb_props_lockpos'), n + 8),
        toggle('vrot', 'Rotation', g('cb_props_lockrot'), n + 8),
    ])
    position = card('card_position', 'Position', pos_body, n)

    # "Hide after" keeps its expand button and timing list.
    has_tb = clean(g('tbpropshas'), add='Height="30"')
    has_btn = g('Btnpropshasexpand')
    has_list = clean(g('haslistcontainer'), add='Grid.Row="1" Grid.ColumnSpan="2" Margin="0,6,0,0"')
    s = ' ' * (n + 8)
    hide = '\n'.join([
        label('prophas', 'Hide Prop after', n + 8),
        f'{s}<Grid Margin="0,0,0,10">',
        f'{s}    <Grid.ColumnDefinitions>',
        f'{s}        <ColumnDefinition Width="*"/>',
        f'{s}        <ColumnDefinition Width="36"/>',
        f'{s}    </Grid.ColumnDefinitions>',
        f'{s}    <Grid.RowDefinitions>',
        f'{s}        <RowDefinition Height="Auto"/>',
        f'{s}        <RowDefinition Height="Auto"/>',
        f'{s}    </Grid.RowDefinitions>',
        reindent(has_tb, n + 12),
        f'{s}    <Border Grid.Column="1" Width="30" Height="30" HorizontalAlignment="Right" CornerRadius="4" Background="{{DynamicResource SeactionHeaderBackgroundBrush}}">',
        reindent(has_btn, n + 16),
        f'{s}    </Border>',
        reindent(has_list, n + 12),
        f'{s}</Grid>',
    ])
    look_body = '\n'.join([
        field('propcolor', 'Prop Color', g('ddpropscolor'), n + 8),
        field('boosterspeed', 'Booster/Slowdown Speed', g('ddpropsboosterspeed'), n + 8),
        two(field('renderdist', 'Render Distance', g('tbpropsrender'), n + 12), '', n + 8),
        hide,
        toggle('invisible', 'Invisible', g('cb_props_invisible'), n + 8),
        toggle('remcollision', 'Remove Collision', g('cb_props_nocollision'), n + 8, tip=('remcollision_tip', 'Only in missions')),
    ])
    look = card('card_look', 'Look', look_body, n)

    rules_body = '\n'.join([
        two(field('asrlspawnon', 'Spawn On', g('ddpropsspawnon'), n + 12),
            field('asrlspawnteam', 'Spawn by Team', g('ddpropsspawnteam'), n + 12), n + 8),
        two(field('asrlspawn', 'Spawn at Rule', g('tbpropsspawnrule'), n + 12), '', n + 8),
        sep(n + 8),
        two(field('asrlclteam', 'Clear up by Team', g('ddpropsteamclear'), n + 12),
            field('asrlclear', 'Clear up at Rule', g('tbpropsclearrule'), n + 12), n + 8),
        toggle('clrlivc', 'Ignore Visibility Check on Clean up', g('cb_props_ignorevscheck'), n + 8, tip=('clrlivc', 'Ignore Visibility Check on Clean up')),
    ])
    rules = card('aslrheader', 'Associated Rule', rules_body, n)

    raw_pairs = [('prpbs', 'tbpropssettings1'), ('prpbs2', 'tbpropssettings2'), ('prpcr', 'tbpropsprpcr'), ('prpct', 'tbpropsprpct'),
                 ('asso', 'tbpropsasso'), ('asst', 'tbpropsasst'), ('asss', 'tbpropsasss'), ('pasc', 'tbpropspasc'), ('prpsdp', 'tbpropsprpsdp')]
    raw = raw_grid(raw_pairs, g, n + 8, team=('actorteam', 'Team', g('ddpropsteamrlprio')))
    advanced = expander_card('advanced', 'Advanced', raw, n)

    new = page_shell('PageInnerNormalProps', 'Normal Props', bar, hidden,
                     ['\n'.join([model, position]), look, '\n'.join([rules, advanced])], base)
    return src[:a - base] + new + src[b:]


def raw_grid(pairs, g, n, team=None):
    """Raw values two per row, labelled with their name in the job data."""
    rows = []
    items = [(k, g(nm)) for k, nm in pairs]
    if team:
        items.insert(0, (team, None))
    for i in range(0, len(items), 2):
        cells = []
        for k, el in items[i:i + 2]:
            if el is None:
                key, fb, e = k
                cells.append(field(key, fb, e, n + 4))
            else:
                cells.append(f'{" " * (n + 4)}<TextBlock Style="{{StaticResource FieldLabelRaw}}" Text="{k}"/>\n'
                             + reindent(clean(el, add='Height="30" Margin="0,0,0,10"'), n + 4))
        while len(cells) < 2:
            cells.append('')
        rows.append(two(cells[0], cells[1], n))
    return '\n'.join(rows)


def build_dynamic(src):
    a, b, old = tab_range(src, 'PageInnerDynamicProps')
    base = len(src[:a].split('\n')[-1])
    n = base + 20
    g = lambda nm: extract(old, nm)
    hidden_box = extract_container(old, '<Border Grid.Row="1" Visibility="Collapsed"')
    hidden = '\n'.join([reindent(g('Lbldpropmodel'), base + 12), reindent(hidden_box, base + 12)])
    bar = entry_bar(g('dddpropno'), ('dprops_placed', 'dynamic props placed'), g('BtnDPropsToStatic'), g('BtndpropsDelete'), g('Btndpropsadd'), base + 8)

    model = reindent('<local:ModelCard x:Name="DPropModelCard" Margin="0,0,0,12"/>', n)
    pos_body = '\n'.join([
        vector('loc', 'Location', [g('tbdpropslocx'), g('tbdpropslocy'), g('tbdpropslocz')], g('Btndpropsgetloc'), n + 8),
        vector('vrot', 'Rotation', [g('tbdpropsrotx'), g('tbdpropsroty'), g('tbdpropsrotz')], g('Btndpropsrotchain'), n + 8),
        two(field('heading', 'Heading', g('tbdpropshead'), n + 12), '', n + 8),
    ])
    position = card('card_position', 'Position', pos_body, n)
    look_body = '\n'.join([
        field('propcolor', 'Prop Color', g('dddpropscolor'), n + 8),
        two(field('dpropdptrpx', 'Activation Timer', g('tbdpropsdptrpx'), n + 12), '', n + 8),
    ])
    look = card('card_look', 'Look', look_body, n)
    rules_body = '\n'.join([
        two(field('asrlspawnon', 'Spawn On', g('dddpropsspawnon'), n + 12),
            field('asrlspawnteam', 'Spawn by Team', g('dddpropsspawnteam'), n + 12), n + 8),
        two(field('asrlspawn', 'Spawn at Rule', g('tbdpropsspawnrule'), n + 12), '', n + 8),
        sep(n + 8),
        two(field('asrlclteam', 'Clear up by Team', g('dddpropsteamclear'), n + 12),
            field('asrlclear', 'Clear up at Rule', g('tbdpropsclearrule'), n + 12), n + 8),
        toggle('clrlivc', 'Ignore Visibility Check on Clean up', g('cb_dprops_ignorevscheck'), n + 8, tip=('clrlivc', 'Ignore Visibility Check on Clean up')),
    ])
    rules = card('aslrheader', 'Associated Rule', rules_body, n)
    raw_pairs = [('prpbs', 'tbdpropssettings1'), ('prpcr', 'tbdpropsprpcr'), ('prpct', 'tbdpropsprpct'),
                 ('asso', 'tbdpropsasso'), ('asst', 'tbdpropsasst'), ('asss', 'tbdpropsasss'), ('pasc', 'tbdpropspasc')]
    raw = raw_grid(raw_pairs, g, n + 8, team=('actorteam', 'Team', g('dddpropsteamrlprio')))
    advanced = expander_card('advanced', 'Advanced', raw, n)
    new = page_shell('PageInnerDynamicProps', 'Dynamic Props', bar, hidden,
                     ['\n'.join([model, position]), '\n'.join([look, rules]), advanced], base)
    return src[:a - base] + new + src[b:]


def names(src):
    return set(re.findall(r'x:Name="([^"]+)"', src))


if __name__ == '__main__':
    before = names(S)
    out = build_normal(S)
    out = build_dynamic(out)
    after = names(out)
    missing = before - after
    print('missing names:', sorted(missing))
    print('new names:', sorted(after - before))
    if '--write' in sys.argv:
        if CRLF:
            out = out.replace('\n', '\r\n')
        open(P, 'w', encoding='utf-8-sig').write(out)

