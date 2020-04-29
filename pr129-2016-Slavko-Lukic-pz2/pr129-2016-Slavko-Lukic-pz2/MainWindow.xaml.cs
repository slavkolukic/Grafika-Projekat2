using pr129_2016_Slavko_Lukic_pz2.Model;
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
using Point = pr129_2016_Slavko_Lukic_pz2.Model.Point;

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
        private static XmlNodeList lineNodeList;

        private List<SubstationEntity> allSubstations = new List<SubstationEntity>();
        private List<SwitchEntity> allSwitches = new List<SwitchEntity>();
        private List<NodeEntity> allNodes = new List<NodeEntity>();
        private List<LineEntity> allLines = new List<LineEntity>();

        private double maxCoordX = 0;
        private double minCoordX = Double.MaxValue;
        private double maxCoordY = 0;
        private double minCoordY = Double.MaxValue;

        private Dictionary<long, PowerEntity> allPowerEntities = new Dictionary<long, PowerEntity>();

        private List<Line> lns = new List<Line>();
        private List<Point> pts = new List<Point>();

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

            ParseLines();
            DrawAllLines();

            foreach (var a in lns)
            {
                foreach (var b in lns)
                {
                    Point ix = FindIntersection(a, b);
                    //Point ix = FindLineIntersection(a, b);
                    if (ix != null)
                        pts.Add(ix);
                }
            }

            foreach (var a in pts)
            {
                double yOffset = 800 / y;
                double xOffset = 1000 / x;

                Rectangle e = new Rectangle();
                e.Width = 1;
                e.Height = 1;
                e.Fill = new SolidColorBrush(Colors.YellowGreen);

                Canvas.SetLeft(e, a.X - 0.5);
                Canvas.SetTop(e, a.Y - 0.5);

                canvas.Children.Add(e);
            }
        }

        private Point FindLineIntersection(Line lineA, Line lineB)
        {
            if (lineA == lineB)
                return null;

            Point retVal = new Point();

            Point P1 = new Point();
            Point Q1 = new Point();

            P1.X = lineA.X1;
            P1.Y = lineA.Y1;

            Q1.X = lineA.X2;
            Q1.Y = lineA.Y2;

            double a1 = Q1.Y - P1.Y;
            double b1 = P1.X - Q1.X;
            double c1 = a1 * (P1.X) + b1 * (P1.Y);

            Point P2 = new Point();
            Point Q2 = new Point();

            P2.X = lineB.X1;
            P2.Y = lineB.Y1;

            Q2.X = lineB.X2;
            Q2.Y = lineB.Y2;

            double a2 = Q2.Y - P2.Y;
            double b2 = P2.X - Q2.X;
            double c2 = a2 * (P2.X) + b2 * (P2.Y);

            double delta = a1 * b2 - a2 * b1;

            if (delta == 0)
                return null;

            double xx = (b2 * c1 - b1 * c2) / delta;
            double yy = (a1 * c2 - a2 * c1) / delta;

            retVal.X = xx;
            retVal.Y = yy;

            return retVal;
        }

        private static Point FindIntersection(Line lineA, Line lineB, double tolerance = 0.001)
        {
            double x1 = lineA.X1, y1 = lineA.Y1;
            double x2 = lineA.X2, y2 = lineA.Y2;

            double x3 = lineB.X1, y3 = lineB.Y1;
            double x4 = lineB.X2, y4 = lineB.Y2;

            // equations of the form x = c (two vertical lines)
            if (Math.Abs(x1 - x2) < tolerance && Math.Abs(x3 - x4) < tolerance && Math.Abs(x1 - x3) < tolerance)
            {
                return default(Point);
            }

            //equations of the form y=c (two horizontal lines)
            if (Math.Abs(y1 - y2) < tolerance && Math.Abs(y3 - y4) < tolerance && Math.Abs(y1 - y3) < tolerance)
            {
                return default(Point);
            }

            //equations of the form x=c (two vertical lines)
            if (Math.Abs(x1 - x2) < tolerance && Math.Abs(x3 - x4) < tolerance)
            {
                return default(Point);
            }

            //equations of the form y=c (two horizontal lines)
            if (Math.Abs(y1 - y2) < tolerance && Math.Abs(y3 - y4) < tolerance)
            {
                return default(Point);
            }

            //general equation of line is y = mx + c where m is the slope
            //assume equation of line 1 as y1 = m1x1 + c1
            //=> -m1x1 + y1 = c1 ----(1)
            //assume equation of line 2 as y2 = m2x2 + c2
            //=> -m2x2 + y2 = c2 -----(2)
            //if line 1 and 2 intersect then x1=x2=x & y1=y2=y where (x,y) is the intersection point
            //so we will get below two equations
            //-m1x + y = c1 --------(3)
            //-m2x + y = c2 --------(4)
            double x, y;

            //lineA is vertical x1 = x2
            //slope will be infinity
            //so lets derive another solution
            if (Math.Abs(x1 - x2) < tolerance)
            {
                //compute slope of line 2 (m2) and c2
                double m2 = (y4 - y3) / (x4 - x3);
                double c2 = -m2 * x3 + y3;

                //equation of vertical line is x = c
                //if line 1 and 2 intersect then x1=c1=x
                //subsitute x=x1 in (4) => -m2x1 + y = c2
                // => y = c2 + m2x1
                x = x1;
                y = c2 + m2 * x1;
            }
            //lineB is vertical x3 = x4
            //slope will be infinity
            //so lets derive another solution
            else if (Math.Abs(x3 - x4) < tolerance)
            {
                //compute slope of line 1 (m1) and c2
                double m1 = (y2 - y1) / (x2 - x1);
                double c1 = -m1 * x1 + y1;

                //equation of vertical line is x = c
                //if line 1 and 2 intersect then x3=c3=x
                //subsitute x=x3 in (3) => -m1x3 + y = c1
                // => y = c1 + m1x3
                x = x3;
                y = c1 + m1 * x3;
            }
            //lineA & lineB are not vertical
            //(could be horizontal we can handle it with slope = 0)
            else
            {
                //compute slope of line 1 (m1) and c2
                double m1 = (y2 - y1) / (x2 - x1);
                double c1 = -m1 * x1 + y1;

                //compute slope of line 2 (m2) and c2
                double m2 = (y4 - y3) / (x4 - x3);
                double c2 = -m2 * x3 + y3;

                //solving equations (3) & (4) => x = (c1-c2)/(m2-m1)
                //plugging x value in equation (4) => y = c2 + m2 * x
                x = (c1 - c2) / (m2 - m1);
                y = c2 + m2 * x;

                //verify by plugging intersection point (x, y)
                //in orginal equations (1) & (2) to see if they intersect
                //otherwise x,y values will not be finite and will fail this check
                if (!(Math.Abs(-m1 * x + y - c1) < tolerance
                    && Math.Abs(-m2 * x + y - c2) < tolerance))
                {
                    return default(Point);
                }
            }

            //x,y can intersect outside the line segment since line is infinitely long
            //so finally check if x, y is within both the line segments
            if (IsInsideLine(lineA, x, y) &&
                IsInsideLine(lineB, x, y))
            {
                return new Point { X = x, Y = y };
            }

            //return default null (no intersection)
            return default(Point);
        }

        private static bool IsInsideLine(Line line, double x, double y)
        {
            return (x >= line.X1 && x <= line.X2
                        || x >= line.X2 && x <= line.X1)
                   && (y >= line.Y1 && y <= line.Y2
                        || y >= line.Y2 && y <= line.Y1);
        }

        private void DrawAllLines()
        {
            double yOffset = 800 / y;
            double xOffset = 1000 / x;

            foreach (var le in allLines)
            {
                double firstX = -1;
                double firstY = -1;
                double secondX = -1;
                double secondY = -1;
                string ttp = "";

                foreach (var pe in allPowerEntities)
                {
                    if (pe.Key == le.FirstEnd)
                    {
                        firstX = pe.Value.X;
                        firstY = pe.Value.Y;
                    }
                    if (pe.Key == le.SecondEnd)
                    {
                        secondX = pe.Value.X;
                        secondY = pe.Value.Y;
                    }
                }

                if (firstX < 0 || firstY < 0 || secondX < 0 || secondY < 0)
                    continue;

                ttp = firstX.ToString() + firstY.ToString() + "|" + secondX.ToString() + secondY.ToString();

                Line line1 = new Line();
                line1.StrokeThickness = 1;
                line1.Stroke = new SolidColorBrush(Colors.MediumVioletRed);
                line1.X1 = firstX * xOffset;
                line1.Y1 = firstY * yOffset;
                line1.X2 = secondX * xOffset;
                line1.Y2 = firstY * yOffset;
                line1.Tag = ttp;
                line1.MouseRightButtonDown += line_MouseRightButtonDown;
                canvas.Children.Add(line1);
                lns.Add(line1);

                Line line2 = new Line();
                line2.StrokeThickness = 1;
                line2.Stroke = new SolidColorBrush(Colors.MediumVioletRed);
                line2.X1 = secondX * xOffset;
                line2.Y1 = firstY * yOffset;
                line2.X2 = secondX * xOffset;
                line2.Y2 = secondY * yOffset;
                line2.Tag = ttp;
                line2.MouseRightButtonDown += line_MouseRightButtonDown;
                canvas.Children.Add(line2);
                lns.Add(line2);
            }
        }

        private void line_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mouseWasDownOn = e.Source as FrameworkElement;
            string[] coords = mouseWasDownOn.Tag.ToString().Split('|');

            foreach (var dot in allDots)
            {
                if (coords[0] == dot.Key)
                    dot.Value.Fill = new SolidColorBrush(Colors.LimeGreen);

                if (coords[1] == dot.Key)
                    dot.Value.Fill = new SolidColorBrush(Colors.LimeGreen);
            }
        }

        private void ParseLines()
        {
            bool firstEndExists;
            bool secondEndExists;
            bool allowAddLine;

            foreach (XmlNode node in lineNodeList)
            {
                firstEndExists = false;
                secondEndExists = false;
                allowAddLine = true;

                LineEntity le = new LineEntity();

                le.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                le.Name = node.SelectSingleNode("Name").InnerText;
                le.FirstEnd = long.Parse(node.SelectSingleNode("FirstEnd").InnerText);
                le.SecondEnd = long.Parse(node.SelectSingleNode("SecondEnd").InnerText);

                //provera da li postoje node-ovi za line-ove
                foreach (var dot in allDots)
                {
                    if (!firstEndExists && dot.Value.ToolTip.ToString().Contains(le.FirstEnd.ToString()))
                        firstEndExists = true;

                    if (!secondEndExists && dot.Value.ToolTip.ToString().Contains(le.SecondEnd.ToString()))
                        secondEndExists = true;

                    if (firstEndExists && secondEndExists)
                        break;
                }

                //provera da li postoji vec taj line
                if (firstEndExists && secondEndExists)
                {
                    foreach (var line in allLines)
                    {
                        if (line.FirstEnd == le.FirstEnd && line.SecondEnd == le.SecondEnd)
                            allowAddLine = false;

                        if (line.FirstEnd == le.SecondEnd && line.SecondEnd == le.FirstEnd)
                            allowAddLine = false;
                    }
                }

                if (allowAddLine)
                    allLines.Add(le);
            }
        }

        private void DrawAllNodes()
        {
            foreach (var ss in allNodes)
            {
                double XX = (((ss.X - minCoordX) * (124 - 0)) / (maxCoordX - minCoordX)) + 0;
                double YY = (((ss.Y - minCoordY) * (99 - 0)) / (maxCoordY - minCoordY)) + 0;

                int xCoord = Convert.ToInt32(XX);
                int yCoord = Convert.ToInt32(YY);

                PowerEntity pe = new PowerEntity();
                pe.Id = ss.Id;
                pe.X = xCoord;
                pe.Y = yCoord;
                allPowerEntities.Add(pe.Id, pe);

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

                PowerEntity pe = new PowerEntity();
                pe.Id = ss.Id;
                pe.X = xCoord;
                pe.Y = yCoord;
                allPowerEntities.Add(pe.Id, pe);

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

                PowerEntity pe = new PowerEntity();
                pe.Id = ss.Id;
                pe.X = xCoord;
                pe.Y = yCoord;
                allPowerEntities.Add(pe.Id, pe);

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
            lineNodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");
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