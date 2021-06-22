using Laser.Orchard.Translator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Translator.ViewModels {
    public class TranslationRecordViewModel {
        public int Id { get; set; }

        public string ContainerName { get; set; }

        public string ContainerType { get; set; }
        /// <summary>
        /// This property containes Type and Name in a single string.
        /// Example: MLaser.Orchard.SecureData
        /// ContainerType = M
        /// ContainerName = Laser.Orchard.SecureData
        /// </summary>
        public string ContainerTypeAndName { get; set; }

        public string Context { get; set; }

        [AllowHtml]
        public string Message { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [AllowHtml]
        public string TranslatedMessage { get; set; }

        public string Language { get; set; }
        public string OriginalLanguage { get; set; }
        public IEnumerable<string> CultureList { get; set; }

        public TranslationRecordViewModel() {

        }

        public TranslationRecordViewModel(TranslationRecord tr) {
            Id = tr.Id;
            ContainerName = tr.ContainerName;
            ContainerType = tr.ContainerType;
            ContainerTypeAndName = tr.ContainerType + tr.ContainerName;
            Context = tr.Context;
            Message = tr.Message;
            TranslatedMessage = tr.TranslatedMessage;
            Language = tr.Language;
            OriginalLanguage = tr.Language;
        }

        public TranslationRecord ToTranslationRecord() {
            TranslationRecord tr = new TranslationRecord {
                Id = Id,
                ContainerName = ContainerTypeAndName.Substring(1),
                ContainerType = ContainerTypeAndName.Substring(0, 1),
                Context = Context,
                Message = Message,
                TranslatedMessage = TranslatedMessage,
                Language = Language
            };
            return tr;
        }
    }
}