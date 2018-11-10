using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Laser.Orchard.Translator.Models
{
    public class TranslationRecord
    {
        public virtual int Id { get; set; }

        public virtual string ContainerName { get; set; }

        public virtual string ContainerType { get; set; }

        public virtual string Context { get; set; }

        [AllowHtml]
        public virtual string Message { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [AllowHtml]
        public virtual string TranslatedMessage { get; set; }

        public virtual string Language { get; set; }
    }
}