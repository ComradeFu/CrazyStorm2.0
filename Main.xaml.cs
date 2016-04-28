﻿/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using CrazyStorm.Core;

namespace CrazyStorm
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private Members
        Config config;
        File file;
        Dictionary<ParticleSystem, CommandStack> commandStacks;
        List<Core.Component> clipBoard;
        string status = String.Empty;
        #endregion

        #region Public Members
        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Status"));
            }
        }
        #endregion

        #region Constructor
        public Main()
        {
            config = new Config("Config.ini");
            commandStacks = new Dictionary<ParticleSystem, CommandStack>();
            clipBoard = new List<Core.Component>();
            file = new File();
            InitializeComponent();
            InitializeConfig();
            InitializeSystem();
        }
        #endregion

        #region Private Methods
        void InitializeSystem()
        {
            InitializeFile();
            InitializeParticle();
            InitializeEdit();
            InitializeStatusBar();
        }
        void InitializeConfig()
        {
            ParticleTabControl.DataContext = config;
        }
        void InitializeFile()
        {
            Title = VersionInfo.AppTitle + " - " + fileName + VersionInfo.Extension;
            ImageList.ItemsSource = file.Images;
            SoundList.ItemsSource = file.Sounds;
            VariableGrid.ItemsSource = file.Globals;
            DeleteVariable.IsEnabled = file.Globals.Count > 0 ? true : false;
        }
        void InitializeParticle()
        {
            DeleteAllParticle();
            selectedParticle = file.ParticleSystems.First();
            foreach (var item in file.ParticleSystems)
            {
                InitializeCommandStack(item);
                AddNewParticleTab(item);
            }
        }
        void InitializeCommandStack(ParticleSystem particle)
        {
            commandStacks[particle] = new CommandStack();
            commandStacks[particle].StackChanged += () =>
            {
                UpdateCommandStackStatus();
            };
        }
        void InitializeEdit()
        {
            CutItem.IsEnabled = false;
            CopyItem.IsEnabled = false;
            PasteItem.IsEnabled = false;
            UndoItem.IsEnabled = false;
            RedoItem.IsEnabled = false;
            DelItem.IsEnabled = false;
            CutButton.IsEnabled = false;
            CopyButton.IsEnabled = false;
            PasteButton.IsEnabled = false;
            UndoButton.IsEnabled = false;
            RedoButton.IsEnabled = false;
        }
        void InitializeStatusBar()
        {
            status = (string)FindResource("Ready");
            StatusText.DataContext = this;
        }
        void InitializeLayerAndComponent()
        {
            selectedLayer = selectedParticle.Layers.First();
            LayerTree.ItemsSource = selectedParticle.Layers;            
            LayerAxis.ItemsSource = selectedParticle.Layers;
            ComponentTree.ItemsSource = selectedParticle.ComponentTree;
            BindComponentItem.IsEnabled = false;
            UnbindComponentItem.IsEnabled = false;
        }
        void UpdateSelectedStatus()
        {
            //Be careful that UpdateScreen() needs to update first,
            //because it will refresh selectedComponents set.
            UpdateScreen();
            UpdateComponentPanels();
            UpdateSelectedGroup();
            UpdateComponentMenu();
            UpdateEditStatus();
        }
        #endregion

        #region Window EventHandlers
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Highlight selected layer.
            TreeViewItem item = (TreeViewItem)LayerTree.ItemContainerGenerator.ContainerFromItem(selectedLayer);
            item.IsSelected = true;
        }
        #endregion
    }
}
