using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Laser.Orchard.SEO.Models {
    [OrchardFeature("Laser.Orchard.Redirects")]
    public class RedirectRule {
        private string _destinationUrl;
        public RedirectRule() {
            CreatedDateTime = DateTime.Now;
        }
        
        public virtual int Id { get; set; }

        public virtual DateTime CreatedDateTime { get; set; }

        [Required]
        [RegularExpression(ValidRelativeUrlPattern, ErrorMessage = "Do not start with '~/'")]
        [Display(Name = "Source URL")]
        [StringLengthMax]
        public virtual string SourceUrl { get; set; }

        [RegularExpression(ValidRelativeUrlPattern, ErrorMessage = "Do not start with '~/'")]
        [Display(Name = "Destination URL")]
        [StringLengthMax]
        public virtual string DestinationUrl {
            get {
                return _destinationUrl == null ? "" : _destinationUrl;
            }
            set {
                _destinationUrl = value;
            }
        }

        public virtual bool IsPermanent { get; set; }

        public const string ValidRelativeUrlPattern = @"^((?!\~\/|\~\\).+)$";

        /// <summary>
        /// Returns a deep copy of the RedirectRule passed as parameter
        /// </summary>
        /// <param name="rule">The RedirectRule object to duplicate</param>
        /// <returns>A deep copy of the RedirectRule passed as parameter</returns>
        public static RedirectRule Copy(RedirectRule rule) {
            return new RedirectRule {
                CreatedDateTime = rule.CreatedDateTime,
                Id = rule.Id,
                SourceUrl = rule.SourceUrl,
                DestinationUrl = rule.DestinationUrl,
                IsPermanent = rule.IsPermanent
            };
        }

        public static IEnumerable<RedirectRule> Copy(IEnumerable<RedirectRule> rules) {
            var copy = new List<RedirectRule>(rules.Count());
            copy.AddRange(rules.Select(rr => Copy(rr)));
            return copy;
        }
    }
}