using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;

namespace MeshDrawingWPF
{
	public class Cpoint2D
	{
		public double x { get; protected set; }
		public double y { get; protected set; }
        public Cpoint2D(double x, double y) => SetPoint2D(x, y);
        public void SetPoint2D(double x, double y)
		{
			this.x = x;
			this.y = y;
		}
	}

	public class Cpoint3D : Cpoint2D
	{
		public double z { get; protected set; }
        public Cpoint3D(double x, double y, double z) : base(x, y) => this.z = z;
        public void SetPoint3D(double x, double y, double z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
	}

	public class Cnode : Cpoint3D
	{
		public bool dummy { get; protected set; }
        public Cnode(double x, double y, double z, bool dummy) : base(x, y, z) => this.dummy = dummy;
        public void SetNode(double x, double y, double z, bool dummy)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.dummy = dummy;
		}
	}

	public class Chexahedron
	{
		public int[] vertexs;
		public int area;
		public Chexahedron()
		{
			this.vertexs = new int[8];
			this.area = 0;
		}
		public Chexahedron(int[] vertexs, int area)
		{
			this.area = area;
			this.vertexs = new int[8];
			for (int i = 0; i < 8; i++)
				this.vertexs[i] = vertexs[i];
		}
	}

	public class Cquadrangle
	{
		public Cpoint2D[] vertexs;
		public int area;
		public Cpoint2D center { get; protected set; }
		public Cquadrangle()
		{
			vertexs = new Cpoint2D[4];
			area = 0;
			center = new Cpoint2D(0, 0);
		}
		public Cquadrangle(Cpoint2D[] vertexs, int area)
		{
			this.area = area;
			this.vertexs = new Cpoint2D[4];
			double x = 0, y = 0;
			for (int i = 0; i < 4; i++)
			{
				this.vertexs[i] = vertexs[i];
				x += vertexs[i].x;
				y += vertexs[i].y;
			}
			center = new Cpoint2D(x / 4.0, y / 4.0);
			sortVertex();
		}
		//поиск угла между вершиной и центром масс
		private double getAngle(Cpoint2D point)
		{
			double res = Math.Atan2(center.y - point.y, point.x - center.x) * (180 / Math.PI);
			if (res < 0)
				res += 360;
			return res;
		}
		//сортировка вершин в порядке обхода
		private void sortVertex()
		{
			double[] angles = new double[4];//углы для каждой точки
			int[] index = new int[4];       //правильный порядок индексов
			for (int i = 0; i < 4; i++)
				angles[i] = getAngle(vertexs[i]);

			int maxIn;
			for (int i = 0; i < 4; i++)
			{
				maxIn = Array.IndexOf(angles, angles.Max());
				index[i] = maxIn;
				angles[maxIn] = -1;
			}

			Cpoint2D[] newVertexs = { vertexs[index[0]], vertexs[index[1]], vertexs[index[2]], vertexs[index[3]] };
			vertexs = newVertexs;
		}
	}

	public class Cmesh
	{
		public SolidColorBrush[] areaColors;

		public List<Chexahedron> fes { get; set; }//конечные элементы
		public List<Cnode> nodes { get; set; }  //узлы сетки    
		public List<Cquadrangle> fesSection { get; set; }   //к.э. в сечении

		public int axleSection { get; set; }        //ось по которой проводится плоскость сечения; x-0, y-1, z-2
		public double valueSection { get; set; }    //значение сечения

		public double minSelection { get; protected set; }  //минимум по оси сечения
		public double maxSelection { get; protected set; }  //максимум по оси сечения

		public double minAxle1 { get; protected set; }  //минимальное по оси axle1 значение в узле
		public double maxAxle1 { get; protected set; }  //максимальное по оси axle1 значение в узле
		public double minAxle2 { get; protected set; }  //минимальное по оси axle2 значение в узле
		public double maxAxle2 { get; protected set; }  //максимальное по оси axle2 значение в узле

		public Cmesh()
		{
			fesSection = new List<Cquadrangle>();
			areaColors = new SolidColorBrush[10];
			areaColors[0] = Brushes.LightBlue;
			areaColors[1] = Brushes.LightCoral;
			areaColors[2] = Brushes.LightCyan;
			areaColors[3] = Brushes.LightGoldenrodYellow;
			areaColors[4] = Brushes.LightGreen;
			areaColors[5] = Brushes.LightPink;
			areaColors[6] = Brushes.LightSalmon;
			areaColors[7] = Brushes.LightSeaGreen;
			areaColors[8] = Brushes.LightSkyBlue;
			areaColors[9] = Brushes.LightYellow;
		}
		public void searchingRange()
		{
			switch (axleSection)
			{
				case 0:
					minSelection = nodes.Min(node => node.x);
					maxSelection = nodes.Max(node => node.x);

					minAxle1 = nodes.Min(node => node.y);
					maxAxle1 = nodes.Max(node => node.y);
					minAxle2 = nodes.Min(node => node.z);
					maxAxle2 = nodes.Max(node => node.z);
					break;
				case 1:
					minSelection = nodes.Min(node => node.y);
					maxSelection = nodes.Max(node => node.y);

					minAxle1 = nodes.Min(node => node.x);
					maxAxle1 = nodes.Max(node => node.x);
					minAxle2 = nodes.Min(node => node.z);
					maxAxle2 = nodes.Max(node => node.z);
					break;
				case 2:
					minSelection = nodes.Min(node => node.z);
					maxSelection = nodes.Max(node => node.z);

					minAxle1 = nodes.Min(node => node.x);
					maxAxle1 = nodes.Max(node => node.x);
					minAxle2 = nodes.Min(node => node.y);
					maxAxle2 = nodes.Max(node => node.y);
					break;
			}
		}

		//вычисляет к.э. в сечении плоскости
		private void computingProjectionOfMesh()
		{
			//поиск конечных элементов, которые пересекает секущая плоскость
			List<int> chosenFEs = new List<int>();  //к.э. пересекаемые плоскостью
			List<Cnode> p = new List<Cnode>();  //узлы к.э.
			List<Cpoint2D> pSection = new List<Cpoint2D>();//узлы к.э. в сечении
			Cpoint3D p0, p1, p2, p3, p4, p5, p6, p7;

			fesSection.Clear();
			switch (axleSection)
			{
				case 0:
					{
						//выбор конечных элементов, которые пересекает плоскость
						for (int i = 0; i < fes.Count(); i++)
						{
							for (int j = 0; j < 8; j++)
								p.Add(nodes[fes[i].vertexs[j]]);

							double minX = p.Min(val => val.x);  //минимальное значение х на к.э.
							double maxX = p.Max(val => val.x);  //максимальное значение х на к.э.

							//если плоскость пересекает к.э.
							if (valueSection >= minX && valueSection <= maxX)
								chosenFEs.Add(i);
							p.Clear();
						}

						//построение сечения к.э. плоскостью
						for (int i = 0; i < chosenFEs.Count(); i++)
						{
							int k = chosenFEs[i];
							p0 = nodes[fes[k].vertexs[0]]; p1 = nodes[fes[k].vertexs[1]];
							p2 = nodes[fes[k].vertexs[2]]; p3 = nodes[fes[k].vertexs[3]];
							p4 = nodes[fes[k].vertexs[4]]; p5 = nodes[fes[k].vertexs[5]];
							p6 = nodes[fes[k].vertexs[6]]; p7 = nodes[fes[k].vertexs[7]];

							Cpoint2D pI1 = new Cpoint2D(0, 0), pI2 = new Cpoint2D(0, 0);  //точки "пересечения" прямой и плоскости
							int numPoints = 0;  //число точек пересечения
							bool intersection;  //было ли пересечение
							List<Tuple<Cpoint3D, Cpoint3D>> segments = new List<Tuple<Cpoint3D, Cpoint3D>>();//отрезки с которыми ищется пересечение плоскости

							segments.Add(Tuple.Create(p0, p1)); segments.Add(Tuple.Create(p1, p3));
							segments.Add(Tuple.Create(p3, p2)); segments.Add(Tuple.Create(p2, p0));
							segments.Add(Tuple.Create(p4, p5)); segments.Add(Tuple.Create(p5, p7));
							segments.Add(Tuple.Create(p7, p6)); segments.Add(Tuple.Create(p6, p4));

							for (int j = 0; j < segments.Count(); j++)
							{
								intersection = isIntersection(segments[j].Item1, segments[j].Item2, ref pI1, ref pI2, ref numPoints);
								if (intersection)
								{
									if (numPoints == 1)
									{
										pSection.Add(pI1);
									}
									if (numPoints == 2)
									{
										pSection.Add(pI1);
										pSection.Add(pI2);
									}
								}
							}
							if (pSection.Count > 4)
								pSection = deleteDuplication(pSection);

							fesSection.Add(new Cquadrangle(new Cpoint2D[4] { pSection[0], pSection[1], pSection[2], pSection[3] }, fes[k].area));
							pSection.Clear();
						}
					}
					break;
				case 1:
					{
						for (int i = 0; i < fes.Count(); i++)
						{
							for (int j = 0; j < 8; j++)
								p.Add(nodes[fes[i].vertexs[j]]);

							double minY = p.Min(val => val.y);  //минимальное значение y на к.э.
							double maxY = p.Max(val => val.y);  //максимальное значение y на к.э.

							//если плоскость пересекает к.э.
							if (valueSection >= minY && valueSection <= maxY)
								chosenFEs.Add(i);
							p.Clear();
						}

						//построение сечения к.э. плоскостью
						for (int i = 0; i < chosenFEs.Count(); i++)
						{
							int k = chosenFEs[i];
							p0 = nodes[fes[k].vertexs[0]]; p1 = nodes[fes[k].vertexs[1]];
							p2 = nodes[fes[k].vertexs[2]]; p3 = nodes[fes[k].vertexs[3]];
							p4 = nodes[fes[k].vertexs[4]]; p5 = nodes[fes[k].vertexs[5]];
							p6 = nodes[fes[k].vertexs[6]]; p7 = nodes[fes[k].vertexs[7]];

							Cpoint2D pI1 = new Cpoint2D(0, 0), pI2 = new Cpoint2D(0, 0);  //точки "пересечения" прямой и плоскости
							int numPoints = 0;  //число точек пересечения
							bool intersection;  //было ли пересечение
							List<Tuple<Cpoint3D, Cpoint3D>> segments = new List<Tuple<Cpoint3D, Cpoint3D>>();//отрезки с которыми ищется пересечение плоскости

							segments.Add(Tuple.Create(p0, p1)); segments.Add(Tuple.Create(p1, p3));
							segments.Add(Tuple.Create(p3, p2)); segments.Add(Tuple.Create(p2, p0));
							segments.Add(Tuple.Create(p4, p5)); segments.Add(Tuple.Create(p5, p7));
							segments.Add(Tuple.Create(p7, p6)); segments.Add(Tuple.Create(p6, p4));

							for (int j = 0; j < segments.Count(); j++)
							{
								intersection = isIntersection(segments[j].Item1, segments[j].Item2, ref pI1, ref pI2, ref numPoints);
								if (intersection)
								{
									if (numPoints == 1)
									{
										pSection.Add(pI1);
									}
									if (numPoints == 2)
									{
										pSection.Add(pI1);
										pSection.Add(pI2);
									}
								}
							}
							if (pSection.Count > 4)
								pSection = deleteDuplication(pSection);
							fesSection.Add(new Cquadrangle(new Cpoint2D[4] { pSection[0], pSection[1], pSection[2], pSection[3] }, fes[k].area));
							pSection.Clear();
						}
					}
					break;
				case 2:
					{
						for (int i = 0; i < fes.Count(); i++)
						{
							for (int j = 0; j < 8; j++)
								p.Add(nodes[fes[i].vertexs[j]]);

							double minZ = p.Min(val => val.z);  //минимальное значение z на к.э.
							double maxZ = p.Max(val => val.z);  //максимальное значение z на к.э.

							//если плоскость пересекает к.э.
							if (valueSection >= minZ && valueSection <= maxZ)
								chosenFEs.Add(i);
							p.Clear();
						}
						//построение сечения к.э. плоскостью
						for (int i = 0; i < chosenFEs.Count(); i++)
						{
							int k = chosenFEs[i];
                            p0 = nodes[fes[k].vertexs[0]]; p1 = nodes[fes[k].vertexs[1]];
                            p2 = nodes[fes[k].vertexs[2]]; p3 = nodes[fes[k].vertexs[3]];
                            p4 = nodes[fes[k].vertexs[4]]; p5 = nodes[fes[k].vertexs[5]];
                            p6 = nodes[fes[k].vertexs[6]]; p7 = nodes[fes[k].vertexs[7]];

                            Cpoint2D pI1 = new Cpoint2D(0, 0), pI2 = new Cpoint2D(0, 0);  //точки "пересечения" прямой и плоскости
							int numPoints = 0;  //число точек пересечения
							bool intersection;  //было ли пересечение
							List<Tuple<Cpoint3D, Cpoint3D>> segments = new List<Tuple<Cpoint3D, Cpoint3D>>();//отрезки с которыми ищется пересечение плоскости

                            segments.Add(Tuple.Create(p0, p1)); segments.Add(Tuple.Create(p1, p3));
                            segments.Add(Tuple.Create(p3, p2)); segments.Add(Tuple.Create(p2, p0));
                            segments.Add(Tuple.Create(p4, p5)); segments.Add(Tuple.Create(p5, p7));
                            segments.Add(Tuple.Create(p7, p6)); segments.Add(Tuple.Create(p6, p4));

                            for (int j = 0; j < segments.Count(); j++)
							{
								intersection = isIntersection(segments[j].Item1, segments[j].Item2, ref pI1, ref pI2, ref numPoints);
								if (intersection)
								{
									if (numPoints == 1)
									{
										pSection.Add(pI1);
									}
									if (numPoints == 2)
									{
										pSection.Add(pI1);
										pSection.Add(pI2);
									}
								}
							}
							if (pSection.Count > 4)
								pSection = deleteDuplication(pSection);

                            if(pSection.Count != 0 && pSection.Count <4)
                            {
                                int yyy = 0;
                            }

                            if (pSection.Count != 0)
                            {
                                fesSection.Add(new Cquadrangle(new Cpoint2D[4] { pSection[0], pSection[1], pSection[2], pSection[3] }, fes[k].area));
                            }
							pSection.Clear();
						}
					}
					break;
			}
		}

		//возвращает точку(и) пересечения прямой и плоскости или false, если (.) пересечения не лежит на отрезке
		private bool isIntersection(Cpoint3D p1, Cpoint3D p2, ref Cpoint2D pI1, ref Cpoint2D pI2, ref int numPoints)
		{

			switch (axleSection)
			{
				case 0://x
					{
                        if (Math.Abs(p1.x - p2.x) < 1e-12 && Math.Abs(p1.x - valueSection) < 1e-12)
                        //if (p1.x == p2.x && p1.x == valueSection)
                        {
                            pI1 = new Cpoint2D(p1.y, p1.z);
                            pI2 = new Cpoint2D(p2.y, p2.z);
                            numPoints = 2;
                            return true;
                        }
						//вычисление точек пересечения прямой и плоскости
						double y = p1.y + ((valueSection - p1.x) * (p2.y - p1.y)) / (p2.x - p1.x);
						double z = p1.z + ((valueSection - p1.x) * (p2.z - p1.z)) / (p2.x - p1.x);
						double minX, maxX, minY, maxY, minZ, maxZ;

						if (p1.x > p2.x) { maxX = p1.x; minX = p2.x; }
						else { maxX = p2.x; minX = p1.x; }
						if (p1.z > p2.z) { maxZ = p1.z; minZ = p2.z; }
						else { maxZ = p2.z; minZ = p1.z; }
						if (p1.y > p2.y) { maxY = p1.y; minY = p2.y; }
						else { maxY = p2.y; minY = p1.y; }

						//если точка принадлежит отрезку
						if (valueSection >= minX && valueSection <= maxX && y >= minY && y <= maxY && z >= minZ && z <= maxZ)
						{
							pI1 = new Cpoint2D(y, z);
							numPoints = 1;
							return true;
						}
					}
					break;
				case 1://y
					{
                        if (Math.Abs(p1.y - p2.y) < 1e-12 && Math.Abs(p1.y - valueSection) < 1e-12)
                        {
                            pI1 = new Cpoint2D(p1.x, p1.z);
                            pI2 = new Cpoint2D(p2.x, p2.z);
                            numPoints = 2;
                            return true;
                        }
						//вычисление точек пересечения прямой и плоскости
						double x = p1.x + ((valueSection - p1.y) * (p2.x - p1.x)) / (p2.y - p1.y);
						double z = p1.z + ((valueSection - p1.y) * (p2.z - p1.z)) / (p2.y - p1.y);
						double minX, maxX, minY, maxY, minZ, maxZ;

						if (p1.x > p2.x) { maxX = p1.x; minX = p2.x; }
						else { maxX = p2.x; minX = p1.x; }
						if (p1.z > p2.z) { maxZ = p1.z; minZ = p2.z; }
						else { maxZ = p2.z; minZ = p1.z; }
						if (p1.y > p2.y) { maxY = p1.y; minY = p2.y; }
						else { maxY = p2.y; minY = p1.y; }

						//если точка принадлежит отрезку
						if (x >= minX && x <= maxX && valueSection >= minY && valueSection <= maxY && z >= minZ && z <= maxZ)
						{
							pI1 = new Cpoint2D(x, z);
							numPoints = 1;
							return true;
						}
					}
					break;
				case 2://z
					{
                        if (Math.Abs(p1.z - p2.z) < 1e-12 && Math.Abs(p1.z - valueSection) < 1e-12)
                        {
                            pI1 = new Cpoint2D(p1.x, p1.y);
                            pI2 = new Cpoint2D(p2.x, p2.y);
                            numPoints = 2;
                            return true;
                        }
						//вычисление точек пересечения прямой и плоскости
						double x = p1.x + ((valueSection - p1.z) * (p2.x - p1.x)) / (p2.z - p1.z);
						double y = p1.y + ((valueSection - p1.z) * (p2.y - p1.y)) / (p2.z - p1.z);
						double minX, maxX, minY, maxY, minZ, maxZ;

						if (p1.x > p2.x) { maxX = p1.x; minX = p2.x; }
						else { maxX = p2.x; minX = p1.x; }
						if (p1.z > p2.z) { maxZ = p1.z; minZ = p2.z; }
						else { maxZ = p2.z; minZ = p1.z; }
						if (p1.y > p2.y) { maxY = p1.y; minY = p2.y; }
						else { maxY = p2.y; minY = p1.y; }

						//если точка принадлежит отрезку
						if (x >= minX && x <= maxX && y >= minY && y <= maxY && valueSection >= minZ && valueSection <= maxZ)
						{
							pI1 = new Cpoint2D(x, y);
							numPoints = 1;
							return true;
						}
					}
					break;
			}

			return false;
		}

		//удаление повторяющихся элементов в List
		private List<Cpoint2D> deleteDuplication(List<Cpoint2D> list)
		{
			for (int i = 0; i < list.Count(); i++)
			{
				Cpoint2D point = list[i];
				for (int j = i + 1; j < list.Count(); j++)
				{
					if (Math.Abs(point.x - list[j].x) < 1e-14 && Math.Abs(point.y - list[j].y) < 1e-14)
					{
						list.RemoveAt(j);
						if (j == i + 1) j--;
					}
				}
			}
			return list;
		}

		public void Draw(Canvas canvas, bool drawDummy, bool differArea, double zoom, Point shift)
		{
			//вычисление к.э. в сечении плоскости
			computingProjectionOfMesh();

			int screenX0, screenY0;  //координаты начала отрисовки сетки
			int screenXn, screenYn;  //координаты конца отрисовки сетки
			int screenWidth, screenHeight;  //размеры сетки при отрисовке
			double xh, yh;

			screenWidth = (int)canvas.Width;
			screenHeight = (int)canvas.Height;
			screenX0 = 40;
			screenY0 = 40;
			screenXn = screenWidth - screenX0;
			screenYn = screenHeight - screenY0;
			xh = (maxAxle1 - minAxle1) / (screenWidth - 2 * screenX0);
			yh = (maxAxle2 - minAxle2) / (screenHeight - 2 * screenY0);

			//удалить все элементы
			canvas.Children.Clear();

            //рисование сетки
            List<Tuple<int, int>> areaswithcolor = new List<Tuple<int, int>>(); //номер подобласти, номер цвета
                                                                                //каждой области присвоить свой цвет
            for (int i = 0; i < fes.Count(); i++)
            {
                //найти номер цвета для данной области
                Tuple<int, int> index = areaswithcolor.Find(x => x.Item1 == fes[i].area);
                if (index == null)
                {
                    //отметить, что этой области принадлежит данный цвет
                    areaswithcolor.Add(new Tuple<int, int>(fes[i].area, areaswithcolor.Count()));
                }
            }

            for (int i = 0; i < fesSection.Count(); i++)
            {
                Point[] rect = new Point[4];
                PointCollection fesPoints = new PointCollection();  //коллекция точек для полигона
                for (int j = 0; j < 4; j++)
                {
                    rect[j].X = (float)(fesSection[i].vertexs[j].x / xh) * zoom + shift.X + screenX0;
                    rect[j].Y = screenHeight - (float)(fesSection[i].vertexs[j].y / yh) * zoom + shift.Y - screenY0;

                    fesPoints.Add(rect[j]);
                }
                if (fesSection[i].area != -1)
                {
                    //различать области по цветам
                    if (differArea)
                    {
                        //найти номер цвета для данной области
                        Tuple<int, int> index = areaswithcolor.Find(x => x.Item1 == fesSection[i].area);
                        Polygon fes1 = new Polygon();
                        fes1.Stroke = System.Windows.Media.Brushes.Black;
                        fes1.Fill = areaColors[index.Item2];
                        fes1.StrokeThickness = 2;
                        fes1.Points = fesPoints;
                        canvas.Children.Add(fes1);
                    }

                    Polygon fes = new Polygon();
                    fes.Stroke = System.Windows.Media.Brushes.Black;
                    fes.StrokeThickness = 2;
                    fes.Points = fesPoints;
                    canvas.Children.Add(fes);
                }
                else
                {
                    //рисовать фиктивные элементы
                    if (drawDummy)
                    {
                        Polygon fes = new Polygon();
                        fes.Stroke = System.Windows.Media.Brushes.Gray;
                        fes.StrokeThickness = 1;
                        fes.Points = fesPoints;
                        canvas.Children.Add(fes);
                    }
                }
            }





            //вертикальная координатная линия
            Line axle1 = new Line();
			axle1.Stroke = Brushes.Black;
			axle1.X1 = screenX0;
			axle1.Y1 = 0;
			axle1.X2 = screenX0;
			axle1.Y2 = screenHeight;
			axle1.StrokeThickness = 2;
			canvas.Children.Add(axle1);

			//горизонтальная координатная линия
			Line axle2 = new Line();
			axle2.Stroke = Brushes.Black;
			axle2.X1 = 0;
			axle2.Y1 = screenYn;
			axle2.X2 = screenWidth;
			axle2.Y2 = screenYn;
			axle2.StrokeThickness = 2;
			canvas.Children.Add(axle2);

			//подпись вертикальной оси
			TextBlock axle1Name = new TextBlock();
			axle1Name.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
			axle1Name.FontFamily = new FontFamily("Arial");
			axle1Name.FontSize = 16;
			Canvas.SetLeft(axle1Name, screenX0 - 20);
			Canvas.SetTop(axle1Name, screenY0 - 30);

			//подпись горизонтальной оси
			TextBlock axle2Name = new TextBlock();
			axle2Name.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
			axle2Name.FontFamily = new FontFamily("Arial");
			axle2Name.FontSize = 16;
			Canvas.SetLeft(axle2Name, screenXn + 20);
			Canvas.SetTop(axle2Name, screenYn + 10);
			switch(axleSection)
			{
				case 0:
					axle1Name.Text = "z";
					axle2Name.Text = "y";
					break;
				case 1:
					axle1Name.Text = "z";
					axle2Name.Text = "x";
					break;
				case 2:
					axle1Name.Text = "y";
					axle2Name.Text = "x";
					break;
			}
			canvas.Children.Add(axle1Name);
			canvas.Children.Add(axle2Name);


			//подписи к координатным осям
			int naxle1 = 10;    //число делений по горизонтальной оси
			int naxle2 = 10;    //число делений по вертикальной оси
			int hatchLength = 3;    //длина штриха

            if(zoom > 1)
            {
                int yyy = (int)(zoom / 0.25) - 4;
                if (yyy > 10)
                    yyy = 10;
                naxle1 *= yyy;
                naxle2 *= yyy;
            }

			double[] axle1Values = new double[naxle1 + 1];  //значения для подписи на горизонтальной оси
			double[] axle2Values = new double[naxle2 + 1];  //значения для подписи на вертикальной оси
			double haxle1 = (maxAxle1 - minAxle1) / (double)naxle1; //шаг по горизонтальной оси
			double haxle2 = (maxAxle2 - minAxle2) / (double)naxle2; //шаг по вертикальной оси
			Point[] axle1ValuesCoord = new Point[axle1Values.Count()];//координаты для подписи на горизонтальной оси
			Point[] axle2ValuesCoord = new Point[axle2Values.Count()];//координаты для подписи на вертикальной оси

			//для горизонтальной оси
			for (int i = 0; i < axle1Values.Count(); i++)
			{
				axle1Values[i] = minAxle1 + haxle1 * i;
				axle1ValuesCoord[i] = new Point((int)((float)(axle1Values[i] / xh)*zoom +shift.X + screenX0), screenYn);

				//текстовый блок для подписи значений на осях
				TextBlock sign = new TextBlock();
				sign.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
				sign.FontFamily = new FontFamily("Arial");
				sign.FontSize = 12;
				sign.Text = axle1Values[i].ToString("F1");
				Canvas.SetLeft(sign, axle1ValuesCoord[i].X);
				Canvas.SetTop(sign, axle1ValuesCoord[i].Y);
				canvas.Children.Add(sign);

				//отметка на координатной линии
				Line hatch = new Line();
				hatch.Stroke = Brushes.Red;
				hatch.X1 = axle1ValuesCoord[i].X;
				hatch.Y1 = axle1ValuesCoord[i].Y + 3 * hatchLength;
				hatch.X2 = axle1ValuesCoord[i].X;
				hatch.Y2 = axle1ValuesCoord[i].Y - hatchLength;
				hatch.StrokeThickness = 1;
				canvas.Children.Add(hatch);
			}
			//для вертикальной оси
			for (int i = 0; i < axle2Values.Count(); i++)
			{
				axle2Values[i] = minAxle2 + haxle2 * i;
				axle2ValuesCoord[i] = new Point(screenX0 - 30, (int)(screenHeight - (float)(axle2Values[i] / yh)*zoom+shift.Y - screenY0));

				//текстовый блок для подписи значений на осях
				TextBlock sign = new TextBlock();
				sign.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
				sign.FontFamily = new FontFamily("Arial");
				sign.FontSize = 12;
				sign.Text = axle2Values[i].ToString("F1");
				Canvas.SetLeft(sign, axle2ValuesCoord[i].X);
				Canvas.SetTop(sign, axle2ValuesCoord[i].Y);
				canvas.Children.Add(sign);

				//отметка на координатной линии
				Line hatch = new Line();
				hatch.Stroke = Brushes.Red;
				hatch.X1 = 30 + axle2ValuesCoord[i].X + hatchLength;
				hatch.Y1 = axle2ValuesCoord[i].Y;
				hatch.X2 = 30 + axle2ValuesCoord[i].X - 3 * hatchLength;
				hatch.Y2 = axle2ValuesCoord[i].Y;
				hatch.StrokeThickness = 1;
				canvas.Children.Add(hatch);
			}


			////рисование сетки
			//List<Tuple<int, int>> areaswithcolor = new List<Tuple<int, int>>(); //номер подобласти, номер цвета
			////каждой области присвоить свой цвет
			//for (int i = 0; i < fes.Count(); i++)
			//{
			//	//найти номер цвета для данной области
			//	Tuple<int, int> index = areaswithcolor.Find(x => x.Item1 == fes[i].area);
			//	if (index == null)
			//	{
			//		//отметить, что этой области принадлежит данный цвет
			//		areaswithcolor.Add(new Tuple<int, int>(fes[i].area, areaswithcolor.Count()));
			//	}
			//}

			//for (int i = 0; i < fesSection.Count(); i++)
			//{
			//	Point[] rect = new Point[4];
			//	PointCollection fesPoints = new PointCollection();	//коллекция точек для полигона
			//	for (int j = 0; j < 4; j++)
			//	{
			//		rect[j].X = (float)(fesSection[i].vertexs[j].x / xh)*zoom+shift.X + screenX0;
			//		rect[j].Y = screenHeight - (float)(fesSection[i].vertexs[j].y / yh)*zoom+shift.Y - screenY0;

   //                 fesPoints.Add(rect[j]);
			//	}
			//	if (fesSection[i].area != -1)
			//	{
			//		//различать области по цветам
			//		if (differArea)
			//		{
			//			//найти номер цвета для данной области
			//			Tuple<int, int> index = areaswithcolor.Find(x => x.Item1 == fesSection[i].area);
			//				Polygon fes1 = new Polygon();
			//				fes1.Stroke = System.Windows.Media.Brushes.Black;
			//				fes1.Fill = areaColors[index.Item2];
			//				fes1.StrokeThickness = 2;
			//				fes1.Points = fesPoints;
			//				canvas.Children.Add(fes1);
			//		}

			//		Polygon fes = new Polygon();
			//		fes.Stroke = System.Windows.Media.Brushes.Black;
			//		fes.StrokeThickness = 2;
			//		fes.Points = fesPoints;
			//		canvas.Children.Add(fes);
			//	}
			//	else
			//	{
			//		//рисовать фиктивные элементы
			//		if (drawDummy)
			//		{
			//			Polygon fes = new Polygon();
			//			fes.Stroke = System.Windows.Media.Brushes.Gray;
			//			fes.StrokeThickness = 1;
			//			fes.Points = fesPoints;
			//			canvas.Children.Add(fes);
			//		}
			//	}
			//}


		}
	}
}
