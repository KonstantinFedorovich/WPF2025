using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Linq;

namespace Program
{
	public class MainWindow : Window
	{
		private TextBox outputBox;
		private Button calculateButton;
		private double initialVelocity;
		private double angle;
		private bool hasVelocity = false;
		private bool hasAngle = false;

		[STAThread]
		public static void Main()
		{
			Application app = new Application();
			app.Run(new MainWindow());
		}

		public MainWindow()
		{
			Title = "Ballistic Trajectory Calculator";
			Width = 600;
			Height = 400;

			// Создаем основной контейнер
			var stackPanel = new StackPanel();

			// Текстовое поле для ввода/вывода
			outputBox = new TextBox
			{
				IsReadOnly = false,
				AcceptsReturn = true,
				TextWrapping = TextWrapping.Wrap,
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				Height = 300,
				Margin = new Thickness(10)
			};

			// Кнопка для расчета
			calculateButton = new Button
			{
				Content = "Calculate Trajectory",
				Margin = new Thickness(10),
				Padding = new Thickness(5),
				IsEnabled = false
			};
			calculateButton.Click += async (sender, e) => await CalculateTrajectoryAsync();

			stackPanel.Children.Add(outputBox);
			stackPanel.Children.Add(calculateButton);
			Content = stackPanel;

			// Начальное сообщение
			outputBox.AppendText("Enter initial velocity (m/s):\n");
			outputBox.CaretIndex = outputBox.Text.Length;

			// Обработчики событий
			outputBox.PreviewKeyDown += OnPreviewKeyDown;
		}

		private void OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				e.Handled = true;
				ProcessInput();
			}
		}

		private void ProcessInput()
		{
			string input = outputBox.Text.Split('\n').Last().Trim();

			if (!hasVelocity)
			{
				if (double.TryParse(input, out initialVelocity) && initialVelocity > 0)
				{
					hasVelocity = true;
					outputBox.AppendText("\nEnter angle (degrees):\n");
				}
				else
				{
					outputBox.AppendText("\nInvalid velocity! Please enter a positive number:\n");
				}
			}
			else if (!hasAngle)
			{
				if (double.TryParse(input, out angle) && angle > 0 && angle < 90)
				{
					hasAngle = true;
					calculateButton.IsEnabled = true;
					outputBox.AppendText("\nPress 'Calculate Trajectory' button to start\n");
					outputBox.IsReadOnly = true;
				}
				else
				{
					outputBox.AppendText("\nInvalid angle! Please enter a value between 0 and 90:\n");
				}
			}
		}

		private async Task CalculateTrajectoryAsync()
		{
			calculateButton.IsEnabled = false;
			outputBox.AppendText("\nCalculating trajectory...\n");

			double radians = angle * Math.PI / 180;
			const double g = 9.80665;
			double t = 0;
			double totalTime = (2 * initialVelocity * Math.Sin(radians)) / g;

			outputBox.AppendText($"Total flight time: {totalTime:F2} s\n\n");

			try
			{
				while (true)
				{
					t += 0.1;
					double y = initialVelocity * Math.Sin(radians) * t - (g * t * t) / 2;
					double x = initialVelocity * Math.Cos(radians) * t;

					if (y <= 0)
					{
						outputBox.AppendText($"Final position: X = {x:F2} m\n");
						break;
					}

					outputBox.AppendText($"Time: {t:F2} s | X: {x:F2} m | Y: {y:F2} m\n");

					// Прокрутка к последней строке
					outputBox.ScrollToEnd();

					await Task.Delay(100);
				}

				outputBox.AppendText("\nCalculation complete!\n");
			}
			catch (Exception ex)
			{
				outputBox.AppendText($"\nError: {ex.Message}\n");
			}
			finally
			{
				calculateButton.IsEnabled = true;
			}
		}
	}
}