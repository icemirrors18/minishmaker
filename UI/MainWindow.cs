﻿using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using MinishMaker.Core;
using MinishMaker.Core.ChangeTypes;
using MinishMaker.Properties;
using MinishMaker.Utilities;


namespace MinishMaker.UI
{
	public partial class MainWindow : Form
	{   
        private Project project_;

		private MapManager mapManager_;
	    private NewProjectWindow newProjectWindow = null;
        private ChestEditorWindow chestEditor = null;
		private MetaTileEditor metatileEditor = null;
		private AreaEditor areaEditor = null;
		//private EnemyPlacementEditor enemyPlacementEditor = null;
		private WarpEditor warpEditor = null;
		private ObjectPlacementEditor objectPlacementEditor = null;

		private Bitmap[] mapLayers;
		private Bitmap[] tileMaps;

        public static Room currentRoom = null;
		public static int currentArea = -1;
		private int selectedTileData = -1;
		private int selectedLayer = 2; //start with bg2
        private Point lastTilePos;
	    private ViewLayer viewLayer = 0;

        struct RepointData
		{
			public int areaIndex;
			public int roomIndex;
			public DataType type;
			public int start;
			public int size;


			public RepointData( int areaIndex, int roomIndex, DataType type, int start, int size )
			{
				this.areaIndex = areaIndex;
				this.roomIndex = roomIndex;
				this.type = type;
				this.start = start;
				this.size = size;
			}
		}
		

	    public enum ViewLayer
	    {
            Both,
            Top,
            Bottom
	    }

		public MainWindow()
        {
            InitializeComponent();
            UpdateWindowTitle();
            /*
			var exeFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Substring(6);
			if(File.Exists(exeFolder+"/Settings.cfg"))
			{
				var settings = File.ReadLines(exeFolder+"/Settings.cfg").ToList();
				project_ = new Project();

				var romFile = settings.Single(x=>x.Contains("romFile")).Split('=')[1];
				var projectFolder = settings.Single(x=>x.Contains("projectFolder")).Split('=')[1];

				mapGridBox.Image = new Bitmap(1,1); //reset some things on loading a rom
				bottomTileGridBox.Image = new Bitmap(1,1);
				topTileGridBox.Image = new Bitmap(1, 1);
				currentRoom = null;
				currentArea = -1;
				selectedTileData = -1;
				selectedLayer = 2; 
				pendingRomChanges = new List<Change>();
				project_.LoadProject();
				LoadMaps();

				var pName = new DirectoryInfo(projectFolder).Name;
				statusText.Text = "Opened last project: "+pName;
			}
            */
        }

        private void UpdateWindowTitle()
        {
            string title = ProductName;

#if DEBUG
            this.Text = $"{ProductName} {AssemblyInfo.GetGitTag()} DEBUG-{AssemblyInfo.GetGitHash()}";
#else
            this.Text = $"{ProductName} {AssemblyInfo.GetGitTag()}";
#endif
        }

        #region MenuBarButtons

        private void NewProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewProject();
        }

        private void OpenProjectToolStripMenuItem_Click( object sender, EventArgs e )
		{
            OpenProject();
		}

        private void saveAllChangesCtrlSToolStripMenuItem_Click(object sender, EventArgs e)
	    {
            SaveAllChanges();
	    }

        private void BuildProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BuildProject();
        }

        private void ExitButtonClick( object sender, EventArgs e )
		{
			Close();
		}

	    private void topLayerToolStripMenuItem_Click(object sender, EventArgs e)
	    {
	        UpdateViewLayer(ViewLayer.Top);
	    }

	    private void bottomLayerToolStripMenuItem_Click(object sender, EventArgs e)
	    {
	        UpdateViewLayer(ViewLayer.Bottom);
	    }

	    private void bothLayersToolStripMenuItem_Click(object sender, EventArgs e)
	    {
	        UpdateViewLayer(ViewLayer.Both);
	    }

	    private void chestEditorStripMenuItem_Click(object sender, EventArgs e)
	    {
	        OpenChestEditor();
	    }

        private void metatileEditorToolStripMenuItem_Click(object sender, EventArgs e)
	    {
	        OpenMetatileEditor();
	    }

	    private void areaEditorToolStripMenuItem_Click(object sender, EventArgs e)
	    {
            OpenAreaEditor();
	    }

		private void warpEditorToolStripMenuItem_Click( object sender, EventArgs e )
		{
			OpenWarpEditor();
		}

		private void objectPlacementEditorToolStripMenuItem_Click( object sender, EventArgs e )
		{
			OpenObjectPlacementEditor(); 
		}

        private void AboutButtonClick( object sender, EventArgs e )
		{
			Form aboutWindow = new AboutWindow();
			aboutWindow.Show();
		}
#endregion

#region ToolStripButtons
        private void openToolStripButton_Click( object sender, EventArgs e )
		{
			OpenProject();
		}

	    private void saveToolStripButton_Click(object sender, EventArgs e)
	    {
            SaveAllChanges();
	    }

	    private void chestToolStripButton_Click(object sender, EventArgs e)
	    {
	        OpenChestEditor();
	    }

	    private void metatileToolStripButton_Click(object sender, EventArgs e)
	    {
	        OpenMetatileEditor();
	    }

	    private void areaToolStripButton_Click(object sender, EventArgs e)
	    {
            OpenAreaEditor();
	    }

        private void objectToolStripButton_Click(object sender, EventArgs e)
        {
            OpenObjectPlacementEditor();
        }

        private void warpToolStripButton_Click(object sender, EventArgs e)
        {
            OpenWarpEditor();
        }

#endregion

#region OtherInteractions

        // Other interactions
        private void tileTabControl_SelectedIndexChanged(object sender, EventArgs e)
	    {
	        selectedLayer = tileTabControl.SelectedIndex + 1;

	    }

        private void MainWindow_DragDrop( object sender, DragEventArgs e )
		{

		}
#endregion

#region ProjectManagement

        private void NewProject()
        {
            if (newProjectWindow != null)
                return;

            newProjectWindow = new NewProjectWindow();
            newProjectWindow.FormClosed += OnNewProjectWindowClosed;
            newProjectWindow.Show();
            /*
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "GBA ROMs|*.gba|All Files|*.*",
                Title = "Select Base TMC ROM"
            };

            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                ROM_ = new ROM(ofd.FileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            if (ROM.Instance.version.Equals(RegionVersion.None))
            {
                MessageBox.Show("Invalid TMC ROM. Please Open a valid ROM.", "Incorrect ROM", MessageBoxButtons.OK);
                statusText.Text = "Unable to determine ROM.";
                return;
            }

            CommonOpenFileDialog fbd = new CommonOpenFileDialog()
            {

                IsFolderPicker = true,
                Title = "Select a project root folder"
            };

            if (fbd.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return;
            }

            if (project_ == null)
            {
                project_ = new Project();
            }

            project_.sourcePath = ROM.Instance.path;
            var st = "Loaded: " + ROM.Instance.path;

            if (project_.projectPath != null)
            {

                project_.LoadProject();//load first as rooms or areas could be added at some point
                mapGridBox.Image = new Bitmap(1, 1); //reset some things on loading a rom
                bottomTileGridBox.Image = new Bitmap(1, 1);
                topTileGridBox.Image = new Bitmap(1, 1);
                currentRoom = null;
                currentArea = -1;
                selectedTileData = -1;
                selectedLayer = 2;
                pendingRomChanges = new List<Change>();
                LoadMaps();
            }
            else
            {
                st += ", also select a project folder.";
            }

            statusText.Text = st;*/
        }

	    private void OnNewProjectWindowClosed(object sender, FormClosedEventArgs e)
	    {
	        if (newProjectWindow.project != null)
            {
                project_ = newProjectWindow.project;
                if (project_.Loaded)
                {
                    LoadProjectData();
                    statusText.Text = "Created new project: " + project_.projectPath + "/" + project_.projectName + ".mmproj";
                }
                else
                    statusText.Text = "Could not load project.";
            }
	        else
	            statusText.Text = "Project creation aborted.";
	        newProjectWindow = null;
	    }

		private void OpenProject()
		{
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Minish Maker Project|*.mmproj|All Files|*.*",
                Title = "Select Project File"
            };

            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

			if(project_== null)
			{
				project_ = new Project(ofd.FileName);
			}

			if(project_.Loaded)
            {
                LoadProjectData();
            }

            statusText.Text = "Loaded: " + ofd.FileName;
		}

        private void LoadProjectData()
        {
            mapGridBox.Image = new Bitmap(1, 1); //reset some things on loading a rom
            bottomTileGridBox.Image = new Bitmap(1, 1);
            topTileGridBox.Image = new Bitmap(1, 1);
            currentRoom = null;
            currentArea = -1;
            selectedTileData = -1;
            selectedLayer = 2;
            LoadMaps();
        }

        private void BuildProject()
        {
            if (Project.Instance == null)
            {
                MessageBox.Show("No project loaded!");
                return;
            }
            // TODO check for pending changes before building, and prompt user

            if (project_.BuildProject())
            {
                MessageBox.Show("Build Completed!");
                statusText.Text = "Build Completed. Output file: " + project_.projectPath + "\\" + project_.projectName + ".gba";
            }
            else
            {
                MessageBox.Show("There was a problem building the project.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusText.Text = "Failed to build project.";
            }
            // TODO check for build completing correctly, probably needs deeper integration with ColorzCore
   
        }

#endregion

        private void LoadMaps()
        {
            mapManager_ = new MapManager();

            roomTreeView.Nodes.Clear();
            // Set up room list
            roomTreeView.BeginUpdate();
            int subsection = 0;

            foreach (MapManager.Area area in mapManager_.MapAreas)
            {
                roomTreeView.Nodes.Add("Area " + StringUtil.AsStringHex2(area.Index));

                foreach (Room room in area.Rooms)
                {
                    roomTreeView.Nodes[subsection].Nodes.Add("Room " + StringUtil.AsStringHex2(room.Index));
                }

                subsection++;
            }

            roomTreeView.EndUpdate();
        }

        private void OpenChestEditor()
	    {
	        if (chestEditorStripMenuItem.Checked)
	            return; // dont open a second one

	        chestEditor = new ChestEditorWindow();

	        if (currentRoom != null)
	        {
	            var chestData = currentRoom.GetChestData();
	            chestEditor.SetData(chestData);
	        }
	        chestEditor.FormClosed += new FormClosedEventHandler(OnChestEditorClose);
	        chestEditorStripMenuItem.Checked = true;
	        chestEditor.Show();
	    }

	    private void OnChestEditorClose(object sender, FormClosedEventArgs e)
	    {
	        chestEditor = null;
	        chestEditorStripMenuItem.Checked = false;
			HighlightChest(-1,-1);
	    }

	    private void OpenMetatileEditor()
	    {
	        if (metatileEditorToolStripMenuItem.Checked)
	            return; // dont open a second one

	        metatileEditor = new MetaTileEditor();

	        if (currentRoom != null)
	        {
				metatileEditor.currentArea = currentArea;
				var room = MapManager.Instance.MapAreas.Single(a=>a.Index==currentArea).Rooms.First();
				if(!room.Loaded)
				{
					room.LoadRoom(currentArea);
				}
	            metatileEditor.RedrawTiles(room);
	        }

	        metatileEditor.FormClosed += new FormClosedEventHandler(OnMetaTileEditorClose);
	        metatileEditorToolStripMenuItem.Checked = true;
	        metatileEditor.Show();
	    }

	    private void OnMetaTileEditorClose(object sender, FormClosedEventArgs e)
	    {
	        metatileEditor = null;
	        metatileEditorToolStripMenuItem.Checked = false;
	    }

	    private void OpenAreaEditor()
	    {
	        if (areaEditorToolStripMenuItem.Checked)
	            return;

	        areaEditor = new AreaEditor();

	        if (currentRoom != null)
	        {
	            areaEditor.LoadArea(currentArea);
	        }

	        areaEditor.FormClosed += new FormClosedEventHandler(OnAreaEditorClose);
	        areaEditorToolStripMenuItem.Checked = true;
	        areaEditor.Show();
        }

		private void OnAreaEditorClose(object sender, FormClosedEventArgs e)
		{
			areaEditor = null;
			areaEditorToolStripMenuItem.Checked = false;
		}

		private void OpenWarpEditor()
		{
			if(warpEditorToolStripMenuItem.Checked)
				return; // dont open a second one

			warpEditor = new WarpEditor();
			warpEditor.FormClosed += new FormClosedEventHandler(OnWarpEditorClose);
	        warpEditorToolStripMenuItem.Checked = true;
	        warpEditor.Show();
		}

		private void OnWarpEditorClose(object sender, FormClosedEventArgs e)
		{
			warpEditor = null;
			warpEditorToolStripMenuItem.Checked = false;
			HighlightWarp(-1,-1);
		}

		private void OpenObjectPlacementEditor()
		{
			if(objectPlacementEditorToolStripMenuItem.Checked)
				return; // dont open a second one

			objectPlacementEditor = new ObjectPlacementEditor();
			objectPlacementEditor.FormClosed += new FormClosedEventHandler(OnObjectPlacementEditorClose);
	        objectPlacementEditorToolStripMenuItem.Checked = true;
	        objectPlacementEditor.Show();
		}

		private void OnObjectPlacementEditorClose(object sender, FormClosedEventArgs e)
		{
			objectPlacementEditor = null;
			objectPlacementEditorToolStripMenuItem.Checked = false;
			HighlightListObject(-1,-1);
		}

        private void roomTreeView_NodeMouseDoubleClick( object sender, TreeNodeMouseClickEventArgs e )
		{
			if( e.Node.Parent != null )
			{
				Console.WriteLine( e.Node.Parent.Text.Split( ' ' )[1] + " " + e.Node.Text.Split( ' ' )[1] );
				int areaIndex = Convert.ToInt32( e.Node.Parent.Text.Split( ' ' )[1], 16 );
				int roomIndex = Convert.ToInt32( e.Node.Text.Split( ' ' )[1], 16 );
				var prevArea = currentArea; //changed in next line so hold temporarily
				var room = FindRoom( areaIndex, roomIndex );

				currentRoom = room;

				mapLayers = room.DrawRoom( areaIndex, true, true );

				selectedTileData = -1;
			    tileTabControl.SelectedIndex = 1; // Reset to bg2

				//0= bg1 (treetops and such)
				//1= bg2 (flooring)
				mapGridBox.Image = OverlayImage( mapLayers[1], mapLayers[0] );
				tileMaps = room.DrawTilesetImages( 16, currentArea );
				bottomTileGridBox.Image = tileMaps[1];
                topTileGridBox.Image = tileMaps[0];

                mapGridBox.Selectable = true;
				mapGridBox.SelectedIndex = -1;
                bottomTileGridBox.Selectable = true;
                topTileGridBox.Selectable = true;

                if (chestEditor != null)
                {
                    var chestData = currentRoom.GetChestData();
                    chestEditor.SetData(chestData);
                }

				if(metatileEditor != null)
				{
					metatileEditor.currentArea = currentArea;
					room = MapManager.Instance.MapAreas.Single(a=>a.Index==currentArea).Rooms.First();
					if(!room.Loaded)
					{
						room.LoadRoom(currentArea);
					}
					metatileEditor.RedrawTiles(currentRoom);
				}

				if(areaEditor != null&& currentArea != prevArea)//still in the same area? dont reload
				{
					areaEditor.LoadArea(areaIndex);
				}

				/*if(enemyPlacementEditor != null)
				{
					enemyPlacementEditor.LoadData();
				}*/

				if(warpEditor != null)
				{
					warpEditor.LoadData();
				}

				if(objectPlacementEditor != null)
				{
					objectPlacementEditor.LoadData();
				}
            }
		}

		public Bitmap OverlayImage( Bitmap baseImage, Bitmap overlay )
		{
			Bitmap finalImage = new Bitmap( baseImage.Width, baseImage.Height );

			using( Graphics g = Graphics.FromImage( finalImage ) )
			{
				//set background color
				g.Clear( Color.Black );

				g.DrawImage( baseImage, new Rectangle( 0, 0, baseImage.Width, baseImage.Height ) );
				g.DrawImage( overlay, new Rectangle( 0, 0, baseImage.Width, baseImage.Height ) );
			}
			//Draw the final image in the gridBox
			return finalImage;
		}

		private Room FindRoom( int areaIndex, int roomIndex )
		{
			int foundIndex = 0;

			currentArea = areaIndex;
			for( int i = 0; i < mapManager_.MapAreas.Count; i++ )
			{
				if( mapManager_.MapAreas[i].Index == areaIndex )
				{
					foundIndex = i;
					break;
				}
				if( i == mapManager_.MapAreas.Count - 1 )
				{
					throw new Exception( "Could not find any area with index: " + areaIndex.Hex() );
				}
			}

			var area = mapManager_.MapAreas[foundIndex];
			for( int j = 0; j < area.Rooms.Count(); j++ )
			{
				if( area.Rooms[j].Index == roomIndex )
				{
					foundIndex = j;
					break;
				}
				if( j == area.Rooms.Count - 1 )
				{
					throw new Exception( "Could not find any room with index: " + roomIndex.Hex() + " in area: " + areaIndex.Hex() );
				}
			}

			return area.Rooms[foundIndex];
		}

        private void SaveAllChanges()
        {
			if(Project.Instance==null)
				return;

            Project.Instance.StartSave();
			Project.Instance.Save();
            Project.Instance.EndSave();

            MessageBox.Show("Project Saved");
        }

        public void HighlightChest(int tileX, int tileY)
		{
			mapGridBox.chestHighlightPoint = new Point(tileX,tileY);
			mapGridBox.Invalidate();
		}

		public void HighlightListObject(int pixelX, int pixelY)
		{
			mapGridBox.listObjectHighlightPoint = new Point(pixelX,pixelY);
			mapGridBox.Invalidate();
		}

		public void HighlightWarp(int pixelX, int pixelY)
		{
			mapGridBox.warpHighlightPoint = new Point(pixelX,pixelY);
			mapGridBox.Invalidate();
		}

	    private void UpdateViewLayer(ViewLayer layer)
	    {
	        if (currentRoom == null)
	            return;

	        switch (layer)
	        {
	            case ViewLayer.Both:
	                mapGridBox.Image = OverlayImage(mapLayers[1], mapLayers[0]);
	                viewLayer = ViewLayer.Both;
	                topTileTab.Enabled = true;
	                bottomTileTab.Enabled = true;
	                break;
	            case ViewLayer.Top:
	                mapGridBox.Image = mapLayers[0];
	                tileTabControl.SelectedIndex = 0;
	                viewLayer = ViewLayer.Top;
                    selectedTileData = topTileGridBox.SelectedIndex;
                    topTileTab.Enabled = true;
	                bottomTileTab.Enabled = false;
	                break;
	            case ViewLayer.Bottom:
	                mapGridBox.Image = mapLayers[1];
	                tileTabControl.SelectedIndex = 1;
	                viewLayer = ViewLayer.Bottom;
                    selectedTileData = bottomTileGridBox.SelectedIndex;
                    bottomTileTab.Enabled = true;
	                topTileTab.Enabled = false;
	                break;
	        }
	    }

        private void discardRoomChangesToolStripMenuItem_Click( object sender, EventArgs e )
		{
			//TODO
		}

#region MapInteraction
	    private void mapGridBox_MouseDown(object sender, MouseEventArgs e)
	    {
	        if (currentRoom == null)
	            return;

	        var tsTileWidth = tileMaps[0].Width / 16;

	        lastTilePos = mapGridBox.GetIndexPoint(mapGridBox.HoverIndex);

	        if (e.Button == MouseButtons.Right)
	        {
	            selectedTileData = currentRoom.GetTileData(selectedLayer, mapGridBox.HoverIndex * 2);//*2 as each tile is 2 bytes
	            mapGridBox.SelectedIndex = mapGridBox.HoverIndex;
	            var newX = selectedTileData % tsTileWidth;
	            var newY = (selectedTileData - newX) / tsTileWidth;
	            // bad practice, entire map selection functions could do with refactor like the tile selection
	            if (selectedLayer == 2)
	            {
	                bottomTileGridBox.SelectedIndex = selectedTileData;
	            }
	            else
	            {
	                topTileGridBox.SelectedIndex = selectedTileData;
	            }

	        }
	        else if (e.Button == MouseButtons.Left)
	        {
	            if (selectedTileData == -1) //no selected tile, nothing to paste
	                return;

	            WriteTile(mapGridBox.GetIndexPoint(mapGridBox.HoverIndex), mapGridBox.HoverIndex, selectedTileData, selectedLayer);
	        }
	    }

	    private void mapGridBox_MouseMove(object sender, MouseEventArgs e)
	    {
            if(currentRoom == null)
                return;

			var xspot=mapGridBox.HoverIndex%currentRoom.roomSize.X;
			var yspot=mapGridBox.HoverIndex/currentRoom.roomSize.X;
			statusXposText.Text="X:"+xspot.Hex().PadLeft(2, '0');
            statusYposText.Text="Y:"+yspot.Hex().PadLeft(2, '0');

	        if (e.Button == MouseButtons.Left)
	        {
	            var currentPos = mapGridBox.GetIndexPoint(mapGridBox.HoverIndex);

	            if (!lastTilePos.Equals(currentPos))
	            {
	                if (selectedTileData == -1) //no selected tile, nothing to paste
	                    return;

                    lastTilePos = currentPos;
	                mapGridBox.SelectedIndex = mapGridBox.HoverIndex;

	                WriteTile(mapGridBox.GetIndexPoint(mapGridBox.HoverIndex), mapGridBox.HoverIndex, selectedTileData, selectedLayer);
                }
	        }
        }
#endregion

#region TilesetInteraction	  
        private void bottomTileGridBox_MouseDown(object sender, MouseEventArgs e)
	    {
	        if (currentRoom == null)
	            return;

	        bottomTileGridBox.SelectedIndex = bottomTileGridBox.HoverIndex;
	        selectedLayer = 2;
	        selectedTileData = bottomTileGridBox.SelectedIndex;
	    }

	    private void topTileGridBox_MouseDown(object sender, MouseEventArgs e)
	    {
	        if (currentRoom == null)
	            return;

	        topTileGridBox.SelectedIndex = topTileGridBox.HoverIndex;
	        selectedLayer = 1;
	        selectedTileData = topTileGridBox.SelectedIndex;
	    }
#endregion

        private void WriteTile (Point p, int pos, int tileData, int layer)
        {
            if (p.X < 0 || p.Y < 0 || p.X > currentRoom.roomSize.X * 16 || p.Y > currentRoom.roomSize.Y * 16)
                return;

            if (layer == 1 && currentRoom.Bg1Exists)
            {
                currentRoom.DrawTile(ref mapLayers[0], p, currentArea, selectedLayer, tileData);

                Project.Instance.AddPendingChange(new Bg1DataChange(currentArea,currentRoom.Index));
            }
            else if (layer == 2 && currentRoom.Bg2Exists)
            {
                currentRoom.DrawTile(ref mapLayers[1], p, currentArea, selectedLayer, tileData);
                Project.Instance.AddPendingChange(new Bg2DataChange(currentArea,currentRoom.Index));
            }

            currentRoom.SetTileData(selectedLayer, pos * 2, selectedTileData);

            // TODO switch on layer view
            UpdateViewLayer(viewLayer);
        }

		public static void Notify(string info, string title)
		{
			MessageBox.Show( info, title, MessageBoxButtons.OK );
		}
	}
}
