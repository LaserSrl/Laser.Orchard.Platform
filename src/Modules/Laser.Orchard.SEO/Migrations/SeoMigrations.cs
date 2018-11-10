using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Roles.Services;
using System;
using System.Linq;

namespace Laser.Orchard.SEO {
    public class SeoMigrations : DataMigrationImpl {

        private readonly IRoleService _roleService;

        public SeoMigrations(IRoleService roleService) {
            _roleService = roleService;
        }

        public int Create() {
            SchemaBuilder.CreateTable("SeoVersionRecord", table => table
                .ContentPartVersionRecord()
                .Column<string>("TitleOverride", c => c.WithLength(255))
                .Column<string>("Keywords", c => c.WithLength(255))
                .Column<string>("Description", c => c.WithLength(400))
                .Column<bool>("RobotsNoIndex")
                .Column<bool>("RobotsNoFollow")
                .Column<bool>("RobotsNoSnippet")
                .Column<bool>("RobotsNoOdp")
                .Column<bool>("RobotsNoArchive")
                .Column<bool>("RobotsUnavailableAfter")
                .Column<DateTime>("RobotsUnavailableAfterDate")
                .Column<bool>("RobotsNoImageIndex")
                .Column<bool>("GoogleNoSiteLinkSearchBox")
                .Column<bool>("GoogleNoTranslate")
            );

            ContentDefinitionManager.AlterPartDefinition("SeoPart", cfg => cfg
                .Attachable()
                .WithDescription("Consente la personalizzazione degli attributi SEO (title, keywords, description, meta).")
            );

            return 3; //return 3 to stay aligned with old obsolete migrations
        }

        public int UpdateFrom3() {
            SchemaBuilder.CreateTable("RobotsFileRecord",
                table => table
                    .Column<int>("Id", col => col.PrimaryKey().Identity())
                    .Column<string>("FileContent", col => col.Nullable().Unlimited().WithDefault(@"User-agent: *" + Environment.NewLine + @"Disallow: /"))
                );
            return 4;
        }

        public int UpdateFrom4() {
            var roles = _roleService.GetRoles().Where(r => _roleService.GetPermissionsForRole(r.Id).Contains("AccessAdminPanel"));

            foreach (var role in roles) {
                if (!_roleService.GetPermissionsForRole(role.Id).Contains("ManageSEO")) {
                    _roleService.CreatePermissionForRole(role.Name, Permissions.ManageSEO.Name);
                }
            }

            return 5;
        }

        public int UpdateFrom5() {
            SchemaBuilder.AlterTable("SeoVersionRecord", table => table
                .AddColumn<bool>("HideDetailMicrodata"));

            SchemaBuilder.AlterTable("SeoVersionRecord", table => table
                .AddColumn<bool>("HideAggregatedMicrodata"));

            return 6;
        }
        public int UpdateFrom6() {
            SchemaBuilder.AlterTable("SeoVersionRecord", table => table
                .AddColumn<string>("CanonicalUrl", c => c.WithLength(400)));
            return 7;
        }

        #region Obsolete code
        //private readonly IRepository<SeoRecord> _record;
        //private readonly IRepository<SeoVersionRecord> _versionRecord;

        //public SeoMigrations(IRepository<SeoRecord> record, IRepository<SeoVersionRecord> versionRecord) {
        //    _record = record;
        //    _versionRecord = versionRecord;
        //}


        //public int Create() {

        //    SchemaBuilder
        //      .CreateTable("SeoRecord",
        //                   table => table
        //                              .ContentPartRecord()
        //                              .Column<string>("TitleOverride", c => c.WithLength(255))
        //                              .Column<string>("Keywords", c => c.WithLength(255))
        //                              .Column<string>("Description", c => c.WithLength(400))
        //                              );

        //    ContentDefinitionManager
        //      .AlterPartDefinition("SeoPart",
        //                           cfg => cfg
        //                             .Attachable()
        //                             .WithDescription("Consente la personalizzazione degli attributi SEO (title, keywords, description, meta).")
        //                             );

        //    return 1;
        //}

        //public int UpdateFrom1() {
        //    SchemaBuilder.CreateTable("SeoVersionRecord",
        //     table => table
        //      .ContentPartVersionRecord()
        //                            .Column<string>("TitleOverride", c => c.WithLength(255))
        //                            .Column<string>("Keywords", c => c.WithLength(255))
        //                            .Column<string>("Description", c => c.WithLength(400))
        //    );

        //    foreach (var row in _record.Table) {
        //        foreach (var version in row.ContentItemRecord.Versions) {
        //            var newItem = new SeoVersionRecord() {
        //                ContentItemRecord = row.ContentItemRecord,
        //                ContentItemVersionRecord = version,
        //                TitleOverride = row.TitleOverride,
        //                Keywords = row.Keywords,
        //                Description = row.Description,
        //            };
        //            _versionRecord.Create(newItem);
        //        }
        //    }
        //    return 2;
        //}

        //public int UpdateFrom2() {
        //    SchemaBuilder.AlterTable("SeoVersionRecord",
        //        t => t.AddColumn<bool>("RobotsNoIndex"));
        //    SchemaBuilder.AlterTable("SeoVersionRecord",
        //        t => t.AddColumn<bool>("RobotsNoFollow")); 
        //    SchemaBuilder.AlterTable("SeoVersionRecord",
        //         t => t.AddColumn<bool>("RobotsNoSnippet"));
        //    SchemaBuilder.AlterTable("SeoVersionRecord",
        //         t => t.AddColumn<bool>("RobotsNoOdp"));
        //    SchemaBuilder.AlterTable("SeoVersionRecord",
        //         t => t.AddColumn<bool>("RobotsNoArchive"));
        //    SchemaBuilder.AlterTable("SeoVersionRecord",
        //         t => t.AddColumn<bool>("RobotsUnavailableAfter"));
        //    SchemaBuilder.AlterTable("SeoVersionRecord",
        //         t => t.AddColumn<DateTime>("RobotsUnavailableAfterDate"));
        //    SchemaBuilder.AlterTable("SeoVersionRecord",
        //         t => t.AddColumn<bool>("RobotsNoImageIndex"));
        //    SchemaBuilder.AlterTable("SeoVersionRecord",
        //         t => t.AddColumn<bool>("GoogleNoSiteLinkSearchBox"));
        //    SchemaBuilder.AlterTable("SeoVersionRecord",
        //         t => t.AddColumn<bool>("GoogleNoTranslate"));

        //    return 3;
        //}
        #endregion
    }
}