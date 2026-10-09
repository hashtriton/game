"""Exact sampled triangle-surface separation from vertices, edges and crossings."""
import numpy as np
from mathutils.bvhtree import BVHTree

def edges(points,faces):
    pairs=sorted({tuple(sorted((a,b))) for face in faces for a,b in zip(face,face[1:]+face[:1])})
    return np.asarray(points,dtype=float)[np.array(pairs)]

def crosses(edge,points,faces):
    tri=np.asarray(points,dtype=float)[np.array(faces)]; origin=edge[:,0,None,:]; direction=(edge[:,1]-edge[:,0])[:,None,:]
    e1=tri[None,:,1]-tri[None,:,0]; e2=tri[None,:,2]-tri[None,:,0]; h=np.cross(direction,e2); det=np.sum(e1*h,axis=2)
    good=np.abs(det)>1e-10; inv=np.divide(1,det,out=np.zeros_like(det),where=good); s=origin-tri[None,:,0]; u=np.sum(s*h,axis=2)*inv; q=np.cross(s,e1); v=np.sum(direction*q,axis=2)*inv; t=np.sum(e2*q,axis=2)*inv
    return bool(np.any(good&(u>=-1e-8)&(v>=-1e-8)&(u+v<=1+1e-8)&(t>=-1e-8)&(t<=1+1e-8)))

def segment_distance(first,second):
    first=np.asarray(first,dtype=float); second=np.asarray(second,dtype=float)
    u=(first[:,1]-first[:,0])[:,None,:]; v=(second[:,1]-second[:,0])[None,:,:]; w=first[:,0,None,:]-second[None,:,0,:]
    a=np.sum(u*u,axis=2); b=np.sum(u*v,axis=2); c=np.sum(v*v,axis=2); d=np.sum(u*w,axis=2); e=np.sum(v*w,axis=2)
    denom=a*c-b*b; valid=denom>1e-16
    s=np.divide(b*e-c*d,denom,out=np.zeros_like(denom),where=valid); t=np.divide(a*e-b*d,denom,out=np.zeros_like(denom),where=valid)
    interior=np.sum((w+s[:,:,None]*u-t[:,:,None]*v)**2,axis=2); interior[~(valid&(s>=0)&(s<=1)&(t>=0)&(t<=1))]=np.inf
    distances=[interior]
    for s in [0,1]:
        t=np.clip((e+s*b)/np.maximum(c,1e-16),0,1); distances.append(np.sum((w+s*u-t[:,:,None]*v)**2,axis=2))
    for t in [0,1]:
        s=np.clip((t*b-d)/np.maximum(a,1e-16),0,1); distances.append(np.sum((w+s[:,:,None]*u-t*v)**2,axis=2))
    return float(np.sqrt(min(np.min(x) for x in distances)))

def separation(p,f,q,g):
    first=edges(p,f); second=edges(q,g)
    if crosses(first,q,g) or crosses(second,p,f): return 0.0
    a=BVHTree.FromPolygons(p,f,all_triangles=True); b=BVHTree.FromPolygons(q,g,all_triangles=True)
    vertex=min(min(a.find_nearest(v)[3] for v in q),min(b.find_nearest(v)[3] for v in p))
    return min(vertex,segment_distance(first,second))
