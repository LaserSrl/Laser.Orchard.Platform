
namespace Laser.Orchard.Highlights.Enums {

    /// <summary>
    /// Web links targets.
    /// </summary>
    public enum LinkTargets {
        /// <summary>
        /// _blank
        /// </summary>
        _blank,
        /// <summary>
        /// _self
        /// </summary>
        _self,
        /// <summary>
        /// _modal
        /// </summary>
        _modal

    }

    /// <summary>
    /// Modalità di visualizzazione delle immagini
    /// </summary>
    public enum DisplayPlugin {
        Slideshow,            // Slideshow
        Lista,                 // Elenco
        Unico,                // Elemento unico 
        Carousel              // Carousel
    };

    /// <summary>
    /// Tipologia di vista da gestire
    /// </summary>
    public enum DisplayTemplate {
        List,                     // Sostituirà tutti i banner list
        SlideShow                // SlideShow immagini
    };

    /// <summary>
    /// Tipologia di titolo
    /// </summary>
    public enum TitleSize {
        Predefinito,        // a seconda della tipologia di item
        Principale,         // H1
        MediaPriorita,      // H4
        BassaPriorita,      // H5
        Titoletto,          // H1 class="scns"
        NessunTitolo
    }

    public enum ItemsSourceTypes { 
        ByHand,
        FromQuery
    }
}

