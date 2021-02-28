using System;
using System.Threading.Tasks;
using CommandLine;
using DuetAPI.Connection;

namespace LEDProgress
{
    class Program
    {
        public class Options
        {
            [Option('q', "quiet", Required = false, HelpText = "Suppress output", Default = false )]
            public bool Quiet { get; set; }
            
            [Option('p', "pin", Required = false, HelpText = "Set the pin that LEDs are on.", Default = 12)]
            public int Pin {get; set;}
            
            [Option('s', "socket", Required = false, HelpText = "UNIX socket to connect to.", Default = Defaults.FullSocketPath)]
            public string Socket {get; set;}
            
            [Option('i', "invert", Required = false, HelpText = "WS281X Invert.", Default = false)]
            public bool Invert {get; set;}
            
            [Option('c', "count", Required = false, HelpText = "Number of LEDs.", Default = 56)]
            public int Count {get; set;}
            
            [Option('b', "brightness", Required = false, HelpText = "Max brightness.", Default = 255)]
            public int Brightness {get; set;}
            
            [Option("status", Required = false, HelpText = "Enable status LEDs", Default = false)]
            public bool Status {get; set;}
            
            [Option("fan0", Required = false, HelpText = "Fan 0 status", Default = false)]
            public bool Fan0 {get; set;}
            
            [Option("fan1", Required = false, HelpText = "Fan 1 status", Default = false)]
            public bool Fan1 {get; set;}
            
            [Option("fan2", Required = false, HelpText = "Fan 2 status", Default = false)]
            public bool Fan2 {get; set;}
            
            [Option("heater0", Required = false, HelpText = "Heater 0 status", Default = false)]
            public bool Heater0 {get; set;}
            
            [Option("heater1", Required = false, HelpText = "Heater 1 status", Default = false)]
            public bool Heater1 {get; set;}
        }
		public static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<Options>(args)
                   .WithParsedAsync<Options>(async o =>
                   {
                        Subscriber sub = new Subscriber(
                            o.Pin, 
                            o.Socket, 
                            o.Count, 
                            o.Quiet, 
                            o.Brightness, 
                            o.Invert,
                            o.Status,
                            o.Fan0,
                            o.Fan1,
                            o.Fan2,
                            o.Heater0,
                            o.Heater1);

                        await sub.Subscribe();
                   });
        }
    }
}
