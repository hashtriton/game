"""Сборка ../menu-mockup.html: template.html + встроенные текстуры + scene.js + still.jpg (кадр-заставка).

Порядок (нужны numpy, scipy, Pillow), из этой папки:
    python -X utf8 tg.py stone ground cobble
    python -X utf8 tg2.py wood crate barrel bark cloth cloth_alpha rust pine flame mist blood scorch macro
    python -X utf8 build.py

Текстуры генерируются в tex/ (в git не хранятся). still.jpg снят с живой сцены через canvas.toDataURL
после отрисовки кадра (в файле зеркален: страница разворачивает его CSS, как и 3D-холст).
"""
import os, base64, json

d = os.path.dirname(os.path.abspath(__file__))
tex = os.path.join(d, 'tex')
names = ['stone_a', 'stone_n', 'stone_r', 'ground_a', 'ground_n', 'ground_r', 'cobble_a', 'cobble_n', 'cobble_r',
         'wood_a', 'wood_n', 'wood_r', 'crate_a', 'crate_n', 'crate_r', 'barrel_a', 'barrel_n', 'barrel_r',
         'bark_a', 'bark_n', 'bark_r', 'cloth_a', 'cloth_n', 'cloth_alpha', 'rust_a', 'rust_n', 'rust_r',
         'pine', 'flame', 'mist', 'blood', 'scorch', 'macro']
png = ('pine', 'flame', 'mist', 'blood', 'scorch')


def data_uri(path, mime):
    with open(path, 'rb') as f:
        return 'data:%s;base64,%s' % (mime, base64.b64encode(f.read()).decode())


parts = []
for n in names:
    ext, mime = ('png', 'image/png') if n in png else ('jpg', 'image/jpeg')
    parts.append("%s:'%s'" % (json.dumps(n), data_uri(os.path.join(tex, n + '.' + ext), mime)))
texdata = 'var TEXDATA={' + ',\n'.join(parts) + '};\n'

with open(os.path.join(d, 'scene.js'), encoding='utf8') as f:
    scene = f.read()
with open(os.path.join(d, 'template.html'), encoding='utf8') as f:
    tpl = f.read()
assert '/*@@SCENE@@*/' in tpl and '/*@@STILL@@*/' in tpl

stillp = os.path.join(d, 'still.jpg')
still = 'background-image:url(%s)' % data_uri(stillp, 'image/jpeg') if os.path.exists(stillp) else ''
out = tpl.replace('/*@@STILL@@*/', still).replace('/*@@SCENE@@*/', texdata + scene)

# длинные и средние тире в файлах запрещены правилами проекта
for code in (0x2012, 0x2013, 0x2014):
    assert chr(code) not in out, 'в сборке найден символ U+%04X' % code

with open(os.path.join(d, '..', 'menu-mockup.html'), 'w', encoding='utf8', newline='') as f:
    f.write(out)
print('собрано', len(out) // 1024, 'КБ; текстуры', len(texdata) // 1024, 'КБ')
