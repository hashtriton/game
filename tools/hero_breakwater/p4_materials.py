"""Six low-contrast procedural surfaces for a daylight material study."""
import bpy


def material(name, color, metallic, roughness, kind):
    m = bpy.data.materials.new(name)
    m.diffuse_color = (*color, 1)
    m.use_nodes = True
    nodes, links = m.node_tree.nodes, m.node_tree.links
    nodes.clear()
    output = nodes.new('ShaderNodeOutputMaterial')
    bsdf = nodes.new('ShaderNodeBsdfPrincipled')
    links.new(bsdf.outputs['BSDF'], output.inputs['Surface'])
    bsdf.inputs['Base Color'].default_value = (*color, 1)
    bsdf.inputs['Metallic'].default_value = metallic
    bsdf.inputs['Roughness'].default_value = roughness
    if kind == 'cyan':
        bsdf.inputs['Emission Color'].default_value = (*color, 1)
        bsdf.inputs['Emission Strength'].default_value = .12
        return m
    coord = nodes.new('ShaderNodeTexCoord')
    stretch = nodes.new('ShaderNodeVectorMath')
    stretch.operation = 'MULTIPLY'
    stretch.inputs[1].default_value = (55, 55, 3) if kind == 'steel' else (2, 2, 2)
    links.new(coord.outputs['Object'], stretch.inputs[0])
    noise = nodes.new('ShaderNodeTexNoise')
    noise.inputs['Scale'].default_value = 2.2
    noise.inputs['Detail'].default_value = 1.5
    links.new(stretch.outputs['Vector'], noise.inputs['Vector'])
    ramp = nodes.new('ShaderNodeMapRange')
    ramp.inputs['To Min'].default_value = roughness - .035
    ramp.inputs['To Max'].default_value = roughness + .04
    links.new(noise.outputs['Fac'], ramp.inputs['Value'])
    links.new(ramp.outputs['Result'], bsdf.inputs['Roughness'])
    if kind in ['paint', 'steel', 'dark', 'bronze']:
        bevel = nodes.new('ShaderNodeBevel')
        bevel.inputs['Radius'].default_value = .0025
        bevel.samples = 3
        links.new(bevel.outputs['Normal'], bsdf.inputs['Normal'])
        geometry = nodes.new('ShaderNodeNewGeometry')
        dot = nodes.new('ShaderNodeVectorMath')
        dot.operation = 'DOT_PRODUCT'
        links.new(bevel.outputs['Normal'], dot.inputs[0])
        links.new(geometry.outputs['Normal'], dot.inputs[1])
        wear = nodes.new('ShaderNodeMapRange')
        wear.clamp = True
        wear.inputs['From Min'].default_value = .94
        wear.inputs['From Max'].default_value = .999
        wear.inputs['To Min'].default_value = .92 if kind == 'paint' else .35
        wear.inputs['To Max'].default_value = 0
        links.new(dot.outputs['Value'], wear.inputs['Value'])
        mix = nodes.new('ShaderNodeMixRGB')
        mix.blend_type = 'MIX'
        mix.inputs[1].default_value = (*color, 1)
        mix.inputs[2].default_value = (.16, .20, .24, 1) if kind == 'paint' else (*[min(1, c * 1.7) for c in color], 1)
        links.new(wear.outputs['Result'], mix.inputs[0])
        ao = nodes.new('ShaderNodeAmbientOcclusion')
        ao.inputs['Distance'].default_value = .10
        ao.samples = 8
        dirt = nodes.new('ShaderNodeMapRange')
        dirt.inputs['To Min'].default_value = .76
        dirt.inputs['To Max'].default_value = 1
        links.new(ao.outputs['AO'], dirt.inputs['Value'])
        shade = nodes.new('ShaderNodeMixRGB')
        shade.blend_type = 'MULTIPLY'
        shade.inputs[0].default_value = 1
        links.new(mix.outputs['Color'], shade.inputs[1])
        links.new(dirt.outputs['Result'], shade.inputs[2])
        links.new(shade.outputs['Color'], bsdf.inputs['Base Color'])
        variation=nodes.new('ShaderNodeMapRange')
        variation.inputs['To Min'].default_value=.93
        variation.inputs['To Max'].default_value=1.035
        links.new(noise.outputs['Fac'],variation.inputs['Value'])
        coarse=nodes.new('ShaderNodeMixRGB')
        coarse.blend_type='MULTIPLY'
        coarse.inputs[0].default_value=1
        links.new(shade.outputs['Color'],coarse.inputs[1])
        links.new(variation.outputs['Result'],coarse.inputs[2])
        links.new(coarse.outputs['Color'],bsdf.inputs['Base Color'])
        if kind=='paint':
            metal=nodes.new('ShaderNodeMapRange')
            metal.inputs['To Min'].default_value=.10
            metal.inputs['To Max'].default_value=.87
            links.new(wear.outputs['Result'],metal.inputs['Value'])
            links.new(metal.outputs['Result'],bsdf.inputs['Metallic'])
            bsdf.inputs['Coat Weight'].default_value=.06
            bsdf.inputs['Coat Roughness'].default_value=.34
    if kind == 'steel':
        bsdf.inputs['Anisotropic'].default_value = .35
        bump=nodes.new('ShaderNodeBump')
        bump.inputs['Strength'].default_value=.06
        bump.inputs['Distance'].default_value=.0003
        links.new(noise.outputs['Fac'],bump.inputs['Height'])
        links.new(bevel.outputs['Normal'],bump.inputs['Normal'])
        links.new(bump.outputs['Normal'],bsdf.inputs['Normal'])
    if kind == 'cloth':
        weave = nodes.new('ShaderNodeTexWave')
        weave.wave_type = 'BANDS'
        weave.bands_direction = 'DIAGONAL'
        weave.inputs['Scale'].default_value = 90
        links.new(coord.outputs['Object'], weave.inputs['Vector'])
        bump = nodes.new('ShaderNodeBump')
        bump.inputs['Strength'].default_value = .07
        bump.inputs['Distance'].default_value = .0008
        links.new(weave.outputs['Fac'], bump.inputs['Height'])
        links.new(bump.outputs['Normal'], bsdf.inputs['Normal'])
        bsdf.inputs['Sheen Weight'].default_value = .18
    for i, n in enumerate(nodes):
        n.location = ((i % 5) * 210, -(i // 5) * 180)
    return m


def create():
    return {
        'paint': material('HB_WhitePaintedSteel', (.79, .78, .73), .10, .32, 'paint'),
        'steel': material('HB_BrushedSteel', (.43, .50, .57), .85, .27, 'steel'),
        'navy': material('HB_NavyClothLeather', (.028, .055, .095), 0, .76, 'cloth'),
        'dark': material('HB_DarkJointSteel', (.035, .048, .060), .68, .48, 'dark'),
        'cyan': material('HB_CyanAccent', (.018, .48, .62), .12, .42, 'cyan'),
        'bronze': material('HB_RestrainedBronze', (.32, .21, .10), .74, .39, 'bronze'),
    }
