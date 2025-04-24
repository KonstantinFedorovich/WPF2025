using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace Project1 // Adjust namespace if needed
{
    public class Bird
    {
        public double V0 { get; set; }
        public double Angle { get; set; }
        private const double G = 9.80665;

        public Bird(double v0_b, double a_b)
        {
            V0 = v0_b;
            Angle = a_b * Math.PI / 180;
        }
        public double CalculatePositionX(double t)
        {
            return V0 * t * Math.Cos(Angle);
        }
        public double CalculatePositionY(double t)
        {
            return V0 * Math.Sin(Angle) * t - (G * t * t) / 2;
        }
        public double TotalTime(double t)
        {
            return (2 * V0 * Math.Sin(Angle)) / G;
        }
    }

    public class TrajectoryWindow : Window
    {
        private Canvas drawCanvas;
        private const double Margin = 20; // Margin around the trajectory
        private Polyline trajectoryLine;
        private ImageBrush backgroundBrush;

        public TrajectoryWindow(List<Point> points, string imagePath)
        {
            Title = "Trajectory Visualization";
            Width = 600;
            Height = 400;

            drawCanvas = new Canvas();

            // Create a background image brush
            backgroundBrush = new ImageBrush();
            try
            {
                BitmapImage bitmap = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
                backgroundBrush.ImageSource = bitmap;
                drawCanvas.Background = backgroundBrush;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Image Error", MessageBoxButton.OK, MessageBoxImage.Error);
                drawCanvas.Background = Brushes.LightGray; // Fallback background
            }

            Content = drawCanvas;

            trajectoryLine = new Polyline
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 2
            };
            drawCanvas.Children.Add(trajectoryLine);

            Loaded += async (sender, e) => await DrawGradually(points);
        }

        private async Task DrawGradually(List<Point> allPoints)
        {
            if (drawCanvas == null || double.IsNaN(drawCanvas.ActualHeight) || double.IsNaN(drawCanvas.ActualWidth) || drawCanvas.ActualHeight <= 0 || drawCanvas.ActualWidth <= 0 || allPoints == null || allPoints.Count < 2)
            {
                return;
            }

            // Find the bounds of the trajectory
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (Point p in allPoints)
            {
                minX = Math.Min(minX, p.X);
                maxX = Math.Max(maxX, p.X);
                minY = Math.Min(minY, p.Y);
                maxY = Math.Max(maxY, p.Y);
            }

            // Calculate scaling factors
            double availableWidth = drawCanvas.ActualWidth - 2 * Margin;
            double availableHeight = drawCanvas.ActualHeight - 2 * Margin;

            double scaleX = (maxX - minX) > 0 ? availableWidth / (maxX - minX) : 1;
            double scaleY = (maxY - minY) > 0 ? availableHeight / (maxY - minY) : 1;

            // Use the smaller scale to fit the entire trajectory
            double scale = Math.Min(scaleX, scaleY);
            if (scale == 0 || double.IsInfinity(scale) || double.IsNaN(scale))
            {
                scale = 1; // Avoid division by zero or infinite/NaN scale
            }

            // Calculate translation to center the trajectory
            double offsetX = Margin - minX * scale + (availableWidth - (maxX - minX) * scale) / 2;
            double offsetY = Margin - maxY * scale + (availableHeight - (maxY - minY) * scale) / 2; // Invert Y for drawing

            PointCollection scaledPoints = new PointCollection();
            for (int i = 0; i < allPoints.Count; i++)
            {
                drawCanvas.Children.Clear(); // Clear the canvas before drawing everything again
                drawCanvas.Children.Add(trajectoryLine); // Re-add the trajectory line

                Point p = allPoints[i];
                scaledPoints.Add(new Point(p.X * scale + offsetX, drawCanvas.ActualHeight - (p.Y * scale + offsetY)));
                trajectoryLine.Points = scaledPoints;

                // Draw Axes
                Line xAxis = new Line
                {
                    X1 = 0,
                    Y1 = drawCanvas.ActualHeight - (0 * scale + offsetY), // Y=0 in world coords
                    X2 = drawCanvas.ActualWidth,
                    Y2 = drawCanvas.ActualHeight - (0 * scale + offsetY),
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                drawCanvas.Children.Add(xAxis);

                Line yAxis = new Line
                {
                    X1 = 0 * scale + offsetX, // X=0 in world coords
                    Y1 = 0,
                    X2 = 0 * scale + offsetX,
                    Y2 = drawCanvas.ActualHeight,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                drawCanvas.Children.Add(yAxis);

                await Task.Delay(50); // Adjust the delay for the drawing speed
            }
        }
    }

    public partial class MainWindow : Window
    {
        private TextBox velocityInput;
        private TextBox angleInput;
        private Button calculateButton;
        private Button resetButton;
        private Button settingsButton;

        public MainWindow()
        {
            Title = "Ballistic Trajectory Calculator";
            Width = 400;
            Height = 300; // Increased height to accommodate more buttons

            // Создание объекта DockPanel.
            DockPanel dock = new DockPanel();
            Content = dock;

            // Создание меню, пристыкованного у верхнего края окна.
            Menu menu = new Menu();
            dock.Children.Add(menu);
            DockPanel.SetDock(menu, Dock.Top);

            // Создание меню File.
            MenuItem itemFile = new MenuItem();
            itemFile.Header = "_File";
            menu.Items.Add(itemFile);

            MenuItem itemExit = new MenuItem();
            itemExit.Header = "E_xit";
            itemExit.Click += ExitOnClick;
            itemFile.Items.Add(itemExit);

            // Создание Grid для элементов управления внизу меню
            Grid inputGrid = new Grid();
            inputGrid.Margin = new Thickness(10); // Add some margin around the input grid
            inputGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            inputGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            inputGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            inputGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row for buttons
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition());
            dock.Children.Add(inputGrid);

            var speedLabel = new Label { Content = "Speed (m/s):" };
            Grid.SetRow(speedLabel, 0);
            Grid.SetColumn(speedLabel, 0);
            inputGrid.Children.Add(speedLabel);

            velocityInput = new TextBox { Margin = new Thickness(5) };
            Grid.SetRow(velocityInput, 0);
            Grid.SetColumn(velocityInput, 1);
            inputGrid.Children.Add(velocityInput);

            var angleLabel = new Label { Content = "Angle (degrees):" };
            Grid.SetRow(angleLabel, 1);
            Grid.SetColumn(angleLabel, 0);
            inputGrid.Children.Add(angleLabel);

            angleInput = new TextBox { Margin = new Thickness(5) };
            Grid.SetRow(angleInput, 1);
            Grid.SetColumn(angleInput, 1);
            inputGrid.Children.Add(angleInput);

            calculateButton = new Button { Content = "Show Trajectory", Margin = new Thickness(5), Padding = new Thickness(5) };
            calculateButton.Click += CalculateButton_Click;
            Grid.SetRow(calculateButton, 2);
            Grid.SetColumn(calculateButton, 0);
            Grid.SetColumnSpan(calculateButton, 2);
            inputGrid.Children.Add(calculateButton);

            // New Buttons in the new row
            StackPanel buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            Grid.SetRow(buttonPanel, 3);
            Grid.SetColumn(buttonPanel, 0);
            Grid.SetColumnSpan(buttonPanel, 2);
            inputGrid.Children.Add(buttonPanel);

            resetButton = new Button { Content = "Reset", Margin = new Thickness(5), Padding = new Thickness(5) };
            resetButton.Click += ResetButton_Click;
            buttonPanel.Children.Add(resetButton);

            settingsButton = new Button { Content = "Settings", Margin = new Thickness(5), Padding = new Thickness(5) };
            settingsButton.Click += SettingsButton_Click;
            buttonPanel.Children.Add(settingsButton);
        }

        private async void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(velocityInput.Text, out double v0) && double.TryParse(angleInput.Text, out double a))
            {
                // Calculate trajectory points
                Bird bird = new Bird(v0, a);
                List<Point> points = new List<Point>();
                double t = 0;
                double dt = 0.01;

                while (bird.CalculatePositionY(t) >= 0)
                {
                    double x = bird.CalculatePositionX(t);
                    double y = bird.CalculatePositionY(t);
                    points.Add(new Point(x, y)); // Store actual coordinates
                    t += dt;
                }

               
                string imagePath = "https://i.pinimg.com/originals/ec/a5/f1/eca5f16deedf6d4a2f06b08c9c6fe34d.png";

                // Show the trajectory in a new window with the background image
                TrajectoryWindow trajectoryWindow = new TrajectoryWindow(points.ToList(), imagePath);
                trajectoryWindow.Show();

                // Optional: Save data to file (keeping your original logic)
                using (StreamWriter f_out = new StreamWriter("file_output1.csv"))
                {
                    t = 0.01;
                    while (bird.CalculatePositionY(t) >= 0)
                    {
                        await f_out.WriteLineAsync($"{bird.CalculatePositionX(t):F2}; {bird.CalculatePositionY(t):F2}");
                        t += dt;
                    }
                }
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter numeric values for speed and angle.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            velocityInput.Text = "";
            angleInput.Text = "";
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Settings functionality will be implemented here.", "Settings");
        }

        void ExitOnClick(object sender, RoutedEventArgs args)
        {
            Close();
        }
    }

    public class App : Application
    {
        [STAThread]
        public static void Main()
        {
            App app = new App();
            app.Run(new MainWindow());
        }
    }
}