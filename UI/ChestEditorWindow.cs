﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MinishMaker.Core;
using MinishMaker.Core.ChangeTypes;
using MinishMaker.Utilities;

namespace MinishMaker.UI
{
	public partial class ChestEditorWindow : Form
	{
		private int chestIndex = -1;
		private List<RoomMetaData.ChestData> chestDataList;
		
		
		public ChestEditorWindow()
		{
			InitializeComponent();
			itemName.DropDownStyle = ComboBoxStyle.DropDownList;
			kinstoneType.DropDownStyle = ComboBoxStyle.DropDownList;
			this.itemName.DataSource = Enum.GetValues(typeof(ItemType));
			this.kinstoneType.DataSource = Enum.GetValues(typeof(KinstoneType));
			chestIndex = -1;
			
			if(chestDataList==null)
			{
				//lock all stuff
				entityType.Enabled = false;
				entityId.Enabled = false;
				itemName.Enabled = false;
				kinstoneType.Enabled =false;
				itemAmount.Enabled = false;
				xPosition.Enabled = false;
				yPosition.Enabled = false;
				nextButton.Enabled = false;
				prevButton.Enabled = false;
				newButton.Enabled = false;
				removeButton.Enabled = false;
			}
		}

		public void SetData(List<RoomMetaData.ChestData> data)
		{
			this.chestDataList = data;
											 
			prevButton.Enabled = false;
			nextButton.Enabled = false;
			newButton.Enabled = true;

			if(data.Count == 0)
			{
				chestIndex = -1;
				entityType.Enabled = false;
				entityId.Enabled = false;
				itemName.Enabled = false;
				kinstoneType.Enabled =false;
				itemAmount.Enabled = false;
				xPosition.Enabled = false;
				yPosition.Enabled = false;
				nextButton.Enabled = false;
				removeButton.Enabled = false;
			}
            else
            {
                entityType.Enabled = true;
                entityId.Enabled = true;
                itemName.Enabled = true;
                kinstoneType.Enabled = true;
                itemAmount.Enabled = true;
                xPosition.Enabled = true;
                yPosition.Enabled = true;
				removeButton.Enabled = true;
                chestIndex = 0;

                LoadChestData(0);

				if(data.Count>1)
				{
					nextButton.Enabled = true;
				}
            }

            if (chestIndex >= 0)
            {
                indexLabel.Text = StringUtil.AsStringHex2(chestIndex);
            }
            else
            {
                indexLabel.Text = "";
            }
        }

  
		private void nextButton_Click( object sender, EventArgs e )
		{

            chestIndex++;
			prevButton.Enabled=true;
			if(chestIndex == chestDataList.Count-1)
			{
				nextButton.Enabled = false;
			}

            if (chestIndex >= 0)
            {
                indexLabel.Text = StringUtil.AsStringHex2(chestIndex);
            }
            else
            {
                indexLabel.Text = "";
            }

            LoadChestData(chestIndex);
        }

		private void prevButton_Click( object sender, EventArgs e )
		{
			chestIndex--;
			nextButton.Enabled = true;			 
			if(chestIndex == 0)
			{
				prevButton.Enabled = false;
			}

            if (chestIndex >= 0)
            {
                indexLabel.Text = StringUtil.AsStringHex2(chestIndex);
            }
            else
            {
                indexLabel.Text = "";
            }

            LoadChestData(chestIndex);
		}

		private void newButton_Click( object sender, EventArgs e )
		{
            if (chestDataList == null)
            {
                return;
            }

            chestIndex = chestDataList.Count;

            indexLabel.Text = StringUtil.AsStringHex2(chestIndex);

            Project.Instance.AddPendingChange(new ChestDataChange(MainWindow.currentArea, MainWindow.currentRoom.Index));
            MainWindow.currentRoom.AddChestData(new RoomMetaData.ChestData(0x02, 0, 0, 0, 0, 0));
            LoadChestData(chestIndex);

            // Don't enable the previous button if there weren't previously any chests
            if (chestDataList.Count > 1)
            {
                prevButton.Enabled = true;
            }

            nextButton.Enabled = false;
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (chestDataList == null || chestDataList.Count <= 0)
            {
                return;
            }

            Project.Instance.AddPendingChange(new ChestDataChange(MainWindow.currentArea, MainWindow.currentRoom.Index));
            MainWindow.currentRoom.RemoveChestData(chestDataList[chestIndex]);

			newButton.Enabled=true;

            if (chestDataList.Count <= 0)
            {
                chestIndex = -1;
                entityType.Enabled = false;
                entityId.Enabled = false;
                itemName.Enabled = false;
                kinstoneType.Enabled = false;
                itemAmount.Enabled = false;
                xPosition.Enabled = false;
                yPosition.Enabled = false;
                nextButton.Enabled = false;
                prevButton.Enabled = false;
				removeButton.Enabled = false;
            }

            if (chestIndex >= chestDataList.Count)
            {
                chestIndex = chestDataList.Count - 1;
            }

            if (chestIndex < chestDataList.Count - 1)
            {
                nextButton.Enabled = true;
            }
            else
            {
                nextButton.Enabled = false;
            }

            if (chestIndex > 0)
            {
                prevButton.Enabled = true;
            }
            else
            {
                prevButton.Enabled = false;
            }

            if (chestIndex >= 0)
            {
                indexLabel.Text = StringUtil.AsStringHex2(chestIndex);
                LoadChestData(chestIndex);
            }
            else
            {
                indexLabel.Text = "";
				((MainWindow)Application.OpenForms[0]).HighlightChest(-1,-1);
            }
        }

        private void LoadChestData(int chest)
        {
            RoomMetaData.ChestData chestData = chestDataList[chest];
            entityType.Text = StringUtil.AsStringHex2(chestData.type);

            if ((TileEntityType)chestData.type == TileEntityType.Chest || (TileEntityType)chestData.type == TileEntityType.BigChest)
            {
                entityId.Text = StringUtil.AsStringHex2(chestData.chestId);
                itemName.SelectedItem = (ItemType)chestData.itemId;
                kinstoneType.SelectedItem = (KinstoneType)chestData.itemSubNumber;
                itemAmount.Text = chestData.itemSubNumber.ToString();

                ushort chestPos = chestData.chestLocation;
				int yPos = chestPos>>6;
				int xPos = chestPos - (yPos<<6);
                xPosition.Text = xPos.Hex();
                yPosition.Text = yPos.Hex();
				((MainWindow)Application.OpenForms[0]).HighlightChest(xPos,yPos);
                itemName.Enabled = true;
                kinstoneType.Enabled = true;
                itemAmount.Enabled = true;
                xPosition.Enabled = true;
                yPosition.Enabled = true;
                entityId.Enabled = true;
                unknown.Text = chestData.unknown.Hex();
            }
			else
			{
                entityId.Text = "00";
                itemName.SelectedItem = ItemType.Untyped;
                kinstoneType.SelectedItem = KinstoneType.UnTyped;
                itemAmount.Text = "0";
                xPosition.Text = "0";
                yPosition.Text = "0";
				((MainWindow)Application.OpenForms[0]).HighlightChest(-1,-1);
                itemName.Enabled = false;
				kinstoneType.Enabled = false;
				itemAmount.Enabled = false;
				xPosition.Enabled = false;
				yPosition.Enabled = false;
				entityId.Enabled = false;
            }
        }

		private void entityType_LostFocus( object sender, EventArgs e )												
		{
			if(!entityType.IsHandleCreated)
				return;		 

			var chest = chestDataList[chestIndex];
			try
			{
				var type = Convert.ToByte(entityType.Text,16);

				if(type == chest.type)
					return;

                Project.Instance.AddPendingChange(new ChestDataChange(MainWindow.currentArea, MainWindow.currentRoom.Index));

                chest.type = type;
			}
			catch
			{
				entityType.Text = StringUtil.AsStringHex2(chest.type);
			}

			chestDataList[chestIndex] = chest;
		}

		private void entityId_LostFocus( object sender, EventArgs e )
		{
			if(!entityId.IsHandleCreated)
				return;

			var chest = chestDataList[chestIndex];
			try
			{
				var chestId = Convert.ToByte(entityId.Text,16);

				if(chestId == chest.chestId)
					return;

                Project.Instance.AddPendingChange(new ChestDataChange(MainWindow.currentArea, MainWindow.currentRoom.Index));

                chest.chestId = chestId;
			}
			catch
			{
				entityId.Text = StringUtil.AsStringHex2(chest.chestId);		  
			}
			chestDataList[chestIndex] = chest;
		}

		private void xPosition_LostFocus( object sender, EventArgs e )
		{
			if(!xPosition.IsHandleCreated)
				return;

			var chest = chestDataList[chestIndex];
			try
			{
				var lowLoc = Convert.ToByte(xPosition.Text,16);
				var hiLoc = Convert.ToByte(yPosition.Text,16);
				ushort location = (ushort)(lowLoc+(hiLoc<<6));

				if(location == chest.chestLocation)
					return;

                Project.Instance.AddPendingChange(new ChestDataChange(MainWindow.currentArea, MainWindow.currentRoom.Index));

                chest.chestLocation = location;
			}
			catch
			{
				var yPos = (chest.chestLocation>>6);
				var xPos = (chest.chestLocation- (yPos<<6));
				xPosition.Text = xPos.Hex();
			}
			chestDataList[chestIndex] = chest;
		}

		private void yPosition_LostFocus( object sender, EventArgs e )
		{
			if(!yPosition.IsHandleCreated)
				return;

			var chest = chestDataList[chestIndex];
			try
			{
				var lowLoc = Convert.ToByte(xPosition.Text,16);
				var hiLoc = Convert.ToByte(yPosition.Text,16);
				ushort location = (ushort)(lowLoc + (hiLoc<<6));

				if(location == chest.chestLocation)
					return;

                Project.Instance.AddPendingChange(new ChestDataChange(MainWindow.currentArea, MainWindow.currentRoom.Index));

                chest.chestLocation = location;
			}
			catch
			{
				var yPos = (chest.chestLocation>>6);
				yPosition.Text = yPos.Hex();
			}
			
			chestDataList[chestIndex] = chest;
		}

		private void kinstoneType_SelectedIndexChanged( object sender, EventArgs e )
		{
			if(!kinstoneType.IsHandleCreated)
				return;

			var chest = chestDataList[chestIndex];
   
			var type = (byte)((int)kinstoneType.SelectedValue);//cant directly go to byte?
	
			if(type == chest.itemSubNumber)
				return;

            Project.Instance.AddPendingChange(new ChestDataChange(MainWindow.currentArea, MainWindow.currentRoom.Index));

            itemAmount.Text = type.ToString();
			chest.itemSubNumber = type;

			chestDataList[chestIndex] = chest;
		}

		private void itemAmount_LostFocus( object sender, EventArgs e )
		{
			if(!itemAmount.IsHandleCreated)
				return;

			var chest = chestDataList[chestIndex];
			try
			{
				var type = Convert.ToByte(itemAmount.Text);

				if(type == chest.itemSubNumber)
					return;

                Project.Instance.AddPendingChange(new ChestDataChange(MainWindow.currentArea, MainWindow.currentRoom.Index));

                chest.itemSubNumber = type;
				kinstoneType.SelectedValue = (KinstoneType)type;
			}
			catch
			{
				itemAmount.Text = chest.itemSubNumber.ToString();
			}

			chestDataList[chestIndex] = chest;
		}

		private void itemName_SelectedIndexChanged( object sender, EventArgs e )
		{
            if(!itemName.IsHandleCreated)
				return;

			ItemType value = (ItemType)itemName.SelectedValue;

			var chest = chestDataList[chestIndex];

			if((int)value == chest.itemId)
				return;

            Project.Instance.AddPendingChange(new ChestDataChange(MainWindow.currentArea, MainWindow.currentRoom.Index));

            chest.itemId = (byte)value;
			chestDataList[chestIndex]=chest;

			amountLabel.Hide();
			itemAmount.Hide();
			kinstoneLabel.Hide();
			kinstoneType.Hide();

			if(value == ItemType.KinstoneX)
			{
				kinstoneLabel.Show();
				kinstoneType.Show();
			}
			else if(value == ItemType.ShellsX)
			{
				amountLabel.Show();
				itemAmount.Show();
			}
		}

        private void unknown_LostFocus(object sender, EventArgs e)
        {
            if (!unknown.IsHandleCreated)
                return;

            var chest = chestDataList[chestIndex];
            try
            {
                var extraval = Convert.ToByte(unknown.Text);

                if (extraval == chest.itemSubNumber)
                    return;

                Project.Instance.AddPendingChange(new ChestDataChange(MainWindow.currentArea, MainWindow.currentRoom.Index));

                chest.unknown = extraval;
            }
            catch
            {
                unknown.Text = chest.unknown.Hex();
            }

            chestDataList[chestIndex] = chest;
        }
    }
}