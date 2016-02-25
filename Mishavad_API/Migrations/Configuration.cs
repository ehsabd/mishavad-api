namespace Mishavad_API.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    using System.Data.Entity.SqlServer;
    using System.Data.Entity.Migrations.Model;
    using System.Collections.Generic;

    using Mishavad_API.Models;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity;

    using System.IO;
    using System.Security.Claims;


    using System.Data.Entity.Infrastructure; // namespace for the EdmxWriter class
    using System.Xml;

    internal sealed class Configuration : DbMigrationsConfiguration<Mishavad_API.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = true;
            //Customize Migration Generator for:
            // - CreatedDateTime column of Campaign 
            SetSqlGenerator("System.Data.SqlClient", new CustomSqlServerMigrationSqlGenerator());
        }

        /// <summary>
        /// write a message to the Package Manager Console
        /// </summary>
        public void Debug(Mishavad_API.Models.ApplicationDbContext context, string s, params object[] args)
        {
            var fullString = string.Format(s, args).Replace("'", "''");
            context.Database.ExecuteSqlCommand(string.Format("print '{0}'", fullString));
        }

        protected override void Seed(Mishavad_API.Models.ApplicationDbContext context)
        {

            CheckReseedInvoiceandAccountIdentity(context);
            
            //TODO: IF you want seeding to work remove this!
            return;
            // TODO: Remove this in-production
            // /*  
            try
            {
                using (var ctx = new Mishavad_API.Models.ApplicationDbContext())
                {
                    using (var writer = new XmlTextWriter(@"D:/My Documents/MishavadProjects/Mishavad_API/Mishavad_API/Model.edmx", System.Text.Encoding.Default))
                    {
                        EdmxWriter.WriteEdmx(ctx, writer);
                    }

                }
            }
            catch { }
            //*/

            Helpers.EncryptionService.LoadBinaryFile("D:/My Documents/MishavadProjects/Mishavad_API/Mishavad_API/App_Data/keys.mdb");

            context.FileServers.AddOrUpdate(c => c.Id, new FileServer { Id = 1, ServerIP = "37.220.11.235" });

            context.CampaignCategories.AddOrUpdate(c => c.Id, new CampaignCategory { Id = 1, Name = "صنعت و فناوری" },
                new CampaignCategory { Id = 2, Name = "آموزش و پژوهش" },
                new CampaignCategory { Id = 3, Name = "خیریه و اجتماعی" });

            context.ProjectStages.AddOrUpdate(c => c.Id, new ProjectStage { Id = 1, Name = "پژوهش مقدماتی" },
                new ProjectStage { Id = 2, Name = "تولید آزمایشی" },
                new ProjectStage { Id = 3, Name = "تولید نهایی" });

            //Add Cities
            if (context.Cities.Count()==0)
                context.Database.ExecuteSqlCommand(File.ReadAllText("D:/My Documents/MishavadProjects/Mishavad_API/Mishavad_API/MySqlQueries/SQLQuery1_AddCities.sql"));

            //TODO: Add ISO 3166-2:IR province codes
            var prov_codes = @"[
                                 {'code': 'IR-03', name: 'اردبیل'},
                                 {'code': 'IR-32', name: 'البرز'},
                                 {'code': 'IR-02', name: 'آذربایجان غربی'},
                                 {'code': 'IR-01', name: 'آذربایجان شرقی'},
                                 {'code': 'IR-06', name: 'بوشهر'},
                                 {'code': 'IR-08', name: 'چهارمحال و بختیاری'},
                                 {'code': 'IR-04', name: 'اصفهان'},
                                 {'code': 'IR-14', name: 'فارس'},
                                 {'code': 'IR-19', name: 'گیلان'},
                                 {'code': 'IR-27', name: 'گلستان'},
                                 {'code': 'IR-24', name: 'همدان'},
                                 {'code': 'IR-23', name: 'هرمزگان'},
                                 {'code': 'IR-05', name: 'ایلام'},
                                 {'code': 'IR-15', name: 'کرمان'},
                                 {'code': 'IR-17', name: 'کرمانشاه'},
                                 {'code': 'IR-29', name: 'خراسان جنوبی'},
                                 {'code': 'IR-30', name: 'خراسان رضوی'},
                                 {'code': 'IR-31', name: 'خراسان شمالی'},
                                 {'code': 'IR-10', name: 'خوزستان'},
                                 {'code': 'IR-18', name: 'کهگیلویه و بویراحمد'},
                                 {'code': 'IR-16', name: 'کردستان'},
                                 {'code': 'IR-20', name: 'لرستان'},
                                 {'code': 'IR-22', name: 'مرکزی'},
                                 {'code': 'IR-21', name: 'مازندران'},
                                 {'code': 'IR-28', name: 'قزوین'},
                                 {'code': 'IR-26', name: 'قم'},
                                 {'code': 'IR-12', name: 'سمنان'},
                                 {'code': 'IR-13', name: 'سیستان و بلوچستان'},
                                 {'code': 'IR-07', name: 'تهران'},
                                 {'code': 'IR-25', name: 'یزد'},
                                 {'code': 'IR-11', name: 'زنجان'}
                                ]";
            if (context.Users.Count()==0)
                SeedUsers(context);

            var store = new CustomUserStore(context);
            var manager = new UserManager<ApplicationUser, int>(store);

            var admins = context.Users.Where(u => u.Email == "ehsabd@outlook.com").ToList();
            //Add Claims for Admins:
            foreach (var admin in admins)
            {
                if (!admin.Claims.Any(c => (c.ClaimType == ClaimTypes.Role && c.ClaimValue == "TopLevelAdmin"))) {
                    manager.AddClaim(admin.Id, new Claim(ClaimTypes.Role, "TopLevelAdmin"));
                }
            }
            /*
           //Remove current info
           if (query.Any())
           {
               foreach (var myuser in query.ToList())
               {
                  
                   //Remove any user infoes when seeding
                   var myinf = context.UserInfos.Find(myuser.Id);
                   if (myinf != null)
                       context.UserInfos.Remove(myinf);

                   // context.Users.Remove(myuser);
                   // context.SaveChangesAsync();
               }
           }
           context.SaveChangesAsync();
           
            */
            Debug(context, "Seeding Campaigns...");
            var ehsabd_user_Id = manager.FindByEmail("ehsabd@outlook.com").Id;
            SeedCampaigns(context, ehsabd_user_Id);
            Debug(context, "Seeding Rewards...");
            SeedRewards(context);
            RemoveRedundantGiftFunds(context);
            context.SaveChangesAsync();
        }

        private void CheckReseedInvoiceandAccountIdentity(ApplicationDbContext ctx)
        {
            try
            {
                ReseedTableIdentity(ctx, "Accounts", 10000000);
            }
            catch
            {
            }
 
            try
            {
                ReseedTableIdentity(ctx, "Transactions", 10000000);
            }
            catch
            {

            }
            try
            {
                ReseedTableIdentity(ctx, "DepositInvoices", 10000000);
            }
            catch
            {

            }
        }
        private void ReseedTableIdentity(ApplicationDbContext ctx, string table,int reseed_value)
        {
            ctx.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('@p0', RESEED, @p1); ", table, reseed_value);
        }

        private void SeedUsers(ApplicationDbContext ctx) {


            //Tests for adding users!
            //TODO: These are for test purposes. Remove all for in-production except admins
            var testUsers = new List<string> { "ehsabd@outlook.com", "hamidrezakhalatbari@yahoo.com", "malizadeh@gmx.com", "homasadeghi@gmx.com" };

            var userDetails = new System.Collections.Generic.Dictionary<string, UserInfo>();

            userDetails.Add("ehsabd@outlook.com", new UserInfo { FirstName = "احسان", LastName = "عبدخدایی", NationalID = "0946940071" });
            userDetails.Add("malizadeh@gmx.com", new UserInfo { FirstName = "مهران", LastName = "علیزاده", NationalID = "1234567890" });
            userDetails.Add("homasadeghi@gmx.com", new UserInfo { FirstName = "هما", LastName = "صادقی", NationalID = "1234567890" });

            TestName[] testFirstNames = {
                new TestName("هانیه","honey","f"),
                new TestName("زهرا","z","f"),
                new TestName("مریم", "m","f"),
                new TestName("لیلا", "lily","f"),
                new TestName("شیلا", "sh","f"),
                new TestName("زهره","zohreh","f"),
                new TestName("سمیرا","samira","f"),
                new TestName("نگین","negin","f"),
                new TestName("نفیسه","nafis","f"),
                new TestName("نگار", "negar", "f")
            };

            TestName[] testLastNames = {
                new TestName("تقی زاده","taghi"),
                new TestName("حاتمی","hatami"),
                new TestName("بحرانی", "bahrani"),
                new TestName("رشیدی", "rashidi"),
                new TestName("ملتی", "mellati"),
                new TestName("کرمی","karami"),
                new TestName("سروستانی","sarvestani"),
                new TestName("رجبی","rajabi"),
                new TestName("رضوی","razavi"),
                new TestName("حق پرست","hagh"),
                new TestName("وطن دوست","vatan"),
                new TestName("محمدی","mohammadi")
            };

            foreach (var firstName in testFirstNames)
                foreach (var lastName in testLastNames)
                {
                    var username = firstName.Name_en + lastName.Name_en + "@gmx.com";
                    testUsers.Add(username);
                    userDetails.Add(username, new UserInfo { FirstName = firstName.Name, LastName = lastName.Name, NationalID = "1234567890" });
                }

            Random rnd = new Random();
            testUsers = testUsers.OrderBy(x => rnd.Next()).ToList();

            var store = new CustomUserStore(ctx);
            var manager = new UserManager<ApplicationUser, int>(store);

            //Add or Update Infoes

            foreach (var username in testUsers)
            {
                var details = userDetails[username];
                var user = new ApplicationUser { Email = username, UserName = username, UserInfo = details };
                var thisuser = manager.FindByEmail(username);

                if (thisuser == null)
                {
                    manager.Create(user, "angel123");
                }
                else {
                    thisuser.UserInfo = details;
                    ctx.Entry(thisuser).State = EntityState.Modified;
                }

            }
            ctx.SaveChangesAsync();
        }

        private void SeedCampaigns(ApplicationDbContext context, int createdById) {

            context.Locations.AddOrUpdate(new Location { CityId = 1092 }, new Location { CityId = 1131 }, new Location { CityId = 1254 });
            context.SaveChangesAsync();
            
            var locations = context.Locations.Select(l=>l.Id).ToList();
            // Add Approved Campaigns
            context.Campaigns.AddOrUpdate(c => c.Title,
                new Campaign
                {
                    CreatedById = createdById,
                    Status = CampaignStatus.PreliminaryRegistered,
                    Title = "بنیاد کودک ایران",
                    Tagline = "بنیاد کودک ایران، رویکردی نوین در حمایت از دانش آموزان نیازمند",
                    Story = @"در ایران به‌نام «موسسه خیریه رفاه کودک» به ثبت رسیده است، در سال 1374 فعالیت خود را آغاز نموده است. اولین دفتر بنیادکودک در شهر شیراز افتتاح شده و هم‌زمان با آن دفتر این مؤسسه در شهرپورتلند ایالت اورگان امریکا نیزبا نام “Child Foundation” تاسیس شد. بنیادکودک در سال 1378 با شمارۀ 398 به ثبت رسیده و رسماً فعالیت خود را آغاز کرد. بنیاد کودک ایران به عنوان یک موسسه خیریه غیردولتی، غیرانتفاعی، غیرسیاسی و مردم نهاد آرمان خود را دستیابی به این هدف مقدس قرار داده‌ است :
«هیچ دانش‌آموز با استعدادی نباید به دلیل فقر ، از تحصیل و تلاش بازبماند»
بنیاد کودک ایران، با دریافت مجوز فعالیت ملی از وزارت کشور به شماره 25540 موفق شده است در طی بیست سال گذشته در شهرهای اصفهان، ارومیه، اردبیل، آمل، ایلام، بم، بروجرد، بوشهر، تبریز، تهران، جیرفت، داراب، رشت، زابل، زاهدان،  شیراز، شهرکرد، کرج، کرمانشاه، کاشان، مشهد، یاسوج و مراغه  دفتر نمایندگی فعال تاسیس نماید و دامنه یاری‌رسانی خود  را به استان‌های مختلف کشور گسترش دهد. علاوه بر این، دفاتر بنیاد در شهرهای قزوین، اهواز، ابهر، یزد، زاهدان، گرگان و کرمان نیز فعالیت مددکاری داشته و دانش‌آموزان نیازمند این استان‌ها را مورد حمایت قرار می‌دهند.   
بنیادکودک، با رویکرد حفظ ارتباط ایرانیان مقیم خارج از کشور با هموطنانشان در ایران، در عرصه بین‌المللی نیز حضور پررنگ و قابل توجهی داشته‌ است. دفاتربین‌المللی بنیاد کودک در 4 قاره جهان ، در کشورهای امریکا، سوئیس، آلمان ، انگلستان و کانادا فعال می‌باشند. بنیادکودک، طی بیست سال گذشته بیش از ده هزاردانش‌آموز کم‌بضاعت و خانواده‌هایشان را تحت حمایت خود قرار داده است. اکنون نیز بیش از 6800 خانواده همچنان با همیاری هموطنان نیکوکارمان توسط شبکه حمایتی گسترده بنیاد کودک حمایت می‌شوند که بیش از هفتصد نفر از آنان درحال حاضر در دانشگاه‌ها و مؤسسات آموزش‌عالی معتبر مشغول به تحصیل‌اند.    
 بنیاد کودک، با به کارگیری روشهای علمی، نوین و همچنین اطلاع رسانی شفاف و عملکرد پاسخگو، برای نخستین‌بار در ایران، یک سیستم نرم‌افزار یکپارچه Webbase جامع را راه‌اندازی نموده و کلیه اطلاعات مربوط به مددجویان ، همیاران ، مالی و حسابداری و گزارش‌دهی خود را تحت آن نرم‌افزار اجرایی ساخته است. این نرم‌افزار با توجه به امکانات و نیازهای مؤسسه و صرفاً برای «بنیادکودک ایران» طراحی و پیاده‌سازی شده است.
از لحاظ تامین منابع مالی و هزینه های ادرای و تشکیلاتی  نیز ،بنیادکودک ایران ،  تنها با کمک‌های مردمی اداره شده و هیچگونه بودجه دولتی در اختیار ندارد. گردش مالی موسسه نیز همه ساله توسط موسسات معتبر (حسابداران رسمی) حسابرسی شده و گزارش‌های آن نیز درهمین وب سایت موسسه در دسترس عموم می‌باشد.
",
                    TargetFund = 10000000,
                    TotalDays = 30,
                    StartDateUtc = DateTime.UtcNow.AddDays(-30).AddHours(-10), //e.g. will end 10 hrs later,
                    CampaignCategoryId = 3,
                    ProjectStageId = 1,
                    LocationId=locations[0],
                    ThumbnailFileServerId =1,ThumbnailFilePath = "childf.jpg" 
                
                },
                new Campaign
                {
                    CreatedById = createdById,
                    Status = CampaignStatus.PreliminaryRegistered,
                    Title = "کمپین تولید جاکت هوشمند",
                    Tagline = "فکر می کنید پوشیدن ژاکت هوشمند چه حسی داشته باشد؟",
                    Story = @"هرگز برای مد یخ هرگز دوباره! کاپشن ThermalTech تنفس، ضد آب و ضد باریک پایین نگاه سنتی زمستان حجیم می باشد. را گرم برشته در حالی که شما در مورد فعالیت در فضای باز طبیعی خود را از قبیل خرید بروید،
قدم زدن در اطراف شهر، و یا کارهای در حال اجرا. ما حتی کاپشن که در طول فعالیت هایی مانند پیاده روی، آهسته دویدن، کمپینگ و اسنوبورد را حفظ خواهد کرد دور فراست جک دارند، بدون توجه به فعالیت، ما سه کاپشن مختلف را تحت پوشش!
ThermalTech پارچه است که به راحتی به تقریبا هر سبک لباس و مواد درج، با ویژگی های کلیدی زیر استفاده از اختراع، انرژی بسیار سبک وزن و جذب تکنولوژی پارچه، پارچه ThermalTech طراحی شده است برای جذب اشعه ماوراء بنفش خورشید (و همچنین انرژی از منابع نور مصنوعی)
و تبدیل آنها به گرما برای گرم استفاده کنندگان تا یک 18F اضافی در عرض چند دقیقه. ضد زنگ مش فولاد موضوعات پارچه در عین حال قوی هستند سبک وزن، اعطای وام به استفاده از طول عمر طولانی، در حالی که آن را طراحی کاغذ نازک وزن اضافی که عایق حرارتی ذخیره سازی به طور معمول به لباس را کاهش می دهد",
                    TargetFund = 20000000,
                    TotalDays = 60,
                    StartDateUtc = DateTime.UtcNow.AddDays(-45), //e.g. will end 15 days later
                    CampaignCategoryId = 1,
                    ProjectStageId = 3,
                    LocationId = locations[1],
                    ThumbnailFileServerId = 1,
                    ThumbnailFilePath = "jacket.jpg" 
                
                },
                new Campaign
                {
                    CreatedById = createdById,
                    Status = CampaignStatus.PreliminaryRegistered,
                    Title = "یک مسواک برقی متفاوت",
                    Tagline = "2 برابر برداشت پلاک با مسواک برقی ",
                    TargetFund = 50000000,
                    TotalDays = 30,
                    StartDateUtc = DateTime.UtcNow.AddDays(-30).AddHours(-1.5), //e.g. will end 1.5 hours later
                    CampaignCategoryId = 1,
                    ProjectStageId = 2,
                    LocationId = locations[2]
                },
                new Campaign
                {
                    CreatedById = createdById,
                    Status = CampaignStatus.PreliminaryRegistered,
                    Title = "بازی شاهزاده رومی",
                    Tagline = "یک بازی ایرانی مبتنی بر انیمیشن ایرانی ",
                    Story = @"آنچه درباره انیمیشن سینمایی شاهزاده روم مورد توجه منتقدان و صاحب نظران سینما قرار گرفت، کیفیت متفاوت آن نسبت به انیمیشن‌های قبلی تولید شده در ایران بود.
تا جایی که رسول صدرعاملی به عنوان رئیس هییت انتخاب جشنواره اعلام کرد که نتوانسته‌اند از شاهزاده روم عبور کنند. 
 همچنین حسن ایوبی به عنوان یکی از تدوینگرهای حرفه‌ای سینما در مصاحبه‌های خود عنوان کرد برخی پیشنهادهای کاری خود را رد کرده تا تدوین شاهزاده روم را انجام دهد.
شایسته ذکر است که اکثر عوامل این فیلم از دانش آموختگان دانشگاه شاهد می باشند [۶]
انيميشن سینمایی «شاهزاده روم» از ۱۳ آبان ۱۳۹۴ همزمان با روز دانش‌آموز در گروه سینمایی آزاد اکران شد. ",
                    TargetFund = 80000000,
                    TotalDays = 90,
                    StartDateUtc = DateTime.UtcNow,
                    CampaignCategoryId = 2,
                    ProjectStageId=3,
                    LocationId= locations[1],
                    ThumbnailFileServerId = 1,
                    ThumbnailFilePath = "kid-sketch-compressed.jpg" 
                
                },
                 new Campaign
                 {
                     CreatedById = createdById,
                     Status = CampaignStatus.PreliminaryRegistered,
                     Title = "بنیاد کودک ایران2",
                     Tagline = "بنیاد کودک ایران، رویکردی نوین در حمایت از دانش آموزان نیازمند",
                     Story = @"در ایران به‌نام «موسسه خیریه رفاه کودک» به ثبت رسیده است، در سال 1374 فعالیت خود را آغاز نموده است. اولین دفتر بنیادکودک در شهر شیراز افتتاح شده و هم‌زمان با آن دفتر این مؤسسه در شهرپورتلند ایالت اورگان امریکا نیزبا نام “Child Foundation” تاسیس شد. بنیادکودک در سال 1378 با شمارۀ 398 به ثبت رسیده و رسماً فعالیت خود را آغاز کرد. بنیاد کودک ایران به عنوان یک موسسه خیریه غیردولتی، غیرانتفاعی، غیرسیاسی و مردم نهاد آرمان خود را دستیابی به این هدف مقدس قرار داده‌ است :
«هیچ دانش‌آموز با استعدادی نباید به دلیل فقر ، از تحصیل و تلاش بازبماند»
بنیاد کودک ایران، با دریافت مجوز فعالیت ملی از وزارت کشور به شماره 25540 موفق شده است در طی بیست سال گذشته در شهرهای اصفهان، ارومیه، اردبیل، آمل، ایلام، بم، بروجرد، بوشهر، تبریز، تهران، جیرفت، داراب، رشت، زابل، زاهدان،  شیراز، شهرکرد، کرج، کرمانشاه، کاشان، مشهد، یاسوج و مراغه  دفتر نمایندگی فعال تاسیس نماید و دامنه یاری‌رسانی خود  را به استان‌های مختلف کشور گسترش دهد. علاوه بر این، دفاتر بنیاد در شهرهای قزوین، اهواز، ابهر، یزد، زاهدان، گرگان و کرمان نیز فعالیت مددکاری داشته و دانش‌آموزان نیازمند این استان‌ها را مورد حمایت قرار می‌دهند.   
بنیادکودک، با رویکرد حفظ ارتباط ایرانیان مقیم خارج از کشور با هموطنانشان در ایران، در عرصه بین‌المللی نیز حضور پررنگ و قابل توجهی داشته‌ است. دفاتربین‌المللی بنیاد کودک در 4 قاره جهان ، در کشورهای امریکا، سوئیس، آلمان ، انگلستان و کانادا فعال می‌باشند. بنیادکودک، طی بیست سال گذشته بیش از ده هزاردانش‌آموز کم‌بضاعت و خانواده‌هایشان را تحت حمایت خود قرار داده است. اکنون نیز بیش از 6800 خانواده همچنان با همیاری هموطنان نیکوکارمان توسط شبکه حمایتی گسترده بنیاد کودک حمایت می‌شوند که بیش از هفتصد نفر از آنان درحال حاضر در دانشگاه‌ها و مؤسسات آموزش‌عالی معتبر مشغول به تحصیل‌اند.    
 بنیاد کودک، با به کارگیری روشهای علمی، نوین و همچنین اطلاع رسانی شفاف و عملکرد پاسخگو، برای نخستین‌بار در ایران، یک سیستم نرم‌افزار یکپارچه Webbase جامع را راه‌اندازی نموده و کلیه اطلاعات مربوط به مددجویان ، همیاران ، مالی و حسابداری و گزارش‌دهی خود را تحت آن نرم‌افزار اجرایی ساخته است. این نرم‌افزار با توجه به امکانات و نیازهای مؤسسه و صرفاً برای «بنیادکودک ایران» طراحی و پیاده‌سازی شده است.
از لحاظ تامین منابع مالی و هزینه های ادرای و تشکیلاتی  نیز ،بنیادکودک ایران ،  تنها با کمک‌های مردمی اداره شده و هیچگونه بودجه دولتی در اختیار ندارد. گردش مالی موسسه نیز همه ساله توسط موسسات معتبر (حسابداران رسمی) حسابرسی شده و گزارش‌های آن نیز درهمین وب سایت موسسه در دسترس عموم می‌باشد.
",
                     TargetFund = 10000000,
                     TotalDays = 30,
                     StartDateUtc = DateTime.UtcNow.AddDays(-30).AddHours(-10), //e.g. will end 10 hrs later,
                     CampaignCategoryId = 3,
                     ProjectStageId = 1,
                     LocationId = locations[0],
                     ThumbnailFileServerId = 1,
                     ThumbnailFilePath = "childf.jpg"

                 },
                new Campaign
                {
                    CreatedById = createdById,
                    Status = CampaignStatus.Approved,
                    Title = "کمپین تولید جاکت هوشمند2",
                    Tagline = "فکر می کنید پوشیدن ژاکت هوشمند چه حسی داشته باشد؟",
                    Story = @"هرگز برای مد یخ هرگز دوباره! کاپشن ThermalTech تنفس، ضد آب و ضد باریک پایین نگاه سنتی زمستان حجیم می باشد. را گرم برشته در حالی که شما در مورد فعالیت در فضای باز طبیعی خود را از قبیل خرید بروید،
قدم زدن در اطراف شهر، و یا کارهای در حال اجرا. ما حتی کاپشن که در طول فعالیت هایی مانند پیاده روی، آهسته دویدن، کمپینگ و اسنوبورد را حفظ خواهد کرد دور فراست جک دارند، بدون توجه به فعالیت، ما سه کاپشن مختلف را تحت پوشش!
ThermalTech پارچه است که به راحتی به تقریبا هر سبک لباس و مواد درج، با ویژگی های کلیدی زیر استفاده از اختراع، انرژی بسیار سبک وزن و جذب تکنولوژی پارچه، پارچه ThermalTech طراحی شده است برای جذب اشعه ماوراء بنفش خورشید (و همچنین انرژی از منابع نور مصنوعی)
و تبدیل آنها به گرما برای گرم استفاده کنندگان تا یک 18F اضافی در عرض چند دقیقه. ضد زنگ مش فولاد موضوعات پارچه در عین حال قوی هستند سبک وزن، اعطای وام به استفاده از طول عمر طولانی، در حالی که آن را طراحی کاغذ نازک وزن اضافی که عایق حرارتی ذخیره سازی به طور معمول به لباس را کاهش می دهد",
                    TargetFund = 20000000,
                    TotalDays = 60,
                    StartDateUtc = DateTime.UtcNow.AddDays(-45), //e.g. will end 15 days later
                    CampaignCategoryId = 1,
                    ProjectStageId = 3,
                    LocationId = locations[1],
                    ThumbnailFileServerId = 1,
                    ThumbnailFilePath = "jacket.jpg"

                },
                new Campaign
                {
                    CreatedById = createdById,
                    Status = CampaignStatus.Approved,
                    Title = "یک مسواک برقی متفاوت2",
                    Tagline = "2 برابر برداشت پلاک با مسواک برقی ",
                    TargetFund = 50000000,
                    TotalDays = 30,
                    StartDateUtc = DateTime.UtcNow.AddDays(-30).AddHours(-1.5), //e.g. will end 1.5 hours later
                    CampaignCategoryId = 1,
                    ProjectStageId = 2,
                    LocationId = locations[2]
                },
                new Campaign
                {
                    CreatedById = createdById,
                    Status = CampaignStatus.Approved,
                    Title = "2بازی شاهزاده رومی",
                    Tagline = "یک بازی ایرانی مبتنی بر انیمیشن ایرانی ",
                    Story = @"آنچه درباره انیمیشن سینمایی شاهزاده روم مورد توجه منتقدان و صاحب نظران سینما قرار گرفت، کیفیت متفاوت آن نسبت به انیمیشن‌های قبلی تولید شده در ایران بود.
تا جایی که رسول صدرعاملی به عنوان رئیس هییت انتخاب جشنواره اعلام کرد که نتوانسته‌اند از شاهزاده روم عبور کنند. 
 همچنین حسن ایوبی به عنوان یکی از تدوینگرهای حرفه‌ای سینما در مصاحبه‌های خود عنوان کرد برخی پیشنهادهای کاری خود را رد کرده تا تدوین شاهزاده روم را انجام دهد.
شایسته ذکر است که اکثر عوامل این فیلم از دانش آموختگان دانشگاه شاهد می باشند [۶]
انيميشن سینمایی «شاهزاده روم» از ۱۳ آبان ۱۳۹۴ همزمان با روز دانش‌آموز در گروه سینمایی آزاد اکران شد. ",
                    TargetFund = 80000000,
                    TotalDays = 90,
                    StartDateUtc = DateTime.UtcNow,
                    CampaignCategoryId = 2,
                    ProjectStageId = 3,
                    LocationId = locations[1],
                    ThumbnailFileServerId = 1,
                    ThumbnailFilePath = "kid-sketch-compressed.jpg"

                }

                );
            context.SaveChangesAsync();
           

        }

        private void SeedRewards(ApplicationDbContext context)
        {
            var rnd = new Random();
            string[] images = { "badge2b-300px.png", "trophy-300px.png", "momoko-Gold-Medallion-300px.png" };
            for (var id = 1; id <= 4; id++)
            {
                context.Rewards.RemoveRange(context.Rewards.Where(r => r.CampaignId == id));
            }
            context.SaveChangesAsync();

            var campIds = context.Campaigns.Select(c => c.Id).ToArray();
            foreach (var id in campIds)
            {
                var j = 0;
                for (var i = 1; i <= 5; i++)
                {
                    j += rnd.Next(1, 5);
                    context.Rewards.Add(new Reward
                    {
                        CampaignId = id,
                        Amount = 10000 + 5000 * j,
                        AddressRequired = rnd.Next(2) == 1,
                        Description = "توضیحات پاداش" + i.ToString(),
                        Title = "پاداش" + i.ToString(),
                        NAvailable = 50 + rnd.Next(15) * 10,
                        ImageFilePath = images[rnd.Next(images.Length)],
                        ImageFileServerId = 1
                    });
                }
            }
            context.SaveChangesAsync();
           
               

        }

        private void RemoveRedundantGiftFunds(ApplicationDbContext context) {
            /*
            We do not seed gift funds here because it is contrary to the real word performance
            Gift funds are seeded within js unit tests
            */

            var camps = context.Campaigns.Where(c => c.Status == CampaignStatus.Approved).ToList();
            foreach (var camp in camps)
            {
                if (camp.CollectedFund > camp.TargetFund * 2.5)
                {
                    RemoveCampaignFunds(context, camp);
                } 
            }
        }

        private void RemoveCampaignFunds(ApplicationDbContext context, Campaign camp)
        {
           /* context.IncomingFundOrders.RemoveRange(context.IncomingFundOrders.Where(o => o.GiftFundCampaignId == camp.Id));
            context.GiftFunds.RemoveRange(context.GiftFunds.Where(gf => gf.CampaignId == camp.Id));
            context.SaveChangesAsync();*/
        }
        private class TestName
        {
            public string Name { get; set; }
            public string Name_en { get; set; }
            public string Gender { get; set; }
            public TestName(string Name, string Name_en, string Gender = "na")
            {
                this.Name = Name;
                this.Name_en = Name_en;
                this.Gender = Gender;
            }

        }
    }

    internal class CustomSqlServerMigrationSqlGenerator : SqlServerMigrationSqlGenerator
        {
            protected override void Generate(AddColumnOperation addColumnOperation)
            {
                SetCreatedDateTimeColumn(addColumnOperation.Column);

                base.Generate(addColumnOperation);
            }

            protected override void Generate(CreateTableOperation createTableOperation)
            {
                SetCreatedDateTimeColumn(createTableOperation.Columns);

                base.Generate(createTableOperation);
            }

        private static void SetCreatedDateTimeColumn(IEnumerable<ColumnModel> columns)
            {
                foreach (var columnModel in columns)
                {
                    SetCreatedDateTimeColumn(columnModel);
                }
            }

            private static void SetCreatedDateTimeColumn(PropertyModel column)
            {
            System.Diagnostics.Debug.WriteLine(column.Name);
                if (column.Name == "CreatedDateUtc")
                {
                    column.DefaultValueSql = "GETUTCDATE()";
                }
            System.Diagnostics.Debug.WriteLine(column.DefaultValueSql);
            }
        }
 

    }

