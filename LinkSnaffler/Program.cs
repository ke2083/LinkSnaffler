using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace LinkSnaffler
{
    class Program
    {


        static void Main(string[] args)
        {
            var error = true;

            while (error == true)
            {
                Console.WriteLine("What URL should we snaffle?");
                var url = Console.ReadLine();
                Uri uri = null;
                if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                {
                    error = false;
                    var parse = new Parse(uri);
                    var links = parse.Uri(uri);
                    var sortedLinks = links.OrderByDescending(l => l.Seen).ThenByDescending(l => l.Variants);
                    Console.WriteLine("Saving report to disk");
                    var doc = new XDocument(new XElement("Links", from l in sortedLinks 
                                                                      select new XElement("Link",
                                                                          new XAttribute("Url", l.Link.ToString()),
                                                                          new XAttribute("Seen", l.Seen),
                                                                          new XAttribute("Status", l.Status.ToString()),
                                                                          new XAttribute("Variants", l.Variants.Count()),
                                                                          new XAttribute("LinkedFrom", l.LinkedFrom.Link == null ? string.Empty : l.LinkedFrom.Link.ToString()),
                                                                          new XElement("Variants", from v in l.Variants
                                                                                                       select new XElement("Variant",
                                                                                                           new XAttribute("Url", v.Link),
                                                                                                           new XAttribute("LinkedFrom", v.LinkedFrom.Link == null ? string.Empty : v.LinkedFrom.Link.ToString()))))));
                    doc.Save("Report.xml");
                    Console.WriteLine("Done");
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("That doesn't look like a real URL...");
                }

            }

        }
    }
}
