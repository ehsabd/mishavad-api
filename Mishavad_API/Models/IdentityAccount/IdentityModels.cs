using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization; //for [Data Contract]

namespace Mishavad_API.Models
{

    public class CustomUserRole : IdentityUserRole<int> { }
    public class CustomUserClaim : IdentityUserClaim<int> { }
    public class CustomUserLogin : IdentityUserLogin<int> { }

    public class CustomRole : IdentityRole<int, CustomUserRole>
    {
        public CustomRole() { }
        public CustomRole(string name) { Name = name; }
    }

    public class CustomUserStore : UserStore<ApplicationUser, CustomRole, int,
        CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public CustomUserStore(ApplicationDbContext context)
            : base(context)
        {
        }
    }

    public class CustomRoleStore : RoleStore<CustomRole, int, CustomUserRole>
    {
        public CustomRoleStore(ApplicationDbContext context)
            : base(context)
        {
        }
    }

    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser<int, CustomUserLogin, CustomUserRole,
    CustomUserClaim>
    {
        
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            //CUSTOM USER CLAIMS
            userIdentity.AddClaim(new Claim("FullName", this.UserInfo.FirstName + " " + this.UserInfo.LastName));
            return userIdentity;
        }

        public virtual UserInfo UserInfo { get; set; }
        public  virtual IList<OperationAuth> OperationAuths { get; set; }

        ///TODO: Add Gift fund queries if needed just like Account Balance to the model
        ///
        [NotMapped]
        public long AccountBalance {
            get
            {
                using (var db = new ApplicationDbContext())
                {
                    return 0;
                }
            }
        }
        /*TODO: Because currently we have not created AccountIds for each user I made AccountId Nullable,
        but this should be fixed*/
        public int? AccountId { get; set; }
        public Account Account { get; set; }

        public virtual IList<Campaign> CreatedCampaigns { get; set; }
        public virtual IList<Transaction> CreatedTransactions { get; set; }
        public virtual IList<UserDocumentMap> UserDocumentMaps { get; set; }

    }

    [DataContract]
    [Encrypted]
    public class UserInfo
    {
        [DataMember]
        [Key, ForeignKey("User")]
        public int UserId { get; set; }
        
        [Encrypted]
        [DataMember]
        public string FirstName { get; set; }
        [Encrypted]
        [DataMember]
        public string LastName { get; set; }

        /// <summary>
        /// m: male
        /// f: female
        /// </summary>
        public char Gender { get; set; }

        /// <summary>
        /// Code Melli
        /// کد ملی
        /// </summary>
        [MaxLength(10)]
        //TODO: Add RegEx validation to accept only string with digits
        public string NationalID { get; set; }

        /// <summary>
        /// Shomare Shenasname
        /// شماره شناسنامه
        /// </summary>
        public string BirthCertNo { get; set; }

        /// <summary>
        /// تاریخ تولد
        /// </summary>
        public System.DateTime? DateOfBirth { get; set; }

        public City CityOfBirth { get; set; }
        public Location HomeLocation { get; set; }
        public Location WorkLocation { get; set; }
        public int? YearsOfEducation { get; set; }
        public string MobileNumber { get; set; }
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Binary File Key/IV Index
        /// </summary>
        [BF_Idx]
        public int BF_Idx { get; set; }

        public virtual ApplicationUser User { get; set; }

    }

}