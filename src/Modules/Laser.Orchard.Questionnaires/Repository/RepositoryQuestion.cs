using Laser.Orchard.Questionnaires.Models;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.Repository {
    public class RepositoryQuestion : Repository<QuestionRecord>, IRepository<QuestionRecord> {
        public RepositoryQuestion(ITransactionManager transactionManager) : base(transactionManager) {
        }

        public override void Update(QuestionRecord entity) {
            if (string.IsNullOrWhiteSpace(entity.Identifier)) {
                entity.Identifier = Guid.NewGuid().ToString("n");
            }
            base.Update(entity);
        }
    }
}