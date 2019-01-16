using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Gpio;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CognitiveServices.Speech.WebAPI.Controllers
{
    [Route("api/[controller]")]
    public class SpeechController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]GpioInput input)
        {
            var gpioPin = Pi.Gpio.Pin00;

            // Colour
            switch (input.Colour)
            {
                //yellow = GPIO017
                case "yellow":
                    gpioPin = Pi.Gpio.Pin00;
                    break;

                //blue = GPIO018
                case "blue":
                    gpioPin = Pi.Gpio.Pin01;
                    break;
            }

            // Set PinMode
            gpioPin.PinMode = GpioPinDriveMode.Output;

            // Read the current state
            var isOn = gpioPin.Read();

            // Command
            switch (input.Command)
            {
                case "on":
                    if(!isOn)
                    {
                        gpioPin.Write(true);
                    }
                    break;
                case "off":
                    if (isOn)
                    {
                        gpioPin.Write(false);
                    }
                    break;
            }
        }

        public class GpioInput
        {
            public string Colour { get; set; }
            public string Command { get; set; }
        }
    }
}
