using MathNet.Numerics.Interpolation;

internal class Program
{
    private static void Main(string[] args)
    {
        var points = new List<Tuple<double, double>>()
        {
            Tuple.Create(0.0, 0.0),
            Tuple.Create(150.0, 200.0),
            Tuple.Create(255.0, 10.0)
        };

        // X와 Y 배열 생성
        double[] xValues = { 3, 150, 253 };
        double[] yValues = { 5, 200, 11 };

        // Cubic Spline 보간 생성
        var spline = CubicSpline.InterpolateNatural(xValues, yValues);

        // 결과를 저장할 리스트
        var integerPoints = new List<Tuple<int, int>>();

        // X가 0부터 255까지 정수일 때의 Y 값을 계산하고 정수 좌표를 저장
        for (int x = 0; x <= 255; x++)
        {
            double y = spline.Interpolate(x); // 스플라인 보간으로 Y 값 계산

            // X와 Y가 모두 정수인 경우에만 리스트에 추가
            //if (Math.Round(y) == y) // y가 정수인지 확인
            //{
                integerPoints.Add(Tuple.Create(x, (int)y));
            //}

            Console.WriteLine($"{x}:{y}");
        }

        //결과 출력
        Console.WriteLine("정수 좌표:");
        foreach (var point in integerPoints)
        {
            Console.WriteLine($"({point.Item1}, {point.Item2})");
        }
    }
}