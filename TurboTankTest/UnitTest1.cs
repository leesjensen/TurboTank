﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TurboTank;
using Agilix.Shared;

namespace TurboTankTest
{
    [TestClass]
    public class UnitTest1
    {

        DynObject joinResponse = DynObject.Parse(@"{
                'status':'running',
                'config':
                {
                    'max_energy':10,
                    'laser_distance':32,
                    'health_loss':1,
                    'battery_power':5,
                    'battery_health':20,
                    'laser_energy':1,
                    'connect_back_timeout':10000000000,
                    'max_health':300,
                    'laser_damage':50,
                    'turn_timeout':500000000
                },
                'energy':5,
                'orientation':'south',
                'health':300,
                'grid':null
        }");

        [TestMethod]
        public void Fire()
        {
            joinResponse["grid"] = 
@"________________________
___W_____WWWWWWWW_______
___W_W__________________
___W_W_______B____X_____
___W_W__________________
___W_W__________________
_WWWWWWWWW________O_____
_____W__________________
_____W_WWWWW____________
_________WWWWWWWW_______
________________________
___________WWWW_________
________________________
________________________
____WWW_________________
________________________";

            TestTankClient client = new TestTankClient(joinResponse, TankAction.Fire);
            Game game = new Game(client);
            game.Run(new SignalWeights());
        }


        [TestMethod]
        public void TurnForBattery()
        {
            joinResponse["grid"] = 
@"________________________
___W_____WWWWWWWW_______
___W_W__________________
___W_W_______B____X_____
___W_W__________________
___W_W__________________
_WWWWWWWWW______________
_____W__________________
_____W_WWWWW____________
_________WWWWWWWW_______
________________________
_____O_____WWWW_________
________________________
________________________
____WWW_________________
________________________";

            TestTankClient client = new TestTankClient(joinResponse, TankAction.Right);
            Game game = new Game(client);
            game.Run(new SignalWeights());
        }


        [TestMethod]
        public void WallAhead()
        {
            joinResponse["grid"] =
@"________________________
___W_____WWWWWWWW_______
___W_W__________________
___W_W_________________
___W_W__________________
___W_W__X_______________
_WWWWWWWWW______________
_____W__________________
_____W_WWWWW____________
_________WWWWWWWW_B_____
________________________
____O______WWWW_________
________________________
________________________
____WWW_________________
________________________";

            TestTankClient client = new TestTankClient(joinResponse, TankAction.Right);
            Game game = new Game(client);
            game.Run(new SignalWeights());
        }

        [TestMethod]
        public void ForwardForBatteryFar()
        {
            joinResponse["grid"] =
@"________________________
___W_____WWWWWWWW_______
___W_W__________________
___W_W____________X_____
___W_W__________________
___W_W__________________
_WWWWWWWWW______________
_____W__________________
_____W_WWWWW____________
_________WWWWWWWW_B_____
________________________
____O______WWWW_________
________________________
________________________
____WWW_________________
________________________";

            TestTankClient client = new TestTankClient(joinResponse, TankAction.Move);
            Game game = new Game(client);
            game.Run(new SignalWeights());
        }

        [TestMethod]
        public void ForwardForBatteryNear()
        {
            joinResponse["grid"] =
@"________________________
___W_____WWWWWWWW_______
___W_W__________________
___W_W____________X_____
___W_W____________B_____
___W_W__________________
_WWWWWWWWW______________
_____W__________________
_____W_WWWWW____________
_________WWWWWWWW_______
________________________
____O______WWWW_________
________________________
________________________
____WWW_________________
________________________";

            TestTankClient client = new TestTankClient(joinResponse, TankAction.Move);
            Game game = new Game(client);
            game.Run(new SignalWeights());
        }


        [TestMethod]
        public void MultipleBatteriesWithOpponentNearOutOfEnergy()
        {
            joinResponse["grid"] =
@"________________________
___W_____WWWWWWWW_______
___W_W_____B____________
___W_W____________X_____
___W_W____________B_____
___W_W__________________
_WWWWWWWWW______________
_____W______O___________
_____W_WWWWW____________
_________WWWWWWWW_______
________________________
__________WWWW_________
________________________
________________________
____WWW_________________
________________________";

            joinResponse["energy"] = 0;
            joinResponse["health"] = 30;

            TestTankClient client = new TestTankClient(joinResponse, TankAction.Move);
            Game game = new Game(client);
            game.Run(new SignalWeights());
        }
        [TestMethod]
        public void DontNeedBattery()
        {
            joinResponse["grid"] =
@"________________________
___W_____WWWWWWWW_______
___W_W_O________________
___W_W____________X_____
___W_W____________B_____
___W_W__________________
_WWWWWWWWW______________
_____W__________________
_____W_WWWWW____________
_________WWWWWWWW_______
________________________
___________WWWW_________
________________________
________________________
____WWW_________________
________________________";

            joinResponse["energy"] = 10;

            TestTankClient client = new TestTankClient(joinResponse, TankAction.Move);
            Game game = new Game(client);
            game.Run(new SignalWeights());
        }


        [TestMethod]
        public void OpponentBehindMe()
        {
            joinResponse["grid"] =
@"________________________
___W_____WWWWWWWW_______
___W_W__________________
___W_W__________________
___W_W_____________O____
___W_W__________________
_WWWWWWWWW_________X____
_____W__________________
_____W_WWWWW____________
_________WWWWWWWW_______
________________________
___________WWWW_________
________________________
________________________
____WWW_________________
________________________";

            TestTankClient client = new TestTankClient(joinResponse, TankAction.Left);
            Game game = new Game(client);
            game.Run(new SignalWeights());
        }


        [TestMethod]
        public void ShotInTheBack()
        {
            joinResponse["grid"] =
@"________________________
___W_____WWWWWWWW_______
___W_W_____________L____
___W_W__________________
___W_W_____________L____
___W_W__________________
_WWWWWWWWW_________X____
_____W__________________
_____W_WWWWW____________
_________WWWWWWWW_______
________________________
___________WWWW_________
___________________O____
________________________
____WWW_________________
________________________";

            TestTankClient client = new TestTankClient(joinResponse, TankAction.Right | TankAction.Left);
            Game game = new Game(client);
            game.Run(new SignalWeights());
        }



        [TestMethod]
        public void OpponentBetweenBatteryWithNoEnergy()
        {
            joinResponse["grid"] =
@"________________________
___W_____WWWWWWWW_______
___W_W__________________
___W_W__________________
___W_W__________________
___W_W__________________
_WWWWWWWWW_________X____
_____W__________________
_____W_WWWWW_______O____
_________WWWWWWWW_______
________________________
___________WWWW____B____
________________________
________________________
____WWW_________________
________________________";

            joinResponse["energy"] = 0;

            TestTankClient client = new TestTankClient(joinResponse, TankAction.Right | TankAction.Left);
            Game game = new Game(client);
            game.Run(new SignalWeights());
        }


        [TestMethod]
        public void ChasingEachOther()
        {
            joinResponse["grid"] =
@"________________________
___W_____WWWWWWWW_______
________________________
________________________
________________________
________________________
_WWWWWWWWW______________
________________________
________________________
_________WWWWWWWW_______
________________________
___________WWWW_________
________________________
________________________
________________________
____X__________O________";

            joinResponse["orientation"] = "west";

            TestTankClient client = new TestTankClient(joinResponse, TankAction.Move);
            Game game = new Game(client);
            game.Run(new SignalWeights());
        }

        [TestMethod]
        public void OppenentBehindWall()
        {
            joinResponse["grid"] =
@"________________________
___W_____WWWWWWWW_______
___W_W__________________
___W_W__B_________X_____
___W_W__________________
___W_W__________________
_WWWWWWWWWWWWWWWWWWWW___
_____W____________O_____
_____W_WWWWW____________
_________WWWWWWWW_______
________________________
___________WWWW_________
________________________
________________________
____WWW_________________
________________________";

            TestTankClient client = new TestTankClient(joinResponse, TankAction.Right);
            Game game = new Game(client);
            game.Run(new SignalWeights());
        }
    
    }
}
