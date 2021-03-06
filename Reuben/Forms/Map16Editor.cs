﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Daiz.Library;
using Daiz.NES.Reuben.ProjectManagement;

namespace Daiz.NES.Reuben
{
    public partial class Map16Editor : Form
    {
        PatternTable CurrentTable;
        private Dictionary<int, BlockProperty> solidityMap = new Dictionary<int, BlockProperty>();

        public Map16Editor()
        {
            InitializeComponent();

            CmbGraphics1.DisplayMember = CmbGraphics2.DisplayMember = CmbPalettes.DisplayMember = CmbDefinitions.DisplayMember = "Name";
            foreach (var g in ProjectController.GraphicsManager.GraphicsInfo)
            {
                CmbGraphics1.Items.Add(g);
                CmbGraphics2.Items.Add(g);
            }

            foreach (var p in ProjectController.PaletteManager.Palettes)
            {
                CmbPalettes.Items.Add(p);
            }

            foreach (var l in ProjectController.LevelManager.LevelTypes)
            {
                CmbDefinitions.Items.Add(l);
            }

            CurrentTable = ProjectController.GraphicsManager.BuildPatternTable(0);
            PtvTable.CurrentTable = CurrentTable;
            BlsBlocks.CurrentTable = CurrentTable;
            BlsBlocks.BlockLayout = ProjectController.LayoutManager.BlockLayouts[0];
            BlvCurrent.CurrentTable = CurrentTable;
            BlsBlocks.SpecialTable = ProjectController.SpecialManager.SpecialTable;
            BlsBlocks.SpecialPalette = ProjectController.SpecialManager.SpecialPalette;

            CmbGraphics1.SelectedIndex = 8;
            CmbGraphics2.SelectedIndex = 0x64;
            CmbPalettes.SelectedIndex = 0;
            CmbDefinitions.SelectedIndex = 0;
            BlsBlocks.SelectionChanged += new EventHandler<TEventArgs<MouseButtons>>(BlsBlocks_SelectionChanged);
            BlsBlocks.SelectedIndex = 0;
            solidityMap[0] = BlockProperty.Background;
            solidityMap[1] = BlockProperty.Foreground;
            solidityMap[2] = BlockProperty.Water;
            solidityMap[3] = BlockProperty.Water | BlockProperty.Foreground;
            solidityMap[4] = BlockProperty.SolidTop;
            solidityMap[5] = BlockProperty.SolidBottom;
            solidityMap[6] = BlockProperty.SolidAll;
            solidityMap[7] = BlockProperty.CoinBlock;
            for (int i = 0; i < 16; i++)
            {
                SpecialTypes.Add(((BlockProperty)(0xF0 | i)).ToString());
            }
        }

        private List<string> NotSolidInteractionTypes = new List<string>()
        {
            "No Interaction",
            "Harmful",
            "Deplete Air",
            "Current Left",
            "Current Right",
            "Current Up",
            "Current Down",
            "Treasure",
            "Locked Door",
            "Enemy Interaction",
            "Trap Activate",
            "Climbable",
            "Coin",
            "Door",
            "Cherry",
            "Power Coin"
        };
        private List<string> SolidInteractionTypes = new List<string>()
        {
            "No Interaction",
            "Harmful",
            "Slick",
            "Conveyor Left",
            "Conveyor Right",
            "Conveyor Up",
            "Conveyor Down",
            "Thin Ice",
            "Vertical Pipe Left",
            "Vertical Pipe Right",
            "Horizontal Pipe Bottom",
            "Climbable",
            "Enemy Interaction",
            "Stone",
            "PSwitch",
            "ESwitch",
        };

        private List<string> MapInteractionTypes = new List<string>()
        {
            "Boundary",
            "Traversable",
            "Enterable and Traversable",
            "Completable",
        };

        private List<string> SpecialTypes = new List<string>();

        private bool updating;
        private void UpdateInteractionSpecialList()
        {
            updating = true;
            if (CmbDefinitions.SelectedIndex == 0)
            {
                CmdInteraction.DataSource = MapInteractionTypes;
                LblSolidity.Visible = CmbSolidity.Visible = false;
            }
            else if (CmbSolidity.SelectedIndex >= 7 || CmbSolidity.SelectedIndex == 5)
            {
                CmdInteraction.DataSource = SpecialTypes;
                LblSolidity.Visible = CmbSolidity.Visible = true;
            }
            else if (CmbSolidity.SelectedIndex <= 3)
            {
                CmdInteraction.DataSource = NotSolidInteractionTypes;
                LblSolidity.Visible = CmbSolidity.Visible = true;
            }

            else
            {
                CmdInteraction.DataSource = SolidInteractionTypes;
                LblSolidity.Visible = CmbSolidity.Visible = true;
            }

            updating = false;
            int i = (int)(BlvCurrent.CurrentBlock.BlockProperty & BlockProperty.PowerCoin);
            if (i > CmdInteraction.Items.Count)
            {
                CmdInteraction.SelectedIndex = 0;
            }
            else
            {
                CmdInteraction.SelectedIndex = i;
            }
        }

        void BlsBlocks_SelectionChanged(object sender, TEventArgs<MouseButtons> e)
        {
            BlvCurrent.PaletteIndex = BlsBlocks.SelectedIndex / 0x40;
            BlvCurrent.CurrentBlock = BlsBlocks.SelectedBlock;
            PtvTable.PaletteIndex = BlsBlocks.SelectedIndex / 0x40;
            LblBlockSelected.Text = "Selected: " + BlsBlocks.SelectedIndex.ToHexString();
            if (BlvCurrent.CurrentBlock != null)
            {
                CmbSolidity.SelectedIndex = solidityMap.Values.ToList().IndexOf(BlvCurrent.CurrentBlock.BlockProperty & BlockProperty.MaskHi);
                if (CmbSolidity.SelectedIndex == -1)
                {
                    CmbSolidity.SelectedIndex = 0;
                }

                int b = (int)(BlvCurrent.CurrentBlock.BlockProperty & BlockProperty.PowerCoin);
                if (b > CmdInteraction.Items.Count)
                {
                    b = 0;
                }
                CmdInteraction.SelectedIndex = b;
                BlockDescription.Text = BlsBlocks.SelectedBlock.Description;
            }
        }

        private void CmbGraphics1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentTable.SetGraphicsbank(0, ProjectController.GraphicsManager.GraphicsBanks[CmbGraphics1.SelectedIndex]);
            CurrentTable.SetGraphicsbank(1, ProjectController.GraphicsManager.GraphicsBanks[CmbGraphics1.SelectedIndex + 1]);
            LblHexGraphics1.Text = "x" + CmbGraphics1.SelectedIndex.ToHexString();
        }

        private void CmbGraphics2_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentTable.SetGraphicsbank(2, ProjectController.GraphicsManager.GraphicsBanks[CmbGraphics2.SelectedIndex]);
            CurrentTable.SetGraphicsbank(3, ProjectController.GraphicsManager.GraphicsBanks[CmbGraphics2.SelectedIndex + 1]);
            LblHexGraphics2.Text = "x" + CmbGraphics2.SelectedIndex.ToHexString();
        }

        private void CmbPalettes_SelectedIndexChanged(object sender, EventArgs e)
        {
            PtvTable.CurrentPalette = CmbPalettes.SelectedItem as PaletteInfo;
            BlsBlocks.CurrentPalette = CmbPalettes.SelectedItem as PaletteInfo;
            PlsView.CurrentPalette = CmbPalettes.SelectedItem as PaletteInfo;
            BlvCurrent.CurrentPalette = CmbPalettes.SelectedItem as PaletteInfo;
        }

        private void CmbDefinitions_SelectedIndexChanged(object sender, EventArgs e)
        {

            BlsBlocks.CurrentDefiniton = ProjectController.BlockManager.GetDefiniton(CmbDefinitions.SelectedIndex);
            if (CmbDefinitions.SelectedIndex != 0)
            {
                FillBlockForTransitions();
                LoadBlockTransitions();
            }
        }

        private void FillBlockForTransitions()
        {


        }

        private void LoadBlockTransitions()
        {
            fireInteractionList.Text = string.Join(",", BlsBlocks.CurrentDefiniton.FireBallTransitions.Select(b => b.ToHexString()));
            iceInteractionList.Text = string.Join(",", BlsBlocks.CurrentDefiniton.IceBallTransitions.Select(b => b.ToHexString()));
            psF1.Value = BlsBlocks.CurrentDefiniton.PSwitchTransitions[0].FromValue;
            psF2.Value = BlsBlocks.CurrentDefiniton.PSwitchTransitions[1].FromValue;
            psF3.Value = BlsBlocks.CurrentDefiniton.PSwitchTransitions[2].FromValue;
            psF4.Value = BlsBlocks.CurrentDefiniton.PSwitchTransitions[3].FromValue;
            psF5.Value = BlsBlocks.CurrentDefiniton.PSwitchTransitions[4].FromValue;
            psF6.Value = BlsBlocks.CurrentDefiniton.PSwitchTransitions[5].FromValue;
            psF7.Value = BlsBlocks.CurrentDefiniton.PSwitchTransitions[6].FromValue;
            psF8.Value = BlsBlocks.CurrentDefiniton.PSwitchTransitions[7].FromValue;
            psT1.Value = BlsBlocks.CurrentDefiniton.PSwitchTransitions[0].ToValue;
            psT2.Value = BlsBlocks.CurrentDefiniton.PSwitchTransitions[1].ToValue;
            psT3.Value = BlsBlocks.CurrentDefiniton.PSwitchTransitions[2].ToValue;
            psT4.Value = BlsBlocks.CurrentDefiniton.PSwitchTransitions[3].ToValue;
            psT5.Value = BlsBlocks.CurrentDefiniton.PSwitchTransitions[4].ToValue;
            psT6.Value = BlsBlocks.CurrentDefiniton.PSwitchTransitions[5].ToValue;
            psT7.Value = BlsBlocks.CurrentDefiniton.PSwitchTransitions[6].ToValue;
            psT8.Value = BlsBlocks.CurrentDefiniton.PSwitchTransitions[7].ToValue;
        }

        private void CommitBlockTransitions()
        {

            var hexes = fireInteractionList.Text.Split(',');

            BlsBlocks.CurrentDefiniton.FireBallTransitions.Clear();
            for (var i = 0; i < 8; i++)
            {
                if (i < hexes.Length && hexes[i].Length > 0)
                {
                    BlsBlocks.CurrentDefiniton.FireBallTransitions.Add((byte)hexes[i].ToIntFromHex());
                }
                else
                {
                    BlsBlocks.CurrentDefiniton.FireBallTransitions.Add(0);
                }
            }


            hexes = iceInteractionList.Text.Split(',');
            BlsBlocks.CurrentDefiniton.IceBallTransitions.Clear();
            for (var i = 0; i < 8; i++)
            {
                if (i < hexes.Length && hexes[i].Length > 0)
                {
                    BlsBlocks.CurrentDefiniton.IceBallTransitions.Add((byte)hexes[i].ToIntFromHex());
                }
                else
                {
                    BlsBlocks.CurrentDefiniton.IceBallTransitions.Add(0);
                }
            }


            BlsBlocks.CurrentDefiniton.PSwitchTransitions[0].FromValue = (int)psF1.Value;
            BlsBlocks.CurrentDefiniton.PSwitchTransitions[1].FromValue = (int)psF2.Value;
            BlsBlocks.CurrentDefiniton.PSwitchTransitions[2].FromValue = (int)psF3.Value;
            BlsBlocks.CurrentDefiniton.PSwitchTransitions[3].FromValue = (int)psF4.Value;
            BlsBlocks.CurrentDefiniton.PSwitchTransitions[4].FromValue = (int)psF5.Value;
            BlsBlocks.CurrentDefiniton.PSwitchTransitions[5].FromValue = (int)psF6.Value;
            BlsBlocks.CurrentDefiniton.PSwitchTransitions[6].FromValue = (int)psF7.Value;
            BlsBlocks.CurrentDefiniton.PSwitchTransitions[7].FromValue = (int)psF8.Value;
            BlsBlocks.CurrentDefiniton.PSwitchTransitions[0].ToValue = (int)psT1.Value;
            BlsBlocks.CurrentDefiniton.PSwitchTransitions[1].ToValue = (int)psT2.Value;
            BlsBlocks.CurrentDefiniton.PSwitchTransitions[2].ToValue = (int)psT3.Value;
            BlsBlocks.CurrentDefiniton.PSwitchTransitions[3].ToValue = (int)psT4.Value;
            BlsBlocks.CurrentDefiniton.PSwitchTransitions[4].ToValue = (int)psT5.Value;
            BlsBlocks.CurrentDefiniton.PSwitchTransitions[5].ToValue = (int)psT6.Value;
            BlsBlocks.CurrentDefiniton.PSwitchTransitions[6].ToValue = (int)psT7.Value;
            BlsBlocks.CurrentDefiniton.PSwitchTransitions[7].ToValue = (int)psT8.Value;

        }

        private void BlvCurrent_MouseDown(object sender, MouseEventArgs e)
        {
            int x = e.X / 16;
            int y = e.Y / 16;
            if (x < 0 || y < 0 || x > 2 || y > 2) return;
            BlvCurrent.SetTile(x, y, (byte)PtvTable.SelectedIndex);
            BlvCurrent.Focus();
            BlsBlocks.UpdateSelection();
        }

        private void RdoNormal_CheckedChanged(object sender, EventArgs e)
        {
            PtvTable.ArrangementMode = ArrangementMode.Normal;
        }

        private void RdoMap16_CheckedChanged(object sender, EventArgs e)
        {
            PtvTable.ArrangementMode = ArrangementMode.Map16;
        }

        private void BtnSaveClose_Click(object sender, EventArgs e)
        {
            CommitBlockTransitions();
            ProjectController.BlockManager.SaveDefinitions(ProjectController.RootDirectory + @"\" + ProjectController.ProjectName + ".tsa");
            ProjectController.BlockManager.SaveBlockStrings(ProjectController.RootDirectory + @"\strings.xml");
            BlsBlocks.SelectionChanged -= BlsBlocks_SelectionChanged;
            this.Close();
        }

        private void ChkShowSpecials_CheckedChanged(object sender, EventArgs e)
        {
            BlsBlocks.ShowSpecialBlocks = ChkShowSpecials.Checked;
        }

        public void ShowDialog(int definitionIndex, int selectedTileIndex, int graphics1, int graphics2, int paletteIndex)
        {
            CmbDefinitions.SelectedIndex = definitionIndex;
            CmbGraphics1.SelectedIndex = graphics1;
            CmbGraphics2.SelectedIndex = graphics2;
            CmbPalettes.SelectedIndex = paletteIndex;
            BlsBlocks.SelectedTileIndex = selectedTileIndex;
            BlsBlocks.Focus();
            this.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        int PreviousBlockX, PreviousBlockY;
        private void BlsBlocks_MouseMove(object sender, MouseEventArgs e)
        {
            int x = e.X / 16;
            int y = e.Y / 16;

            if (PreviousBlockX == x && PreviousBlockY == y) return;
            PreviousBlockX = x;
            PreviousBlockY = y;
            int index = (y * 16) + x;
            int defIndex = CmbDefinitions.SelectedIndex;

            if (index > -1 && index < 256)
            {
                if (defIndex == 0)
                {
                    TSAToolTip.SetToolTip(BlsBlocks, ProjectController.BlockManager.GetBlockString(defIndex, index));
                }

                else
                {
                    TSAToolTip.SetToolTip(BlsBlocks, ProjectController.BlockManager.GetBlockString(defIndex, index));
                }
            }
        }

        private void BlsBlocks_MouseDown(object sender, MouseEventArgs e)
        {
            BlsBlocks.Focus();


        }

        Block copyBlock = null;
        private void BlsBlocks_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.C && (e.Modifiers == Keys.Control || (e.Modifiers == (Keys.Shift | Keys.Control))))
            {
                copyBlock = BlsBlocks.SelectedBlock;
            }
            else if (e.KeyCode == Keys.V && (e.Modifiers & Keys.Control) > Keys.None)
            {

                if (copyBlock == null)
                {
                    return;
                }
                if ((e.Modifiers & Keys.Shift) == Keys.None)
                {
                    BlvCurrent.SetTile(0, 0, copyBlock[0, 0]);
                    BlvCurrent.SetTile(1, 0, copyBlock[1, 0]);
                    BlvCurrent.SetTile(0, 1, copyBlock[0, 1]);
                    BlvCurrent.SetTile(1, 1, copyBlock[1, 1]);
                    BlockDescription.Text = BlvCurrent.CurrentBlock.Description = copyBlock.Description;
                }

                BlvCurrent.CurrentBlock.BlockProperty = copyBlock.BlockProperty;
                BlsBlocks_SelectionChanged(null, null);
            }
        }

        private void BtnApplyGlobally_Click(object sender, EventArgs e)
        {
            ConfirmForm cForm = new ConfirmForm();
            cForm.StartPosition = FormStartPosition.CenterParent;
            cForm.Owner = ReubenController.MainWindow;

            if (cForm.Confirm("Are you sure you want to apply this definiton to block " + BlsBlocks.SelectedTileIndex.ToHexString() + " in every definition set?"))
            {
                bool first = true;
                foreach (BlockDefinition bDef in ProjectController.BlockManager.AllDefinitions)
                {
                    if (first)
                    {
                        first = false;
                        continue;
                    }
                    Block b = bDef[BlsBlocks.SelectedTileIndex];
                    b[0, 0] = BlvCurrent.CurrentBlock[0, 0];
                    b[0, 1] = BlvCurrent.CurrentBlock[0, 1];
                    b[1, 0] = BlvCurrent.CurrentBlock[1, 0];
                    b[1, 1] = BlvCurrent.CurrentBlock[1, 1];
                    b.Description = BlockDescription.Text;
                    b.BlockProperty = BlvCurrent.CurrentBlock.BlockProperty;
                }
            }
        }

        int PreviousTileX, PreviousTileY;
        private void PtvTable_MouseMove(object sender, MouseEventArgs e)
        {
            int x = e.X / 16;
            int y = e.Y / 16;
            if (PreviousTileX == x && PreviousTileY == y) return;
            PreviousTileX = x;
            PreviousTileY = y;
            TSAToolTip.SetToolTip(PtvTable, ((y * 16) + x).ToHexString());
        }

        private void ChkBlockProperties_CheckedChanged(object sender, EventArgs e)
        {
            BlsBlocks.ShowBlockSolidity = ChkBlockProperties.Checked;
        }

        private void SpecialList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!updating)
            {
                if (CmbDefinitions.SelectedIndex == 0)
                {
                    BlsBlocks.SelectedBlock.BlockProperty = (BlockProperty)CmdInteraction.SelectedIndex;
                    BlsBlocks.UpdateSelection();
                }
                else
                {
                    BlsBlocks.SelectedBlock.BlockProperty = (BlvCurrent.CurrentBlock.BlockProperty & BlockProperty.MaskHi) | (BlockProperty)CmdInteraction.SelectedIndex;
                    BlsBlocks.UpdateSelection();
                }
            }
        }

        private void BlockDescription_TextChanged(object sender, EventArgs e)
        {
            BlsBlocks.SelectedBlock.Description = BlockDescription.Text;
        }

        private void CmbSolidity_SelectedIndexChanged(object sender, EventArgs e)
        {
            BlsBlocks.SelectedBlock.BlockProperty = solidityMap[CmbSolidity.SelectedIndex] | (BlvCurrent.CurrentBlock.BlockProperty & BlockProperty.PowerCoin);
            UpdateInteractionSpecialList();
            BlsBlocks.UpdateSelection();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CommitBlockTransitions();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (Block b in BlsBlocks.CurrentDefiniton.BlockList)
            {
                b.BlockProperty = BlockProperty.Background;
                b.Description = "";
                b[0, 0] = 0xEE;
                b[0, 1] = 0xFF;
                b[1, 0] = 0xFF;
                b[1, 1] = 0xEE;
            }
        }

        private void ChkShowInteractions_CheckedChanged(object sender, EventArgs e)
        {
            BlsBlocks.ShowTileInteractions = ChkShowInteractions.Checked;
        }

        private void BlsBlocks_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                Block copyBlock = Block.Deserialize(Clipboard.GetText());
                {
                    if (copyBlock != null)
                    {

                        BlvCurrent.SetTile(0, 0, copyBlock[0, 0]);
                        BlvCurrent.SetTile(1, 0, copyBlock[1, 0]);
                        BlvCurrent.SetTile(0, 1, copyBlock[0, 1]);
                        BlvCurrent.SetTile(1, 1, copyBlock[1, 1]);
                        BlockDescription.Text = BlvCurrent.CurrentBlock.Description = copyBlock.Description;
                        BlvCurrent.CurrentBlock.BlockProperty = copyBlock.BlockProperty;
                    }

                    BlsBlocks_SelectionChanged(null, null);

                    Clipboard.Clear();
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (BlsBlocks.SelectedBlock != null)
                {
                    Clipboard.SetText(Block.Serialize(BlsBlocks.SelectedBlock));
                }
            }
        }
    }
}

