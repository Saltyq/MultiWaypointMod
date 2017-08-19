using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using GTA.Math;

namespace MultiWaypointMod
{
    public class Waypoints : Script
    {
        string version = "1.0";

        List<Blip> _waypointPool = new List<Blip>();

        int waypointCount = 0;

        int colorIndex;

        BlipColor[] colors = new BlipColor[]
        {
            BlipColor.Blue,
            BlipColor.Green,
            BlipColor.Red,
            BlipColor.White,
            BlipColor.Yellow,
        };

        Ped playerPed;
        Player player;

        bool debug = false;

        bool firstTime = true;
        bool modActive = false;

        string removeAll = "J";
        string removeNearest = "K";
        string toggleMod = "P";

        public Waypoints()
        {
            Tick += onTick;
            KeyDown += onKeyDown;
            Interval = 1;

            LoadSettings();
        }

        void LoadSettings()
        {
            try
            {
                ScriptSettings config = ScriptSettings.Load(@".\scripts\MultiWaypoint.ini");
                removeAll = config.GetValue("Keys", "RemoveAllWaypoints", "J");
                removeNearest = config.GetValue("Keys", "RemoveNearestWaypoint", "K");
                toggleMod = config.GetValue("Keys", "Toggle", "P");
            } catch (Exception ex)
            {
                UI.Notify("Failed to load INI, check MW.log");
                File.WriteAllLines(@".\scripts\MW.log", new string[] { "ERROR: ", ex.ToString(), });
            }
        }

        private void onTick(object sender, EventArgs e)
        {

            if (firstTime)
            {
                UI.Notify("~b~MultiWaypointMod ~w~Loaded.\nUse " + toggleMod + " to enable.\nCurrent Version: " + version);
                firstTime = false;
            }
            if (debug)
            {
                UI.ShowSubtitle(waypointCount.ToString());
            }
            if (modActive)
            {
                player = Game.Player;
                playerPed = player.Character;
                if (Function.Call<bool>(Hash.IS_WAYPOINT_ACTIVE))
                {
                    //colorIndex = new Random().Next(5);
                    waypointCount++;
                    Blip newWaypoint = World.CreateBlip(WaypointCoords());
                    newWaypoint.Sprite = BlipSprite.Waypoint;
                    newWaypoint.Name = "Waypoint";
                    //newWaypoint.ShowRoute = true;
                    Function.Call(Hash.SET_WAYPOINT_OFF);
                    _waypointPool.Add(newWaypoint);
                    if (debug)
                    {
                        UI.Notify("waypoint added");
                    }
                }
                if (waypointCount > 0)
                {

                    for (int i = 0; i < _waypointPool.Count; i++)
                    {
                        if (debug)
                        {
                            UI.Notify(_waypointPool.Count.ToString());
                        }
                        if (World.GetDistance(_waypointPool[i].Position, playerPed.Position) <= 50)
                        {
                            _waypointPool[i].Remove();
                            _waypointPool.Remove(_waypointPool[i]);
                            waypointCount--;
                        }
                    }
                }
                ShowText(0.013f, 0.77f, 0.4f, Convert.ToInt32(GTA.Font.Pricedown), Color.White.R, Color.White.G, Color.White.B, "# of waypoints: " + _waypointPool.Count.ToString());
            }
            else
            {
                ShowText(0.013f, 0.77f, 0.25f, Convert.ToInt32(GTA.Font.Pricedown), Color.White.R, Color.White.G, Color.White.B, "Waypoint Mod Disabled, press " + toggleMod + " to enable.");
            }
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {

            Keys removeAllPoints = (Keys)Enum.Parse(typeof(Keys), removeAll);
            Keys removeNearestPoint = (Keys)Enum.Parse(typeof(Keys), removeNearest);
            Keys toggleModKey = (Keys)Enum.Parse(typeof(Keys), toggleMod);

            if (e.KeyCode == removeAllPoints)
            {
                for (int i = 0; i <= _waypointPool.Count; i++)
                {
                    _waypointPool[i].Remove();
                    waypointCount--;
                    if (waypointCount == 0)
                    {
                        _waypointPool.Clear();
                    }
                }
            }
            if (e.KeyCode == removeNearestPoint)
            {
                for (int i = 0; i <= _waypointPool.Count; i++)
                {
                    if (World.GetDistance(_waypointPool[i].Position, playerPed.Position) <= 200)
                    {
                        _waypointPool[i].Remove();
                        _waypointPool.Remove(_waypointPool[i]);
                        waypointCount--;
                    }
                }
            }
            if (e.KeyCode == toggleModKey)
            {
                modActive = !modActive;
            }
        }

        private static Vector3 WaypointCoords()
        {
            Vector3 wpVec = new Vector3();
            Blip wpBlip = new Blip(Function.Call<int>(Hash.GET_FIRST_BLIP_INFO_ID, 8));

            if (Function.Call<bool>(Hash.IS_WAYPOINT_ACTIVE))
            {
                wpVec = Function.Call<GTA.Math.Vector3>(Hash.GET_BLIP_COORDS, wpBlip);

            }
            else
            {
                UI.ShowSubtitle("Waypoint not set!");
            }
            return wpVec;
        }

        void ShowText(float x, float y, float scale, int font, int r, int g, int b, string text)
        {
            Function.Call(Hash.SET_TEXT_FONT, font);
            Function.Call(Hash.SET_TEXT_SCALE, scale, scale);
            Function.Call(Hash.SET_TEXT_COLOUR, r, g, b, 255);
            Function.Call(Hash.SET_TEXT_WRAP, 0.0, 1.0);
            Function.Call(Hash.SET_TEXT_CENTRE, false);
            Function.Call(Hash.SET_TEXT_DROPSHADOW, 2, 2, 0, 0, 0);
            Function.Call(Hash.SET_TEXT_EDGE, 1, 1, 1, 1, 205);
            Function.Call(Hash._SET_TEXT_ENTRY, "STRING");
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, text);
            Function.Call(Hash._DRAW_TEXT, x, y);
        }

    }
}
