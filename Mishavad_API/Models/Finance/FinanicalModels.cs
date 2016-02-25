using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mishavad_API.Models
{
    /*NOTE 
    IMPORTANT: We used the first answer of this thread as the general guideline to design this part
    http://stackoverflow.com/questions/29688982/derived-account-balance-vs-stored-account-balance-for-a-simple-bank-account/29713230#29713230
    We only used Transactions a bit differently. Transactios are usually pairs of JournalEntries
    We don't have running balance/total in JournalEntries or Accounts tables. 
    We use monthly statements and this month calculations to calculate them
        you may also use codes in 
    https://github.com/simplcommerce/SimplCommerce
   See: http://www.theserverside.com/news/1364193/Nuts-and-Bolts-of-Transaction-Processing
   https://dba.stackexchange.com/questions/5608/writing-a-simple-bank-schema-how-should-i-keep-my-balances-in-sync-with-their-t/5647#5647
   http://stackoverflow.com/questions/147207/transactions-in-rest

       */

    /// <summary>
    /// The total money in an account could be computed by means of summation of all its journal entries.
    /// Usuallyt this occurs at the end of each journal entry, and is stored in Balance.
    /// This procedure may be done using STORED PROCEDURES in DB. This could result in a
    /// faster processing of transactions. We could alter the row related to account in that procedure.
    /// 
    /// 
    /// </summary>
    public class Account
    {
        public int Id { get; set; }
        /// <summary>
        /// A property like Username which identifies an MshAccount, usually based on the user email. 
        /// For systemic accounts like Platform account or Anonymous Contributor accounts this could be anything else
        /// Maybe we use mobile number in future too!
        /// NOTEs:
        /// 1) to create an Account with UserAccount type, there must be an Email specified
        /// 2) We do not query Transactions related to an account, we usually query related joural entries
        /// </summary>
        [Index(IsUnique = true)]
        [StringLength(300)]//need this for index
        public string AccountName { get; set; }
        public AccountType AccountType { get; set; }
        public virtual IList<JournalEntry> JournalEntries { get; set; }
    }

    /// <summary>
    /// Every transaction consists of two or more matching journal entries (Debit/Credit, Double-Entry)
    /// see these links:
    ///     http://www.math-cs.gordon.edu/courses/cs211/ATMExample/UseCases.html
    ///     http://www.cs.usfca.edu/~srollins/courses/cs682-s08/web/notes/transactions.html
    ///     
    /// Creation of a Transaction with its JournalEntries means that it is completed. 
    /// There is no such thing as 'pending' transaction here mainly because we don't have a distributed database here.
    /// 
    /// We may use an encrypted Hash to validate a transaction and its journal entries
    /// 
    /// When the campaign failes a there will be chargeback transactions related to all its transactions.
    /// The related transactions are obtained by using JournalEntry.Transaction property for all JournalEntries
    /// related to the campaign's Account.
    /// 
    /// When the campaign successds, money remains in Campaign Account until someone 
    /// withdraw it from Mishavad to a Bank Account
    /// 
    /// TODO: Rewards should also be added somewhere in this model! Maybe they need accounts like Reward sale etc.
    /// </summary>
    public class Transaction
    {
        public long Id { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedDateUtc { get; set; }
        /// <summary>
        /// Id of the ApplicationUser who created this transaction
        /// </summary>
        public int CreatedById { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }

        /// <summary>
        /// Note that we can't replace this by an InvoiceId because there are internal gifts having no
        /// and Invoices could also be removed or archived. However the Id is usually copied from DepositInvoice
        /// in case there is a payment gateway deposit.
        /// </summary>
        public long? ExtraInfoJSONId { get; set; }
        public virtual ExtraInfoJSON ExtraInfoJSON { get; set; }

        public IEnumerable<JournalEntry> GenerateTransferEntries(int Amount, int CreditAccountId, int DebitAccountId)
        {
            yield return new JournalEntry
            {
                AccountId = CreditAccountId,
                Amount = Amount,
                IsCredit = true
            };
            yield return new JournalEntry
            {
                AccountId = DebitAccountId,
                Amount = Amount,
                IsCredit = false
            };
        }
       
    }

    public class JournalEntry
    {
        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedDateUtc { get; set; }

        public long Id { get; set; }

        public long TransactionId { get; set; }

        [Required]
        public int AccountId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Amount { get; set; }

        [Required]
        public bool IsCredit { get; set; }

        public virtual Account Account { get; set; }
        public virtual Transaction Transaction { get; set; }
    }

    /// <summary>
    /// TODO: This summary needs to be edited. Some statements are related to other entities
    /// The DepositeInvoice type is used exclusively to deposit money into an Account.
    /// It's created before banking procedure to transfer fund succeeds.
    /// One usually adds fund to their account when they want to contribute to a campaign.
    /// This campaign is identified using a Transaction. 
    /// the Invoice is approved (usually by redirection from a bank). 
    /// 
    /// NOTE: MerchantCode or MerchantID is not stored in Invoices they are generated in the controllers
    /// 
    /// IMP NOTE: Notice that we used same PK/FK for this entity because the Invoices DEPEND on transactions
    /// It seems awry but an Invoice have to have a corresponding transaction but a transaction do not.
    /// 
    /// NOTE: DepositInvoice is an archivable entity. It should not tie with the whole DB integrity. 
    /// Bear this in mind while designing the database
    /// </summary>
    public class DepositInvoice
    {
        public long Id { get; set; }

        [Required]
        public OrderStatus Status { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Amount { get; set; }

        /// <summary>
        /// The DateTime the invoice is created
        /// Usually the following payment parameters are the same as this property:
        ///     Pasargad:
        ///         InvoiceDate
        ///         TimeStamp
        ///     ZarinPal: 
        ///         None!
        /// So we first create invoice and then assign these variables
        /// </summary>
        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedDateUtc { get; set; }

        /// <summary>
        /// Indicates the gateway in which this invoice is going to be paid. 
        /// If the payment failed, we will create another invoice
        /// </summary>
        public PaymentGateway PaymentGateway { get; set; }

        /// <summary>
        /// Pasargad: 
        ///     transactionReferenceID
        /// ZarinPal
        ///     refID
        /// 
        /// NOTE: to ensure compatiblity with all external payment systems we store all external Ids in string format.
        /// Because the number of Invoices are generally not that much there is no problem in terms of their size
        /// The other reason is that we generally do not query invoices for ordinary users so the number of queries are very small
        /// Also we archive invoice data from time to time to free some spaces
        /// </summary>
        public string GatewayTransactionReferenceID { get; set; }

        /// <summary>
        /// AccountName to be deposit in. Either AccountName of Anon. Contributors or AccountName related to a new account or existing account
        /// once the invoice is approved by bank we create Account for the new account or use the previous AccountId to add
        /// the Transaction and Journal Entries. 
        /// Why AccountName instead of AccountId:
        /// 1) We do not need to create accounts for unpaid invoices
        /// 2) We could discard invoices over time so size is not an important thing
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// An optional property related to the reciever campaign. If AccountName does not exist or
        /// is systemic account( e.g, Anon.Contributors etc) this property is necessary.
        /// The Account of ReceiverCampaign will be obtained by ReceiverCampaign.Account property
        /// </summary>
        public int? ReceiverCampaignId { get; set; }
        public virtual Campaign ReceiverCampaign { get; set; }

        public string ReferenceNumber { get; set; }

        /// <summary>
        /// TraceNumber in Pasargad Payment (Shomare Peygiri)
        /// </summary>
        public string GatewayTraceNumber { get; set; }

        public long? ExtraInfoJSONId { get; set; }
        public virtual ExtraInfoJSON ExtraInfoJSON  { get;set;}
    }

    /*NOTE: we create transactions after successful DepositInvoices
        */
    public class TransactionDepositInvoiceMap
    {
        [Key, ForeignKey("Transaction")]
        public long TransactionId { get; set; }
        /*NOTE: This is nullable because we may want to remove invoices but we don't want to lose transactions!*/
        public long? DepositInvoiceId { get; set; }

        public virtual DepositInvoice DepositInvoice { get; set; }
        public virtual Transaction Transaction { get; set; }
    }

    /*
        A 1--0..1 relationship between Transaction and Reward
        Just kept RewardId Nullable for consistency between Transaction/Reward Cascade delete
        Note that because TransactionId is PK, it does not cause cascade delete though it is not nullable  
    */
    public class TransactionRewardMap
    {
        [Key, ForeignKey("Transaction")]
        public long TransactionId { get; set; }
        public int? RewardId { get; set; }
        public virtual Reward Reward { get; set; }
        public virtual Transaction Transaction { get; set; }
    }

    /// <summary>
    /// This is a 1..0 or 1..1 relationship and because two entities of the same type are related to each other,
    /// We can't use both PK/FK for both of them. So we must use unique indecies. Unique FK is not supported in EF
    /// I think the cleanest way to design chargebacks is this. However I could change my mind in future.
    /// The idea that we assign only OriginalTransaction both PK/FK in this entity is not sufficient because
    /// we need both ChargedbackBy and ChargebackOf. We currently calculate these by joins in controllers.
    /// For the same reasons I made both OriginalTransactionId and ChargebackTransactionId Nullable, because
    /// when we remove either of them we must be able to trace their mate transaction so the Map must not be deleted
    /// </summary>
    public class TransactionChargebackMap
    {
        public long Id { get; set; }
        // index to speed up joins, we both need to find Originals by chargebacks and chargebacks by orgiginals
        [Index(IsUnique = true)]
        public long? OriginalTransactionId { get; set; }
        [Index(IsUnique =true)]
        public long? ChargebackTransactionId { get; set; }
        public virtual Transaction OriginalTransaction { get; set; }
        public virtual Transaction ChargebackTransaction { get; set; }

    }

    public class ExtraInfoJSON
    {
        public long Id { get; set; }
        /// <summary>
        /// A JSON string which contains info related to this invoice
        /// These could include the payer/doner firstName/lastName/Comments etc.
        /// This property should be evaluated in terms of XSS attacks before saving.
        /// This property will be copied to the same property in Transaction because 
        /// Invoices as noted above are detachable from the database
        /// </summary>
        public string InfoJSON { get; set; }
    }

}