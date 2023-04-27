namespace App
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server...");
            RoomServer roomServer = new RoomServer(8081);
            ActivityServer activityServer = new ActivityServer(8082);

            roomServer.Start();
            activityServer.Start();

        }
    }
}