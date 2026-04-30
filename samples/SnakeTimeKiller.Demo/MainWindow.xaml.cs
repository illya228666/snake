using System;
using System.Windows;

namespace SnakeTimeKiller.Demo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateRows();
            UpdateColumns();
            UpdateSpeed();
            Snake.Focus();
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            Snake.Start();
        }

        private void PauseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Snake.Pause();
            Snake.Focus();
        }

        private void ResetButton_OnClick(object sender, RoutedEventArgs e)
        {
            Snake.Reset();
            Snake.Focus();
        }

        private void RowsSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Snake == null)
            {
                return;
            }

            UpdateRows();
        }

        private void ColumnsSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Snake == null)
            {
                return;
            }

            UpdateColumns();
        }

        private void SpeedSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Snake == null)
            {
                return;
            }

            UpdateSpeed();
        }

        private void UpdateRows()
        {
            var value = (int)Math.Round(RowsSlider.Value);
            RowsValueText.Text = value.ToString();
            Snake.Rows = value;
        }

        private void UpdateColumns()
        {
            var value = (int)Math.Round(ColumnsSlider.Value);
            ColumnsValueText.Text = value.ToString();
            Snake.Columns = value;
        }

        private void UpdateSpeed()
        {
            var value = (int)Math.Round(SpeedSlider.Value);
            SpeedValueText.Text = value + " ms";
            Snake.TickInterval = TimeSpan.FromMilliseconds(value);
        }
    }
}
