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
using System.Reflection;
using CrazyStorm.Core;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Private Members
        Point lastMouseDown;
        DependencyObject lastSelectedItem;
        #endregion

        #region Private Methods
        void UpdateSelectedGroup()
        {
            //Get all visible components in this particle system.
            var set = new List<Component>();
            foreach (var layer in selectedParticle.Layers)
                if (layer.Visible)
                    set.AddRange(layer.Components);
            //Clean removed component.
            for (int i = 0; i < selectedComponents.Count; ++i)
                if (!set.Contains(selectedComponents[i]))
                {
                    selectedComponents.RemoveAt(i);
                    i--;
                }
            //Update selected group.
            if (selectedComponents.Count > 0)
            {
                SelectedGroup.Opacity = 1;
                if (selectedComponents.Count == 1)
                {
                    var component = selectedComponents.First();
                    SelectedGroupType.Text = component.GetType().Name;
                    SelectedGroupName.DataContext = component;
                    SelectedGroupName.SetBinding(TextBlock.TextProperty, "Name");
                    SelectedGroupTip.Text = (string)FindResource("DoubleClickTip");
                    SelectedGroupImage.Source = new BitmapImage(
                        new Uri(@"Images/button-" + component.GetType().Name + ".png", UriKind.Relative));
                }
                else
                {
                    SelectedGroupImage.Source = new BitmapImage(new Uri(@"Images/group.png", UriKind.Relative));
                    SelectedGroupType.Text = "Group";
                    SelectedGroupName.Text = selectedComponents.Count + (string)FindResource("ComponentUnit");
                    SelectedGroupTip.Text = string.Empty;
                }
            }
            else
                SelectedGroup.Opacity = 0;
        }
        void CreatePropertyPanel(Component component)
        {
            TabItem item;
            //Prevent from repeating tab of components.  
            for (int i = 2; i < LeftTabControl.Items.Count; ++i)
            {
                item = LeftTabControl.Items[i] as TabItem;
                if (item.DataContext == component)
                {
                    item.Focus();
                    return;
                }
            }
            item = new TabItem();
            item.DataContext = component;
            item.Style = (Style)FindResource("CanCloseStyle");
            var scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            var panel = new PropertyPanel(commandStacks[selectedParticle],
                file, selectedParticle.CustomType, component, UpdateProperty);
            scroll.Content = panel;
            panel.OnBeginEditing += () =>
            {
                editingProperties = true;
            };
            panel.OnEndEditing += () =>
            {
                editingProperties = false;
            };
            item.Content = scroll;
            LeftTabControl.Items.Add(item);
            item.Focus();
        }
        void UpdateProperty()
        {
            foreach (TabItem item in LeftTabControl.Items)
            {
                var scroll = item.Content as ScrollViewer;
                if (scroll != null)
                {
                    var content = scroll.Content as PropertyPanel;
                    if (content != null)
                        content.UpdateProperty();
                }
            }
            UpdateScreen();
        }
        void UpdatePropertyPanel()
        {
            //Get all visible components in this particle system.
            var set = new List<Component>();
            foreach (var layer in selectedParticle.Layers)
                if (layer.Visible)
                    set.AddRange(layer.Components);
            //Remove the tab which is not belonging to any component.
            for (int i = 2; i < LeftTabControl.Items.Count; ++i)
            {
                TabItem item = LeftTabControl.Items[i] as TabItem;
                if (!set.Contains(item.DataContext))
                {
                    LeftTabControl.Items.RemoveAt(i);
                    i--;
                }
            }
        }
        void BindComponent()
        {
            bindingLines = new List<Line>();
            foreach (var component in selectedComponents)
            {
                //If component has parent, caculate the absolute position.
                float x = component.X;
                float y = component.Y;
                if (component.Parent != null)
                {
                    Vector2 parent = component.Parent.GetAbsolutePosition();
                    x += parent.x;
                    y += parent.y;
                }
                var line = DrawHelper.GetLine((int)x + config.ScreenWidthOver2, (int)y + config.ScreenHeightOver2,
                    (int)screenMousePos.X, (int)screenMousePos.Y, 2, true, Colors.White, 0.5f);
                line.DataContext = component;
                bindingLines.Add(line);
            }
            SelectComponents(null, true);
        }
        void UnbindComponent()
        {
            if (selectedComponents.Count > 0)
                new UnbindComponentCommand().Do(commandStacks[selectedParticle], selectedComponents);
        }
        #endregion

        #region Window EventHandler
        private void ComponentButton_Click(object sender, RoutedEventArgs e)
        {
            //Create corresponding component according to different button.
            var button = sender as Button;
            aimRect = VisualHelper.VisualDownwardSearch((DependencyObject)ParticleTabControl.SelectedContent, "AimBox");
            aimRect.SetValue(OpacityProperty, 1.0d);
            aimComponent = ComponentFactory.Create(button.Name);
        }
        private void ComponentButton_MouseEnter(object sender, MouseEventArgs e)
        {
            //Light up button when mouse enter.
            var image = sender as Image;
            var button = VisualHelper.VisualUpwardSearch<Button>(image) as Button;
            image.Source = new BitmapImage(new Uri(@"Images\button-" + button.Name + "-on.png", UriKind.Relative));
        }
        private void ComponentButton_MouseLeave(object sender, MouseEventArgs e)
        {
            //Reset button when mouse leave.
            var image = sender as Image;
            var button = VisualHelper.VisualUpwardSearch<Button>(image) as Button;
            image.Source = new BitmapImage(new Uri(@"Images\button-" + button.Name + ".png", UriKind.Relative));
        }
        private void ComponentTree_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            lastMouseDown = e.GetPosition(ComponentTree);
            lastSelectedItem = sender as DependencyObject;
            //Select pointed component when mouse left-button down. 
            var textBlock = sender as TextBlock;
            var set = new List<CrazyStorm.Core.Component>();
            set.Add((Component)textBlock.DataContext);
            SelectComponents(set, true);
        }
        private void ComponentTree_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(ComponentTree);
                if ((Math.Abs(currentPosition.X - lastMouseDown.X) > 2.0) || 
                    (Math.Abs(currentPosition.Y - lastMouseDown.Y) > 2.0))
                {
                    if (lastSelectedItem != null)
                        DragDrop.DoDragDrop(lastSelectedItem, sender, DragDropEffects.Move);
                }
            }
        }
        private void ComponentTree_CheckDrop(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }
        private void ComponentTree_Drop(object sender, DragEventArgs e)
        {
            var sourceComponent = ((TextBlock)lastSelectedItem).DataContext as Component;
            if (!(e.OriginalSource is TextBlock))
                return;

            var targetComponent = ((TextBlock)e.OriginalSource).DataContext as Component;
            if (sourceComponent == targetComponent)
                return;

            if (targetComponent.Children.Contains(sourceComponent))
            {
                //A way to change the node from parenthood to brotherhood. 
                var tree = new Component();
                foreach (Component item in selectedParticle.ComponentTree)
                    tree.Children.Add(item);

                var parent = tree.FindParent(targetComponent);
                if (parent != null)
                {
                    if (parent == tree)
                    {
                        selectedParticle.ComponentTree.Add(sourceComponent);
                        sourceComponent.TransPositiontoAbsolute();
                        sourceComponent.Parent = null;
                    }
                    else
                    {
                        parent.Children.Add(sourceComponent);
                        sourceComponent.TransPositiontoAbsolute();
                        sourceComponent.Parent = parent;
                        sourceComponent.TransPositiontoRelative();
                    }
                    targetComponent.Children.Remove(sourceComponent);
                }
            }
            else
            {
                //Add source component to target component as child
                var tree = new Component();
                foreach (Component item in selectedParticle.ComponentTree)
                    tree.Children.Add(item);

                var parent = tree.FindParent(sourceComponent);
                if (parent != null)
                {
                    if (parent == tree)
                        selectedParticle.ComponentTree.Remove(sourceComponent);
                    else
                        parent.Children.Remove(sourceComponent);

                    targetComponent.Children.Add(sourceComponent);
                    sourceComponent.TransPositiontoAbsolute();
                    sourceComponent.Parent = targetComponent;
                    sourceComponent.TransPositiontoRelative();
                }
            }
        }
        private void DeleteComponentItem_Click(object sender, RoutedEventArgs e)
        {
            new DelComponentCommand().Do(commandStacks[selectedParticle], selectedParticle, selectedComponents);
            UpdateSelectedStatus();
        }
        private void TabClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = VisualHelper.VisualUpwardSearch<TabItem>(sender as DependencyObject) as TabItem;
            var tab = VisualHelper.VisualUpwardSearch<TabControl>(item) as TabControl;
            if (tab != null)
            {
                var scroll = item.Content as ScrollViewer;
                var panel = scroll.Content as PropertyPanel;
                tab.Items.Remove(item);
            }
        }
        private void BindComponentItem_Click(object sender, RoutedEventArgs e)
        {
            BindComponent();
        }
        private void UnbindComponentItem_Click(object sender, RoutedEventArgs e)
        {
            UnbindComponent();
        }
        #endregion
    }
}
