using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    // Directly adds a part and credits/reputation/shards into the player's save. There is also an option to not notify the player.

    [Node(false, "Tasks/Add Reward")]
    public class AddRewardNode : Node
    {
        //Node things
        public const string ID = "AddRewardNode";

        public override string GetName
        {
            get { return ID; }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        public override string Title
        {
            get { return "Add Reward"; }
        }

        public override Vector2 MinSize
        {
            get { return new Vector2(208, 50); }
        }

        public RewardWrapper wrapper;
        public bool showPopup = true;
        public bool partShinyBool = false;

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

            showPopup = RTEditorGUI.Toggle(showPopup, "Show Popup:");
            wrapper.creditReward = RTEditorGUI.IntField("Credit Reward: ", wrapper.creditReward);
            wrapper.reputationReward = RTEditorGUI.IntField("Reputation Reward: ", wrapper.reputationReward);
            wrapper.shardReward = RTEditorGUI.IntField("Shard Reward: ", wrapper.shardReward);

            wrapper.partReward = RTEditorGUI.Toggle(wrapper.partReward, "Part reward", GUILayout.Width(200f));
            if (wrapper.partReward)
            {
                height += 320f;
                GUILayout.Label("Part ID:");
                wrapper.partID = GUILayout.TextField(wrapper.partID, GUILayout.Width(200f));
                if (ResourceManager.Instance != null && wrapper.partID != null && (GUI.changed || !init))
                {
                    init = true;
                    PartBlueprint partBlueprint = ResourceManager.GetAsset<PartBlueprint>(wrapper.partID);
                    if (partBlueprint != null)
                    {
                        partTexture = ResourceManager.GetAsset<Sprite>(partBlueprint.spriteID).texture;
                    }
                    else
                    {
                        partTexture = null;
                    }
                }

                if (partTexture != null)
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
                if (wrapper.partAbilityID < 0)
                {
                    wrapper.partAbilityID = RTEditorGUI.IntField("Ability ID", 0, GUILayout.Width(200f));
                    Debug.LogWarning("This identification does not exist!");
                }
                string abilityName = AbilityUtilities.GetAbilityNameByID(wrapper.partAbilityID, null);
                if (abilityName != "Name unset")
                {
                    GUILayout.Label("Ability: " + abilityName);
                    height += 24f;
                }
                wrapper.partTier = RTEditorGUI.IntField("Part tier", wrapper.partTier, GUILayout.Width(200f));
                GUILayout.Label("Part Secondary Data:");
                wrapper.partSecondaryData = GUILayout.TextArea(wrapper.partSecondaryData, GUILayout.Width(200f));
                partShinyBool = RTEditorGUI.Toggle(partShinyBool, "Is shiny");
            }
            else
            {
                height += 160f;
            }
        }

        public override int Traverse()
        {
            if (showPopup)
            {
                AudioManager.PlayClipByID("clip_victory", true);
                DialogueSystem.ShowReward(wrapper);
            }

            SectorManager.instance.player.AddCredits(wrapper.creditReward);
            SectorManager.instance.player.reputation += wrapper.reputationReward;
            SectorManager.instance.player.shards += wrapper.shardReward;
            if (wrapper.partReward)
            {
                SectorManager.instance.player.cursave.partInventory.Add(
                    new EntityBlueprint.PartInfo
                    {
                        partID = wrapper.partID,
                        abilityID = wrapper.partAbilityID,
                        tier = wrapper.partTier,
                        secondaryData = wrapper.partSecondaryData,
                        shiny = partShinyBool
                    });
            }

            return 0;
        }
    }
}
