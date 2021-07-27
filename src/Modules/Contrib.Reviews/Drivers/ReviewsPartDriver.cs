using System.Collections.Generic;
using System.Linq;
using Contrib.Reviews.Models;
using Contrib.Voting.Models;
using Contrib.Voting.Services;

using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Security;
using Orchard.ContentManagement.Handlers;
using System.Reflection;
using System;

namespace Contrib.Reviews.Drivers {
    
    public class ReviewsPartDriver : ContentPartDriver<ReviewsPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IVotingService _votingService;
        private readonly IRepository<ReviewRecord> _reviewRepository;

        public ReviewsPartDriver(IOrchardServices orchardServices, IVotingService votingService, IRepository<ReviewRecord> reviewRepository) {
            _orchardServices = orchardServices;
            _votingService = votingService;
            _reviewRepository = reviewRepository;
        }

        protected override DriverResult Display(ReviewsPart part, string displayType, dynamic shapeHelper) {
            if (!part.ShowStars && !part.ShowReviews)
                return null;

            ReviewsPart reviewsPart = BuildStars(part, displayType);

            var driverResults = new List<DriverResult>();

            if (part.ShowStars) {
                driverResults.AddRange(new []{ ContentShape(
                                     "Parts_Stars",
                                         () => shapeHelper.Parts_Stars(ContentPart: reviewsPart)),
                                ContentShape(
                                    "Parts_Stars_Details",
                                        () => shapeHelper.Parts_Stars_Details(ContentPart: reviewsPart)),
                                ContentShape(
                                    "Parts_Stars_Details_ReadOnly",
                                        () => shapeHelper.Parts_Stars_Details_ReadOnly(ContentPart: reviewsPart)),
                                ContentShape(
                                    "Parts_Stars_SummaryAdmin",
                                        () => shapeHelper.Parts_Stars_SummaryAdmin(ContentPart: reviewsPart)),
                                ContentShape(
                                    "Parts_Stars_AverageOnly",
                                        () => shapeHelper.Parts_Stars_AverageOnly(ContentPart: reviewsPart)),
                                ContentShape(
                                    "Parts_Stars_NoAverage",
                                        () => shapeHelper.Parts_Stars_NoAverage(ContentPart: reviewsPart))});
            }

            //if (part.ShowReviews) {
                driverResults.Add(ContentShape(
                        "Parts_Reviews",
                            () => shapeHelper.Parts_Reviews(ContentPart: reviewsPart)));
            //}

            return Combined(driverResults.ToArray());
        }

        private ReviewsPart BuildStars(ReviewsPart part, string displayType) {
            part.Rating.CurrentVotingResult = _votingService.GetResult(part.ContentItem.Id, "average") ?? new ResultRecord();

            // get the user's vote
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser != null) {
                var userRating = _votingService.Get(vote => vote.Username == currentUser.UserName && vote.ContentItemRecord == part.ContentItem.Record).FirstOrDefault();
                if (userRating != null) {
                    part.Rating.UserRating = userRating.Value;
                }
                else {
                    part.Rating.UserRating = 0;
                }
            }

            if (displayType == "Detail" || displayType == "Details") {
                BuildReviews(part, part.Rating.CurrentVotingResult, currentUser);
            }

            return part;
        }

        private void BuildReviews(ReviewsPart part, ResultRecord currentVotingResult, IUser currentUser) {
            IEnumerable<VoteRecord> voteRecords = _votingService.Get(p => p.ContentItemRecord.Id == part.ContentItem.Id);
            IEnumerable<ReviewRecord> reviewRecords = _reviewRepository.Fetch(r => r.ContentItemRecordId == part.ContentItem.Id);
            IEnumerable<Review> reviews =
                from v in voteRecords
                join r in reviewRecords on v.Id equals r.VoteRecordId
                select new Review {
                    Id = r.Id,
                    Comment = r.Comment,
                    CreatedUtc = r.CreatedUtc,
                    Rating = new Rating {CurrentVotingResult = currentVotingResult, UserRating = v.Value},
                    UserName = _votingService.MaskUserName(v.Username),
                    IsCurrentUsersReview = currentUser != null ? v.Username == currentUser.UserName : false
                };
            part.Reviews.AddRange(reviews.OrderByDescending(r => r.IsCurrentUsersReview).ThenByDescending(r => r.CreatedUtc));
            part.UserHasReviewed = currentUser != null && part.Reviews.Any(r => r.UserName == currentUser.UserName);
        }


        protected override void Importing(ReviewsPart part, ImportContentContext context) {
        }

        protected override void Exporting(ReviewsPart part,ExportContentContext context) {
        }
    }
}