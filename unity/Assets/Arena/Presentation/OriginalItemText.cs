using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Arena.Original;

namespace Arena
{
    // Read-only descriptions use the same catalog and resolved intrinsic
    // profiles as equipment. They never evaluate damage or mutate a session.
    public sealed class OriginalItemText
    {
        readonly OriginalGameCatalogs catalogs;
        readonly OriginalItemRules rules;
        readonly OriginalInventoryEffects effects;
        readonly OriginalItemEffectText effectText;
        readonly Dictionary<string,string> descriptions=new Dictionary<string,string>();

        public OriginalItemText(OriginalGameCatalogs catalogs)
        {
            this.catalogs=catalogs;
            rules=new OriginalItemRules(catalogs.Items,catalogs.Native,catalogs.ObservedItems);
            effects=new OriginalInventoryEffects(catalogs.Passives,
                new OriginalItemUseRules(catalogs.Items,catalogs.Combat,catalogs.ObservedItems.itemUse));
            effectText=new OriginalItemEffectText(catalogs.Items,catalogs.Combat,effects);
        }

        public string Describe(string offerId)
        {
            if(string.IsNullOrEmpty(offerId))return "";
            if(descriptions.TryGetValue(offerId,out string cached))return cached;
            var offer=catalogs.Items.Item(offerId);if(offer==null)return "";
            string id=catalogs.Items.FromWorld(offerId)?.inventoryId??offerId;
            var item=catalogs.Items.Item(id);
            var text=new StringBuilder(item.displayName);
            var gold=rules.Field(offerId,"goldcost");var souls=rules.Field(offerId,"lumbercost");
            if(gold.known&&souls.known)text.Append("\nЦена: ").Append(gold.value).Append(" золота").Append(souls.value>0?" / "+souls.value+" душ":"");
            var slots=new OriginalItemInstance[6];
            slots[0]=new OriginalItemInstance{instanceId=1,itemId=id,ownerId=1};
            var plan=effects.Plan(new OriginalInventorySnapshot{ownerId=1,heroSlots=slots,servantSlots=new OriginalItemInstance[6]});
            var profile=plan.NativeProfile;
            var stats=new List<string>();
            if(profile.known)
            {
                Add(profile.attackDamage,"к урону");Add(profile.armor,"к броне");
                // I05P publishes all three candidate abilities but grants
                // only the owner's primary attribute in ComposeEquipment.
                if(id=="I05P")
                {
                    double str=catalogs.Combat.Ability("A11P").Number("DataC1");
                    double agi=catalogs.Combat.Ability("A11Q").Number("DataA1");
                    double intel=catalogs.Combat.Ability("A11R").Number("DataB1");
                    Add(profile.strength-str,"к силе");Add(profile.agility-agi,"к ловкости");Add(profile.intelligence-intel,"к интеллекту");
                    if(str==agi&&str==intel)Add(str,"к основной характеристике");
                }
                else {Add(profile.strength,"к силе");Add(profile.agility,"к ловкости");Add(profile.intelligence,"к интеллекту");}
                Add(profile.maxHealthFlat,"к здоровью");Add(profile.maxManaFlat,"к мане");
                Add(profile.attackSpeedFraction*100,"% к скорости атаки");
                Add(profile.moveSpeedFlat,"к скорости движения");
                Add(profile.healthRegenPerSecond,"здоровья/с");Add(profile.manaRegenBaseFraction*100,"% к базовому восстановлению маны");
            }
            if(stats.Count>0)text.Append("\n\nБазовые бонусы\n").Append(string.Join(" · ",stats));
            string activeDescription=effectText.DescribeActive(id),passiveDescription=effectText.DescribePassive(id);
            if(!string.IsNullOrEmpty(activeDescription))
            {
                text.Append("\n\nАктивное применение");
                var active=catalogs.Combat.Ability(item.cooldownId);
                if(active!=null)
                {
                    if(active.TryNumber("Cost1",out double mana,out _))text.Append("\nМана: ").Append(N(mana));
                    if(active.TryNumber("Cool1",out double cooldown,out _))text.Append("  Перезарядка: ").Append(N(cooldown)).Append(" с");
                }
                text.Append("\n").Append(activeDescription);
            }
            if(!string.IsNullOrEmpty(passiveDescription))text.Append("\n\nОсобые свойства\n").Append(passiveDescription);
            var recipes=new List<OriginalItemRecipe>();
            string linked=catalogs.Items.QuickBuy(id)?.linkedResultId;
            foreach(var recipe in catalogs.Items.recipes)
                if(recipe.resultId==id||linked!=null&&recipe.resultId==linked)recipes.Add(recipe);
            foreach(var recipe in recipes)
            {
                text.Append("\n\nСборка: ").Append(catalogs.Items.Item(recipe.resultId).displayName);
                foreach(var ingredient in recipe.ingredients)
                    text.Append("\n").Append(ingredient.count).Append(" × ").Append(catalogs.Items.Item(ingredient.itemId).displayName);
            }
            var upgrades=new List<string>();
            foreach(var recipe in catalogs.Items.recipes)
                if(Array.Exists(recipe.ingredients,ingredient=>ingredient.itemId==id))
                {string name=catalogs.Items.Item(recipe.resultId).displayName;if(!upgrades.Contains(name))upgrades.Add(name);}
            if(upgrades.Count>0)text.Append("\n\nИспользуется в сборке\n").Append(string.Join(", ",upgrades));
            return descriptions[offerId]=text.ToString();

            void Add(double value,string label)
            {if(Math.Abs(value)>1e-9)stats.Add((value>0?"+":"")+N(value)+" "+label);}
        }

        static string N(double value)=>value.ToString("0.##",CultureInfo.InvariantCulture);
    }
}
