using System.Collections.ObjectModel;
using System.Windows;
using Telerik.Windows.Data;

namespace WpfCurveGraph02
{
    public class ClosestPointFinder
    {
        public ObservableCollection<Point> PointCollections { get; set; }

        public ClosestPointFinder()
        {
        }

        public Point FindClosestPoint(Point target)
        {
            double minDistance = double.MaxValue;
            Point closestPoint = new Point(-1, -1);

            foreach (var point in PointCollections)
            {
                double distance = GetDistance(target, point);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = point;
                }
            }

            return closestPoint;
        }

        // 두 점 사이의 유클리드 거리 계산 메서드
        private double GetDistance(Point p1, Point p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
