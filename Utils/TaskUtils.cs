namespace Shared;

public static class TaskUtils
{
    public static async Task Delay(int ms)
    {
        await Task.Delay(ms);
    }

    public static async Task Delay(int min, int max)
    {
        var rnd = new Random();
        var time = rnd.Next(1000, 1300);
        await Task.Delay(time);
        return;
    }
}
