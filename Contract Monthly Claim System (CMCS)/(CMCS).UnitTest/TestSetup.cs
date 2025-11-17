using Contract_Monthly_Claim_System__CMCS_.Controllers;
using Xunit;

namespace Contract_Monthly_Claim_System__CMCS_.UnitTests
{
    /// <summary>
    /// Test setup class to ensure clean state for each test
    /// </summary>
    public static class TestSetup
    {
        /// <summary>
        /// Resets all static data in controllers to ensure clean test state
        /// </summary>
        public static void ResetAllControllers()
        {
            UserController.ResetStaticData();
            RoleController.ResetStaticData();
            ClaimController.ResetStaticData();
            ApprovalController.ResetStaticData();
        }
    }

    /// <summary>
    /// Base test class that automatically resets controller data before each test
    /// </summary>
    public abstract class CleanTestBase
    {
        public CleanTestBase()
        {
            // Reset all controller static data before each test
            TestSetup.ResetAllControllers();
        }
    }
}
