"""Set Unity sprite geometry from each shark's alpha island; pixels stay unchanged."""
from collections import deque
from pathlib import Path
import re
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
ASSET = ROOT / 'Assets/_Game/Resources/Art/Generated/shop-shark-skins.png'
alpha = Image.open(ASSET).getchannel('A')
meta_path = Path(str(ASSET) + '.meta')
meta = meta_path.read_text()

def sprite_outline(match):
    block = match.group()
    name = re.search(r'name: (\w+)', block)[1]
    values = [int(re.search(r'        ' + key + r': (\d+)', block)[1])
              for key in ('x', 'y', 'width', 'height')]
    x, y, width, height = values
    pixels = alpha.load()
    points = {(u, v) for v in range(height) for u in range(width)
              if pixels[x + u, alpha.height - y - height + v] > 0}
    islands = []
    while points:
        seed = points.pop()
        island = {seed}
        pending = deque([seed])
        while pending:
            u, v = pending.popleft()
            for neighbour in ((u-1,v), (u+1,v), (u,v-1), (u,v+1)):
                if neighbour in points:
                    points.remove(neighbour)
                    island.add(neighbour)
                    pending.append(neighbour)
        islands.append(island)
    shark = max(islands, key=len)
    rows = {}
    for u, v in shark:
        low, high = rows.get(v, (u, u))
        rows[v] = (min(low,u), max(high,u))
    for island in islands:
        if island is shark or len(island) < 100:
            continue
        assert not any(v in rows and rows[v][0]-1 <= u <= rows[v][1]+1
                       for u,v in island), name + ': neighbour intersects outline'
    left, right = [], []
    for v in sorted(rows):
        low, high = rows[v]
        left.extend([(max(0,low-1), v), (max(0,low-1), v+1)])
        right.extend([(min(width,high+2), v), (min(width,high+2), v+1)])
    polygon = left + right[::-1]
    # Remove collinear steps without moving the silhouette boundary.
    simplified = []
    for p in polygon:
        while len(simplified) >= 2:
            a, b = simplified[-2:]
            if (b[0]-a[0])*(p[1]-b[1]) != (b[1]-a[1])*(p[0]-b[0]):
                break
            simplified.pop()
        simplified.append(p)
    lines = ['      outline:']
    for i, (u,v) in enumerate(simplified):
        prefix = '      - - ' if i == 0 else '        - '
        lines.append(prefix + '{x: ' + str(u-width/2) + ', y: ' + str(height/2-v) + '}')
    print(name, 'shark pixels:', len(shark), 'excluded islands:', len(islands)-1)
    return block.replace('      outline: []', '\n'.join(lines))

meta = re.sub(r'    - serializedVersion: 2\n      name: .*?(?=    - serializedVersion: 2\n      name: |^    outline:)',
              sprite_outline, meta, flags=re.S | re.M)
meta_path.write_text(meta.replace('  spriteMeshType: 1','  spriteMeshType: 0'))
for path in (ROOT / 'Assets/_Game/Prefab/UI/MenuDesign').rglob('*.prefab'):
    original = path.read_text()
    def enable_mesh(match):
        block = match.group()
        if 'c1c750e109be1024fbac1435a2a27326' in block:
            return block.replace('m_UseSpriteMesh: 0','m_UseSpriteMesh: 1')
        return block
    updated = re.sub(r'--- !u!114 &\d+\n.*?(?=--- !u!|\Z)', enable_mesh, original, flags=re.S)
    if updated != original:
        path.write_text(updated)
        print('Enabled sprite geometry:', path.name)

