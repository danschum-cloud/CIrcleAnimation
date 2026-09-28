using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CircleAnimation
{
    public class AnimationRecord
    {
        public int Id { get; set; }
        public DateTime StartedAt { get; set; }

        public override string ToString() =>
            StartedAt.ToString("dd.MM.yyyy HH:mm:ss");
    }
}