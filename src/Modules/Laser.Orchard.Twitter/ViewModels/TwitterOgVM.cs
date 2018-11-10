using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.Twitter.ViewModels {

    public class TwitterOgVM {

        public TwitterOgVM() {
            Card = "summary_large_image";
            SendAsOpenGraph = true;
        }

        #region Card

        [Required]
        public string Card { get; set; }

        /// <summary>
        /// Valorizzato con l'utente twitter inserito nell'open authentication
        /// </summary>
        [Required]
        public string Site { get; set; }

        /// <summary>
        /// attualmente non viene utilizzato
        /// </summary>
        public string Creator { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string Image { get; set; }

        #endregion Card

        public string Message { get; set; }
        public bool SendAsOpenGraph { get; set; }
    }
}