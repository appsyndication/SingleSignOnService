using BrockAllen.MembershipReboot;

namespace FireGiant.MembershipReboot.AzureStorage
{
    public class AtsUserAccountConfig : MembershipRebootConfiguration<AtsUserAccount>
    {
        public AtsUserAccountConfig(string connectionString)
        {
            this.TableStorageConnectionString = connectionString;
        }

        public string TableStorageConnectionString { get; }
    }
}
