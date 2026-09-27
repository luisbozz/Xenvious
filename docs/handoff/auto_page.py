"""Generic converter for the old settings pages (columns of 290 px with bold headings,
label + checkbox rows, watermark text boxes, X/Y/Z triples with a cursor button).

It keeps every element that has an x:Name (and its handlers) and rebuilds the page in the
card layout: each old column becomes a card, the first heading is the card title, later
headings become CardSub, X/Y/Z triples become one vector row, label + checkbox rows become
FormRow switches, text boxes and dropdowns get a FieldLabel above them (from their Tag).
Containers that carry an x:Name, a Visibility or an IsEnabled are kept as a StackPanel with
the same attributes, so code and bindings that hide them keep working.

Use: build_auto(src, 'PageInnerMissionGeneral', ...) returns the new source. Always check the
"missing names" output; a missing name means the converter dropped something the code uses.
"""
import re
import sys
sys.path.insert(0, 'docs/handoff')
from vehicle_page import *  # noqa
import props_page as pp


# ----- tiny XAML tree over the original text -----

class Node:
    def __init__(self, tag, start, open_end, attrs):
        self.tag, self.start, self.open_end, self.attrs = tag, start, open_end, attrs
        self.end = open_end
        self.children = []
        self.parent = None

    def text(self, src):
        return src[self.start:self.end]

    def name(self):
        return self.attrs.get('x:Name')

    def elems(self):
        return [c for c in self.children if '.' not in c.tag]


ATTR = re.compile(r'([\w:.]+)\s*=\s*"((?:[^"])*)"')


def parse(src, start, end):
    """Elements between start and end as a list of root nodes."""
    roots, stack, i = [], [], start
    while i < end:
        j = src.find('<', i, end)
        if j < 0:
            break
        if src.startswith('<!--', j):
            i = src.index('-->', j) + 3
            continue
        # find the end of the tag outside quotes
        k, q = j + 1, False
        while True:
            ch = src[k]
            if ch == '"':
                q = not q
            elif ch == '>' and not q:
                break
            k += 1
        tagtext = src[j:k + 1]
        if tagtext.startswith('</'):
            node = stack.pop()
            node.end = k + 1
        else:
            m = re.match(r'<([\w:.]+)', tagtext)
            node = Node(m.group(1), j, k + 1, dict(ATTR.findall(tagtext)))
            if stack:
                node.parent = stack[-1]
                stack[-1].children.append(node)
            else:
                roots.append(node)
            if tagtext.endswith('/>'):
                node.end = k + 1
            else:
                stack.append(node)
        i = k + 1
    return roots


CONTROLS = {'CheckBox', 'TextBox', 'ComboBox', 'Button', 'ListView', 'ListBox', 'Slider', 'RadioButton',
            'PasswordBox', 'ToggleButton', 'Image', 'ProgressBar', 'DataGrid', 'Canvas', 'TabControl'}
CONTAINERS = {'Grid', 'StackPanel', 'Border', 'WrapPanel', 'DockPanel', 'ScrollViewer', 'Viewbox', 'UniformGrid'}
KEEP_ATTRS = ('x:Name', 'Visibility', 'IsEnabled', 'ToolTip', 'DataContext')


def is_control(n):
    return n.tag in CONTROLS or (n.tag.startswith('local:') and '.' not in n.tag)


def contains_control(n):
    return is_control(n) or any(contains_control(c) for c in n.elems())


def is_heading(n):
    if n.tag not in ('TextBlock', 'Label'):
        return False
    size = n.attrs.get('FontSize', '0')
    try:
        size = float(size)
    except ValueError:
        size = 0
    return n.attrs.get('FontWeight') == 'Bold' or size >= 21


def text_of(n):
    """Binding or plain text of a TextBlock/Label."""
    return n.attrs.get('Text') or n.attrs.get('Content') or ''


def label_from_tag(ctrl):
    tag = ctrl.attrs.get('Tag', '')
    if tag.startswith('{Binding Translation['):
        return tag
    if tag and not tag.startswith('{'):
        return tag
    tip = ctrl.attrs.get('ToolTip', '')
    if tip.startswith('{Binding Translation['):
        return tip
    return ''


# ----- items -----

SKIP = set()  # names moved to the bar on top; the cards leave them out

def walk(n, src, out):
    """Turns node n into items (tuples) appended to out."""
    if n.name() in SKIP:
        return
    if '.' in n.tag or n.tag in ('Rectangle',) and not n.name():
        if n.tag == 'Rectangle':
            out.append(('sep',))
        return
    if is_control(n):
        out.append(control_item(n, src, ''))
        return
    if n.tag in ('TextBlock', 'Label'):
        if is_heading(n) and not n.name():
            out.append(('head', text_of(n)))
        elif n.name():
            out.append(('raw', n.text(src)))
        return
    if n.tag in ('Path', 'Ellipse', 'Line', 'Polygon') and not n.name():
        return
    kids = n.elems()
    # a row: one label and one control (maybe wrapped in a Border)
    row = as_row(n, src)
    if row == ('skip',):
        return
    if row:
        items = [row]
    else:
        items = []
        for c in kids:
            walk(c, src, items)
        items = merge_vectors(items, src)
    keep = {k: v for k, v in n.attrs.items() if k in KEEP_ATTRS and not (k == 'Visibility' and v == 'Visible')}
    if n.tag in CONTAINERS and keep and any(i[0] != 'head' for i in items):
        out.append(('group', keep, items))
    elif n.tag in CONTAINERS and n.name() and not items:
        # filled by code (e.g. a WrapPanel of generated checkboxes): keep as it is
        out.append(('raw', n.text(src)))
    elif n.tag not in CONTAINERS and n.tag not in ('TextBlock', 'Label') and n.name():
        out.append(('raw', n.text(src)))
    else:
        out.extend(items)


def control_item(ctrl, src, label):
    if ctrl.tag == 'CheckBox':
        if not label:
            label = ('text', ctrl.attrs.get('ToolTip', '') or ctrl.name() or '', '')
        return ('toggle', label, ctrl.text(src), ctrl)
    if ctrl.tag in ('TextBox', 'ComboBox', 'PasswordBox', 'Slider'):
        return ('field', label or label_from_tag(ctrl), ctrl.text(src), ctrl)
    if ctrl.tag == 'Button':
        return ('button', ctrl.text(src), ctrl)
    return ('raw', ctrl.text(src))


def unwrap(n):
    """A control inside plain wrapper Borders."""
    while n.tag == 'Border' and not n.name() and len(n.elems()) == 1:
        n = n.elems()[0]
    return n


def as_row(n, src):
    if n.tag != 'Grid' or n.name():
        return None
    kids = [unwrap(c) for c in n.elems()]
    labels = [c for c in kids if c.tag in ('TextBlock', 'Label') and not is_heading(c)]
    ctrls = [c for c in kids if is_control(c)]
    if len(labels) == 1 and len(ctrls) == 1 and len(kids) == 2:
        lab, ctrl = labels[0], ctrls[0]
        if ctrl.name() in SKIP:
            return ('skip',)
        if lab.name() or not (lab.attrs.get('Text') or lab.attrs.get('Content')):
            label = ('node', clean(lab.text(src), drop=DROP + ['FontSize', 'VerticalAlignment', 'TextTrimming', 'Grid.Column', 'Foreground', 'FontWeight'],
                                   add='Style="{StaticResource FormLabel}"'))
        else:
            label = ('text', text_of(lab), lab.attrs.get('ToolTip', ''))
        return control_item(ctrl, src, label)
    return None


def merge_vectors(items, src):
    """X/Y/Z text boxes in a row (names ending in x, y, z) plus a cursor button -> one vector."""
    out, i = [], 0
    while i < len(items):
        it = items[i]
        if it[0] == 'field' and it[3].tag == 'TextBox' and i + 2 < len(items):
            a, b, c = items[i], items[i + 1], items[i + 2]
            na, nb, nc = (x[3].name() or '' if x[0] == 'field' else '' for x in (a, b, c))
            if na.lower().endswith('x') and nb.lower().endswith('y') and nc.lower().endswith('z') and na[:-1] == nb[:-1] == nc[:-1]:
                btn = None
                if i + 3 < len(items) and items[i + 3][0] == 'button' and 'getloc' in (items[i + 3][2].name() or '').lower():
                    btn = items[i + 3][1]
                    i += 1
                out.append(('vector', [a[2], b[2], c[2]], btn))
                i += 3
                continue
        out.append(it)
        i += 1
    return out


# ----- emit -----

def label_xaml(label, n):
    s = ' ' * n
    if isinstance(label, tuple) and label[0] == 'node':
        return reindent(label[1], n)
    if isinstance(label, tuple):
        label = label[1]
    if not label:
        return ''
    if label.startswith('{') or ' ' in label or label[:1].isupper():
        return f'{s}<TextBlock Style="{{StaticResource FieldLabel}}" Text="{label}"/>'
    return f'{s}<TextBlock Style="{{StaticResource FieldLabelRaw}}" Text="{label}"/>'


def is_raw_field(it):
    if it[0] != 'field' or it[3].tag != 'TextBox':
        return False
    lab = it[1]
    if isinstance(lab, tuple):
        return False
    lab = (lab or '').strip()
    return bool(lab) and not lab.startswith('{') and ' ' not in lab and not lab[:1].isupper()


def emit(items, n, vec_label=None):
    s = ' ' * n
    # raw values (short lowercase names) two per row
    paired, i = [], 0
    while i < len(items):
        if is_raw_field(items[i]) and i + 1 < len(items) and is_raw_field(items[i + 1]):
            paired.append(('pair', items[i], items[i + 1]))
            i += 2
        else:
            paired.append(items[i])
            i += 1
    items = paired
    out = []
    pending = None  # heading waiting for the next item
    for it in items:
        kind = it[0]
        if kind == 'head':
            if pending:
                out.append(f'{s}<TextBlock Style="{{StaticResource CardSub}}" Text="{pending}"/>')
            pending = it[1]
            continue
        if kind == 'vector':
            lab = pending or ''
            pending = None
            block = [f'{s}<TextBlock Style="{{StaticResource FieldLabel}}" Text="{lab}"/>'] if lab else []
            btn = icon_button(it[2], 0) if it[2] else None
            v = vector('x', 'x', it[1], btn, n).split('\n', 1)[1]  # drop vector()'s own label
            v = v.replace(' CornerRadius="4" Background="{DynamicResource SeactionHeaderBackgroundBrush}"', '')
            out.extend(block + [v])
            continue
        if pending:
            out.append(f'{s}<TextBlock Style="{{StaticResource CardSub}}" Text="{pending}"/>')
            pending = None
        if kind == 'sep':
            if out and 'Rectangle' not in out[-1]:
                out.append(sep(n))
        elif kind == 'toggle':
            label, cb = it[1], it[2]
            if isinstance(label, tuple) and label[0] == 'node':
                out.append('\n'.join([
                    f'{s}<Grid Style="{{StaticResource FormRow}}">',
                    f'{s}    <Grid.ColumnDefinitions>\n{s}        <ColumnDefinition Width="*"/>\n{s}        <ColumnDefinition Width="46"/>\n{s}    </Grid.ColumnDefinitions>',
                    reindent(label[1], n + 4),
                    reindent(clean(cb, add='Grid.Column="1" Style="{StaticResource FormToggle}"'), n + 4),
                    f'{s}</Grid>']))
            else:
                text = label[1] if isinstance(label, tuple) else (label or '')
                tip = label[2] if isinstance(label, tuple) and len(label) > 2 else ''
                tt = f' ToolTip="{tip}"' if tip else ''
                out.append('\n'.join([
                    f'{s}<Grid Style="{{StaticResource FormRow}}">',
                    f'{s}    <Grid.ColumnDefinitions>\n{s}        <ColumnDefinition Width="*"/>\n{s}        <ColumnDefinition Width="46"/>\n{s}    </Grid.ColumnDefinitions>',
                    f'{s}    <TextBlock Style="{{StaticResource FormLabel}}" Text="{text}"{tt}/>',
                    reindent(clean(cb, add='Grid.Column="1" Style="{StaticResource FormToggle}"'), n + 4),
                    f'{s}</Grid>']))
        elif kind == 'field':
            lab = label_xaml(it[1], n)
            ctrl = it[3]
            add = 'Height="30" Margin="0,0,0,10"' if ctrl.tag != 'Slider' else 'Margin="0,0,0,10"'
            out.append((lab + '\n' if lab else '') + reindent(clean(it[2], add=add), n))
        elif kind == 'pair':
            cells = []
            for f in it[1:]:
                cells.append(f'{s}    <TextBlock Style="{{StaticResource FieldLabelRaw}}" Text="{f[1].strip()}"/>\n'
                             + reindent(clean(f[2], add='Height="30" Margin="0,0,0,10"'), n + 4))
            out.append(two(cells[0], cells[1], n))
        elif kind == 'button':
            btn = it[1]
            if 'TitleBarButton' in btn or 'CustomButton' in btn:
                btn = clean(btn, drop=DROP + ['Style', 'local:CornerRadiusSetter.CornerRadius', 'Foreground', 'Padding'],
                            add='Style="{StaticResource FormButton}" Height="32" Padding="12,0" Margin="0,0,0,10"', drop_tag=False)
            out.append(reindent(btn, n))
        elif kind == 'raw':
            out.append(reindent(clean(it[1], drop=['Grid.Row', 'Grid.Column', 'Grid.RowSpan', 'Grid.ColumnSpan'], drop_tag=False), n))
        elif kind == 'group':
            attrs = ' '.join(f'{k}="{v}"' for k, v in it[1].items())
            inner = emit(it[2], n + 4)
            out.append(f'{s}<StackPanel {attrs}>\n{inner}\n{s}</StackPanel>')
    if pending:
        out.append(f'{s}<TextBlock Style="{{StaticResource CardSub}}" Text="{pending}"/>')
    return '\n'.join(o for o in out if o.strip())


def split_title(items):
    """First heading becomes the card title, unless it labels a vector right below it."""
    for i, it in enumerate(items):
        if it[0] == 'head':
            nxt = items[i + 1] if i + 1 < len(items) else None
            if nxt and nxt[0] == 'vector':
                return None, items
            return it[1], items[:i] + items[i + 1:]
        if it[0] != 'sep':
            return None, items
    return None, items


def weight(items):
    w = 0
    for it in items:
        if it[0] == 'group':
            w += weight(it[2])
        elif it[0] == 'vector':
            w += 3
        elif it[0] != 'sep':
            w += 1
    return w


def build_cards(sections, n, default_title):
    """sections: list of item lists, one per card."""
    cards = []
    # An "Advanced" heading starts a folded card of its own (raw values).
    split = []
    for items in sections:
        k = next((i for i, it in enumerate(items) if it[0] == 'head' and 'Translation[advanced]' in it[1]), None)
        if k is None:
            split.append((items, False))
        else:
            split.append((items[:k], False))
            split.append((items[k + 1:], True))
    for items, folded in split:
        if folded:
            body = emit(items, n + 8)
            if body.strip():
                s = ' ' * n
                cards.append((2, f'''{s}<Border Style="{{StaticResource DashCard}}" Margin="0,0,0,12">
{s}    <Expander Style="{{StaticResource CardExpander}}" Header="{{Binding Translation[advanced], FallbackValue=Advanced}}">
{s}        <StackPanel Margin="14,12,14,4">
{body}
{s}        </StackPanel>
{s}    </Expander>
{s}</Border>'''))
            continue
        title, rest = split_title(items)
        if not rest:
            continue
        body = emit(rest, n + 8)
        if not body.strip():
            continue
        s = ' ' * n
        t = title or f'{{Binding Translation[general], FallbackValue={default_title}}}'
        cards.append((weight(rest), f'''{s}<Border Style="{{StaticResource DashCard}}" Margin="0,0,0,12">
{s}    <DockPanel>
{s}        <Border DockPanel.Dock="Top" Style="{{StaticResource DashCardHeader}}">
{s}            <TextBlock Style="{{StaticResource DashCardTitle}}" Text="{t}"/>
{s}        </Border>
{s}        <StackPanel Margin="14,12,14,4">
{body}
{s}        </StackPanel>
{s}    </DockPanel>
{s}</Border>'''))
    return cards


def columns_of(cards, count):
    """Cards in page order, spread over count columns by height."""
    cols = [[] for _ in range(count)]
    heights = [0] * count
    for w, c in cards:
        k = heights.index(min(heights))
        cols[k].append(c)
        heights[k] += w + 3
    return ['\n'.join(c) for c in cols if c]


def find_columns(roots):
    """The old page's column StackPanels (children of its WrapPanel), or None."""
    for r in roots:
        stack = [r]
        while stack:
            n = stack.pop(0)
            if n.tag == 'WrapPanel':
                cols = [c for c in n.elems() if contains_control(c) and not is_control(unwrap(c))]
                if len(cols) >= 2:
                    return n, cols
            stack.extend(n.elems())
    return None, None


def bar_xaml(old, n, combos, add=None, delete=None, count=None):
    """Bar on top: the selectors of the page as ‹ value › fields with their label, then delete / add."""
    s = ' ' * n
    parts = []
    for name in combos:
        el = extract(old, name)
        node = parse(el, 0, len(el))[0]
        lab = label_from_tag(node)
        lab_x = (f'{s}            <TextBlock VerticalAlignment="Center" FontSize="14" Foreground="{{StaticResource NavMutedBrush}}" Margin="0,0,8,0" Text="{lab}"/>\n' if lab else '')
        combo = clean(el, add='Width="84" Height="30" BorderThickness="0"')
        parts.append(f'''{lab_x}{s}            <Border Background="{{DynamicResource ComboBoxBackground}}" BorderBrush="{{DynamicResource ComboBoxBorder}}" BorderThickness="1" CornerRadius="4" Height="32" Margin="0,0,16,0">
{s}                <StackPanel Orientation="Horizontal">
{s}                    <Button Style="{{StaticResource EntryStepButton}}" Content="‹" Tag="{{Binding ElementName={name}}}" CommandParameter="-1" Click="EntryStep_Click" ToolTip="{T('entry_prev', 'Previous')}"/>
{reindent(combo, n + 20)}
{s}                    <Button Style="{{StaticResource EntryStepButton}}" Content="›" Tag="{{Binding ElementName={name}}}" CommandParameter="1" Click="EntryStep_Click" ToolTip="{T('entry_next', 'Next')}"/>
{s}                </StackPanel>
{s}            </Border>''')
    right = []
    if delete:
        d = extract(old, delete)
        click = re.search(r'Click="([^"]+)"', d).group(1)
        right.append(f'''{s}            <Button x:Name="{delete}" Style="{{StaticResource FormButton}}" Width="34" Height="32" Padding="0" Margin="0,0,6,0" Click="{click}" ToolTip="{T('delete', 'Delete')}">
{s}                <Path Data="{ICON_TRASH}" Stroke="{{DynamicResource TextColor}}" StrokeThickness="1.5" StrokeLineJoin="Round" Width="16" Height="16"/>
{s}            </Button>''')
    if add:
        d = extract(old, add)
        click = re.search(r'Click="([^"]+)"', d).group(1)
        right.append(f'''{s}            <Button x:Name="{add}" Style="{{StaticResource FormButtonPrimary}}" Height="32" Padding="10,0,12,0" Click="{click}" ToolTip="{T('add', 'Add')}">
{s}                <StackPanel Orientation="Horizontal">
{s}                    <Path Data="{ICON_PLUS}" Stroke="#FF202225" StrokeThickness="2" Width="16" Height="16" Margin="0,0,6,0" VerticalAlignment="Center"/>
{s}                    <TextBlock VerticalAlignment="Center" Foreground="#FF202225" Text="{T('add', 'Add')}"/>
{s}                </StackPanel>
{s}            </Button>''')
    left = ''
    if count:
        left = f'''{s}        <TextBlock VerticalAlignment="Center" FontSize="14" Foreground="{{StaticResource NavMutedBrush}}">
{s}            <Run FontSize="17" FontWeight="Bold" Foreground="{{DynamicResource TextColor}}" Text="{{Binding Items.Count, ElementName={combos[-1]}, Mode=OneWay}}"/>
{s}            <Run Text="{T(count[0], count[1])}"/>
{s}        </TextBlock>'''
    body = '\n'.join(parts + right)
    return f'''{s}<Border DockPanel.Dock="Top" Padding="2,0" Margin="0,12,0,0">
{s}    <DockPanel>
{s}        <StackPanel DockPanel.Dock="Right" Orientation="Horizontal">
{body}
{s}        </StackPanel>
{left}
{s}    </DockPanel>
{s}</Border>'''


def build_auto(src, tab, default_title='General', entry=None, max_cols=4, sections=None, bar=None):
    """bar: dict(combos=[...], add=name, delete=name, count=(key, fallback)) for the bar on top."""
    SKIP.clear()
    if bar:
        SKIP.update(bar.get('combos', []))
        SKIP.update(x for x in (bar.get('add'), bar.get('delete')) if x)
    a, b, old = tab_range(src, tab)
    base = len(src[:a].split('\n')[-1])
    opening = old[:old.index('>') + 1]
    roots = parse(src, a + len(opening), b - len('</TabItem>'))
    n = base + 20

    wrap_name = ''
    if sections is None:
        wrap, cols = find_columns(roots)
        if wrap is not None and wrap.name():
            wrap_name = f' x:Name="{wrap.name()}"'
        sections = []
        if cols:
            covered = set()
            for c in cols:
                items = []
                walk(c, src, items)
                sections.append(merge_vectors(items, src))
            # anything outside the wrap panel (rare) goes into its own card
            outside = []
            for r in roots:
                outside_walk(r, wrap, src, outside)
            if any(i[0] != 'head' for i in outside):
                sections.insert(0, merge_vectors(outside, src))
        else:
            items = []
            for r in roots:
                walk(r, src, items)
            items = merge_vectors(items, src)
            # headings split the flat list into cards
            cur = []
            for it in items:
                if it[0] == 'head' and any(x[0] != 'head' for x in cur):
                    sections.append(cur)
                    cur = []
                cur.append(it)
            if cur:
                sections.append(cur)

    cards = build_cards(sections, n, default_title)
    columns = columns_of(cards, min(max_cols, max(1, len(cards))))
    bar_x = bar_xaml(old, base + 8, **bar) if bar else ''
    s = ' ' * base
    cols = '\n'.join(f'{s}                <StackPanel Width="340" Margin="0,0,12,0">\n{c}\n{s}                </StackPanel>' for c in columns)
    new = f'''{s}{opening.strip()}

{s}    <!-- Card layout (docs/handoff/auto_page.py): one card per old column, headings as sub titles. -->
{s}    <DockPanel Margin="10,0">
{bar_x}
{s}        <ScrollViewer VerticalScrollBarVisibility="Auto" HorizontalScrollBarVisibility="Disabled">
{s}            <WrapPanel{wrap_name} Orientation="Horizontal" Margin="0,12,0,12">
{cols}
{s}            </WrapPanel>
{s}        </ScrollViewer>
{s}    </DockPanel>

{s}</TabItem>'''
    new = '\n'.join(l for l in new.split('\n') if l.strip() or l == '')
    return src[:a - base] + new + src[b:]


def outside_walk(n, wrap, src, out):
    if n is wrap:
        return
    if n.elems() and any(contains(n, wrap) for _ in [0]):
        for c in n.elems():
            outside_walk(c, wrap, src, out)
        return
    walk(n, src, out)


def contains(n, target):
    return n is target or any(contains(c, target) for c in n.elems())


def live_names(src):
    """x:Name values outside XAML comments."""
    return names(re.sub(r'<!--[\s\S]*?-->', '', src))


def run(tab, allow_missing=(), **kw):
    kw = {k: v for k, v in kw.items() if v is not None}
    # read the file each time, so several pages can be converted one after the other
    pp.S = open(pp.P, encoding='utf-8-sig').read().replace('\r\n', '\n')
    before = live_names(pp.S)
    out = build_auto(pp.S, tab, **kw)
    after = live_names(out)
    missing = sorted(before - after - set(allow_missing))
    print(tab, 'missing names:', missing)
    print(tab, 'new names:', sorted(after - before))
    if '--write' in sys.argv and not missing:
        if pp.CRLF:
            out = out.replace('\n', '\r\n')
        open(pp.P, 'w', encoding='utf-8-sig').write(out)
        print('written')
    return out


def summary(out, tab):
    a = out.index(f'x:Name="{tab}"')
    b = out.index('</TabItem>', a)
    part = out[a:b]
    titles = re.findall(r'DashCardTitle\}" Text="([^"]*)"', part)
    print(' cards:', [re.sub(r'\{Binding Translation\[([^\]]*)\].*', r'\1', t) for t in titles])
    print(' empty labels:', part.count('Text=""'), ' raw kept:', len(re.findall(r'FieldLabelRaw', part)))


if __name__ == '__main__':
    for tab in [a for a in sys.argv[1:] if not a.startswith('--')]:
        summary(run(tab), tab)
