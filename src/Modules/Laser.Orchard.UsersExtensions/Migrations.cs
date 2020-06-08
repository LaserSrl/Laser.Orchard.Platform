using System;
using System.Data;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.ContentTypes.Events;

namespace Laser.Orchard.UsersExtensions {
    public class Migrations : DataMigrationImpl {

        private readonly IContentDefinitionEventHandler _contentDefinitionEventHandlers;

        public Migrations(
            IContentDefinitionEventHandler contentDefinitionEventHandlers) {

            _contentDefinitionEventHandlers = contentDefinitionEventHandlers;
        }

        public int Create() {
            ContentDefinitionManager.AlterPartDefinition("UserRegistrationPolicyPart", part=> part
                .Attachable(true)
                .WithDescription("When a content is in editing mode, it adds the ability to accept/deny a policy and save the choice.")
                );

            ContentDefinitionManager.AlterTypeDefinition("User", content => content
                .WithPart("UserRegistrationPolicyPart"));

            ContentDefinitionManager.AlterPartDefinition("FavoriteCulturePart", builder => builder
                .Attachable(false));
            ContentDefinitionManager.AlterTypeDefinition("User", content => content
                .WithPart("FavoriteCulturePart"));
            _contentDefinitionEventHandlers.ContentPartAttached(
                new ContentPartAttachedContext { ContentTypeName = "User", ContentPartName = "FavoriteCulturePart" });

            return 3;
        }

        /// <summary>
        /// This migration added when we implemented the front end settings for display/
        /// edit controlled by ProfilePart, that need things you want to show on front end to 
        /// be in the actual definitions of ContentTypes.
        /// </summary>
        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition("FavoriteCulturePart", builder => builder
                .Attachable(false));
            ContentDefinitionManager.AlterTypeDefinition("User", content => content
                .WithPart("FavoriteCulturePart"));
            _contentDefinitionEventHandlers.ContentPartAttached(
                new ContentPartAttachedContext { ContentTypeName = "User", ContentPartName = "FavoriteCulturePart" });

            return 2;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterPartDefinition("UserRegistrationPolicyPart", part => part
                .Attachable(true)
                .WithDescription("When a content is in editing mode, it adds the ability to accept/deny a policy and save the choice.")
                );
            return 3;
        }

    }
}
