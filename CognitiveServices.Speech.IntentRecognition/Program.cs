using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Intent;
using Newtonsoft.Json;

namespace CognitiveServices.Speech.IntentRecognition
{
    class Program
    {
        private static readonly string YourAppKey = "<YOUR-APP-KEY>";
        private static readonly string YourRegion = "<YOUR-REGION>";
        private static readonly string YourAppId = "<YOUR-APP-ID>";

        public static async Task RecognizeSpeechAsync()
        {
            // Create an instance of a speech config with the specified app key and region.
            var config = SpeechConfig.FromSubscription(YourAppKey, YourRegion);

            // Use the microphone as input.
            var audioConfig = AudioConfig.FromDefaultMicrophoneInput();

            var stopRecognition = new TaskCompletionSource<int>();

            // Create a new IntentRecognizer, which uses the config and the audioConfig.
            using (var recognizer = new IntentRecognizer(config, audioConfig))
            {
                // Create a Language Understanding model using the app id.
                var model = LanguageUnderstandingModel.FromAppId(YourAppId);

                // Add the intents which are specified by the LUIS app.
                recognizer.AddIntent(model, "HomeAutomation.TurnOff", "off");
                recognizer.AddIntent(model, "HomeAutomation.TurnOn", "on");

                // Subscribe to events.
                recognizer.Recognizing += (s, e) =>
                {
                    Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                };

                recognizer.Recognized += (s, e) =>
                {
                    // The LUIS app recognized an intent.
                    if (e.Result.Reason == ResultReason.RecognizedIntent)
                    {
                        // Get the result from the event arguments and print it into the console.
                        var responseJson = e.Result.Properties.GetProperty(PropertyId.LanguageUnderstandingServiceResponse_JsonResult);

                        Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                        Console.WriteLine($"    Intent Id: {e.Result.IntentId}.");
                        Console.WriteLine($"    Language Understanding JSON: {responseJson}.");

                        // Deserialize the JSON result into an object.
                        var responseObject = JsonConvert.DeserializeObject<SpeechResponse>(responseJson);

                        // Get the intent out of the result. This gives us the command.
                        var intent = responseObject.topScoringIntent.intent;
                        if (intent == "HomeAutomation.TurnOn")
                        {
                            intent = "on";
                        }
                        else if (intent == "HomeAutomation.TurnOff")
                        {
                            intent = "off";
                        }

                        // Get the colour entity out of the result.
                        var colourEntity = responseObject.entities.FirstOrDefault(x => x.type == "Colour");
                        var colour = colourEntity.entity;

                        // Create the request we will send to the web API.
                        var request = new SpeechRequest
                        {
                            Colour = colour,
                            Command = intent
                        };

                        // Create a new HttpClient and send the request.
                        var client = new HttpClient();
                        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                        client.PostAsync("http://<your-local-raspberrypi-ip>/api/Speech", content);
                    }

                    // The speech service recognized speech.
                    else if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                        Console.WriteLine($"    Intent not recognized.");
                    }

                    // The input has not been recognized.
                    else if (e.Result.Reason == ResultReason.NoMatch)
                    {
                        Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                    }
                };

                recognizer.Canceled += (s, e) =>
                {
                    Console.WriteLine($"CANCELED: Reason={e.Reason}");

                    if (e.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }

                    stopRecognition.TrySetResult(0);
                };

                recognizer.SessionStarted += (s, e) =>
                {
                    Console.WriteLine("\n    Session started event.");
                };

                recognizer.SessionStopped += (s, e) =>
                {
                    Console.WriteLine("\n    Session stopped event.");
                    Console.WriteLine("\nStop recognition.");
                    stopRecognition.TrySetResult(0);
                };

                // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                // Waits for completion.
                // Use Task.WaitAny to keep the task rooted.
                Task.WaitAny(new[] { stopRecognition.Task });

                // Stops recognition.
                await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            }
        }

        static void Main()
        {
            RecognizeSpeechAsync().Wait();
            Console.WriteLine("Please press a key to continue.");
            Console.ReadLine();
        }
    }
}
