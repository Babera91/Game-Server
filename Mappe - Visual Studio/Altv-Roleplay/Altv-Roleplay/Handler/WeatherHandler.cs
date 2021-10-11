using AltV.Net.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using AltV.Net;

namespace Altv_Roleplay.Handler
{
    public class Weather
    {
        public string main { get; set; }
    }

    public class Root
    {
        public List<Weather> weather { get; set; }
    }
    class WeatherHandler
    {
        public static string currentWeatherType = "ExtraSunny";
        public static bool isNotDifferentWeather = false;
        public static void SetRealWeather(IPlayer player)
        {
            if (player == null || !player.Exists) return;
            player.Emit("Client:Weather:SetWeather", currentWeatherType);
        }

        public static void GetRealWeatherType()
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    // PLEASE GENERATE YOUR OWN API KEY AT https://openweathermap.org/ FOR FREE
                    string apiKey = "97c8c714887da0fc26e66c28ac0a040e";

                    var urljson = wc.DownloadString("http://api.openweathermap.org/data/2.5/weather?q=berlin&appid=" + apiKey);

                    switch (JsonConvert.DeserializeObject<Root>(urljson).weather[0].main)
                    {
                        case "Drizzle":
                            if (currentWeatherType == "Clearing") isNotDifferentWeather = true;
                            else isNotDifferentWeather = false;
                            currentWeatherType = "Clearing";
                            break;
                        case "Clear":
                            if (currentWeatherType == "Clear") isNotDifferentWeather = true;
                            else isNotDifferentWeather = false;
                            currentWeatherType = "Clear";
                            break;
                        case "Clouds":
                            if (currentWeatherType == "Clouds") isNotDifferentWeather = true;
                            else isNotDifferentWeather = false;
                            currentWeatherType = "Clouds";
                            break;
                        case "Rain":
                            if (currentWeatherType == "Rain") isNotDifferentWeather = true;
                            else isNotDifferentWeather = false;
                            currentWeatherType = "Rain";
                            break;
                        case "Thunderstorm":
                            if (currentWeatherType == "Thunder") isNotDifferentWeather = true;
                            else isNotDifferentWeather = false;
                            currentWeatherType = "Thunder";
                            break;
                        case "Thunder":
                            if (currentWeatherType == "Thunder") isNotDifferentWeather = true;
                            else isNotDifferentWeather = false;
                            currentWeatherType = "Thunder";
                            break;
                        case "Foggy":
                            if (currentWeatherType == "Foggy") isNotDifferentWeather = true;
                            else isNotDifferentWeather = false;
                            currentWeatherType = "Foggy";
                            break;
                        case "Fog":
                            if (currentWeatherType == "Foggy") isNotDifferentWeather = true;
                            else isNotDifferentWeather = false;
                            currentWeatherType = "Foggy";
                            break;
                        case "Mist":
                            if (currentWeatherType == "Foggy") isNotDifferentWeather = true;
                            else isNotDifferentWeather = false;
                            currentWeatherType = "Foggy";
                            break;
                        case "Smoke":
                            if (currentWeatherType == "Foggy") isNotDifferentWeather = true;
                            else isNotDifferentWeather = false;
                            currentWeatherType = "Foggy";
                            break;
                        case "Smog":
                            if (currentWeatherType == "Smog") isNotDifferentWeather = true;
                            else isNotDifferentWeather = false;
                            currentWeatherType = "Smog";
                            break;
                        case "Overcast":
                            if (currentWeatherType == "Overcast") isNotDifferentWeather = true;
                            else isNotDifferentWeather = false;
                            currentWeatherType = "Overcast";
                            break;
                        case "Snowing":
                            if (currentWeatherType == "Snow") isNotDifferentWeather = true;
                            else isNotDifferentWeather = false;
                            currentWeatherType = "Snow";
                            break;
                        case "Snow":
                            if (currentWeatherType == "Snow") isNotDifferentWeather = true;
                            else isNotDifferentWeather = false;
                            currentWeatherType = "Snow";
                            break;
                        case "Blizzard":
                            if (currentWeatherType == "Smog") isNotDifferentWeather = true;
                            else isNotDifferentWeather = false;
                            currentWeatherType = "Smog";
                            break;
                        default:
                            Console.WriteLine("Missing Weather: " + JsonConvert.DeserializeObject<Root>(urljson).weather[0].main);
                            if (currentWeatherType == "Clear") isNotDifferentWeather = true;
                            else isNotDifferentWeather = false;
                            currentWeatherType = "Clear";
                            break;
                    }
                }
            } catch (Exception err)
            {
                Alt.Log($"{err}");
            }
        }
    }
}
