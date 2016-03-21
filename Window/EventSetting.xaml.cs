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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CrazyStorm.Core;
using CrazyStorm.Script;

namespace CrazyStorm
{
    /// <summary>
    /// Interaction logic for EventSetting.xaml
    /// </summary>
    public partial class EventSetting : Window
    {
        #region Private Members
        EventGroup eventGroup;
        Script.Environment environment;
        IList<Resource> sounds;
        IList<ParticleType> types;
        IList<PropertyPanelItem>[] properties;
        bool emitter;
        bool aboutParticle;
        bool isPlaySound;
        bool isEditing;
        #endregion

        #region Constructor
        public EventSetting(EventGroup eventGroup, Script.Environment environment,
            IList<Resource> sounds, IList<ParticleType> types, IList<PropertyPanelItem>[] properties, 
            bool emitter, bool aboutParticle)
        {
            this.eventGroup = eventGroup;
            this.environment = environment;
            this.sounds = sounds;
            this.types = types;
            this.properties = properties;
            this.emitter = emitter;
            this.aboutParticle = aboutParticle;
            InitializeComponent();
            InitializeSetting();
            LoadContent();
        }
        #endregion

        #region Private Methods
        void InitializeSetting()
        {
            GroupBox.DataContext = eventGroup;
            EventList.ItemsSource = eventGroup.Events;
            EmitParticle.Visibility = emitter ? Visibility.Visible : Visibility.Collapsed;
            ChangeType.Visibility = aboutParticle ? Visibility.Visible : Visibility.Collapsed;
        }
        void LoadContent()
        {
            //Load locals and globals.
            foreach (var item in environment.Locals)
            {
                LeftConditionComboBox.Items.Add(item.Key);
                RightConditionComboBox.Items.Add(item.Key);
                PropertyComboBox.Items.Add(item.Key);
                //Split fields of struct for convenience
                foreach (var structItem in environment.Structs)
                {
                    if (structItem.Key == item.Value.GetType().ToString())
                    {
                        foreach (var fieldItem in structItem.Value.GetFields())
                        {
                            LeftConditionComboBox.Items.Add(item.Key + "." + fieldItem.Key);
                            RightConditionComboBox.Items.Add(item.Key + "." + fieldItem.Key);
                            PropertyComboBox.Items.Add(item.Key + "." + fieldItem.Key);
                        }
                    }
                }
            }
            foreach (var item in environment.Globals)
            {
                LeftConditionComboBox.Items.Add(item.Key);
                RightConditionComboBox.Items.Add(item.Key);
                PropertyComboBox.Items.Add(item.Key);
            }
            //Load sounds.
            foreach (FileResource item in sounds)
            {
                if (item.IsValid)
                    SoundCombo.Items.Add(item);
            }
            //Load particle types.
            //First needs to merge repeated type name.
            var typesNorepeat = new List<ParticleType>();
            foreach (var item in types)
            {
                bool exist = false;
                for (int i = 0; i < typesNorepeat.Count; ++i)
                    if (item.Name == typesNorepeat[i].Name)
                    {
                        exist = true;
                        break;
                    }

                if (!exist)
                    typesNorepeat.Add(item);
            }
            TypeCombo.ItemsSource = typesNorepeat;
        }
        void ChangeTextBoxState(TextBox source, bool hasError)
        {
            if (hasError)
            {
                var tip = new ToolTip();
                var tipText = new TextBlock();
                tipText.Text = (string)FindResource("ValueInvalid");
                tip.Content = tipText;
                source.ToolTip = tip;
                source.Background = new SolidColorBrush(Color.FromRgb(255, 190, 190));
            }
            else
            {
                source.ToolTip = null;
                source.Background = new SolidColorBrush(Colors.White);
            }
        }
        bool BuildCondition(out string text)
        {
            text = string.Empty;
            //Check if there have errors
            if (LeftValue.ToolTip != null || RightValue.ToolTip != null)
                return false;

            //Build left condition
            string leftCondition = string.Empty;
            string leftCompare = string.Empty;
            if (LeftLessThan.IsChecked == true)
                leftCompare = " < ";
            else if (LeftEqual.IsChecked == true)
                leftCompare = " = ";
            else if (LeftMoreThan.IsChecked == true)
                leftCompare = " > ";

            if (LeftConditionComboBox.SelectedItem != null && LeftValue.Text != string.Empty &&
                leftCompare != string.Empty)
            {
                leftCondition = LeftConditionComboBox.SelectedItem + leftCompare + LeftValue.Text;
            }
            //Build right condition
            string rightCondition = string.Empty;
            string rightCompare = string.Empty;
            if (RightLessThan.IsChecked == true)
                rightCompare = " < ";
            else if (RightEqual.IsChecked == true)
                rightCompare = " = ";
            else if (RightMoreThan.IsChecked == true)
                rightCompare = " > ";

            if (RightConditionComboBox.SelectedItem != null && RightValue.Text != string.Empty &&
                rightCompare != string.Empty)
            {
                rightCondition = RightConditionComboBox.SelectedItem + rightCompare + RightValue.Text;
            }
            //Allow empty condition
            if (leftCondition == string.Empty && rightCondition == string.Empty)
                return true;

            //Build condition
            text = leftCondition != string.Empty ? leftCondition : rightCondition;
            string concat = string.Empty;
            if (And.IsChecked == true)
                concat = " & ";
            else if (Or.IsChecked == true)
                concat = " | ";

            if (concat != string.Empty && leftCondition != string.Empty && rightCondition != string.Empty)
            {
                text = leftCondition + concat + rightCondition;
            }
            return true;
        }
        bool BuildEventText(out string text)
        {
            text = string.Empty;
            //Check if there have errors
            if (ResultValue.ToolTip != null || ChangeTime.ToolTip != null || ExecuteTime.ToolTip != null)
                return false;

            //Build condition
            string condition = string.Empty;
            if (!BuildCondition(out condition))
                return false;
            //Build property event
            string propertyEvent = string.Empty;
            string changeType = string.Empty;
            if (ChangeTo.IsChecked == true)
                changeType = " == ";
            else if (Increase.IsChecked == true)
                changeType = " += ";
            else if (Decrease.IsChecked == true)
                changeType = " -= ";

            string changeMode = string.Empty;
            if (Linear.IsChecked == true)
                changeMode = "Linear";
            else if (Accelerated.IsChecked == true)
                changeMode = "Accelerated";
            else if (Decelerated.IsChecked == true)
                changeMode = "Decelerated";
            else if (Fixed.IsChecked == true)
                changeMode = "Fixed";

            if (PropertyComboBox.SelectedItem != null && ResultValue.Text != string.Empty &&
                changeType != string.Empty && changeMode != string.Empty && ChangeTime.Text != string.Empty)
            {
                propertyEvent = PropertyComboBox.SelectedItem + changeType + ResultValue.Text + ", " +
                    changeMode + ", " + ChangeTime.Text;
                if (ExecuteTime.Text != string.Empty)
                    propertyEvent += ", " + ExecuteTime.Text;
            }
            if (propertyEvent == string.Empty)
                return false;

            if (condition == string.Empty)
                text = propertyEvent;
            else
                text = condition + " : " + propertyEvent;

            return true;
        }
        bool BuildSpecialEventText(out string text)
        {
            text = string.Empty;
            //Build condition
            string condition = string.Empty;
            if (!BuildCondition(out condition))
                return false;
            //Build special event
            string specialEvent = string.Empty;
            if (EmitParticle.IsChecked == true)
            {
                specialEvent = "EmitParticle()";
            }
            else if (PlaySound.IsChecked == true)
            {
                if (SoundCombo.SelectedItem == null)
                    return false;

                specialEvent = "PlaySound(" + SoundCombo.SelectedItem + ", " + VolumeSlider.Value + ")";
            }
            else if (Loop.IsChecked == true)
            {
                //Check if there have errors
                if (LoopTime.ToolTip != null || StopCondition.ToolTip != null)
                    return false;

                if (LoopTime.Text == string.Empty && StopCondition.Text == string.Empty)
                    return false;

                string arguments = LoopTime.Text != string.Empty ? LoopTime.Text : StopCondition.Text;
                if (LoopTime.Text != string.Empty && StopCondition.Text != string.Empty)
                {
                    arguments = LoopTime.Text + ", " + StopCondition.Text;
                }
                specialEvent = "Loop(" + arguments + ")";
            }
            else if (ChangeType.IsChecked == true)
            {
                if (TypeCombo.SelectedItem == null || ColorCombo.SelectedItem == null)
                    return false;

                specialEvent = "ChangeType(" + TypeCombo.SelectedItem + ", " + 
                    ((ComboBoxItem)ColorCombo.SelectedItem).Content + ")";
            }
            if (specialEvent == string.Empty)
                return false;

            if (condition == string.Empty)
                text = specialEvent;
            else
                text = condition + " : " + specialEvent;

            return true;
        }
        void ResetAll()
        {
            LeftConditionComboBox.SelectedIndex = -1;
            LeftMoreThan.IsChecked = false;
            LeftEqual.IsChecked = false;
            LeftLessThan.IsChecked = false;
            LeftValue.Text = string.Empty;
            And.IsChecked = false;
            Or.IsChecked = false;
            RightConditionComboBox.SelectedIndex = -1;
            RightMoreThan.IsChecked = false;
            RightEqual.IsChecked = false;
            RightLessThan.IsChecked = false;
            RightValue.Text = string.Empty;
            PropertyComboBox.SelectedIndex = -1;
            ChangeTo.IsChecked = false;
            Increase.IsChecked = false;
            Decrease.IsChecked = false;
            ResultValue.Text = string.Empty;
            Linear.IsChecked = false;
            Accelerated.IsChecked = false;
            Decelerated.IsChecked = false;
            Fixed.IsChecked = false;
            ChangeTime.Text = string.Empty;
            ExecuteTime.Text = string.Empty;
            EmitParticle.IsChecked = false;
            PlaySound.IsChecked = false;
            Loop.IsChecked = false;
            ChangeType.IsChecked = false;
        }
        void MapEventText(string text)
        {
            ResetAll();
            Dictionary<string, RadioButton> buttonMap = new Dictionary<string, RadioButton>();
            buttonMap[">"] = LeftMoreThan;
            buttonMap["="] = LeftEqual;
            buttonMap["<"] = LeftLessThan;
            buttonMap["&"] = And;
            buttonMap["|"] = Or;
            string[] split = text.Split(':');
            string condition = string.Empty;
            string eventText = string.Empty;
            //Backfill condition
            if (split.Length == 2)
            {
                condition = split[0];
                split = condition.Split(' ');
                if (split.Length == 8)
                {
                    LeftConditionComboBox.SelectedIndex = LeftConditionComboBox.Items.IndexOf(split[0]);
                    buttonMap[split[1]].IsChecked = true;
                    LeftValue.Text = split[2];
                    buttonMap[split[3]].IsChecked = true;
                    RightConditionComboBox.SelectedIndex = RightConditionComboBox.Items.IndexOf(split[4]);
                    buttonMap[">"] = RightMoreThan;
                    buttonMap["="] = RightEqual;
                    buttonMap["<"] = RightLessThan;
                    buttonMap[split[5]].IsChecked = true;
                    RightValue.Text = split[6];
                }
                else
                {
                    LeftConditionComboBox.SelectedIndex = LeftConditionComboBox.Items.IndexOf(split[0]);
                    buttonMap[split[1]].IsChecked = true;
                    LeftValue.Text = split[2];
                }
                eventText = split[1];
            }
            else
            {
                eventText = split[0];
            }
            //Backfill event
            if (eventText.Contains('='))
            {
                //TODO
            }
            else
            {
                //TODO
            }
        }
        #endregion

        #region Window EventHandler
        private void Linear_Checked(object sender, RoutedEventArgs e)
        {
            Accelerated.IsChecked = false;
            Decelerated.IsChecked = false;
            Fixed.IsChecked = false;
        }
        private void Accelerated_Checked(object sender, RoutedEventArgs e)
        {
            Linear.IsChecked = false;
            Decelerated.IsChecked = false;
            Fixed.IsChecked = false;
        }
        private void Decelerated_Checked(object sender, RoutedEventArgs e)
        {
            Accelerated.IsChecked = false;
            Linear.IsChecked = false;
            Fixed.IsChecked = false;
        }
        private void Fixed_Checked(object sender, RoutedEventArgs e)
        {
            Accelerated.IsChecked = false;
            Decelerated.IsChecked = false;
            Linear.IsChecked = false;
        }
        private void EmitParticleButton_Checked(object sender, RoutedEventArgs e)
        {
            PlaySoundPanel.Visibility = Visibility.Collapsed;
            LoopPanel.Visibility = Visibility.Collapsed;
            ChangeTypePanel.Visibility = Visibility.Collapsed;
        }
        private void PlaySoundButton_Checked(object sender, RoutedEventArgs e)
        {
            PlaySoundPanel.Visibility = Visibility.Visible;
            LoopPanel.Visibility = Visibility.Collapsed;
            ChangeTypePanel.Visibility = Visibility.Collapsed;
        }
        private void LoopButton_Checked(object sender, RoutedEventArgs e)
        {
            PlaySoundPanel.Visibility = Visibility.Collapsed;
            LoopPanel.Visibility = Visibility.Visible;
            ChangeTypePanel.Visibility = Visibility.Collapsed;
        }
        private void ChangeTypeButton_Checked(object sender, RoutedEventArgs e)
        {
            PlaySoundPanel.Visibility = Visibility.Collapsed;
            LoopPanel.Visibility = Visibility.Collapsed;
            ChangeTypePanel.Visibility = Visibility.Visible;
        }
        private void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            string text;
            if (BuildEventText(out text))
                eventGroup.Events.Add(text);
        }
        private void AddSpecialEvent_Click(object sender, RoutedEventArgs e)
        {
            string text;
            if (BuildSpecialEventText(out text))
                eventGroup.Events.Add(text);
        }
        private void EditEvent_Click(object sender, RoutedEventArgs e)
        {
            var panel = (((e.OriginalSource as FrameworkElement).Parent as ContextMenu).PlacementTarget) as DockPanel;
            panel.Background = SystemColors.HighlightBrush;
            MapEventText((string)EventList.SelectedItem);
            EventList.IsEnabled = false;
            isEditing = true;
        }
        private void DeleteEvent_Click(object sender, RoutedEventArgs e)
        {
            var item = EventList.SelectedItem;
            if (item != null)
                eventGroup.Events.Remove((string)item);
        }
        private void LeftConditionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LeftMoreThan.IsChecked = false;
            LeftEqual.IsChecked = false;
            LeftLessThan.IsChecked = false;
            LeftMoreThan.IsEnabled = true;
            LeftLessThan.IsEnabled = true;
            LeftValue.Text = string.Empty;
            ChangeTextBoxState(LeftValue, false);
            if (e.AddedItems.Count == 0)
                return;

            string selection = e.AddedItems[0].ToString();
            foreach (var item in environment.Locals)
            {
                if (selection == item.Key)
                {
                    LeftMoreThan.IsEnabled = (item.Value is int || item.Value is float);
                    LeftLessThan.IsEnabled = (item.Value is int || item.Value is float);
                    return;
                }
            }
        }
        private void RightConditionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RightMoreThan.IsChecked = false;
            RightEqual.IsChecked = false;
            RightLessThan.IsChecked = false;
            RightMoreThan.IsEnabled = true;
            RightLessThan.IsEnabled = true;
            RightValue.Text = string.Empty;
            ChangeTextBoxState(RightValue, false);
            if (e.AddedItems.Count == 0)
                return;

            string selection = e.AddedItems[0].ToString();
            foreach (var item in environment.Locals)
            {
                if (selection == item.Key)
                {
                    RightMoreThan.IsEnabled = (item.Value is int || item.Value is float);
                    RightLessThan.IsEnabled = (item.Value is int || item.Value is float);
                    return;
                }
            }
        }
        private void PropertyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeTo.IsChecked = false;
            Increase.IsChecked = false;
            Decrease.IsChecked = false;
            Increase.IsEnabled = true;
            Decrease.IsEnabled = true;
            ResultValue.Text = string.Empty;
            ChangeTextBoxState(ResultValue, false);
            if (e.AddedItems.Count == 0)
                return;

            string selection = e.AddedItems[0].ToString();
            foreach (var item in environment.Locals)
            {
                if (selection == item.Key)
                {
                    Increase.IsEnabled = !(item.Value is Enum);
                    Decrease.IsEnabled = !(item.Value is Enum);
                    return;
                }
            }
        }
        private void LeftValue_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeTextBoxState(LeftValue, false);
            LeftValue.Text = LeftValue.Text.Trim();
            string input = LeftValue.Text;
            if (input == string.Empty)
                return;

            if (LeftConditionComboBox.SelectedItem != null)
            {
                string[] selection = ((string)LeftConditionComboBox.SelectedItem).Split('.');
                object value = environment.GetLocal(selection[0]);
                if (value != null)
                {
                    for (int i = 0; i < properties.Length; ++i)
                    {
                        if (properties[i] == null)
                            break;

                        foreach (var item in properties[i])
                        {
                            if (selection[0] == item.Name && selection.Length <= 1)
                            {
                                var attribute = item.Info.GetCustomAttributes(false)[0] as PropertyAttribute;
                                if (!attribute.IsLegal(input, out value))
                                {
                                    ChangeTextBoxState(LeftValue, true);
                                    return;
                                }
                                LeftValue.Text = value.ToString();
                                return;
                            }
                            else if (selection[0] == item.Name)
                            {
                                value = environment.GetStructs(item.Info.PropertyType.ToString()).GetField(selection[1]);
                                if (!Rule.IsMatchWith(value, input))
                                {
                                    ChangeTextBoxState(LeftValue, true);
                                }
                                return;
                            }
                        }
                    }
                }
                if (value == null)
                {
                    value = environment.GetGlobal(selection[0]);
                }
                if (value != null)
                {
                    float testValue;
                    if (!float.TryParse(input, out testValue))
                    {
                        ChangeTextBoxState(LeftValue, true);
                        return;
                    }
                }
            }
        }
        private void RightValue_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeTextBoxState(RightValue, false);
            RightValue.Text = RightValue.Text.Trim();
            string input = RightValue.Text;
            if (input == string.Empty)
                return;

            if (RightConditionComboBox.SelectedItem != null)
            {
                string[] selection = ((string)RightConditionComboBox.SelectedItem).Split('.');
                object value = environment.GetLocal(selection[0]);
                if (value != null)
                {
                    for (int i = 0; i < properties.Length; ++i)
                    {
                        if (properties[i] == null)
                            break;

                        foreach (var item in properties[i])
                        {
                            if (selection[0] == item.Name && selection.Length <= 1)
                            {
                                var attribute = item.Info.GetCustomAttributes(false)[0] as PropertyAttribute;
                                if (!attribute.IsLegal(input, out value))
                                {
                                    ChangeTextBoxState(RightValue, true);
                                    return;
                                }
                                RightValue.Text = value.ToString();
                                return;
                            }
                            else if (selection[0] == item.Name)
                            {
                                value = environment.GetStructs(item.Info.PropertyType.ToString()).GetField(selection[1]);
                                if (!Rule.IsMatchWith(value, input))
                                    ChangeTextBoxState(RightValue, true);

                                return;
                            }
                        }
                    }
                }
                if (value == null)
                {
                    value = environment.GetGlobal(selection[0]);
                }
                if (value != null)
                {
                    float testValue;
                    if (!float.TryParse(input, out testValue))
                    {
                        ChangeTextBoxState(RightValue, true);
                        return;
                    }
                }
            }
        }
        private void ResultValue_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeTextBoxState(ResultValue, false);
            ResultValue.Text = ResultValue.Text.Trim();
            string input = ResultValue.Text;
            if (input == string.Empty)
                return;

            if (PropertyComboBox.SelectedItem != null)
            {
                try
                {
                    string[] selection = ((string)PropertyComboBox.SelectedItem).Split('.');
                    object value = environment.GetLocal(selection[0]);
                    if (value != null)
                    {
                        for (int i = 0; i < properties.Length; ++i)
                        {
                            if (properties[i] == null)
                                break;

                            foreach (var item in properties[i])
                            {
                                if (selection[0] == item.Name && selection.Length <= 1)
                                {
                                    var attribute = item.Info.GetCustomAttributes(false)[0] as PropertyAttribute;
                                    if (attribute.IsLegal(input, out value))
                                    {
                                        ResultValue.Text = value.ToString();
                                        return;
                                    }
                                    var lexer = new Lexer();
                                    lexer.Load(input);
                                    var syntaxTree = new Parser(lexer).Expression();
                                    if (syntaxTree is Number)
                                        throw new ScriptException();

                                    var result = syntaxTree.Test(environment);
                                    if (!(Rule.IsMatchWith(item.Info.PropertyType, result.GetType())))
                                        throw new ScriptException();

                                    return;
                                }
                                else if (selection[0] == item.Name)
                                {
                                    value = environment.GetStructs(item.Info.PropertyType.ToString()).GetField(selection[1]);
                                    var lexer = new Lexer();
                                    lexer.Load(input);
                                    var syntaxTree = new Parser(lexer).Expression();
                                    var result = syntaxTree.Test(environment);
                                    if (!Rule.IsMatchWith(value.GetType(), result.GetType()))
                                        throw new ScriptException();

                                    return;
                                }
                            }
                        }
                    }
                    if (value == null)
                    {
                        value = environment.GetGlobal(selection[0]);
                    }
                    if (value != null)
                    {
                        var lexer = new Lexer();
                        lexer.Load(input);
                        var syntaxTree = new Parser(lexer).Expression();
                        var result = syntaxTree.Test(environment);
                        if (!(result is float))
                            throw new ScriptException();
                    }
                }
                catch (ScriptException ex)
                {
                    ChangeTextBoxState(ResultValue, true);
                }
            }
        }
        private void ChangeTime_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeTextBoxState(ChangeTime, false);
            ChangeTime.Text = ChangeTime.Text.Trim();
            string input = ChangeTime.Text;
            if (input == string.Empty)
                return;

            int value;
            if (!int.TryParse(input, out value))
                ChangeTextBoxState(ChangeTime, true);
            else if (value <= 0)
                ChangeTime.Text = "1";
        }
        private void ExecuteTime_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeTextBoxState(ExecuteTime, false);
            ExecuteTime.Text = ExecuteTime.Text.Trim();
            string input = ExecuteTime.Text;
            if (input == string.Empty)
                return;

            int value;
            if (!int.TryParse(input, out value))
                ChangeTextBoxState(ExecuteTime, true);
            else if (value < 0)
                ExecuteTime.Text = "0";
        }
        private void LoopTime_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeTextBoxState(LoopTime, false);
            LoopTime.Text = LoopTime.Text.Trim();
            string input = LoopTime.Text;
            if (input == string.Empty)
                return;

            int value;
            if (!int.TryParse(input, out value))
                ChangeTextBoxState(LoopTime, true);
            else if (value <= 0)
                LoopTime.Text = "1";
        }
        private void Condition_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeTextBoxState(Condition, false);
            Condition.Text = Condition.Text.Trim();
            string input = Condition.Text;
            if (input == string.Empty)
            {
                eventGroup.Condition = input;
                return;
            }
            try
            {
                var lexer = new Lexer();
                lexer.Load(input);
                var syntaxTree = new Parser(lexer).Expression();
                var result = syntaxTree.Test(environment);
                if (!(result is bool))
                    throw new ScriptException();
                eventGroup.Condition = input;
            }
            catch (ScriptException ex)
            {
                ChangeTextBoxState(Condition, true);
            }
        }
        private void StopCondition_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeTextBoxState(StopCondition, false);
            StopCondition.Text = StopCondition.Text.Trim();
            string input = StopCondition.Text;
            if (input == string.Empty)
                return;

            try
            {
                var lexer = new Lexer();
                lexer.Load(input);
                var syntaxTree = new Parser(lexer).Expression();
                var result = syntaxTree.Test(environment);
                if (!(result is bool))
                    throw new ScriptException();
            }
            catch (ScriptException ex)
            {
                ChangeTextBoxState(StopCondition, true);
            }
        }
        private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TypeCombo.SelectedItem != null)
            {
                ColorCombo.Items.Clear();
                var selectedItem = TypeCombo.SelectedItem as ParticleType;
                foreach (var item in types)
                {
                    if (item.Name == selectedItem.Name)
                    {
                        var color = new ComboBoxItem();
                        color.Content = item.Color.ToString();
                        ColorCombo.Items.Add(color);
                    }
                }
            }
        }
        private void SoundTextButton_Click(object sender, RoutedEventArgs e)
        {
            if (SoundCombo.SelectedItem != null)
            {
                isPlaySound = !isPlaySound;
                if (isPlaySound)
                {
                    SoundTestButton.Content = (string)FindResource("Pause");
                    MediaPlayer.Source = new Uri(((FileResource)SoundCombo.SelectedItem).AbsolutePath, UriKind.Absolute);
                    MediaPlayer.Volume = VolumeSlider.Value / 100;
                    MediaPlayer.LoadedBehavior = MediaState.Manual;
                    MediaPlayer.Play();
                }
                else
                {
                    SoundTestButton.Content = (string)FindResource("Test");
                    MediaPlayer.Stop();
                }
            }
        }
        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            isPlaySound = false;
            SoundTestButton.Content = (string)FindResource("Test");
            MediaPlayer.Stop();
        }
        #endregion
    }
}
