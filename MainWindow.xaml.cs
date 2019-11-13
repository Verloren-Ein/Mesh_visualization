using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;




namespace MeshDrawingWPF
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	/// 
	public partial class MainWindow : Window
	{
		int minHeight;  //минимально допустимая высота окна
		int minWidth;   //минимально допустимая ширина окна

		int curHeight;  //текущая высота окна
		int curWidth;   //текущая ширина окна

        double zoom = 1;    //коэффициент масштабирования изображения
        Point shift = new Point(0, 0);//сдвиг
        Point pointPos = new Point(0, 0);
        Point oldShift = new Point(0, 0);

        public string initialSettings = Directory.GetCurrentDirectory().ToString() + "\\initial_settings.txt"; //имя файла в котором хранятся начальные настройки формы
		public string initialDirectory;                 //начальная директория файлового диалога
		public string filter;                           //форматы файлового диалога

		StreamReader reader;
		StreamWriter writer;
		string line;        //одна строка из файла            
		string[] splitline; //строка line, разбитая на подстроки

		public static string pathNodes; //путь к файлу, содержащему узлы
		public static string pathFEs;   //путь к файлу, содержащему к.э.
		bool setPathNodes = false;      //получен ли путь к файлу с узлами
		bool setPathFEs = false;        //получен ли путь к файлу с к.э.
		bool readyToDraw = false;       //можно ли начинать отрисовку сетки
		bool drawDummy = false;         //рисовать ли фиктивные элементы
		bool differArea = false;        //расскрашивать разные подобласти разным цветом

		public Cmesh mesh;  //сетка

		//информация о сетке
		int nNodes;     //число всех узлов
		int nFEs;       //число всех конечных элементов
		int nDummyNodes;//число фиктивных узлов
		int nDummyFEs;  //число фиктивных конечных элементов
		int nAreas;     //число подобластей

		//диалоговое окно
		Microsoft.Win32.OpenFileDialog dlg;
		//окно для сохранения
		Microsoft.Win32.SaveFileDialog sdlg;

		//регулярное выражение для ввода только чисел
		Regex inputRegex = new Regex(@"[\b]|[0-9.-]");

		string currentTextboxVal;

		public MainWindow()
		{
			InitializeComponent();
			canvas.ClipToBounds = true;	//запретить выход за границы элемента

			dlg = new Microsoft.Win32.OpenFileDialog();
			sdlg = new Microsoft.Win32.SaveFileDialog();

			//создать новую сетку
			mesh = new Cmesh();
			mesh.axleSection = comboBox.SelectedIndex;

			nNodes = 0;
			nFEs = 0;
			nDummyNodes = 0;
			nDummyFEs = 0;
			nAreas = 0;

			curHeight = minHeight = (int)this.MinHeight;    //текущий размер окна =
			curWidth = minWidth = (int)this.MinWidth;       //минимальному размеру окна

			//определить фильтер файлов
			filter = "txt files (*.txt)|*.txt";
			//открыть файл начальных настроек
			reader = new StreamReader(initialSettings);
			//если файл пуст
			if (reader.EndOfStream == true)
			{
				//установить начальный каталог по умолчанию
				initialDirectory = "d:\\";
			}
			else
			{
				//установить начальным каталогом предыдущий использованный каталог
				line = reader.ReadLine().ToString();
				initialDirectory = line;
			}
			reader.Close();

			dlg.DefaultExt = ".txt";//расширение по умолчанию
			dlg.Filter = filter;    //фильтр файлов
			dlg.InitialDirectory = initialDirectory;//начальная директория поиска
			dlg.RestoreDirectory = true;

			//отключить элементы управления до загрузки файлов
			label_Copy.IsEnabled = false;
			comboBox.IsEnabled = false;
			label2.IsEnabled = false;
			label3.IsEnabled = false;
			label4.IsEnabled = false;
			label4_Copy.IsEnabled = false;
			textBox.IsEnabled = false;
			slider.IsEnabled = false;
			label5.IsEnabled = false;
			checkBox.IsEnabled = false;
			checkBox1.IsEnabled = false;
			button2.IsEnabled = false;
			label7.Content = "";
		}

		//чтение файлов узлов и к.э.
		private bool loadData()
		{
			try
			{
				nNodes = 0;
				nFEs = 0;
				nDummyNodes = 0;
				nDummyFEs = 0;
				nAreas = 0;

				//чтение файла узлов
				reader = new StreamReader(pathNodes);
                int number;
				double x, y, z;
				int dummy;
				mesh.nodes = new List<Cnode>();

				line = reader.ReadLine().ToString();
				Int32.TryParse(line, out nNodes);
				while (reader.EndOfStream != true)
				{
					line = reader.ReadLine().ToString();
					splitline = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    Int32.TryParse(splitline[0], out number);
                    Double.TryParse(splitline[1], NumberStyles.Float, CultureInfo.InvariantCulture, out x);
					Double.TryParse(splitline[2], NumberStyles.Float, CultureInfo.InvariantCulture, out y);
					Double.TryParse(splitline[3], NumberStyles.Float, CultureInfo.InvariantCulture, out z);
					Int32.TryParse(splitline[4], out dummy);

					if (dummy == 1)
						mesh.nodes.Add(new Cnode(x, y, z, false));
					else
					{
						//фиктивный узел
						mesh.nodes.Add(new Cnode(x, y, z, true));
						nDummyNodes++;
					}
				}
				reader.Close();

				//чтение файлов конечных элементов
				reader = new StreamReader(pathFEs);
				int[] n = new int[8];
				int area;

				mesh.fes = new List<Chexahedron>();

				line = reader.ReadLine().ToString();
				Int32.TryParse(line, out nFEs);

				int[] areas = new int[nFEs];
				int j = 0;
				while (reader.EndOfStream != true)
				{
					line = reader.ReadLine().ToString();
					splitline = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
					for (int i = 0; i < 8; i++)
					{
						Int32.TryParse(splitline[i], out n[i]);
					}
					Int32.TryParse(splitline[8], out area);
					mesh.fes.Add(new Chexahedron(n, area));
					areas[j] = area;
					j++;
					if (area == -1)
						nDummyFEs++;
                    Int32.TryParse(splitline[9], out area);
                }
				reader.Close();
				IEnumerable<int> distinctAreas = areas.Distinct();
				if (nDummyFEs > 0)
					nAreas = distinctAreas.Count() - 1;
				else
					nAreas = distinctAreas.Count();

				initialDirectory = pathFEs.Replace(dlg.SafeFileName, "");
				writer = new StreamWriter(initialSettings);
				writer.WriteLine(initialDirectory);
				writer.Close();

				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при чтении.\nПроверьте корректность файлов.\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				setPathFEs = false;
				setPathNodes = false;
				label1.Content = "не выбран";
				label1_Copy.Content = "не выбран";

				readyToDraw = false;
				canvas.Children.Clear();

				//отключить элементы управления до загрузки файлов
				label_Copy.IsEnabled = false;
				comboBox.IsEnabled = false;
				label2.IsEnabled = false;
				label3.IsEnabled = false;
				label4.IsEnabled = false;
				label4_Copy.IsEnabled = false;
				textBox.IsEnabled = false;
				slider.IsEnabled = false;
				label5.IsEnabled = false;
				checkBox.IsEnabled = false;
				checkBox1.IsEnabled = false;
				button2.IsEnabled = false;
				return false;
			}
		}

		private void tryDraw()
		{
			if (readyToDraw)
			{
				try
				{
					mesh.Draw(canvas, drawDummy, differArea, zoom, shift);
				}
				catch (Exception ex)
				{
					readyToDraw = false;
					MessageBox.Show("Ошибка при чтении.\nПроверьте корректность файлов.\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
					setPathFEs = false;
					setPathNodes = false;
					label1.Content = "не выбран";
					label1_Copy.Content = "не выбран";

					canvas.Children.Clear();
					//отключить элементы управления до загрузки файлов
					label_Copy.IsEnabled = false;
					comboBox.IsEnabled = false;
					label2.IsEnabled = false;
					label3.IsEnabled = false;
					label4.IsEnabled = false;
					label4_Copy.IsEnabled = false;
					textBox.IsEnabled = false;
					slider.IsEnabled = false;
					label5.IsEnabled = false;
					checkBox.IsEnabled = false;
					checkBox1.IsEnabled = false;
					button2.IsEnabled = false;
				}
			}
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			dlg.FileName = "nodes";	//имя файла по умолчанию
			bool? result = dlg.ShowDialog();
			if (result == true)
			{
				pathNodes = dlg.FileName;
				setPathNodes = true;
			}
			if (setPathNodes)
				label1.Content = "выбран";

			//если выбраны пути для обоих файлов
			if (setPathNodes && setPathFEs)
			{
				if (loadData())
				{
					//включить элементы управления
					label_Copy.IsEnabled = true;
					comboBox.IsEnabled = true;
					label2.IsEnabled = true;
					label3.IsEnabled = true;
					label4.IsEnabled = true;
					label4_Copy.IsEnabled = true;
					textBox.IsEnabled = true;
					slider.IsEnabled = true;
					label5.IsEnabled = true;
					checkBox.IsEnabled = true;
					checkBox1.IsEnabled = true;
					button2.IsEnabled = true;
					label7.Content = "Число узлов: " + nNodes.ToString() + "; Число к.э.: " + nFEs.ToString() + "; Число фиктивных узлов: " + nDummyNodes.ToString() + "; Число фиктивных к.э.: " + nDummyFEs.ToString() + "; Число областей: " + nAreas.ToString() + ".";

					//определить границы интервала сечения
					mesh.searchingRange();
					label4.Content = mesh.minSelection.ToString();
					label4_Copy.Content = mesh.maxSelection.ToString();
					//установить начальное значение сечения как минимальное
					textBox.Text = mesh.minSelection.ToString();
					//запомнить последнее верное значение textBox
					currentTextboxVal = textBox.Text;

					slider.Minimum = mesh.minSelection;
					slider.Maximum = mesh.maxSelection;
					//переместить трэкбар в начальное положение
					slider.Value = slider.Minimum;

					readyToDraw = true;
					tryDraw();
				}
			}
		}

		private void button1_Click(object sender, RoutedEventArgs e)
		{
			dlg.FileName = "fes"; //имя файла по умолчанию
			bool? result = dlg.ShowDialog();
			if (result == true)
			{
				pathFEs = dlg.FileName;
				setPathFEs = true;
			}
			if (setPathFEs)
				label1_Copy.Content = "выбран";

			//если выбраны пути для обоих файлов
			if (setPathNodes && setPathFEs)
			{
				if (loadData())
				{
					//comboBox1_SelectedIndexChanged(sender, e);
					//groupBoxSection.Enabled = true;
					//groupBoxParamDrawing.Enabled = true;
					//buttonSave.Enabled = true;
					//groupBoxLoadFiles.Enabled = false;

					//включить элементы управления
					label_Copy.IsEnabled = true;
					comboBox.IsEnabled = true;
					label2.IsEnabled = true;
					label3.IsEnabled = true;
					label4.IsEnabled = true;
					label4_Copy.IsEnabled = true;
					textBox.IsEnabled = true;
					slider.IsEnabled = true;
					label5.IsEnabled = true;
					checkBox.IsEnabled = true;
					checkBox1.IsEnabled = true;
					button2.IsEnabled = true;
					label7.Content = "Число узлов: " + nNodes.ToString() + "; Число к.э.: " + nFEs.ToString() + "; Число фиктивных узлов: " + nDummyNodes.ToString() + "; Число фиктивных к.э.: " + nDummyFEs.ToString() + "; Число областей: " + nAreas.ToString() + ".";

					//определить границы интервала сечения
					mesh.searchingRange();
					label4.Content = mesh.minSelection.ToString();
					label4_Copy.Content = mesh.maxSelection.ToString();
					//установить начальное значение сечения как минимальное
					textBox.Text = mesh.minSelection.ToString();
					//запомнить последнее верное значение textBox
					currentTextboxVal = textBox.Text;

					slider.Minimum = mesh.minSelection;
					slider.Maximum = mesh.maxSelection;
					//переместить трэкбар в начальное положение
					slider.Value = slider.Minimum;

					readyToDraw = true;
					tryDraw();
				}
			}
		}

		private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (setPathNodes && setPathFEs)
			{
				//передать текущую ось сечения
				mesh.axleSection = comboBox.SelectedIndex;
				//найти границы по оси сечения
				mesh.searchingRange();
				//устновить минимум и максимум
				label4.Content = mesh.minSelection.ToString();
				label4_Copy.Content = mesh.maxSelection.ToString();
				//установить начальное значение сечения как минимальное
				textBox.Text = mesh.minSelection.ToString();
				//запомнить последнее верное значение textBox
				currentTextboxVal = textBox.Text;

				slider.Minimum = mesh.minSelection;
				slider.Maximum = mesh.maxSelection;

				//переместить трэкбар в начальное положение
				slider.Value = slider.Minimum;

				tryDraw();
			}
		}

		private void label4_MouseDown(object sender, MouseButtonEventArgs e)
		{
			textBox.Text = mesh.minSelection.ToString();
			slider.Value = slider.Minimum;
			//запомнить последнее верное значение textBox
			currentTextboxVal = textBox.Text;
			//if (setPathNodes && setPathFEs && textBox.Text != "")
			//{
			//	readyToDraw = true;
			//	pictureBox1.Invalidate();
			//}
			tryDraw();
		}

		private void label4_Copy_MouseDown(object sender, MouseButtonEventArgs e)
		{
			textBox.Text = mesh.maxSelection.ToString();
			//запомнить последнее верное значение textBox
			currentTextboxVal = textBox.Text;
			slider.Value = slider.Maximum;
			tryDraw();
		}

		private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			textBox.Text = slider.Value.ToString().Replace(",", ".");
			//запомнить последнее верное значение textBox
			currentTextboxVal = textBox.Text;
			mesh.valueSection = slider.Value;


			//readyToDraw = true;
			//pictureBox1.Invalidate();
			tryDraw();
		}

		private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			//проверка, подходит ли введенный символ под установленное правило
			Match match = inputRegex.Match(e.Text);
			//если нет
			if (!match.Success)
			{
				//то обработка события прекращается и ввода неправильного символа не происходит
				e.Handled = true;
			}
		}

		private void textBox_KeyDown(object sender, KeyEventArgs e)
		{
			//если был нажат Enter
			if (e.Key == Key.Enter)
			{
				string stringValue = textBox.Text;
				double doubleValue = 0;
				bool parsed = Double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out doubleValue);
				if(parsed)
				{
					if (doubleValue > mesh.maxSelection)
						doubleValue = mesh.maxSelection;
					if (doubleValue < mesh.minSelection)
						doubleValue = mesh.minSelection;

					mesh.valueSection = doubleValue;
					slider.Value = doubleValue;
					textBox.Text = doubleValue.ToString();
					currentTextboxVal = doubleValue.ToString();
				}
				else
				{
					textBox.Text = currentTextboxVal;
					textBox.SelectAll();
					textBox.Focus();
				}
				tryDraw();
			}
		}

		private void checkBox_Checked(object sender, RoutedEventArgs e)
		{
			drawDummy = true;
			tryDraw();
		}

		private void checkBox_Unchecked(object sender, RoutedEventArgs e)
		{
			drawDummy = false;
			tryDraw();
		}

		private void checkBox1_Checked(object sender, RoutedEventArgs e)
		{
			differArea = true;
			tryDraw();
		}

		private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
		{
			differArea = false;
			tryDraw();
		}

		private void win_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			int w = 0;
			int h = 0;

			if (this.WindowState == System.Windows.WindowState.Maximized)
			{
				w = (int)SystemParameters.WorkArea.Width - curWidth;
				h = (int)SystemParameters.WorkArea.Height - curHeight;
				curWidth = (int)SystemParameters.WorkArea.Width;
				curHeight = (int)SystemParameters.WorkArea.Height;
			}
			if(this.WindowState == System.Windows.WindowState.Normal)
			{
				w = (int)this.Width - curWidth;
				h = (int)this.Height - curHeight;
				curWidth = (int)this.Width;
				curHeight = (int)this.Height;
			}
			canvas.Width = canvas.Width + w;
			canvas.Height = canvas.Height + h;

			tryDraw();
		}

		private void button2_Click(object sender, RoutedEventArgs e)
		{
			sdlg.Filter = "png files (*.png)|*.png";
			sdlg.RestoreDirectory = true;
			bool? result = sdlg.ShowDialog();

			if (result == true)
			{
				string savePath = sdlg.FileName;
				RenderTargetBitmap bmp = new RenderTargetBitmap(curWidth, curHeight, 96d, 96d, System.Windows.Media.PixelFormats.Default);
				bmp.Render(canvas);

				var crop = new CroppedBitmap(bmp, new Int32Rect((int)canvas.Margin.Left, (int)canvas.Margin.Top, (int)canvas.RenderSize.Width, (int)canvas.RenderSize.Height));

				BitmapEncoder pngEncoder = new PngBitmapEncoder();
				pngEncoder.Frames.Add(BitmapFrame.Create(crop));

				using (var fs = System.IO.File.OpenWrite(savePath))
				{
					pngEncoder.Save(fs);
				}
			}
		}

		private void AboutProgramm_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Очень интересная информация о программе\n2017", "О программе", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			Environment.Exit(0);
		}

		private void New_Click(object sender, RoutedEventArgs e)
		{
			readyToDraw = false;
			setPathFEs = false;
			setPathNodes = false;
			label1.Content = "не выбран";
			label1_Copy.Content = "не выбран";

			canvas.Children.Clear();

			//отключить элементы управления до загрузки файлов
			label_Copy.IsEnabled = false;
			comboBox.IsEnabled = false;
			label2.IsEnabled = false;
			label3.IsEnabled = false;
			label4.IsEnabled = false;
			label4_Copy.IsEnabled = false;
			textBox.IsEnabled = false;
			slider.IsEnabled = false;
			label5.IsEnabled = false;
			checkBox.IsEnabled = false;
			checkBox1.IsEnabled = false;
			button2.IsEnabled = false;
			label7.Content = "";
		}

        private void canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //увеличить масштаб
            if (e.Delta > 0)
            {
                if (zoom - 0.25 > 0)
                    zoom -= 0.25;
            }
            //уменьшить масштаб
            if (e.Delta < 0)
            {
                zoom += 0.25;
            }
            tryDraw();

            //shift = Mouse.GetPosition(Application.Current.MainWindow);
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            //если нажата левая кнопка мыши
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point pt = e.GetPosition(canvas);
                
                if (-pointPos.X + pt.X != 0 || -pointPos.Y + pt.Y != 0)
                {
                   // shift = new Point(-pointPos.X + pt.X, -pointPos.Y + pt.Y);
                    shift.X = oldShift.X - pointPos.X + pt.X;
                    shift.Y = oldShift.Y - pointPos.Y + pt.Y;
                }  
                tryDraw();
            }
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pointPos = e.GetPosition(canvas);
            oldShift = shift;
        }
    }
}
