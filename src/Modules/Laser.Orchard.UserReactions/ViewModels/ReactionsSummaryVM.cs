using System.Runtime.Serialization;

namespace Laser.Orchard.UserReactions.ViewModels {
    [DataContract]
    public class ReactionsSummaryVM {
        [DataMember(Name = "UserAuthenticated")]
        public bool UserAuthenticated { get; set; }
        [DataMember(Name = "UserAuthorized")]
        public bool UserAuthorized { get; set; }
        [DataMember(Name = "ContentId")]
        public int ContentId { get; set; }
        [DataMember(Name = "Reactions")]
        public UserReactionsVM[] Reactions { get; set; }

        public ReactionsSummaryVM() {
            Reactions = new UserReactionsVM[0];
        }
    }
}