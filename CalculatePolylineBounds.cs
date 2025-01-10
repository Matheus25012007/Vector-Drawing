using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Projeto_Adriana___Desenho_Vetorial
{
    internal class CalculatePolylineBounds
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Type { get; set; }
        public Color StrokeColor { get; set; }
        public double StrokeThickness { get; set; }
        public List<Point> Points { get; set; }
    }
}