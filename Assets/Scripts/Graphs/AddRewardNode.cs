using UnityEngine;
using NodeEditorFramework.Utilities;
using System.Collections.Generic;


namespace NodeEditorFramework.Standard
{
    // Directly adds a part and credits/reputation/shards into the player's save. There is also an option to not notify the player.

    [Node(false, "Tasks/AddReward")]
    public class AddRewardNode : Node
    {
        //Node things
        public const string ID = "AddRewardNode";
        public override string GetName { get { return ID; } }

        public override bool AutoLayout { get { return true; } }

        public override string Title { get { return "Add Reward"; } }
        public override Vector2 MinSize { get { return new Vector2(208, 50); } }
    
        public RewardWrapper wrapper;
        public bool showPopup = true;
        
        bool init = false;
        Texture2D partTexture;
        float height = 320f;

        
        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob inputLeft;

        
        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob outputRight;

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            inputLeft.DisplayLayout();
            outputRight.DisplayLayout();
            GUILayout.EndHorizontal();

            showPopup = RTEditorGUI.Toggle(showPopup, "Show popup:");
            GUILayout.Label("Credit reward:");
            wrapper.creditReward = RTEditorGUI.IntField(wrapper.creditReward, GUILayout.Width(208f));
            GUILayout.Label("Reputation reward:");
            wrapper.reputationReward = RTEditorGUI.IntField(wrapper.reputationReward, GUILayout.Width(208f));
            GUILayout.Label("Shard reward:");
            wrapper.shardReward = RTEditorGUI.IntField(wrapper.shardReward, GUILayout.Width(208f));

            wrapper.partReward = RTEditorGUI.Toggle(wrapper.partReward, "Part reward", GUILayout.Width(200f));
            if(wrapper.partReward)
            {
                height += 320f;
                GUILayout.Label("Part ID:");
                wrapper.partID = GUILayout.TextField(wrapper.partID, GUILayout.Width(200f));
                if (ResourceManager.Instance != null && wrapper.partID != null && (GUI.changed || !init))
                {
                    init = true;
                    PartBlueprint partBlueprint = ResourceManager.GetAsset<PartBlueprint>(wrapper.partID);
                    if(partBlueprint != null)
                    {
                        partTexture = ResourceManager.GetAsset<Sprite>(partBlueprint.spriteID).texture;
                    }
                    else
                    {
                        partTexture = null;
                    }
                }
                if(partTexture != null)
                {
                    GUILayout.Label(partTexture);
                    height += partTexture.height + 8f;
                }
                else
                {
                    NodeEditorGUI.nodeSkin.label.normal.textColor = Color.red;
                    GUILayout.Label("<Part not found>");
                    NodeEditorGUI.nodeSkin.label.normal.textColor = NodeEditorGUI.NE_TextColor;
                }
                wrapper.partAbilityID = RTEditorGUI.IntField("Ability ID", wrapper.partAbilityID, GUILayout.Width(200f));
                string abilityName = AbilityUtilities.GetAbilityNameByID(wrapper.partAbilityID, null);
                if (abilityName != "Name unset")
                {
                    GUILayout.Label("Ability: " + abilityName);
                    height += 24f;
                }
                wrapper.partTier = RTEditorGUI.IntField("Part tier", wrapper.partTier, GUILayout.Width(200f));
                GUILayout.Label("Part Secondary Data:");
                wrapper.partSecondaryData = GUILayout.TextField(wrapper.partSecondaryData, GUILayout.Width(200f));
            }
            else
            {
                height += 160f;
            }
        }

        public override int Traverse()
        {
            if(showPopup) 
            {
                AudioManager.PlayClipByID("clip_victory", true);
                DialogueSystem.ShowReward(wrapper);
            }
            SectorManager.instance.player.AddCredits(wrapper.creditReward);
            SectorManager.instance.player.reputation += wrapper.reputationReward;
            SectorManager.instance.player.shards += wrapper.shardReward;
            if(wrapper.partReward)
            {
                SectorManager.instance.player.cursave.partInventory.Add(
                    new EntityBlueprint.PartInfo
                    {
                        partID = wrapper.partID,
                        abilityID = wrapper.partAbilityID,
                        tier = wrapper.partTier,
                        secondaryData = wrapper.partSecondaryData
                    });
            }
            return 0;
        }
    }
}