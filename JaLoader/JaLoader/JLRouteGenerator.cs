using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoader
{
    public class JLRouteGenerator : RouteGeneratorC
    {
        public RouteGeneratorC baseRouteGenerator { get; private set; }

        static internal List<Country> countries = new List<Country>();
        internal GameObject _camera;

        void Awake()
        {
            Global = this;
        }

        void CreateOriginalCountries()
        {
            // Germany (to Dresden)
            var germany = new Country("DE");
            germany.AddRoadSegment(0, "Start", baseRouteGenerator.segmentLibraryGerStart);
            germany.AddRoadSegment(1, "Generic", (GameObject[])baseRouteGenerator.segmentLibraryGer2.Concat(baseRouteGenerator.segmentLibraryGer3));
            germany.AddRoadSegment(1, "Stops", (GameObject[])baseRouteGenerator.stopOffLibraryGer2.Concat(baseRouteGenerator.stopOffLibraryGer3));
            germany.AddRoadSegment(1, "Roundabouts", new GameObject[]
            {
                baseRouteGenerator.roundaboutGer2_1,
                baseRouteGenerator.roundaboutGer2_2,
                baseRouteGenerator.roundaboutGer2_3,
                baseRouteGenerator.roundaboutGer3_1
            });
            germany.AddRoadSegment(0, "DestinationObjects", baseRouteGenerator.destinationObjectsGer);
            countries.Add(germany);
        }

        public static Country GetCountryFromCode(string countryCode)
        {
            return countries.Where(c => c.GenericCountryCode == countryCode).FirstOrDefault();
        }

        void Start()
        {
            _camera = Camera.main.gameObject;
            RouteDistances();
            PickDestination();
            carLogic = GameObject.FindWithTag("CarLogic");
            seatLogic = carLogic.GetComponent<CarLogicC>().playerSeat;
            if (ES3.Exists("economyState") && routeLevel != 0)
            {
                economyState = ES3.LoadInt("economyState");
            }
            else
            {
                GenerateMarketEconomies();
            }
        }

        public new void SpawnRoute1()
        {
            routeChosen = 1;
            SetRoadConditions();
            SetWeather();

            GameObject gameObject = Instantiate(route1RoundaboutSegment);
            gameObject.transform.position = firstSegmentTarget.transform.position;
            spawnedRouteSegments.Add(startingEnvironment);
            spawnedRouteSegments.Add(gameObject);
            GameObject gameObject2 = Instantiate(route1Segments[0]);
            gameObject2.transform.position = gameObject.GetComponent<RoundaboutC>().continueNode.transform.position;
            spawnedRouteSegments.Add(gameObject2);
            routeAmmo++;
        }
    }
}
