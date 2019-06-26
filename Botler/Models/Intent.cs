using System;
using System.Collections.Generic;

namespace Botler.Models
{
    public class Intent : IComparable
    {
        public Intent()
        {
            EntitiesCollected = new HashSet<Entity>();
        }

        public string Name { get; set; }

        public double Score { get; set; }

        public bool NeedEntities { get; set; } = false;

        public int EntityLimit { get; set; }

        public int EntityLowerBound { get; set; }

        public string DialogID { get; set; }

        public ICollection<Entity> EntitiesCollected { get; set; }

        public string EntityNeedResponse { get; set; }


        public override string ToString()
        {
            return Name + " " + Score + " " + EntityLimit;
        }

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            Intent otherIntent = (Intent) obj;

            return this.Score.CompareTo(otherIntent.Score);
        }

    }
}