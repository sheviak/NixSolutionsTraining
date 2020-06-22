namespace SeaFightDll.Core
{
    public abstract class BaseShip
    {
        public int Length { get; set; }
        public double Speed { get; set; }
        public int RangeOfAction { get; set; }
        public string Way { get; set; }

        public BaseShip(int length, double speed, int rangeOfAction, string way)
        {
            this.Length = length;
            this.Speed = speed;
            this.RangeOfAction = rangeOfAction;
            this.Way = way;
        }

        public virtual string GetInfo()
        {
            return
                $"Длина коробля: {this.Length}\n" +
                $"Скорость коробля: {this.Speed}\n" +
                $"Дальность действия: {this.RangeOfAction}\n" +
                $"Путь движения: {this.Way}";
        }

        public static bool operator ==(BaseShip baseShip1, object obj)
        {
            if (!(obj is BaseShip baseShip2))
            {
                return false;
            }

            return (baseShip1.GetType() == baseShip2.GetType()) && (baseShip1.Length == baseShip2.Length && baseShip1.Speed == baseShip2.Speed);
        }

        public static bool operator !=(BaseShip baseShip1, object obj)
        {
            if (!(obj is BaseShip baseShip2))
            {
                return true;
            }

            return (baseShip1.GetType() != baseShip2.GetType()) || (baseShip1.Length != baseShip2.Length || baseShip1.Speed != baseShip2.Speed);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}