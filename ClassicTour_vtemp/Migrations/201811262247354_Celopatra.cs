namespace ClassicTour_vtemp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Celopatra : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Credit_Orders",
                c => new
                    {
                        id = c.String(nullable: false, maxLength: 128),
                        price = c.String(nullable: false),
                        providerid = c.String(nullable: false),
                        date = c.DateTime(nullable: false),
                        description = c.String(),
                        verified = c.Boolean(nullable: false),
                        status = c.Boolean(nullable: false),
                        Paid = c.Boolean(nullable: false),
                        customer_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.AspNetUsers", t => t.customer_Id, cascadeDelete: true)
                .Index(t => t.customer_Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        first_name = c.String(nullable: false),
                        last_name = c.String(nullable: false),
                        national_code = c.String(nullable: false),
                        gender = c.Int(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        id = c.String(nullable: false, maxLength: 128),
                        price = c.String(nullable: false),
                        count = c.Int(nullable: false),
                        date = c.DateTime(nullable: false),
                        paymentmethod = c.Int(nullable: false),
                        authority = c.String(),
                        refid = c.String(),
                        trackid = c.String(),
                        sellerid = c.String(),
                        verificationstatus = c.Int(nullable: false),
                        status = c.Boolean(nullable: false),
                        customer_Id = c.String(nullable: false, maxLength: 128),
                        tour_id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.AspNetUsers", t => t.customer_Id, cascadeDelete: true)
                .ForeignKey("dbo.Tours", t => t.tour_id, cascadeDelete: true)
                .Index(t => t.customer_Id)
                .Index(t => t.tour_id);
            
            CreateTable(
                "dbo.Tours",
                c => new
                    {
                        id = c.String(nullable: false, maxLength: 128),
                        title = c.String(nullable: false),
                        tourcode = c.Int(nullable: false, identity: true),
                        startdate = c.String(nullable: false),
                        enddate = c.String(nullable: false),
                        days = c.Int(nullable: false),
                        nights = c.Int(nullable: false),
                        capacity = c.Int(nullable: false),
                        startpoint = c.String(nullable: false),
                        city = c.String(nullable: false),
                        price = c.String(nullable: false),
                        img = c.String(),
                        alt = c.String(),
                        status = c.Boolean(nullable: false),
                        description = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Credit_Orders", "customer_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Orders", "tour_id", "dbo.Tours");
            DropForeignKey("dbo.Orders", "customer_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.Orders", new[] { "tour_id" });
            DropIndex("dbo.Orders", new[] { "customer_Id" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Credit_Orders", new[] { "customer_Id" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.Tours");
            DropTable("dbo.Orders");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Credit_Orders");
        }
    }
}
