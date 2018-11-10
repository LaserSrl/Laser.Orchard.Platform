using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.Questionnaires.Models {

    public enum GameType { Competition, NoRanking }

    public class GamePart : ContentPart<GamePartRecord> {

        [Required]
        public string AbstractText {
            get { return this.Retrieve(r => r.AbstractText); }
            set { this.Store(r => r.AbstractText, value); }
        }

        [Required]
        public DateTime GameDate {
            get { return this.Retrieve(r => r.GameDate); }
            set { this.Store(r => r.GameDate, value); }
        }

        [Required]
        public string RankingIOSIdentifier {
            get { return this.Retrieve(r => r.RankingIOSIdentifier); }
            set { this.Store(r => r.RankingIOSIdentifier, value); }
        }

        [Required]
        public string RankingAndroidIdentifier {
            get { return this.Retrieve(r => r.RankingAndroidIdentifier); }
            set { this.Store(r => r.RankingAndroidIdentifier, value); }
        }

        [Required]
        public Int32 MyOrder {
            get { return this.Retrieve(r => r.MyOrder); }
            set { this.Store(r => r.MyOrder, value); }
        }

        public bool workflowfired {
            get { return this.Retrieve(r => r.workflowfired); }
            set { this.Store(r => r.workflowfired, value); }
        }

        public Int32 QuestionsSortedRandomlyNumber {
            get { return this.Retrieve(r => r.QuestionsSortedRandomlyNumber); }
            set { this.Store(r => r.QuestionsSortedRandomlyNumber, value); }
        }

        public bool RandomResponse {
            get { return this.Retrieve(r => r.RandomResponse); }
            set { this.Store(r => r.RandomResponse, value); }
        }

        public Decimal AnswerPoint {
            get { return this.Retrieve(r => r.AnswerPoint); }
            set { this.Store(r => r.AnswerPoint, value); }
        }

        public Decimal AnswerTime {
            get { return this.Retrieve(r => r.AnswerTime); }
            set { this.Store(r => r.AnswerTime, value); }
        }

        public int State {
            get { return this.Retrieve(r => r.State); }
            set { this.Store(r => r.State, value); }
        }

        public GameType GameType {
            get { return this.Retrieve(r => r.GameType); }
            set { this.Store(r => r.GameType, value); }
        }
    }

    public class GamePartRecord : ContentPartRecord {

        public virtual string AbstractText { get; set; }

        public virtual DateTime GameDate { get; set; }

        public virtual string RankingIOSIdentifier { get; set; }

        public virtual string RankingAndroidIdentifier { get; set; }

        public virtual Int32 MyOrder { get; set; }

        public virtual bool workflowfired { get; set; }

        public virtual Int32 QuestionsSortedRandomlyNumber { get; set; }

        public virtual bool RandomResponse { get; set; }

        public virtual Decimal AnswerPoint { get; set; }

        public virtual Decimal AnswerTime { get; set; }

        public virtual int State { get; set; }

        public virtual GameType GameType { get; set; }
    }
}