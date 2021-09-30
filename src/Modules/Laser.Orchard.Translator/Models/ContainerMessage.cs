using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Translator.Models {
    /// <summary>
    /// USE WITH CAUTION!
    /// This class contains the message count for a container type - container name combination.
    /// The count can be either the number of messages to translate, the number of translated messages or the total number of messages.
    /// It depends on how the query used to get the messages is written.
    /// </summary>
    public class ContainerMessage {
        public string ContainerType { get; set; }
        public string ContainerName { get; set; }
        public int Count { get; set; }
    }
}