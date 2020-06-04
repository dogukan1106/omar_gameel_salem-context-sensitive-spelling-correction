using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winnow
{
    public class Sample
    {
        public bool[] Features { get; set; }
        public bool Class { get; set; }

        public Sample ToggleClass()
        {
            var invertedSample = (Sample)this.MemberwiseClone();
            invertedSample.Class = !invertedSample.Class;
            return invertedSample;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("(" + Class + ")[");

            for (int i = 0; i < Features.Length; i++)
            {
                sb.Append(Convert.ToInt16(Features[i]));

                if (i < Features.Length - 1)
                {
                    sb.Append(',');
                }
            }

            sb.Append("]");

            return sb.ToString();
        }
    }
}