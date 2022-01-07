using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redmine.ManagerWPF.Database.Migrations._2022._01
{
    [Migration(202201071033)]
    public class InitMigration_202201071033 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Projects").Exists())
            {
                Create.Table("Projects")
                    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                    .WithColumn("SourceId").AsInt32().NotNullable()
                    .WithColumn("Name").AsString(1000).NotNullable()
                    .WithColumn("Description").AsString(32000).Nullable()
                    .WithColumn("Link").AsString(1000).Nullable()
                    .WithColumn("DataStart").AsDateTime2().NotNullable()
                    .WithColumn("DateEnd").AsDateTime2().Nullable()
                    .WithColumn("Status").AsString();
            }

            if (!Schema.Table("Issues").Exists())
            {
                Create.Table("Issues")
                    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                    .WithColumn("SourceId").AsInt32().NotNullable()
                    .WithColumn("Name").AsString(1000).NotNullable()
                    .WithColumn("Description").AsString(32000).Nullable()
                    .WithColumn("Link").AsString(1000).Nullable()
                    .WithColumn("MainTaskId")
                        .AsInt32()
                        .Indexed("IX_Issues_MainTaskId")
                        .ForeignKey("FK_Issues_MainTaskId", "Issues", "Id")
                        .Nullable()
                    .WithColumn("ProjectId")
                        .AsInt32()
                        .Indexed("IX_Issues_ProjectId")
                        .ForeignKey("FK_Issues_ProjectId", "Projects", "Id")
                        .NotNullable()
                    .WithColumn("Status").AsString()
                    .WithColumn("Done").AsBoolean().WithDefaultValue(false);
            }

            if (!Schema.Table("Comments").Exists())
            {
                Create.Table("Comments")
                    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                    .WithColumn("SourceId").AsInt32().NotNullable()
                    .WithColumn("Text").AsString(32000).Nullable()
                    .WithColumn("Link").AsString(1000).Nullable()
                    .WithColumn("CreatedBy").AsString().Nullable()
                    .WithColumn("Date").AsDateTime2().NotNullable()
                     .WithColumn("IssueId")
                        .AsInt32()
                        .Indexed("IX_Comments_IssueId")
                        .ForeignKey("FK_Comments_IssueId", "Issues", "Id")
                        .NotNullable()
                    .WithColumn("Done").AsBoolean().WithDefaultValue(false);
            }

            if (!Schema.Table("TimeIntervals").Exists())
            {
                Create.Table("TimeIntervals")
                    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                    .WithColumn("TimeIntervalStart").AsDateTime2().Nullable()
                    .WithColumn("TimeIntervalEnd").AsDateTime2().Nullable()
                     .WithColumn("IssueId")
                        .AsInt32()
                        .Indexed("IX_TimeIntervals_IssueId")
                        .ForeignKey("FK_TimeIntervals_IssueId", "Issues", "Id")
                        .Nullable()
                    .WithColumn("CommentId")
                        .AsInt32()
                        .Indexed("IX_TimeIntervals_CommentId")
                        .ForeignKey("FK_TimeIntervals_CommentId", "Comments", "Id")
                        .Nullable()
                   .WithColumn("Note").AsString(32000).Nullable();

            }
        }

        public override void Down()
        {
            if (Schema.Table("TimeIntervals").Exists())
            {
                Delete.Table("TimeIntervals");
            }

            if (Schema.Table("Comments").Exists())
            {
                Delete.Table("Comments");
            }

            if (Schema.Table("Issues").Exists())
            {
                Delete.Table("Issues");
            }

            if (Schema.Table("Projects").Exists())
            {
                Delete.Table("Projects");
            }
        }
    }
}

