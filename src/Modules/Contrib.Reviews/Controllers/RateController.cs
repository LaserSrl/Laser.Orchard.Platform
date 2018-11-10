using System.Linq;
using System.Web.Mvc;
using Contrib.Reviews.Models;
using Contrib.Voting.Models;
using Contrib.Voting.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Mvc.Extensions;
using Orchard.Security;

namespace Contrib.Reviews.Controllers {
    public class RateController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly IVotingService _votingService;
        private readonly IRepository<ReviewRecord> _reviewRecordRepository;

        public RateController(IOrchardServices orchardServices, IContentManager contentManager, IVotingService votingService,
            IRepository<ReviewRecord> reviewRecordRepository) {
            _orchardServices = orchardServices;
            _reviewRecordRepository = reviewRecordRepository;
            _contentManager = contentManager;
            _votingService = votingService;
        }

        [HttpPost]
        public ActionResult Apply(int contentId, int rating, string returnUrl) {
            var content = _contentManager.Get(contentId);
            if (content == null)
                return this.RedirectLocal(returnUrl, "~/");

            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (RequestIsInvalid(currentUser, rating)) // invalid and no-op (0)
                return this.RedirectLocal(returnUrl, "~/");

            var currentVote = _votingService.Get(vote => vote.Username == currentUser.UserName && vote.ContentItemRecord == content.Record).FirstOrDefault();
            if (rating == -1) {
                ClearRating(currentVote);
            }
            else {
                Vote(content, currentUser, rating, currentVote);
            }

            return this.RedirectLocal(returnUrl, "~/");
        }

        private static bool RequestIsInvalid(IUser currentUser, int rating) {
            return currentUser == null || rating < -1 || rating > 5 || rating == 0;
        }

        private void Vote(ContentItem content, IUser currentUser, int rating, VoteRecord currentVote) {
            if (currentVote != null)
                _votingService.ChangeVote(currentVote, rating);
            else {
                _votingService.Vote(content, currentUser.UserName, HttpContext.Request.UserHostAddress, rating);
            }
        }

        private void ClearRating(VoteRecord currentVote) {
            if (currentVote != null) {
                _votingService.RemoveVote(currentVote);
                foreach (var reviewRecord in _reviewRecordRepository.Fetch(rr => rr.VoteRecordId == currentVote.Id)) {
                    _reviewRecordRepository.Delete(reviewRecord);
                }
            }
        }
    }
}