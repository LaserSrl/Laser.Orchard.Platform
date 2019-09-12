using Laser.Orchard.SEO.Models;
using Orchard;
using System.Collections.Generic;

namespace Laser.Orchard.SEO.Services {
    public interface IRedirectService : ISingletonDependency {
        /// <summary>
        /// Gets the RedirectRule objects based on the pagination
        /// </summary>
        /// <param name="startIndex">Start index for pagination</param>
        /// <param name="pageSize">Page size (maximum number of objects)</param>
        /// <returns>An IEnumerable of RedirectRule objects</returns>
        IEnumerable<RedirectRule> GetRedirects(int startIndex = 0, int pageSize = 0);

        /// <summary>
        /// Get the RedirectRule objects with the give ids.
        /// </summary>
        /// <param name="itemIds">The ids of the RedirectRule objects</param>
        /// <returns>An IEnumerable of RedirectRule objects</returns>
        IEnumerable<RedirectRule> GetRedirects(int[] itemIds);

        /// <summary>
        /// Get the total number of RedirectRule objects.
        /// </summary>
        /// <returns>The total number of RedirectRule objects.</returns>
        int GetRedirectsTotalCount();

        /// <summary>
        /// Get the RedirectRule object with the given Id
        /// </summary>
        /// <param name="id">The id of the desired RedirectRule object</param>
        /// <returns>The desired RedirectRule object</returns>
        RedirectRule GetRedirect(int id);

        /// <summary>
        /// Get the RedirectRule object for the given SourceUrl
        /// </summary>
        /// <param name="path">The SourceUrl of the desired RedirectRule object</param>
        /// <returns>The desired RedirectRule object</returns>
        RedirectRule GetRedirect(string path);

        /// <summary>
        /// Updates the RedirectRule object. The object is identified by the id.
        /// </summary>
        /// <param name="redirectRule">The RedirectRule object we wish to update.</param>
        /// <returns>The updated RedirectRule object</returns>
        /// <exception cref="RedirectRuleDuplicateException">Throws a RedirectRuleDuplicateException if a RedirectRule with the same 
        /// SourceUrl already exists.</exception>  
        RedirectRule Update(RedirectRule redirectRule);

        /// <summary>
        /// Adds a new RedirectRule object to the repository.
        /// </summary>
        /// <param name="redirectRule">The RedirectRule object we wish to add</param>
        /// <returns>The new RedirectRule object</returns>
        /// <exception cref="RedirectRuleDuplicateException">Throws a RedirectRuleDuplicateException if a RedirectRule with the same 
        /// SourceUrl already exists.</exception>  
        RedirectRule Add(RedirectRule redirectRule);

        /// <summary>
        /// Deletes the RedirectRule object given by the id.
        /// </summary>
        /// <param name="id">The id of the RedirectRule object to be deleted</param>
        void Delete(int id);

        /// <summary>
        /// Deletes the RedirectRule object. The object is identified by the id.
        /// </summary>
        /// <param name="redirectRule">The RedirectRule object to be deleted.</param>
        void Delete(RedirectRule redirectRule);
        /// <summary>
        /// Clear redirects cache.
        /// </summary>
        void ClearCache();
        /// <summary>
        /// Number of elements in the redirects cache.
        /// </summary>
        /// <returns></returns>
        int CountCached();
    }
}
