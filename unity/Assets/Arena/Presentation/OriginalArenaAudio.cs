using System.Collections.Generic;
using Arena.Original;
using UnityEngine;

namespace Arena
{
    // Original synthesized cues. Presentation only: no random/gameplay state is consumed.
    [DisallowMultipleComponent]
    public sealed class OriginalArenaAudio : MonoBehaviour
    {
        OriginalArenaRuntime runtime;
        AudioSource source;
        AudioClip strike,spell,hurt,death,phaseCue,click;
        readonly Dictionary<int,OriginalWorldUnitView> previous=new Dictionary<int,OriginalWorldUnitView>();
        OriginalWorldSnapshot last;
        OriginalMatchPhase phase;
        bool hasPhase;
        float nextCombatSound;

        void Awake()
        {
            runtime=GetComponent<OriginalArenaRuntime>();
            source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;
            strike=Tone("Arena strike",.12f,180,55,.7f);
            spell=Tone("Arena spell",.38f,360,840,.06f);
            hurt=Tone("Arena impact",.13f,100,42,.5f);
            death=Tone("Arena death",.55f,170,35,.18f);
            phaseCue=Tone("Arena phase",.8f,520,780,.01f);
            click=Tone("Arena click",.065f,720,510,.025f);
        }
        public void Click(){if(source)source.PlayOneShot(click,.16f);}
        void Update()
        {
            var view=runtime?runtime.View:null;
            if(view==null||!view.started||view.world==null)
            {last=null;previous.Clear();hasPhase=false;return;}
            if(hasPhase&&phase!=view.phase)source.PlayOneShot(phaseCue,.22f);
            phase=view.phase;hasPhase=true;
            if(ReferenceEquals(last,view.world))return;
            int sounds=0;
            foreach(var unit in view.world.units)
            {
                if(previous.TryGetValue(unit.entityId,out var old)&&!unit.hidden&&runtime.UnitVisible(unit)&&
                    runtime.LocalUnit!=null&&Near(unit.position,runtime.LocalUnit.position)&&sounds<3&&Time.unscaledTime>=nextCombatSound)
                {
                    AudioClip cue=old.health>0&&unit.health<=0?death:
                        unit.castSequence!=old.castSequence?spell:
                        unit.attackSequence!=old.attackSequence?strike:
                        unit.entityId==runtime.LocalUnit.entityId&&unit.health<old.health?hurt:null;
                    if(cue){source.PlayOneShot(cue,cue==death?.25f:.18f);sounds++;}
                }
            }
            if(sounds>0)nextCombatSound=Time.unscaledTime+.045f;
            previous.Clear();foreach(var unit in view.world.units)previous[unit.entityId]=unit;
            last=view.world;
        }
        static bool Near(OriginalPoint a,OriginalPoint b)
        {double x=a.x-b.x,y=a.y-b.y;return x*x+y*y<1400*1400;}
        static AudioClip Tone(string name,float seconds,float start,float end,float noise)
        {
            const int rate=22050;var data=new float[Mathf.CeilToInt(seconds*rate)];
            uint seed=173;double angle=0;
            for(int i=0;i<data.Length;i++)
            {
                float progress=(float)i/data.Length;
                angle+=2*System.Math.PI*Mathf.Lerp(start,end,progress)/rate;
                seed^=seed<<13;seed^=seed>>17;seed^=seed<<5;
                float random=(seed&65535)/32767.5f-1;
                float envelope=Mathf.Min(1,i/(rate*.008f))*Mathf.Pow(1-progress,2);
                data[i]=((float)System.Math.Sin(angle)*(1-noise)+random*noise)*envelope*.65f;
            }
            var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        void OnDestroy()
        {
            foreach(var clip in new[]{strike,spell,hurt,death,phaseCue,click})if(clip)Destroy(clip);
            if(source)Destroy(source);
        }
    }
}
