"""Mechanical prefab migration: preserve existing IDs and typed component links."""
from pathlib import Path
import re
import shutil

ROOT = Path(__file__).resolve().parents[2]
BACKUP = ROOT / 'output' / 'win-transition-backup'
HELPER_GUID = '3d86f42482744f43a72c43801f7fbe51'
SHADER_GUID = '05871a9562664dba846d08fb1e80bcc3'
IDS = [8801000000000000000 + n for n in range(1, 7)]
VISUAL_GO, VISUAL_TF, HELPER, SHADOW_GO, SHADOW_TF, SHADOW_RENDERER = IDS


def backup(path):
    destination = BACKUP / path.relative_to(ROOT)
    destination.parent.mkdir(parents=True, exist_ok=True)
    if not destination.exists():
        shutil.copy2(path, destination)


def blocks(source):
    return re.split(r'(?=^--- !u!)', source, flags=re.M)


def file_id(block):
    return int(re.search(r'^--- !u!\d+ &(\d+)', block).group(1))


def transform(go, parent, children):
    entries = ''.join(f'  - {{fileID: {child}}}\n' for child in children)
    child_field = f'  m_Children:\n{entries}' if children else '  m_Children: []\n'
    return f'''--- !u!4 &{go}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {VISUAL_GO if go == VISUAL_TF else SHADOW_GO}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
{child_field}  m_Father: {{fileID: {parent}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
'''


def game_object(identifier, name, components):
    entries = ''.join(f'  - component: {{fileID: {component}}}\n' for component in components)
    return f'''--- !u!1 &{identifier}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
{entries}  m_Layer: 0
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
'''


def migrate_prefab(path):
    source = path.read_text(encoding='utf-8-sig')
    assert HELPER_GUID not in source, f'Already migrated: {path}'
    parts = blocks(source)
    tile = next(part for part in parts if 'Assembly-CSharp::MapTile' in part)
    root_go = int(re.search(r'm_GameObject: \{fileID: (\d+)\}', tile).group(1))
    face_id = int(re.search(r'spriteRenderer: \{fileID: (\d+)\}', tile).group(1))
    label_id = int(re.search(r'label: \{fileID: (\d+)\}', tile).group(1))
    root = next(part for part in parts if part.startswith('--- !u!4 ') and f'm_GameObject: {{fileID: {root_go}}}' in part)
    root_tf = file_id(root)
    children = [int(value) for value in re.findall(r'^  - \{fileID: (\d+)\}', root, re.M)]
    assert len(children) == 2
    face = next(part for part in parts if part.startswith(f'--- !u!212 &{face_id}\n'))
    depth = next(part for part in parts if part.startswith('--- !u!23 ') and 'm_SortingOrder: -2' in part)
    depth_id = file_id(depth)
    assert all(f'&{identifier}\n' not in source for identifier in IDS)
    migrated = []
    for part in parts:
        if part.startswith(f'--- !u!1 &{root_go}\n'):
            part = part.replace(f'  - component: {{fileID: {face_id}}}\n', '')
            part = part.replace('  m_Layer:', f'  - component: {{fileID: {HELPER}}}\n  m_Layer:', 1)
        elif part == root:
            part = re.sub(r'  m_Children:\n(?:  - \{fileID: \d+\}\n)+',
                          f'  m_Children:\n  - {{fileID: {VISUAL_TF}}}\n  - {{fileID: {SHADOW_TF}}}\n', part)
        elif part == face:
            part = part.replace(f'm_GameObject: {{fileID: {root_go}}}', f'm_GameObject: {{fileID: {VISUAL_GO}}}')
        elif part == tile:
            part += f'  winVisual: {{fileID: {HELPER}}}\n'
        elif part.startswith('--- !u!4 ') and file_id(part) in children:
            part = part.replace(f'm_Father: {{fileID: {root_tf}}}', f'm_Father: {{fileID: {VISUAL_TF}}}')
        migrated.append(part)
    shadow = face.replace(f'&{face_id}\n', f'&{SHADOW_RENDERER}\n', 1)
    shadow = shadow.replace(f'm_GameObject: {{fileID: {root_go}}}', f'm_GameObject: {{fileID: {SHADOW_GO}}}')
    shadow = re.sub(r'  m_Color: \{[^\n]+\}', '  m_Color: {r: 0, g: 0.08, b: 0.12, a: 0}', shadow)
    shadow = shadow.replace('m_SortingOrder: -1', 'm_SortingOrder: -3')
    helper = f'''--- !u!114 &{HELPER}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {root_go}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {HELPER_GUID}, type: 3}}
  m_Name:
  m_EditorClassIdentifier: Assembly-CSharp::TileWinVisual
  visualRoot: {{fileID: {VISUAL_TF}}}
  face: {{fileID: {face_id}}}
  depth: {{fileID: {depth_id}}}
  label: {{fileID: {label_id}}}
  shadowTransform: {{fileID: {SHADOW_TF}}}
  shadow: {{fileID: {SHADOW_RENDERER}}}
  liftDistance: 0.4
  orthographicScaleBoost: 0.12
'''
    result = ''.join(migrated) + game_object(VISUAL_GO, 'WinVisualRoot', [VISUAL_TF, face_id])
    result += transform(VISUAL_TF, root_tf, children) + helper
    result += game_object(SHADOW_GO, 'WinShadow', [SHADOW_TF, SHADOW_RENDERER])
    result += transform(SHADOW_TF, root_tf, []) + shadow
    original_ids = set(re.findall(r'^--- !u!\d+ &(\d+)', source, re.M))
    all_ids = re.findall(r'^--- !u!\d+ &(\d+)', result, re.M)
    assert len(all_ids) == len(set(all_ids))
    assert original_ids.issubset(set(all_ids))
    backup(path)
    path.write_text(result, encoding='utf-8', newline='\n')
    print(f'Migrated {path.name}: original IDs preserved')


for name in ['EmptyTile', 'BlockTile', 'OneWayStraight', 'OneWayCorner', 'RotateTile']:
    migrate_prefab(ROOT / f'Assets/_Game/Prefab/Map/Tiles/{name}.prefab')

shader_path = ROOT / 'Assets/_Game/Shaders/TileWinFade.shader'
shader = (ROOT / 'Assets/_Game/Shaders/Pseudo3D.shader').read_text(encoding='utf-8')
shader = shader.replace('A Mothers Ocean/Pseudo 3D', 'A Mothers Ocean/Tile Win Fade')
shader = shader.replace('        _BaseColor(', '        _Opacity("Opacity", Range(0, 1)) = 1\n        _BaseColor(', 1)
shader = shader.replace('"Opaque"', '"Transparent"').replace('"Geometry"', '"Transparent"')
shader = shader.replace('ZWrite On', 'ZWrite Off\n            Blend SrcAlpha OneMinusSrcAlpha')
shader = shader.replace('half4 _BaseColor;', 'half4 _BaseColor;\n                half _Opacity;')
shader = shader.replace('_BaseColor.a);', '_BaseColor.a * _Opacity);')
assert not shader_path.exists()
shader_path.write_text(shader, encoding='utf-8', newline='\n')
shader_path.with_suffix('.shader.meta').write_text(f'fileFormatVersion: 2\nguid: {SHADER_GUID}\n', encoding='utf-8')
for name in ['TileEmpty', 'TileBlock', 'TilePath', 'TileRotate']:
    path = ROOT / f'Assets/_Game/Materials/Pseudo3D/{name}.mat'
    source = path.read_text(encoding='utf-8-sig')
    assert '5a4f59d7d7314e68a02cc68f3b9b12a1' in source
    backup(path)
    source = source.replace('5a4f59d7d7314e68a02cc68f3b9b12a1', SHADER_GUID)
    source = source.replace('    m_Floats: []', '    m_Floats:\n    - _Opacity: 1')
    path.write_text(source, encoding='utf-8', newline='\n')
print('Tile-only fade shader wired; player materials unchanged')
