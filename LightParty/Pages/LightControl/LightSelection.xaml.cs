using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using LightParty.LightController;
using Q42.HueApi;
using LightParty.Services;
using System.Diagnostics;
using System.Threading.Tasks;
using LightParty.Connection;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LightParty.Pages.LightControl
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LightSelection : Page
    {
        dynamic uiParent;
        private readonly float mainTargetOutlineThickness = 4f; //Border outline thickness of the main target
        private readonly float otherTargetOutlineThickness = 3.4f; //Border outline thickness of other main targets

        public LightSelection()
        {
            this.InitializeComponent();
        }

        public void GiveVariables<T>(T newParent)
        {
            uiParent = newParent;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CreateLightButtons();
        }

        async void CreateLightButtons()
        {
            await BasicLightController.GetAllLights();

            int currentColumn = 0;
            foreach (Light light in BridgeInformation.lights)
            {
                Button newButton = new Button()
                {
                    Name = light.Name + "LightButton",
                    Tag = light.Id,

                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Stretch,

                    Padding = new Thickness(-2),
                    Margin = new Thickness(0, 0, 30, 0),

                    Background = new LinearGradientBrush(GenerateGradientStopCollection(LightInformation.GetLightColor(light)), 0)
                };
                newButton.Click += LightButtonClick;
                LightGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                Grid.SetColumn(newButton, currentColumn);

                Border border = new Border()
                {
                    BorderThickness = new Thickness(0, 0, 0, 0),
                    BorderBrush = new SolidColorBrush((Color)this.Resources["SystemAccentColor"])
                };

                Grid buttonGrid = new Grid();
                buttonGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                buttonGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(6, GridUnitType.Pixel) });

                TextBlock lightNameBlock = new TextBlock()
                {
                    Text = light.Name,
                    Style = (Style)Application.Current.Resources["MediumInformation"],
                    Foreground = new SolidColorBrush(Color.FromArgb(255,0,0,0)),

                    Padding = new Thickness(10, 0, 10, 0),

                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                };
                Grid.SetRowSpan(lightNameBlock, 2);
                Grid.SetRow(lightNameBlock, 0);
                buttonGrid.Children.Add(lightNameBlock);

                ProgressBar progressBar = new ProgressBar()
                {
                    Value = LightInformation.GetBrightnessInPercent(light),
                    Minimum = 1,
                    Maximum = 100,

                    Foreground = new SolidColorBrush(Color.FromArgb(25, 0, 0, 0)),
                    Background = new SolidColorBrush(Color.FromArgb(22, 0, 0, 0)),

                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                Grid.SetRow(progressBar, 1);
                buttonGrid.Children.Add(progressBar);

                border.Child = buttonGrid;
                newButton.Content = border;
                LightGrid.Children.Add(newButton);

                if (BridgeInformation.usedLights.Contains(light.Id.ToString()) && light.Id.ToString() != BridgeInformation.mainLightTarget.ToString())
                    SetLightButtonOutline(newButton, true, otherTargetOutlineThickness);

                if (BridgeInformation.mainLightTarget != null)
                {
                    if (light.Id.ToString() == BridgeInformation.mainLightTarget.Id.ToString())
                        SetLightButtonOutline(newButton, true, mainTargetOutlineThickness);
                }

                currentColumn++;
            }

            if (BridgeInformation.usedLights.Count > 0)
                uiParent.LightSelectionChanged();
        }

        private GradientStopCollection GenerateGradientStopCollection(Color colorInput)
        {
            Color firstColor = ColorAssistant.GetStepBetweenTwoColors(Color.FromArgb(255, 255, 255, 255), colorInput, 0.5f);

            GradientStopCollection gradientStops = new GradientStopCollection();
            gradientStops.Add(new GradientStop() { Color = firstColor, Offset = 0 });
            gradientStops.Add(new GradientStop() { Color = colorInput, Offset = 1 });

            return gradientStops;
        }

        private async void LightButtonClick(object sender, RoutedEventArgs e)
        {
            BasicLightController.canControl = false;

            int id = Convert.ToInt32(((Button)sender).Tag);
            SetLightButtonOutline((Button)sender, BasicLightController.ChangeLightSelection(id.ToString()), otherTargetOutlineThickness);
            SetAutoMainTarget();

            await BasicLightController.GetAllLights();
            BasicLightController.canControl = true;

            uiParent.LightSelectionChanged();
        }

        private void SetLightButtonOutline(Button button, bool set, float thickness)
        {
            Border border = button.Content as Border;

            if (set)
            {
                border.BorderThickness = new Thickness(thickness, thickness, thickness, thickness);
                button.Margin = new Thickness(0, 0, button.Margin.Right - (border.BorderThickness.Left + border.BorderThickness.Right), 0);
            }
            else
            {
                button.Margin = new Thickness(0, 0, button.Margin.Right + (border.BorderThickness.Left + border.BorderThickness.Right), 0);
                border.BorderThickness = new Thickness(0, 0, 0, 0);
            }
        }

        private void SetAutoMainTarget()
        {
            if (BridgeInformation.usedLights.Count == 1)
            {
                BridgeInformation.mainLightTarget = BridgeInformation.lights[Convert.ToInt32(BridgeInformation.usedLights[0]) - 1];
                SetLightButtonOutline(LightGrid.Children[Convert.ToInt32(BridgeInformation.mainLightTarget.Id) - 1] as Button, false, otherTargetOutlineThickness);
                SetLightButtonOutline(LightGrid.Children[Convert.ToInt32(BridgeInformation.mainLightTarget.Id) - 1] as Button, true, mainTargetOutlineThickness);
            }
        }

        public async Task UpdateLightButtons()
        {
            await Task.Delay(10);
            await BasicLightController.GetAllLights();

            foreach (Button lightButton in LightGrid.Children)
            {
                Light light = BridgeInformation.lights[Convert.ToInt32(lightButton.Tag) - 1];
                lightButton.Background = new LinearGradientBrush(GenerateGradientStopCollection(LightInformation.GetLightColor(light)), 0);

                Grid buttonGrid = (Grid)((Border)lightButton.Content).Child;
                ProgressBar progressBar = buttonGrid.Children[1] as ProgressBar;
                progressBar.Value = LightInformation.GetBrightnessInPercent(light);
            }
        }
    }
}
