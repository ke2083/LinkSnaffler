using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkSnaffler
{
    public class LinkInformation
    {
        // Fields...

        public Uri Link { get; set; }
        public ICollection<LinkInformation> Variants { get; set; }
        public LinkInformation LinkedFrom { get; set; }
        public int Seen { get; set; }
        public int Status { get; set; }

        public void AddVariant(LinkInformation from)
        {
            var existant = Variants.FirstOrDefault(v => v.ToString() == from.ToString());
            if (existant == null)
            {
                from.Seen = 1;
                Variants.Add(from);
            }
            else
            {
                existant.Seen += 1;
            }
        }
        /// <summary>
        /// Initializes a new instance of the LinkInformation class.
        /// </summary>
        public LinkInformation()
        {
            Variants = new List<LinkInformation>();
        }

    }
}
