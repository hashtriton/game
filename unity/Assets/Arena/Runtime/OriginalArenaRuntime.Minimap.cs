using Arena.Original;
using UnityEngine;

namespace Arena
{
    public sealed partial class OriginalArenaRuntime
    {
        public bool RequestMinimapOrder(Vector3 point,bool attackMove=false)
        {
            if(InputBlocked||SelectedUnit==null||SelectedUnit.health<=0||SelectedUnit.ownerSlot!=network.LocalSlot)return false;
            if(ArmedSkillId!=null||ArmedItemInstance!=0)
            {ArmedSkillId=null;ArmedItemInstance=0;return false;}
            AttackTargetArmed=false;ArmedSkillId=null;ArmedItemInstance=0;
            return network.SendCommand(attackMove?OriginalSessionCommandKind.AttackMove:OriginalSessionCommandKind.Move,
                x:point.x*map.unitsPerMeter,y:point.z*map.unitsPerMeter,actorEntityId:SelectedUnit.entityId);
        }
    }
}
