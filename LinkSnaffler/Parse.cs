using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkSnaffler
{
    public class Parse
    {

        private readonly Uri host;
        private readonly ICollection<LinkInformation> links;

        private object locker;

        /// <summary>
        /// Initializes a new instance of the Parse class.
        /// </summary>
        public Parse(Uri host)
        {
            this.host = host;
            this.links = new List<LinkInformation>();
            this.LinksToFollow = new List<LinkToFollow>();
            this.locker = new Object();
        }

        private void AddLink(Uri link, Uri from, int status)
        {
            // Make sure we are on the same domain.
            if (host.Host != link.Host) return;

            var b = new UriBuilder(from ?? link);
            lock (locker)
            {
                LinkInformation parent = new LinkInformation();
                // Do we have a parent link?
                if (from != null)
                {
                    // Have we seen it before?
                    parent = links.FirstOrDefault(l => l.Link.ToString() == b.Path);
                    if (parent == null)
                    {
                        // Add it and this variant of it.
                        var ub = new UriBuilder();
                        ub.Host = host.Host;
                        ub.Path = b.Path;

                        parent = new LinkInformation { Link = ub.Uri, Status = status };
                        parent.AddVariant(parent);
                    }
                    else
                    {
                        parent.AddVariant(parent);
                    }
                }

                var foundLink = links.FirstOrDefault(l => l.Link.AbsolutePath.ToString() == link.AbsolutePath.ToString());
                if (foundLink == null)
                {
                    foundLink = new LinkInformation { Link = link, LinkedFrom = parent, Status = status };
                    links.Add(foundLink);
                }
                else
                {
                    foundLink.AddVariant(foundLink);
                }

            }
        }

        private ICollection<LinkToFollow> LinksToFollow { get; set; }

        private void FindLinks(HtmlAgilityPack.HtmlNode node, ICollection<LinkToFollow> links, Uri parent, int status)
        {
            if (node.Name == "a")
            {
                System.Uri uri = null;
                var address = node.GetAttributeValue("href", string.Empty);
                if (address.Contains("javascript")) return;

                if (System.Uri.TryCreate(address, UriKind.RelativeOrAbsolute, out uri))
                {
                    if (!uri.IsAbsoluteUri)
                    {
                        var ub = new UriBuilder();
                        ub.Host = host.Host.ToString();
                        ub.Path = uri.ToString();
                        uri = ub.Uri;
                    }

                    if (!links.Any(l => l.Uri.ToString() == uri.ToString()) && uri.Host == host.Host)
                    {
                        links.Add(new LinkToFollow { Uri = uri, Followed = false });
                        AddLink(uri, parent, status);
                    }
                }

            }
            else
            {
                foreach (var n in node.ChildNodes)
                {
                    FindLinks(n, links, parent, status);
                }
            }
        }

        private ICollection<LinkInformation> Follow(Uri uri)
        {
            using (var request = new System.Net.WebClient())
            {
                var html = new HtmlAgilityPack.HtmlWeb();
                html.UserAgent = "LinkSnaffler";

                Console.WriteLine(uri.ToString());
                var contents = html.Load(uri.ToString());
                FindLinks(contents.DocumentNode, LinksToFollow, uri, (int)html.StatusCode);
                while (LinksToFollow.Any(l => l.Followed == false))
                {
                    var nextLink = LinksToFollow.First(l => l.Followed == false);
                    Console.WriteLine(string.Format("{0} ({1} to go)", nextLink.Uri, LinksToFollow.Count(l => l.Followed == false)));
                    nextLink.Followed = true;
                    try
                    {
                        var page = html.Load(nextLink.Uri.ToString());
                        FindLinks(page.DocumentNode, LinksToFollow, nextLink.Uri, (int)html.StatusCode);
                    }
                    catch (System.Net.WebException)
                    {
                        continue;
                    }
                }

                Console.WriteLine(links.Count() + " snaffled");
            }

            return links;
        }

        public ICollection<LinkInformation> Uri(Uri uri)
        {
            return Follow(uri);
        }
    }
}
