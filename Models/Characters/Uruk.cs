using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using W8_assignment_template.Interfaces;

namespace W8_assignment_template.Models.Characters
{
    public class Uruk : CharacterBase, ILootable
    {
        public string Treasure { get; set; }

        public Uruk()
        {
        }

        public Uruk(string name, string type, int level, int hp, string treasure, IRoom startingRoom) : base(name, type, level, hp, startingRoom)
        {
            Treasure = treasure;
        }

        public override void UniqueBehavior()
        {
            throw new NotImplementedException();
        }
    }
}
