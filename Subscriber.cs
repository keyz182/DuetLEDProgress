using System;
using System.Collections.Generic;
using System.Drawing;
using rpi_ws281x;
using DuetAPI.Connection;
using DuetAPIClient;
using System.Threading;
using System.Threading.Tasks;
using DuetAPI.ObjectModel;
using ColorMine.ColorSpaces;

namespace LEDProgress
{
    class Subscriber{
        private string _socketPath;
        private int _count;
        private byte _brightness;
        private bool _quiet = false;

        private Settings _settings;

        private void Log(string str){
            if(this._quiet) return;
            Console.WriteLine(str);
        }

        public Subscriber(int pin, string socketPath, int count, bool quiet, byte brightness, bool invert){
            this._socketPath = socketPath;
            this._count = count;
            this._quiet = quiet;
            this._brightness = brightness;
            //The default settings uses a frequency of 800000 Hz and the DMA channel 10.
			this._settings = Settings.CreateDefaultSettings();

            Log("Initializing WS281x");
			//Set brightness to maximum (255)
			//Use Unknown as strip type. Then the type will be set in the native assembly.
			this._settings.Channel_1 = new Channel(this._count, pin, this._brightness, invert, StripType.WS2812_STRIP);
        }

        public async Task Subscribe(CancellationToken cancellationToken = default(CancellationToken)){
            // Create a new connection and connect to DuetControlServer
            // https://duet3d.github.io/DuetSoftwareFramework/api/DuetAPIClient.SubscribeConnection.html
            Log("Connecting to Duet");
            using (var controller = new WS281x(this._settings))
            using (SubscribeConnection connection = new SubscribeConnection()){
                List<string> filter = null;
                await connection.Connect(SubscriptionMode.Full, filter, this._socketPath, cancellationToken);

                Log("Fetching Object Model");
                ObjectModel model = await connection.GetObjectModel(cancellationToken);

                do {
                    for(var i = 0; i < this._count; i++){
				        controller.SetLEDColor(0, i, Color.Black);
                    }

                    int duration = model.Job.Duration ?? 0;
                    int left = model.Job.TimesLeft.File ?? 0;

                    float percentage = ((float)duration / (float)(duration + left));

                    var scaled = this._count * percentage;

                    var fullLeds = (int)Math.Floor(scaled);
                    var partial = scaled - fullLeds;

                    for(var i = 0; i < fullLeds; i++){
				        controller.SetLEDColor(0, i, Color.Green);
                    }

                    controller.SetLEDColor(0, fullLeds, GetPartialColor(partial));
                    controller.Render();

                    Log(string.Format("{0}%", percentage));

				    System.Threading.Thread.Sleep(25);

                    //TODO LEDS HERE
                    Log("Updating Model");
                    var json = await connection.GetObjectModelPatch(cancellationToken);
                    model.UpdateFromJson(json.RootElement);
                }while(!cancellationToken.IsCancellationRequested);
            
                for(var i = 0; i < this._count; i++){
                    controller.SetLEDColor(0, i, Color.Black);
                }

                controller.Render();
            }
        }

        public Color GetPartialColor(double partial){
            var hsv = (new Hex("00FF00")).To<Hsv>();
            hsv.S = partial;
            hsv.V = partial;
            return Color.FromArgb((int)hsv.ToRgb().R, (int)hsv.ToRgb().G, (int)hsv.ToRgb().B);
        }
    }
}