using AutoMapper;
using Laser.Orchard.Questionnaires.Models;
using Laser.Orchard.Questionnaires.Services;
using Laser.Orchard.Questionnaires.ViewModels;
using Laser.Orchard.StartupConfig.Localization;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.ContentManagement.Handlers;
using Orchard.Tasks.Scheduling;
using System;
using System.Globalization;
using Orchard.Localization.Services;
using System.Xml.Linq;

namespace Laser.Orchard.Questionnaires.Drivers {

    public class GamePartDriver : ContentPartCloningDriver<GamePart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IDateLocalization _dateLocalization;

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Mobile.Questionnaires.Game"; }
        }

        public GamePartDriver(IOrchardServices orchardServices, IDateLocalization dateLocalization,
            IScheduledTaskManager taskManager, IDateLocalizationServices dateServices,
            IQuestionnairesServices questionnairesServices) {
            _orchardServices = orchardServices;
            _dateLocalization = dateLocalization;
        }

        protected override DriverResult Editor(GamePart part, dynamic shapeHelper) {
            var viewModel = new GamePartVM();

            DateTime? tmpGameDate = _dateLocalization.ReadDateLocalized(part.GameDate);
            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<GamePart, GamePartVM>().ForMember(dest => dest.GameDate, opt => opt.Ignore());
            });

            var mapper = mapperConfiguration.CreateMapper();
            mapper.Map<GamePart, GamePartVM>(part, viewModel);
            viewModel.GameDate = _dateLocalization.WriteDateLocalized(tmpGameDate,true);
            return ContentShape("Parts_GamePart_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/GamePart_Edit", Model: viewModel, Prefix: Prefix));
        }

        protected override DriverResult Editor(GamePart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new GamePartVM();
            if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                var mapperConfiguration = new MapperConfiguration(cfg => {
                    cfg.CreateMap<GamePartVM, GamePart>()
                        .ForMember(dest => dest.GameDate, opt => opt.Ignore());
                });

                var mapper = mapperConfiguration.CreateMapper();

                mapper.Map<GamePartVM, GamePart>(viewModel, part);
                if (!String.IsNullOrWhiteSpace(viewModel.GameDate)) {
                    part.GameDate = _dateLocalization.StringToDatetime(viewModel.GameDate, "") ?? DateTime.Now;
                }
            }
            
            return Editor(part, shapeHelper);
        }

        protected override void Importing(GamePart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            part.AbstractText = root.Attribute("AbstractText").Value;
            part.AnswerPoint = Decimal.Parse(root.Attribute("AnswerPoint").Value, CultureInfo.InvariantCulture);
            part.AnswerTime = Decimal.Parse(root.Attribute("AnswerTime").Value, CultureInfo.InvariantCulture);
            part.GameDate = DateTime.Parse(root.Attribute("GameDate").Value, CultureInfo.InvariantCulture);
            part.GameType = ((GameType)Enum.Parse(typeof(GameType), root.Attribute("GameType").Value));
            part.MyOrder = Int32.Parse(root.Attribute("MyOrder").Value);
            part.QuestionsSortedRandomlyNumber = Int32.Parse(root.Attribute("QuestionsSortedRandomlyNumber").Value);
            part.RandomResponse = Boolean.Parse(root.Attribute("RandomResponse").Value);
            part.RankingAndroidIdentifier = root.Attribute("RankingAndroidIdentifier").Value;
            part.RankingIOSIdentifier = root.Attribute("RankingIOSIdentifier").Value;
            part.State = Int32.Parse(root.Attribute("State").Value);
            //workflowfired non serve per l'import
        }

        protected override void Exporting(GamePart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("AbstractText", part.AbstractText);
            root.SetAttributeValue("AnswerPoint", part.AnswerPoint.ToString(CultureInfo.InvariantCulture));
            root.SetAttributeValue("AnswerTime", part.AnswerTime.ToString(CultureInfo.InvariantCulture));
            root.SetAttributeValue("GameDate", part.GameDate.ToString(CultureInfo.InvariantCulture));
            root.SetAttributeValue("GameType", part.GameType);
            root.SetAttributeValue("MyOrder", part.MyOrder);
            root.SetAttributeValue("QuestionsSortedRandomlyNumber", part.QuestionsSortedRandomlyNumber);
            root.SetAttributeValue("RandomResponse", part.RandomResponse);
            root.SetAttributeValue("RankingAndroidIdentifier", part.RankingAndroidIdentifier);
            root.SetAttributeValue("RankingIOSIdentifier", part.RankingIOSIdentifier);
            root.SetAttributeValue("State", part.State);
            //workflowfired non serve per l'import
        }

        protected override void Cloning(GamePart originalPart, GamePart clonePart, CloneContentContext context) {
            clonePart.AbstractText = originalPart.AbstractText;
            clonePart.GameDate = originalPart.GameDate;
            clonePart.RankingIOSIdentifier = originalPart.RankingIOSIdentifier;
            clonePart.RankingAndroidIdentifier = originalPart.RankingAndroidIdentifier;
            clonePart.MyOrder = originalPart.MyOrder;
            //worflowFired is set in the QuestionnaireServices
            clonePart.QuestionsSortedRandomlyNumber = originalPart.QuestionsSortedRandomlyNumber;
            clonePart.RandomResponse = originalPart.RandomResponse;
            clonePart.AnswerPoint = originalPart.AnswerPoint;
            clonePart.AnswerTime = originalPart.AnswerTime;
            //State is set in the Handler
            clonePart.GameType = originalPart.GameType;
        }
    }
}