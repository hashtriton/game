using UnityEngine;
using UnityEngine.UI;

namespace Arena
{
    public enum OriginalHudSymbol { Copies,Shield,Blade,Flame,Arrow,Sun,Attributes }

    // Small vector marks remain sharp at every canvas scale; no imported artwork.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class OriginalHudGlyph : MaskableGraphic
    {
        OriginalHudSymbol symbol;
        public void Set(OriginalHudSymbol value)
        {if(symbol!=value){symbol=value;SetVerticesDirty();}}
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();var rect=rectTransform.rect;
            float scale=Mathf.Min(rect.width,rect.height)*.45f;
            Vector2 Point(float x,float y)=>rect.center+new Vector2(x,y)*scale;
            void Line(float x1,float y1,float x2,float y2,float thickness=1.7f)
            {
                var a=Point(x1,y1);var b=Point(x2,y2);var delta=b-a;
                if(delta.sqrMagnitude<.001f)return;
                var n=new Vector2(-delta.y,delta.x).normalized*thickness*.5f;
                int i=mesh.currentVertCount;
                mesh.AddVert(a-n,color,Vector2.zero);mesh.AddVert(a+n,color,Vector2.zero);
                mesh.AddVert(b+n,color,Vector2.zero);mesh.AddVert(b-n,color,Vector2.zero);
                mesh.AddTriangle(i,i+1,i+2);mesh.AddTriangle(i,i+2,i+3);
            }
            void Diamond(float x,float y,float size)
            {Line(x,y+size,x+size,y);Line(x+size,y,x,y-size);Line(x,y-size,x-size,y);Line(x-size,y,x,y+size);}
            switch(symbol)
            {
                case OriginalHudSymbol.Copies:Diamond(-.3f,.2f,.55f);Diamond(.3f,-.2f,.55f);break;
                case OriginalHudSymbol.Shield:
                    Line(-.7f,.7f,.7f,.7f);Line(.7f,.7f,.55f,-.3f);Line(.55f,-.3f,0,-.85f);
                    Line(0,-.85f,-.55f,-.3f);Line(-.55f,-.3f,-.7f,.7f);Line(0,.45f,0,-.5f);break;
                case OriginalHudSymbol.Blade:
                    Line(-.75f,-.75f,.7f,.7f,3);Line(.7f,.7f,.65f,.25f);Line(.7f,.7f,.25f,.65f);
                    Line(-.65f,-.1f,-.1f,-.65f,2.5f);break;
                case OriginalHudSymbol.Arrow:
                    Line(-.7f,-.7f,.7f,.7f,2.2f);Line(.7f,.7f,.7f,.05f);Line(.7f,.7f,.05f,.7f);
                    Line(-.7f,-.7f,-.75f,-.2f);Line(-.7f,-.7f,-.2f,-.75f);break;
                case OriginalHudSymbol.Flame:
                    Line(0,1,-.65f,-.1f);Line(-.65f,-.1f,-.4f,-.7f);Line(-.4f,-.7f,.35f,-.7f);
                    Line(.35f,-.7f,.65f,-.15f);Line(.65f,-.15f,.35f,.5f);Line(.35f,.5f,.12f,.1f);
                    Line(.12f,.1f,0,1);Line(-.12f,-.5f,.05f,-.05f);break;
                case OriginalHudSymbol.Sun:
                    Diamond(0,0,.42f);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Line(Mathf.Cos(a)*.65f,Mathf.Sin(a)*.65f,Mathf.Cos(a),Mathf.Sin(a));}break;
                case OriginalHudSymbol.Attributes:
                    Line(-.6f,-.7f,-.6f,.1f,3);Line(0,-.7f,0,.7f,3);Line(.6f,-.7f,.6f,.4f,3);break;
            }
        }
    }
}
