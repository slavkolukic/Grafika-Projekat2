﻿using pr129_2016_Slavko_Lukic_pz2.Model;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace pr129_2016_Slavko_Lukic_pz2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, Ellipse> allDots = new Dictionary<string, Ellipse>();

        private static double x = 125;
        private static double y = 100;
        private static int elipseSize = 6;

        private static List<int> XCoordinates = new List<int>();
        private static List<int> YCoordinates = new List<int>();

        private static XmlDocument xmlDoc = new XmlDocument();
        private static XmlNodeList substationNodeList;
        private static XmlNodeList switchNodeList;
        private static XmlNodeList nodeNodeList;

        private List<SubstationEntity> allSubstations = new List<SubstationEntity>();
        private List<SwitchEntity> allSwitches = new List<SwitchEntity>();
        private List<NodeEntity> allNodes = new List<NodeEntity>();

        private double maxCoordX = 0;
        private double minCoordX = Double.MaxValue;
        private double maxCoordY = 0;
        private double minCoordY = Double.MaxValue;

        public MainWindow()
        {
            InitializeComponent();
            DrawGrid(x, y);
            LoadXml();

            ParseSubstations();
            ParseNodes();
            ParseSwitches();

            DrawAllSubstations();
            DrawAllSwitches();
            DrawAllNodes();
        }

        private void DrawAllNodes()
        {
            foreach (var ss in allNodes)
            {
                double XX = (((ss.X - minCoordX) * (124 - 0)) / (maxCoordX - minCoordX)) + 0;
                double YY = (((ss.Y - minCoordY) * (99 - 0)) / (maxCoordY - minCoordY)) + 0;

                int xCoord = Convert.ToInt32(XX);
                int yCoord = Convert.ToInt32(YY);

                Ellipse e = new Ellipse();
                e.Width = elipseSize;
                e.Height = elipseSize;
                e.Fill = new SolidColorBrush(Colors.Cyan);
                e.Stroke = new SolidColorBrush(Colors.Black);
                e.StrokeThickness = 0.5;
                e.ToolTip = "NODE | id: " + ss.Id + ", name: " + ss.Name;

                Canvas.SetLeft(e, XCoordinates[xCoord] - elipseSize / 2);
                Canvas.SetTop(e, YCoordinates[yCoord] - elipseSize / 2);

                string key = xCoord.ToString() + yCoord.ToString();

                if (allDots.ContainsKey(key))
                {
                    allDots[key].Fill = new SolidColorBrush(Colors.BlueViolet);
                    allDots[key].ToolTip += "\n" + e.ToolTip;
                }
                else
                {
                    canvas.Children.Add(e);
                    allDots.Add(key, e);
                }
            }
        }

        private void DrawAllSwitches()
        {
            foreach (var ss in allSwitches)
            {
                double XX = (((ss.X - minCoordX) * (124 - 0)) / (maxCoordX - minCoordX)) + 0;
                double YY = (((ss.Y - minCoordY) * (99 - 0)) / (maxCoordY - minCoordY)) + 0;

                int xCoord = Convert.ToInt32(XX);
                int yCoord = Convert.ToInt32(YY);

                Ellipse e = new Ellipse();
                e.Width = elipseSize;
                e.Height = elipseSize;
                e.Fill = new SolidColorBrush(Colors.HotPink);
                e.Stroke = new SolidColorBrush(Colors.Black);
                e.StrokeThickness = 0.5;
                e.ToolTip = "SW | id: " + ss.Id + ", name: " + ss.Name + ", status: " + ss.Status;

                Canvas.SetLeft(e, XCoordinates[xCoord] - elipseSize / 2);
                Canvas.SetTop(e, YCoordinates[yCoord] - elipseSize / 2);

                string key = xCoord.ToString() + yCoord.ToString();

                if (allDots.ContainsKey(key))
                {
                    allDots[key].Fill = new SolidColorBrush(Colors.BlueViolet);
                    allDots[key].ToolTip += "\n" + e.ToolTip;
                }
                else
                {
                    canvas.Children.Add(e);
                    allDots.Add(key, e);
                }
            }
        }

        private void DrawAllSubstations()
        {
            foreach (var ss in allSubstations)
            {
                double XX = (((ss.X - minCoordX) * (124 - 0)) / (maxCoordX - minCoordX)) + 0;
                double YY = (((ss.Y - minCoordY) * (99 - 0)) / (maxCoordY - minCoordY)) + 0;

                int xCoord = Convert.ToInt32(XX);
                int yCoord = Convert.ToInt32(YY);

                Ellipse e = new Ellipse();
                e.Width = elipseSize;
                e.Height = elipseSize;
                e.Fill = new SolidColorBrush(Colors.Orange);
                e.Stroke = new SolidColorBrush(Colors.Black);
                e.StrokeThickness = 0.5;
                e.ToolTip = "SBS | id: " + ss.Id + ", name: " + ss.Name;

                Canvas.SetLeft(e, XCoordinates[xCoord] - elipseSize / 2);
                Canvas.SetTop(e, YCoordinates[yCoord] - elipseSize / 2);

                string key = xCoord.ToString() + yCoord.ToString();

                if (allDots.ContainsKey(key))
                {
                    allDots[key].Fill = new SolidColorBrush(Colors.BlueViolet);
                    allDots[key].ToolTip += "\n" + e.ToolTip;
                }
                else
                {
                    canvas.Children.Add(e);
                    allDots.Add(key, e);
                }
            }
        }

        private void ParseSwitches()
        {
            double newX, newY;
            foreach (XmlNode node in switchNodeList)
            {
                SwitchEntity sw = new SwitchEntity();

                sw.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                sw.Name = node.SelectSingleNode("Name").InnerText;
                sw.X = double.Parse(node.SelectSingleNode("X").InnerText);
                sw.Y = double.Parse(node.SelectSingleNode("Y").InnerText);
                sw.Status = node.SelectSingleNode("Status").InnerText;

                ToLatLon(sw.X, sw.Y, 34, out newX, out newY);
                sw.X = newX;
                sw.Y = newY;

                if (sw.X > maxCoordX)
                    maxCoordX = sw.X;
                if (sw.X < minCoordX)
                    minCoordX = sw.X;

                if (sw.Y > maxCoordY)
                    maxCoordY = sw.Y;
                if (sw.Y < minCoordY)
                    minCoordY = sw.Y;

                allSwitches.Add(sw);
            }
        }

        private void ParseNodes()
        {
            double newX, newY;
            foreach (XmlNode node in nodeNodeList)
            {
                NodeEntity ne = new NodeEntity();

                ne.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                ne.Name = node.SelectSingleNode("Name").InnerText;
                ne.X = double.Parse(node.SelectSingleNode("X").InnerText);
                ne.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                ToLatLon(ne.X, ne.Y, 34, out newX, out newY);
                ne.X = newX;
                ne.Y = newY;

                if (ne.X > maxCoordX)
                    maxCoordX = ne.X;
                if (ne.X < minCoordX)
                    minCoordX = ne.X;

                if (ne.Y > maxCoordY)
                    maxCoordY = ne.Y;
                if (ne.Y < minCoordY)
                    minCoordY = ne.Y;

                allNodes.Add(ne);
            }
        }

        private void ParseSubstations()
        {
            double newX, newY;
            foreach (XmlNode node in substationNodeList)
            {
                SubstationEntity sub = new SubstationEntity();

                sub.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                sub.Name = node.SelectSingleNode("Name").InnerText;
                sub.X = double.Parse(node.SelectSingleNode("X").InnerText);
                sub.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                ToLatLon(sub.X, sub.Y, 34, out newX, out newY);
                sub.X = newX;
                sub.Y = newY;

                if (sub.X > maxCoordX)
                    maxCoordX = sub.X;
                if (sub.X < minCoordX)
                    minCoordX = sub.X;

                if (sub.Y > maxCoordY)
                    maxCoordY = sub.Y;
                if (sub.Y < minCoordY)
                    minCoordY = sub.Y;

                allSubstations.Add(sub);
            }
        }

        private void LoadXml()
        {
            xmlDoc.Load("Geographic.xml");

            substationNodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            nodeNodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            switchNodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");
        }

        private void DrawGrid(double x, double y)
        {
            DrawVerticalLines(x);
            DrawHorizontalLines(y);
        }

        private void DrawHorizontalLines(double y)
        {
            double offset = 800 / y;

            for (double i = 0; i < y; ++i)
            {
                Line line = new Line();
                line.StrokeThickness = 0.2;
                line.Stroke = new SolidColorBrush(Colors.DimGray);
                line.X1 = 0;
                line.X2 = 1000;
                line.Y1 = offset * i;
                line.Y2 = offset * i;
                canvas.Children.Add(line);

                YCoordinates.Add((int)(offset * i));
            }
        }

        private void DrawVerticalLines(double x)
        {
            double offset = 1000 / x;

            for (double i = 0; i < x; ++i)
            {
                Line line = new Line();
                line.StrokeThickness = 0.2;
                line.Stroke = new SolidColorBrush(Colors.DimGray);
                line.X1 = offset * i;
                line.X2 = offset * i;
                line.Y1 = 0;
                line.Y2 = 800;
                canvas.Children.Add(line);

                XCoordinates.Add((int)(offset * i));
            }
        }

        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Shape clickedShape = e.OriginalSource as Shape;

            if (clickedShape != null && clickedShape.GetType().Name.ToString() == "Ellipse")
            {
                DoubleAnimation widthAnimation = new DoubleAnimation
                {
                    From = 6,
                    To = 60,
                    Duration = TimeSpan.FromSeconds(1.5)
                };

                DoubleAnimation heightAnimation = new DoubleAnimation
                {
                    From = 6,
                    To = 60,
                    Duration = TimeSpan.FromSeconds(1.5)
                };
                canvas.Children.Remove(clickedShape);
                canvas.Children.Add(clickedShape);

                Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(Ellipse.WidthProperty));
                Storyboard.SetTarget(widthAnimation, clickedShape);

                Storyboard.SetTargetProperty(heightAnimation, new PropertyPath(Ellipse.HeightProperty));
                Storyboard.SetTarget(heightAnimation, clickedShape);

                Storyboard s = new Storyboard();
                s.Children.Add(widthAnimation);
                s.Children.Add(heightAnimation);

                s.Completed += (t, r) => StoryboardCompleted(clickedShape);
                s.Begin();
            }
        }

        private void StoryboardCompleted(Shape e)
        {
            DoubleAnimation myDoubleAnimation2 = new DoubleAnimation();
            myDoubleAnimation2.From = 60;
            myDoubleAnimation2.To = 6;
            myDoubleAnimation2.Duration = new Duration(TimeSpan.FromSeconds(1.5));
            e.BeginAnimation(Ellipse.WidthProperty, myDoubleAnimation2);
            e.BeginAnimation(Ellipse.HeightProperty, myDoubleAnimation2);
        }
    }
}