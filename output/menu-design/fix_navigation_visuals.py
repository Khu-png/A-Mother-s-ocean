"""Patch navigation presentation only, preserving Unity IDs and event bindings."""
import hashlib
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / 'Assets/_Game/Art/MenuDesign'
CHROME = ART / 'NavigationChrome.png'
CHROME_GUID = hashlib.md5(b'A-Mothers-Ocean-NavigationChrome-v1').hexdigest()
IMAGE_GUID = 'fe87c0e1cc204ed48ad3b37840f39efc'
TMP_GUID = 'f4688fdb7df04437aeb418b961361dc5'
TAB_GUID = 'eb4e568451ea5974ca19b02df8a68093'


def field(text, key):
    match = re.search(r'^  ' + re.escape(key) + r':[^\n]*(?:\n(?:    [^\n]*|\n))*', text, re.M)
    if not match:
        raise ValueError(f'Missing field {key}')
    return match.group(0).strip()[len(key) + 1:].strip()


def reference(text, key):
    return int(re.search(r'fileID["\s]*:\s*(-?\d+)', field(text, key))[1])


def set_field(block, key, value):
    pattern = r'^  ' + re.escape(key) + r':[^\n]*(?:\n(?:    [^\n]*|\n))*'
    replacement = '  ' + key + ': ' + json.dumps(value)
    block['text'], count = re.subn(pattern, lambda _: replacement, block['text'], count=1, flags=re.M)
    if count != 1:
        raise ValueError(f'Expected one {key} in {block["id"]}')


def vector(x, y):
    return dict(x=x, y=y)


class UnityFile:
    def __init__(self, path):
        self.path = path
        self.original = path.read_text(encoding='utf-8')
        matches = list(re.finditer(r'^--- !u!(\d+) &(-?\d+)(?: stripped)?\n', self.original, re.M))
        self.prefix = self.original[:matches[0].start()]
        self.blocks = []
        for index, match in enumerate(matches):
            end = matches[index + 1].start() if index + 1 < len(matches) else len(self.original)
            self.blocks.append(dict(tag=int(match[1]), id=int(match[2]),
                                    text=self.original[match.start():end].rstrip()))

    def named(self, name):
        nodes = [b for b in self.blocks if b['tag'] == 1 and '  m_Name:' in b['text'] and
                 field(b['text'], 'm_Name').strip('"\'') == name]
        if len(nodes) != 1:
            raise ValueError(f'{self.path}: expected unique {name}')
        return nodes[0]

    def components(self, node, tag=None, script=None):
        candidates = []
        for block in self.blocks:
            if block['tag'] in (1, 1001, 1660057539):
                continue
            try:
                owner = reference(block['text'], 'm_GameObject')
            except ValueError:
                continue
            if owner == node['id'] and (tag is None or block['tag'] == tag):
                if script is None or script in field(block['text'], 'm_Script'):
                    candidates.append(block)
        return candidates

    def rect(self, node):
        return self.components(node, tag=224)[0]

    def child(self, parent, name):
        parent_id = self.rect(parent)['id']
        for node in self.blocks:
            if node['tag'] == 1 and '  m_Name:' in node['text'] and field(node['text'], 'm_Name').strip('"\'') == name:
                rect = self.rect(node)
                if reference(rect['text'], 'm_Father') == parent_id:
                    return node
        raise ValueError(f'Missing child {name}')

    def bounds(self, node, low, high):
        rect = self.rect(node)
        set_field(rect, 'm_AnchorMin', vector(*low))
        set_field(rect, 'm_AnchorMax', vector(*high))
        set_field(rect, 'm_AnchoredPosition', vector(0, 0))
        set_field(rect, 'm_SizeDelta', vector(0, 0))

    def apply(self):
        layout = self.named('NavigationLayout')
        fitter = self.components(layout, tag=114,
                                 script='86710e43de46f6f4bac7c8e50813a599')[0]
        set_field(fitter, 'm_AspectRatio', 4.0)
        set_field(self.rect(layout), 'm_SizeDelta', vector(1000, 250))
        bottom = self.rect(self.named('BottomNavigation'))
        set_field(bottom, 'm_SizeDelta', vector(0, 250))
        set_field(bottom, 'm_AnchoredPosition', vector(0, 125))
        band = self.named('NavigationBand')
        self.bounds(band, (0, 0), (1, .82))
        image = self.components(band, tag=114, script=IMAGE_GUID)[0]
        set_field(image, 'm_Sprite', dict(fileID=21300001, guid=CHROME_GUID, type=3))
        set_field(image, 'm_Color', dict(r=1, g=1, b=1, a=1))
        set_field(image, 'm_Type', 0)
        set_field(image, 'm_PreserveAspect', 0)

        for index, name in enumerate(['Gift', 'Shop', 'Home', 'Skin', 'Tank']):
            tab = self.named(name + 'Tab')
            self.bounds(tab, (index / 5, 0), ((index + 1) / 5, 1))
            highlight = self.child(tab, 'SelectedHighlight')
            self.bounds(highlight, (-.025, 0), (1.025, 1))
            image = self.components(highlight, tag=114, script=IMAGE_GUID)[0]
            set_field(image, 'm_Sprite', dict(fileID=21300002, guid=CHROME_GUID, type=3))
            set_field(image, 'm_Color', dict(r=1, g=1, b=1, a=1))
            set_field(image, 'm_Type', 0)
            set_field(image, 'm_PreserveAspect', 0)

            icon = self.child(tab, 'Icon')
            width = .94 if name == 'Home' else .88
            self.bounds(icon, ((1 - width) / 2, .32), ((1 + width) / 2, .98))
            label = self.child(tab, 'Label')
            self.bounds(label, (.025, .065), (.975, .30))
            text = self.components(label, tag=114, script=TMP_GUID)[0]
            for key, value in dict(m_fontSize=44, m_fontSizeBase=44, m_fontSizeMin=38,
                                   m_fontSizeMax=44, m_fontStyle=0, m_fontWeight=700,
                                   m_enableAutoSizing=1, m_characterSpacing=0).items():
                set_field(text, key, value)
            for controller in self.components(tab, tag=114, script=TAB_GUID):
                set_field(controller, 'selectedScale', 1.10)
                set_field(controller, 'selectedOffset', 6)

    def save(self):
        contents = self.prefix + '\n'.join(b['text'] for b in self.blocks) + '\n'
        backup = ROOT / 'output/menu-design/navigation-backup' / self.path.relative_to(ROOT)
        backup.parent.mkdir(parents=True, exist_ok=True)
        if not backup.exists():
            backup.write_text(self.original, encoding='utf-8')
        if contents != self.original:
            self.path.write_text(contents, encoding='utf-8')


def write_importer():
    template = (ART / 'MenuObjects.png.meta').read_text()
    template = re.sub(r'^guid: \w+', 'guid: ' + CHROME_GUID, template, flags=re.M)
    before, rest = template.split('  internalIDToNameTable:', 1)
    rest = rest[rest.index('  externalObjects:'):]
    rest = rest[:rest.index('  spriteSheet:')]
    sprites = [('NavigationBar', 50, 405, 2075, 285),
               ('SelectedTab', 55, 30, 425, 355)]
    ids = ''.join(f'  - first:\n      213: {21300001+i}\n    second: {s[0]}\n'
                  for i, s in enumerate(sprites))
    body = '  spriteSheet:\n    serializedVersion: 2\n    sprites:\n'
    for i, (name, x, y, width, height) in enumerate(sprites):
        body += (f'    - serializedVersion: 2\n      name: {name}\n      rect:\n'
                 f'        serializedVersion: 2\n        x: {x}\n        y: {y}\n'
                 f'        width: {width}\n        height: {height}\n'
                 '      alignment: 0\n      pivot: {x: 0.5, y: 0.5}\n'
                 '      border: {x: 0, y: 0, z: 0, w: 0}\n      customData: \n'
                 '      outline: []\n      physicsShape: []\n      tessellationDetail: -1\n'
                 f'      bones: []\n      spriteID: {hashlib.md5(name.encode()).hexdigest()}\n'
                 f'      internalID: {21300001+i}\n      vertices: []\n'
                 '      indices: \n      edges: []\n      weights: []\n')
    body += ('    outline: []\n    customData: \n    physicsShape: []\n    bones: []\n'
             '    spriteID: \n    internalID: 0\n    vertices: []\n    indices: \n'
             '    edges: []\n    weights: []\n    secondaryTextures: []\n'
             '    nameFileIdTable:\n      NavigationBar: 21300001\n      SelectedTab: 21300002\n'
             '  mipmapLimitGroupName: \n  pSDRemoveMatte: 0\n  userData: \n'
             '  assetBundleName: \n  assetBundleVariant: \n')
    Path(str(CHROME) + '.meta').write_text(before + '  internalIDToNameTable:\n' + ids + rest + body)


if __name__ == '__main__':
    write_importer()
    files = [ROOT / 'Assets/_Game/Prefab/UI/MainmenuRuntime.prefab',
             ROOT / 'Assets/_Game/Prefab/UI/MenuDesign/MenuShellDesign.prefab']
    files += sorted((ROOT / 'Assets/_Game/Scenes/MenuDesign').glob('*.unity'))
    for path in files:
        doc = UnityFile(path)
        doc.apply()
        doc.save()
        print(path.relative_to(ROOT))
