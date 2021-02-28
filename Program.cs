using System;
using System.Drawing;
using System.Threading.Tasks;
using rpi_ws281x;
using DuetAPI.Connection;
using DuetAPIClient;

namespace LEDProgress
{
    class Program
    {
		public static async Task Main(string[] args)
        {
            // Parse the command line arguments
            int pin = 12;
            int count = 56;
            string lastArg = null, socketPath = Defaults.FullSocketPath;
            bool quiet = false;
            foreach (string arg in args)
            {
                if (lastArg == "-s" || lastArg == "--socket")
                {
                    socketPath = arg;
                }
                else if (lastArg == "-p" || lastArg == "--pin")
                {
                    pin = int.Parse(arg);
                }
                else if (lastArg == "-c" || lastArg == "--count")
                {
                    count = int.Parse(arg);
                }
                else if (arg == "-q" || arg == "--quiet")
                {
                    quiet = true;
                }
                else if (arg == "-h" || arg == "--help")
                {
                    Console.WriteLine("Available command line arguments:");
                    Console.WriteLine("-s, --socket <socket>: UNIX socket to connect to");
                    Console.WriteLine("-p, --pin <pin>: The pin the LEDs are run from");
                    Console.WriteLine("-c, --count <count>: The number of LEDs");
                    Console.WriteLine("-h, --help: Display this help text");
                    return;
                }
                lastArg = arg;
            }

            Subscriber sub = new Subscriber(pin, socketPath, count);

            await sub.Subscribe();
        }
    }
}
