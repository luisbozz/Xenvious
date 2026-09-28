import sys
sys.path.insert(0, 'docs/handoff')
from auto_page import *  # noqa


def build_ipl(src):
    a, b, old = tab_range(src, 'PageInnerMissionIPL')
    base = len(src[:a].split('\\n')[-1]) if False else len(src[:a].split('\n')[-1])
    opening = old[:old.index('>') + 1]
    n = base + 20
    g = lambda nm: extract(old, nm)
    raw = ['tbMissioniplop', 'tbMissioniplop2', 'tbMissionintop', 'tbMissionintop2', 'tbMissionintop3']
    tags = ['iplop', 'iplop2', 'intop', 'intop2', 'intop3']
    pairs = raw_grid(list(zip(tags, raw)), g, n + 8)
    s = ' ' * n
    values = f'''{s}<Border Style="{{StaticResource DashCard}}">
{s}    <Expander Style="{{StaticResource CardExpander}}" Header="{{Binding Translation[advanced], FallbackValue=Advanced}}">
{s}        <StackPanel Margin="14,12,14,4">
{pairs}
{s}        </StackPanel>
{s}    </Expander>
{s}</Border>'''
    ipls = re.findall(r'<local:IPL [^>]*/>', old)
    cards = '\n'.join(' ' * (base + 16) + clean(x, drop=['Margin']) for x in ipls)
    t = ' ' * base
    new = f'''{t}{opening.strip()}

{t}    <!-- Interiors as cards (switch loads the IPL, teleport goes there); the raw IPL values folded on the left. -->
{t}    <DockPanel Margin="10,12,10,0">
{t}        <StackPanel DockPanel.Dock="Left" Width="340" Margin="0,0,12,0">
{t}            <TextBlock FontSize="13" Foreground="{{StaticResource NavMutedBrush}}" TextWrapping="Wrap" Margin="2,0,0,12" Text="{T('ipl_intro', 'Interiors and map parts the job loads. The switch turns one on or off, Teleport takes you there.')}"/>
{values}
{t}        </StackPanel>
{t}        <ScrollViewer VerticalScrollBarVisibility="Auto" HorizontalScrollBarVisibility="Disabled">
{t}            <WrapPanel Orientation="Horizontal">
{cards}
{t}            </WrapPanel>
{t}        </ScrollViewer>
{t}    </DockPanel>

{t}</TabItem>'''
    return src[:a - base] + new + src[b:]


if __name__ == '__main__':
    pp.S = open(pp.P, encoding='utf-8-sig').read().replace('\r\n', '\n')
    before = live_names(pp.S)
    out = build_ipl(pp.S)
    after = live_names(out)
    print('missing', sorted(before - after), 'new', sorted(after - before))
    if '--write' in sys.argv and not (before - after):
        open(pp.P, 'w', encoding='utf-8-sig').write(out.replace('\n', '\r\n') if pp.CRLF else out)
        print('written')
