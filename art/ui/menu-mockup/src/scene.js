/* ============ grim night arena: three.js, procedural geometry + generated PBR textures, HDR post ============ */
function initGL(onReady){
  if(!window.THREE||/[?&]nogl/.test(location.search))return null;
  var T3=THREE,glc=document.getElementById('gl'),R;
  try{R=new T3.WebGLRenderer({canvas:glc,antialias:false,powerPreference:'high-performance'})}catch(e){return null}
  if(!R.getContext())return null;
  var lowq=/[?&]q=low/.test(location.search),res=lowq?.5:1,rand=mulberry(2024);
  var aniso=Math.min(8,R.capabilities.getMaxAnisotropy());
  var hdr=R.capabilities.isWebGL2&&!/[?&]nopost/.test(location.search)&&!!(R.extensions.get('EXT_color_buffer_float')||R.extensions.get('EXT_color_buffer_half_float'));
  function RW(){return Math.max(8,Math.round(W*res))}
  function RH(){return Math.max(8,Math.round(H*res))}
  R.setPixelRatio(1);R.setSize(RW(),RH(),false);R.outputColorSpace=T3.SRGBColorSpace;
  if(!hdr){R.toneMapping=T3.ACESFilmicToneMapping;R.toneMappingExposure=1.5}
  R.shadowMap.enabled=true;R.shadowMap.autoUpdate=false;R.shadowMap.type=T3.PCFSoftShadowMap;
  var scene=new T3.Scene(),FOGC=0x131d26;
  scene.fog=new T3.FogExp2(FOGC,.0078);
  var cam=new T3.PerspectiveCamera(27.6,W/H,1,1200);

  /* ---------- post: HDR target, bloom, grade, grain ---------- */
  var post=null,triGeo=new T3.BufferGeometry(),triCam=new T3.OrthographicCamera(-1,1,1,-1,0,1);
  triGeo.setAttribute('position',new T3.BufferAttribute(new Float32Array([-1,-1,0,3,-1,0,-1,3,0]),3));
  triGeo.setAttribute('uv',new T3.BufferAttribute(new Float32Array([0,0,2,0,0,2]),2));
  var PVS='varying vec2 vUv;void main(){vUv=uv;gl_Position=vec4(position.xy,0.,1.);}';
  var FS_BRIGHT='uniform sampler2D tex;uniform float thr;varying vec2 vUv;void main(){vec3 c=texture2D(tex,vUv).rgb;float l=max(c.r,max(c.g,c.b));float k=smoothstep(thr,thr*2.4,l);gl_FragColor=vec4(c*k,1.);}';
  var FS_BLUR='uniform sampler2D tex;uniform vec2 dir;varying vec2 vUv;void main(){vec3 c=texture2D(tex,vUv).rgb*.227027;vec2 o1=dir*1.3846,o2=dir*3.2308;c+=(texture2D(tex,vUv+o1).rgb+texture2D(tex,vUv-o1).rgb)*.316216;c+=(texture2D(tex,vUv+o2).rgb+texture2D(tex,vUv-o2).rgb)*.07027;gl_FragColor=vec4(c,1.);}';
  var FS_COMP=[
    'uniform sampler2D tS,t1,t2,t3;uniform float exposure,bloom,time,aspect,grain;varying vec2 vUv;',
    'float hash(vec2 p){return fract(sin(dot(p,vec2(12.9898,78.233)))*43758.5453);}',
    'vec3 aces(vec3 x){return clamp((x*(2.51*x+.03))/(x*(2.43*x+.59)+.14),0.,1.);}',
    'void main(){',
    ' vec2 cc=(vUv-.5)*vec2(aspect,1.);float r2=dot(cc,cc);vec2 ca=(vUv-.5)*r2*.0022;',
    ' vec3 c=vec3(texture2D(tS,vUv+ca).r,texture2D(tS,vUv).g,texture2D(tS,vUv-ca).b);',
    ' vec3 b=texture2D(t1,vUv).rgb*.5+texture2D(t2,vUv).rgb*.65+texture2D(t3,vUv).rgb*.8;',
    ' c+=b*bloom;c*=exposure;',
    ' float l=dot(c,vec3(.2126,.7152,.0722));',
    ' c*=mix(vec3(.86,1.0,1.12),vec3(1.14,1.0,.82),smoothstep(.015,.85,l));',
    ' c=mix(vec3(l),c,.82);c=aces(c);',
    ' c*=mix(.46,1.,smoothstep(1.0,.28,sqrt(r2)*1.05));',
    ' c=pow(c,vec3(1./2.2));',
    ' c+=(hash(vUv*vec2(1920.,1080.)+fract(time)*61.)-.5)*grain;',
    ' gl_FragColor=vec4(c,1.);}'].join('\n');
  function mkPass(fs,u){var m=new T3.ShaderMaterial({vertexShader:PVS,fragmentShader:fs,uniforms:u,depthTest:false,depthWrite:false}),me=new T3.Mesh(triGeo,m),sc=new T3.Scene();me.frustumCulled=false;sc.add(me);return{m:m,s:sc}}
  function mkRT(w,h,ms,dep){return new T3.WebGLRenderTarget(Math.max(2,w|0),Math.max(2,h|0),{type:T3.HalfFloatType,minFilter:T3.LinearFilter,magFilter:T3.LinearFilter,depthBuffer:!!dep,samples:ms||0})}
  function setupPost(){
    if(!hdr)return;
    if(post)Object.keys(post.rt).forEach(function(k){post.rt[k].dispose()});
    var w=RW(),h=RH(),rt={scene:mkRT(w,h,lowq?0:4,true),br:mkRT(w>>1,h>>1),a1:mkRT(w>>2,h>>2),b1:mkRT(w>>2,h>>2),a2:mkRT(w>>3,h>>3),b2:mkRT(w>>3,h>>3),a3:mkRT(w>>4,h>>4),b3:mkRT(w>>4,h>>4)};
    if(!post)post={
      bright:mkPass(FS_BRIGHT,{tex:{value:null},thr:{value:1.35}}),
      blur:mkPass(FS_BLUR,{tex:{value:null},dir:{value:new T3.Vector2()}}),
      comp:mkPass(FS_COMP,{tS:{value:null},t1:{value:null},t2:{value:null},t3:{value:null},exposure:{value:1.9},bloom:{value:.55},time:{value:0},aspect:{value:W/H},grain:{value:.04}})
    };
    post.rt=rt;var u=post.comp.m.uniforms;u.tS.value=rt.scene.texture;u.t1.value=rt.b1.texture;u.t2.value=rt.b2.texture;u.t3.value=rt.b3.texture;
  }
  function blurTo(src,a,b){
    var u=post.blur.m.uniforms;
    u.tex.value=src.texture;u.dir.value.set(1/a.width,0);R.setRenderTarget(a);R.render(post.blur.s,triCam);
    u.tex.value=a.texture;u.dir.value.set(0,1/b.height);R.setRenderTarget(b);R.render(post.blur.s,triCam);
  }
  function renderFrame(tsum){
    if(!post){R.render(scene,cam);return}
    var rt=post.rt;
    R.setRenderTarget(rt.scene);R.render(scene,cam);
    post.bright.m.uniforms.tex.value=rt.scene.texture;R.setRenderTarget(rt.br);R.render(post.bright.s,triCam);
    blurTo(rt.br,rt.a1,rt.b1);blurTo(rt.b1,rt.a2,rt.b2);blurTo(rt.b2,rt.a3,rt.b3);
    post.comp.m.uniforms.time.value=tsum;R.setRenderTarget(null);R.render(post.comp.s,triCam);
  }
  setupPost();

  /* ---------- textures (generated offline, embedded) ---------- */
  var TL=new T3.TextureLoader(),pend=0,isReady=false,failed=false;
  function done(err){if(err)failed=true;if(--pend<=0&&!isReady){isReady=true;R.shadowMap.needsUpdate=true;if(onReady)onReady(!failed)}}
  var texCache={};
  function img(name,srgb,rep,fresh){
    if(!fresh&&texCache[name])return texCache[name];
    pend++;var t=TL.load(TEXDATA[name],function(){done(false)},undefined,function(){done(true)});
    t.colorSpace=srgb?T3.SRGBColorSpace:T3.NoColorSpace;t.anisotropy=aniso;
    if(rep)t.wrapS=t.wrapT=T3.RepeatWrapping;if(!fresh)texCache[name]=t;return t;
  }
  function cvs2(w,h){var c=document.createElement('canvas');c.width=w;c.height=h;return c}
  function ctex(cv,srgb){var t=new T3.CanvasTexture(cv);if(srgb!==false)t.colorSpace=T3.SRGBColorSpace;t.anisotropy=aniso;return t}
  var macroTex=img('macro',false,true);
  function macroize(m,sc,amt,mode){
    var fn=mode==='w'
      ?'float mc=texture2D(tMacro,vec2(vWp.x+vWp.z,vWp.y)*'+sc.toFixed(4)+').r;diffuseColor.rgb*=mix('+(1-amt).toFixed(3)+','+(1+amt).toFixed(3)+',mc);diffuseColor.rgb*=mix(.42,1.,smoothstep(0.,3.4,vWp.y));'
      :'float mc=texture2D(tMacro,vWp.xz*'+sc.toFixed(4)+').r;diffuseColor.rgb*=mix('+(1-amt).toFixed(3)+','+(1+amt).toFixed(3)+',mc);';
    m.onBeforeCompile=function(sh){
      sh.uniforms.tMacro={value:macroTex};
      sh.vertexShader=sh.vertexShader.replace('#include <common>','#include <common>\nvarying vec3 vWp;')
        .replace('#include <begin_vertex>','#include <begin_vertex>\nvec4 wp4=vec4(transformed,1.);\n#ifdef USE_INSTANCING\nwp4=instanceMatrix*wp4;\n#endif\nvWp=(modelMatrix*wp4).xyz;');
      sh.fragmentShader=sh.fragmentShader.replace('#include <common>','#include <common>\nvarying vec3 vWp;uniform sampler2D tMacro;')
        .replace('#include <map_fragment>','#include <map_fragment>\n'+fn);
    };
    m.customProgramCacheKey=function(){return 'macro'+mode+sc+amt};
    return m;
  }
  function pbr(n,o){
    o=o||{};var ns=o.ns==null?1:o.ns;delete o.ns;
    var m=new T3.MeshStandardMaterial(Object.assign({map:img(n+'_a',true,true),normalMap:img(n+'_n',false,true),roughnessMap:img(n+'_r',false,true),roughness:1,metalness:0},o));
    m.normalScale=new T3.Vector2(ns,ns);return m;
  }
  var MT={
    stone:macroize(pbr('stone',{ns:1.3}),.011,.28,'w'),
    ground:macroize(pbr('ground',{ns:1.1}),.012,.32,'g'),
    cobble:macroize(pbr('cobble',{ns:1.2,color:0xd9d4cc}),.016,.25,'g'),
    wood:pbr('wood',{ns:1.1}),
    crate:pbr('crate',{ns:1.0}),
    barrel:pbr('barrel',{ns:1.0}),
    bark:pbr('bark',{ns:1.2}),
    iron:pbr('rust',{metalness:.7,roughness:.8,ns:1.0,color:0xb8b8b8}),
    lid:new T3.MeshStandardMaterial({color:0x1d140d,roughness:.95}),
    dark:new T3.MeshStandardMaterial({color:0x090705,roughness:1}),
    bone:new T3.MeshStandardMaterial({color:0xb9ad94,roughness:.8}),
    outer:macroize(pbr('ground',{ns:.9,color:0x6f7068}),.004,.3,'g')
  };
  MT.barkD=MT.bark.clone();MT.barkD.side=T3.DoubleSide;MT.ironD=MT.iron.clone();MT.ironD.side=T3.DoubleSide;
  function clothMat(col,o){
    var m=new T3.MeshStandardMaterial(Object.assign({map:cloth_a,normalMap:cloth_n,alphaMap:cloth_al,alphaTest:.5,side:T3.DoubleSide,roughness:.95,color:col},o||{}));return m;
  }
  var cloth_a=img('cloth_a',true,true),cloth_n=img('cloth_n',false,true),cloth_al=img('cloth_alpha',false,true);

  /* procedural canvas sprites */
  function makeGlow(){var cv=cvs2(128,128),c=cv.getContext('2d'),g=c.createRadialGradient(64,64,0,64,64,64);g.addColorStop(0,'rgba(255,255,255,1)');g.addColorStop(.18,'rgba(255,255,255,.55)');g.addColorStop(.5,'rgba(255,255,255,.14)');g.addColorStop(1,'rgba(255,255,255,0)');c.fillStyle=g;c.fillRect(0,0,128,128);return ctex(cv)}
  function makeEye(){
    var cv=cvs2(128,64),c=cv.getContext('2d');
    c.save();c.beginPath();c.moveTo(4,34);c.quadraticCurveTo(64,-2,124,34);c.quadraticCurveTo(64,58,4,34);c.closePath();c.clip();
    var g=c.createRadialGradient(64,34,1,64,34,62);g.addColorStop(0,'#fff8de');g.addColorStop(.3,'#ffc060');g.addColorStop(.7,'#d9531c');g.addColorStop(1,'#5a1006');c.fillStyle=g;c.fillRect(0,0,128,64);
    c.fillStyle='#120300';c.beginPath();c.moveTo(64,6);c.quadraticCurveTo(70,34,64,60);c.quadraticCurveTo(58,34,64,6);c.fill();c.restore();return ctex(cv);
  }
  function makeSoft(){var cv=cvs2(128,128),c=cv.getContext('2d'),g=c.createRadialGradient(64,64,0,64,64,64);g.addColorStop(0,'rgba(0,0,0,.8)');g.addColorStop(.55,'rgba(0,0,0,.35)');g.addColorStop(1,'rgba(0,0,0,0)');c.fillStyle=g;c.fillRect(0,0,128,128);return ctex(cv)}
  function makeEmber(){var cv=cvs2(32,32),c=cv.getContext('2d'),g=c.createRadialGradient(16,16,0,16,16,16);g.addColorStop(0,'rgba(255,230,170,1)');g.addColorStop(.3,'rgba(255,150,60,.8)');g.addColorStop(1,'rgba(255,90,20,0)');c.fillStyle=g;c.fillRect(0,0,32,32);return ctex(cv)}
  var glowTex=makeGlow(),eyeTex=makeEye(),softTex=makeSoft(),emberTex=makeEmber();
  var rippleTex=null,mistTex=img('mist',true,true),bloodTex=img('blood',true,false),scorchTex=img('scorch',true,false),pineTex=img('pine',true,false);

  /* ---------- helpers ---------- */
  function hash3(x,y,z){var s=Math.sin(x*127.1+y*311.7+z*74.7)*43758.5453;return s-Math.floor(s)}
  function vn3(x,y,z){
    var xi=Math.floor(x),yi=Math.floor(y),zi=Math.floor(z),xf=x-xi,yf=y-yi,zf=z-zi;xf=xf*xf*(3-2*xf);yf=yf*yf*(3-2*yf);zf=zf*zf*(3-2*zf);
    function L(a,b,t){return a+(b-a)*t}
    return L(L(L(hash3(xi,yi,zi),hash3(xi+1,yi,zi),xf),L(hash3(xi,yi+1,zi),hash3(xi+1,yi+1,zi),xf),yf),L(L(hash3(xi,yi,zi+1),hash3(xi+1,yi,zi+1),xf),L(hash3(xi,yi+1,zi+1),hash3(xi+1,yi+1,zi+1),xf),yf),zf);
  }
  function roughen(g,amp,fq){
    var p=g.attributes.position;
    for(var i=0;i<p.count;i++){var x=p.getX(i),y=p.getY(i),z=p.getZ(i);
      p.setXYZ(i,x+(vn3(x*fq+11,y*fq,z*fq)-.5)*2*amp,y+(vn3(x*fq,y*fq+37,z*fq)-.5)*2*amp,z+(vn3(x*fq,y*fq,z*fq+71)-.5)*2*amp)}
    g.computeVertexNormals();return g;
  }
  function scaleUV(g,su,sv,ou,ov){var uv=g.attributes.uv;for(var i=0;i<uv.count;i++)uv.setXY(i,uv.getX(i)*su+(ou||0),uv.getY(i)*sv+(ov||0))}
  function sbox(w,h,d,unit,amp){
    var sw=Math.min(10,Math.max(1,Math.round(w/1.1))),sh=Math.min(10,Math.max(1,Math.round(h/1.1))),sd=Math.min(6,Math.max(1,Math.round(d/1.1)));
    var g=new T3.BoxGeometry(w,h,d,sw,sh,sd),uv=g.attributes.uv,cnt=[(sd+1)*(sh+1),(sd+1)*(sh+1),(sw+1)*(sd+1),(sw+1)*(sd+1),(sw+1)*(sh+1),(sw+1)*(sh+1)],dims=[[d,h],[d,h],[w,d],[w,d],[w,h],[w,h]],k=0;
    for(var f=0;f<6;f++){var ox=rand()*4,oy=rand()*4;for(var i=0;i<cnt[f];i++,k++)uv.setXY(k,uv.getX(k)*dims[f][0]/unit+ox,uv.getY(k)*dims[f][1]/unit+oy)}
    if(amp)roughen(g,amp,.8);return g;
  }
  function put(o,x,y,z,cast,rec){o.position.set(x,y,z);o.castShadow=cast!==false;o.receiveShadow=rec!==false;scene.add(o);return o}
  function solid(w,h,d,x,y,z,mat,unit,ry,amp){var o=put(new T3.Mesh(sbox(w,h,d,unit||4,amp==null?.05:amp),mat||MT.stone),x,y+h/2,z);if(ry)o.rotation.y=ry;return o}
  function gbox(par,w,h,d,x,y,z,mat,rx,ry,rz){var o=new T3.Mesh(new T3.BoxGeometry(w,h,d),mat);o.position.set(x,y+h/2,z);if(rx||ry||rz)o.rotation.set(rx||0,ry||0,rz||0);o.castShadow=o.receiveShadow=true;par.add(o);return o}

  /* ---------- lights ---------- */
  var moon=new T3.DirectionalLight(0xa4b8dc,6.5);moon.position.set(-75,42,18);moon.target.position.set(0,0,22);
  moon.castShadow=true;var smz=lowq?1024:2048;moon.shadow.mapSize.set(smz,smz);var sc=moon.shadow.camera;sc.left=-52;sc.right=52;sc.top=52;sc.bottom=-52;sc.near=10;sc.far=240;moon.shadow.bias=-.0005;moon.shadow.normalBias=.07;moon.shadow.radius=3;
  scene.add(moon,moon.target);
  var hemi=new T3.HemisphereLight(0x3d5675,0x2a2218,.7);scene.add(hemi);
  var rim=new T3.DirectionalLight(0x3f6a86,.5);rim.position.set(50,34,120);scene.add(rim);
  var lights=[];
  function plight(x,y,z,colr,inten,dist,flick){var l=new T3.PointLight(colr,inten,dist,2);l.position.set(x,y,z);scene.add(l);lights.push({l:l,base:inten,f:flick||0,ph:rand()*9});return l}

  /* ---------- sky ---------- */
  var skyU={time:{value:0}};
  var FS_SKY=[
    'varying vec3 vD;uniform float time;',
    'float h2(vec2 p){return fract(sin(dot(p,vec2(127.1,311.7)))*43758.5453);}',
    'float vn(vec2 p){vec2 i=floor(p),f=fract(p);f=f*f*(3.-2.*f);return mix(mix(h2(i),h2(i+vec2(1.,0.)),f.x),mix(h2(i+vec2(0.,1.)),h2(i+vec2(1.,1.)),f.x),f.y);}',
    'float fbm(vec2 p){float s=0.,a=.5;for(int i=0;i<5;i++){s+=a*vn(p);p*=2.03;a*=.5;}return s;}',
    'void main(){vec3 d=normalize(vD);float h=max(d.y,0.);',
    ' vec3 hor=vec3(.17,.075,.04),mid=vec3(.022,.034,.05),top=vec3(.004,.008,.015);',
    ' vec3 c=mix(hor,mid,smoothstep(0.,.16,h));c=mix(c,top,smoothstep(.1,.8,h));',
    ' float glow=pow(max(0.,d.z),2.2)*(1.-smoothstep(0.,.32,h));c+=vec3(.5,.18,.06)*glow*.5;',
    ' vec2 p=d.xz/(h+.2)*1.3+vec2(time*.01,0.);float m=smoothstep(.4,.78,fbm(p))*smoothstep(0.,.22,h);',
    ' c=mix(c,vec3(.016,.02,.028)+vec3(.1,.04,.015)*glow,m*.8);gl_FragColor=vec4(c,1.);}'].join('\n');
  var sky=new T3.Mesh(new T3.SphereGeometry(900,24,16),new T3.ShaderMaterial({uniforms:skyU,side:T3.BackSide,depthWrite:false,fog:false,vertexShader:'varying vec3 vD;void main(){vD=normalize(position);gl_Position=projectionMatrix*modelViewMatrix*vec4(position,1.);}',fragmentShader:FS_SKY}));
  sky.renderOrder=-10;sky.frustumCulled=false;scene.add(sky);
  try{
    var skyScene=new T3.Scene(),sky2=new T3.Mesh(sky.geometry,sky.material);skyScene.add(sky2);
    var pmrem=new T3.PMREMGenerator(R);scene.environment=pmrem.fromScene(skyScene,0,1,2000).texture;pmrem.dispose();
  }catch(err){if(window.console)console.warn('env map skipped',err)}

  /* ---------- ground ---------- */
  var AX=30,AZ=46;
  var gOutG=new T3.PlaneGeometry(1000,1000);gOutG.rotateX(-Math.PI/2);scaleUV(gOutG,200,200);
  var gOut=new T3.Mesh(gOutG,MT.outer);gOut.position.y=-.06;gOut.receiveShadow=true;scene.add(gOut);
  var gArG=new T3.PlaneGeometry(68,56);gArG.rotateX(-Math.PI/2);scaleUV(gArG,13.6,11.2);
  var gAr=new T3.Mesh(gArG,MT.ground);gAr.position.set(0,0,24);gAr.receiveShadow=true;scene.add(gAr);
  (function(){
    var rg=new T3.CircleGeometry(12.6,64);rg.rotateX(-Math.PI/2);scaleUV(rg,5.04,5.04);
    var am=new T3.CanvasTexture((function(){
      var cv=cvs2(256,256),c=cv.getContext('2d'),id=c.createImageData(256,256),d=id.data;
      for(var i=0;i<65536;i++){var x=(i&255)-128,y=(i>>8)-128,r=Math.sqrt(x*x+y*y)/128,n=fbmA((i&255)/256,(i>>8)/256,9,9,3),a=sstep(1.0,.8,r+(n-.5)*.34);d[i*4]=d[i*4+1]=d[i*4+2]=a*255;d[i*4+3]=255}
      c.putImageData(id,0,0);return cv})());
    am.repeat.set(1/5.04,1/5.04);am.wrapS=am.wrapT=T3.RepeatWrapping;
    var pm=macroize(new T3.MeshStandardMaterial({map:MT.cobble.map,normalMap:MT.cobble.normalMap,roughnessMap:MT.cobble.roughnessMap,roughness:1,transparent:true,alphaMap:am,depthWrite:false,polygonOffset:true,polygonOffsetFactor:-1,polygonOffsetUnits:-1,color:0xd9d4cc}),.016,.25,'g');
    var ring=new T3.Mesh(rg,pm);ring.position.set(0,.02,23);ring.receiveShadow=true;ring.renderOrder=1;scene.add(ring);
  })();

  /* ---------- decals ---------- */
  function decal(map,x,z,w,d,ry,col,op,blend,y){
    var g=new T3.PlaneGeometry(w,d);g.rotateX(-Math.PI/2);
    var m=new T3.MeshBasicMaterial({map:map,transparent:true,depthWrite:false,polygonOffset:true,polygonOffsetFactor:-2,polygonOffsetUnits:-2,color:col,opacity:op,blending:blend||T3.NormalBlending,fog:blend===T3.AdditiveBlending?false:true});
    var o=new T3.Mesh(g,m);o.position.set(x,y||.04,z);o.rotation.y=ry||0;o.renderOrder=2;scene.add(o);return o;
  }
  function litDecal(map,x,z,s,ry,op,col){
    var g=new T3.PlaneGeometry(s,s);g.rotateX(-Math.PI/2);
    var m=new T3.MeshStandardMaterial({map:map,transparent:true,depthWrite:false,polygonOffset:true,polygonOffsetFactor:-2,polygonOffsetUnits:-2,opacity:op,roughness:.25,color:col||0xffffff});
    var o=new T3.Mesh(g,m);o.position.set(x,.035,z);o.rotation.y=ry;o.receiveShadow=true;o.renderOrder=2;scene.add(o);return o;
  }
  function pool(x,y,z,r,col,op,vert,ry){
    var g=new T3.PlaneGeometry(r*2,r*(vert?1.5:2)*(vert?1:1)),m=new T3.MeshBasicMaterial({map:glowTex,transparent:true,depthWrite:false,blending:T3.AdditiveBlending,color:col,opacity:op,fog:false,polygonOffset:true,polygonOffsetFactor:-3,polygonOffsetUnits:-3});
    var o=new T3.Mesh(g,m);o.position.set(x,y,z);if(vert){o.rotation.y=ry||0}else{o.rotation.x=-Math.PI/2}o.renderOrder=3;scene.add(o);return o;
  }

  /* ---------- fire ---------- */
  var flameVars=[];
  for(var fv=0;fv<6;fv++){var ft=img('flame',true,false,true);ft.repeat.set(.25,.5);flameVars.push(ft)}
  var fires=[];
  function fire(x,y,z,h,lightInt,lcol){
    var v=(rand()*6)|0;
    var mat=new T3.SpriteMaterial({map:flameVars[v],color:new T3.Color(2.0,1.25,.55),transparent:true,blending:T3.AdditiveBlending,depthWrite:false,fog:false});
    var sp=new T3.Sprite(mat);sp.center.set(.5,.04);sp.scale.set(h*.55,h*1.25,1);sp.position.set(x,y,z);scene.add(sp);
    var gm=new T3.SpriteMaterial({map:glowTex,color:new T3.Color(1.8,.75,.25),transparent:true,blending:T3.AdditiveBlending,depthWrite:false,fog:false,opacity:.3});
    var gs=new T3.Sprite(gm);gs.scale.set(h*1.9,h*1.6,1);gs.position.set(x,y+h*.35,z);scene.add(gs);
    var l=lightInt?plight(x,y+h*.6,z,lcol||0xff8a3a,lightInt,26,.4):null;
    fires.push({sp:sp,gs:gs,h:h,v:v,ph:rand()*20,l:l});
  }
  function torch(x,y,z,nx,nz,real){
    var g=new T3.CylinderGeometry(.05,.07,1.2,6);scaleUV(g,1,1);var o=new T3.Mesh(g,MT.wood);o.position.set(x,y,z);o.castShadow=true;scene.add(o);
    var br=new T3.Mesh(new T3.BoxGeometry(.5*(nz?1:.2),.08,.5*(nx?1:.2)),MT.iron);br.position.set(x-nx*.12,y-.2,z-nz*.12);scene.add(br);
    var wrap=new T3.Mesh(new T3.CylinderGeometry(.11,.07,.28,7),MT.dark);wrap.position.set(x,y+.55,z);scene.add(wrap);
    fire(x,y+.62,z,2.0,real?110:0);
    pool(x+nx*.06,y+.3,z+nz*.06,3.3,0xff7a2a,.18,true,nx?Math.PI/2*(nx>0?1:-1):(nz>0?0:Math.PI));
    pool(x+nx*2.2,.05,z+nz*2.2,4.4,0xff7a2a,.28,false);
  }
  function brazier(x,y,z,s,li){
    var post=new T3.Mesh(new T3.CylinderGeometry(.09*s,.14*s,y,7),MT.iron);post.position.set(x,y/2,z);post.castShadow=true;scene.add(post);
    var bowl=new T3.Mesh(new T3.CylinderGeometry(.55*s,.3*s,.42*s,10,1,true),MT.ironD);bowl.position.set(x,y+.2*s,z);bowl.castShadow=true;scene.add(bowl);
    var coal=new T3.Mesh(new T3.CircleGeometry(.5*s,10),new T3.MeshBasicMaterial({color:new T3.Color(2.2,.6,.15)}));coal.rotation.x=-Math.PI/2;coal.position.set(x,y+.36*s,z);scene.add(coal);
    fire(x,y+.4*s,z,2.3*s,li);
    pool(x,.05,z,6.5*s,0xff7a2a,.42,false);
  }

  /* ---------- walls and towers ---------- */
  function merlon(x,y,z,w,d,ry){
    var q=rand();if(q<.1)return;var h=q<.2?.7:1.2;
    solid(w,h,d,x,y,z,MT.stone,2.4,ry,.025);
  }
  function wallRun(axis,fixed,from,to){
    var len=to-from,n=Math.max(1,Math.round(len/6)),seg=len/n;
    for(var k=0;k<n;k++){
      var a=from+seg*k,b=a+seg,mid=(a+b)/2,hh=6.1+rand()*.5,dmg=rand()<.22?1.1+rand()*.8:0;
      if(axis==='x'){
        solid(seg+.04,hh-dmg,2.4,mid,-.4,fixed,MT.stone,4,0,.07);solid(seg+.1,.4,2.8,mid,hh-dmg-.4,fixed,MT.stone,4,0,.05);solid(seg+.3,1.1,3.2,mid,-.5,fixed,MT.stone,4,0,.08);
        var cnt=Math.round(seg/2.6);if(!dmg)for(var m=0;m<cnt;m++)merlon(a+(m+.5)*seg/cnt,hh,fixed-.85,1.3,.9,0);
      }else{
        solid(2.4,hh-dmg,seg+.04,fixed,-.4,mid,MT.stone,4,0,.07);solid(2.8,.4,seg+.1,fixed,hh-dmg-.4,mid,MT.stone,4,0,.05);solid(3.2,1.1,seg+.3,fixed,-.5,mid,MT.stone,4,0,.08);
        var cnt2=Math.round(seg/2.6),ix=fixed+(fixed<0?1.05:-1.05);if(!dmg)for(var m2=0;m2<cnt2;m2++)merlon(ix,hh,a+(m2+.5)*seg/cnt2,.9,1.3,0);
      }
      if(dmg){ // collapsed section: rubble at the foot
        for(var r=0;r<7;r++){var rx=axis==='x'?mid+(rand()-.5)*seg:fixed+(fixed<0?1:-1)*(1.6+rand()*2),rz=axis==='x'?fixed-1.6-rand()*2:mid+(rand()-.5)*seg;
          var s=.3+rand()*.7;solid(s*1.4,s,s*1.2,rx,0,rz,MT.stone,2,rand()*3,.08).rotation.set(rand()*.5,rand()*3,rand()*.5)}
      }
    }
  }
  wallRun('x',AZ+1.2,-AX-1.2,-8.8);wallRun('x',AZ+1.2,8.8,AX+1.2);
  wallRun('z',-AX-1.2,0,14.2);wallRun('z',-AX-1.2,31.8,AZ);wallRun('z',AX+1.2,0,14.2);wallRun('z',AX+1.2,31.8,AZ);
  /* buttresses on the inner faces */
  [-22,-14,14,22].forEach(function(x){solid(1.6,4.6,1.5,x,0,AZ-.9,MT.stone,3,0,.06);solid(1.9,.3,1.8,x,4.6,AZ-.9,MT.stone,3,0,.04)});
  [6,36].forEach(function(z){solid(1.5,4.6,1.6,-AX+.9,0,z,MT.stone,3,0,.06);solid(1.8,.3,1.9,-AX+.9,4.6,z,MT.stone,3,0,.04);solid(1.5,4.6,1.6,AX-.9,0,z,MT.stone,3,0,.06);solid(1.8,.3,1.9,AX-.9,4.6,z,MT.stone,3,0,.04)});
  var slitMat=new T3.MeshBasicMaterial({color:new T3.Color(1.5,.72,.26),fog:true});
  [[-6.9,AZ+1.2,0],[6.9,AZ+1.2,0],[-AX-1.2,16.2,1],[-AX-1.2,29.8,1],[AX+1.2,16.2,1],[AX+1.2,29.8,1]].forEach(function(g){
    solid(3.8,10.8,3.8,g[0],-.4,g[1],MT.stone,4,0,.08);solid(4.4,.6,4.4,g[0],10.2,g[1],MT.stone,4,0,.05);
    for(var i=-1;i<=1;i++){merlon(g[0]+i*1.45,10.8,g[1]-1.8,1.1,.9,0);merlon(g[0]+i*1.45,10.8,g[1]+1.8,1.1,.9,0);if(i){merlon(g[0]-1.8*i,10.8,g[1],.9,1.1,0)}}
    var sl=new T3.Mesh(new T3.PlaneGeometry(.3,1.7),slitMat);sl.position.set(g[0],6.4,g[1]-1.91);scene.add(sl);
  });
  /* back gatehouse: lintel and a half-raised portcullis */
  solid(10.6,2.0,3,0,8.0,AZ+1.2,MT.stone,3,0,.06);solid(1.3,2.4,3.1,0,7.6,AZ+1.2,MT.stone,3,0,.05);
  (function(){
    var gz=AZ+.2,bars=new T3.Group();
    for(var i=-4;i<=4;i++){var b=new T3.Mesh(new T3.BoxGeometry(.2,5.2,.2),MT.iron);b.position.set(i*1.1,5.6,gz);b.castShadow=true;bars.add(b);
      var tip=new T3.Mesh(new T3.ConeGeometry(.17,.7,4),MT.iron);tip.rotation.x=Math.PI;tip.position.set(i*1.1,2.65,gz);bars.add(tip)}
    [3.6,5.6,7.4].forEach(function(y){var h=new T3.Mesh(new T3.BoxGeometry(9.8,.22,.24),MT.iron);h.position.set(0,y,gz+.02);h.castShadow=true;bars.add(h)});
    scene.add(bars);
  })();
  function cyl(rr,hh,x,z,n){
    var g=new T3.CylinderGeometry(rr*.93,rr,hh,n,8,false);scaleUV(g,rr*6.283/4,hh/4);roughen(g,.06,.7);
    put(new T3.Mesh(g,MT.stone),x,hh/2,z);
    var ring=new T3.Mesh(roughen(new T3.CylinderGeometry(rr*1.08,rr*1.08,.6,n,2),.04,.8),MT.stone);put(ring,x,hh+.3,z);
    var cnt=Math.round(rr*6.283/2.4);for(var i=0;i<cnt;i++){var a=i/cnt*6.283;merlon(x+Math.cos(a)*rr*.97,hh+.6,z+Math.sin(a)*rr*.97,1.2,.9,-a-Math.PI/2)}
    for(var s=0;s<5;s++){var a2=s/5*6.283+.4,sl=new T3.Mesh(new T3.PlaneGeometry(.3,1.7),slitMat);sl.position.set(x+Math.cos(a2)*(rr*.97),hh*.55,z+Math.sin(a2)*(rr*.97));sl.rotation.y=-a2+Math.PI/2;scene.add(sl)}
  }
  cyl(3.7,13.4,-AX-1.4,AZ+1.4,18);cyl(3.7,13.4,AX+1.4,AZ+1.4,18);cyl(3,6.5,-AX-1.4,-.4,16);cyl(3,6.5,AX+1.4,-.4,16);
  brazier(-AX-1.4,13.9,AZ+1.4,1.2,230);brazier(AX+1.4,13.9,AZ+1.4,1.2,230);
  /* wall torches */
  [[-18,AZ-.3,0,-1,1],[-10,AZ-.3,0,-1,0],[10,AZ-.3,0,-1,0],[18,AZ-.3,0,-1,1]].forEach(function(t){torch(t[0],3.6,t[1],0,-1,!!t[4])});
  [[10,0],[41,0],[2,0],[33,0]].forEach(function(t){torch(-AX+.35,3.6,t[0],1,0,!!t[1]);torch(AX-.35,3.6,t[0],-1,0,!!t[1])});

  /* ---------- tattered banners ---------- */
  function gridMesh(nx,ny,fn,mat,flip){
    var pos=new Float32Array((nx+1)*(ny+1)*3),uv=new Float32Array((nx+1)*(ny+1)*2),idx=[];
    for(var j=0;j<=ny;j++)for(var i=0;i<=nx;i++){var p=fn(i/nx,j/ny),k=j*(nx+1)+i;pos[k*3]=p[0];pos[k*3+1]=p[1];pos[k*3+2]=p[2];uv[k*2]=i/nx;uv[k*2+1]=flip?j/ny:1-j/ny}
    for(j=0;j<ny;j++)for(i=0;i<nx;i++){var a=j*(nx+1)+i,b=a+1,c=a+nx+1,d=c+1;idx.push(a,c,b,b,c,d)}
    var g=new T3.BufferGeometry();g.setAttribute('position',new T3.BufferAttribute(pos,3));g.setAttribute('uv',new T3.BufferAttribute(uv,2));g.setIndex(idx);g.computeVertexNormals();
    var m=new T3.Mesh(g,mat);m.castShadow=true;m.receiveShadow=true;scene.add(m);return m;
  }
  var waving=[];
  function hang(x,y,z,w,h,mat,face,amp){
    var m=gridMesh(8,14,function(u,v){return[(u-.5)*w,-v*h,0]},mat);m.position.set(x,y,z);m.rotation.y=face||0;
    waving.push({m:m,base:m.geometry.attributes.position.array.slice(),w:w,h:h,ph:rand()*6,amp:amp||.12});return m;
  }
  var bannerMats=[clothMat(0x4a2a22),clothMat(0x2f3a30),clothMat(0x3a3028)];
  [-22,-14,14,22].forEach(function(bx,i){var rod=new T3.Mesh(new T3.CylinderGeometry(.05,.05,1.9,6),MT.wood);rod.rotation.z=Math.PI/2;rod.position.set(bx,6.2,AZ-.42);scene.add(rod);hang(bx,6.1,AZ-.5,1.7,3.7,bannerMats[i%3],Math.PI)});
  [[-6.9,AZ-.7],[6.9,AZ-.7]].forEach(function(g,i){hang(g[0],9.8,g[1],1.5,4.2,bannerMats[i],Math.PI)});

  /* ---------- cobbled plaza, well ---------- */
  (function(){
    var pts=[[3.3,0],[3.4,.15],[3.4,1.3],[3.1,1.42],[2.55,1.34],[2.45,1.1],[2.45,.5],[0,.45]].map(function(p){return new T3.Vector2(p[0],p[1])});
    var g=new T3.LatheGeometry(pts,40);scaleUV(g,5,1.3);roughen(g,.045,1.1);put(new T3.Mesh(g,MT.stone),0,0,23);
    var wm=new T3.MeshStandardMaterial({color:0x06302d,emissive:new T3.Color(.08,.62,.54),emissiveIntensity:.45,roughness:.08,metalness:.1});
    var water=new T3.Mesh(new T3.CircleGeometry(2.46,40),wm);water.rotation.x=-Math.PI/2;water.position.set(0,1.2,23);scene.add(water);
    rippleTex=img('mist',true,true,true);var rp=new T3.MeshStandardMaterial({map:rippleTex,transparent:true,emissive:0x7fe8d0,emissiveIntensity:.6,depthWrite:false,opacity:.45,color:0xffffff});
    var rip=new T3.Mesh(new T3.CircleGeometry(2.44,40),rp);rip.rotation.x=-Math.PI/2;rip.position.set(0,1.22,23);scene.add(rip);waving.well=rip;
    var gl2=new T3.Sprite(new T3.SpriteMaterial({map:glowTex,color:new T3.Color(.2,1.3,1.1),blending:T3.AdditiveBlending,depthWrite:false,fog:false,transparent:true,opacity:.6}));gl2.scale.set(7,4.6,1);gl2.position.set(0,2.2,23);scene.add(gl2);
    pool(0,.06,23,10,0x2ee6c8,.24,false);
    plight(0,3.0,23,0x42e8cc,200,30,.12);
    /* winch frame over the well */
    gbox(scene,.28,3.4,.28,-3.1,.2,23,MT.wood);gbox(scene,.28,3.4,.28,3.1,.2,23,MT.wood);gbox(scene,6.8,.26,.26,0,3.5,23,MT.wood);
    var ch=new T3.Mesh(new T3.CylinderGeometry(.03,.03,1.8,5),MT.iron);ch.position.set(0,2.6,23);scene.add(ch);
    var bk=new T3.Mesh(new T3.CylinderGeometry(.28,.22,.4,8),MT.barrel);bk.position.set(0,1.55,23);scene.add(bk);
  })();
  brazier(-10.5,1.5,23,1.0,120);brazier(10.5,1.5,23,1.0,120);brazier(0,1.5,36,1.0,110);

  /* ---------- pens: palisades with stakes ---------- */
  var stakes=[],skullPos=[];
  [[-18,34,.75],[-17,13,-.6],[16,14,.5],[9,5.5,.15],[-9,5.5,-.15]].forEach(function(pn,pi){
    var L=4.8,Wd=2.3,V=[[-L,0],[-L*.55,-Wd],[L*.55,-Wd],[L,0],[L*.55,Wd],[-L*.55,Wd]].map(function(q){return[pn[0]+q[0]*Math.cos(pn[2])-q[1]*Math.sin(pn[2]),pn[1]+q[0]*Math.sin(pn[2])+q[1]*Math.cos(pn[2])]});
    for(var k=0;k<6;k++){
      var a=V[k],b=V[(k+1)%6],ln=Math.hypot(b[0]-a[0],b[1]-a[1]),ang=Math.atan2(b[1]-a[1],b[0]-a[0]),cnt=Math.round(ln/.42);
      for(var s=0;s<cnt;s++){var t=(s+.5)/cnt;stakes.push({x:a[0]+(b[0]-a[0])*t+(rand()-.5)*.08,z:a[1]+(b[1]-a[1])*t+(rand()-.5)*.08,h:.75+rand()*.5,lx:(rand()-.5)*.2,lz:(rand()-.5)*.2,ry:rand()*6})}
      for(var rr=0;rr<2;rr++){var rail=new T3.Mesh(new T3.CylinderGeometry(.06,.06,ln+.3,6),MT.barkD);rail.rotation.z=Math.PI/2;rail.rotation.y=-ang;rail.position.set((a[0]+b[0])/2,.55+rr*.85,(a[1]+b[1])/2);rail.rotation.order='YXZ';rail.rotation.set(0,-ang,Math.PI/2);rail.castShadow=true;scene.add(rail)}
      if(rand()<.5)skullPos.push([(a[0]+b[0])/2+(rand()-.5)*.3,(a[1]+b[1])/2,ang]);
    }
    /* trampled earth and blood inside */
    decal(softTex,pn[0],pn[1],12,8,pn[2],0x000000,.5);
  });
  (function(){
    var g=new T3.CylinderGeometry(.03,.14,2.7,7,2);g.translate(0,1.35,0);scaleUV(g,1.5,3);
    var im=new T3.InstancedMesh(g,MT.barkD,stakes.length),o=new T3.Object3D(),cc=new T3.Color();
    stakes.forEach(function(s,i){o.position.set(s.x,0,s.z);o.rotation.set(s.lx,s.ry,s.lz);o.scale.set(1,s.h,1);o.updateMatrix();im.setMatrixAt(i,o.matrix);cc.setRGB(.7+rand()*.4,.7+rand()*.3,.7+rand()*.3);im.setColorAt(i,cc)});
    im.castShadow=im.receiveShadow=true;scene.add(im);
  })();
  /* skulls on stakes and bone scatter */
  function skull(x,y,z,ry){
    var g=new T3.Group(),cr=new T3.Mesh(new T3.SphereGeometry(.17,10,8),MT.bone);cr.scale.set(1,.95,1.15);g.add(cr);
    var jw=new T3.Mesh(new T3.BoxGeometry(.18,.1,.16),MT.bone);jw.position.set(0,-.16,.07);g.add(jw);
    [-.07,.07].forEach(function(ex){var e=new T3.Mesh(new T3.SphereGeometry(.05,6,5),MT.dark);e.position.set(ex,.02,.15);g.add(e)});
    g.position.set(x,y,z);g.rotation.y=ry;g.traverse(function(m){if(m.isMesh){m.castShadow=true}});scene.add(g);
  }
  skullPos.forEach(function(s){var px=s[0]+Math.cos(s[2]+1.57)*.0,pz=s[1];var pole=new T3.Mesh(new T3.CylinderGeometry(.045,.06,2.1,6),MT.barkD);pole.position.set(px,1.05,pz);pole.castShadow=true;scene.add(pole);skull(px,2.2,pz,rand()*6)});
  [[-3.6,43.4],[3.8,43.8],[-27.2,20.4],[-27.4,26.2],[27.2,20],[27.5,26.4]].forEach(function(s){var pole=new T3.Mesh(new T3.CylinderGeometry(.05,.07,2.5,6),MT.barkD);pole.position.set(s[0],1.25,s[1]);pole.castShadow=true;scene.add(pole);skull(s[0],2.6,s[1],rand()*6)});
  (function(){
    var N=90,g=new T3.CylinderGeometry(.035,.035,.42,5);g.rotateZ(Math.PI/2);
    var im=new T3.InstancedMesh(g,MT.bone,N),o=new T3.Object3D(),cl=[[-3,41],[3,40],[-18,34],[-17,13],[16,14],[9,5.5],[-9,5.5],[0,29],[22,38],[-26,24],[26,22]];
    for(var i=0;i<N;i++){var c=cl[(rand()*cl.length)|0];o.position.set(c[0]+(rand()-.5)*6,.04,c[1]+(rand()-.5)*5);o.rotation.set((rand()-.5)*.3,rand()*6,(rand()-.5)*.3);var s=.6+rand()*1.0;o.scale.set(s,1,1);o.updateMatrix();im.setMatrixAt(i,o.matrix)}
    im.castShadow=true;im.receiveShadow=true;scene.add(im);
  })();
  /* blood and scorch */
  [[-2,42,2],[3,41,1.3],[-4,38,1.8],[-26.5,22,2],[26.5,24,2],[-17,16,1.6],[15,17,1.6],[0,33,1.6],[7,27,1.4],[-9,19,1.3]].forEach(function(b){litDecal(bloodTex,b[0],b[1],b[2]*3,rand()*6,.8)});
  [[-10.5,23,3],[10.5,23,3],[0,36,3],[-3,28,2]].forEach(function(b){decal(scorchTex,b[0],b[1],b[2]*2.2,b[2]*2.2,rand()*6,0xffffff,.85)});

  /* ---------- ruined market stalls ---------- */
  var hideMat=clothMat(0x7a4a38);
  function stall(x,z,ci,seed){
    var q=mulberry(seed),G=new T3.Group();G.position.set(x,0,z);scene.add(G);
    gbox(G,3.0,1.0,1.1,0,0,0,MT.wood);
    gbox(G,3.3,2.5,.14,0,0,.85,MT.wood,0,0,(q()-.5)*.03);
    [[-1.6,-1.15,3.0],[1.6,-1.15,q()<.35?2.0:3.0],[-1.6,.85,3.6],[1.6,.85,3.6]].forEach(function(p){gbox(G,.16,p[2],.16,p[0],0,p[1],MT.barkD,(q()-.5)*.08,0,(q()-.5)*.08)});
    var cm=clothMat([0xa07a58,0x9a4a3c,0x6f7658,0x8a7448][ci%4]);
    var can=gridMesh(14,9,function(u,v){var xx=(u-.5)*3.9,t=v;return[xx,3.6-.8*t-.28*Math.sin(u*Math.PI)*Math.sin(t*Math.PI*.9)+(Math.sin(u*9+t*4)*.04),.9-2.8*t]},cm);can.position.set(x,0,z);
    [[-.9,1.0],[0,1.0],[.9,1.0]].forEach(function(p){var s=new T3.Mesh(new T3.SphereGeometry(.34,8,6),clothMat(0x4a4030,{alphaTest:0,alphaMap:null}));s.scale.set(1,.85,.8);s.position.set(p[0],1.38,p[1]-.05);s.castShadow=true;G.add(s)});
    var lan=new T3.Mesh(new T3.CylinderGeometry(.11,.11,.26,6),MT.iron);lan.position.set(1.4,2.55,-1.2);G.add(lan);
    [-1.15,-.35,.9].forEach(function(hx,hi){hang(x+hx,2.82,z-1.86,.5+q()*.15,.9+q()*.5,hideMat,Math.PI,.05)});
    var gs=new T3.Sprite(new T3.SpriteMaterial({map:glowTex,color:new T3.Color(1.1,.5,.18),blending:T3.AdditiveBlending,depthWrite:false,fog:false,transparent:true,opacity:.55}));gs.scale.set(1.7,1.7,1);gs.position.set(x+1.4,2.55,z-1.2);scene.add(gs);
    var core=new T3.Mesh(new T3.SphereGeometry(.08,8,6),new T3.MeshBasicMaterial({color:new T3.Color(3,1.6,.6)}));core.position.copy(gs.position);scene.add(core);
    pool(x+1.2,.05,z-1.8,3.4,0xff8a3a,.3,false);
    return gs;
  }
  var sackGeo=roughen(new T3.SphereGeometry(.38,10,8),.035,2.2);sackGeo.translate(0,.3,0);
  var sackList=[];
  function addSacks(cx,cz,n,spread){for(var i=0;i<n;i++){var a=rand()*6.283,rr=rand()*spread;sackList.push([cx+Math.cos(a)*rr,cz+Math.sin(a)*rr,.8+rand()*.5,rand()*6,rand()<.3?.9:0])}}
  var stallDefs=[[-9.5,30],[-6.2,32.5],[6.2,32.5],[9.5,30],[-9.5,16],[-6.2,13.6],[6.2,13.6],[9.5,16]];
  var stallGlows=stallDefs.map(function(s,i){return stall(s[0],s[1],i,300+i)});
  stallDefs.forEach(function(sd){addSacks(sd[0]+2.1,sd[1]-1.0,3,.9)});
  addSacks(-6,40,5,1.6);addSacks(7,41,5,1.6);addSacks(-25,22,4,1.4);addSacks(25,21,4,1.4);addSacks(-19,10,4,1.2);addSacks(20,25,3,1.2);

  /* ---------- barrels, crates (instanced) ---------- */
  var barrelPos=[];
  for(var bz=29.6;bz<=44.8;bz+=1.38)for(var bx=12;bx<=29;bx+=1.38){var ex=(bx-21)/8.6,ez=(bz-37.2)/7.8;if(ex*ex+ez*ez+(rand()-.5)*.16<1&&rand()<.97)barrelPos.push([bx+(rand()-.5)*.16,bz+(rand()-.5)*.16])}
  for(bz=7;bz<=18;bz+=1.38)barrelPos.push([28.8+(rand()-.5)*.1,bz]);
  for(bz=36.5;bz<=44;bz+=1.38)for(bx=-29;bx<=-26;bx+=1.38)if(rand()<.8)barrelPos.push([bx,bz]);
  var blobs=[];
  (function(){
    var prof=[[0.001,0],[.42,0],[.5,.2],[.57,.62],[.5,1.08],[.43,1.28],[.001,1.28]].map(function(p){return new T3.Vector2(p[0],p[1])}),g=new T3.LatheGeometry(prof,18);
    var im=new T3.InstancedMesh(g,MT.barrel,barrelPos.length),lid=new T3.InstancedMesh(new T3.CircleGeometry(.42,16),MT.lid,barrelPos.length),dm=new T3.Object3D(),dl=new T3.Object3D(),cc=new T3.Color();
    barrelPos.forEach(function(p,i){
      var s=.95+rand()*.12,ry=rand()*6.283,tl=rand()<.07?(rand()-.5)*.3:(rand()-.5)*.05;dm.position.set(p[0],0,p[1]);dm.rotation.set(tl,ry,(rand()-.5)*.05);dm.scale.set(s,s*(.98+rand()*.06),s);dm.updateMatrix();im.setMatrixAt(i,dm.matrix);
      dl.position.set(p[0],1.282*s,p[1]);dl.rotation.set(-Math.PI/2+tl,0,0);dl.scale.set(s,s,s);dl.updateMatrix();lid.setMatrixAt(i,dl.matrix);
      var k=.55+rand()*.55;cc.setRGB(k,k*(.9+rand()*.1),k*(.82+rand()*.15));im.setColorAt(i,cc);blobs.push([p[0],p[1],1.5]);
    });
    im.castShadow=im.receiveShadow=true;lid.receiveShadow=true;scene.add(im,lid);
  })();
  var crates=[[-14,10,.2],[-12.4,9.2,-.3],[-13.5,11.6,.1],[10,8.5,.4],[11.7,9.4,.1],[-20,25,.2],[-8,40,.3],[-6.6,41.4,-.2],[8,18.6,0],[24,34,.5],[-23,19,.2],[19,26.4,.3],[-2,6,-.2],[3,38,.1],[-24,7,.5],[26,18,-.3],[.4,29.4,.2]];
  crates.forEach(function(k){
    var g=new T3.BoxGeometry(1.6,1.5,1.6),o=put(new T3.Mesh(g,MT.crate),k[0],.75,k[1]);o.rotation.y=k[2];o.rotation.z=(rand()-.5)*.03;blobs.push([k[0],k[1],2.6]);
    if(rand()<.35){var o2=put(new T3.Mesh(new T3.BoxGeometry(1.25,1.25,1.25),MT.crate),k[0]+.1,2.125,k[1]);o2.rotation.y=k[2]+.4}
  });
  /* ---------- siege debris: ruined front wall, chevaux de frise, carts, shields, ladders ---------- */
  (function(){
    var q=mulberry(4242),x=-26;
    while(x<26){
      var w=2.2+q()*2.6,h=.6+q()*1.3;
      if(q()<.6){var rb=solid(w,h,2.0,x+w/2,-.4,-2.2+q()*.8,MT.stone,4,(q()-.5)*.22,.14);rb.rotation.z=(q()-.5)*.1;for(var rr=0;rr<5;rr++){var rs=.25+q()*.55;solid(rs*1.5,rs,rs*1.2,x+q()*w,0,-3.2-q()*1.6,MT.stone,2,q()*3,.07)}}
      x+=w+(q()<.5?2+q()*4.5:.4);
    }
    [-24,-16,-9,-3,5,12,19,25].forEach(function(cx,i){
      var g=new T3.Group();g.position.set(cx+(q()-.5)*2,0,3.5+q()*3.5);g.rotation.y=q()*3;scene.add(g);
      var b1=new T3.Mesh(new T3.CylinderGeometry(.07,.07,2.9,6),MT.barkD);b1.rotation.z=Math.PI/2;b1.position.y=.55;b1.castShadow=true;g.add(b1);
      [1,-1].forEach(function(sgn){var d=new T3.Mesh(new T3.CylinderGeometry(.07,.07,2.7,6),MT.barkD);d.rotation.x=Math.PI/2;d.rotation.z=0;d.position.y=.55;d.rotation.set(sgn*.7,0,0);d.position.set(0,.8,0);d.castShadow=true;g.add(d);
        for(var k=-2;k<=2;k++){var sp=new T3.Mesh(new T3.ConeGeometry(.04,.5,5),MT.iron);sp.position.set(k*.55,.55,sgn*0);sp.rotation.z=Math.PI/2*0;g.add(sp)}});
    });
    function wheel(par,x,y,z,r,ry,rz){
      var g=new T3.Group(),rim=new T3.Mesh(new T3.TorusGeometry(r,.07,6,16),MT.barkD);g.add(rim);
      for(var k=0;k<4;k++){var sp=new T3.Mesh(new T3.BoxGeometry(.07,r*2,.07),MT.barkD);sp.rotation.z=k*Math.PI/4;g.add(sp)}
      var hub=new T3.Mesh(new T3.CylinderGeometry(.13,.13,.2,8),MT.ironD);hub.rotation.x=Math.PI/2;g.add(hub);
      g.position.set(x,y,z);g.rotation.set(0,ry||0,rz||0);g.traverse(function(m){if(m.isMesh)m.castShadow=true});par.add(g);
    }
    [[-19,9.5,.4,0],[17.5,24,-.7,1]].forEach(function(c){
      var g=new T3.Group();g.position.set(c[0],0,c[1]);g.rotation.y=c[2];scene.add(g);
      gbox(g,2.8,.14,1.5,0,.78,0,MT.wood,0,0,c[3]?.14:0);
      [-1,1].forEach(function(sd){gbox(g,2.8,.5,.08,0,.9,sd*.72,MT.wood,0,0,c[3]?.14:0)});
      gbox(g,.08,.5,1.5,-1.38,.9,0,MT.wood);
      wheel(g,-.9,.55,.85,.55,Math.PI/2,0);if(!c[3])wheel(g,.9,.55,.85,.55,Math.PI/2,0);else wheel(g,1.5,.1,1.6,.55,0,Math.PI/2*.97);
      gbox(g,2.4,.08,.08,2.2,.45,0,MT.barkD,0,0,.35);
      blobs.push([c[0],c[1],4]);
    });
    var sg=new T3.CylinderGeometry(.5,.5,.07,14);sg.translate(0,.04,0);scaleUV(sg,1,1);
    var si=new T3.InstancedMesh(sg,new T3.MeshStandardMaterial({map:MT.wood.map,normalMap:MT.wood.normalMap,roughness:.8,metalness:.1}),30),o=new T3.Object3D(),cc=new T3.Color();
    var scol=[[.55,.18,.14],[.2,.28,.4],[.42,.4,.36],[.5,.35,.15],[.18,.34,.3]];
    for(var i=0;i<30;i++){var zone=q();var xx=zone<.5?(q()-.5)*30:(q()<.5?-1:1)*(10+q()*16),zz=zone<.5?6+q()*30:3+q()*22;o.position.set(xx,.08+q()*.05,zz);o.rotation.set((q()-.5)*.35,q()*6,(q()-.5)*.35);var sc3=.8+q()*.4;o.scale.set(sc3,1,sc3);o.updateMatrix();si.setMatrixAt(i,o.matrix);var c=scol[(q()*5)|0];cc.setRGB(c[0]*1.4,c[1]*1.4,c[2]*1.4);si.setColorAt(i,cc);blobs.push([xx,zz,1.3])}
    si.castShadow=true;si.receiveShadow=true;scene.add(si);
    [-15,-3.2,11].forEach(function(lx){
      var g=new T3.Group();g.position.set(lx,0,AZ-3.3);g.rotation.x=.4;scene.add(g);
      [-.32,.32].forEach(function(rx){var r=new T3.Mesh(new T3.BoxGeometry(.09,6.6,.09),MT.barkD);r.position.set(rx,3.3,0);r.castShadow=true;g.add(r)});
      for(var k=0;k<14;k++){var rg=new T3.Mesh(new T3.BoxGeometry(.64,.07,.07),MT.barkD);rg.position.set(0,.4+k*.45,0);g.add(rg)}
    });
  })();

  /* ---------- arrows, logs, wreck ---------- */
  (function(){
    var N=60,g=new T3.CylinderGeometry(.018,.018,1.1,4);g.translate(0,.4,0);
    var im=new T3.InstancedMesh(g,new T3.MeshStandardMaterial({color:0x3b2c1c,roughness:.9}),N),o=new T3.Object3D();
    for(var i=0;i<N;i++){var zone=rand();var x,z;if(zone<.5){x=(rand()-.5)*14;z=AZ-2-rand()*8}else{x=(rand()<.5?-1:1)*(AX-2-rand()*6);z=14+rand()*16}
      o.position.set(x,0,z);o.rotation.set((rand()-.5)*.9,rand()*6,(rand()-.5)*.9);o.updateMatrix();im.setMatrixAt(i,o.matrix)}
    im.castShadow=true;scene.add(im);
    [[-21,6.2,-3.4],[-27,3.6,15],[25,5,36],[-8,4.4,2.6]].forEach(function(l){ // fallen logs
      var lg=new T3.CylinderGeometry(.3,.34,5,8);lg.rotateZ(Math.PI/2);scaleUV(lg,1.5,2);var m=put(new T3.Mesh(lg,MT.bark),l[0],.32,l[2]);m.rotation.y=l[1]});
  })();

  /* ---------- trees ---------- */
  var cardList=[];
  function pine(x,z,s,seed){
    var q=mulberry(seed),tg=new T3.CylinderGeometry(.14*s,.4*s,8.4*s,8,3);scaleUV(tg,1.5,5);roughen(tg,.03,1.2);
    put(new T3.Mesh(tg,MT.bark),x,4.2*s,z);
    var tiers=9;
    for(var j=0;j<tiers;j++){
      var t=j/(tiers-1),y=(1.2+j*.92)*s,L=(4.2-t*3)*s,n=Math.round(17-t*8);
      for(var k=0;k<n;k++){if(j<2&&q()<.72)continue;cardList.push({x:x,y:y+(q()-.5)*.35*s,z:z,yaw:k/n*6.283+q()*.5+j*.45,droop:.5+q()*.3-t*.28,L:L*(.82+q()*.36),h:.46+q()*.12,c:.55+.5*t+q()*.25,g:q()})}
    }
  }
  var treeDefs=[[-27,41,1.25],[-24.4,43,1.05],[-26.2,37.4,1.15],[-23,39.6,.95],[-27.2,33,1.15],[-24.6,31.2,1.05],[-27,12,1.25],[-24.8,9,1.15],[-27.4,5.6,1.05],[-23.4,5,1.2],[-24,13.2,.95],[4,8,1.05],[2,6.4,1.15],[6.2,6,.95],[22,13,1.15],[24,10.5,1.05],[20,10,.95],[25.5,15,1.15],[-12,41.5,1.15],[13,7.4,.85],[-4,5,.85]];
  treeDefs.forEach(function(t,i){pine(t[0],t[1],t[2],700+i)});
  (function(){
    var g=new T3.PlaneGeometry(1,1);g.rotateX(-Math.PI/2);g.translate(.5,0,0);
    var mat=new T3.MeshStandardMaterial({map:pineTex,alphaTest:.45,alphaToCoverage:!lowq&&hdr,side:T3.DoubleSide,roughness:.95,color:0xffffff}),im=new T3.InstancedMesh(g,mat,cardList.length),o=new T3.Object3D(),cc=new T3.Color();
    cardList.forEach(function(c,i){
      o.position.set(c.x,c.y,c.z);o.rotation.set(0,c.yaw,-c.droop,'YZX');o.scale.set(c.L,1,c.L*c.h);o.updateMatrix();im.setMatrixAt(i,o.matrix);
      var k=Math.min(1.2,c.c);cc.setRGB(.5*k,.6*k+.05*c.g,.52*k);im.setColorAt(i,cc);
    });
    im.castShadow=true;im.receiveShadow=true;scene.add(im);
  })();
  /* gnarled dead trees: recursive tapered tubes merged into one mesh */
  function deadTree(x,z,sc,seed){
    var r=mulberry(seed),P=[],N=[],U=[],I=[],vc=0;
    function ring(p,d,rad,v,sides){
      var up=Math.abs(d.y)>.9?new T3.Vector3(1,0,0):new T3.Vector3(0,1,0),s1=new T3.Vector3().crossVectors(d,up).normalize(),s2=new T3.Vector3().crossVectors(s1,d).normalize();
      for(var i=0;i<=sides;i++){var a=i/sides*6.2832,cx=Math.cos(a),sy=Math.sin(a),nx=s1.x*cx+s2.x*sy,ny=s1.y*cx+s2.y*sy,nz=s1.z*cx+s2.z*sy;P.push(p.x+nx*rad,p.y+ny*rad,p.z+nz*rad);N.push(nx,ny,nz);U.push(i/sides*Math.max(1,rad*2.4),v)}
    }
    function branch(p0,dir,len,rad,depth){
      var segs=Math.max(3,Math.round(len/.9)),sides=depth>=2?8:6,p=p0.clone(),d=dir.clone().normalize(),v=0,pts=[];
      for(var i=0;i<=segs;i++){pts.push({p:p.clone(),d:d.clone(),rad:rad*(1-i/segs*.85)*(i===0&&depth===3?1.6:1),v:v});p.addScaledVector(d,len/segs);d.add(new T3.Vector3((r()-.5)*.55,(r()-.5)*.3+(depth<3?-.05:.07),(r()-.5)*.55)).normalize();v+=len/segs/2.4}
      var base=vc;pts.forEach(function(q){ring(q.p,q.d,q.rad,q.v,sides);vc+=sides+1});
      for(i=1;i<=segs;i++){var a0=base+(i-1)*(sides+1);for(var k=0;k<sides;k++){var q2=a0+k;I.push(q2,q2+1,q2+sides+1,q2+1,q2+sides+2,q2+sides+1)}}
      if(depth>0)for(i=1;i<segs;i++)if(r()<(depth===3?.6:.5)){var nd=pts[i].d.clone().add(new T3.Vector3((r()-.5)*2.2,(r()-.15)*.7,(r()-.5)*2.2)).normalize();branch(pts[i].p,nd,len*(.4+r()*.22),rad*.5,depth-1)}
    }
    branch(new T3.Vector3(0,0,0),new T3.Vector3((r()-.5)*.3,1,(r()-.5)*.3),7.5,.42,3);
    var g=new T3.BufferGeometry();g.setAttribute('position',new T3.Float32BufferAttribute(P,3));g.setAttribute('normal',new T3.Float32BufferAttribute(N,3));g.setAttribute('uv',new T3.Float32BufferAttribute(U,2));g.setIndex(I);
    var m=new T3.Mesh(g,MT.barkD);m.scale.setScalar(sc);m.position.set(x,0,z);m.rotation.y=r()*6;m.castShadow=true;m.receiveShadow=true;scene.add(m);
  }
  [[-34,52,1.4],[-12,58,1.7],[14,56,1.5],[36,54,1.4],[-44,30,1.3],[44,34,1.5],[-40,5,1.3],[42,2,1.2],[2,64,1.9],[-27,-6,1.0],[27,-8,1.0]].forEach(function(t,i){deadTree(t[0],t[1],t[2],900+i)});
  deadTree(-28.5,2.8,.9,999);deadTree(28,3.4,.95,998);deadTree(-1.5,43.6,.8,997);
  (function(){
    var N=420,tiers=[[2.8,5.2,2.6],[2.2,4.6,5.6],[1.6,4.2,8.4],[1.0,3.4,10.9]],ims=tiers.map(function(t){var g=new T3.ConeGeometry(t[0],t[1],9);g.translate(0,t[2],0);return new T3.InstancedMesh(g,new T3.MeshStandardMaterial({color:0xffffff,roughness:1}),N)}),o=new T3.Object3D(),cc=new T3.Color(),i=0,guard=0;
    while(i<N&&guard++<4000){
      var x=(rand()*2-1)*190,z=-12+rand()*210,ok=(z>=56&&Math.abs(x)<190)||(Math.abs(x)>=46&&z>=22);
      if(!ok)continue;
      var sc2=.8+rand()*1.2;o.position.set(x,0,z);o.rotation.set(0,rand()*6,0);o.scale.set(sc2,sc2*(.9+rand()*.6),sc2);o.updateMatrix();
      cc.setHSL(.52+rand()*.05,.25,.012+rand()*.016);
      ims.forEach(function(im){im.setMatrixAt(i,o.matrix);im.setColorAt(i,cc)});i++;
    }
    ims.forEach(function(im){im.count=i;scene.add(im)});
  })();

  /* ---------- rubble and dead grass ---------- */
  (function(){
    var g=new T3.DodecahedronGeometry(.8,1),p=g.attributes.position;for(var i=0;i<p.count;i++){var f=.75+((Math.sin(p.getX(i)*9.3+p.getZ(i)*5.1+p.getY(i)*3.7)*43758.5)%1+1)%1*.55;p.setXYZ(i,p.getX(i)*f,p.getY(i)*f*.75,p.getZ(i)*f)}g.computeVertexNormals();scaleUV(g,.6,.6);
    var N=90,im=new T3.InstancedMesh(g,MT.stone,N),o=new T3.Object3D(),cc=new T3.Color();
    for(i=0;i<N;i++){var s=.25+rand()*.9,edge=rand()<.55,xx=edge?(rand()<.5?-1:1)*(AX-1.5-rand()*3):(rand()*2-1)*(AX-3),zz=edge?3+rand()*(AZ-6):2+rand()*(AZ-4);o.position.set(xx,s*.2,zz);o.rotation.set(rand()*3,rand()*6,rand()*3);o.scale.set(s,s*.8,s);o.updateMatrix();im.setMatrixAt(i,o.matrix);var k=.6+rand()*.5;cc.setRGB(k,k,k);im.setColorAt(i,cc);}
    im.castShadow=im.receiveShadow=true;scene.add(im);
    var gg=new T3.PlaneGeometry(1.2,1.2);gg.translate(0,.6,0);
    var gcv=cvs2(128,128),gc=gcv.getContext('2d'),q=mulberry(8);
    for(i=0;i<70;i++){var gx=14+q()*100,gh=22+q()*52,bend=(q()-.5)*34,gr=gc.createLinearGradient(0,128,0,128-gh);gr.addColorStop(0,'#14120c');gr.addColorStop(1,q()<.5?'#4a432c':'#5b5136');gc.strokeStyle=gr;gc.lineWidth=2.6;gc.lineCap='round';gc.beginPath();gc.moveTo(gx,128);gc.quadraticCurveTo(gx+bend*.3,128-gh*.6,gx+bend,128-gh);gc.stroke()}
    var gm=new T3.MeshStandardMaterial({map:ctex(gcv),alphaTest:.4,side:T3.DoubleSide,roughness:1}),NN=620,gi=new T3.InstancedMesh(gg,gm,NN),oo=new T3.Object3D();
    for(i=0;i<NN;i++){
      var ed=rand(),x2,z2;
      if(ed<.4){x2=(rand()<.5?-1:1)*(AX-.5-rand()*2.2);z2=rand()*AZ}else if(ed<.6){x2=(rand()*2-1)*AX;z2=AZ-.5-rand()*2}else{x2=(rand()*2-1)*(AX-1);z2=1+rand()*(AZ-3)}
      var s2=.5+rand()*.8;oo.position.set(x2,0,z2);oo.rotation.set(0,rand()*3.14,0);oo.scale.set(s2,s2*(.6+rand()*.5),s2);oo.updateMatrix();gi.setMatrixAt(i,oo.matrix);cc.setRGB(.6+rand()*.4,.55+rand()*.35,.4+rand()*.3);gi.setColorAt(i,cc);
    }
    gi.receiveShadow=true;scene.add(gi);
  })();

  (function(){
    var sm=new T3.MeshStandardMaterial({map:cloth_a,normalMap:cloth_n,roughness:1,color:0xffffff}),im=new T3.InstancedMesh(sackGeo,sm,sackList.length),o=new T3.Object3D(),cc=new T3.Color();
    sackList.forEach(function(k,i){o.position.set(k[0],0,k[1]);o.rotation.set(0,k[3],0);o.scale.set(k[2],k[2],k[2]);o.updateMatrix();im.setMatrixAt(i,o.matrix);var t=.5+rand()*.4;cc.setRGB(t*.95,t*.82,t*.6);im.setColorAt(i,cc);blobs.push([k[0],k[1],1.1*k[2]])});
    im.castShadow=im.receiveShadow=true;scene.add(im);
  })();
  (function(){ /* contact shadows */
    var g=new T3.PlaneGeometry(1,1);g.rotateX(-Math.PI/2);
    var im=new T3.InstancedMesh(g,new T3.MeshBasicMaterial({map:softTex,transparent:true,depthWrite:false,color:0x000000,opacity:.85,polygonOffset:true,polygonOffsetFactor:-1,polygonOffsetUnits:-1}),blobs.length),o=new T3.Object3D();
    blobs.forEach(function(b,i){o.position.set(b[0],.03,b[1]);o.scale.set(b[2],1,b[2]);o.updateMatrix();im.setMatrixAt(i,o.matrix)});im.renderOrder=2;scene.add(im);
  })();

  /* ---------- smoke columns ---------- */
  var smokeTex=(function(){var cv=cvs2(128,128),c=cv.getContext('2d'),id=c.createImageData(128,128),d=id.data;
    for(var i=0;i<16384;i++){var x=(i&127)/128-.5,y=(i>>7)/128-.5,r=Math.sqrt(x*x+y*y)*2,n=fbmA((i&127)/128,(i>>7)/128,4,4,4),a=Math.max(0,1-r)*(.4+.9*n);a=Math.min(1,a*a*1.6);d[i*4]=d[i*4+1]=d[i*4+2]=255;d[i*4+3]=a*255}
    c.putImageData(id,0,0);return ctex(cv)})();
  var smoke=[];
  fires.forEach(function(f,fi){
    if(f.h<1.9)return;
    for(var k=0;k<7;k++){
      var m=new T3.SpriteMaterial({map:smokeTex,color:new T3.Color(.11,.1,.095),transparent:true,depthWrite:false,opacity:0,fog:true}),sp=new T3.Sprite(m);sp.visible=false;scene.add(sp);
      smoke.push({sp:sp,f:f,t:k/7*6,life:6})
    }
  });

  /* ---------- haze, embers, ash ---------- */
  var mists=[];
  [[0,.7,12,80,.2],[0,1.5,30,84,.22],[-6,.6,44,76,.2],[8,2.4,4,64,.15],[0,3.6,24,90,.1]].forEach(function(m,i){
    var mm=new T3.Mesh(new T3.PlaneGeometry(m[3],m[3]*.62),new T3.MeshBasicMaterial({map:mistTex,transparent:true,opacity:m[4],depthWrite:false,fog:false,side:T3.DoubleSide,color:0x2e404f}));
    mm.rotation.x=-Math.PI/2;mm.position.set(m[0],m[1],m[2]);mm.renderOrder=4;scene.add(mm);mists.push({m:mm,ph:i*2,x0:m[0]});
  });
  function particles(n,col,size,op,spread,yMax){
    var pos=new Float32Array(n*3),g=new T3.BufferGeometry();
    for(var i=0;i<n;i++){pos[i*3]=(rand()*2-1)*spread;pos[i*3+1]=rand()*yMax;pos[i*3+2]=-4+rand()*62}
    g.setAttribute('position',new T3.BufferAttribute(pos,3));
    var p=new T3.Points(g,new T3.PointsMaterial({map:emberTex,color:col,size:size,transparent:true,opacity:op,blending:T3.AdditiveBlending,depthWrite:false,fog:false}));p.frustumCulled=false;scene.add(p);return p;
  }
  var embers=particles(170,new T3.Color(2.4,1.2,.5),.5,.9,36,24),NE=170;
  var ash=particles(140,0x8a929a,.35,.28,40,26),NA=140;
  ash.material.blending=T3.NormalBlending;

  /* ---------- foreground column ---------- */
  (function(){var g=new T3.CylinderGeometry(2.4,2.8,40,20,10);scaleUV(g,5,10);roughen(g,.12,.5);put(new T3.Mesh(g,MT.stone),30.5,20,-16,true,true);
    hang(27.6,22,-18.6,2.0,6,bannerMats[0],0,.08)})();

  /* ---------- standards (variant B): tattered war banners ---------- */
  var stds=new T3.Group();scene.add(stds);
  function makeBanner(h){
    var W2=512,H2=1024,cv=cvs2(W2,H2),c=cv.getContext('2d'),ev=cvs2(W2,H2),e=ev.getContext('2d'),dark={knight:['#14201a','#233629'],pyro:['#2a120d','#432018'],archer:['#0c1e22','#173c42']}[h.id];
    var g=c.createLinearGradient(0,0,W2,0);g.addColorStop(0,dark[0]);g.addColorStop(.5,dark[1]);g.addColorStop(1,dark[0]);c.fillStyle=g;c.fillRect(0,0,W2,H2);
    var id=c.getImageData(0,0,W2,H2),d=id.data;
    for(var y=0;y<H2;y++)for(var x=0;x<W2;x++){var w=((x&3)<2?1:.88)*((y&3)<2?1:.9),f=.55+.7*fbmA(x/W2,y/H2,10,14,4)+.1*Math.sin(x*.11),dirt=1-.55*sstep(.55,.85,fbmA(x/W2,y/H2,4,6,3))*(y/H2),i=(y*W2+x)*4;d[i]*=w*f*dirt;d[i+1]*=w*f*dirt;d[i+2]*=w*f*dirt}
    c.putImageData(id,0,0);
    c.strokeStyle='#8d7140';c.lineWidth=7;c.strokeRect(30,30,W2-60,H2-60);c.strokeStyle='rgba(141,113,64,.5)';c.lineWidth=2;c.strokeRect(46,46,W2-92,H2-92);
    function emblem(cc,col,lw,blur){cc.save();cc.translate(W2/2-160,350-160);cc.scale(5,5);cc.strokeStyle=col;cc.lineWidth=lw/5;cc.lineJoin='miter';cc.lineCap='butt';if(blur){cc.shadowColor=col;cc.shadowBlur=blur}
      var o=ICOD[h.id];(o.p||[]).forEach(function(dd){cc.stroke(new Path2D(dd))});(o.c||[]).forEach(function(a){cc.beginPath();cc.arc(a[0],a[1],a[2],0,6.283);cc.stroke()});cc.restore()}
    emblem(c,h.accent,5,20);emblem(c,h.accent,5,0);
    c.globalCompositeOperation='destination-out';
    c.beginPath();c.moveTo(0,H2);c.lineTo(0,H2-120);c.lineTo(W2*.25,H2-90);c.lineTo(W2*.25,H2);c.closePath();c.fill();
    c.beginPath();c.moveTo(W2*.25,H2);c.lineTo(W2*.5,H2-200);c.lineTo(W2*.75,H2);c.closePath();c.fill();
    for(var tk=0;tk<9;tk++){c.beginPath();c.ellipse(60+Math.random()*390,300+Math.random()*640,6+Math.random()*16,10+Math.random()*30,Math.random()*3,0,6.283);c.fill()}
    c.globalCompositeOperation='source-over';
    e.fillStyle='#000';e.fillRect(0,0,W2,H2);emblem(e,h.accent,6,16);
    return{map:ctex(cv),em:ctex(ev)};
  }
  [[HEROES[1],-7.5,9],[HEROES[0],0,12.5],[HEROES[2],7.5,9]].forEach(function(q){
    var h=q[0],bt=makeBanner(h),mat=new T3.MeshStandardMaterial({map:bt.map,emissiveMap:bt.em,emissive:0xffffff,emissiveIntensity:.8,alphaTest:.5,roughness:.95,side:T3.DoubleSide});
    var pole=new T3.Mesh(new T3.CylinderGeometry(.13,.17,13.6,8),MT.barkD);pole.position.set(q[1],6.8,q[2]);pole.castShadow=true;stds.add(pole);
    var fin=new T3.Mesh(new T3.ConeGeometry(.22,1.1,6),MT.iron);fin.position.set(q[1],14.2,q[2]);fin.castShadow=true;stds.add(fin);
    var bar=new T3.Mesh(new T3.CylinderGeometry(.07,.07,3.6,6),MT.barkD);bar.rotation.z=Math.PI/2;bar.position.set(q[1],12.6,q[2]);stds.add(bar);
    var cm=hang(q[1],12.5,q[2]-.25,3.2,6.4,mat,Math.PI,.16);scene.remove(cm);stds.add(cm);
    var gs=new T3.Sprite(new T3.SpriteMaterial({map:glowTex,color:new T3.Color(h.accent).multiplyScalar(1.2),blending:T3.AdditiveBlending,depthWrite:false,fog:false,transparent:true,opacity:.3}));gs.scale.set(5,5,1);gs.position.set(q[1],8.9,q[2]-.6);stds.add(gs);
  });
  stds.visible=false;

  /* ---------- the eyes in the dark ---------- */
  var gb=new T3.Mesh(new T3.PlaneGeometry(11,8.2),new T3.MeshBasicMaterial({color:0x010203,fog:false,side:T3.DoubleSide}));gb.position.set(0,4.1,AZ+7);scene.add(gb);
  var gpatch=new T3.Mesh(new T3.PlaneGeometry(12,9),new T3.MeshBasicMaterial({color:0x020304,transparent:true,opacity:.95,fog:false,depthWrite:false}));gpatch.rotation.x=-Math.PI/2;gpatch.position.set(0,.07,AZ+4.2);scene.add(gpatch);
  var eyeGroups=[{n:15,x:[-4,4],y:[.9,2.6],z:[AZ+3.2,AZ+6.2],wide:1},{n:4,x:[-AX-9.5,-AX-3.5],y:[.9,2.4],z:[18.8,27.2]},{n:4,x:[AX+3.5,AX+9.5],y:[.9,2.4],z:[18.8,27.2]}];
  var eyes=[],PAL=[[0xffd9a0,0xff6a2b],[0xffb0a0,0xff2a18],[0xe4ffa8,0x8ce020]];
  eyeGroups.forEach(function(gp){
    for(var i=0;i<gp.n;i++){
      var e={gp:gp,state:0,t:0,dur:rand()*5,open:0,sz:.3,sp:.35,big:false,sprs:[]};
      for(var k=0;k<3;k++){
        var m=new T3.SpriteMaterial({map:k<2?eyeTex:glowTex,color:k<2?0xffffff:0xff6a2b,transparent:true,opacity:0,depthWrite:false,blending:k<2?T3.NormalBlending:T3.AdditiveBlending,fog:false}),s=new T3.Sprite(m);s.visible=false;scene.add(s);e.sprs.push(s);
      }
      eyes.push(e);
    }
  });
  function placeEye(e){
    var gp=e.gp;e.big=!!gp.wide&&rand()<.12;
    var x=gp.x[0]+rand()*(gp.x[1]-gp.x[0]),y=gp.y[0]+rand()*(gp.y[1]-gp.y[0])+(e.big?.5:0),z=gp.z[0]+rand()*(gp.z[1]-gp.z[0]);
    e.sz=(e.big?.95:.5+rand()*.26);e.sp=e.sz*(e.big?1.6:1.5);
    e.sprs[0].position.set(x-e.sp/2,y,z);e.sprs[1].position.set(x+e.sp/2,y,z);e.sprs[2].position.set(x,y,z);
    var pr=rand(),pal=PAL[pr<.62?0:pr<.86?1:2];if(e.big)pal=[0xffe08a,0xffb02a];
    e.sprs[0].material.color.setHex(pal[0]);e.sprs[1].material.color.setHex(pal[0]);e.sprs[2].material.color.setHex(pal[1]);e.sprs[2].material.color.multiplyScalar(1.3);
  }
  function setEye(e){
    var o=e.open,w=e.sz*(.55+.45*o),h=e.sz*.5*Math.pow(o,1.3)+.001;
    e.sprs[0].scale.set(w,h,1);e.sprs[1].scale.set(w,h,1);var gl=e.sz*(e.big?5.2:3.2)*(.4+.6*o);e.sprs[2].scale.set(gl,gl*.55,1);
    e.sprs[0].material.opacity=e.sprs[1].material.opacity=Math.min(1,o*1.4);e.sprs[2].material.opacity=.5*o*o;
    var vis=o>.01;e.sprs[0].visible=e.sprs[1].visible=e.sprs[2].visible=vis;
  }
  function updateEyes(dt){
    for(var i=0;i<eyes.length;i++){
      var e=eyes[i];e.t+=dt;
      if(e.state===0){if(e.t>e.dur){placeEye(e);e.state=1;e.t=0}}
      else if(e.state===1){var p=Math.min(1,e.t/.55);e.open=1-Math.pow(1-p,3);if(p>=1){e.state=2;e.t=0;e.dur=1.4+rand()*5;e.blinks=rand()<.6?1+((rand()*2)|0):0;e.nb=e.dur*(.3+rand()*.5)}}
      else if(e.state===2){
        e.open=1;
        if(e.blinks>0&&e.t>e.nb){e.state=4;e.bt=0}
        else if(e.t>e.dur){e.state=3;e.t=0}
      }else if(e.state===4){e.bt+=dt;var b=e.bt/.22;e.open=b<.5?1-b*2*.95:(b-.5)*2*.95+.05;if(b>=1){e.open=1;e.state=2;e.blinks--;e.nb=e.t+.25+rand()*1.2}}
      else{var p2=Math.min(1,e.t/.32);e.open=1-p2*p2;if(p2>=1){e.open=0;e.state=0;e.t=0;e.dur=.4+rand()*4.5}}
      setEye(e);
    }
  }

  /* ---------- camera presets ---------- */
  var VIEWS={
    mainA:{p:[0,47,-52],t:[-2,0,22]},mainB:{p:[0,47,-52],t:[0,0,22]},
    hero:{p:[-14,36,-34],t:[6,3,26]},coop:{p:[12,42,-42],t:[-5,2,22]},
    rules:{p:[2,26,-12],t:[0,4,30]},settings:{p:[2,26,-12],t:[0,4,30]},pause:{p:[0,46,-54],t:[0,0,22]}
  };
  var cp=new T3.Vector3(0,47,-52),ct=new T3.Vector3(-2,0,22),vp=VIEWS.mainA,mx=0,my=0;
  var voff=-40,voffT=-40;cam.setViewOffset(W,H,0,voff,W,H);
  function setView(screen,variant){
    var sv=screen==='main'&&variant==='B';if(sv!==stds.visible){stds.visible=sv;R.shadowMap.needsUpdate=true}
    vp=screen==='main'?(variant==='A'?VIEWS.mainA:VIEWS.mainB):VIEWS[screen]||VIEWS.mainB;
    slow=screen!=='main'&&screen!=='pause';voffT=screen==='main'&&variant==='B'?-135:-40;
  }
  var slow=false,clock=new T3.Clock(),tsum=0,frame=0,fpsAcc=0,fpsN=0;
  document.getElementById('frame').addEventListener('mousemove',function(e){var b=this.getBoundingClientRect();mx=(e.clientX-b.left)/b.width-.5;my=(e.clientY-b.top)/b.height-.5});
  var reduceM=window.matchMedia&&window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  function loop(){
    requestAnimationFrame(loop);
    if(document.hidden||failed)return;
    var dt=Math.min(.05,clock.getDelta());tsum+=dt;frame++;
    if(slow&&(frame&1))return;
    var d2=reduceM?0:1,k=1-Math.pow(.02,dt);
    cp.x+=(vp.p[0]+mx*3.5*d2+Math.sin(tsum*.13)*1.2*d2-cp.x)*k;cp.y+=(vp.p[1]+my*-2*d2-cp.y)*k;cp.z+=(vp.p[2]-cp.z)*k;
    ct.x+=(vp.t[0]-ct.x)*k;ct.y+=(vp.t[1]-ct.y)*k;ct.z+=(vp.t[2]-ct.z)*k;
    voff+=(voffT-voff)*k;cam.setViewOffset(W,H,0,voff,W,H);cam.position.copy(cp);cam.lookAt(ct);sky.position.copy(cp);skyU.time.value=tsum;
    updateEyes(dt);
    lights.forEach(function(L){var f=1+L.f*(Math.sin(tsum*7+L.ph)*.5+Math.sin(tsum*13.3+L.ph*2)*.5)*.5+L.f*.25*Math.sin(tsum*23+L.ph*3);L.l.intensity=L.base*f});
    fires.forEach(function(f){
      var sw=1+.14*Math.sin(tsum*9+f.ph)+.09*Math.sin(tsum*17+f.ph*2);
      f.sp.scale.set(f.h*.55*(1+.08*Math.sin(tsum*13+f.ph)),f.h*1.25*sw,1);f.gs.material.opacity=.26+.1*Math.sin(tsum*11+f.ph);
      f.sp.material.rotation=Math.sin(tsum*3+f.ph)*.06;
    });
    for(var vi=0;vi<flameVars.length;vi++){var kk=(Math.floor(tsum*10+vi*2.3))%8;flameVars[vi].offset.set((kk%4)*.25,.5-(kk>>2)*.5)}
    waving.forEach(function(w){var pa=w.m.geometry.attributes.position,b=w.base,t=tsum*1.4+w.ph;for(var i=0;i<pa.count;i++){var v=-b[i*3+1]/w.h;pa.setZ(i,b[i*3+2]+Math.sin(b[i*3]*2.6+t+v*2)*w.amp*v*(.5+v))}pa.needsUpdate=true;w.m.geometry.computeVertexNormals()});
    if(rippleTex){rippleTex.offset.x=tsum*.02;rippleTex.offset.y=tsum*.015}
    mists.forEach(function(m){m.m.position.x=m.x0+Math.sin(tsum*.05+m.ph)*5});
    mistTex.offset.set(tsum*.004,tsum*.002);
    var ep=embers.geometry.attributes.position;for(var i=0;i<NE;i++){var y=ep.getY(i)+dt*(1.2+(i%5)*.5);if(y>24){y=0}ep.setY(i,y);ep.setX(i,ep.getX(i)+Math.sin(tsum*.9+i)*dt*1.4)}ep.needsUpdate=true;
    var ap=ash.geometry.attributes.position;for(i=0;i<NA;i++){var y2=ap.getY(i)-dt*(.5+(i%4)*.2);if(y2<0)y2=26;ap.setY(i,y2);ap.setX(i,ap.getX(i)+Math.sin(tsum*.4+i*1.3)*dt*.9)}ap.needsUpdate=true;
    smoke.forEach(function(sm){sm.t+=dt;if(sm.t>sm.life)sm.t=0;var u=sm.t/sm.life,f=sm.f,sp=sm.sp,sz=f.h*(1.2+u*4.2);sp.visible=true;sp.position.set(f.sp.position.x+Math.sin(tsum*.5+f.ph+u*3)*u*1.4+u*u*2.2,f.sp.position.y+f.h*.9+u*9,f.sp.position.z+Math.cos(tsum*.4+f.ph)*u*.8);sp.scale.set(sz,sz,1);sp.material.opacity=.3*Math.sin(Math.min(1,u*1.2)*Math.PI)*(1-u*.4)});
    renderFrame(tsum);
    if(frame<90&&!lowq){fpsAcc+=dt;fpsN++;if(frame===60&&fpsAcc/fpsN>.034&&res>.6){res=.75;R.setSize(RW(),RH(),false);setupPost()}}
  }
  loop();
  return{setView:setView};
}


