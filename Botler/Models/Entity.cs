using System;

namespace Botler.Models
{
    public class Entity
    {
        public Entity () {}

        public string Type { get; set; }

        public string Text { get; set; }

        public int StartIndex { get; set; }

        public int EndIndex { get; set; }


        public override bool Equals(Object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;

            Entity ent = (Entity)obj;

            if(ent.Type.Equals(Type) && ent.Text.Equals(Text))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ Text.GetHashCode();
        }


        public override string ToString()
        {
            return "Type: " + Type + " Text " + Text + " StartIndext " + StartIndex + " EndIndex " + EndIndex;
        }

    }
}