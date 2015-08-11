using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TurboTank;
using Agilix.Shared;

namespace TurboTankTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
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
                'orientation':'north',
                'health':300,
                'grid': '________________________
___W_____WWWWWWWW_______
___W_W__________________
___W_W_______B__________
___W_W__________________
___W_W__________________
_WWWWWWWWW___L____O_____
_____W__________________
_____W_WWWWW____________
_________WWWWWWWW_______
________________________
___________WWWW_________
__X_____________________
________________________
____WWW_________________
________________________'
}");

            TestTankClient client = new TestTankClient(joinResponse, TurboTank.Action.Left);
            Game game = new Game(client);
        }
    }
}
