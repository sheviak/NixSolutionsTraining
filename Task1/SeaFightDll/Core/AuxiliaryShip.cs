using SeaFightDll.Interfaces;

namespace SeaFightDll.Core
{
    public class AuxiliaryShip : BaseShip, IAuxilliaryShip
    {
        public AuxiliaryShip(int length, double speed, int rangeOfAction, string way)
            : base(length, speed, rangeOfAction, way)
        {

        }

        public override string GetInfo()
        {
            return string.Concat("Тип коробля: вспомогательный корабль\n", base.GetInfo());
        }

        public string GetRepairs()
        {
            return $"Ремонт был произведен на {this.RangeOfAction} клеток!";
        }
    }
}