using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Industrial_warehouse
{
    internal class BoxPile
    {
        [JsonProperty]
        double _x;
        [JsonProperty]
        double _y;
        [JsonProperty]
        int _amount;
        [JsonProperty]
        DateTime _lastActivityDate;

        public double X { get { return _x; } }
        public double Y { get { return _y; } }

        public int Amount
        {
            get { return _amount; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("amount cannot be negative");
                _amount = value;
            }
        }

        public DateTime LastActivityDate { get { return _lastActivityDate; } set { _lastActivityDate = value; } }        

        public BoxPile(double x, double y, int amount, DateTime lastActivityDate)
        {
            if (x < 0 || y < 0 || amount < 0) throw new ArgumentException("Invalid values for BoxPile fields !");
            _x= x;
            _y = y;
            _amount = amount;
            _lastActivityDate = lastActivityDate;
        }

        public override string ToString()
        {
            return $"Width:{_x} Height:{_y} Units in stock:{_amount} Last activity:{_lastActivityDate}";
        }
    }
}
