"""Offline asset authoring only. No game scripts or existing scenes are changed."""
import copy
import hashlib
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / 'Assets/_Game/Art/MenuDesign'
PREFABS = ROOT / 'Assets/_Game/Prefab/UI/MenuDesign'
SCENES = ROOT / 'Assets/_Game/Scenes/MenuDesign'
IMAGE = 'fe87c0e1cc204ed48ad3b37840f39efc'
BUTTON = '4e29b1a8efbd4b44bb3f3716e73f07ff'
TMP = 'f4688fdb7df04437aeb418b961361dc5'
FITTER = '86710e43de46f6f4bac7c8e50813a599'
OUTLINE = 'e19747de3f5aca642ab2be37e372fb86'
W, H = 960, 1370
FONT = 'bad6fe02600c36f4a8b854656d73e38a'


def guid(path):
    return hashlib.md5(str(path.relative_to(ROOT)).replace('\\', '/').encode()).hexdigest()


def ref(file_id=0, asset=None, kind=3):
    result = {'fileID': file_id}
    if asset:
        result.update(guid=asset, type=kind)
    return result


def vec(x=0, y=0, z=None, w=None):
    result = {'x': x, 'y': y}
    if z is not None:
        result['z'] = z
    if w is not None:
        result['w'] = w
    return result


def color(r=1, g=1, b=1, a=1):
    return dict(r=r, g=g, b=b, a=a)


def yaml_lines(value, level=0):
    pad = '  ' * level
    if isinstance(value, dict):
        lines = []
        for key, item in value.items():
            if isinstance(item, (dict, list)) and item:
                lines.append(f'{pad}{key}:')
                lines += yaml_lines(item, level + 1)
            else:
                lines.append(f'{pad}{key}: {json.dumps(item, ensure_ascii=True)}')
        return lines
    lines = []
    for item in value:
        nested = yaml_lines(item, level + 1)
        lines.append(pad + '- ' + nested[0].lstrip())
        lines += nested[1:]
    return lines


def common(go=None):
    data = dict(m_ObjectHideFlags=0, m_CorrespondingSourceObject=ref(),
                m_PrefabInstance=ref(), m_PrefabAsset=ref())
    if go:
        data['m_GameObject'] = ref(go)
    return data


def asset_meta(path):
    meta = Path(str(path) + '.meta')
    if not meta.exists():
        importer = 'PrefabImporter' if path.suffix == '.prefab' else 'DefaultImporter'
        meta.write_text(f'fileFormatVersion: 2\nguid: {guid(path)}\n{importer}:\n'
                        '  externalObjects: {}\n  userData: \n  assetBundleName: \n'
                        '  assetBundleVariant: \n', encoding='utf-8')


def directory(path):
    if path.exists():
        return
    directory(path.parent)
    path.mkdir()
    Path(str(path) + '.meta').write_text(
        f'fileFormatVersion: 2\nguid: {guid(path)}\nfolderAsset: yes\nDefaultImporter:\n'
        '  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')


def sprite(path):
    text = Path(str(ROOT / path) + '.meta').read_text()
    asset = re.search(r'^guid: (\w+)', text, re.M)[1]
    found = re.search(r'internalID:\s*(\d{6,})', text)
    return ref(int(found[1]) if found else 21300000, asset)


def atlas(name):
    return ref(21300001 + ATLAS_NAMES.index(name), guid(ART / 'MenuObjects.png'))


ATLAS_NAMES = ['RewardChest', 'LockedChest', 'PearlButton', 'Undo', 'Hint',
               'Shuffle', 'GiftIcon', 'ShopIcon', 'HomeIcon', 'SkinIcon',
               'TankIcon', 'GlassTile']
BG = sprite('Assets/_Game/Resources/Art/MenuUI/Clean/ocean_menu_background.png')
MOTHER = sprite('Assets/_Game/Resources/Art/Generated/mother_shark_sprite.png')
BABY = ref(21300000, guid(ART/'BabySharkHappy.png'))


class Document:
    def __init__(self, name):
        self.objects = []
        self.name = name
        self.next_id = 1000
        self.root = self.node(name, size=(W, H))

    def allocate(self):
        self.next_id += 1
        key = f'{self.name}:{self.next_id}'.encode()
        return int.from_bytes(hashlib.sha256(key).digest()[:8], 'little') & ((1 << 62) - 1)

    def add(self, tag, kind, data):
        file_id = self.allocate()
        self.objects.append((tag, file_id, kind, data))
        return file_id

    def node(self, name, parent=None, pos=(0, 0), size=(100, 100), anchors=None,
             active=True, rotation=0):
        design_size=size
        if parent and anchors is None:
            parent_width,parent_height=parent['design_size']
            if parent_width>0 and parent_height>0:
                anchors=((.5+(pos[0]-size[0]/2)/parent_width,
                          .5+(pos[1]-size[1]/2)/parent_height),
                         (.5+(pos[0]+size[0]/2)/parent_width,
                          .5+(pos[1]+size[1]/2)/parent_height))
                pos=(0,0)
                size=(0,0)
        go = common()
        go.update(serializedVersion=6, m_Component=[], m_Layer=5, m_Name=name,
                  m_TagString='Untagged', m_Icon=ref(), m_NavMeshLayer=0,
                  m_StaticEditorFlags=0, m_IsActive=int(active))
        go_id = self.add(1, 'GameObject', go)
        rt = common(go_id)
        import math
        rt.update(m_LocalRotation=vec(0, 0, math.sin(math.radians(rotation / 2)),
                                     math.cos(math.radians(rotation / 2))),
                  m_LocalPosition=vec(0, 0, 0), m_LocalScale=vec(1, 1, 1),
                  m_ConstrainProportionsScale=0, m_Children=[],
                  m_Father=ref(parent['rt_id'] if parent else 0),
                  m_LocalEulerAnglesHint=vec(0, 0, rotation),
                  m_AnchorMin=vec(*(anchors[0] if anchors else (.5, .5))),
                  m_AnchorMax=vec(*(anchors[1] if anchors else (.5, .5))),
                  m_AnchoredPosition=vec(*pos), m_SizeDelta=vec(*size),
                  m_Pivot=vec(.5, .5))
        rt_id = self.add(224, 'RectTransform', rt)
        go['m_Component'].append({'component': ref(rt_id)})
        if parent:
            parent['rt']['m_Children'].append(ref(rt_id))
        return dict(go=go, go_id=go_id, rt=rt, rt_id=rt_id,design_size=design_size)

    def mono(self, node, script, fields, class_name=''):
        data = common(node['go_id'])
        data.update(m_Enabled=1, m_EditorHideFlags=0,
                    m_Script=ref(11500000, script), m_Name='',
                    m_EditorClassIdentifier=class_name)
        data.update(fields)
        file_id = self.add(114, 'MonoBehaviour', data)
        node['go']['m_Component'].append({'component': ref(file_id)})
        return file_id

    def graphic(self, node, fields, script=IMAGE):
        renderer = common(node['go_id'])
        renderer['m_CullTransparentMesh'] = 1
        renderer_id = self.add(222, 'CanvasRenderer', renderer)
        node['go']['m_Component'].append({'component': ref(renderer_id)})
        base = dict(m_Material=ref(), m_Color=color(), m_RaycastTarget=0,
                    m_RaycastPadding=vec(0, 0, 0, 0), m_Maskable=1,
                    m_OnCullStateChanged={'m_PersistentCalls': {'m_Calls': []}})
        base.update(fields)
        return self.mono(node, script, base)

    def image(self, name, image, pos, size, parent=None, tint=None, active=True,
              preserve=True, rotation=0):
        node = self.node(name, parent or self.root, pos, size, active=active,
                         rotation=rotation)
        node['graphic'] = self.graphic(node, dict(m_Sprite=image or ref(), m_Type=0,
            m_PreserveAspect=int(preserve), m_FillCenter=1, m_FillMethod=4,
            m_FillAmount=1, m_FillClockwise=1, m_FillOrigin=0,
            m_UseSpriteMesh=0, m_PixelsPerUnitMultiplier=1,
            m_Color=tint or color()))
        return node

    def text(self, name, text, pos, size, font=44, parent=None, tint=None):
        node = self.node(name, parent or self.root, pos, size)
        fields = dict(m_text=text, m_isRightToLeft=0,
            m_fontAsset=ref(11400000, FONT, 2),
            m_sharedMaterial=ref(2100000, guid(ART/'MenuText.mat'), 2),
            m_fontSharedMaterials=[], m_fontMaterial=ref(), m_fontMaterials=[],
            m_fontColor32={'serializedVersion': 2, 'rgba': 4294967295},
            m_fontColor=tint or color(), m_enableVertexGradient=0,
            m_colorMode=3, m_fontColorGradient={k: color() for k in
                ['topLeft', 'topRight', 'bottomLeft', 'bottomRight']},
            m_fontColorGradientPreset=ref(), m_spriteAsset=ref(),
            m_tintAllSprites=0, m_StyleSheet=ref(), m_TextStyleHashCode=-1183493901,
            m_overrideHtmlColors=0, m_faceColor={'serializedVersion': 2,'rgba':4294967295},
            m_fontSize=font, m_fontSizeBase=font, m_fontWeight=700,
            m_enableAutoSizing=1, m_fontSizeMin=font * .8, m_fontSizeMax=font,
            m_fontStyle=0, m_HorizontalAlignment=2, m_VerticalAlignment=512,
            m_textAlignment=65535, m_characterSpacing=0, m_characterHorizontalScale=1,
            m_wordSpacing=0, m_lineSpacing=0, m_lineSpacingMax=0,
            m_paragraphSpacing=0, m_charWidthMaxAdj=0, m_TextWrappingMode=0,
            m_wordWrappingRatios=.4, m_overflowMode=0, m_linkedTextComponent=ref(),
            parentLinkedComponent=ref(), m_enableKerning=1, m_ActiveFontFeatures='',
            m_enableExtraPadding=1, checkPaddingRequired=0, m_isRichText=1,
            m_EmojiFallbackSupport=1, m_parseCtrlCharacters=1, m_isOrthographic=1,
            m_isCullingEnabled=0, m_horizontalMapping=0, m_verticalMapping=0,
            m_uvLineOffset=0, m_geometrySortingOrder=0, m_IsTextObjectScaleStatic=0,
            m_VertexBufferAutoSizeReduction=0, m_useMaxVisibleDescender=1,
            m_pageToDisplay=1, m_margin=vec(6, 2, 6, 2),
            m_isUsingLegacyAnimationComponent=0, m_isVolumetricText=0,
            m_hasFontAssetChanged=0, m_baseMaterial=ref(), m_maskOffset=vec(0,0,0,0))
        self.graphic(node, fields, TMP)
        return node

    def button(self, name, label, pos, size, parent=None):
        node = self.image(name, atlas('PearlButton'), pos, size, parent)
        self.button_component(node)
        if label:
            self.text('Label', label, (0,0), (size[0]*.77,size[1]*.60),
                      min(48,size[1]*.40), node, color())
        return node

    def button_component(self, node):
        for tag, file_id, kind, fields in self.objects:
            if file_id == node['graphic']:
                fields['m_RaycastTarget'] = 1
        self.mono(node, BUTTON, dict(m_Navigation=dict(m_Mode=0,m_WrapAround=0,
            m_SelectOnUp=ref(),m_SelectOnDown=ref(),m_SelectOnLeft=ref(),
            m_SelectOnRight=ref()), m_Transition=1,
            m_Colors=dict(m_NormalColor=color(),m_HighlightedColor=color(1,1,1,1),
                m_PressedColor=color(.75,.9,.95,1),m_SelectedColor=color(),
                m_DisabledColor=color(.55,.55,.55,.6),m_ColorMultiplier=1,m_FadeDuration=.1),
            m_SpriteState=dict(m_HighlightedSprite=ref(),m_PressedSprite=ref(),
                m_SelectedSprite=ref(),m_DisabledSprite=ref()),
            m_AnimationTriggers=dict(m_NormalTrigger='Normal',m_HighlightedTrigger='Highlighted',
                m_PressedTrigger='Pressed',m_SelectedTrigger='Selected',m_DisabledTrigger='Disabled'),
            m_Interactable=1,m_TargetGraphic=ref(node['graphic']),
            m_OnClick={'m_PersistentCalls':{'m_Calls':[]}}))

    def fit(self, node, ratio):
        self.mono(node, FITTER, dict(m_AspectMode=3,m_AspectRatio=ratio))

    def nested(self, path, parent, name, active=True, stretch=True):
        source = DOCS[path]
        source_guid = guid(path)
        instance_id, stripped_id = self.allocate(), self.allocate()
        mods = []
        for field, value in [('m_Name',name),('m_IsActive',int(active))]:
            mods.append(dict(target=ref(source.root['go_id'],source_guid),
                             propertyPath=field,value=str(value),objectReference=ref()))
        values = dict(m_AnchoredPosition=vec(), m_SizeDelta=vec(),
                      m_AnchorMin=vec(0,0),m_AnchorMax=vec(1,1)) if stretch else {}
        for key, components in values.items():
            for axis, value in components.items():
                mods.append(dict(target=ref(source.root['rt_id'],source_guid),
                    propertyPath=f'{key}.{axis}',value=str(value),objectReference=ref()))
        data=common()
        data.update(serializedVersion=2,m_Modification=dict(serializedVersion=3,
            m_TransformParent=ref(parent['rt_id']),m_Modifications=mods,
            m_RemovedComponents=[],m_RemovedGameObjects=[],m_AddedGameObjects=[],
            m_AddedComponents=[]),m_SourcePrefab=ref(100100000,source_guid))
        self.objects.append((1001,instance_id,'PrefabInstance',data))
        self.objects.append((224,stripped_id,'RectTransform',dict(
            m_CorrespondingSourceObject=ref(source.root['rt_id'],source_guid),
            m_PrefabInstance=ref(instance_id),m_PrefabAsset=ref())))
        parent['rt']['m_Children'].append(ref(stripped_id))

    def save(self, path, scene=False):
        header='%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n'
        chunks=[]
        for tag,file_id,kind,data in self.objects:
            stripped=' stripped' if ('m_GameObject' not in data and tag==224) else ''
            chunks.append(f'--- !u!{tag} &{file_id}{stripped}\n'+
                          '\n'.join(yaml_lines({kind:data}))+'\n')
        if scene:
            chunks.append('--- !u!1660057539 &9223372036854775807\nSceneRoots:\n'
                          '  m_ObjectHideFlags: 0\n  m_Roots:\n'
                          f'  - {{fileID: {self.root["rt_id"]}}}\n')
        contents=header+''.join(chunks)
        if not path.exists() or path.read_text(encoding='utf-8')!=contents:
            path.write_text(contents,encoding='utf-8')
        asset_meta(path)
        DOCS[path]=self


DOCS={}


def build_components():
    tile=Document('SkinSlot')
    tile.root['rt']['m_SizeDelta']=vec(240,240)
    tile.root['design_size']=(240,240)
    bg=tile.image('Frame',atlas('GlassTile'),(0,0),(240,240))
    tile.button_component(bg)
    tile.image('Portrait',MOTHER,(0,8),(195,160))
    tile.text('StateLabel','Owned',(0,-80),(200,42),30)
    tile.save(PREFABS/'Components/SkinSlot.prefab')
    power=Document('PowerUpOffer')
    power.root['rt']['m_SizeDelta']=vec(280,350)
    power.root['design_size']=(280,350)
    power.image('Frame',atlas('GlassTile'),(0,0),(280,350),preserve=False)
    power.image('PowerUpIcon',atlas('Undo'),(0,78),(150,150))
    power.text('Quantity','x3',(0,-20),(220,48),38)
    power.button('BuyButton','150',(0,-100),(230,85))
    power.save(PREFABS/'Components/PowerUpOffer.prefab')
    baby=Document('BabySharkVisual')
    baby.root['rt']['m_SizeDelta']=vec(430,300)
    baby.root['design_size']=(430,300)
    baby.image('BabyShark',BABY,(0,0),(430,300))
    baby.save(PREFABS/'Components/BabySharkVisual.prefab')


def build_pages():
    gift=Document('GiftPage')
    gift.text('Title','GIFTS',(0,590),(850,110),76)
    gift.image('RewardChest',atlas('RewardChest'),(0,245),(670,520))
    gift.text('RewardCaption','Hard Level Reward',(0,-30),(850,72),46)
    gift.button('OpenRewardButton','Open',(0,-150),(520,160))
    for x,name,quantity in [(-260,'Undo','x2'),(0,'Hint','x1'),(260,'Shuffle','x1')]:
        gift.image(name+'Reward',atlas(name),(x,-350),(185,185))
        gift.text(name+'Quantity',quantity,(x,-452),(180,50),36)
    gift.image('LockedChest',atlas('LockedChest'),(0,-555),(240,165))
    gift.text('LockedCaption','Beat a very hard level',(0,-650),(850,48),32)
    gift.node('NoRewardState',gift.root,active=False)
    gift.save(PREFABS/'Pages/GiftPage.prefab')

    shop=Document('ShopPage')
    shop.text('Title','SHOP',(0,590),(850,110),76)
    shop.image('FeaturedSkinPreview',MOTHER,(0,320),(690,370))
    shop.text('FeaturedSkinName','Classic',(0,90),(800,65),48)
    shop.text('FeaturedSkinPrice','800 coins',(-170,-5),(300,60),38)
    shop.button('BuySkinButton','Buy',(185,-5),(300,100))
    shop.text('PowerUpHeading','Power-ups',(0,-170),(850,75),54)
    for x,name,price in [(-310,'Undo',150),(0,'Hint',200),(310,'Shuffle',250)]:
        frame=shop.image(name+'Offer',atlas('GlassTile'),(x,-430),(290,390),preserve=False)
        shop.image('Icon',atlas(name),(0,100),(175,175),frame)
        shop.text('Quantity','x3',(0,-3),(230,55),38,frame)
        shop.text('Price',f'{price} coins',(0,-65),(230,45),30,frame)
        shop.button('BuyButton','Buy',(0,-140),(240,95),frame)
    shop.node('InsufficientCoinsState',shop.root,active=False)
    shop.save(PREFABS/'Pages/ShopPage.prefab')

    home=Document('HomePage')
    home.image('CurrentLevelBadge',sprite('Assets/_Game/Resources/Art/MenuUI/Clean/shark_level_badge.png'),
               (0,140),(680,920))
    home.text('LevelText','LEVEL\n49',(0,395),(430,240),80)
    home.image('PlayButton',sprite('Assets/_Game/Resources/Art/MenuUI/Clean/play_pearl.png'),
               (0,-430),(610,220))
    home.button_component(home.root if 'graphic' in home.root else
                          next_node(home,'PlayButton'))
    home.save(PREFABS/'Pages/HomePage.prefab')

    skins=Document('SkinPage')
    skins.text('Title','SKINS',(0,590),(850,110),76)
    skins.image('SelectedSharkPreview',MOTHER,(0,330),(650,370))
    skins.button('PreviousSkinButton','<',(-395,330),(115,115))
    skins.button('NextSkinButton','>',(395,330),(115,115))
    skins.text('SelectedSkinName','Classic',(0,90),(700,65),48)
    skins.button('EquipSkinButton','Equipped',(0,-10),(430,100))
    styles=[('Classic',color(),True),('Coral',color(1,.65,.83),True),
            ('Reef',color(.65,1,.72),True),('Gold',color(1,.87,.4),False),
            ('Twilight',color(.8,.65,1),False),('Ice',color(.65,.9,1),False)]
    for index,(name,tint,owned) in enumerate(styles):
        x=(-310,0,310)[index%3]
        y=-255-(index//3)*295
        frame=skins.image(name+'Slot',atlas('GlassTile'),(x,y),(275,275),
                          tint=color(.6,1,1) if index==0 else color())
        skins.button_component(frame)
        skins.image('Portrait',MOTHER,(0,26),(235,175),frame,tint)
        skins.text('SkinName',name,(0,-70),(240,45),30,frame)
        skins.text('OwnershipState','Equipped' if index==0 else ('Owned' if owned else 'Locked'),
                   (0,-108),(240,38),24,frame)
    skins.save(PREFABS/'Pages/SkinPage.prefab')

    tank=Document('TankPage')
    tank.text('Title','TANK',(0,590),(850,110),76)
    tank.image('BabySharkNear',BABY,(-175,90),(510,355),rotation=-8)
    tank.image('BabySharkFar',BABY,(220,335),(335,235),rotation=10)
    tank.image('BabySharkSmall',BABY,(180,-330),(370,260),rotation=-12)
    tank.node('SwimmingArea',tank.root,size=(900,1080))
    tank.save(PREFABS/'Pages/TankPage.prefab')


def next_node(doc,name):
    go=next((file_id,data) for tag,file_id,kind,data in doc.objects
            if tag==1 and data['m_Name']==name)
    rt=next((file_id,data) for tag,file_id,kind,data in doc.objects
            if tag==224 and data.get('m_GameObject')==ref(go[0]))
    graphic=next(file_id for tag,file_id,kind,data in doc.objects
                 if tag==114 and data.get('m_GameObject')==ref(go[0])
                 and data['m_Script']['guid']==IMAGE)
    return dict(go=go[1],go_id=go[0],rt=rt[1],rt_id=rt[0],graphic=graphic)


def build_shell():
    shell=Document('MenuShellDesign')
    shell.root['rt'].update(m_AnchorMin=vec(0,0),m_AnchorMax=vec(1,1),m_SizeDelta=vec())
    group=common(shell.root['go_id'])
    group.update(m_Enabled=1,m_Alpha=1,m_Interactable=1,m_BlocksRaycasts=1,
                 m_IgnoreParentGroups=0)
    group_id=shell.add(225,'CanvasGroup',group)
    shell.root['go']['m_Component'].append({'component':ref(group_id)})
    background=shell.image('OceanBackground',BG,(0,0),(0,0),preserve=False)
    background['rt'].update(m_AnchorMin=vec(0,0),m_AnchorMax=vec(1,1))
    wash=shell.image('BackgroundWash',None,(0,0),(0,0),
                     tint=color(.02,.28,.38,.15),preserve=False)
    wash['rt'].update(m_AnchorMin=vec(0,0),m_AnchorMax=vec(1,1))
    safe=shell.node('SafeAreaRoot',shell.root,size=(0,0),anchors=((.025,.035),(.975,.965)))
    header=shell.node('Header',safe,pos=(0,-85),size=(0,150),anchors=((0,1),(1,1)))
    hud=shell.node('HeaderLayout',header,size=(1000,150))
    shell.fit(hud,1000/150)
    coins=shell.image('CoinBar',sprite('Assets/_Game/Resources/Art/MenuUI/Clean/coin_gold_blank.png'),
                      (-280,0),(350,125),hud)
    shell.text('CoinAmount','1580',(30,0),(210,75),42,coins)
    shell.image('EnergyBar',sprite('Assets/_Game/Resources/Art/MenuUI/Clean/energy_clean.png'),
                (80,0),(320,120),hud)
    settings=shell.image('SettingsButton',sprite('Assets/_Game/Resources/Art/MenuUI/Clean/settings_coral.png'),
                         (410,0),(140,140),hud)
    shell.button_component(settings)
    viewport=shell.node('ContentViewport',safe,size=(0,-395),pos=(0,15),anchors=((0,0),(1,1)))
    shell.mono(viewport,'3312d7739989d2b4e91e6319e9a96d76',
               dict(m_Padding=vec(0,0,0,0),m_Softness=vec(0,0)))
    content=shell.node('PageContainer',viewport,size=(W,H))
    shell.fit(content,W/H)
    for name in ['Gift','Shop','Home','Skin','Tank']:
        shell.nested(PREFABS/f'Pages/{name}Page.prefab',content,name+'Page',name=='Home')
    navigation=shell.node('BottomNavigation',safe,pos=(0,115),size=(0,230),anchors=((0,0),(1,0)))
    nav=shell.node('NavigationLayout',navigation,size=(1000,230))
    shell.fit(nav,1000/230)
    shell.image('NavigationBand',None,(0,0),(1000,220),nav,
                color(.025,.24,.38,.94),preserve=False)
    for i,name in enumerate(['Gift','Shop','Home','Skin','Tank']):
        x=(i-2)*195
        item=shell.node(name+'Tab',nav,pos=(x,0),size=(180,220))
        highlight=shell.image('SelectedHighlight',atlas('GlassTile'),(0,0),(190,235),
                              item,color(.25,1,1,1),active=name=='Home',preserve=False)
        icon=shell.image('Icon',atlas(name+'Icon'),(0,28),(135,135),item)
        shell.button_component(icon)
        shell.text('Label',name,(0,-70),(180,50),32,item)
    shell.save(PREFABS/'MenuShellDesign.prefab')


def build_scene(name):
    doc=Document('MenuDesignCanvas')
    canvas=common(doc.root['go_id'])
    canvas.update(m_Enabled=1,serializedVersion=3,m_RenderMode=0,m_Camera=ref(),
        m_PlaneDistance=100,m_PixelPerfect=0,m_ReceivesEvents=1,m_OverrideSorting=0,
        m_OverridePixelPerfect=0,m_SortingBucketNormalizedSize=0,
        m_VertexColorAlwaysGammaSpace=0,m_AdditionalShaderChannelsFlag=25,
        m_UpdateRectTransformForStandalone=0,m_SortingLayerID=0,m_SortingOrder=0,
        m_TargetDisplay=0)
    file_id=doc.add(223,'Canvas',canvas)
    doc.root['go']['m_Component'].append({'component':ref(file_id)})
    doc.mono(doc.root,'0cd44c1031e13a943bb63640046fad76',dict(m_UiScaleMode=1,
        m_ReferencePixelsPerUnit=100,m_ScaleFactor=1,m_ReferenceResolution=vec(1080,1920),
        m_ScreenMatchMode=0,m_MatchWidthOrHeight=.5,m_PhysicalUnit=3,
        m_FallbackScreenDPI=96,m_DefaultSpriteDPI=96,m_DynamicPixelsPerUnit=1,
        m_PresetInfoIsWorld=0))
    # Inline shell for an easily selectable design scene; pages remain prefab instances.
    shell=copy.deepcopy(DOCS[PREFABS/'MenuShellDesign.prefab'])
    offset=100000
    def remap(value):
        if isinstance(value,dict):
            if 'fileID' in value and 'guid' not in value and value['fileID']:
                value['fileID']+=offset
            else:
                for item in value.values(): remap(item)
        elif isinstance(value,list):
            for item in value: remap(item)
    for tag,file_id,kind,data in shell.objects:
        remap(data)
        doc.objects.append((tag,file_id+offset,kind,data))
    shell.root['rt']['m_Father']=ref(doc.root['rt_id'])
    doc.root['rt']['m_Children'].append(ref(shell.root['rt_id']+offset))
    if name!='Home':
        for tag,file_id,kind,data in doc.objects:
            if tag==1001:
                mods=data['m_Modification']['m_Modifications']
                page=next(m['value'] for m in mods if m['propertyPath']=='m_Name')
                next(m for m in mods if m['propertyPath']=='m_IsActive')['value']=str(int(page==name+'Page'))
            if tag==1 and data['m_Name']=='SelectedHighlight':
                data['m_IsActive']=0
        # Highlight the requested tab without introducing a runtime controller.
        target_go=next(data for tag,_,_,data in doc.objects
                       if tag==1 and data['m_Name']==name+'Tab')
        target_rt=target_go['m_Component'][0]['component']['fileID']
        child_ids=next(data['m_Children'] for tag,file_id,_,data in doc.objects if file_id==target_rt)
        highlight_rt=child_ids[0]['fileID']
        highlight_go=next(data['m_GameObject']['fileID'] for tag,file_id,_,data in doc.objects if file_id==highlight_rt)
        next(data for tag,file_id,_,data in doc.objects if file_id==highlight_go)['m_IsActive']=1
    doc.save(SCENES/f'{name}Design.unity',scene=True)


def import_atlas(width,height):
    source=(ROOT/'Assets/_Game/Resources/Art/MenuUI/Clean/play_clean.png.meta').read_text()
    prefix=source.split('  spriteSheet:')[0]
    prefix=re.sub(r'^guid: \w+',f'guid: {guid(ART/"MenuObjects.png")}',prefix,flags=re.M)
    prefix=prefix.replace('spriteMode: 1','spriteMode: 2').replace('maxTextureSize: 2048','maxTextureSize: 4096')
    prefix=prefix.replace('textureCompression: 1','textureCompression: 0')
    ids='  internalIDToNameTable:\n'+''.join(
        f'  - first:\n      213: {21300001+i}\n    second: {name}\n' for i,name in enumerate(ATLAS_NAMES))
    prefix=prefix.replace('  internalIDToNameTable: []\n',ids)
    sheets=['  spriteSheet:\n    serializedVersion: 2\n    sprites:\n']
    # Sprite import rectangles are authoring data, measured against the generated atlas.
    rects=[(0,0,410,400),(410,80,320,320),(728,200,358,160),
           (0,400,365,330),(375,400,335,330),(725,400,345,330),
           (0,745,365,315),(390,745,325,315),(725,745,355,315),
           (0,1080,365,330),(375,1080,345,330),(735,1080,345,330)]
    for i,name in enumerate(ATLAS_NAMES):
        x,top,cw,ch=rects[i]
        y=height-top-ch
        sheets.append(f'    - serializedVersion: 2\n      name: {name}\n'
            f'      rect:\n        serializedVersion: 2\n        x: {x}\n        y: {y}\n'
            f'        width: {cw}\n        height: {ch}\n      alignment: 0\n'
            '      pivot: {x: 0.5, y: 0.5}\n      border: {x: 0, y: 0, z: 0, w: 0}\n'
            '      customData: \n      outline: []\n      physicsShape: []\n'
            f'      tessellationDetail: -1\n      bones: []\n      spriteID: {hashlib.md5(name.encode()).hexdigest()}\n'
            f'      internalID: {21300001+i}\n      vertices: []\n      indices: \n      edges: []\n      weights: []\n')
    sheets.append('    outline: []\n    customData: \n    physicsShape: []\n    bones: []\n'
        '    spriteID: \n    internalID: 0\n    vertices: []\n    indices: \n    edges: []\n'
        '    weights: []\n    secondaryTextures: []\n    nameFileIdTable:\n')
    sheets.extend(f'      {name}: {21300001+i}\n' for i,name in enumerate(ATLAS_NAMES))
    sheets.append('  mipmapLimitGroupName: \n  pSDRemoveMatte: 0\n  userData: \n'
                  '  assetBundleName: \n  assetBundleVariant: \n')
    (ART/'MenuObjects.png.meta').write_text(prefix+''.join(sheets),encoding='utf-8')


def text_material():
    source=(ROOT/'Assets/_Game/Resources/Fonts/LilitaOne-Regular SDF.asset').read_text()
    material=source.split('--- !u!')[1]
    material='--- !u!'+material
    material=material.replace('&-6823154853810081404','&2100000')
    material=material.replace('m_Name: LilitaOne-Regular SDF Material','m_Name: MenuText')
    material=material.replace('m_Texture: {fileID: 4097147507068566325}',
        'm_Texture: {fileID: 4097147507068566325, guid: '+FONT+', type: 2}')
    material=material.replace('_OutlineWidth: 0','_OutlineWidth: 0.12')
    material=material.replace('_OutlineColor: {r: 0, g: 0, b: 0, a: 1}',
                              '_OutlineColor: {r: 0.01, g: 0.12, b: 0.22, a: 1}')
    path=ART/'MenuText.mat'
    path.write_text('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n'+material)
    Path(str(path)+'.meta').write_text(f'fileFormatVersion: 2\nguid: {guid(path)}\n'
        'NativeFormatImporter:\n  externalObjects: {}\n  mainObjectFileID: 2100000\n'
        '  userData: \n  assetBundleName: \n  assetBundleVariant: \n')


def happy_baby_importer():
    path=ART/'BabySharkHappy.png'
    source=(ROOT/'Assets/_Game/Resources/Art/MenuUI/Clean/play_clean.png.meta').read_text()
    source=re.sub(r'^guid: \w+',f'guid: {guid(path)}',source,flags=re.M)
    source=re.sub(r'spriteID: \w+',f'spriteID: {guid(path)}',source)
    source=source.replace('textureCompression: 1','textureCompression: 0')
    Path(str(path)+'.meta').write_text(source,encoding='utf-8')


if __name__=='__main__':
    import sys
    for folder in [ART,PREFABS/'Pages',PREFABS/'Components',SCENES]:
        directory(folder)
    if len(sys.argv)==3:
        import_atlas(int(sys.argv[1]),int(sys.argv[2]))
    text_material()
    happy_baby_importer()
    build_components()
    build_pages()
    build_shell()
    for page in ['Gift','Shop','Home','Skin','Tank']:
        build_scene(page)
    print('Authored 9 prefabs and 5 design scenes. Existing runtime assets untouched.')
