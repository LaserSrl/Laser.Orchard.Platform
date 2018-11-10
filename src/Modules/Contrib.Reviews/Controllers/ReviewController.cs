using System.Web.Mvc;
using Contrib.Reviews.Models;
using Contrib.Voting.Models;
using Contrib.Voting.Services;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using System.Linq;
using Orchard.Services;
using Orchard.UI.Notify;

namespace Contrib.Reviews.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IVotingService _votingService;
        private readonly IRepository<ReviewRecord> _reviewRepository;
        private readonly IClock _clock;

        public Localizer T { get; set; }

        public ReviewController(IOrchardServices orchardServices, IVotingService votingService, IRepository<ReviewRecord> reviewRepository, IClock clock) {
            _orchardServices = orchardServices;
            _votingService = votingService;
            _reviewRepository = reviewRepository;
            _clock = clock;

            T = NullLocalizer.Instance;
        }

        [HttpPost]
        [Authorize]
        public ActionResult Create(int contentId, string comment, string returnUrl) {
            IUser currentUser = _orchardServices.WorkContext.CurrentUser;

            VoteRecord userVote = _votingService.Get(v => v.ContentItemRecord.Id == contentId && v.Username == currentUser.UserName).FirstOrDefault();
            if (userVote == null) {
                _orchardServices.Notifier.Error(T("In order to submit a review, you must also submit a rating."));
                TempData["Comment"] = comment;
            } else if(_reviewRepository.Fetch(r => r.VoteRecordId == userVote.Id).Any()) {
                _orchardServices.Notifier.Error(T("You have already left a review for this item."));
            } else if(string.IsNullOrWhiteSpace(comment)) {
                _orchardServices.Notifier.Error(T("Please fill out your comment before submitting your review."));
            } else if(comment.Length > 1200) {
                _orchardServices.Notifier.Error(T("Your comment must be less than 1,200 characters in length."));
                TempData["Comment"] = comment;
            } else {
                var review = new ReviewRecord {Comment = comment, CreatedUtc = _clock.UtcNow, ContentItemRecordId = contentId, VoteRecordId = userVote.Id};
                _reviewRepository.Create(review);
                _orchardServices.Notifier.Information(T("Thank you for submitting your review."));
            }

            return this.RedirectLocal(returnUrl, "~/");
        }

        [HttpPost]
        [Authorize]
        public ActionResult Delete(int reviewId, string returnUrl) {
            var review = _reviewRepository.Get(reviewId);
            if (review == null) {
                _orchardServices.Notifier.Error(T("Review was not found."));
                return this.RedirectLocal(returnUrl, "~/");
            }
            VoteRecord voteRecord = _votingService.Get(review.VoteRecordId);
            if (voteRecord == null) {
                _orchardServices.Notifier.Error(T("Rating was not found."));
                return this.RedirectLocal(returnUrl, "~/");
            }
            if (voteRecord.Username != _orchardServices.WorkContext.CurrentUser.UserName) {
                _orchardServices.Notifier.Error(T("You are not authorized to delete this Review."));
                return this.RedirectLocal(returnUrl, "~/");
            }
            _reviewRepository.Delete(review);
            _votingService.RemoveVote(voteRecord);
            _orchardServices.Notifier.Information(T("Your Review has been deleted."));
            return this.RedirectLocal(returnUrl, "~/");
        }
    }
}