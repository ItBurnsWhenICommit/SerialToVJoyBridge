using System.IO.Ports;
using CoreDX.vJoy.Wrapper;

namespace SerialToVJoyBridge
{
    class Program
    {
        static string version = "0.0.1";
        static SerialPort _serialPort;
        static VJoyControllerManager _vJoy;
        static IVJoyController _virtualJoy;
        static int prevValue = 33000;
        static int chunk = 32000 / 1024;

        public static void Main(string[] args)
        {
            Console.WriteLine("Serial (Atmel) ebrake <---> Vjoy bridge started");
            Console.WriteLine("Ver. "+version);

            _vJoy = VJoyControllerManager.GetManager();
            _virtualJoy = _vJoy.AcquireController(SetVjoyIndex());
            Console.WriteLine("Virtual joystick data : ");
            Console.WriteLine("Axis max value: "+ _virtualJoy.AxisMaxValue);
            Console.WriteLine("Has X axis: "+ _virtualJoy.HasAxisX);

            if (!_virtualJoy.HasAxisX)
            {
                Console.WriteLine("*****************************");
                Console.WriteLine("WARNING !!! Vjoy controller does not have an X axis, make sure you assign it in the vjoy settings!");
                Environment.Exit(0);
            }

            _serialPort = new SerialPort();
            _serialPort.PortName = SetPortName();
            _serialPort.BaudRate = 115200;

            try
            {
                _serialPort.Open();
            }
            catch(Exception e) { 
                Console.WriteLine("Error while opening com port : " + e.Message);
                Environment.Exit(1);
            }

            Console.WriteLine("-----------------------------");
            Console.WriteLine("Program came this far, all seems fine. Piping ebrake data to vjoy...");

            while (true)
            {
                try
                {
                    int value = 0;
                    string message = _serialPort.ReadLine();
                    try
                    {
                        value = int.Parse(message);
                    }
                    catch (Exception e) { continue; }

                    if (value != prevValue) {
                        _virtualJoy.SetAxisX(chunk * value);
                        prevValue = value;
                    }

                    //Console.WriteLine(message);
                }
                catch (TimeoutException) { }
                catch (Exception e)
                {
                    Console.WriteLine("Seems like serial port closed, closing this app. (" + e.Message + ")");
                    Environment.Exit(66);
                }
            }
        }

        static string SetPortName()
        {
            string portName = String.Empty;


            while (portName == "" || !(portName.ToLower()).StartsWith("com"))
            {
                Console.WriteLine("-----------------------------");
                Console.WriteLine("Available Ports:");
                int count = 0;
                foreach (string s in SerialPort.GetPortNames())
                {
                    Console.WriteLine("   {0}", s);
                    count++;
                }

                if (count == 0)
                {
                    Console.WriteLine("*****************************");
                    Console.WriteLine("No COM ports detected , make sure you plug in the ebrake");
                    Environment.Exit(1);
                }

                if (count == 1)
                {
                    Console.WriteLine("Only one port available, guessing this is the ebrake? Im taking it.");
                    return SerialPort.GetPortNames()[0];
                }
               
                Console.Write("Enter COM port of your ebrake: ");
                portName = Console.ReadLine();

            }

            return portName;
        }

        static uint SetVjoyIndex()
        {
            uint vjoyIndex = 9999;

            while(vjoyIndex == 9999)
            {
                Console.WriteLine("-----------------------------");
                Console.Write("Enter vJoy controller number we fake as:");
                vjoyIndex = uint.Parse(Console.ReadLine());
            }

            return vjoyIndex;
        }
    }
}


