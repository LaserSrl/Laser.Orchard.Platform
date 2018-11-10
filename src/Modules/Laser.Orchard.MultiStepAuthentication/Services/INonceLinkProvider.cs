using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.MultiStepAuthentication.Services {
    public interface INonceLinkProvider : IDependency {

        /// <summary>
        /// Given the nonce, returns URI link for it.
        /// </summary>
        /// <param name="nonce">The nonce for the link.</param>
        /// <returns>A string containing the URI.</returns>
        string FormatURI(string nonce);


        /// <summary>
        /// Given the nonce, returns URI link for it.
        /// </summary>
        /// <param name="nonce">The nonce for the link.</param>
        /// <returns>A string containing the URI.</returns>
        string FormatURI(string nonce,FlowType? flow);

        /// <summary>
        /// Gets the schema that will be used when generating URIs with this provider.
        /// </summary>
        /// <returns>The schema used when generating URIs.</returns>
        string GetSchema();
    }
}
