using System;
using System.IO;
using System.Linq;
using Arena;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>Reproducible authoring in the live Editor; touches only Arena generated content.</summary>
public static class ArenaBuilder
{
    const string Generated = "Assets/Arena/Generated";
    const string Castle = "Assets/ThirdParty/KenneyCastle/";
    const string Characters = "Assets/ThirdParty/Quaternius/";
    public const string ScenePath = LiaMapBuilder.ScenePath;

    public static void Build() => LiaMapBuilder.Build();

    public static void ConfigureRendering()
    {
        var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Generated+"/ArenaURP.asset");
        if(!pipeline)
        {
            var renderer=ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(renderer,Generated+"/ArenaRenderer.asset");
            pipeline=UniversalRenderPipelineAsset.Create(renderer);
            AssetDatabase.CreateAsset(pipeline,Generated+"/ArenaURP.asset");
        }
        pipeline.msaaSampleCount=4;pipeline.shadowDistance=70;
        pipeline.supportsCameraDepthTexture=true;
        GraphicsSettings.defaultRenderPipeline=pipeline;
        int current=QualitySettings.GetQualityLevel();
        for(int i=0;i<QualitySettings.names.Length;i++)
        {
            QualitySettings.SetQualityLevel(i,false);QualitySettings.renderPipeline=pipeline;
        }
        QualitySettings.SetQualityLevel(current,false);
        EditorUtility.SetDirty(pipeline);
    }

    public static Material Material(string name,Color color,string texture=null,float metallic=0,bool unlit=false)
    {
        string path=Generated+"/"+name+".mat";
        var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        var shader=Shader.Find(unlit?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit");
        if(!shader) throw new InvalidOperationException("URP shader not available.");
        if(!mat){mat=new Material(shader);AssetDatabase.CreateAsset(mat,path);}
        mat.shader=shader;mat.SetColor("_BaseColor",color);
        if(texture!=null)
        {
            var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(texture);
            if(!tex)throw new FileNotFoundException(texture);
            mat.SetTexture("_BaseMap",tex);
        }
        if(!unlit){mat.SetFloat("_Metallic",metallic);mat.SetFloat("_Smoothness",.18f);}
        mat.enableInstancing=true;EditorUtility.SetDirty(mat);return mat;
    }

    public static GameObject CharacterPrefab(string name,Material body,Material weapon,string idle,string run,string attack)
    {
        string path=Characters+name+".fbx";
        var importer=(ModelImporter)AssetImporter.GetAtPath(path);
        if(importer==null)throw new FileNotFoundException(path);
        if(importer.animationType!=ModelImporterAnimationType.Legacy || !importer.importAnimation)
        {
            importer.animationType=ModelImporterAnimationType.Legacy;
            importer.importAnimation=true;importer.SaveAndReimport();
        }
        var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var instance=UnityEngine.Object.Instantiate(model);
        instance.name=name;
        foreach(var renderer in instance.GetComponentsInChildren<Renderer>())
            renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>m && m.name.ToLowerInvariant().Contains("sword")?weapon:body).ToArray();
        foreach(var animator in instance.GetComponentsInChildren<Animator>())UnityEngine.Object.DestroyImmediate(animator);
        var animation=instance.GetComponent<Animation>()??instance.AddComponent<Animation>();
        var clips=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
        string[] aliases={"Idle","Run","Attack","Death"};string[] sources={idle,run,attack,"Death"};
        for(int i=0;i<aliases.Length;i++)
        {
            var original=clips.FirstOrDefault(c=>c.name.Split('|').Last()==sources[i]);
            if(!original)throw new InvalidOperationException(name+" missing animation "+sources[i]);
            string clipPath=Generated+"/"+name+"_"+aliases[i]+".anim";
            var copy=AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if(!copy){copy=UnityEngine.Object.Instantiate(original);AssetDatabase.CreateAsset(copy,clipPath);}
            else EditorUtility.CopySerialized(original,copy);
            copy.name=aliases[i];copy.legacy=true;copy.wrapMode=i<2?WrapMode.Loop:i==3?WrapMode.ClampForever:WrapMode.Once;
            animation.AddClip(copy,aliases[i]);EditorUtility.SetDirty(copy);
            if(i==0)animation.clip=copy;
        }
        animation.playAutomatically=true;
        animation.cullingType=AnimationCullingType.AlwaysAnimate;
        var prefab=PrefabUtility.SaveAsPrefabAsset(instance,Generated+"/"+name+".prefab");
        UnityEngine.Object.DestroyImmediate(instance);
        return prefab;
    }

    public static GameObject Prefab(string path,Transform parent,Material material)
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if(!prefab)throw new FileNotFoundException(path);
        var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.SetParent(parent);SetMaterials(go,material);return go;
    }
    public static void SetMaterials(GameObject go,Material material)
    {
        foreach(var r in go.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(_=>material).ToArray();
    }
    public static void Fit(GameObject go,Vector3 position,float size,bool width)
    {
        var renderers=go.GetComponentsInChildren<Renderer>();
        if(renderers.Length==0)throw new InvalidOperationException("No renderer: "+go.name);
        var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
        float dimension=width?Mathf.Max(bounds.size.x,bounds.size.z):bounds.size.y;
        float scale=size/Mathf.Max(.001f,dimension);
        go.transform.localScale*=scale;
        go.transform.position=position-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z)*scale;
    }
    static void Primitive(string name,PrimitiveType type,Vector3 position,Vector3 scale,Material material,Transform parent)
    {
        var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent);
        go.transform.position=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;
        UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
    }
    static Vector3 Radial(float radius,float degrees)=>new Vector3(Mathf.Sin(degrees*Mathf.Deg2Rad)*radius,0,Mathf.Cos(degrees*Mathf.Deg2Rad)*radius);
    static void Ring(string name,float radius,float width,float y,Material mat,Transform parent)
    {
        var go=new GameObject(name);go.transform.SetParent(parent);
        var line=go.AddComponent<LineRenderer>();line.sharedMaterial=mat;line.useWorldSpace=false;
        line.loop=true;line.widthMultiplier=width;line.positionCount=128;line.alignment=LineAlignment.TransformZ;
        go.transform.rotation=Quaternion.Euler(90,0,0);go.transform.position=new Vector3(0,y,0);
        for(int i=0;i<128;i++){float a=i*Mathf.PI*2/128;line.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0));}
    }
}
