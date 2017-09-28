/*
Copyright 2017 James Craig

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System.Collections.Generic;
using System.Net;

namespace Spidey
{
    /// <summary>
    /// Basic options class
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Options"/> class.
        /// </summary>
        public Options()
        {
            StartLocations = new List<string>();
            Ignore = new List<string>();
            Allow = new List<string>();
            FollowOnly = new List<string>();
        }

        /// <summary>
        /// Gets the default.
        /// </summary>
        /// <value>The default.</value>
        public static Options Default => new Options();

        /// <summary>
        /// Gets or sets the allowed items.
        /// </summary>
        /// <value>The allowed items.</value>
        public List<string> Allow { get; set; }

        /// <summary>
        /// Gets the credentials.
        /// </summary>
        /// <value>The credentials.</value>
        public NetworkCredential Credentials { get; set; }

        /// <summary>
        /// Gets or sets the follow only list.
        /// </summary>
        /// <value>The follow only list.</value>
        public List<string> FollowOnly { get; set; }

        /// <summary>
        /// Gets or sets the ignore list.
        /// </summary>
        /// <value>The ignore list.</value>
        public List<string> Ignore { get; set; }

        /// <summary>
        /// Gets the proxy.
        /// </summary>
        /// <value>The proxy.</value>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        /// Gets or sets the start locations.
        /// </summary>
        /// <value>The start locations.</value>
        public List<string> StartLocations { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use default credentials].
        /// </summary>
        /// <value><c>true</c> if [use default credentials]; otherwise, <c>false</c>.</value>
        public bool UseDefaultCredentials { get; set; }
    }
}