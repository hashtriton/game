using System;

namespace Arena.Original
{
    public sealed partial class OriginalWorld
    {
        // RemoveDestructable does not publish a death callback or leave its
        // footprint behind. Authored and dynamic identities remain distinct.
        public bool RemoveAuthoredDoodad(int editorId)
        {
            if(!doodads.TryGetValue(editorId,out var d)||d.dynamic)return false;
            if(!navigation.SetDoodadAlive(editorId,false))return false;
            doodads.Remove(editorId);ClearTargets(OriginalWorldTargetKind.Doodad,editorId);revision++;return true;
        }
        public bool SetDoodadInvulnerability(int editorId,bool enabled)
        {
            if(!doodads.TryGetValue(editorId,out var d))return false;
            if(d.invulnerable!=enabled){d.invulnerable=enabled;revision++;}return true;
        }
    }
}
