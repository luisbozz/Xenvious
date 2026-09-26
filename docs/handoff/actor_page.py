import sys
sys.path.insert(0, 'docs/handoff')
from props_page import *  # noqa
import props_page as pp


def toggles(items, g, n):
    return '\n'.join(toggle(k, fb, g(cb), n) for k, fb, cb in items)


def build_actor(src):
    a, b, old = tab_range(src, 'PageActor')
    base = len(src[:a].split('\n')[-1])
    n = base + 20
    g = lambda nm: extract(old, nm)
    hidden = reindent(extract_container(old, '<Border Grid.Row="1" Visibility="Collapsed"'), base + 12)
    dup = clean(g('BtnActorDuplicate'), drop=DROP + ['Style', 'local:CornerRadiusSetter.CornerRadius'],
                add='Style="{StaticResource FormButton}" Height="32" Padding="12,0" Margin="0,0,10,0"')
    bar = entry_bar(g('ddactorno'), ('actors_placed', 'actors placed'), dup, g('BtnActorDelete'), g('BtnActorAdd'), base + 8)
    # entry_bar cleans the "move" button with drop_tag=False; the duplicate button is already clean.

    m = n + 8
    model = reindent('<local:ModelCard x:Name="ActorModelCard" Margin="0,0,0,12"/>', n)
    pos = card('card_position', 'Position', '\n'.join([
        vector('loc', 'Location', [g('tbactorlocx'), g('tbactorlocy'), g('tbactorlocz')], g('Btnactorgetloc'), m),
        two(field('heading', 'Heading', g('tbactorhead'), m + 4), field('actordmv', 'Model Variation', g('tbactordmv'), m + 4), m),
        two(field('actorpcash', 'Ped Cash', g('tbactorpcash'), m + 4), field('actorsize', 'Icon Size', g('tbactorblipsize'), m + 4), m),
    ]), n)
    respawn = card('actorrsp', 'Respawn', '\n'.join([
        field('actorrsp', 'Respawn', g('ddActorrsp'), m),
        two(field('actorrr', 'Respawn Range', g('tbactorrr'), m + 4), field('actorpspdl', 'Respawn Time', g('tbactorpspdl'), m + 4), m),
        toggle('respawnrlivc', 'Ignore Visibility Check on Respawn', g('cbactorrespawnrlivc'), m, tip=('respawnrlivc', 'Ignore Visibility Check on Respawn')),
    ]), n)

    combat = card('card_combat', 'Combat', '\n'.join([
        field('actorweap', 'Weapon', g('ddActorweap'), m),
        toggle('actortacticlelight', 'Tacticle Light', g('cbactortacticlelight'), m),
        two(field('actoraccu', 'Accuracy', g('ddActoraccu'), m + 4), field('actorhealth', 'Health', g('ddActorhealth'), m + 4), m),
        two(field('actorcombat', 'Combat Style', g('ddActorcombat'), m + 4), field('actoridle', 'Idle Action', g('ddActoridle'), m + 4), m),
        field('actorcar', 'Spawn in Car', g('ddActorcar'), m),
        sep(m),
        sub('actorrel', 'Relationship', m),
        two(field('actorteam', 'Team', g('ddActorteam'), m + 4), field('actorrel', 'Team Relationship', g('ddActorrel'), m + 4), m),
        sep(m),
        toggle('actorfoll', 'Follow Player', g('cbactorfoll'), m),
        two(field('actorfolr', 'Follow Range', g('tbactorfolr'), m + 4), field('actorteam', 'Team', g('ddActorfollteam'), m + 4), m),
    ]), n)

    behaviour = card('card_behaviour', 'Behaviour', toggles([
        ('actorstationary', 'Stationary until combat', 'cbactorstationary'),
        ('actorfmdc', 'Free movement during combat', 'cbactorfmdc'),
        ('actorfightunarmed', 'Fight even if unarmed', 'cbactorfightunarmed'),
        ('actorwh', 'Weapon holstered', 'cbactorwh'),
        ('actorroav', 'Rockets only at Vehicles', 'cbactorroav'),
        ('actorddb', 'Disable Drive Bys', 'cbactorddb'),
        ('actorcantleaveveh', 'Cant leave Vehicle', 'cbactorcantleaveveh'),
        ('actorcanttarget', 'Cant Target', 'cbactorcanttarget'),
        ('actorfgf', 'Friendly Gunfire', 'cbactorfgf'),
        ('actorie', 'Ignore explosions', 'cbactorie'),
        ('actorantifall', 'Anti Fall', 'cbactorantifall'),
        ('actordiw', 'Die in water', 'cbactordiw'),
        ('actords', 'Disbale Scream', 'cbactords'),
        ('actordiswd', 'Disable Weapon Drop', 'cbactordiswd'),
        ('actorremarmor', 'Remove Armor', 'cbactorremarmor'),
        ('actorspd', 'Spawn Actor Dead', 'cbactorspd'),
    ], g, m), n)
    proofs = card('actorproofs', 'Proof against', toggles([
        ('actorbulletproof', 'Bullets', 'cbactorbulletproof'),
        ('actorfireproof', 'Fires', 'cbactorfireproof'),
        ('actorexplosionproof', 'Explosisions', 'cbactorexplosionproof'),
        ('actorcollisionproof', 'Collisions', 'cbactorcollisionproof'),
        ('actormeeleproof', 'Meele', 'cbactormeeleproof'),
        ('actorsteamproof', 'Steam', 'cbactorsteamproof'),
        ('actordrowningproof', 'Drowning', 'cbactordrowningproof'),
    ], g, m), n)

    goto = card('actorgoto', 'Goto Points', '\n'.join([
        two(field('number', 'Number', g('ddActorgoto'), m + 4), '', m),
        vector('loc', 'Location', [g('tbactoractvx'), g('tbactoractvy'), g('tbactoractvz')], g('Btnactorgetlocgoto'), m),
        two(field('actorgotosize', 'Size', g('tbactoractvsize'), m + 4), field('actvvehspeed', 'Vehicle Speed', g('tbactoractvvehspeed'), m + 4), m),
        toggle('actoractvloop', 'Loop', g('cbactoractvloop'), m),
        toggle('actvhadest', 'Hover at Destination', g('cbactoractvhadest'), m),
        toggle('actvradest', 'Rappel at Destination', g('cbactoractvradest'), m),
        toggle('actvrpadest', 'Allow Passenger Rappel at Destination', g('cbactoractvrpadest'), m),
        sep(m),
        vector('actorgotoawl', 'Wait point (awl)', [g('tbactoractvawlx'), g('tbactoractvawly'), g('tbactoractvawlz')], g('Btnactorgetlocgotoawl'), m),
        raw_grid([('agvr', 'tbactoractvspeed'), ('bits', 'tbactoractvbs'), ('achf', 'tbactoractvachf'), ('awt', 'tbactoractvawt'),
                  ('awr', 'tbactoractvawr'), ('awlr', 'tbactoractvawlr'), ('ags', 'tbactoractvags')], g, m),
    ]), n)

    rules = card('aslrheader', 'Associated Rule', '\n'.join([
        two(field('asrlspawnon', 'Spawn On', g('ddactorspawnon'), m + 4), field('asrlactionon', 'Action Begins', g('ddactoractionon'), m + 4), m),
        two(field('asrlactsspawnteam', 'Spawn/Action by Team', g('ddactorspawnteam'), m + 4), field('asrlactsspawn', 'Spawn/Action at Rule', g('tbactorspawnrule'), m + 4), m),
        toggle('spwnrlivc', 'Ignore Visibility Check on Spawn', g('cbactorspwnrlivc'), m, tip=('spwnrlivc', 'Ignore Visibility Check on Spawn')),
        sep(m),
        two(field('asrlclteam', 'Clear up by Team', g('ddactorteamclear'), m + 4), field('asrlclear', 'Clear up at Rule', g('tbactorclearrule'), m + 4), m),
        toggle('clrlivc', 'Ignore Visibility Check on Clean up', g('cbactorivc'), m, tip=('clrlivc', 'Ignore Visibility Check on Clean up')),
    ]), n)

    # Bitset picker and its value side by side, as before.
    s = ' ' * m
    pbs = '\n'.join([
        f'{s}<TextBlock Style="{{StaticResource FieldLabelRaw}}" Text="pedbs … pbs23"/>',
        f'{s}<Grid Margin="0,0,0,10">',
        f'{s}    <Grid.ColumnDefinitions>',
        f'{s}        <ColumnDefinition Width="110"/>',
        f'{s}        <ColumnDefinition Width="*"/>',
        f'{s}    </Grid.ColumnDefinitions>',
        reindent(clean(g('ddActorpbs'), add='Height="30"'), m + 4),
        reindent(clean(g('tbactorpbs'), add='Grid.Column="1" Height="30"'), m + 4),
        f'{s}</Grid>',
    ])
    raw = '\n'.join([
        pbs,
        two(field('actorteam', 'Team', g('ddactorteamrlprio'), m + 4), field('rule', 'Rule', g('tbactorrule'), m + 4), m),
        two(field('priority', 'Priority', g('tbactorpriority'), m + 4), '', m),
        two(field('jtop', 'Jump To Objective(Pass)', g('tbactorjtop'), m + 4), field('jtof', 'Jump To Objective(Fail)', g('tbactorjtof'), m + 4), m),
        raw_grid([('team', 'tbactorteam'), ('spwn', 'tbactorspwn'), ('objt', 'tbactorobjt'), ('acts', 'tbactoracts'),
                  ('scrrq', 'tbactorscrrq'), ('awlsrl', 'tbactorawysrl'), ('pedcr', 'tbactorpedcr'), ('pedct', 'tbactorpedct')], g, m),
    ])
    advanced = expander_card('advanced', 'Advanced', raw, n)

    new = page_shell('PageActor', 'Actor', bar, hidden, [
        '\n'.join([model, pos, respawn]),
        combat,
        '\n'.join([behaviour, proofs]),
        '\n'.join([goto, rules, advanced]),
    ], base)
    new = new.replace('<DockPanel>', '<DockPanel Margin="10,0">', 1)
    return src[:a - base] + new + src[b:]


before = names(pp.S)
out = build_actor(pp.S)
after = names(out)
print('missing names:', sorted(before - after))
print('new names:', sorted(after - before))
if '--write' in sys.argv:
    if pp.CRLF:
        out = out.replace('\n', '\r\n')
    open(pp.P, 'w', encoding='utf-8-sig').write(out)
