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
        private int _brightness;
        private bool _quiet = false;
        private bool _status = false;
        private bool _fan0 = false;
        private bool _fan1 = false;
        private bool _fan2 = false;
        private bool _heater0 = false;
        private bool _heater1 = false;
        private int Count {
            get{
                var count = _count;
                if(_status){
                    if(_fan0) count--;
                    if(_fan1) count--;
                    if(_fan2) count--;
                    if(_heater0) count--;
                    if(_heater1) count--;
                }

                return count;
            }
        }

        private Settings _settings;

        private void Log(string str){
            if(this._quiet) return;
            Console.WriteLine(str);
        }

        public Subscriber(int pin, 
                            string socketPath, 
                            int count, 
                            bool quiet, 
                            int brightness, 
                            bool invert, 
                            bool status, 
                            bool fan0, 
                            bool fan1, 
                            bool fan2, 
                            bool heater0, 
                            bool heater1){
            this._socketPath = socketPath;
            this._count = count;
            this._quiet = quiet;
            this._brightness = brightness;
            //The default settings uses a frequency of 800000 Hz and the DMA channel 10.
			this._settings = Settings.CreateDefaultSettings();

            this._status = status;
            this._fan0 = fan0;
            this._fan1 = fan1;
            this._fan2 = fan2;
            this._heater0 = heater0;
            this._heater1 = heater1;

            Log("Initializing WS281x");
			//Set brightness to maximum (255)
			//Use Unknown as strip type. Then the type will be set in the native assembly.
			this._settings.Channel_1 = new Channel(this._count, pin, (byte)this._brightness, invert, StripType.WS2812_STRIP);
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
                    var purple = new Hsv();
                    purple.H = 300;
                    purple.S = 1;
                    purple.V = 0.33;
                    for(var i = 0; i < this._count; i++){
				        controller.SetLEDColor(0, i, GetColorFromHSV(purple));
                    }

                    int duration = model.Job.Duration ?? 0;
                    int left = model.Job.TimesLeft.File ?? 0;

                    float percentage = ((float)duration / (float)(duration + left));

                    var scaled = this.Count * percentage;

                    var fullLeds = (int)Math.Floor(scaled);
                    var partial = scaled - fullLeds;

                    for(var i = 0; i < fullLeds; i++){
				        controller.SetLEDColor(0, i, Color.Green);
                    }

                    controller.SetLEDColor(0, fullLeds, GetPartialColor(new Hex("00FF00"), partial));

                    if(this._status){
                        Log("Showing Status");
                        var pos = this.Count;
                        if(this._fan0){
                            Log(string.Format("Fan0: {0}", model.Fans[0].ActualValue));
                            controller.SetLEDColor(0, pos++, GetPartialColor(new Hex("0000FF"), model.Fans[0].ActualValue));
                        }
                        if(this._fan1){
                            Log(string.Format("Fan0: {0}", model.Fans[1].ActualValue));
                            controller.SetLEDColor(0, pos++, GetPartialColor(new Hex("0000FF"), model.Fans[1].ActualValue));
                        }
                        if(this._fan2){
                            Log(string.Format("Fan0: {0}", model.Fans[2].ActualValue));
                            controller.SetLEDColor(0, pos++, GetPartialColor(new Hex("0000FF"), model.Fans[2].ActualValue));
                        }
                        if(this._heater0){
                            Log(string.Format("Heater0: {0}", model.Heat.Heaters[0].State.ToString()));
                            switch(model.Heat.Heaters[0].State){
                                case HeaterState.Active:
                                    controller.SetLEDColor(0, pos++, Color.Green);
                                    break;
                                case HeaterState.Fault:
                                    controller.SetLEDColor(0, pos++, Color.Red);
                                    break;
                                case HeaterState.Off:
                                    controller.SetLEDColor(0, pos++, Color.Black);
                                    break;
                                case HeaterState.Offline:
                                    controller.SetLEDColor(0, pos++, Color.Black);
                                    break;
                                case HeaterState.Standby:
                                    controller.SetLEDColor(0, pos++, Color.LightGreen);
                                    break;
                                case HeaterState.Tuning:
                                    controller.SetLEDColor(0, pos++, Color.Blue);
                                    break;
                                default:
                                    controller.SetLEDColor(0, pos++, Color.Black);
                                    break;
                            }
                        }
                        if(this._heater1){
                            Log(string.Format("Heater1: {0}", model.Heat.Heaters[1].State.ToString()));
                            switch(model.Heat.Heaters[1].State){
                                case HeaterState.Active:
                                    controller.SetLEDColor(0, pos++, Color.Green);
                                    break;
                                case HeaterState.Fault:
                                    controller.SetLEDColor(0, pos++, Color.Red);
                                    break;
                                case HeaterState.Off:
                                    controller.SetLEDColor(0, pos++, Color.Black);
                                    break;
                                case HeaterState.Offline:
                                    controller.SetLEDColor(0, pos++, Color.Black);
                                    break;
                                case HeaterState.Standby:
                                    controller.SetLEDColor(0, pos++, Color.LightGreen);
                                    break;
                                case HeaterState.Tuning:
                                    controller.SetLEDColor(0, pos++, Color.Blue);
                                    break;
                                default:
                                    controller.SetLEDColor(0, pos++, Color.Black);
                                    break;
                            }
                        }
                    }
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

        public Color GetColorFromHSV(Hsv colour){
            return Color.FromArgb((int)colour.ToRgb().R, (int)colour.ToRgb().G, (int)colour.ToRgb().B);
        }

        public Color GetPartialColor(Hex colour, double partial){
            var hsv = colour.To<Hsv>();
            hsv.S = partial;
            hsv.V = partial;
            return Color.FromArgb((int)hsv.ToRgb().R, (int)hsv.ToRgb().G, (int)hsv.ToRgb().B);
        }
    }
}