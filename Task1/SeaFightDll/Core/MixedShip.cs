using SeaFightDll.Interfaces;

namespace SeaFightDll.Core
{
    public class MixedShip : BaseShip, IWarShip, IAuxilliaryShip
    {
        public MixedShip(int length, double speed, int rangeOfAction, string way)
            : base(length, speed, rangeOfAction, way)
        {

        }

        public string GetRepairs()
        {
            return $"Ремонт был произведен на {this.RangeOfAction} клеток!";
        }

        public string GetShot()
        {
            return $"Выстрел сделан на {this.RangeOfAction} клеток!";
        }
    }
}