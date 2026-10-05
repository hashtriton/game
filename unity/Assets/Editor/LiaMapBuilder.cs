using System;
using System.IO;
using System.Linq;
using Arena;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>Reconstructs the measured 3.9c layout with CC0 replacement scenery.</summary>
public static class LiaMapBuilder
{
    const string Root="Assets/Arena/Generated";
    const string Castle="Assets/ThirdParty/KenneyCastle/";
    const string Characters="Assets/ThirdParty/Quaternius/";
    public const string ScenePath="Assets/Arena/Scenes/Lia39Arena.unity";

    [MenuItem("Game/Rebuild Life in Arena 3.9c layout")]
    public static void Build()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode first.");
        if(Enumerable.Range(0,UnityEngine.SceneManagement.SceneManager.sceneCount).Any(i=>UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty))
            throw new InvalidOperationException("Save open scenes before rebuilding.");
        Directory.CreateDirectory(Root);Directory.CreateDirectory("Assets/Arena/Scenes");
        AssetDatabase.Refresh();ArenaBuilder.ConfigureRendering();
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var mapObject=new GameObject("Life in Arena 3.9c - measured layout");
        var map=mapObject.AddComponent<ArenaMap>();
        map.layoutJson=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Arena/Data/lia39-layout.json");
        if(!map.layoutJson)throw new FileNotFoundException("Extract the local original map first.");
        map.unitsPerMeter=64;
        var data=map.Layout;
        BuildTerrain(map);
        var props=new GameObject("Original doodad positions - CC0 substitutes").transform;
        var palette=ArenaBuilder.Material("Castle palette",Color.white,Castle+"Textures/variation-a.png");
        int visibleCount=0,skippedCount=0,measuredCount=0,barrelCount=0;
        foreach(var d in data.doodads)
        {
            string model=ReplacementModel(d);
            if(model==null){skippedCount++;continue;}
            PlaceDoodad(d,map,props,model,palette);
            visibleCount++;
            if(d.geometryBoundsAvailable)measuredCount++;
            if(IsBarrel(d))barrelCount++;
        }
        var landmarks=new GameObject("Original service and spawn locations").transform;
        foreach(var unit in data.staticUnits)
        {
            // x0 initializes vendors. Compact Wi shops are an alternative display
            // mode at one shared position; do not instantiate them over the Nn layout.
            if(unit.@function!="x0")continue;
            bool tavern=unit.sourceLine>=15558 && unit.sourceLine<=15659;
            bool fountain=unit.id=="n00A" && unit.sourceLine==15692;
            bool shop=unit.sourceLine>=15694 && unit.sourceLine<=15706;
            if(!tavern && !fountain && !shop)continue;
            var point=new Vector3(unit.x/map.unitsPerMeter,0,unit.y/map.unitsPerMeter);
            point.y=map.SampleHeight(point);
            string asset=ServiceModel(tavern,fountain,unit.sourceLine);
            var servicePivot=new GameObject(unit.id+" / "+unit.name).transform;
            servicePivot.SetParent(landmarks,false);
            var building=ReplacementPrefab(asset,servicePivot,ReplacementMaterial(asset,palette));
            ArenaBuilder.Fit(building,Vector3.zero,fountain?2.4f:tavern?1.35f:1.15f,true);
            servicePivot.position=point;
            servicePivot.rotation=Quaternion.Euler(0,90-unit.facing,0);
        }
        var heroMat=ArenaBuilder.Material("Warrior",Color.white,Characters+"Warrior_Texture.png");
        var swordMat=ArenaBuilder.Material("Warrior sword",Color.white,Characters+"Warrior_Sword_Texture.png");
        var monsterMat=ArenaBuilder.Material("Monsters",Color.white,Characters+"Atlas_Monsters.png");
        var hero=ArenaBuilder.CharacterPrefab("Warrior",heroMat,swordMat,"Idle_Weapon","Run_Weapon","Sword_Attack");
        var melee=ArenaBuilder.CharacterPrefab("Orc",monsterMat,monsterMat,"Idle","Run","Punch");
        var ranged=ArenaBuilder.CharacterPrefab("GreenBlob",monsterMat,monsterMat,"Idle","Walk","Bite_Front");
        var boss=ArenaBuilder.CharacterPrefab("Demon",monsterMat,monsterMat,"Idle","Run","Punch");
        var effect=ArenaBuilder.Material("Combat effects",Color.white,null,0,true);
        effect.SetFloat("_Surface",1);effect.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);
        effect.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);effect.SetFloat("_ZWrite",0);
        effect.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");effect.renderQueue=(int)RenderQueue.Transparent;
        var camera=new GameObject("Arena camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();
        camera.tag="MainCamera";camera.transform.position=new Vector3(-.78f,31,-4);
        camera.transform.rotation=Quaternion.Euler(56,0,0);camera.fieldOfView=50;camera.farClipPlane=350;
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.075f,.105f,.09f);
        camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
        var sun=new GameObject("Sun").AddComponent<Light>();sun.type=LightType.Directional;
        sun.transform.rotation=Quaternion.Euler(52,-30,0);sun.color=new Color(1,.92f,.79f);sun.intensity=1.4f;sun.shadows=LightShadows.Soft;
        RenderSettings.ambientMode=AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=new Color(.52f,.59f,.64f);RenderSettings.ambientEquatorColor=new Color(.42f,.47f,.39f);
        RenderSettings.ambientGroundColor=new Color(.22f,.25f,.17f);RenderSettings.fog=false;
        var systems=new GameObject("Arena game");var game=systems.AddComponent<ArenaGame>();
        game.arenaMap=map;game.heroPrefab=hero;game.meleePrefab=melee;game.rangedPrefab=ranged;game.bossPrefab=boss;
        game.arenaCamera=camera;game.effectMaterial=effect;
        var hud=systems.AddComponent<ArenaHud>();hud.game=game;
        hud.mapTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Lia39Minimap.png");
        var control=camera.gameObject.AddComponent<ArenaMapCamera>();control.game=game;control.view=camera;
        PlayerSettings.productName="Arena Map Prototype";PlayerSettings.runInBackground=true;
        PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        if(!EditorSceneManager.SaveScene(scene,ScenePath))throw new IOException("Map scene failed to save.");
        AssetDatabase.SaveAssets();Selection.activeGameObject=mapObject;
        Debug.Log("LIA39_MAP_BUILT terrain="+data.terrain.width+"x"+data.terrain.height+
            " sourceDoodads="+data.doodads.Length+" visibleSubstitutes="+visibleCount+
            " measuredGeometry="+measuredCount+" barrels="+barrelCount+" skipped="+skippedCount);
    }

    static bool IsBarrel(MapDoodad d) => d.id=="LTbr" || d.id=="LTbs" || d.id=="LTex" || d.id=="D01F" || d.id=="D01G";

    static string ReplacementModel(MapDoodad d)
    {
        string source=(d.modelPathReference??"").ToLowerInvariant();
        if(d.category=="blocker" || d.category=="hidden" || d.category=="light" ||
            source.Contains("dummy") || source.Contains("glow") || source.Contains("omni"))return null;
        if(IsBarrel(d))return BarrelModel();
        if(source.Contains("crate") || d.id=="LTba")return CrateModel();
        if(source.Contains("tree_root"))return Castle+"tree-log.fbx";
        if(d.category=="tree")return Castle+"tree-large.fbx";
        if(d.category=="plant")return Castle+"tree-small.fbx";
        if(d.category=="rock")return Castle+"rocks-small.fbx";
        if(d.category=="pillar" || d.category=="statue")return Castle+"wall-pillar.fbx";
        if(d.category=="wall")return Castle+(source.Contains("fence") || source.Contains("lattice")?
            "wall-narrow-wood-fence.fbx":source.Contains("bigblock")?"wall-stud.fbx":"wall-half-modular.fbx");
        if(d.category=="floor")return Castle+(source.Contains("ladder") || source.Contains("stair")?
            "stairs-stone.fbx":"ground.fbx");
        if(source.StartsWith("r1_") || source.StartsWith("r2_") || source.StartsWith("r3_"))
        {
            // Opaque ruin names are classified by measured rest geometry, never by
            // guessing that every unknown source object was a rock.
            if(!d.geometryBoundsAvailable)return null;
            Vector3 size=OriginalGeometrySize(d);
            float wide=Mathf.Max(size.x,size.z),narrow=Mathf.Min(size.x,size.z);
            if(size.y>wide*1.3f)return Castle+"wall-pillar.fbx";
            if(wide>narrow*2f && size.y>narrow*.3f)return Castle+"wall-half-modular.fbx";
            if(source.Contains("ring"))return Castle+"tower-hexagon-top.fbx";
            return Castle+(size.y<wide*.45f?"ground.fbx":"wall-stud.fbx");
        }
        // Small props with no meaningful free equivalent remain explicitly absent.
        return null;
    }

    static Vector3 OriginalGeometrySize(MapDoodad d)
    {
        var min=d.geometryBoundsMin;var max=d.geometryBoundsMax;
        return new Vector3(max[0]-min[0],max[2]-min[2],max[1]-min[1]);
    }

    static void PlaceDoodad(MapDoodad d,ArenaMap map,Transform parent,string asset,Material palette)
    {
        var pivot=new GameObject(d.id+"#"+d.editorId+" / "+d.name).transform;
        pivot.SetParent(parent,false);
        // Fit in a separate identity frame so imported FBX root rotations/scales
        // cannot exchange the measured horizontal and vertical axes.
        var fitted=new GameObject("Fitted CC0 replacement").transform;
        fitted.SetParent(pivot,false);
        var model=ReplacementPrefab(asset,fitted,d.category=="floor" || asset.EndsWith("ground.fbx")?
            ArenaBuilder.Material("Ruin floor stone",new Color(.42f,.40f,.34f)):ReplacementMaterial(asset,palette));
        var renderers=model.GetComponentsInChildren<Renderer>();
        if(renderers.Length==0)throw new InvalidOperationException("Replacement has no renderers: "+asset);
        var bounds=renderers[0].bounds;
        foreach(var renderer in renderers.Skip(1))bounds.Encapsulate(renderer.bounds);
        Vector3 scale=d.scale!=null && d.scale.Length==3?
            new Vector3(d.scale[0],d.scale[2],d.scale[1]):Vector3.one;
        Vector3 targetSize,targetCenter;
        if(d.geometryBoundsAvailable && d.geometryBoundsMin?.Length==3 && d.geometryBoundsMax?.Length==3)
        {
            var min=d.geometryBoundsMin;var max=d.geometryBoundsMax;
            targetSize=Vector3.Scale(OriginalGeometrySize(d),scale)/map.unitsPerMeter;
            targetCenter=Vector3.Scale(new Vector3((min[0]+max[0])*.5f,(min[2]+max[2])*.5f,
                (min[1]+max[1])*.5f),scale)/map.unitsPerMeter;
        }
        else
        {
            // Stock mesh geometry is external to the map. These dimensions are
            // explicit substitute estimates; source XYZ, rotation and scale remain.
            Vector3 estimate=IsBarrel(d)?new Vector3(42,60,42):
                d.category=="tree"?new Vector3(150,270,150):
                d.category=="plant"?new Vector3(40,35,40):
                d.category=="rock"?new Vector3(85,70,85):new Vector3(60,65,60);
            targetSize=Vector3.Scale(estimate,scale)/map.unitsPerMeter;
            targetCenter=Vector3.up*(targetSize.y*.5f);
            pivot.name+=" [stock size estimate]";
        }
        var factor=new Vector3(targetSize.x/Mathf.Max(.001f,bounds.size.x),
            targetSize.y/Mathf.Max(.001f,bounds.size.y),targetSize.z/Mathf.Max(.001f,bounds.size.z));
        fitted.localScale=factor;
        fitted.localPosition=targetCenter-Vector3.Scale(bounds.center,factor);
        // Original .doo Z is an absolute authored elevation. Do not snap it to
        // terrain: buried foundations and raised scenery intentionally differ.
        pivot.position=new Vector3(d.x,d.z,d.y)/map.unitsPerMeter;
        pivot.rotation=Quaternion.Euler(0,-d.rotationRadians*Mathf.Rad2Deg,0);
        foreach(var child in pivot.GetComponentsInChildren<Transform>())child.gameObject.isStatic=true;
    }

    static string BarrelModel() => "Assets/ThirdParty/KenneyPirate/Models/barrel.fbx";
    static string CrateModel() => "Assets/ThirdParty/KenneyPirate/Models/crate.fbx";
    static string ServiceModel(bool tavern,bool fountain,int sourceLine) => fountain?
        "Assets/ThirdParty/KenneyFantasyTown/Models/fountain-round-detail.fbx":tavern?
        "Assets/ThirdParty/KenneyPirate/Models/structure.fbx":
        "Assets/ThirdParty/KenneyFantasyTown/Models/stall-"+(sourceLine%2==0?"red":"green")+".fbx";
    static Material ReplacementMaterial(string asset,Material castlePalette)
    {
        if(asset.StartsWith(Castle))return castlePalette;
        bool pirate=asset.Contains("KenneyPirate");
        return ArenaBuilder.Material(pirate?"Pirate palette":"Town palette",Color.white,
            "Assets/ThirdParty/"+(pirate?"KenneyPirate":"KenneyFantasyTown")+"/Models/Textures/colormap.png");
    }

    static GameObject ReplacementPrefab(string asset,Transform parent,Material palette)
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(asset);
        if(!prefab)throw new FileNotFoundException("Required CC0 replacement is missing: "+asset);
        var result=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
        result.transform.SetParent(parent,false);
        foreach(var renderer in result.GetComponentsInChildren<Renderer>())
            renderer.sharedMaterials=renderer.sharedMaterials.Select(material=>
                material && material.name.IndexOf("Water",StringComparison.OrdinalIgnoreCase)>=0?
                ArenaBuilder.Material("Fountain water",new Color(.476415f,.86381f,1f)):palette).ToArray();
        return result;
    }

    static void BuildTerrain(ArenaMap map)
    {
        var source=map.Layout.terrain;
        var terrainData=AssetDatabase.LoadAssetAtPath<TerrainData>(Root+"/Lia39Terrain.asset");
        if(!terrainData){terrainData=new TerrainData();AssetDatabase.CreateAsset(terrainData,Root+"/Lia39Terrain.asset");}
        terrainData.heightmapResolution=source.width;
        terrainData.size=new Vector3((source.width-1)*source.cellSize/map.unitsPerMeter,20,(source.height-1)*source.cellSize/map.unitsPerMeter);
        var heights=new float[source.height,source.width];
        for(int z=0;z<source.height;z++)for(int x=0;x<source.width;x++)heights[z,x]=(source.vertices[z*source.width+x].height/map.unitsPerMeter+10)/20;
        terrainData.SetHeights(0,0,heights);
        var layers=new TerrainLayer[source.groundTextures.Length];
        for(int i=0;i<layers.Length;i++)
        {
            string id=source.groundTextures[i];string path=Root+"/Terrain_"+id+".terrainlayer";
            var layer=AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
            if(!layer){layer=new TerrainLayer();AssetDatabase.CreateAsset(layer,path);}
            string texPath=Root+"/Terrain_"+id+".asset";
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if(!texture){texture=new Texture2D(32,32,TextureFormat.RGB24,false);AssetDatabase.CreateAsset(texture,texPath);}
            var pixels=new Color[32*32];var random=new System.Random(100+i);var tint=GroundColor(id);
            for(int y=0;y<32;y++)for(int x=0;x<32;x++)
            {
                float pattern=1;
                if(id=="Qcbp" || id=="Qstp")
                {
                    int row=y/8,brickX=(x+(row%2)*4)%8;
                    if(y%8==0 || brickX==0)pattern=.72f;
                }
                else if(id=="Qcrp")pattern=x%6<2?.76f:1.06f;
                pixels[y*32+x]=tint*(.92f+(float)random.NextDouble()*.16f)*pattern;
            }
            texture.SetPixels(pixels);texture.Apply();texture.wrapMode=TextureWrapMode.Repeat;
            layer.diffuseTexture=texture;layer.tileSize=new Vector2(2,2);layers[i]=layer;EditorUtility.SetDirty(layer);EditorUtility.SetDirty(texture);
        }
        terrainData.terrainLayers=layers;terrainData.alphamapResolution=256;
        var weights=new float[256,256,layers.Length];
        for(int z=0;z<256;z++)for(int x=0;x<256;x++)
        {
            float gx=x/255f*(source.width-1),gz=z/255f*(source.height-1);
            int vx=Mathf.Min(source.width-2,Mathf.FloorToInt(gx)),vz=Mathf.Min(source.height-2,Mathf.FloorToInt(gz));
            float fx=gx-vx,fz=gz-vz;
            for(int dz=0;dz<2;dz++)for(int dx=0;dx<2;dx++)
            {
                int texture=source.vertices[(vz+dz)*source.width+vx+dx].groundTextureIndex;
                weights[z,x,Mathf.Clamp(texture,0,layers.Length-1)]+=(dx==0?1-fx:fx)*(dz==0?1-fz:fz);
            }
        }
        terrainData.SetAlphamaps(0,0,weights);
        var terrainObject=Terrain.CreateTerrainGameObject(terrainData);terrainObject.name="Terrain - original 65x65 heights and tiles";
        terrainObject.transform.position=new Vector3(source.origin[0]/map.unitsPerMeter,-10,source.origin[1]/map.unitsPerMeter);
        var terrain=terrainObject.GetComponent<Terrain>();terrain.heightmapPixelError=3;terrain.basemapDistance=200;terrain.drawInstanced=true;
        var terrainMat=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Terrain URP.mat");
        if(!terrainMat){terrainMat=new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));AssetDatabase.CreateAsset(terrainMat,Root+"/Terrain URP.mat");}
        terrain.materialTemplate=terrainMat;
        EditorUtility.SetDirty(terrainData);
        BuildMapTexture(map);
    }

    static Color GroundColor(string id)
    {
        switch(id)
        {
            case "Qdrt": return new Color(.43f,.33f,.21f);
            case "Qdrr": return new Color(.34f,.27f,.17f);
            case "Qcrp": return new Color(.31f,.29f,.17f);
            case "Qcbp": return new Color(.49f,.46f,.40f);
            case "Qstp": return new Color(.54f,.52f,.45f);
            case "Qgrs": return new Color(.31f,.41f,.19f);
            case "Qgrt": return new Color(.22f,.34f,.14f);
            case "Bdrh": return new Color(.49f,.37f,.23f);
            default: throw new InvalidDataException("Unmapped terrain rawcode: "+id);
        }
    }

    static void BuildMapTexture(ArenaMap map)
    {
        const int size=256;var data=map.Layout.terrain;
        var pixels=new Color[size*size];
        for(int z=0;z<size;z++)for(int x=0;x<size;x++)
        {
            var vertex=data.vertices[Mathf.Min(data.height-1,z*(data.height-1)/size)*data.width+Mathf.Min(data.width-1,x*(data.width-1)/size)];
            pixels[z*size+x]=GroundColor(data.groundTextures[vertex.groundTextureIndex]);
        }
        foreach(var d in map.Layout.doodads)
        {
            if(ReplacementModel(d)==null)continue;
            int x=Mathf.RoundToInt((d.x-data.origin[0])/((data.width-1)*data.cellSize)*(size-1));
            int z=Mathf.RoundToInt((d.y-data.origin[1])/((data.height-1)*data.cellSize)*(size-1));
            var color=IsBarrel(d)?new Color(.47f,.23f,.07f):
                d.category=="tree"?new Color(.08f,.22f,.13f):new Color(.27f,.3f,.25f);
            for(int dx=-1;dx<=1;dx++)for(int dz=-1;dz<=1;dz++)if(x+dx>=0 && x+dx<size && z+dz>=0 && z+dz<size)pixels[(z+dz)*size+x+dx]=color;
        }
        var texture=new Texture2D(size,size,TextureFormat.RGB24,false);texture.SetPixels(pixels);texture.Apply();
        File.WriteAllBytes(Root+"/Lia39Minimap.png",texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(Root+"/Lia39Minimap.png");
    }
}
