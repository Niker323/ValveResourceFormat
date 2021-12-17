using GUI.Controls;
using SteamDatabase.ValvePak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GUI
{
    partial class MainForm
    {
        //For sets
        public class MyNodeData
        {
            public string Name { get; set; }

            public List<MainForm.MyNodeData> Childs { get; set; }

            public Dictionary<string, string> Data { get; set; }

            public int Level { get; set; }

            public void AddStringStringData(string str1, string str2, int level)
            {
                if (level > this.Level)
                    this.Childs[checked(this.Childs.Count - 1)].AddStringStringData(str1, str2, level);
                else
                    this.Data[str1] = str2;
            }

            public void CreateChild(string name, int level)
            {
                if (level > this.Level)
                    this.Childs[checked(this.Childs.Count - 1)].CreateChild(name, level);
                else
                    this.Childs.Add(new MainForm.MyNodeData()
                    {
                        Name = name,
                        Level = checked(this.Level + 1),
                        Childs = new List<MainForm.MyNodeData>(),
                        Data = new Dictionary<string, string>()
                    });
            }

            public MainForm.MyNodeData GetChild(int level) => level > this.Level ? this.Childs[0] : this.Childs[0].GetChild(level);

            public MainForm.MyNodeData FindChildByName(string name)
            {
                int index = 0;
                while (this.Childs.Count > index)
                {
                    if (this.Childs[index].Name == name)
                        return this.Childs[index];
                    checked { ++index; }
                }
                return (MainForm.MyNodeData)null;
            }
        }

        private int nodenum = 1;

        public void SetTreeView(TreeViewWithSearchResults tree)
        {
            treeView1.Tag = tree;
        }

        public void LoadFileInfo(TreeNode node)
        {
            if (!(node.Tag.GetType() == typeof(PackageEntry)))
                return;

            var package = node.TreeView.Tag as TreeViewWithSearchResults.TreeViewPackageTag;
            var file = node.Tag as PackageEntry;
            package.Package.ReadEntry(file, out var output);

            string empty1 = string.Empty;
            if (output != null)
                empty1 = Encoding.UTF8.GetString(output);
            MainForm.MyNodeData data = new MainForm.MyNodeData();
            data.Name = "Base";
            data.Childs = new List<MainForm.MyNodeData>();
            data.Data = new Dictionary<string, string>();
            int level = 0;
            string empty2 = string.Empty;
            bool flag1 = false;
            string str = string.Empty;
            bool flag2 = false;
            int index = 0;
            while (empty1.Length > index)
            {
                if (flag1)
                {
                    if (empty1[index] == '"')
                    {
                        flag1 = false;
                        if (!flag2)
                        {
                            str = empty2;
                            flag2 = true;
                            empty2 = string.Empty;
                        }
                        else
                        {
                            data.AddStringStringData(str, empty2, level);
                            str = string.Empty;
                            empty2 = string.Empty;
                            flag2 = false;
                        }
                    }
                    else
                        empty2 += empty1[index].ToString();
                }
                else if (empty1[index] == '"')
                {
                    if (!flag1)
                        flag1 = true;
                }
                else if (empty1[index] == '{')
                {
                    data.CreateChild(str, level);
                    flag2 = false;
                    checked { ++level; }
                    empty2 = string.Empty;
                    str = string.Empty;
                }
                else if (empty1[index] == '}')
                    checked { --level; }
                checked { ++index; }
            }
            //Console.WriteLine("End Loading");
            //Console.WriteLine(data.Childs.Count.ToString());
            List<MyNodeData> allHeroItems = FindAllHeroItems(data);

            //Array nodes = new Array();
            //treeView1.Nodes. .CopyTo(nodes, 0);

            //Console.WriteLine(allHeroItems.Count.ToString());

            //var error = Convert.ToInt32("error");
            treeView2.Tag = (object)allHeroItems;
            TreeNode treeNodeArray = treeView1.Nodes[0];
            CreateNodes(allHeroItems, treeNodeArray);
        }

        public void CreateNodes(List<MyNodeData> itemlist, TreeNode startnode)
        {
            SortedDictionary<string, List<MainForm.MyNodeData>> source = new SortedDictionary<string, List<MainForm.MyNodeData>>();
            int index1 = 0;
            while (itemlist.Count > index1)
            {
                Dictionary<string, string> data = itemlist[index1].FindChildByName("used_by_heroes").Data;
                string key1 = data.First().Key;
                List<MainForm.MyNodeData> myNodeDataList1;
                if (source.TryGetValue(key1, out myNodeDataList1))
                {
                    myNodeDataList1.Add(itemlist[index1]);
                    string key2 = data.First().Key;
                    List<MainForm.MyNodeData> myNodeDataList2 = myNodeDataList1;
                    source[key2] = myNodeDataList2;
                }
                else
                {
                    List<MainForm.MyNodeData> myNodeDataList3 = new List<MainForm.MyNodeData>();
                    myNodeDataList3.Add(itemlist[index1]);
                    string key3 = data.First().Key;
                    List<MainForm.MyNodeData> myNodeDataList4 = myNodeDataList3;
                    source[key3] = myNodeDataList4;
                }
                checked { ++index1; }
            }

            //source.Sort(delegate (KeyValuePair<string, List<MyNodeData>> x, KeyValuePair<string, List<MyNodeData>> y)
            //{
            //    return x.FindChildByName("used_by_heroes").Data.ElementAt<KeyValuePair<string, string>>(0).Key.CompareTo(y.FindChildByName("used_by_heroes").Data.ElementAt<KeyValuePair<string, string>>(0).Key);
            //});

            int index2 = 0;
            foreach (KeyValuePair<string, List<MainForm.MyNodeData>> keyValuePair in source)
            {
                MainForm.MyNodeData data = new MainForm.MyNodeData();
                data.Name = keyValuePair.Key;
                List<MainForm.MyNodeData> myNodeDataList = keyValuePair.Value;
                TreeNode node1 = this.AddTreeNodeByObject(data, startnode);
                data.Name = "Bundles";
                TreeNode node2 = this.AddTreeNodeByObject(data, node1);
                int index3 = 0;
                while (myNodeDataList.Count > index3)
                {
                    string str;
                    if (myNodeDataList[index3].Data.TryGetValue("prefab", out str) && str == "bundle")
                    {
                        TreeNode node3 = this.AddTreeNodeByObject(myNodeDataList[index3], node2);
                        MainForm.MyNodeData childByName = myNodeDataList[index3].FindChildByName("bundle");
                        if (childByName != null)
                        {
                            foreach (KeyValuePair<string, string> strkv in childByName.Data)
                            {
                                string key = strkv.Key;
                                MainForm.MyNodeData itemByDataName = FindItemByDataName(itemlist, key);
                                if (itemByDataName != null)
                                    this.AddTreeNodeByObject(itemByDataName, node3);
                            }
                        }
                    }
                    checked { ++index3; }
                }
                data.Name = "Immortals";
                TreeNode node4 = this.AddTreeNodeByObject(data, node1);
                int index5 = 0;
                while (myNodeDataList.Count > index5)
                {
                    string str;
                    if (myNodeDataList[index5].Data.TryGetValue("item_rarity", out str) && str == "immortal")
                    {
                        TreeNode node5 = this.AddTreeNodeByObject(myNodeDataList[index5], node4);
                        MainForm.MyNodeData childByName = myNodeDataList[index5].FindChildByName("bundle");
                        if (childByName != null)
                        {
                            foreach (KeyValuePair<string, string> strkv in childByName.Data)
                            {
                                string key = strkv.Key;
                                MainForm.MyNodeData itemByDataName = FindItemByDataName(itemlist, key);
                                if (itemByDataName != null)
                                    this.AddTreeNodeByObject(itemByDataName, node5);
                            }
                        }
                    }
                    checked { ++index5; }
                }
                data.Name = "Arcana";
                TreeNode node6 = (TreeNode)null;
                int index7 = 0;
                while (myNodeDataList.Count > index7)
                {
                    string str;
                    if (myNodeDataList[index7].Data.TryGetValue("item_rarity", out str) && str == "arcana")
                    {
                        if (node6 == null)
                            node6 = this.AddTreeNodeByObject(data, node1);
                        TreeNode node7 = this.AddTreeNodeByObject(myNodeDataList[index7], node6);
                        MainForm.MyNodeData childByName = myNodeDataList[index7].FindChildByName("bundle");
                        if (childByName != null)
                        {
                            foreach (KeyValuePair<string, string> strkv in childByName.Data)
                            {
                                string key = strkv.Key;
                                MainForm.MyNodeData itemByDataName = FindItemByDataName(itemlist, key);
                                if (itemByDataName != null)
                                    this.AddTreeNodeByObject(itemByDataName, node7);
                            }
                        }
                    }
                    checked { ++index7; }
                }
                data.Name = "All";
                TreeNode node8 = this.AddTreeNodeByObject(data, node1);
                int index9 = 0;
                while (myNodeDataList.Count > index9)
                {
                    TreeNode node9 = this.AddTreeNodeByObject(myNodeDataList[index9], node8);
                    MainForm.MyNodeData childByName = myNodeDataList[index9].FindChildByName("bundle");
                    if (childByName != null)
                    {
                        foreach (KeyValuePair<string, string> strkv in childByName.Data)
                        {
                            string key = strkv.Key;
                            MainForm.MyNodeData itemByDataName = FindItemByDataName(itemlist, key);
                            if (itemByDataName != null)
                                this.AddTreeNodeByObject(itemByDataName, node9);
                        }
                    }
                    checked { ++index9; }
                }
                checked { ++index2; }
            }
        }

        public static MyNodeData FindItemByDataName(List<MyNodeData> list, string name)
        {
            int index = 0;
            while (list.Count > index)
            {
                string str;
                if (list[index].Data.TryGetValue(nameof(name), out str) && str == name)
                    return list[index];
                checked { ++index; }
            }
            return null;
        }

        public static List<MainForm.MyNodeData> FindAllHeroItems(MyNodeData data)
        {
            List<MyNodeData> myNodeDataList = new List<MyNodeData>();
            MyNodeData child = data.Childs[0].Childs[7];
            int index = 0;
            while (child.Childs.Count > index)
            {
                string str;
                if (child.Childs[index].FindChildByName("used_by_heroes") != null && (!child.Childs[index].Data.TryGetValue("prefab", out str) || !(str == "treasure_chest") && !(str == "retired_treasure_chest")))
                    myNodeDataList.Add(child.Childs[index]);
                checked { ++index; }
            }
            return myNodeDataList;
        }

        public TreeNode AddTreeNodeByObject(MainForm.MyNodeData data, TreeNode node)
        {
            TreeNode treeNode = node.Nodes.Add(data.Name);
            treeNode.Name = nameof(node) + (object)this.nodenum;
            string str;
            if (data.Data != null && data.Data.TryGetValue("image_inventory", out str))
                treeNode.Tag = (object)str;
            checked { ++this.nodenum; }
            return treeNode;
        }

        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;
            if (node.Tag == null)
                return;
            TreeViewWithSearchResults tag1 = (TreeViewWithSearchResults)treeView1.Tag;
            string tag2 = (string)node.Tag;
            Console.WriteLine(tag2);
            TreeNode[] nodesByPath = GetNodesByPath(tag1.mainTreeView.Nodes[0].Nodes.Find("panorama", false)[0], "images/" + tag2 + "_png.vtex_c");
            if (nodesByPath != null)
            {
                Console.WriteLine(nodesByPath.Length.ToString());
                if (nodesByPath.Length > 0)
                {
                    GUI.Types.Viewers.Package.OpenFileFromNode(nodesByPath[0]);
                    this.treeView1.Select();
                }
            }
        }

        private TreeNode[] GetNodesByPath(TreeNode tree, string path)
        {
            if (path.IndexOf("/") != path.LastIndexOf("/"))
            {
                TreeNode[] treeNodeArray = tree.Nodes.Find(path.Substring(0, path.IndexOf("/")), false);
                if ((uint)treeNodeArray.Length > 0U)
                    return GetNodesByPath(treeNodeArray[0], path.Substring(checked(path.IndexOf("/") + 1)));
            }
            else
            {
                TreeNode[] treeNodeArray = tree.Nodes.Find(path.Substring(0, path.IndexOf("/")), false);
                if ((uint)treeNodeArray.Length > 0U)
                    return treeNodeArray[0].Nodes.Find(path.Substring(checked(path.IndexOf("/") + 1)), false);
            }
            return null;
        }

        private void AddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.treeView1.SelectedNode.Tag == null)
                return;
            if (this.treeView1.SelectedNode.Nodes.Count == 0)
            {
                MainForm.MyNodeData childByName = new MainForm.MyNodeData()
                {
                    Childs = ((List<MainForm.MyNodeData>)this.treeView2.Tag)
                }.FindChildByName(this.treeView1.SelectedNode.Text);
                Dictionary<string, string> data = childByName.FindChildByName("used_by_heroes").Data;
                TreeNodeCollection nodes1 = this.treeView2.Nodes;
                KeyValuePair<string, string> keyValuePair = data.First();
                string key1 = keyValuePair.Key;
                TreeNode[] treeNodeArray = nodes1.Find(key1, true);
                TreeNode treeNode1;
                if (treeNodeArray.Length == 0)
                {
                    TreeNodeCollection nodes2 = this.treeView2.Nodes;
                    keyValuePair = data.First();
                    string key2 = keyValuePair.Key;
                    treeNode1 = nodes2.Add(key2);
                    TreeNode treeNode2 = treeNode1;
                    keyValuePair = data.First();
                    string key3 = keyValuePair.Key;
                    treeNode2.Name = key3;
                }
                else
                    treeNode1 = treeNodeArray[0];
                TreeNode treeNode3 = treeNode1.Nodes.Add(childByName.Name);
                treeNode3.Name = "node" + (object)this.nodenum;
                checked { ++this.nodenum; }
                string str;
                if (childByName.Data != null && childByName.Data.TryGetValue("image_inventory", out str))
                    treeNode3.Tag = (object)str;
            }
            else
            {
                int index = 0;
                while (this.treeView1.SelectedNode.Nodes.Count > index)
                {
                    MainForm.MyNodeData childByName = new MainForm.MyNodeData()
                    {
                        Childs = ((List<MainForm.MyNodeData>)this.treeView2.Tag)
                    }.FindChildByName(this.treeView1.SelectedNode.Nodes[index].Text);
                    Dictionary<string, string> data = childByName.FindChildByName("used_by_heroes").Data;
                    TreeNode[] treeNodeArray = this.treeView2.Nodes.Find(data.First().Key, true);
                    TreeNode treeNode4;
                    if (treeNodeArray.Length == 0)
                    {
                        treeNode4 = this.treeView2.Nodes.Add(data.First().Key);
                        treeNode4.Name = data.First().Key;
                    }
                    else
                        treeNode4 = treeNodeArray[0];
                    TreeNode treeNode5 = treeNode4.Nodes.Add(childByName.Name);
                    treeNode5.Name = "node" + (object)this.nodenum;
                    checked { ++this.nodenum; }
                    string str;
                    if (childByName.Data != null && childByName.Data.TryGetValue("image_inventory", out str))
                        treeNode5.Tag = (object)str;
                    checked { ++index; }
                }
            }
        }

        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = 0;
            while (this.treeView2.Nodes.Count > index)
            {
                this.treeView2.Nodes.RemoveAt(index);
                checked { ++index; }
            }
        }

        private void CreateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage tabPage = new TabPage("Wearables");
            this.mainTabs.TabPages.Add(tabPage);
            this.mainTabs.SelectTab(tabPage);
            string str1 = string.Empty;
            int index1 = 0;
            while (this.treeView2.Nodes.Count > index1)
            {
                string str2 = str1 + this.treeView2.Nodes[index1].Name + ":" + Environment.NewLine + "\"AttachWearables\"" + Environment.NewLine + "{" + Environment.NewLine;
                int index2 = 0;
                while (this.treeView2.Nodes[index1].Nodes.Count > index2)
                {
                    str2 = str2 + "\t\"Wearable" + (object)checked(index2 + 1) + "\" { \"ItemDef\" \"" + this.treeView2.Nodes[index1].Nodes[index2].Text + "\" }" + Environment.NewLine;
                    checked { ++index2; }
                }
                str1 = str2 + "}" + Environment.NewLine;
                checked { ++index1; }
            }
            TextBox textBox1 = new TextBox();
            textBox1.Dock = DockStyle.Fill;
            textBox1.ScrollBars = ScrollBars.Vertical;
            textBox1.Multiline = true;
            textBox1.ReadOnly = true;
            textBox1.Text = str1;
            TextBox textBox2 = textBox1;
            tabPage.Controls.Add((Control)textBox2);
        }

        private void RemoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView2.SelectedNode != null)
                treeView2.Nodes.Remove(treeView2.SelectedNode);
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (disposing && searchForm != null)
            {
                searchForm.Dispose();
                searchForm = null;
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Core");
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findToolStripButton = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainTabs = new System.Windows.Forms.TabControl();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItemsToRight = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItemsToLeft = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItems = new System.Windows.Forms.ToolStripMenuItem();
            this.vpkContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.extractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.decompileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.copyFileNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openWithDefaultAppToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.treeView2 = new System.Windows.Forms.TreeView();
            this.menuStrip1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.vpkContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.Window;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.findToolStripButton,
            this.exportToolStripButton,
            this.settingsToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 1, 0, 0);
            this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuStrip1.Size = new System.Drawing.Size(1101, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
            this.openToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(64, 23);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItem_Click);
            // 
            // findToolStripButton
            // 
            this.findToolStripButton.Enabled = false;
            this.findToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("findToolStripButton.Image")));
            this.findToolStripButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.findToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.findToolStripButton.Name = "findToolStripButton";
            this.findToolStripButton.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findToolStripButton.Size = new System.Drawing.Size(58, 23);
            this.findToolStripButton.Text = "&Find";
            this.findToolStripButton.Click += new System.EventHandler(this.FindToolStripMenuItem_Click);
            // 
            // exportToolStripButton
            // 
            this.exportToolStripButton.Enabled = false;
            this.exportToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("exportToolStripButton.Image")));
            this.exportToolStripButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.exportToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.exportToolStripButton.Name = "exportToolStripButton";
            this.exportToolStripButton.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.exportToolStripButton.Size = new System.Drawing.Size(78, 20);
            this.exportToolStripButton.Text = "Export";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("settingsToolStripMenuItem.Image")));
            this.settingsToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(77, 23);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.OnSettingsItemClick);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("aboutToolStripMenuItem.Image")));
            this.aboutToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(68, 23);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.OnAboutItemClick);
            // 
            // mainTabs
            // 
            this.mainTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabs.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.mainTabs.Location = new System.Drawing.Point(0, 0);
            this.mainTabs.Margin = new System.Windows.Forms.Padding(0);
            this.mainTabs.Name = "mainTabs";
            this.mainTabs.Padding = new System.Drawing.Point(0, 0);
            this.mainTabs.SelectedIndex = 0;
            this.mainTabs.Size = new System.Drawing.Size(807, 508);
            this.mainTabs.TabIndex = 1;
            this.mainTabs.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnTabClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeToolStripMenuItem,
            this.closeToolStripMenuItemsToRight,
            this.closeToolStripMenuItemsToLeft,
            this.closeToolStripMenuItems});
            this.contextMenuStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Table;
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(234, 124);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("closeToolStripMenuItem.Image")));
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(233, 30);
            this.closeToolStripMenuItem.Text = "Close this &tab";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.CloseToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItemsToRight
            // 
            this.closeToolStripMenuItemsToRight.Image = ((System.Drawing.Image)(resources.GetObject("closeToolStripMenuItemsToRight.Image")));
            this.closeToolStripMenuItemsToRight.Name = "closeToolStripMenuItemsToRight";
            this.closeToolStripMenuItemsToRight.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.closeToolStripMenuItemsToRight.Size = new System.Drawing.Size(233, 30);
            this.closeToolStripMenuItemsToRight.Text = "Close all tabs to &right";
            this.closeToolStripMenuItemsToRight.Click += new System.EventHandler(this.CloseToolStripMenuItemsToRight_Click);
            // 
            // closeToolStripMenuItemsToLeft
            // 
            this.closeToolStripMenuItemsToLeft.Image = ((System.Drawing.Image)(resources.GetObject("closeToolStripMenuItemsToLeft.Image")));
            this.closeToolStripMenuItemsToLeft.Name = "closeToolStripMenuItemsToLeft";
            this.closeToolStripMenuItemsToLeft.Size = new System.Drawing.Size(233, 30);
            this.closeToolStripMenuItemsToLeft.Text = "Close all tabs to &left";
            this.closeToolStripMenuItemsToLeft.Click += new System.EventHandler(this.CloseToolStripMenuItemsToLeft_Click);
            // 
            // closeToolStripMenuItems
            // 
            this.closeToolStripMenuItems.Image = ((System.Drawing.Image)(resources.GetObject("closeToolStripMenuItems.Image")));
            this.closeToolStripMenuItems.Name = "closeToolStripMenuItems";
            this.closeToolStripMenuItems.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.closeToolStripMenuItems.Size = new System.Drawing.Size(233, 30);
            this.closeToolStripMenuItems.Text = "Close &all tabs";
            this.closeToolStripMenuItems.Click += new System.EventHandler(this.CloseToolStripMenuItems_Click);
            // 
            // vpkContextMenu
            // 
            this.vpkContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractToolStripMenuItem,
            this.decompileToolStripMenuItem,
            this.toolStripSeparator1,
            this.copyFileNameToolStripMenuItem,
            this.openWithDefaultAppToolStripMenuItem});
            this.vpkContextMenu.Name = "vpkContextMenu";
            this.vpkContextMenu.Size = new System.Drawing.Size(193, 98);
            // 
            // extractToolStripMenuItem
            // 
            this.extractToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("extractToolStripMenuItem.Image")));
            this.extractToolStripMenuItem.Name = "extractToolStripMenuItem";
            this.extractToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.extractToolStripMenuItem.Text = "Export";
            this.extractToolStripMenuItem.Click += new System.EventHandler(this.ExtractToolStripMenuItem_Click);
            // 
            // decompileToolStripMenuItem
            // 
            this.decompileToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("decompileToolStripMenuItem.Image")));
            this.decompileToolStripMenuItem.Name = "decompileToolStripMenuItem";
            this.decompileToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.decompileToolStripMenuItem.Text = "Decompile && export";
            this.decompileToolStripMenuItem.Click += new System.EventHandler(this.DecompileToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(189, 6);
            // 
            // copyFileNameToolStripMenuItem
            // 
            this.copyFileNameToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("copyFileNameToolStripMenuItem.Image")));
            this.copyFileNameToolStripMenuItem.Name = "copyFileNameToolStripMenuItem";
            this.copyFileNameToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.copyFileNameToolStripMenuItem.Text = "Copy file path";
            this.copyFileNameToolStripMenuItem.Click += new System.EventHandler(this.CopyFileNameToolStripMenuItem_Click);
            // 
            // openWithDefaultAppToolStripMenuItem
            // 
            this.openWithDefaultAppToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openWithDefaultAppToolStripMenuItem.Image")));
            this.openWithDefaultAppToolStripMenuItem.Name = "openWithDefaultAppToolStripMenuItem";
            this.openWithDefaultAppToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.openWithDefaultAppToolStripMenuItem.Text = "Open with default app";
            this.openWithDefaultAppToolStripMenuItem.Click += new System.EventHandler(this.OpenWithDefaultAppToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Cursor = System.Windows.Forms.Cursors.VSplit;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.mainTabs);
            this.splitContainer1.Size = new System.Drawing.Size(1101, 508);
            this.splitContainer1.SplitterDistance = 290;
            this.splitContainer1.TabIndex = 2;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.treeView1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.statusStrip1);
            this.splitContainer2.Panel2.Controls.Add(this.treeView2);
            this.splitContainer2.Size = new System.Drawing.Size(290, 508);
            this.splitContainer2.SplitterDistance = 367;
            this.splitContainer2.TabIndex = 1;
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            treeNode1.Name = "Node0";
            treeNode1.Text = "Core";
            this.treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.treeView1.Size = new System.Drawing.Size(290, 367);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeView1_AfterSelect);
            this.treeView1.DoubleClick += new System.EventHandler(this.AddToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1,
            this.toolStripDropDownButton2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 115);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(290, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.ShowDropDownArrow = false;
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(33, 20);
            this.toolStripDropDownButton1.Text = "Add";
            this.toolStripDropDownButton1.Click += new System.EventHandler(this.AddToolStripMenuItem_Click);
            // 
            // toolStripDropDownButton2
            // 
            this.toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3,
            this.toolStripMenuItem2,
            this.toolStripMenuItem1});
            this.toolStripDropDownButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton2.Image")));
            this.toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            this.toolStripDropDownButton2.Size = new System.Drawing.Size(60, 20);
            this.toolStripDropDownButton2.Text = "Actions";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(117, 22);
            this.toolStripMenuItem3.Text = "Create";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.CreateToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(117, 22);
            this.toolStripMenuItem2.Text = "Remove";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.RemoveToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(117, 22);
            this.toolStripMenuItem1.Text = "Clear";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.ClearToolStripMenuItem_Click);
            // 
            // treeView2
            // 
            this.treeView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView2.Location = new System.Drawing.Point(0, 0);
            this.treeView2.Name = "treeView2";
            this.treeView2.Size = new System.Drawing.Size(290, 137);
            this.treeView2.TabIndex = 0;
            this.treeView2.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeView1_AfterSelect);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1101, 532);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(347, 340);
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.vpkContextMenu.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.TabControl mainTabs;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip vpkContextMenu;
        private System.Windows.Forms.ToolStripMenuItem extractToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyFileNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItemsToLeft;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItemsToRight;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItems;
        private System.Windows.Forms.ToolStripMenuItem findToolStripButton;
        private System.Windows.Forms.ToolStripDropDownButton exportToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem openWithDefaultAppToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem decompileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private TreeView treeView1;
        private StatusStrip statusStrip1;
        private TreeView treeView2;
        private ToolStripDropDownButton toolStripDropDownButton1;
        private ToolStripDropDownButton toolStripDropDownButton2;
        private ToolStripMenuItem toolStripMenuItem3;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripMenuItem toolStripMenuItem1;
    }
}

