using panpan;

internal class Program
{
    private static void Main(string[] args)
    {
        var app = new App("panpan", new panpanExample.TestScene(),320*4,180*4);
        app.Run();
    }
}