using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Orchard.Core.Common.ViewModels;
using Orchard.Core.Contents.ViewModels;

namespace Laser.Orchard.AdvancedSearch.ViewModels {
    public class ListContentsViewModelExtension : ListContentsViewModel {
        public ListContentsViewModelExtension() {
            AdvancedOptions = new AdvancedContentOptions();
        }
        public AdvancedContentOptions AdvancedOptions { get; set; }

    }
    public class AdvancedContentOptions {
        public int SelectedLanguageId { get; set; }
        public int SelectedUntranslatedLanguageId { get; set; }
        public List<int> SelectedTermIds { get; set; }
        public string SelectedOwner { get; set; }
        public string SelectedStatus { get; set; }

        public string SelectedFromDate { get; set; }
        public DateTimeEditor SelectedFromDateEditor
        {
            get
            {
                return new DateTimeEditor {
                    Date = SelectedFromDate,
                    Time = "",
                    ShowDate = true,
                    ShowTime = false
                };
            }
            set
            {
                SelectedFromDate = value.Date;
            }
        }
        public string SelectedToDate { get; set; }
        public DateTimeEditor SelectedToDateEditor
        {
            get
            {
                return new DateTimeEditor {
                    Date = SelectedToDate,
                    Time = "",
                    ShowDate = true,
                    ShowTime = false
                };
            }
            set
            {
                SelectedToDate = value.Date;
            }
        }

        //[DataType(DataType.Date)]
        //public DateTime? SelectedFromDate { get;  }
        //[DataType(DataType.Date)]
        //public DateTime? SelectedToDate { get;  }

        public DateFilterOptions DateFilterType { get; set; }
        public bool HasMedia { get; set; }
        public IEnumerable<KeyValuePair<int, string>> LanguageOptions { get; set; }
        public IEnumerable<KeyValuePair<int, string>> TaxonomiesOptions { get; set; }
        public IEnumerable<KeyValuePair<string, string>> StatusOptions { get; set; }

        //used with the MayChooseToSeeOthersContent permission
        private bool _ownedByMe = true;
        public bool OwnedByMe { 
            get {return _ownedByMe;}
            set { _ownedByMe = value; }
        }

        //used with the SeesAllContent permission
        private bool _ownedByMeSeeAll = false;
        public bool OwnedByMeSeeAll {
            get { return _ownedByMeSeeAll; }
            set { _ownedByMeSeeAll = value; }
        }


        //stuff to query on the Content Picker Field
        public int? CPFIdToSearch { get; set; } //Id of the item that we will search in content picker fields
        public string CPFName { get; set; } //name of the CPF

        /// <summary>
        /// Returns the string description of a culture by its identifier.
        /// </summary>
        /// <param name="cultureId">The culture numeral identifier, greater than 0.</param>
        /// <returns>The string description of the desired culture.</returns>
        public string CultureById(int cultureId) {
            return this.LanguageOptions
                .Where(lO => lO.Key == cultureId)
                .FirstOrDefault()
                .Value.ToString();
        }

        /// <summary>
        /// Return the string for a term given its identifier.
        /// </summary>
        /// <param name="termId">The term's numerical identifier.</param>
        /// <returns>The string corresponding to the term.</returns>
        public string TermById(int termId) {
            return this.TaxonomiesOptions
                .Where(tO => tO.Key == termId)
                .FirstOrDefault()
                .Value.ToString();
        }

    }

}