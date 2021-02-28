using System;
using System.Threading.Tasks;
using DuetAPI.Connection;

namespace LEDProgress
{
    class Program
    {
		public static async Task Main(string[] args)
        {
            // Parse the command line arguments
            int pin = 12;
            int count = 56;
            byte brightness = 255;
            string lastArg = null, socketPath = Defaults.FullSocketPath;
            bool quiet = false;
            bool invert = false;
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
                else if (arg == "-i" || arg == "--invert")
                {
                    invert = true;
                }
                else if (arg == "-b" || arg == "--brightness")
                {
                    brightness = byte.Parse(arg);
                }
                else if (arg == "-h" || arg == "--help")
                {
                    Console.WriteLine("Available command line arguments:");
                    Console.WriteLine("-s, --socket <socket>: UNIX socket to connect to");
                    Console.WriteLine("-p, --pin <pin>: The pin the LEDs are run from");
                    Console.WriteLine("-i, --invert: WS281x Invert");
                    Console.WriteLine("-c, --count <count>: The number of LEDs");
                    Console.WriteLine("-b, --brightness <brightness>: Max brighness (0-255)");
                    Console.WriteLine("-h, --help: Display this help text");
                    return;
                }
                lastArg = arg;
            }

            Subscriber sub = new Subscriber(pin, socketPath, count, quiet, brightness, invert);

            await sub.Subscribe();
        }
    }
}
