# DSF WS281x LED Progress Indicator (WIP)

Extract the release (or copy build artifacts) to `/opt/LEDProgress`.
Copy `ledprogress.service` to `/etc/systemd/service/`. 
Run:
```
sudo systemctl daemon-reload
sudo systemctl enable ledprogress
sudo systemctl start ledprogress
```



```
$ LEDProgress.exe -h
Available command line arguments:
-s, --socket <socket>: UNIX socket to connect to
-p, --pin <pin>: The pin the LEDs are run from
-i, --invert: WS281x Invert
-c, --count <count>: The number of LEDs
-b, --brightness <brightness>: Max brighness (0-255)
-h, --help: Display this help text
```