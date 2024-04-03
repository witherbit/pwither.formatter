using pwither.formatter;

namespace testApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            var test = new Test
            {
                TestStr = "test",
                DateTime = DateTime.Now,
                InnerTest = new InnerTest
                {
                    Data = new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }
                },
                TestInt = 1,
            };
            test.I = 151;
            var data = BitSerializer.Serialize(test);
            Console.WriteLine(data);
            var test2 = BitSerializer.Deserialize<Test>(data);
            Console.WriteLine($"{test2.TestInt} {test2.DateTime.ToString("G")} {test2.TestStr} {test2.InnerTest.Data.Length} {test2.InnerTest.Data[2]} {test2.I}");
            Console.ReadLine();
        }
    }
}
