using Orchard.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo {
    public class VimeoMigrations : DataMigrationImpl {

        public int Create() {
            //update 20160310: I am going to do the complete migration here, so that when the module is enabled for the first time 
            //it doesn't have to go through all the updates.
            SchemaBuilder.CreateTable("VimeoSettingsPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("ChannelName")
                    .Column<string>("GroupName")
                    .Column<string>("AlbumName")
                    .Column<string>("License")
                    .Column<string>("Privacy")
                    .Column<string>("Password")
                    .Column<bool>("ReviewLink", col => col.WithDefault(false))
                    .Column<string>("Locale")
                    .Column<string>("ContentRatings")
                    .Column<string>("Whitelist")
                    .Column<bool>("AlwaysUploadToGroup")
                    .Column<bool>("AlwaysUploadToAlbum")
                    .Column<bool>("AlwaysUploadToChannel")
                    .Column<string>("AccountType")
                    .Column<DateTime>("LastTimeAccountTypeWasChecked")
                    .Column<string>("UserId")
                    .Column<Int64>("UploadQuotaSpaceFree")
                    .Column<Int64>("UploadQuotaSpaceMax")
                    .Column<Int64>("UploadQuotaSpaceUsed")
                    .Column<DateTime>("LastTimeQuotaWasChecked")
                    .Column<string>("ChannelId")
                    .Column<string>("GroupId")
                    .Column<string>("AlbumId")
                );

            SchemaBuilder.CreateTable("UploadsInProgressRecord",
                table => table
                    .Column<int>("Id", col => col.Identity().PrimaryKey())
                    .Column<Int64>("UploadSize")
                    .Column<Int64>("UploadedSize")
                    .Column<string>("Uri")
                    .Column<string>("CompleteUri")
                    .Column<string>("TicketId")
                    .Column<string>("UploadLinkSecure")
                    .Column<DateTime>("CreatedTime")
                    .Column<int>("MediaPartId")
                    .Column<DateTime>("LastVerificationTime")
                    .Column<DateTime>("ScheduledVerificationTime")
                    .Column<DateTime>("LastProgressTime")
                );

            SchemaBuilder.CreateTable("UploadsCompleteRecord",
                table => table
                    .Column<int>("Id", col => col.Identity().PrimaryKey())
                    .Column<string>("Uri")
                    .Column<int>("ProgressId")
                    .Column<DateTime>("CreatedTime")
                    .Column<bool>("Patched")
                    .Column<bool>("UploadedToGroup")
                    .Column<bool>("UploadedToChannel")
                    .Column<bool>("UploadedToAlbum")
                    .Column<bool>("IsAvailable")
                    .Column<int>("MediaPartId")
                    .Column<DateTime>("ScheduledTerminationTime")
                    .Column<bool>("MediaPartFinished")
                );

            SchemaBuilder.CreateTable("VimeoAccessTokenRecord",
                table => table
                    .Column<int>("Id", col => col.PrimaryKey().Identity())
                    .Column<string>("AccessToken")
                    .Column<int>("RateLimitLimit")
                    .Column<int>("RateLimitRemaining")
                    .Column<DateTime>("RateLimitReset")
                    .Column<double>("RateAvailableRatio")
                );

            return 18;

            //SchemaBuilder.CreateTable("VimeoSettingsPartRecord",
            //    table => table
            //        .ContentPartRecord()
            //        .Column<string>("AccessToken")
            //        .Column<string>("ChannelName")
            //        .Column<string>("GroupName")
            //        .Column<string>("AlbumName")
            //    );

            //return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("UploadsInProgressRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("UploadSize")
                    .Column<int>("UploadedSize")
                    .Column<string>("Uri")
                    .Column<string>("CompleteUri")
                    .Column<int>("TicketId")
                    .Column<string>("UploadLinkSecure")
                );

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("UploadsInProgressRecord",
                table => table
                    .AlterColumn("TicketId",
                        col => col
                            .WithType(System.Data.DbType.String)
                        )
                );

            return 3;
        }

        public int UpdateFrom3() {
            SchemaBuilder.DropTable("UploadsInProgressRecord");

            SchemaBuilder.CreateTable("UploadsInProgressRecord",
                table => table
                    .Column<int>("Id", col => col.Identity().PrimaryKey())
                    .Column<int>("UploadSize")
                    .Column<int>("UploadedSize")
                    .Column<string>("Uri")
                    .Column<string>("CompleteUri")
                    .Column<string>("TicketId")
                    .Column<string>("UploadLinkSecure")
                );

            SchemaBuilder.CreateTable("UploadsCompleteRecord",
                table => table
                    .Column<int>("Id", col => col.Identity().PrimaryKey())
                    .Column<string>("Uri")
                    .Column<int>("ProgressId")
                );

            //update to settings: added default video settings
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<string>("License")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<string>("Privacy")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<string>("Password")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<bool>("ReviewLink", col => col.WithDefault(false))
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<string>("Locale")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<string>("ContentRatings")
                );

            return 4;
        }

        public int UpdateFrom4() {
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<string>("Whitelist")
                );

            return 5;
        }

        public int UpdateFrom5() {
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<bool>("AlwaysUploadToGroup")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<bool>("AlwaysUploadToAlbum")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<bool>("AlwaysUploadToChannel")
                );

            SchemaBuilder.AlterTable("UploadsCompleteRecord",
                table => table
                    .AddColumn<DateTime>("CreatedTime")
                );
            SchemaBuilder.AlterTable("UploadsInProgressRecord",
                table => table
                    .AddColumn<DateTime>("CreatedTime")
                );

            return 6;
        }

        public int UpdateFrom6() {
            SchemaBuilder.AlterTable("UploadsInProgressRecord",
                table => table
                    .AddColumn<int>("MediaPartId")
                );

            SchemaBuilder.AlterTable("UploadsCompleteRecord",
                table => table
                    .AddColumn<bool>("Patched")
                );
            SchemaBuilder.AlterTable("UploadsCompleteRecord",
                table => table
                    .AddColumn<bool>("UploadedToGroup")
                );
            SchemaBuilder.AlterTable("UploadsCompleteRecord",
                table => table
                    .AddColumn<bool>("UploadedToChannel")
                );
            SchemaBuilder.AlterTable("UploadsCompleteRecord",
                table => table
                    .AddColumn<bool>("UploadedToAlbum")
                );
            SchemaBuilder.AlterTable("UploadsCompleteRecord",
                table => table
                    .AddColumn<bool>("IsAvailable")
                );
            SchemaBuilder.AlterTable("UploadsCompleteRecord",
                table => table
                    .AddColumn<int>("MediaPartId")
                );

            return 7;
        }

        public int UpdateFrom7() {
            SchemaBuilder.AlterTable("UploadsInProgressRecord",
                table => table
                    .AlterColumn("UploadSize",
                        col => col
                            .WithType(System.Data.DbType.Int64)
                        )
                );
            SchemaBuilder.AlterTable("UploadsInProgressRecord",
                table => table
                    .AlterColumn("UploadedSize",
                        col => col
                            .WithType(System.Data.DbType.Int64)
                        )
                );
            return 8;
        }

        //Update 2019/08/31: Optimization of API calls
        public int UpdateFrom8() {
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<string>("AccountType")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<DateTime>("LastTimeAccountTypeWasChecked")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<string>("UserId")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<int>("RateLimitLimit")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<int>("RateLimitRemaining")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<DateTime>("RateLimitReset")
                );

            return 9;
        }

        public int UpdateFrom9() {
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<Int64>("UploadQuotaSpaceFree")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<Int64>("UploadQuotaSpaceMax")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<Int64>("UploadQuotaSpaceUsed")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<DateTime>("LastTimeQuotaWasChecked")
                );

            return 10;
        }

        public int UpdateFrom10() {
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<string>("ChannelId")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<string>("GroupId")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<string>("AlbumId")
                );

            return 11;
        }

        public int UpdateFrom11() {
            SchemaBuilder.AlterTable("UploadsInProgressRecord",
                table => table
                    .AddColumn<DateTime>("LastVerificationTime")
                );
            SchemaBuilder.AlterTable("UploadsInProgressRecord",
                table => table
                    .AddColumn<DateTime>("ScheduledVerificationTime")
                );

            return 12;
        }

        public int UpdateFrom12() {
            SchemaBuilder.AlterTable("UploadsInProgressRecord",
                table => table
                    .AddColumn<DateTime>("LastProgressTime")
                );

            return 13;
        }

        public int UpdateFrom13() {
            SchemaBuilder.AlterTable("UploadsCompleteRecord",
                table => table
                    .AddColumn<DateTime>("ScheduledTerminationTime")
                );

            return 14;
        }

        public int UpdateFrom14() {
            SchemaBuilder.AlterTable("UploadsCompleteRecord",
                table => table
                    .AddColumn<bool>("MediaPartFinished")
                );

            return 15;
        }

        public int UpdateFrom15() {
            SchemaBuilder.CreateTable("VimeoAccessTokenRecord",
                table => table
                    .Column<int>("Id", col => col.PrimaryKey().Identity())
                    .Column<string>("AccessToken")
                    .Column<int>("RateLimitLimit")
                    .Column<int>("RateLimitRemaining")
                    .Column<DateTime>("RateLimitReset")
                );

            return 16;
        }

        public int UpdateFrom16() {
            SchemaBuilder.AlterTable("VimeoAccessTokenRecord",
                table => table
                    .AddColumn<double>("RateAvailableRatio")
                );

            return 17;
        }

        public int UpdateFrom17() {
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .DropColumn("AccessToken")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .DropColumn("RateLimitLimit")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .DropColumn("RateLimitRemaining")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .DropColumn("RateLimitReset")
                );

            return 18;
        }
    }
}