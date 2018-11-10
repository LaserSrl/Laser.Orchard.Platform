using Laser.Orchard.Events.Models;
using Laser.Orchard.Questionnaires.Models;
using Laser.Orchard.Questionnaires.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentPicker.Fields;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Laser.Orchard.Questionnaires.Handlers {

    public class GamePartHandler : ContentHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IQuestionnairesServices _questionnairesServices;

        public GamePartHandler(IRepository<GamePartRecord> repository, IContentDefinitionManager contentDefinitionManager,
            IQuestionnairesServices questionnairesServices) {
            _contentDefinitionManager = contentDefinitionManager;
            _questionnairesServices = questionnairesServices;

            Filters.Add(StorageFilter.For(repository));

            OnInitializing<GamePart>((context, part) => {
                int currYear = DateTime.Now.Year;
                int currMonth = DateTime.Now.Month;
                int currDay = DateTime.Now.Day;
                part.GameDate = new DateTime(currYear, currMonth, currDay);
                AdjustContentItem(context.ContentItem);
            });

            OnPublished<GamePart>((context, part) => GamePartPublished(context));
            OnUnpublished<GamePart>((context, part) => GamePartUnpublished(context));
            OnRemoved<GamePart>((context, part) => GamePartRemoved(context));
        }

        private void AdjustContentItem(ContentItem ci) {
            if (ci.As<GamePart>() != null) {
                if (ci.As<ActivityPart>() == null) {
                    _contentDefinitionManager.AlterTypeDefinition(ci.ContentType,
                     cfg => cfg
                    .WithPart("ActivityPart"));
                }
            }
        }

        protected override void BuildDisplayShape(BuildDisplayContext context) {
            base.BuildDisplayShape(context);
            if (context.ContentItem.As<GamePart>() != null) {
                if (context.ContentItem.As<ActivityPart>() != null) {
                    if (DateTime.Now < context.ContentItem.As<ActivityPart>().DateTimeStart) {
                        context.ContentItem.As<GamePart>().State = 2;
                    }
                    else {
                        if (DateTime.Now > context.ContentItem.As<ActivityPart>().DateTimeEnd)
                            context.ContentItem.As<GamePart>().State = 0;
                        else
                            context.ContentItem.As<GamePart>().State = 1;
                    }
                }
            }
            if (context.DisplayType == "Detail") {
                if (context.ContentItem.As<GamePart>() != null) {
                    if (context.ContentItem.As<ActivityPart>().DateTimeStart > DateTime.Now || context.ContentItem.As<ActivityPart>().DateTimeEnd < DateTime.Now) {
                        RemoveQuestions(context.ContentItem);
                    }
                    else {
                        ShuffleQuestions(context.ContentItem);
                    }
                }
            }
        }

        private void RemoveQuestions(ContentItem ci) {
            if (ci.Parts != null) {
                var fields = ci.Parts.SelectMany(x => x.Fields.Where(f => f.FieldDefinition.Name == typeof(ContentPickerField).Name)).Cast<ContentPickerField>();
                foreach (dynamic field in fields) {
                    if (((IEnumerable<ContentItem>)field.ContentItems).Count() > 0) {
                        ContentItem sampleContentItem = (ContentItem)field.ContentItems[0];
                        if (sampleContentItem.As<QuestionnairePart>() != null) {
                            RemoveQuestionsFromField(ci, field);
                        }
                    }
                }
            }
        }

        private void RemoveQuestionsFromField(ContentItem ci, ContentField field) {
            field.Storage = null;
    //            ((dynamic)field).Ids = new int[] { };
    
        }

        private void ShuffleQuestions(ContentItem ci) {
            if (ci.Parts != null) {
                var fields = ci.Parts.SelectMany(x => x.Fields.Where(f => f.FieldDefinition.Name == typeof(ContentPickerField).Name)).Cast<ContentPickerField>();
                foreach (dynamic field in fields) {
                    if (((IEnumerable<ContentItem>)field.ContentItems).Count() > 0) {
                        ContentItem sampleContentItem = (ContentItem)field.ContentItems[0];
                        if (sampleContentItem.As<QuestionnairePart>() != null) {
                            ShuffleQuestions(ci, field);
                        }
                    }
                }
            }
        }

        private void ShuffleQuestions(ContentItem ci, dynamic field) {
            ContentItem sampleContentItem = (ContentItem)field.ContentItems[0];
            QuestionnairePart qp = sampleContentItem.As<QuestionnairePart>();
            if (qp != null) {
                Int32 QuestionsSortedRandomlyNumber = ci.As<GamePart>().QuestionsSortedRandomlyNumber;// qp.Settings.GetModel<QuestionnairesPartSettingVM>().QuestionsSortedRandomlyNumber;
                if (QuestionsSortedRandomlyNumber > 0) {
                    qp.QuestionsToDisplay = Shuffle(qp.Questions.Where(x => x.Published).ToList().ConvertAll(x => (dynamic)x)).ConvertAll(x => (QuestionRecord)x).ToList().Take(QuestionsSortedRandomlyNumber).ToList();
                }

                bool RandomResponse = ci.As<GamePart>().RandomResponse;// qp.Settings.GetModel<QuestionnairesPartSettingVM>().RandomResponse;
                if (RandomResponse) {
                    foreach (QuestionRecord qr in qp.Questions) {
                        qr.Answers = Shuffle(qr.Answers.ToList().ConvertAll(x => (dynamic)x)).ConvertAll(x => (AnswerRecord)x).ToList();
                    }
                }
            }
        }

        private List<dynamic> Shuffle(List<dynamic> list) {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1) {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                dynamic value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }

        protected void GamePartPublished(PublishContentContext context) {
            Int32 gId = ((dynamic)context.ContentItem).Id;
            DateTime timeGameEnd = ((dynamic)context.ContentItem).ActivityPart.DateTimeEnd;
            _questionnairesServices.ScheduleEmailTask(gId, timeGameEnd);
            //base.Published(context);
        }

        protected void GamePartUnpublished(PublishContentContext context) {
            Int32 gId = ((dynamic)context.ContentItem).Id;
            _questionnairesServices.UnscheduleEmailTask(gId);
            //base.Unpublished(context);
        }

        protected void GamePartRemoved(RemoveContentContext context) {
            Int32 gId = ((dynamic)context.ContentItem).Id;
            _questionnairesServices.UnscheduleEmailTask(gId);
            //base.Removed(context);
        }
    }
}