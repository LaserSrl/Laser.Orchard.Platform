using Orchard.ContentManagement;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.Questionnaires.Models {
    public class QuestionnaireSpecificAccessPart : ContentPart {
        private static readonly char[] separator = new[] { '{', '}', ',' };

        public IEnumerable<int> UserIds {
            get {
                return DecodeIds(this.SerializedUserIds);
            }
            set {
                this.SerializedUserIds = EncodeIds(value);
            }
        }

        public string SerializedUserIds {
            get { return this.Retrieve(x => x.SerializedUserIds, ""); }
            set { this.Store(x => x.SerializedUserIds, value); }
        }

        private static string EncodeIds(IEnumerable<int> ids) {
            if (ids == null || !ids.Any()) {
                return string.Empty;
            }

            // use {1},{2} format so it can be filtered with delimiters
            return "{" + string.Join("},{", ids.ToArray()) + "}";
        }


        public static IEnumerable<int> DecodeIds(string ids) {
            if (String.IsNullOrWhiteSpace(ids)) {
                return new int[0];
            }
            // if some of the slices of the string cannot be properly parsed,
            // we still will return those that can.
            return ids
                .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => {
                    int i = -1;
                    if (int.TryParse(s, out i)) {
                        return i;
                    }
                    // if we can't parse return a negative value
                    return -1;
                })
                // take only those that parsed properly
                .Where(i => i > 0)
                .ToList();
        }
    }
}