using Laser.Orchard.SEO.Models;
using Orchard;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.SEO.Services {
    public interface IRedirectService : IDependency {
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
        /// Get the redirects cached in memory
        /// </summary>
        /// <returns></returns>
        IEnumerable<RedirectRule> GetCachedRedirects();
        
        /// <summary>
        /// Get the queryable redirects from the DB
        /// </summary>
        /// <returns></returns>
        IQueryable<RedirectRule> GetTable();
        
        /// <summary>
        /// Evict cache for redirects
        /// </summary>
        void ClearCache();
    }
}
