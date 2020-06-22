using SeaFightDll.Interfaces;

namespace SeaFightDll.Core
{
    public class WarShip : BaseShip, IWarShip
    {
        public WarShip(int length, double speed, int rangeOfAction, string way)
            : base(length, speed, rangeOfAction, way)
        {

        }

        public override string GetInfo()
        {
            return string.Concat("Тип коробля: военный корабль\n", base.GetInfo());
        }

        public string GetShot()
        {
            return $"Выстрел сделан на {this.RangeOfAction} клеток!";
        }
    }
}