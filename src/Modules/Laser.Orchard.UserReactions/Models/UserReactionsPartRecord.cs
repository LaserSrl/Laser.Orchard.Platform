using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Laser.Orchard.UserReactions.Models {
  
    /// <summary>
    /// Oggetto content cioè contenuto della pagina
    /// </summary>
    public class UserReactionsPart : ContentPart<UserReactionsPartRecord> {

        //per ogni content posso inserire una lista di reactions (vedi oggetto sotto)
        public IList<UserReactionsSummaryRecord> Reactions {
            get {
                return Record.Reactions;
            }
        }

    }

    /// <summary>
    /// Record di reactions 
    /// </summary>
    public class UserReactionsPartRecord : ContentPartRecord {

        public UserReactionsPartRecord() {
           Reactions = new List<UserReactionsSummaryRecord>();
        }

       public virtual IList<UserReactionsSummaryRecord> Reactions { get; set; }
    }

   
    
}